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

using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities.Cache;

namespace MediaPortal.Plugins.TvWishList.Items
{
    public class Recording 
    {
        
        #region Recording Member

        public string Description { get; set; }
        public DateTime EndTime { get; set; }
        public string EpisodeName { get; set; }
        public string EpisodeNum { get; set; }
        public string EpisodeNumber { get { return SeriesNum + "." + EpisodeNum; } }  //= SeriesNum.EpisodeNum
        public string EpisodePart { get; set; }
        public string FileName { get; set; }
        public string Genre { get; set; }
        public int IdChannel { get; set; }
        public int IdRecording { get; private set; }
        public int Idschedule { get; set; }
        //public int IdServer { get; set; }       
        public int KeepUntil { get; set; }
        public DateTime KeepUntilDate { get; set; }
        public string SeriesNum { get; set; }
        
        public DateTime StartTime { get; set; }
        public int StopTime { get; set; }
        public int TimesWatched { get; set; }
        public string Title { get; set; }

        #endregion Recording Member

        //ILogger Log = ServiceRegistration.Get<ILogger>();

        #region public methods
        public static IList<Recording> ListAll()
        {
            DateTime start = DateTime.Now; //DEBUG PERFORMANCE
            
            IList<Mediaportal.TV.Server.TVDatabase.Entities.Recording> rawrecordingstv = ServiceAgents.Instance.RecordingServiceAgent.ListAllActiveRecordingsByMediaType(Mediaportal.TV.Server.TVDatabase.Entities.Enums.MediaTypeEnum.TV);
            DateTime end = DateTime.Now; //DEBUG PERFORMANCE
            Log.Debug("IList<Recording> ListAll() time=" + end.Subtract(start).TotalSeconds.ToString()); //DEBUG PERFORMANCE
            IList<Recording> allrecordings = new List<Recording>();
            foreach (Mediaportal.TV.Server.TVDatabase.Entities.Recording myrecording in rawrecordingstv)
            {
                Recording newrecording = new Recording();
                newrecording.IdRecording = myrecording.IdRecording;
                newrecording.Title = myrecording.Title;
                newrecording.FileName = myrecording.FileName;
                if (myrecording.IdChannel != null)
                {
                    newrecording.IdChannel = (int)myrecording.IdChannel;
                }
                else
                {
                    newrecording.IdChannel = -1;
                }
                newrecording.StartTime = myrecording.StartTime;
                newrecording.EndTime = myrecording.EndTime;
                newrecording.Description = myrecording.Description;
                newrecording.EpisodeName = myrecording.EpisodeName;
                newrecording.EpisodeNum = myrecording.EpisodeNum;
                newrecording.EpisodePart = myrecording.EpisodePart;
                if (myrecording.ProgramCategory != null)
                    newrecording.Genre = myrecording.ProgramCategory.Category;
                else
                    newrecording.Genre = string.Empty;
                if (myrecording.IdSchedule != null)
                {
                    newrecording.Idschedule = (int)myrecording.IdSchedule;
                }
                else
                {
                    newrecording.Idschedule = -1;
                }
                newrecording.KeepUntil = myrecording.KeepUntil;
                if (myrecording.KeepUntilDate != null)
                {
                    newrecording.KeepUntilDate = (DateTime)myrecording.KeepUntilDate;
                }
                else
                {
                    newrecording.KeepUntilDate = DateTime.ParseExact("2999-01-30_00:00", "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                }
                newrecording.SeriesNum = myrecording.SeriesNum;
                newrecording.StopTime = myrecording.StopTime;
                newrecording.TimesWatched = myrecording.TimesWatched;
                //add whatever you need here
                Log.Debug("myrecording = " + myrecording.Title);
                Log.Debug("myrecording keepuntildate== " + newrecording.KeepUntilDate.ToString());
                allrecordings.Add(newrecording);
                
            }
            DateTime start3 = DateTime.Now; //DEBUG PERFORMANCE
            IList<Mediaportal.TV.Server.TVDatabase.Entities.Recording> rawrecordingsradio = ServiceAgents.Instance.RecordingServiceAgent.ListAllActiveRecordingsByMediaType(Mediaportal.TV.Server.TVDatabase.Entities.Enums.MediaTypeEnum.Radio);
            DateTime end3 = DateTime.Now; //DEBUG PERFORMANCE
            Log.Debug("IList<Recording> ListAll() time=" + end3.Subtract(start3).TotalSeconds.ToString()); //DEBUG PERFORMANCE
            
            foreach (Mediaportal.TV.Server.TVDatabase.Entities.Recording myrecording in rawrecordingsradio)
            {
                Recording newrecording = new Recording();
                newrecording.IdRecording = myrecording.IdRecording;
                newrecording.Title = myrecording.Title;
                newrecording.FileName = myrecording.FileName;
                if (myrecording.IdChannel != null)
                {
                    newrecording.IdChannel = (int)myrecording.IdChannel;
                }
                else
                {
                    newrecording.IdChannel = -1;
                }
                newrecording.StartTime = myrecording.StartTime;
                newrecording.EndTime = myrecording.EndTime;
                newrecording.Description = myrecording.Description;
                newrecording.EpisodeName = myrecording.EpisodeName;
                newrecording.EpisodeNum = myrecording.EpisodeNum;
                newrecording.EpisodePart = myrecording.EpisodePart;
                if (myrecording.ProgramCategory != null)
                    newrecording.Genre = myrecording.ProgramCategory.Category;
                else
                    newrecording.Genre = string.Empty;
                if (myrecording.IdSchedule != null)
                {
                    newrecording.Idschedule = (int)myrecording.IdSchedule;
                }
                else
                {
                    newrecording.Idschedule = -1;
                }
                newrecording.KeepUntil = myrecording.KeepUntil;
                if (myrecording.KeepUntilDate != null)
                {
                    newrecording.KeepUntilDate = (DateTime)myrecording.KeepUntilDate;
                }
                else
                {
                    newrecording.KeepUntilDate = DateTime.ParseExact("2999-01-30_00:00", "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                }
                newrecording.SeriesNum = myrecording.SeriesNum;
                newrecording.StopTime = myrecording.StopTime;
                newrecording.TimesWatched = myrecording.TimesWatched;
                //add whatever you need here


                Log.Debug("myrecording = " + myrecording.Title);
                Log.Debug("myrecording keepuntildate== " + newrecording.KeepUntilDate.ToString());
                allrecordings.Add(newrecording);
            }
            DateTime end2 = DateTime.Now; //DEBUG PERFORMANCE
            Log.Debug("IList<Recording> ListAll() total time=" + end2.Subtract(start).TotalSeconds.ToString()); //DEBUG PERFORMANCE
            return allrecordings;
        }


        public void Persist()
        {
            Log.Debug("Saving Recording");
            Mediaportal.TV.Server.TVDatabase.Entities.Recording myrecording = new Mediaportal.TV.Server.TVDatabase.Entities.Recording();
            myrecording.IdRecording = this.IdRecording;
            myrecording.Title = this.Title;
            myrecording.FileName = this.FileName;
            if (this.IdChannel == -1)
            {
                myrecording.IdChannel = null;
            }
            else
            {
                myrecording.IdChannel = this.IdChannel;
            }
            myrecording.StartTime = this.StartTime;
            myrecording.EndTime = this.EndTime;
            myrecording.Description = this.Description;
            myrecording.EpisodeName = this.EpisodeName;
            myrecording.EpisodeNum = this.EpisodeNum;
            myrecording.EpisodePart = this.EpisodePart;
            if (this.Genre != string.Empty)
            {
                Log.Debug("Creating new program category for recording");
                myrecording.ProgramCategory = new Mediaportal.TV.Server.TVDatabase.Entities.ProgramCategory();
                myrecording.ProgramCategory.Category = this.Genre;
            }
            else
            {
                myrecording.ProgramCategory = null;
            }
            if (this.Idschedule == -1)
            {
                myrecording.IdSchedule = null;
            }
            else
            {
                myrecording.IdSchedule = this.Idschedule;
            }
            myrecording.KeepUntil = this.KeepUntil;
            myrecording.KeepUntilDate = this.KeepUntilDate;
            myrecording.SeriesNum = this.SeriesNum;
            myrecording.StopTime = this.StopTime;
            myrecording.TimesWatched = this.TimesWatched;
            ServiceAgents.Instance.RecordingServiceAgent.SaveRecording(myrecording);

        }
        

        public void Delete()
        {
            Log.Debug("Delete Recording");

            ServiceAgents.Instance.RecordingServiceAgent.DeleteRecording(this.IdRecording);
            
        }

        public static Recording Retrieve(int idRecording)
        {
            Mediaportal.TV.Server.TVDatabase.Entities.Recording myrecording = ServiceAgents.Instance.RecordingServiceAgent.GetRecording(idRecording);
            Recording newrecording = new Recording();
            newrecording.IdRecording = myrecording.IdRecording;
            newrecording.Title = myrecording.Title;
            newrecording.FileName = myrecording.FileName;
            if (myrecording.IdChannel != null)
            {
                newrecording.IdChannel = (int)myrecording.IdChannel;
            }
            else
            {
                newrecording.IdChannel = -1;
            }
            newrecording.StartTime = myrecording.StartTime;
            newrecording.EndTime = myrecording.EndTime;
            newrecording.Description = myrecording.Description;
            newrecording.EpisodeName = myrecording.EpisodeName;
            newrecording.EpisodeNum = myrecording.EpisodeNum;
            newrecording.EpisodePart = myrecording.EpisodePart;
            if (myrecording.ProgramCategory != null)
                newrecording.Genre = myrecording.ProgramCategory.Category;
            else
                newrecording.Genre = string.Empty;
            if (myrecording.IdSchedule != null)
            {
                newrecording.Idschedule = (int)myrecording.IdSchedule;
            }
            else
            {
                newrecording.Idschedule = -1;
            }
            newrecording.KeepUntil = myrecording.KeepUntil;
            if (myrecording.KeepUntilDate != null)
            {
                newrecording.KeepUntilDate = (DateTime)myrecording.KeepUntilDate;
            }
            else
            {
                newrecording.KeepUntilDate = DateTime.ParseExact("2999-01-30_00:00", "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            }
            newrecording.SeriesNum = myrecording.SeriesNum;
            newrecording.StopTime = myrecording.StopTime;
            newrecording.TimesWatched = myrecording.TimesWatched;
            //add whatever you need here

            return newrecording;

        }


        public static IList<Recording> GeneralSqlQuery(string command)
        {
            using (IRecordingRepository recordingRepository = new RecordingRepository(true))
            {
                IEnumerable<Mediaportal.TV.Server.TVDatabase.Entities.Recording> allqueryRecordings = recordingRepository.ObjectContext.ExecuteStoreQuery<Mediaportal.TV.Server.TVDatabase.Entities.Recording>(command);
                recordingRepository.UnitOfWork.SaveChanges();

                IList<Recording> myrecordings = new List<Recording>();
                foreach (Mediaportal.TV.Server.TVDatabase.Entities.Recording myrecording in allqueryRecordings)
                {
                    Recording newrecording = new Recording();
                    newrecording.IdRecording = myrecording.IdRecording;
                    newrecording.Title = myrecording.Title;
                    newrecording.FileName = myrecording.FileName;
                    if (myrecording.IdChannel != null)
                    {
                        newrecording.IdChannel = (int)myrecording.IdChannel;
                    }
                    else
                    {
                        newrecording.IdChannel = -1;
                    }
                    newrecording.StartTime = myrecording.StartTime;
                    newrecording.EndTime = myrecording.EndTime;
                    newrecording.Description = myrecording.Description;
                    newrecording.EpisodeName = myrecording.EpisodeName;
                    newrecording.EpisodeNum = myrecording.EpisodeNum;
                    newrecording.EpisodePart = myrecording.EpisodePart;
                    if (myrecording.ProgramCategory != null)
                        newrecording.Genre = myrecording.ProgramCategory.Category;
                    else
                        newrecording.Genre = string.Empty;
                    if (myrecording.IdSchedule != null)
                    {
                        newrecording.Idschedule = (int)myrecording.IdSchedule;
                    }
                    else
                    {
                        newrecording.Idschedule = -1;
                    }
                    newrecording.KeepUntil = myrecording.KeepUntil;
                    if (myrecording.KeepUntilDate != null)
                    {
                        newrecording.KeepUntilDate = (DateTime)myrecording.KeepUntilDate;
                    }
                    else
                    {
                        newrecording.KeepUntilDate = DateTime.ParseExact("2999-01-30_00:00", "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    newrecording.SeriesNum = myrecording.SeriesNum;
                    newrecording.StopTime = myrecording.StopTime;
                    newrecording.TimesWatched = myrecording.TimesWatched;
                    //add whatever you need here

                    myrecordings.Add(newrecording);
                }
                return myrecordings;
            }
        }


        #endregion public methods
    }
}
