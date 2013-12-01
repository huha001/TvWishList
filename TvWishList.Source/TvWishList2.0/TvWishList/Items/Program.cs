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


using System.Data.Entity;


using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities.Cache;

using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server;
using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishList.Items
{
     

    public class Program 
    {
        
        #region Program Member

        public Program(int idProgram)
        {
            this.IdProgram = idProgram;
        }
        public Program(int idChannel, DateTime startTime, DateTime endTime, string title, string description, string genre, Program.ProgramState state, DateTime originalAirDate, string seriesNum, string episodeNum, string episodeName, string episodePart, int starRating, string classification, int parentalRating)
        {
            this.IdChannel = idChannel;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.Title = title;
            this.Description = description;
            this.Genre = genre;
            this.State = state;
            this.OriginalAirDate = originalAirDate;
            this.SeriesNum = seriesNum;
            this.EpisodeNum = episodeNum;
            this.EpisodeName = EpisodeName;
            this.EpisodePart = episodePart;
            this.StarRating = starRating;
            this.Classification = classification;
            this.ParentalRating = parentalRating;
        }

        public Program(int idProgram, int idChannel, DateTime startTime, DateTime endTime, string title, string description, string genre, Program.ProgramState state, DateTime originalAirDate, string seriesNum, string episodeNum, string episodeName, string episodePart, int starRating, string classification, int parentalRating)
        {
            this.IdProgram = idProgram;
            this.IdChannel = idChannel;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.Title = title;
            this.Description = description;
            this.Genre = genre;
            this.State = state;
            this.OriginalAirDate = originalAirDate;
            this.SeriesNum = seriesNum;
            this.EpisodeNum = episodeNum;
            this.EpisodeName = EpisodeName;
            this.EpisodePart = episodePart;
            this.StarRating = starRating;
            this.Classification = classification;
            this.ParentalRating = parentalRating;
        }

        public string Classification { get; set; }
        public string Description { get; set; }
        public DateTime EndTime { get; set; }
        public string EpisodeName { get; set; }
        public string EpisodeNum { get; set; }
        public string EpisodeNumber { get { return SeriesNum + "." + EpisodeNum; } }  //= SeriesNum.EpisodeNum
        public string EpisodePart { get; set; }
        public string Genre { get; set; }
        public bool HasConflict { get; set; }
        public int IdChannel { get; set; }
        public int IdProgram { get; private set; }
        public bool IsChanged { get; private set; }
        public bool IsPartialRecordingSeriesPending { get; set; }
        public bool IsRecording { get; private set; }
        public bool IsRecordingManual { get; set; }
        public bool IsRecordingOnce { get; set; }
        public bool IsRecordingOncePending { get; set; }
        public bool IsRecordingSeries { get; set; }
        public bool IsRecordingSeriesPending { get; set; }
        public bool Notify { get; set; }
        public DateTime OriginalAirDate { get; set; }
        public int ParentalRating { get; set; }
        public string SeriesNum { get; set; }
        public int StarRating { get; set; }
        public DateTime StartTime { get; set; }
        public Program.ProgramState State { get; set; }
        public string Title { get; set; }
        
               
        #endregion Schedule Member

        //ILogger Log = ServiceRegistration.Get<ILogger>();

        #region public methods
        //testprogram = Program.RetrieveByTitleTimesAndChannel(oneschedule.ProgramName, oneschedule.StartTime, oneschedule.EndTime, oneschedule.IdChannel);
        public static Program RetrieveByTitleTimesAndChannel(string Title, DateTime StartTime, DateTime EndTime, int IdChannel)
        {
            Log.Debug("Program RetrieveByTitleTimesAndChannel");
            Log.Debug("Title=" + Title);
            Log.Debug("StartTime=" + StartTime.ToString());
            Log.Debug("EndTime=" + EndTime.ToString());
            Log.Debug("IdChannel=" + IdChannel.ToString());

            IList<Mediaportal.TV.Server.TVDatabase.Entities.Program> rawprograms = ServiceAgents.Instance.ProgramServiceAgent.GetProgramsByChannelAndTitleAndStartEndTimes(IdChannel, Title, StartTime, EndTime);
            Log.Debug(rawprograms.Count.ToString() + " programs found");


            if (rawprograms.Count == 1)// redo general query to get all data
            {
                Log.Debug("EpisodeName from GetProgramsByChannelAndTitleAndStartEndTimes=" + rawprograms[0].EpisodeName);

                string command = String.Format("select * from programs where ( idprogram = '{0}' ) ", rawprograms[0].IdProgram.ToString());
                Log.Debug(command);
                IList<Program> myprograms = Program.GeneralSqlQuery(command);
                Log.Debug(myprograms.Count.ToString()+" programs found from sql query");
                if (myprograms.Count == 1)
                {
                    Log.Debug("EpisodeName from direct SQL query=" + myprograms[0].EpisodeName);
                    return myprograms[0];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }



            /*
            if (rawprograms.Count > 0)
            {
                TVDatabase.Entities.Program rawprogram = rawprograms[0];
                string genre;
                if (rawprogram.ProgramCategory != null)
                    genre = rawprogram.ProgramCategory.Category;
                else
                    genre = "";
                Program myprogram = new Program((int)rawprogram.IdProgram, (int)rawprogram.IdChannel, (DateTime)rawprogram.StartTime, (DateTime)rawprogram.EndTime, rawprogram.Title, rawprogram.Description, genre, (ProgramState)rawprogram.State, (DateTime)rawprogram.OriginalAirDate, rawprogram.SeriesNum, rawprogram.EpisodeNum, rawprogram.EpisodeName, rawprogram.EpisodePart, (int)rawprogram.StarRating, rawprogram.Classification, (int)rawprogram.ParentalRating);


                Log.Debug("rawProgramTitle=" + rawprogram.Title);
                Log.Debug("FoundProgramStartTime=" + rawprogram.StartTime.ToString());
                Log.Debug("FoundProgramEndTime=" + rawprogram.EndTime.ToString());
                Log.Debug("FoundProgramIdChannel=" + rawprogram.IdChannel.ToString());
                Log.Debug("FoundProgramEpisodeName=" + rawprogram.EpisodeName);
                Log.Debug("FoundProgramEpisodePart=" + rawprogram.EpisodePart);
                
                
                Log.Debug("FoundProgramTitle=" + myprogram.Title);
                Log.Debug("FoundProgramStartTime=" + myprogram.StartTime.ToString());
                Log.Debug("FoundProgramEndTime=" + myprogram.EndTime.ToString());
                Log.Debug("FoundProgramIdChannel=" + myprogram.IdChannel.ToString());
                Log.Debug("FoundProgramEpisodeName=" + myprogram.EpisodeName);
                Log.Debug("FoundProgramEpisodePart=" + myprogram.EpisodePart);
                return myprogram;
            }
            else
            {
                return null;
            }*/
            
        }

        //alternativeprograms = Program.RetrieveEveryTimeOnThisChannel(oneschedule.ProgramName, oneschedule.IdChannel);
        public static IList<Program> RetrieveEveryTimeOnThisChannel(string ProgramName, int IdChannel)
        {
            IList<Mediaportal.TV.Server.TVDatabase.Entities.Program> rawprograms = ServiceAgents.Instance.ProgramServiceAgent.GetProgramsByTitle(ProgramName, Mediaportal.TV.Server.TVDatabase.Entities.Enums.StringComparisonEnum.StartsWith);

            IList<Program> myprograms= new List<Program>();

            foreach (Mediaportal.TV.Server.TVDatabase.Entities.Program rawprogram in rawprograms)
            {
                if (rawprogram.IdChannel == IdChannel)
                {
                    string genre;
                    if (rawprogram.ProgramCategory != null)
                        genre = rawprogram.ProgramCategory.Category;
                    else
                        genre = "";
                    Program myprogram = new Program((int)rawprogram.IdProgram, (int)rawprogram.IdChannel, (DateTime)rawprogram.StartTime, (DateTime)rawprogram.EndTime, rawprogram.Title, rawprogram.Description, genre, (ProgramState)rawprogram.State, (DateTime)rawprogram.OriginalAirDate, rawprogram.SeriesNum, rawprogram.EpisodeNum, rawprogram.EpisodeName, rawprogram.EpisodePart, (int)rawprogram.StarRating, rawprogram.Classification, (int)rawprogram.ParentalRating);
                    myprograms.Add(myprogram);
                }
            }
            return myprograms;
        }


        public static IList<Program> GeneralSqlQuery(string command)
        {
            using (IProgramRepository programRepository = new ProgramRepository(true))
            {
                IEnumerable<Mediaportal.TV.Server.TVDatabase.Entities.Program> allqueryprograms = programRepository.ObjectContext.ExecuteStoreQuery<Mediaportal.TV.Server.TVDatabase.Entities.Program>(command);
                programRepository.UnitOfWork.SaveChanges();
                IList<Program> myprograms = new List<Program>();
                foreach (Mediaportal.TV.Server.TVDatabase.Entities.Program rawprogram in allqueryprograms)
                {
                    Program myprogram = new Program(rawprogram.IdProgram);
                    myprogram.IdChannel = rawprogram.IdChannel;
                    myprogram.StartTime = rawprogram.StartTime;
                    myprogram.EndTime = rawprogram.EndTime;
                    myprogram.Title = rawprogram.Title;
                    myprogram.Description = rawprogram.Description;
                    if (rawprogram.ProgramCategory != null)
                        myprogram.Genre = rawprogram.ProgramCategory.Category;
                    else
                        myprogram.Genre = string.Empty;
                    try
                    {
                        myprogram.State = (Program.ProgramState)rawprogram.State; // rawprogram.State;
                    }
                    catch
                    {
                        myprogram.State = Program.ProgramState.None;
                    }
                    if (rawprogram.OriginalAirDate != null)
                    {
                        myprogram.OriginalAirDate = (DateTime)rawprogram.OriginalAirDate;
                    }
                    else
                    {
                        myprogram.OriginalAirDate = DateTime.ParseExact("2000-01-31_00:00", "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    }                    
                    myprogram.SeriesNum = rawprogram.SeriesNum;
                    myprogram.EpisodeNum = rawprogram.EpisodeNum;
                    myprogram.EpisodeName = rawprogram.EpisodeName;
                    myprogram.EpisodePart = rawprogram.EpisodePart;
                    myprogram.StarRating = rawprogram.StarRating;
                    myprogram.Classification = rawprogram.Classification;
                    myprogram.ParentalRating = rawprogram.ParentalRating;

                    /*Program myprogram = new Program(rawprogram.IdProgram, rawprogram.IdChannel, rawprogram.StartTime, rawprogram.EndTime, rawprogram.Title, 
                                                    rawprogram.Description, rawprogram.ProgramCategory.Category, 0, (DateTime)rawprogram.OriginalAirDate, 
                                                    rawprogram.SeriesNum, rawprogram.EpisodeNum, rawprogram.EpisodeName, rawprogram.EpisodePart, rawprogram.StarRating, 
                                                    rawprogram.Classification, rawprogram.ParentalRating);*/
                    myprograms.Add(myprogram);
                }
                return myprograms;
            }
        }


        
        

        #endregion public methods

        public enum ProgramState
        {
            None = 0,
            Notify = 1,
            RecordOnce = 2,
            RecordSeries = 4,
            RecordManual = 8,
            Conflict = 16,
            RecordOncePending = 32,
            RecordSeriesPending = 64,
            PartialRecordSeriesPending = 128,
        }

        
    }
}
