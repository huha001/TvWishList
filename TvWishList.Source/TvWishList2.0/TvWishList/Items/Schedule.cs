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
using System.Linq;
using System.Text;
using System;

using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server;
using Log = TvLibrary.Log.huha.Log;
//using SetupTv.Sections;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities.Cache;

using MediaPortal.Plugins.TvWishList.Setup;

namespace MediaPortal.Plugins.TvWishList.Items
{
    public class Schedule 
    {
        public Schedule(int idChannel, string programName, DateTime startTime, DateTime endTime)
        {
            this.IdChannel = idChannel;
            this.ProgramName = programName;
            this.StartTime = startTime;
            this.EndTime = endTime;
        }
        public Schedule(int idChannel, int scheduleType, string programName, DateTime startTime, DateTime endTime, int maxAirings, int priority, string directory, int quality, int keepMethod, DateTime keepDate, int preRecordInterval, int postRecordInterval, DateTime canceled)
        {
            this.IdChannel = idChannel;
            //this.IdParentSchedule = idParentSchedule;
            this.ScheduleType = scheduleType;
            this.ProgramName = programName;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.MaxAirings = maxAirings;
            this.Priority = priority;
            this.Directory = directory;
            this.Quality = quality;
            this.KeepMethod = keepMethod;
            this.KeepDate = keepDate;
            this.PreRecordInterval = preRecordInterval;
            this.PostRecordInterval = postRecordInterval;
            this.Canceled = canceled;
        }
        public Schedule(int idSchedule, int idChannel, int scheduleType, string programName, DateTime startTime, DateTime endTime, int maxAirings, int priority, string directory, int quality, int keepMethod, DateTime keepDate, int preRecordInterval, int postRecordInterval, DateTime canceled)
        {
            this.IdSchedule = idSchedule;
            //this.IdParentSchedule = idParentSchedule;
            this.IdChannel = idChannel;
            this.ScheduleType = scheduleType;
            this.ProgramName = programName;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.MaxAirings = maxAirings;
            this.Priority = priority;
            this.Directory = directory;
            this.Quality = quality;
            this.KeepMethod = keepMethod;
            this.KeepDate = keepDate;
            this.PreRecordInterval = preRecordInterval;
            this.PostRecordInterval = postRecordInterval;
            this.Canceled = canceled;
        }

        #region Schedule Member
        public DateTime Canceled { get; set; }
        public string Directory { get; set; }
        //public bool DoesUseEpisodeManagement { get; set; }
        public DateTime EndTime { get; set; }
        public int IdChannel { get; set; }
        public int IdParentSchedule { get; set; }
        public int IdSchedule { get; private set; }        
        public DateTime KeepDate { get; set; }
        public int KeepMethod { get; set; }
        public int MaxAirings { get; set; }
        public int PostRecordInterval { get; set; }
        public int PreRecordInterval { get; set; }
        public int Priority { get; set; }
        public string ProgramName { get; set; }
        public int Quality { get; set; }
        
        public int RecommendedCard { get; set; }
        public int ScheduleType { get; set; }
        public bool Series { get; set; }
        public DateTime StartTime { get; set; }
                            
        #endregion Schedule Member

        //ILogger Log = ServiceRegistration.Get<ILogger>();

        #region public methods
        public static IList<Schedule> ListAll()
        {
            IList<Mediaportal.TV.Server.TVDatabase.Entities.Schedule> rawschedules = ServiceAgents.Instance.ScheduleServiceAgent.ListAllSchedules();

            IList<Schedule> allschedules = new List<Schedule>();
            foreach (Mediaportal.TV.Server.TVDatabase.Entities.Schedule myschedule in rawschedules)
            {
                Schedule newschedule = new Schedule(myschedule.IdChannel, myschedule.ProgramName, myschedule.StartTime, myschedule.EndTime);
                //newschedule.Canceled = myschedule.Canceled; bug
                newschedule.Directory = myschedule.Directory;
                //newschedule.IdParentSchedule = (int)myschedule.IdParentSchedule ; bug
                newschedule.IdSchedule = myschedule.IdSchedule ;
                if (myschedule.KeepDate != null)
                {
                    newschedule.KeepDate = (DateTime)myschedule.KeepDate;
                }
                else
                {
                    newschedule.KeepDate = DateTime.ParseExact("2999-01-30_00:00", "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                }
                newschedule.KeepMethod = myschedule.KeepMethod ;
                newschedule.MaxAirings = myschedule.MaxAirings ;
                newschedule.PostRecordInterval = myschedule.PostRecordInterval ;
                newschedule.PreRecordInterval = myschedule.PreRecordInterval ;
                newschedule.Priority = myschedule.Priority ;
                newschedule.Quality = myschedule.Quality;
                newschedule.ScheduleType = myschedule.ScheduleType;
                newschedule.Series = myschedule.Series;
                //add whatever you need here
                newschedule.RecommendedCard = 0;  //no more supported set everything to default 0
                
                //Log.Debug("schedule = " + newschedule.ProgramName);
                allschedules.Add(newschedule);
            }
            return allschedules;   
        }

        public static void DebugSchedules()
        {
            IList<Mediaportal.TV.Server.TVDatabase.Entities.Schedule> rawschedules = ServiceAgents.Instance.ScheduleServiceAgent.ListAllSchedules();

            foreach (Mediaportal.TV.Server.TVDatabase.Entities.Schedule myschedule in rawschedules)
            {
                Log.Debug("**************************SCHEDULE**************************");
                Log.Debug("ProgramName=" + myschedule.ProgramName);
                Log.Debug("**************************SCHEDULE**************************");
                Log.Debug("Canceled="+myschedule.Canceled.ToString());
                if (myschedule.CanceledSchedules != null)
                    Log.Debug("CanceledSchedules.count=" + myschedule.CanceledSchedules.Count.ToString());
                else
                    Log.Debug("CanceledSchedules=null");

                if (myschedule.ChangeTracker != null)
                    Log.Debug("ChangeTracker.ChangeTrackingEnabled=" + myschedule.ChangeTracker.ChangeTrackingEnabled.ToString());
                else
                    Log.Debug("ChangeTracker=null");

                if (myschedule.Channel != null)
                    Log.Debug("Channel.DisplayName=" + myschedule.Channel.DisplayName);
                else
                    Log.Debug("Channel=null");

                if (myschedule.ConflictingSchedules != null)
                    Log.Debug("ConflictingSchedules.Count=" + myschedule.ConflictingSchedules.Count.ToString());
                else
                    Log.Debug("ConflictingSchedules=null");

                if (myschedule.Conflicts != null)
                    Log.Debug("Conflicts.Count=" + myschedule.Conflicts.Count.ToString());
                else
                    Log.Debug("Conflicts=null");

                Log.Debug("Directory=" + myschedule.Directory);
                Log.Debug("EndTime=" + myschedule.EndTime.ToString());
                Log.Debug("IdChannel=" + myschedule.IdChannel.ToString());
                Log.Debug("IdParentSchedule=" + myschedule.IdParentSchedule.ToString());
                Log.Debug("IdSchedule=" + myschedule.IdSchedule.ToString());
                Log.Debug("KeepDate=" + myschedule.KeepDate.ToString());
                Log.Debug("KeepMethod=" + myschedule.KeepMethod.ToString());
                Log.Debug("MaxAirings=" + myschedule.MaxAirings.ToString());

                if (myschedule.ParentSchedule != null)
                    Log.Debug("ParentSchedule.IdSchedule=" + myschedule.ParentSchedule.IdSchedule.ToString());
                else
                    Log.Debug("ParentSchedule=null");

                Log.Debug("PostRecordInterval=" + myschedule.PostRecordInterval.ToString());
                Log.Debug("PreRecordInterval=" + myschedule.PreRecordInterval.ToString());
                Log.Debug("Priority=" + myschedule.Priority.ToString());
                Log.Debug("ProgramName=" + myschedule.ProgramName);
                Log.Debug("Quality=" + myschedule.Quality.ToString());

                if (myschedule.Recordings != null)
                    Log.Debug("Recordings.Count=" + myschedule.Recordings.Count.ToString());
                else
                    Log.Debug("Recordings=null");

                if (myschedule.Schedules != null)
                    Log.Debug("Schedules.Count=" + myschedule.Schedules.Count.ToString());
                else
                    Log.Debug("Schedules=null");

                Log.Debug("ScheduleType=" + myschedule.ScheduleType.ToString());
                Log.Debug("Series=" + myschedule.Series.ToString());
                Log.Debug("StartTime=" + myschedule.StartTime.ToString());

            }
                
        }



        public void Persist()
        {
            Log.Debug("Persist Schedule");
            Mediaportal.TV.Server.TVDatabase.Entities.Schedule myschedule = new Mediaportal.TV.Server.TVDatabase.Entities.Schedule();
            myschedule.Canceled = DateTime.ParseExact("2000-01-01_00:00", "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            myschedule.CanceledSchedules = new Mediaportal.TV.Server.TVDatabase.Entities.TrackableCollection<Mediaportal.TV.Server.TVDatabase.Entities.CanceledSchedule>();
            myschedule.ChangeTracker = new Mediaportal.TV.Server.TVDatabase.Entities.ObjectChangeTracker();
            myschedule.Channel = ServiceAgents.Instance.ChannelServiceAgent.GetChannel(this.IdChannel);
            //myschedule.ConflictingSchedules
            //myschedule.Conflicts
            myschedule.Directory = "";
            myschedule.EndTime = EndTime;
            myschedule.IdChannel = IdChannel;
            //myschedule.IdParentSchedule
            //myschedule.IdSchedule
            myschedule.KeepDate = KeepDate;
            myschedule.KeepMethod = KeepMethod;
            myschedule.MaxAirings = MaxAirings;
            myschedule.ParentSchedule = null;
            myschedule.PostRecordInterval = PostRecordInterval;
            myschedule.PreRecordInterval = PreRecordInterval;
            myschedule.Priority = Priority;
            myschedule.ProgramName = ProgramName;
            myschedule.Quality = Quality;
            //myschedule.Recordings = new TVDatabase.Entities.TrackableCollection<Mediaportal.TV.Server.TVDatabase.Entities.Recording>(); BUG
            //myschedule.Schedules = new TVDatabase.Entities.TrackableCollection<Mediaportal.TV.Server.TVDatabase.Entities.Schedule>(); Bug
            myschedule.ScheduleType = 0;
            myschedule.Series = Series;
            myschedule.StartTime = StartTime;
            
            Log.Debug("before myschedule.IdSchedule=" + myschedule.IdSchedule.ToString());
            
            if (TvWishListSetup.Setup)
            {
                ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(myschedule);
                Log.Debug("Added schedule by Setup: " + myschedule.ProgramName);
            }
            else//Tv server schedules
            {
                try
                {
                    ScheduleManagement.SaveSchedule(myschedule);
                    //ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(myschedule);

                }
                catch (Exception exc)
                {
                    Log.Debug("Exception=" + exc.Message);
                    return;
                }
                Log.Debug("Added schedule by Server: " + myschedule.ProgramName);
            }
            IdSchedule = myschedule.IdSchedule;
            Log.Debug("after myschedule.IdSchedule=" + myschedule.IdSchedule.ToString());
            
        }

        public void Delete()
        {
            ServiceAgents.Instance.ScheduleServiceAgent.DeleteSchedule(this.IdSchedule);
            Log.Debug("Deleted schedule " + this.ProgramName);

        }

        public static void Delete(int idSchedule)
        {
            ServiceAgents.Instance.ScheduleServiceAgent.DeleteSchedule(idSchedule);
            Log.Debug("Deleted schedule with id=" + idSchedule.ToString());

        }

        [CLSCompliant(false)]
        public Channel ReferencedChannel()
        { //return channel of schedule
            return Channel.Retrieve(this.IdChannel);
        }

        public static IList<Schedule> GetConflictingSchedules(Schedule oneschedule)
        {
            Log.Debug("reference schedule = " + oneschedule.ProgramName + " start=" + oneschedule.StartTime.ToString() + " end=" + oneschedule.EndTime.ToString() + " channel=" + oneschedule.IdChannel);
            Log.Debug("total schedules = "+Schedule.ListAll().Count.ToString());

            Mediaportal.TV.Server.TVDatabase.Entities.Schedule myrawschedule = Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.ScheduleManagement.GetSchedule(oneschedule.IdSchedule);
            List<Mediaportal.TV.Server.TVDatabase.Entities.Schedule> notViewableSchedules = new List<Mediaportal.TV.Server.TVDatabase.Entities.Schedule>();
            IList<Mediaportal.TV.Server.TVDatabase.Entities.Schedule> conflicts = Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.ScheduleManagementhuha.GetConflictingSchedules(myrawschedule, out notViewableSchedules);

            IList<Schedule> myschedules = new List<Schedule>();
            foreach (Mediaportal.TV.Server.TVDatabase.Entities.Schedule rawschedule in conflicts)
            {
                //public Schedule(int idSchedule, int idChannel, int scheduleType, string programName, DateTime startTime, DateTime endTime, int maxAirings, int priority, string directory, int quality, int keepMethod, DateTime keepDate, int preRecordInterval, int postRecordInterval, DateTime canceled)
                DateTime keepDate;
                if (rawschedule.KeepDate != null)
                {
                    keepDate = (DateTime)rawschedule.KeepDate;
                }
                else
                {
                    keepDate = DateTime.ParseExact("2999-01-30_00:00", "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                }

                


                Schedule myschedule = new Schedule(rawschedule.IdSchedule, rawschedule.IdChannel, rawschedule.ScheduleType, rawschedule.ProgramName,
                                                   rawschedule.StartTime, rawschedule.EndTime, rawschedule.MaxAirings, rawschedule.Priority, rawschedule.Directory, rawschedule.Quality,
                                                   rawschedule.KeepMethod, keepDate, rawschedule.PreRecordInterval, rawschedule.PostRecordInterval, rawschedule.Canceled);

                myschedules.Add(myschedule);
            }
            return myschedules;
        }

        public static IList<Schedule> GeneralSqlQuery(string command)
        {
            using (IScheduleRepository scheduleRepository = new ScheduleRepository(true))
            {
                IEnumerable<Mediaportal.TV.Server.TVDatabase.Entities.Schedule> allqueryschedules = scheduleRepository.ObjectContext.ExecuteStoreQuery<Mediaportal.TV.Server.TVDatabase.Entities.Schedule>(command);
                scheduleRepository.UnitOfWork.SaveChanges();
                IList<Schedule> myschedules = new List<Schedule>();
                foreach (Mediaportal.TV.Server.TVDatabase.Entities.Schedule rawschedule in allqueryschedules)
                {
                    //public Schedule(int idSchedule, int idChannel, int scheduleType, string programName, DateTime startTime, DateTime endTime, int maxAirings, int priority, string directory, int quality, int keepMethod, DateTime keepDate, int preRecordInterval, int postRecordInterval, DateTime canceled)
                    DateTime keepDate;
                    if (rawschedule.KeepDate != null)
                    {
                        keepDate = (DateTime)rawschedule.KeepDate;
                    }
                    else
                    {
                        keepDate = DateTime.ParseExact("2999-01-30_00:00", "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    }


                    Schedule myschedule = new Schedule(rawschedule.IdSchedule, rawschedule.IdChannel, rawschedule.ScheduleType, rawschedule.ProgramName, 
                                                       rawschedule.StartTime, rawschedule.EndTime, rawschedule.MaxAirings, rawschedule.Priority, rawschedule.Directory, rawschedule.Quality,
                                                       rawschedule.KeepMethod, keepDate, rawschedule.PreRecordInterval, rawschedule.PostRecordInterval, rawschedule.Canceled);

                    myschedules.Add(myschedule);
                }
                return myschedules;
            }
        }

        #endregion public methods
    }
}
