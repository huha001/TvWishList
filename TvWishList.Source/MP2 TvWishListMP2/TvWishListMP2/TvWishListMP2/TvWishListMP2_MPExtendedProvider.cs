#region Copyright (C) 2007-2012 Team MediaPortal

/*
    Copyright (C) 2007-2012 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

using MediaPortal.Common;
using MediaPortal.Common.General;
using MediaPortal.Common.Localization;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.Settings;
using MediaPortal.UI.Presentation.UiNotifications;
using MPExtended.Services.TVAccessService.Interfaces;
using MediaPortal.Plugins.TvWishListMP.Providers.Settings;
using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishListMP2.MPExtended
{
  public class MyTvWishListMP2MPExtendedProvider 
  {
    #region Internal class

    internal class ServerContext
    {
      public string ServerName;
      public string Username;
      public string Password;
      public ITVAccessService TvServer;
      public bool ConnectionOk;

      


      public static bool IsLocal(string host)
      {
        if (string.IsNullOrEmpty(host))
          return true;

        string lowerHost = host.ToLowerInvariant();
        return lowerHost == "localhost" || lowerHost == LocalSystem.ToLowerInvariant() || host == "127.0.0.1" || host == "::1";
      }

      public void CreateChannel()
      {
        Binding binding;
        EndpointAddress endpointAddress;
        bool useAuth = !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        if (IsLocal(ServerName))
        {
          endpointAddress = new EndpointAddress("net.pipe://localhost/MPExtended/TVAccessService");
          binding = new NetNamedPipeBinding { MaxReceivedMessageSize = 10000000 };
        }
        else
        {
          endpointAddress = new EndpointAddress(string.Format("http://{0}:4322/MPExtended/TVAccessService", ServerName));
          BasicHttpBinding basicBinding = new BasicHttpBinding { MaxReceivedMessageSize = 10000000 };
          if (useAuth)
          {
            basicBinding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
            basicBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
          }
          binding = basicBinding;
        }
        binding.OpenTimeout = TimeSpan.FromSeconds(5);
        ChannelFactory<ITVAccessService> factory = new ChannelFactory<ITVAccessService>(binding);
        if (factory.Credentials != null && useAuth)
        {
          factory.Credentials.UserName.UserName = Username;
          factory.Credentials.UserName.Password = Password;
        }
        TvServer = factory.CreateChannel(endpointAddress);
      }
    }

    #endregion

        #region Constants

        private const string RES_TV_CONNECTION_ERROR_TITLE = "[Settings.Plugins.TV.ConnectionErrorTitle]";
        private const string RES_TV_CONNECTION_ERROR_TEXT = "[Settings.Plugins.TV.ConnectionErrorText]";
        private const int MAX_RECONNECT_ATTEMPTS = 2;

        #endregion

        #region Fields

        private static readonly string LocalSystem = SystemName.LocalHostName;
        private ServerContext[] _tvServers;
        private int _reconnectCounter = 0;

        //ILogger Log = ServiceRegistration.Get<ILogger>();

    
        //Instance
        public static MyTvWishListMP2MPExtendedProvider _instance = null;

        public static MyTvWishListMP2MPExtendedProvider Instance
        {
            get { return (MyTvWishListMP2MPExtendedProvider)_instance; }
        }

        //Init
        public bool _initialized = false;
        public bool Initialized
        {
            get { return (bool)_initialized; }
        }

        //Only single Tv server is supported at the moment
        public int _server_index = 0;
        public int Server_Index
        {
            get { return _server_index; }
            set { _server_index = value; }
        }

        //needed for schedules
        public Schedule _newSchedule = null;
        public Schedule NewSchedule
        {
            get { return _newSchedule; }
            set { _newSchedule = value; }
        }

        //needed for recordings
        public Recording _newRecording = null;
        public Recording NewRecording
        {
            get { return _newRecording; }
            set { _newRecording = value; }
        }

        #endregion

        #region constructor
        public MyTvWishListMP2MPExtendedProvider()
        {
            _instance = this;
        }
        #endregion constructor


        #region ITimeshiftControl Member

        public bool Init()
        {      
            CreateAllTvServerConnections();
            
            _initialized = true;
            return true;
        }

        public bool DeInit()
        {
            _initialized = false;

            if (_tvServers == null)
                return false;

            _tvServers = null;
            return true;
        }

        private ITVAccessService TvServer(int serverIndex)
        {
            return _tvServers[serverIndex].TvServer;
        }

        private bool CheckConnection(int serverIndex)
        {
            bool reconnect = false;
            ServerContext tvServer = _tvServers[serverIndex];
            try
            {
                if (tvServer.TvServer != null)
                    tvServer.ConnectionOk = tvServer.TvServer.TestConnectionToTVService();   //tvServer.TvServer.TestConnectionToTVService();

                _reconnectCounter = 0;
            }
            catch (CommunicationObjectFaultedException)
            {
                reconnect = true;
            }
            catch (ProtocolException)
            {
                reconnect = true;
            }
            catch (Exception ex)
            {
                NotifyException(ex, serverIndex);
                return false;
            }
            if (reconnect)
            {
                // Try to reconnect
                tvServer.CreateChannel();
                if (_reconnectCounter++ < MAX_RECONNECT_ATTEMPTS)
                    return CheckConnection(serverIndex);

                return false;
            }
            return tvServer.ConnectionOk;
        }

        private void NotifyException(Exception ex, int serverIndex)
        {
            NotifyException(ex, null, serverIndex);
        }

        private void NotifyException(Exception ex, string localizationMessage, int serverIndex)
        {
            string serverName = _tvServers[serverIndex].ServerName;
            string notification = string.IsNullOrEmpty(localizationMessage)
                                    ? string.Format("{0}:", serverName)
                                    : ServiceRegistration.Get<ILocalization>().ToString(localizationMessage, serverName);
            notification += " " + ex.Message;

            ServiceRegistration.Get<INotificationService>().EnqueueNotification(NotificationType.Error, RES_TV_CONNECTION_ERROR_TITLE,
                notification, true);
            Log.Error(notification);
        }

        private void CreateAllTvServerConnections()
        {
            MPExtendedProviderSettings setting = ServiceRegistration.Get<ISettingsManager>().Load<MPExtendedProviderSettings>();
            if (setting.TvServerHost == null)
                return;

            string[] serverNames = setting.TvServerHost.Split(';');
            _tvServers = new ServerContext[serverNames.Length];

            Log.Debug("serverNames.Length=" + serverNames.Length.ToString());

            for (int serverIndex = 0; serverIndex < serverNames.Length; serverIndex++)
            {
                try
                {
                    string serverName = serverNames[serverIndex];
                    Log.Debug("serverName=" + serverName);
                    ServerContext tvServer = new ServerContext
                    {
                        ServerName = serverName,
                        ConnectionOk = false,
                        Username = setting.Username,
                        Password = setting.Password
                    };
                    Log.Debug("1");
                    _tvServers[serverIndex] = tvServer;
                    Log.Debug("2");
                    tvServer.CreateChannel();
                    Log.Debug("3");
                }
                catch (Exception ex)
                {
                    NotifyException(ex, RES_TV_CONNECTION_ERROR_TEXT, serverIndex);
                }
            }
        }

        #endregion


        #region ITvServerDatabase

        public bool GetConnectedServers(out List<ServerName> serverNames)
        {
            serverNames = new List<ServerName>();
            try
            {
                int idx = 0;
                //serverNames.Clear();
                foreach (ServerContext tvServer in _tvServers)
                {
                    if (!CheckConnection(idx))
                        continue;

                    ServerName connectedServer = new ServerName();
                    connectedServer.Name = tvServer.ServerName;
                    connectedServer.ServerIndex = idx;
               
                    serverNames.Add(connectedServer);
                    idx++;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return false;
            }
        }
        #endregion ITvServerDatabase



        #region Settings
        public bool ReadSetting(string tagName, string defaultValue, out Setting setting)
        {
            Log.Debug("a");
            setting = new Setting();
            Log.Debug("b");
            setting.Value = defaultValue;
            setting.Tag = tagName;

            if (!CheckConnection(_server_index))
                return false;

            try
            {
                Log.Debug("c");
                setting.Value = TvServer(_server_index).ReadSettingFromDatabase(tagName);
                Log.Debug("d");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return false;
        }
    
        public bool WriteSetting(string tagName, string value)
        {
            if (!CheckConnection(_server_index))
                return false;

            try
            {
                TvServer(_server_index).WriteSettingToDatabase(tagName, value);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return false;
        }
        #endregion Settings

        #region Groups
        public bool ReadAllChannelGroups(out IList<ChannelGroup> allgroups)
        {
            allgroups = new List<ChannelGroup>();

            try
            {
                IList<WebChannelGroup> tvGroups = TvServer(_server_index).GetGroups();

                foreach (WebChannelGroup mychannelgroup in tvGroups)
                {
                    ChannelGroup item = new ChannelGroup();
                    item.Id = mychannelgroup.Id;
                    item.GroupName = mychannelgroup.GroupName;
                    allgroups.Add(item);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return false;
        }

        public bool ReadAllRadioChannelGroups(out IList<RadioChannelGroup> allgroups)
        {
            allgroups = new List<RadioChannelGroup>();

            try
            {
                IList<WebChannelGroup> radioGroups = TvServer(_server_index).GetRadioGroups();

                foreach (WebChannelGroup myradiochannelgroup in radioGroups)
                {
                    RadioChannelGroup item = new RadioChannelGroup();
                    item.Id = myradiochannelgroup.Id;
                    item.GroupName = myradiochannelgroup.GroupName;
                    allgroups.Add(item);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return false;
        }
        #endregion Groups

        #region Channels
        public bool ReadAllChannels(out IList<Channel> allchannels)
        {
            allchannels = new List<Channel>();

            try
            {
                IList<WebChannelGroup> tvGroups = TvServer(_server_index).GetGroups();

                foreach (WebChannelGroup mychannelgroup in tvGroups)
                {
                    if (mychannelgroup.GroupName.ToLower() == "all channels")
                    {
                        IList<WebChannelBasic> alltvchannels = TvServer(_server_index).GetChannelsBasic(mychannelgroup.Id);

                        foreach (WebChannelBasic mychannel in alltvchannels)
                        {
                            Channel item = new Channel();
                            item.Id = mychannel.Id;
                            item.Name = mychannel.Title;
                            allchannels.Add(item);
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return false;
        }

        public bool ReadAllChannelsByGroup(int groupId, out IList<Channel> allchannels)
        {
            allchannels = new List<Channel>();

            try
            {
                IList<WebChannelBasic> alltvchannels = TvServer(_server_index).GetChannelsBasic(groupId);

                foreach (WebChannelBasic mychannel in alltvchannels)
                {
                    Channel item = new Channel();
                    item.Id = mychannel.Id;
                    item.Name = mychannel.Title;
                    allchannels.Add(item);
                }
                return true;

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return false;
        }

        public bool ReadAllRadioChannels(out IList<RadioChannel> allradiochannels)
        {
            allradiochannels = new List<RadioChannel>();

            try
            {
                IList<WebChannelGroup> radioGroups = TvServer(_server_index).GetRadioGroups();

                foreach (WebChannelGroup mychannelgroup in radioGroups)
                {
                    if (mychannelgroup.GroupName.ToLower() == "all channels")
                    {
                        IList<WebChannelBasic> allradchannels = TvServer(_server_index).GetRadioChannelsBasic(mychannelgroup.Id);

                        foreach (WebChannelBasic mychannel in allradchannels)
                        {
                            RadioChannel item = new RadioChannel();
                            item.Id = mychannel.Id;
                            item.Name = mychannel.Title;
                            allradiochannels.Add(item);
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return false;
        }

        public bool ReadAllRadioChannelsByGroup(int groupId, out IList<RadioChannel> allradiochannels)
        {
            allradiochannels = new List<RadioChannel>();

            try
            {
                IList<WebChannelBasic> allradchannels = TvServer(_server_index).GetRadioChannelsBasic(groupId);

                

                foreach (WebChannelBasic mychannel in allradchannels)
                {
                    RadioChannel item = new RadioChannel();
                    item.Id = mychannel.Id;
                    item.Name = mychannel.Title;
                    allradiochannels.Add(item);
                }
                return true;

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return false;
        }
        #endregion Channels

        #region cards
        public bool ReadAllCards(out IList<Card> allCards)
        {
            allCards = new List<Card>();

            try
            {
                IList<WebCard> tvCards = TvServer(_server_index).GetCards();
                foreach (WebCard mycard in tvCards)
                {
                    Card item = new Card();
                    item.Id = mycard.Id;
                    item.Name = mycard.Name;
                    allCards.Add(item);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return false;
        }
        #endregion cards

      #region Schedules
        public bool ReadAllSchedules(out IList<Schedule> allSchedules)
        {
            allSchedules = new List<Schedule>();

            try
            {
                IList<WebScheduleBasic> webSchedules = TvServer(_server_index).GetSchedules();
                foreach (WebScheduleBasic myschedule in webSchedules)
                {
                    Schedule item = new Schedule();
                    item.ProgramName = myschedule.Title;
                    item.Id = myschedule.Id;
                    item.IdChannel = myschedule.ChannelId;
                    item.StartTime = myschedule.StartTime;
                    item.EndTime = myschedule.EndTime;
                    item.ScheduleType = myschedule.ScheduleType;

                    item.PreRecordInterval = myschedule.PreRecordInterval;
                    item.PostRecordInterval = myschedule.PostRecordInterval;
                    item.MaxAirings = myschedule.MaxAirings;
                    item.KeepDate = myschedule.KeepDate;
                    item.KeepMethod = myschedule.KeepMethod;
                    item.Priority = myschedule.Priority;
                    item.Series = myschedule.Series;
                    
                    allSchedules.Add(item);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return false;
        }

        public bool ScheduleNew()
        {           
            try
            {
                TvServer(_server_index).AddSchedule(_newSchedule.IdChannel, _newSchedule.ProgramName, _newSchedule.StartTime, _newSchedule.EndTime, _newSchedule.ScheduleType);
                _newSchedule = null;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return false;
        }

        public bool ScheduleDelete()
        {
            try
            {
                TvServer(_server_index).DeleteSchedule(_newSchedule.Id);
                _newSchedule = null;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return false;
        }

        

      #endregion Schedules


      #region Recordings
        public bool ReadAllRecordings(out IList<Recording> allRecordings)
        {
            allRecordings = new List<Recording>();

            try
            {
                IList<WebRecordingBasic> webRecordings = TvServer(_server_index).GetRecordings();
                foreach (WebRecordingBasic myrecording in webRecordings)
                {
                    Recording item = new Recording();
                    item.Title = myrecording.Title;
                    item.IdRecording = myrecording.Id;
                    item.FileName = myrecording.FileName;
                    item.StartTime = myrecording.StartTime;
                    item.EndTime = myrecording.EndTime;
                    item.IdChannel = myrecording.ChannelId;
                    allRecordings.Add(item);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return false;
        }

        
      #endregion Recordings

  }
}
