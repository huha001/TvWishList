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

namespace MediaPortal.Plugins.TvWishListMP2.MPExtended
{
    public class Schedule : ISchedule
    {
        #region Schedule Member

        public int Id { get; set; }
        public string ProgramName { get; set; }
        public int IdChannel { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int ScheduleType { get; set; }

        public int PreRecordInterval { get; set; }
        public int PostRecordInterval { get; set; }
        public int MaxAirings { get; set; }
        public DateTime KeepDate { get; set; }
        public int KeepMethod { get; set; }
        public int Priority { get; set; }
        public int RecommendedCard { get; set; }
        public bool Series { get; set; }
                                       
        #endregion Schedule Member

        //ILogger Log = ServiceRegistration.Get<ILogger>();

        #region public methods
        public static IList<Schedule> ListAll()
        {
            IList<ISchedule> allISchedules = null;

            if (Main_GUI.MyTvProvider == null)
            {
                Main_GUI.MyTvProvider = ServiceRegistration.Get<ITvWishListTVProvider>();
            }

            if (Main_GUI.MyTvProvider.Initialized == false)
            {
                Main_GUI.MyTvProvider.Init();
            }

            Main_GUI.MyTvProvider.ReadAllSchedules(out allISchedules);

            IList<Schedule> allschedules = new List<Schedule>();
            foreach (ISchedule myschedule in allISchedules)
            {
                allschedules.Add(new Schedule { ProgramName = myschedule.ProgramName, 
                                                    Id = myschedule.Id, 
                                                    IdChannel = myschedule.IdChannel, 
                                                    StartTime = myschedule.StartTime, 
                                                    EndTime = myschedule.EndTime, 
                                                    ScheduleType = (int)myschedule.ScheduleType,
                                                    PreRecordInterval = myschedule.PreRecordInterval,
                                                    PostRecordInterval = myschedule.PostRecordInterval,
                                                    MaxAirings = myschedule.MaxAirings,
                                                    KeepDate = myschedule.KeepDate,
                                                    KeepMethod = (int)myschedule.KeepMethod,
                                                    Priority = myschedule.Priority,
                                                    Series = myschedule.Series });
            }
            return allschedules;
            
        }

        public void Persist()
        {
            if (Main_GUI.MyTvProvider == null)
            {
                Main_GUI.MyTvProvider = ServiceRegistration.Get<ITvWishListTVProvider>();
            }

            if (Main_GUI.MyTvProvider.Initialized == false)
            {
                Main_GUI.MyTvProvider.Init();
            }

            Main_GUI.MyTvProvider.NewSchedule = this;
            Main_GUI.MyTvProvider.ScheduleNew();
        }

        public void Delete()
        {
            if (Main_GUI.MyTvProvider == null)
            {
                Main_GUI.MyTvProvider = ServiceRegistration.Get<ITvWishListTVProvider>();
            }

            if (Main_GUI.MyTvProvider.Initialized == false)
            {
                Main_GUI.MyTvProvider.Init();
            }

            Main_GUI.MyTvProvider.NewSchedule = this;
            Main_GUI.MyTvProvider.ScheduleDelete();
        }

        #endregion public methods
    }
}
