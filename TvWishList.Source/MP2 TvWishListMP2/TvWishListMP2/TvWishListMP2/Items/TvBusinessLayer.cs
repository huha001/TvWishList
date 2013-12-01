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
using System.Linq;
using System.Text;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Plugins.TvWishListMP2.Models;
using Log = TvLibrary.Log.huha.Log;


namespace MediaPortal.Plugins.TvWishList.Items
{
    class TvBusinessLayer 
    {
        //ILogger Log = ServiceRegistration.Get<ILogger>();

        public TvBusinessLayer()
        {
            Log.Debug("TvBusinessLayer class created");
        }


        public Setting GetSetting(string tagName, string defaultValue)
        {
            Log.Debug("TvBusinessLayer:  GetSetting(string tagName="+tagName+", string defaultValue="+defaultValue);

            if (Main_GUI.MyTvProvider == null)
            {
                Main_GUI.MyTvProvider = ServiceRegistration.Get<ITvWishListTVProvider>();
            }

            if (Main_GUI.MyTvProvider.Initialized == false)
            {
                Main_GUI.MyTvProvider.Init();
            }
            ISetting setting = new Setting();

            Main_GUI.MyTvProvider.ReadSetting(tagName, defaultValue, out setting);
            Setting mysetting = new Setting();
            mysetting.Tag = setting.Tag;
            mysetting.Value = setting.Value;

            return mysetting;
            

        }

        public Schedule AddSchedule(int channelId,string title, DateTime start, DateTime end, Int32 ScheduleType)
        {
            Log.Debug("TvBusinessLayer:  GetSetting(int channelId=" + channelId.ToString() + "string title="+title+",DateTime start=" + start.ToString() + ", DateTime end=" + end.ToString() + ",int ScheduleType=" + ScheduleType.ToString());

            Schedule schedule = new Schedule();
            schedule.IdChannel = channelId;
            schedule.ProgramName = title;
            schedule.StartTime = start;
            schedule.EndTime = end;
            schedule.ScheduleType = ScheduleType;

            //retrieve schedule ID from listall!!
            foreach (Schedule myschedule in Schedule.ListAll())
            {
                if ((myschedule.IdChannel == schedule.IdChannel) && (myschedule.ProgramName == schedule.ProgramName) && (myschedule.StartTime == schedule.StartTime) && (myschedule.EndTime == schedule.EndTime) && (myschedule.ScheduleType == schedule.ScheduleType))
                {
                    schedule.Id = myschedule.Id;

                    schedule.PreRecordInterval = myschedule.PreRecordInterval;
                    schedule.PostRecordInterval = myschedule.PostRecordInterval;
                    schedule.MaxAirings = myschedule.MaxAirings;
                    schedule.KeepDate = myschedule.KeepDate;
                    schedule.KeepMethod = myschedule.KeepMethod;
                    schedule.Priority = myschedule.Priority;
                    schedule.RecommendedCard = myschedule.RecommendedCard;
                    schedule.Series = myschedule.Series;
                    break;
                }
            }
            schedule.Persist();

            return schedule;
        }
    }
}
