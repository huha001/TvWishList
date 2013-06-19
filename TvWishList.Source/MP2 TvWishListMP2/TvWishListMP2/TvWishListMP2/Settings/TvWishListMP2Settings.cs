#region Copyright (C) 2007-2011 Team MediaPortal

/*
    Copyright (C) 2007-2011 Team MediaPortal
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
using MediaPortal.Common.Settings;

namespace MediaPortal.Plugins.TvWishListMP2.Settings
{

    //Settings will be saved in C:\ProgramData\Team MediaPortal\MP2-Client\Config\<USER>\Plugins.TvWishListMP2.Settings.TvWishListMP2.xml
    public class TvWishListMP2Settings
    {
        //**********************************************
        // Config Main
        //**********************************************
        [Setting(SettingScope.User, false)]
        public bool Verbose { get; set; }

        [Setting(SettingScope.User, false)]
        public bool DisableQuickMenu { get; set; }

        [Setting(SettingScope.User, true)]
        public bool DisableInfoWindow { get; set; }

        [Setting(SettingScope.User, "60")]
        public string TimeOut { get; set; }

        //**********************************************
        // Config Filters
        //**********************************************
        [Setting(SettingScope.User, false)]
        public bool Active { get; set; }

        //no searchfor

        [Setting(SettingScope.User, true)]
        public bool MatchType { get; set; }

        [Setting(SettingScope.User, true)]
        public bool Group { get; set; }

        [Setting(SettingScope.User, true)]
        public bool RecordType { get; set; }

        [Setting(SettingScope.User, true)]
        public bool Action { get; set; }

        [Setting(SettingScope.User, false)]
        public bool Exclude { get; set; }

        //no viewed

        [Setting(SettingScope.User, false)]
        public bool PreRecord { get; set; }

        [Setting(SettingScope.User, false)]
        public bool PostRecord { get; set; }

        [Setting(SettingScope.User, false)]
        public bool EpisodeName { get; set; }

        [Setting(SettingScope.User, false)]
        public bool EpisodePart { get; set; }

        [Setting(SettingScope.User, false)]
        public bool EpisodeNumber { get; set; }

        [Setting(SettingScope.User, false)]
        public bool SeriesNumber { get; set; }

        [Setting(SettingScope.User, false)]
        public bool KeepEpisodes { get; set; }

        [Setting(SettingScope.User, false)]
        public bool KeepUntil { get; set; }

        [Setting(SettingScope.User, false)]
        public bool RecommendedCard { get; set; }

        [Setting(SettingScope.User, false)]
        public bool Priority { get; set; }

        [Setting(SettingScope.User, false)]
        public bool AfterTime { get; set; }

        [Setting(SettingScope.User, false)]
        public bool BeforeTime { get; set; }

        [Setting(SettingScope.User, false)]
        public bool AfterDays { get; set; }

        [Setting(SettingScope.User, false)]
        public bool BeforeDays { get; set; }

        [Setting(SettingScope.User, true)]
        public bool Channel { get; set; }

        [Setting(SettingScope.User, true)]
        public bool Skip { get; set; }

        [Setting(SettingScope.User, true)]
        public bool Name { get; set; }

        [Setting(SettingScope.User, false)]
        public bool UseFolderName { get; set; }

        [Setting(SettingScope.User, false)]
        public bool WithinNextHours { get; set; }

        // no scheduled

        // no tvwishid

        // no recorded

        // no deleted

        // no emailed

        // no conflicts

        [Setting(SettingScope.User, false)]
        public bool EpisodeCriteria { get; set; }

        [Setting(SettingScope.User, false)]
        public bool PreferredGroup { get; set; }

        [Setting(SettingScope.User, false)]
        public bool IncludeRecordings { get; set; }

        //**********************************************
        // Config Defaults
        //**********************************************

        [Setting(SettingScope.User, "")]
        public string UserDefaultFormatsString { get; set; }
        

        //**********************************************
        // Config Formats
        //**********************************************

        [Setting(SettingScope.User, "")]
        public string DateAndTimeFormat { get; set; }

        [Setting(SettingScope.User, "")]
        public string MainItemFormat { get; set; }

        [Setting(SettingScope.User, "")]
        public string ResultItemFormat { get; set; }

        [Setting(SettingScope.User, "")]
        public string EmailMainTextBoxFormat { get; set; }

        [Setting(SettingScope.User, "")]
        public string EmailResultsTextBoxFormat { get; set; }

        [Setting(SettingScope.User, "")]
        public string ViewMainTextBoxFormat { get; set; }

        [Setting(SettingScope.User, "")]
        public string ViewResultsTextBoxFormat { get; set; }


        //**********************************************
        // Not Changable By User
        //**********************************************

        [Setting(SettingScope.User, ';')]
        public char TvWishItemSeparator { get; set; }

        [Setting(SettingScope.User, 100)]
        public int DialogSleep { get; set; }

        [Setting(SettingScope.User, true)]
        public bool WideSkin { get; set; }

        [Setting(SettingScope.User, 1)]
        public int Sort { get; set; }

        [Setting(SettingScope.User, false)]
        public bool SortReverse { get; set; }

        [Setting(SettingScope.User, false)]
        public bool Email { get; set; }

        [Setting(SettingScope.User, false)]
        public bool Deleted { get; set; }

        [Setting(SettingScope.User, false)]
        public bool Conflicts { get; set; }

        [Setting(SettingScope.User, false)]
        public bool Scheduled { get; set; }

        [Setting(SettingScope.User, false)]
        public bool Recorded { get; set; }

        [Setting(SettingScope.User, false)]
        public bool View { get; set; }
        
    }
}