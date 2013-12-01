
#region Copyright (C) 2007-2013 Team MediaPortal

/*
    Copyright (C) 2007-2013 Team MediaPortal
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
using System.Threading;
using System.Collections.Generic; 
using System.Globalization;
using System.Linq;
using MediaPortal.Common;
using MediaPortal.Common.Localization; 
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement; 
using MediaPortal.Common.Settings;
using MediaPortal.UI.Presentation.UiNotifications;

using Log = TvLibrary.Log.huha.Log;
using MediaPortal.Plugins.TvWishList;
using MediaPortal.Plugins.TvWishList.Items;
using MediaPortal.Plugins.TvWishListMP2NativeTvProvider.Items;
using MediaPortal.Plugins.TvWisListhMP2.Settings;
using MediaPortal.Common.Configuration;
using MediaPortal.Common.Configuration.ConfigurationClasses;
using MediaPortal.Plugins.TvWishListMP2NativeTvProvider.Settings;

namespace MediaPortal.Plugins.TvWishListMP2NativeTvProvider
{                
    public class TvWishListMP2NativeTvProvider : ITvWishListTVProvider 
    {
    
        //Instance
        public static TvWishListMP2NativeTvProvider _instance = null;

        public ITvWishListTVProvider Instance
        {
            get { return (TvWishListMP2NativeTvProvider)_instance; }
        }

        //Init
        public bool _initialized = false;
        public bool Initialized
        {
            get { return (bool)_initialized; }
        }

        //needed for schedules
        public ISchedule _newSchedule = null;
        public ISchedule NewSchedule
        {
            get { return _newSchedule; }
            set { _newSchedule = value; }
        }

        //needed for recordings
        public IRecording _newRecording = null;
        public IRecording NewRecording
        {
            get { return _newRecording; } 
            set { _newRecording = value; }
        }

        //Only single Tv server is supported at the moment
        public int _server_index = 0;
        public int Server_Index
        {
            get { return _server_index; }
            set { _server_index = value; }
        }



        public TvWishListMP2NativeTvProvider()
        { 
            _initialized = true;
            _instance = this;

            ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
            TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();
            Log.DebugValue = settings.Verbose;

            string host = settingsManager.Load<TvWishListMP2NativeTvProviderSettings>().TvServerHost;
            Log.Debug("host=" + host);
            
            PipeClient mypipe = new PipeClient(host,"MP2TvWishListPipe");
            mypipe.Debug = Log.DebugValue;

            Log.Debug("TvWishListNativeTvProvider initialized");
            Log.Debug("mypipe.Debug=" + mypipe.Debug.ToString());

            // get server host name from tvserver


        }



        public void Dispose()
        {
          DeInit();
          _initialized = false;
        }

    

        public bool Init()
        {
          return true;
        }

        public bool DeInit()
        {
          try
          {
        
          }
          catch (Exception ex)
          {
            NotifyException(ex);
          }
          return false;
        }


        #region ITvWishListTVProvider
        public bool GetConnectedServers(out IList<IServerName> serverNames)
        {
            serverNames = new List<IServerName>();
            IServerName connectedServer = new ServerName();
            connectedServer.Name = System.Environment.MachineName;
            connectedServer.ServerIndex = 0;
            serverNames.Add(connectedServer);
            return true;
        }

        public bool ReadAllCards(out IList<ICard> allCards)
        {
            allCards = new List<ICard>();

            try
            {
                string response = string.Empty;
                string command = PipeCommands.ReadAllCards.ToString();
                Log.Debug("command=" + command);
                response = PipeClient.Instance.RunSingleCommand(command);
                Log.Debug("response=" + response);
                string[] allCardsString = response.Split('\n');
                Log.Debug(allCardsString.Length.ToString()+" elements found");
                for (int i = 0; i < (allCardsString.Length-1) / 2; i++)
                {
                    Card mycard = new Card();
                    //Log.Debug(allCardsString[2*i]);
                    mycard.Id = int.Parse(allCardsString[2*i]);
                    //Log.Debug(allCardsString[2*i+1]);
                    mycard.Name = allCardsString[2*i + 1];
                    allCards.Add(mycard);
                }
            }
            catch (Exception exc)
            {
                Log.Debug("***Error ReadAllCards: Exception="+exc.Message);
                return false;
            }
            return true;
        }

        public bool ReadAllChannels(out IList<IChannel> allChannels)
        {
            allChannels = new List<IChannel>();
            try
            {
                string response = string.Empty;
                string command = PipeCommands.ReadAllChannels.ToString();
                Log.Debug("command=" + command);
                response = PipeClient.Instance.RunSingleCommand(command);
                Log.Debug("response=" + response);
                string[] allChannelsString = response.Split('\n');
                for (int i = 0; i < (allChannelsString.Length-1) / 2; i++)
                {
                    Channel mychannel = new Channel();
                    mychannel.Id = int.Parse(allChannelsString[2*i]);
                    mychannel.Name = allChannelsString[2*i + 1];
                    allChannels.Add(mychannel);
                }
            }
            catch (Exception exc)
            {
                Log.Debug("***Error ReadAllChannels: Exception=" + exc.Message);
                return false;
            }
            return true;
        }

        public bool ReadAllChannelsByGroup(int groupid, out IList<IChannel> allChannels)
        {
            allChannels = new List<IChannel>();
            try
            {
                //debug only 

                IList<IChannelGroup> allChannelGroups = new List<IChannelGroup>();
                ReadAllChannelGroups(out  allChannelGroups);

                string response = string.Empty;
                string command = PipeCommands.ReadAllChannelsByGroup.ToString() + groupid.ToString();
                Log.Debug("command=" + command);
                response = PipeClient.Instance.RunSingleCommand(command);
                Log.Debug("response=" + response);
                string[] allChannelsString = response.Split('\n');
                for (int i = 0; i < (allChannelsString.Length-1) / 2; i++)
                {
                    Channel mychannel = new Channel();
                    mychannel.Id = int.Parse(allChannelsString[2*i]);
                    mychannel.Name = allChannelsString[2*i + 1];
                    allChannels.Add(mychannel);
                }
            }
            catch (Exception exc)
            {
                Log.Debug("***Error ReadAllChannelsByGroup: Exception=" + exc.Message);
                return false;
            }
            return true;
        }

        public bool ReadAllRadioChannels(out IList<IRadioChannel> allRadioChannels)
        {
            allRadioChannels = new List<IRadioChannel>();
            try
            {
                string response = string.Empty;
                string command = PipeCommands.ReadAllRadioChannels.ToString();
                Log.Debug("command=" + command);
                response = PipeClient.Instance.RunSingleCommand(command);
                Log.Debug("response=" + response);
                string[] allRadioChannelsString = response.Split('\n');
                for (int i = 0; i < (allRadioChannelsString.Length-1) / 2; i++)
                {
                    RadioChannel myRadiochannel = new RadioChannel();
                    myRadiochannel.Id = int.Parse(allRadioChannelsString[2*i]);
                    myRadiochannel.Name = allRadioChannelsString[2*i + 1];
                    allRadioChannels.Add(myRadiochannel);
                }
            }
            catch (Exception exc)
            {
                Log.Debug("***Error ReadAllRadioChannels: Exception=" + exc.Message);
                return false;
            }
            return true;
        }

        public bool ReadAllRadioChannelsByGroup(int groupid, out IList<IRadioChannel> allRadioChannels)
        {
            allRadioChannels = new List<IRadioChannel>();
            try
            {
                string response = string.Empty;
                string command = PipeCommands.ReadAllRadioChannels.ToString() + groupid.ToString();
                Log.Debug("command=" + command);
                response = PipeClient.Instance.RunSingleCommand(command);
                Log.Debug("response=" + response);
                string[] allRadioChannelsString = response.Split('\n');
                for (int i = 0; i < (allRadioChannelsString.Length-1) / 2; i++)
                {
                    RadioChannel myRadiochannel = new RadioChannel();
                    myRadiochannel.Id = int.Parse(allRadioChannelsString[2*i]);
                    myRadiochannel.Name = allRadioChannelsString[2*i + 1];
                    allRadioChannels.Add(myRadiochannel);
                }
            }
            catch (Exception exc)
            {
                Log.Debug("***Error ReadAllRadioChannelsByGroup: Exception=" + exc.Message);
                return false;
            }
            return true;
        }

        public bool ReadAllChannelGroups(out IList<IChannelGroup> allChannelGroups)
        {
            allChannelGroups = new List<IChannelGroup>();
            try
            {           
                string response = string.Empty;
                string command = PipeCommands.ReadAllChannelGroups.ToString();
                Log.Debug("command=" + command);
                response = PipeClient.Instance.RunSingleCommand(command);
                Log.Debug("response=" + response);
                string[] allChannelGroupsString = response.Split('\n');
                for (int i = 0; i < (allChannelGroupsString.Length-1) / 2; i++)
                {
                    ChannelGroup mychannelgroup = new ChannelGroup();
                    mychannelgroup.Id = int.Parse(allChannelGroupsString[2*i]);
                    mychannelgroup.GroupName = allChannelGroupsString[2*i + 1];
                    allChannelGroups.Add(mychannelgroup);
                }
            }
            catch (Exception exc)
            {
                Log.Debug("***Error ReadAllChannelGroups: Exception=" + exc.Message);
                return false;
            }
            return true;
        }

        public bool ReadAllRadioChannelGroups(out IList<IRadioChannelGroup> allRadioChannelGroups)
        {
            allRadioChannelGroups = new List<IRadioChannelGroup>();
            try
            {
                string response = string.Empty;
                string command = PipeCommands.ReadAllRadioChannelGroups.ToString();
                Log.Debug("command=" + command);
                response = PipeClient.Instance.RunSingleCommand(command);
                Log.Debug("response=" + response);
                string[] allRadioChannelGroupsString = response.Split('\n');
                for (int i = 0; i < (allRadioChannelGroupsString.Length-1) / 2; i++)
                {
                    RadioChannelGroup myRadiochannelgroup = new RadioChannelGroup();
                    myRadiochannelgroup.Id = int.Parse(allRadioChannelGroupsString[2*i]);
                    myRadiochannelgroup.GroupName = allRadioChannelGroupsString[2*i + 1];
                    allRadioChannelGroups.Add(myRadiochannelgroup);
                }
            }
            catch (Exception exc)
            {
                Log.Debug("***Error ReadAllRadioChannelGroups: Exception=" + exc.Message);
                return false;
            }
            return true;
        }

        public bool ReadAllRecordings(out IList<IRecording> allRecordings)
        {
            allRecordings = new List<IRecording>();
            try
            {
                string response = string.Empty;
                string command = PipeCommands.ReadAllRecordings.ToString();
                Log.Debug("command=" + command);
                response = PipeClient.Instance.RunSingleCommand(command);
                Log.Debug("response=" + response);
                string[] allRecordingsString = response.Split('\n');
                for (int i = 0; i < (allRecordingsString.Length-1) / 6; i++)
                {
                    Recording myrecording = new Recording();
                    myrecording.IdRecording = int.Parse(allRecordingsString[6*i]);
                    myrecording.Title = allRecordingsString[6 * i + 1];
                    myrecording.FileName = allRecordingsString[6 * i + 2];
                    myrecording.IdChannel = int.Parse(allRecordingsString[6 * i + 3]);
                    myrecording.StartTime = DateTime.ParseExact(allRecordingsString[6 * i + 4], "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                    myrecording.EndTime = DateTime.ParseExact(allRecordingsString[6 * i + 5], "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);

                    allRecordings.Add(myrecording);
                }
            }
            catch (Exception exc)
            {
                Log.Debug("***Error ReadAllRecordings: Exception=" + exc.Message);
                return false;
            }
            return true;
        }

        public bool ReadAllSchedules(out IList<ISchedule> allSchedules)
        {
            allSchedules = new List<ISchedule>();
            try
            {
                string response = string.Empty;
                string command = PipeCommands.ReadAllSchedules.ToString();
                Log.Debug("command=" + command);
                response = PipeClient.Instance.RunSingleCommand(command);
                Log.Debug("response=" + response);
                string[] allSchedulesString = response.Split('\n');
                for (int i = 0; i < (allSchedulesString.Length-1) / 14; i++)
                {
                    Schedule mySchedule = new Schedule();

                    mySchedule.Id = int.Parse(allSchedulesString[14*i]);
                    mySchedule.ProgramName = allSchedulesString[14 * i + 1];
                    mySchedule.IdChannel = int.Parse(allSchedulesString[14 * i + 2]);
                    mySchedule.StartTime = DateTime.ParseExact(allSchedulesString[14 * i + 3], "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                    mySchedule.EndTime = DateTime.ParseExact(allSchedulesString[14 * i + 4], "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                    mySchedule.ScheduleType = int.Parse(allSchedulesString[14 * i + 5]);
                    mySchedule.PreRecordInterval = int.Parse(allSchedulesString[14 * i + 6]);
                    mySchedule.PostRecordInterval = int.Parse(allSchedulesString[14 * i + 7]);
                    mySchedule.MaxAirings = int.Parse(allSchedulesString[14 * i + 8]);
                    mySchedule.KeepDate = DateTime.ParseExact(allSchedulesString[14 * i + 9], "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                    mySchedule.KeepMethod = int.Parse(allSchedulesString[14 * i + 10]);
                    mySchedule.Priority = int.Parse(allSchedulesString[14 * i + 11]);
                    mySchedule.RecommendedCard = int.Parse(allSchedulesString[14 * i + 12]);
                    mySchedule.Series = bool.Parse(allSchedulesString[14 * i + 13]);

                    allSchedules.Add(mySchedule);
                }
            }
            catch (Exception exc)
            {
                Log.Debug("***Error ReadAllSchedules: Exception=" + exc.Message);
                return false;
            }
            return true;
        }

        public bool ScheduleDelete()
        {
            try
            {
                string response = string.Empty;
                string command = PipeCommands.ScheduleDelete.ToString() +NewSchedule.Id.ToString();
                Log.Debug("command=" + command);
                response = PipeClient.Instance.RunSingleCommand(command);
                Log.Debug("response=" + response);           
            }
            catch (Exception exc)
            {
                Log.Debug("***Error ScheduleDelete: Exception=" + exc.Message);
                return false;
            }
            return true;
        }

        public bool ScheduleNew()
        {
            try
            {
                string scheduleString = NewSchedule.ProgramName + "\n" + NewSchedule.IdChannel.ToString() + "\n" + NewSchedule.StartTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture) + "\n" + NewSchedule.EndTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);

                string response = string.Empty;
                string command = PipeCommands.ScheduleNew.ToString() + scheduleString;
                Log.Debug("command=" + command);
                response = PipeClient.Instance.RunSingleCommand(command);
                Log.Debug("response=" + response);
            }
            catch (Exception exc)
            {
                Log.Debug("***Error ScheduleNew: Exception=" + exc.Message);
                return false;
            }
            return true;
        }

        public bool ReadSetting(string tag, string value, out ISetting setting)
        {
            setting = new Setting();
            
            try
            {
                string response = string.Empty;
                string command = PipeCommands.ReadSetting.ToString() + tag;
                Log.Debug("command=" + command);
                response = PipeClient.Instance.RunSingleCommand(command);
                Log.Debug("response=" + response);
                setting.Tag = tag;
                setting.Value = response;
                return true;
            }
            catch (Exception exc)
            {
                Log.Debug("***Error ReadSetting: Exception=" + exc.Message);
                return false;
            }
        }

        public bool WriteSetting(string tag, string value)
        {
            try
            {

                string response = string.Empty;
                string command = PipeCommands.WriteSetting.ToString() +  tag + "\n" + value;
                Log.Debug("command=" + command);
                response = PipeClient.Instance.RunSingleCommand(command);
                Log.Debug("response=" + response);
                return true;
            }
            catch (Exception exc)
            {
                Log.Debug("***Error WriteSetting: Exception=" + exc.Message);
                return false;
            }
        }


        #endregion ITvWishListTVProvider


        #region Exeption handling

        private void NotifyException(Exception ex, string localizationMessage = null)
        {
          string notification = string.IsNullOrEmpty(localizationMessage)
                                  ? ex.Message
                                  : ServiceRegistration.Get<ILocalization>().ToString(localizationMessage, ex.Message);

          ServiceRegistration.Get<INotificationService>().EnqueueNotification(NotificationType.Error, "Error", notification, true);
          ServiceRegistration.Get<ILogger>().Error(notification);
        }

        #endregion
 
    }
}
