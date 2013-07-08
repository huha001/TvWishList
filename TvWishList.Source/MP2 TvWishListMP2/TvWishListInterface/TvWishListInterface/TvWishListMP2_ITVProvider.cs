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

using System.Collections.Generic;

namespace MediaPortal.Plugins.TvWishListMP2.MPExtended
{
  public interface ITvWishListTVProvider
  {
      #region properties
      ITvWishListTVProvider Instance { get; }
      bool Initialized { get; }
      int Server_Index { get; set; }
      ISchedule NewSchedule { get; set; }
      IRecording NewRecording { get; set; }
      #endregion properties 


      bool Init();

        bool DeInit();
       
        #region ITvServerDatabase
        bool GetConnectedServers(out IList<IServerName> serverNames);       
        #endregion ITvServerDatabase



        #region Settings
        bool ReadSetting(string tagName, string defaultValue, out ISetting setting);


        bool WriteSetting(string tagName, string value);
        
        #endregion Settings

        #region Groups
        bool ReadAllChannelGroups(out IList<IChannelGroup> allgroups);
        bool ReadAllRadioChannelGroups(out IList<IRadioChannelGroup> allgroups);
        
        #endregion Groups

        #region Channels
        bool ReadAllChannels(out IList<IChannel> allchannels);
        

        bool ReadAllChannelsByGroup(int groupId, out IList<IChannel> allchannels);
        

        bool ReadAllRadioChannels(out IList<IRadioChannel> allradiochannels);


        bool ReadAllRadioChannelsByGroup(int groupId, out IList<IRadioChannel> allradiochannels);
        
        #endregion Channels

        #region cards
        bool ReadAllCards(out IList<ICard> allCards);        
        #endregion cards

      #region Schedules
        bool ReadAllSchedules(out IList<ISchedule> allSchedules);
        
        bool ScheduleNew();
        
        bool ScheduleDelete();
      #endregion Schedules


      #region Recordings
        bool ReadAllRecordings(out IList<IRecording> allRecordings);        
      #endregion Recordings

  }
}
