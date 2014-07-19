#region Copyright (C)
/* 
 *	Copyright (C) 2005-2012 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion Copyright (C)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Mail;
using System.Globalization;
using System.Xml;
using Log = TvLibrary.Log.huha.Log;
//using TvLibrary.Log.huha;
using TvLibrary.Interfaces;
using MediaPortal.Plugins.TvWishList.Setup;
using TvEngine;

#if (MPTV2)
// native TV3.5 for MP2
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.SetupControls;
using MediaPortal.Common.Utils;

using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;

/*
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities.Cache;*/
 
using MediaPortal.Plugins.TvWishList.Items;

using Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.Interfaces;


#else 
using TvControl;
using TvEngine.Events;
using TvLibrary.Implementations;
using TvDatabase;
using SetupTv;
#endif

using TvEngine.PowerScheduler.Interfaces;

using MediaPortal.Plugins;
//using TvWishList;


using Microsoft.Win32;

using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;



namespace MediaPortal.Plugins.TvWishList
{
    /// <summary>
    /// base class for tv-server plugins
    /// </summary>
    ///

    public class TvWishList : ITvServerPlugin
    {
   
        #region Members
        EpgParser epgwatchclass = null; 
        public const byte NO_SSL = 0;
        public const byte SSL_PORT = 1;
        public const byte STLS = 2;
        
        bool runpolling = true;
        int _recording = 0;
        bool _timeshifting = false;
        bool _useRecordingFlag = true;
        bool _runEPGScript = false;

        bool _ProcessFileRenamingRunning = false;

        DateTime _RecordingFlagTime = DateTime.Now;

        bool DEBUG = false;
        string TV_USER_FOLDER="NOTFOUND";
        //string filewatcherstartepg = "NOTFOUND";
        //string filewatcherfinishedepg = "NOTFOUND";
        string filewatcherSetupTvStarted = "NOTFOUND";
        string filewatcherNextEpgCheck = "NOTFOUND";

        ITvServerEvent events = null;
        //FileSystemWatcher StartEPGsearch = new FileSystemWatcher();
        FileSystemWatcher SetupTvStarted = new FileSystemWatcher();
        FileSystemWatcher NextEpgCheck = new FileSystemWatcher();

        static DateTime _NextEpgTime = System.DateTime.Now;
        static DateTime _PreviousEpgTime = System.DateTime.Now;

        string FileRenameXML = "FileRenameXML.xml"; //global file containing all file renaming jobs

        string ProcessName1 = "comskip";
        string ProcessName2 = "dummy process (ignore this message)";
        int ComSkipMaxWait = 60; //maximum comskip waiting 60 minutes
        

        System.Threading.Thread receivethread = null;

        //Server Pipe
        System.Threading.Thread ServerThread = null;
        System.Threading.Thread ServerTimeOutCounter = null;
        NamedPipeServerStream pipeServer = null;
        NamedPipeClientStream pipeClient = null;
        bool NewServerMessage = false;
        string ServerMessage = string.Empty;
        string TimeOutValuestring = "60";
        bool PipeServerActive = true;
        bool PipeServerBusy = false;
        string MessageFromClient = string.Empty;

        


        #endregion Members


        #region properties
        /// <summary>
        /// returns the name of the plugin
        /// </summary>
        public string Name
        {
            get { return "TvWishList"; }
        }

        /// <summary>
        /// returns the version of the plugin
        /// </summary>
        public string Version { get { return "1.4.0.1"; } }

        /// <summary>
        /// returns the author of the plugin
        /// </summary>
        public string Author { get { return "huha"; } }

        /// <summary>
        /// returns if the plugin should only run on the master server
        /// or also on slave servers
        /// </summary>
        public bool MasterOnly { get { return false; } }

      
        #endregion

        #region Methods
        /// <summary>
        /// Starts the plugin
        /// </summary>
        [CLSCompliant(false)]

#if (MPTV2)
        public void Start(IInternalControllerService controller)
#else
        public void Start(IController controller)
#endif
        
        {            
            try
            {
                Log.Info("TvWishList started");

                SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
                SystemEvents.SessionEnded += new SessionEndedEventHandler(SystemEvents_SessionEnded);

                events = GlobalServiceProvider.Instance.Get<ITvServerEvent>();
                events.OnTvServerEvent += new TvServerEventHandler(events_OnTvServerEvent);

                TvBusinessLayer layer = new TvBusinessLayer();
                Setting setting = null; 
                //set BUSY = false
                setting = layer.GetSetting("TvWishList_BUSY", "false");
                setting.Value = "false";
                setting.Persist();

                

                setting = layer.GetSetting("TvWishList_Debug", "false");
                Log.Info("setting DEBUG MODE = " + setting.Value);
                

                DEBUG = false;
                Boolean.TryParse(setting.Value, out DEBUG);
                Log.DebugValue = DEBUG;

                Log.Info("DEBUG MODE = " + DEBUG.ToString());

                //unlock pluginversion
                setting = layer.GetSetting("TvWishList_LockingPluginname", "NONE");
                setting.Value = "NONE";
                setting.Persist();

                //write Tv version
                setting = layer.GetSetting("TvWishList_TvServerVersion", "0.0.0.0");
                setting.Value = Version;
                setting.Persist();
                Log.Debug("TvVersion = "+Version);


                //is host name stored in database for TvWishListMP?
                setting = layer.GetSetting("TvWishList_MachineName", "NONE");
                Log.Debug("TvServer Machine Name="+setting.Value);
                if (setting.Value != System.Environment.MachineName.ToString())
                {
                    setting.Value = System.Environment.MachineName.ToString();
                    setting.Persist();
                    Log.Debug("Overwriting TvServer Machine Name To " + setting.Value);
                }

                //save pipename for Tvserver2
                setting = layer.GetSetting("TvWishList_PipeName", "NONE");
                Log.Debug("TvWishList_PipeName=" + setting.Value);
#if (MPTV2)
                setting.Value = "MP2TvWishListPipe";
#else
                setting.Value = "TvWishListPipe";
#endif
                setting.Persist();
                

                TV_USER_FOLDER = layer.GetSetting("TvWishList_TV_USER_FOLDER", "NOT_FOUND").Value;
                if ((File.Exists(TV_USER_FOLDER + @"\TvService.exe") == true) || (Directory.Exists(TV_USER_FOLDER) == false))
                {
                    //autodetect paths
                    InstallPaths instpaths = new InstallPaths();  //define new instance for folder detection
#if (MPTV2) //Native MP2 Tv server
                    instpaths.GetInstallPathsMP2();
                    TV_USER_FOLDER = instpaths.TV2_USER_FOLDER;
#else
                    instpaths.GetInstallPaths();
                    TV_USER_FOLDER = instpaths.TV_USER_FOLDER;
#endif
                    Logdebug("TV server user folder detected at " + TV_USER_FOLDER); 

                    if ((File.Exists(TV_USER_FOLDER + @"\TvService.exe") == true) || (Directory.Exists(TV_USER_FOLDER) == false))
                    {
                        Log.Error(@" TV server user folder does not exist - using C:\MediaPortal\TvWishList");
                        Logdebug(@" TV server user folder does not exist - using C:\MediaPortal\TvWishList");
                        TV_USER_FOLDER = @"C:\MediaPortal";
                        if (Directory.Exists(TV_USER_FOLDER) == false)
                            Directory.CreateDirectory(TV_USER_FOLDER + @"\TvWishList");
                    }
                    else
                    {//store found TV_USER_FOLDER
                        setting = layer.GetSetting("TvWishList_TV_USER_FOLDER", "NOT_FOUND");
                        setting.Value = TV_USER_FOLDER;
                        setting.Persist();
                    }
                }

                


                _RecordingFlagTime = DateTime.Now.AddHours(1.0); //add 1 hour to give time for setup
                _NextEpgTime = DateTime.Now.AddHours(1.0); //add 1 hour to give time for setup
                setting = layer.GetSetting("TvWishList_NextEpgDate", _NextEpgTime.ToString("yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture));
                try
                {
                    _NextEpgTime = DateTime.ParseExact(setting.Value, "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception exc)
                {
                    Log.Error("NextEpgDate reading failed with exception: " + exc.Message);
                }
                Log.Debug("Start(IController controller):  _NextEpgTime=" + _NextEpgTime.ToString());



                

                /*
                // lock next time for receive mail for upcoming startups if Tvserver is being restarted after each standby
                setting = layer.GetSetting("TvWishList_SetEpgTime", "false");
                setting.Value = "false";
                setting.Persist();*/


                epgwatchclass = new EpgParser();

                if (epgwatchclass == null)
                {
                    Log.Error("EpgParser could not be initiated - aborting operation");
                    return;
                }
                else
                {
                    epgwatchclass.newlabelmessage += new setuplabelmessage(SendServerPipeMessage);
                    Logdebug("EpgParser initiated");
                }
                
                //start pollingthread
                runpolling = true;
                System.Threading.Thread th = new System.Threading.Thread(startpolling);
                th.IsBackground = true;
                th.Start();
                Logdebug("Polling thread starting");
                

                /*
                // activate filewatcher for active command
                try
                {
                    filewatcherstartepg = TV_USER_FOLDER + @"\TvWishList\StartEPGsearch.txt";
                    if (File.Exists(filewatcherstartepg) == true)
                    {
                        File.Delete(filewatcherstartepg);
                    }

                    filewatcherfinishedepg = TV_USER_FOLDER + @"\TvWishList\FinishedEPGsearch.txt";
                    if (File.Exists(filewatcherfinishedepg) == true)
                    {
                        File.Delete(filewatcherfinishedepg);
                    }

                    FileInfo myfileinfo = new FileInfo(filewatcherstartepg);
                    StartEPGsearch.Path = myfileinfo.DirectoryName;
                    StartEPGsearch.Created += new FileSystemEventHandler(StartEPGsearchFilewatcher);
                    StartEPGsearch.Filter = myfileinfo.Name;
                    StartEPGsearch.EnableRaisingEvents = true;
                    Logdebug("file watcher StartEPGsearch enabled");
                }
                catch (Exception ex)
                {
                    Log.Error("Error in starting StartEPGsearch File watcher: Exception message was " + ex.Message);
                    return;

                }*/

                // activate filewatcher for setupTvStarted
                try
                {
                    filewatcherSetupTvStarted = TV_USER_FOLDER + @"\TvWishList\SetupTvStarted.txt";
                    if (File.Exists(filewatcherSetupTvStarted) == true)
                    {
                        File.Delete(filewatcherSetupTvStarted);
                    }

                    FileInfo myfileinfo = new FileInfo(filewatcherSetupTvStarted);
                    SetupTvStarted.Path = myfileinfo.DirectoryName;
                    SetupTvStarted.Created += new FileSystemEventHandler(SetupTvStartedFilewatcher);
                    SetupTvStarted.Filter = myfileinfo.Name;
                    SetupTvStarted.EnableRaisingEvents = true;
                    Logdebug("file watcher SetupTvStarted enabled");
                }
                catch (Exception ex)
                {
                    Log.Error("Error in starting SetupTvStarted File watcher: Exception message was " + ex.Message);
                    return;

                }


                // activate filewatcher for NextEpgCheck
                try
                {
                    filewatcherNextEpgCheck = TV_USER_FOLDER + @"\TvWishList\NextEpgCheck.txt";
                    if (File.Exists(filewatcherNextEpgCheck) == true)
                    {
                        File.Delete(filewatcherNextEpgCheck);
                    }

                    FileInfo myfileinfo = new FileInfo(filewatcherNextEpgCheck);
                    NextEpgCheck.Path = myfileinfo.DirectoryName;
                    NextEpgCheck.Created += new FileSystemEventHandler(NextEpgCheckFilewatcher);
                    NextEpgCheck.Filter = myfileinfo.Name;
                    NextEpgCheck.EnableRaisingEvents = true;
                    Logdebug("file watcher NextEpgCheck enabled");
                }
                catch (Exception ex)
                {
                    Log.Error("Error in starting NextEpgCheck File watcher: Exception message was " + ex.Message);
                    return;

                }
                
                //startpipeserver and listen for commands
                StartServer();

            }
            catch (Exception ex)
            {
                Log.Debug("Error in starting TvWishList: Exception message was " + ex.Message);
                return;

            }
            Log.Debug("TvWishList start completed");


            
        }



        /// <summary>
        /// Stops the plugin
        /// </summary>
        public void Stop()
        {
            try
            {
                runpolling = false;  //terminate polling loop
                SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
                SystemEvents.SessionEnded -= new SessionEndedEventHandler(SystemEvents_SessionEnded);
                //ITvServerEvent events = GlobalServiceProvider.Instance.Get<ITvServerEvent>();
                if (events != null)
                {
                    events.OnTvServerEvent -= new TvServerEventHandler(events_OnTvServerEvent);
                }

                //disable filewatchers
                if (SetupTvStarted != null)
                {
                    SetupTvStarted.EnableRaisingEvents = false;
                }

                if (NextEpgCheck != null)
                {
                    NextEpgCheck.EnableRaisingEvents = false;
                }
                //StartEPGsearch.EnableRaisingEvents = false;

                //stop pipeserver
                StopServer();

                Log.Info("TvWishList stopped");
            }
            catch (Exception exc)
            {
                Log.Debug("Stop failed with exception message:");
                Log.Error("Stop failed with exception message:");
                Log.Debug(exc.Message);
                Log.Error(exc.Message);
            }
        }

        /// <summary>
        /// returns the setup sections for display in SetupTv
        /// </summary>
        [CLSCompliant(false)]
        public SectionSettings Setup
        {
            get { return new TvWishListSetup(); }
        }
 
        #endregion


        #region Implementation

        public void NextEpgCheckFilewatcher(object sender, FileSystemEventArgs e)
        {
            Logdebug("NextEpgCheckFilewatcher Started");
            //disable filewatcher
            NextEpgCheck.EnableRaisingEvents = false;

            Thread.Sleep(1000); //wait for storing data

            _NextEpgTime = DateTime.Now;
            _NextEpgTime = NextEpgTime();

            Log.Debug(" next checking time after filewatcher=" + _NextEpgTime.ToString());

            if (File.Exists(filewatcherNextEpgCheck) == true)
            {
                try
                {
                    File.Delete(filewatcherNextEpgCheck);
                }
                catch (Exception ex)
                {
                    Log.Error("Error in deleting NextEpgCheck File " + filewatcherNextEpgCheck + " watcher: Exception message was " + ex.Message);
                }
            }

            //enable filewatcher
            NextEpgCheck.EnableRaisingEvents = true;
        }

        public void SetupTvStartedFilewatcher(object sender, FileSystemEventArgs e)
        {
            Logdebug("SetupTvStartedFilewatcher");
            //disable filewatcher
            SetupTvStarted.EnableRaisingEvents = false;


            /*
            //debug only
            foreach (Mediaportal.TV.Server.TVDatabase.Entities.Schedule allschedule in Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.ScheduleManagement.ListAllSchedules())
            {
                List<Mediaportal.TV.Server.TVDatabase.Entities.Schedule> notViewableSchedules = new List<Mediaportal.TV.Server.TVDatabase.Entities.Schedule>();
                IList<Mediaportal.TV.Server.TVDatabase.Entities.Schedule> mylist = Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.ScheduleManagementhuha.GetConflictingSchedules(allschedule, out notViewableSchedules);

                Log.Debug("*****************************************************************");
                Log.Debug("***********************allschedule=" + allschedule.ProgramName + "  s=" + allschedule.StartTime.ToString() + " ch=" + allschedule.IdChannel.ToString()); 
                Log.Debug("mylist.Count="+mylist.Count.ToString());
                foreach (Mediaportal.TV.Server.TVDatabase.Entities.Schedule test in mylist)
                {
                    Log.Debug("mylistSchedules = " + test.ProgramName + " id=" + test.IdSchedule.ToString() + " start=" + test.StartTime.ToString() + " end=" + test.EndTime.ToString() + " channel=" + test.IdChannel);
                }
                
                Log.Debug("notViewableSchedules.Count="+notViewableSchedules.Count.ToString());
                Log.Debug("*****************************************************************");
                foreach (Mediaportal.TV.Server.TVDatabase.Entities.Schedule test in notViewableSchedules)
                {
                    Log.Debug("notViewableSchedules = " + test.ProgramName + " id=" + test.IdSchedule.ToString() + " start=" + test.StartTime.ToString() + " end=" + test.EndTime.ToString() + " channel=" + test.IdChannel);
                }
                Log.Debug("");
            }

            //end debug
            */

            //Thread.Sleep(2000); //wait for storing data

            Process[] allnamedprocesses;
            do
            {
                System.Threading.Thread.Sleep(1000); //sleep 1s
                allnamedprocesses = Process.GetProcessesByName("SetupTv");                
                Log.Debug("allnamedprocesses.Length=" + allnamedprocesses.Length.ToString());
            }
            while ((allnamedprocesses.Length > 0) && (File.Exists(filewatcherSetupTvStarted)));

            //SetupTv completed or TvWishList deactivated
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting = null;

            //unlock data
            setting = layer.GetSetting("TvWishList_LockingPluginname", "NONE");
            setting.Value = "NONE";
            setting.Persist();

            if (File.Exists(filewatcherSetupTvStarted) == true)
            {
                File.Delete(filewatcherSetupTvStarted);
            }

            setting = layer.GetSetting("TvWishList_BUSY", "false");
            setting.Value = "false";
            setting.Persist();

            Logdebug("SetupTvStartedFilewatcher completed busy=false, unlocked");


            //enable filewatcher
            SetupTvStarted.EnableRaisingEvents = true;
        }





        public DateTime NextEpgTime()
        {
            try
            {
                TvBusinessLayer layer = new TvBusinessLayer();
                Setting setting = null;
                string hours = layer.GetSetting("TvWishList_CheckEpgHours", "06").Value;
                int hournumber = 0;
                try
                {
                    hournumber = Convert.ToInt32(hours);
                }
                catch (Exception exc)
                {
                    Log.Error("Error: Could not convert EPG hours " + hours + " into number - setting to 06");
                    Log.Error("Exception message was:\n" + exc.Message + "\n");
                    hournumber = 6;
                }

                string minutes = layer.GetSetting("TvWishList_CheckEpgMinutes", "00").Value;
                Logdebug("tvserver minutes =" + minutes);
                int minutenumber = 0;
                try
                {
                    minutenumber = Convert.ToInt32(minutes);
                }
                catch (Exception exc)
                {
                    Log.Error("Error: Could not convert EPG minutes " + minutenumber + " into number - setting to 00");
                    Log.Error("Exception message was:\n" + exc.Message + "\n");
                    minutenumber = 0;
                }


                if (layer.GetSetting("TvWishList_Every", "false").Value.ToLower() == "true")
                {  //process every xx days
                    string days = layer.GetSetting("TvWishList_CheckEpgDays", "07").Value;
                    int daynumber = 7;
                    try
                    {
                        daynumber = Convert.ToInt32(days);
                    }
                    catch (Exception exc)
                    {
                        Log.Error("Error: Could not convert EPG days " + days + " into number - setting to 07");
                        Log.Error("Exception message was:\n" + exc.Message + "\n");
                        daynumber = 7;
                    }

                    _NextEpgTime = _NextEpgTime.AddDays(daynumber);
                }
                else //process week days
                {
                    // extract weekday

                    bool Sunday = false;
                    if (layer.GetSetting("TvWishList_Sunday", "false").Value.ToLower() == "true")
                    {
                        Sunday = true;
                        Log.Debug("Sunday");
                    }
                    bool Monday = false;
                    if (layer.GetSetting("TvWishList_Monday", "false").Value.ToLower() == "true")
                    {
                        Monday = true;
                        Log.Debug("Monday");
                    }
                    bool Tuesday = false;
                    if (layer.GetSetting("TvWishList_Tuesday", "false").Value.ToLower() == "true")
                    {
                        Tuesday = true;
                        Log.Debug("Tuesday");
                    }
                    bool Wednesday = false;
                    if (layer.GetSetting("TvWishList_Wednesday", "false").Value.ToLower() == "true")
                    {
                        Wednesday = true;
                        Log.Debug("Wednesday");
                    }
                    bool Thursday = false;
                    if (layer.GetSetting("TvWishList_Thursday", "false").Value.ToLower() == "true")
                    {
                        Thursday = true;
                        Log.Debug("Thursday");
                    }
                    bool Friday = false;
                    if (layer.GetSetting("TvWishList_Friday", "false").Value.ToLower() == "true")
                    {
                        Friday = true;
                        Log.Debug("Friday");
                    }
                    bool Saturday = false;
                    if (layer.GetSetting("TvWishList_Saturday", "false").Value.ToLower() == "true")
                    {
                        Saturday = true;
                        Log.Debug("Saturday");
                    }

                    
                    string testdatestring = DateTime.Now.ToString("yyyy-MM-dd") + "_" + hournumber.ToString("D2") + ":" + minutenumber.ToString("D2");
                    DateTime testdate = DateTime.ParseExact(testdatestring, "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    Logdebug("testdate=" + testdate.ToString());
                    int daynumber = 7;
                    if (testdate > DateTime.Now)
                    {
                        daynumber = 0;
                    }


                    DayOfWeek weekday = _NextEpgTime.DayOfWeek;

                    if (weekday == DayOfWeek.Sunday)
                    {
                        if (Monday)
                            daynumber = 1;
                        else if (Tuesday)
                            daynumber = 2;
                        else if (Wednesday)
                            daynumber = 3;
                        else if (Thursday)
                            daynumber = 4;
                        else if (Friday)
                            daynumber = 5;
                        else if (Saturday)
                            daynumber = 6;
                    }
                    else if (weekday == DayOfWeek.Monday)
                    {
                        if (Tuesday)
                            daynumber = 1;
                        else if (Wednesday)
                            daynumber = 2;
                        else if (Thursday)
                            daynumber = 3;
                        else if (Friday)
                            daynumber = 4;
                        else if (Saturday)
                            daynumber = 5;
                        else if (Sunday)
                            daynumber = 6;
                    }
                    else if (weekday == DayOfWeek.Tuesday)
                    {
                        if (Wednesday)
                            daynumber = 1;
                        else if (Thursday)
                            daynumber = 2;
                        else if (Friday)
                            daynumber = 3;
                        else if (Saturday)
                            daynumber = 4;
                        else if (Sunday)
                            daynumber = 5;
                        else if (Monday)
                            daynumber = 6;
                    }
                    else if (weekday == DayOfWeek.Wednesday)
                    {
                        if (Thursday)
                            daynumber = 1;
                        else if (Friday)
                            daynumber = 2;
                        else if (Saturday)
                            daynumber = 3;
                        else if (Sunday)
                            daynumber = 4;
                        else if (Monday)
                            daynumber = 5;
                        else if (Tuesday)
                            daynumber = 6;
                    }
                    else if (weekday == DayOfWeek.Thursday)
                    {
                        if (Friday)
                            daynumber = 1;
                        else if (Saturday)
                            daynumber = 2;
                        else if (Sunday)
                            daynumber = 3;
                        else if (Monday)
                            daynumber = 4;
                        else if (Tuesday)
                            daynumber = 5;
                        else if (Wednesday)
                            daynumber = 6;
                    }
                    else if (weekday == DayOfWeek.Friday)
                    {
                        if (Saturday)
                            daynumber = 1;
                        else if (Sunday)
                            daynumber = 2;
                        else if (Monday)
                            daynumber = 3;
                        else if (Tuesday)
                            daynumber = 4;
                        else if (Wednesday)
                            daynumber = 5;
                        else if (Thursday)
                            daynumber = 6;
                    }
                    else if (weekday == DayOfWeek.Saturday)
                    {
                        if (Sunday)
                            daynumber = 1;
                        else if (Monday)
                            daynumber = 2;
                        else if (Tuesday)
                            daynumber = 3;
                        else if (Wednesday)
                            daynumber = 4;
                        else if (Thursday)
                            daynumber = 5;
                        else if (Friday)
                            daynumber = 6;
                    }

                    _PreviousEpgTime = _NextEpgTime;
                    _NextEpgTime = _NextEpgTime.AddDays(daynumber);

                    Log.Debug("_PreviousEpgTime=" + _PreviousEpgTime.ToString());
                    Log.Debug("_NextEpgTime=" + _NextEpgTime.ToString());
                    Log.Debug("daynumber=" + daynumber.ToString());

                }

                string datestring = _NextEpgTime.ToString("yyyy-MM-dd");
                datestring = datestring + "_" + hours + ":" + minutes;
                Logdebug("Next epg checking date set to datestring=" + datestring);

                _NextEpgTime = DateTime.ParseExact(datestring, "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                _NextEpgTime = EPGUpdateBeforeSchedule(_NextEpgTime);

                setting = layer.GetSetting("TvWishList_NextEpgDate","2999-01-01");
                setting.Value = _NextEpgTime.ToString("yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                Logdebug("Next epg checking date set to " + setting.Value);                
                setting.Persist();
            }
            catch (Exception ex)
            {
                Log.Error("Error in starting setting next Epg date time: Exception message was " + ex.Message);

            }
            return _NextEpgTime;
        }

        public DateTime EPGUpdateBeforeSchedule(DateTime _nextEpgTime)
        {
            Logdebug("EPGUpdateBeforeSchedule() _nextEpgTime="+_nextEpgTime.ToString());
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting = null;

            setting = layer.GetSetting("TvWishList_CheckEPGScheduleMinutes","00");
            bool EPGBeforeSchedule = false;
            Boolean.TryParse(setting.Value, out EPGBeforeSchedule);
            
            setting = layer.GetSetting("TvWishList_SlowCPU","true");
            _useRecordingFlag = false;
            Boolean.TryParse(setting.Value, out _useRecordingFlag);

            if (EPGBeforeSchedule)
            {
                string minutes = layer.GetSetting("TvWishList_BeforeEPGMinutes", "00").Value;
                Logdebug("tvserver minutes =" + minutes);
                double minutenumber = 0;
                try
                {
                    minutenumber = Convert.ToDouble(minutes);
                }
                catch (Exception exc)
                {
                    Log.Error("Error: Could not convert EPG minutes " + minutenumber + " into number - setting to 00");
                    Log.Error("Exception message was:\n" + exc.Message + "\n");
                    minutenumber = 0;
                }

                //get next schedule start

                DateTime firstStartTime = DateTime.Now.AddYears(100); //always in the future
                Log.Debug("firstStartTime ="+firstStartTime.ToString());
                Boolean found = false;
                foreach (Schedule myschedule in Schedule.ListAll())
                {
                    DateTime newStartTime = myschedule.StartTime.AddMinutes(minutenumber * (-1.0));
                    newStartTime = newStartTime.AddMinutes(myschedule.PreRecordInterval * (-1.0));
                    Log.Debug("Schedule ProgramName=" + myschedule.ProgramName);
                    Log.Debug("newStartTime="+newStartTime.ToString());
                    if ((newStartTime > DateTime.Now) && (newStartTime < firstStartTime) )
                    {
                        firstStartTime = newStartTime;
                        found = true;
                        Logdebug("New minimum schedule time found = "+newStartTime.ToString());
                    }
                }

                //update next epg time
                if ((found) && (firstStartTime < _nextEpgTime))
                {
                    _runEPGScript = true; //run script before next EPG check
                    _nextEpgTime = firstStartTime;
                    Logdebug("updating with new schedule start time nextEPGtime=" + _nextEpgTime.ToString());

                    //store data, because check needs to be done after each recording
                    setting = layer.GetSetting("TvWishList_NextEpgDate", "2999-01-01");
                    setting.Value = _NextEpgTime.ToString("yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    Logdebug("Next epg checking date set to " + setting.Value);
                    setting.Persist();
                }
               
            }
            Logdebug("EPGUpdateBeforeSchedule() completed _nextEpgTime=" + _nextEpgTime.ToString());
            return _nextEpgTime;
        }



        void startpolling()
        {
            try
            {
                Logdebug("Polling thread started");
                while (runpolling == true)
                {
                    
                    System.Threading.Thread.Sleep(60000); //sleep 60s
                    Logdebug("Polling Thread Sleeping");
                    //Logdebug("Polling Thread: _NextEpgTime=" + _NextEpgTime.ToString());
                    if (DateTime.Now > _NextEpgTime) 
                    {
                        if (((_recording == 0) && (_timeshifting == false) && (_useRecordingFlag)) || ((DateTime.Now > _RecordingFlagTime && (_useRecordingFlag)) || (_useRecordingFlag == false)))
                        {
                            SetStandbyAllowed(false);

                            if (_runEPGScript)
                            {
                                string scriptname = "RunBeforeEachSchedule.bat";
                                string scriptfolder = TV_USER_FOLDER + @"\TvWishList";
                                string scriptfile = scriptfolder + @"\"+scriptname;

                                if (File.Exists(scriptfile) == true)
                                {
                                    Log.Debug("User batch process " + scriptname + " started");
                                    ProcessStartInfo TvWishListBatchStart = new ProcessStartInfo(scriptfile);
                                    TvWishListBatchStart.WorkingDirectory = scriptfolder;
                                    TvWishListBatchStart.UseShellExecute = true;

                                    Process TvWishListBatchExecute = new Process();
                                    TvWishListBatchExecute.StartInfo = TvWishListBatchStart;
                                    try
                                    {
                                        TvWishListBatchExecute.Start();
                                    }
                                    catch (Exception exc)
                                    {
                                        Log.Error("<RED>Could not start " + scriptfile + "  " + TvWishListBatchStart.Arguments);
                                        if (DEBUG == true)
                                            Log.Debug("<RED>Exception message is " + exc.Message);

                                        _runEPGScript = false;
                                        continue;
                                    }
                                    TvWishListBatchExecute.WaitForExit(1000 * 60 * 3); //wait 3 minutes maximum
                                    if (TvWishListBatchExecute.HasExited == true)
                                    {
                                        if (TvWishListBatchExecute.ExitCode != 0)
                                        {
                                            Log.Error("<RED>Batch process completed with Errorcode " + TvWishListBatchExecute.ExitCode.ToString());
                                            _runEPGScript = false;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Log.Error("<RED>Batch process did not complete within 3 minutes");
                                        _runEPGScript = false;
                                        continue;
                                    }
                                    Log.Debug("User batch process " + scriptname + " completed ");
                                }//end scriptfile exists

                                _runEPGScript = false;
                            }// end: need to run scriptfile

                            //_NextEpgTime is updated in the epgwatch thread
                            receivethread = new System.Threading.Thread(epgwatch);
                            receivethread.IsBackground = true;
                            receivethread.Priority = ThreadPriority.BelowNormal;
                            receivethread.Start();

                            Logdebug("receive thread started");
                            while (receivethread.IsAlive == true)
                            {
                                System.Threading.Thread.Sleep(1000); //sleep 1s
                                //Logdebug("receive thread running");
                            }
                            Logdebug("receive thread ended");
                            receivethread = null;
                            SetStandbyAllowed(true);
                        }

                    }
                }
                Logdebug("Polling thread stopped");
            }
            catch (Exception exc)
            {
                Log.Debug("Polling thread failed with exception message:");
                Log.Error("Polling thread failed with exception message:");
                Log.Debug(exc.Message);
                Log.Error(exc.Message);
                
            }


        }

        void events_OnTvServerEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                TvServerEventArgs tvEvent = (TvServerEventArgs)eventArgs;
                switch (tvEvent.EventType)
                {
                    ///StartZapChannel is called just before the server is going to change channels

                    ///StartRecording is called just before the server is going to start recording
                    case TvServerEventType.StartRecording:
                        _recording++;
                        _RecordingFlagTime = DateTime.Now.AddHours(4.0);
                        Logdebug("Recording flag increased to " + _recording .ToString()+ " - start recording");
                        Logdebug("RecordingFlagTime increased to " + _RecordingFlagTime.ToString() + " - start recording");
                        break;
                    ///RecordingStarted is called when the recording is started
                    case TvServerEventType.RecordingStarted:                        
                        Logdebug("recording started");
                        break;
                    ///RecordingEnded is called when the recording has been stopped
                    case TvServerEventType.RecordingEnded:
                        if (_recording > 0)
                            _recording--;

                        Logdebug("Recording flag decreased to " + _recording.ToString() + " recording ended");

                        //check for filerenaming
                        System.Threading.Thread pfr = new System.Threading.Thread(ProcessFileRenaming);  //ensure only one thread is running!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        pfr.IsBackground = true;
                        pfr.Start();
                        Logdebug("ProcessFileRenaming thread starting");
                        

                        //update nextepg for new schedule  
                        _NextEpgTime = EPGUpdateBeforeSchedule(_NextEpgTime);

                        //_NextEpgTime = NextEpgTime();!!!!bug


                        

                

                        break;
                    ///Timeshifting started
                    case TvServerEventType.StartTimeShifting:
                        _timeshifting = true;
                        Logdebug("Recording flag set to true -start timeshifting");
                        break;
                    ///Timeshifting ended
                    case TvServerEventType.EndTimeShifting:
                        _timeshifting = false;
                        Logdebug("Recording flag set to false -stop timeshifting");
                        break;
                }
            }
            catch (Exception exc)
            {
                Log.Debug("events_OnTvServerEvent failed with exception message:");
                Log.Error("events_OnTvServerEvent failed with exception message:");
                Log.Debug(exc.Message);
                Log.Error(exc.Message);
            }
        }


        private void ProcessFileRenaming()
        {
            if (_ProcessFileRenamingRunning == true)
            {
                Log.Debug("Old ProcessFileRenaming is running - returning thread");
                return;
            }
            _ProcessFileRenamingRunning = true;

            string filename = TV_USER_FOLDER + @"\TvWishList\" + FileRenameXML;
            Logdebug("Starting ProcessFileRenaming - file=" + filename);
            //process file renaming from xml file
            if (File.Exists(filename) == false)
            {
                Logdebug("Could not find jobfile " + filename);
                _ProcessFileRenamingRunning = false;
                return;
            }

            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting = null;
            //wait for comskip to finish  
            setting = layer.GetSetting("TvWishList_WaitComSkipMinutes", "60");
            try
            {
                int i = Convert.ToInt32(setting.Value);
                ComSkipMaxWait = i;
            }
            catch
            {
                ComSkipMaxWait = 60;
            }


            Log.Debug("ComSkipMaxWait=" + ComSkipMaxWait.ToString());

            System.Threading.Thread.Sleep(10000);//sleep 10s
            Process[] sprocs1 = Process.GetProcessesByName(ProcessName1);
            if (sprocs1.Length > 0)
            {
                sprocs1[0].WaitForExit(60000 * ComSkipMaxWait); //wait 60s * ComSkipMaxWait
                if (sprocs1[0].HasExited)
                {
                    Log.Debug("Process " + ProcessName1 + " has been finished");
                }
                else
                {
                    Log.Error("Process " + ProcessName1 + " did not finish");
                }
            }
            else
            {
                Log.Debug("Process " + ProcessName1 + " does not exist");
            }

            System.Threading.Thread.Sleep(10000);//sleep 10s
            Process[] sprocs2 = Process.GetProcessesByName(ProcessName2);
            if (sprocs2.Length > 0)
            {
                sprocs2[0].WaitForExit(60000 * ComSkipMaxWait); //wait 60s * MaxWait
                if (sprocs2[0].HasExited)
                {
                    Log.Debug("Process " + ProcessName2 + " has been finished");
                }
                else
                {
                    Log.Error("Process " + ProcessName2 + " did not finish");
                }
            }
            else
            {
                Log.Debug("Process " + ProcessName2 + " does not exist");
            }



            System.Threading.Thread.Sleep(10000);//sleep 10s
            Log.Debug("read xml file");
            //read xml file
            XmlDocument renamexmldoc = new XmlDocument();
            try
            {
                renamexmldoc.Load(filename);
                XmlNode root = renamexmldoc.DocumentElement;
                XmlNodeList nodealljobs = renamexmldoc.SelectNodes("/AllJobs/Job");
                Log.Debug("Reading Nodes");


#if(TV100)
            IList allrecordings = Recording.ListAll();
#elif(TV101)
            IList<Recording> allrecordings = Recording.ListAll();
#elif((TV11)||(TV12))
                IList<Recording> allrecordings = Recording.ListAll();
#endif
                foreach (XmlNode nodejob in nodealljobs)
                {
                    //get xml node
                    string programname = nodejob.Attributes["ScheduleName"].Value;
                    Log.Debug("programname=" + programname);
                    string folder = nodejob.Attributes["Folder"].Value;
                    Log.Debug("folder=" + folder);
                    DateTime startTime = DateTime.ParseExact(nodejob.Attributes["Start"].Value, "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                    Logdebug("startTime =" + startTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture));
                    DateTime endTime = DateTime.ParseExact(nodejob.Attributes["End"].Value, "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                    Logdebug("endTime =" + endTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture));
                    

                    int channelid = -1;
                    int.TryParse(nodejob.Attributes["idChannel"].Value, out channelid);
                    Logdebug("channelid =" + channelid.ToString());
                    bool nodeExists = true;
                    

                    //search all my recordings for match
                    foreach (Recording myrecording in allrecordings)
                    {
                        if (myrecording.StartTime == myrecording.EndTime)
                        {//during recording the start time will be equal to the end time and text all in capitol letters - in that case ignore and wait till recording is finished - schedule does exist in parallel during recording
                            continue;
                        }
                        string myrecording_startstring = myrecording.StartTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                        DateTime myrecording_start = DateTime.ParseExact(myrecording_startstring, "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                        //Logdebug("myrecording_startTime =" + myrecording_start.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture));

                        string myrecording_endstring = myrecording.EndTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                        DateTime myrecording_end = DateTime.ParseExact(myrecording_endstring, "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                        //Logdebug("myrecording_endTime =" + myrecording_end.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture));

                        if ((startTime == myrecording_start) && (endTime == myrecording_end) && (channelid == myrecording.IdChannel) && (File.Exists(myrecording.FileName) == true))
                        { //xml list entry has been recorded and matches start, end time and channel id
                            Logdebug("Recording matched for " + programname);

                            //rename filenames and update recording with new location
                             
                            string directory = System.IO.Path.GetDirectoryName(myrecording.FileName);  //valid directory
                            string filenameNoExtension = System.IO.Path.GetFileNameWithoutExtension(myrecording.FileName); //valid filename without extension
                            

                            char[] invalidPathChars = Path.GetInvalidPathChars();                           
                            for (int i = 0; i < invalidPathChars.Length; i++)
                            {
                                folder = folder.Replace(invalidPathChars[i], '_');  //valid  subdirectory name after replacement
                            }



                            string newname = directory + @"\" + folder + @"\" + filenameNoExtension + ".ts";
                            

                            if (Directory.Exists(directory + @"\" + folder) == false)
                            {
                                Log.Debug("Creating directory " + directory + @"\" + folder);
                                Directory.CreateDirectory(directory + @"\" + folder);
                            }

                            if (File.Exists(newname) == false)
                            {

                                Logdebug("Moving " + myrecording.FileName + " to " + newname);
                                File.Move(myrecording.FileName, newname);

                                String xmlfilename = myrecording.FileName.Substring(0, myrecording.FileName.Length - 2) + "xml";  //substitute .ts by .xml

                                if (File.Exists(xmlfilename) == true)
                                {
                                    String newxmlfilename = directory + @"\" + folder + @"\" + filenameNoExtension + ".xml";   //bug in older version
                                    File.Move(xmlfilename, newxmlfilename);
                                    Logdebug("Moving " + xmlfilename + " to " + newxmlfilename);                                   
                                }


                                //update recording file name
                                myrecording.FileName = newname;
                                myrecording.Persist();


                                //move other comskip files as well
                                string[] morefiles = Directory.GetFiles(directory, filenameNoExtension + "*");
                                foreach (string singlefile in morefiles)
                                {
                                    string shortname=System.IO.Path.GetFileName(singlefile);
                                    string newfilename = directory + @"\" + folder + @"\" + shortname;
                                    if (File.Exists(newfilename) == false)
                                    {
                                        Logdebug("Moving " + singlefile + " to " + newfilename);
                                        File.Move(singlefile, newfilename);                                        
                                    }
                                    else
                                    {
                                        Log.Error("file " + singlefile + " does exist already - will not move file");
                                    }
                                }

                                //removing node after successful completion
                                root.RemoveChild(nodejob);
                                Logdebug("Removing childnode after successful renaming for " + programname);
                                nodeExists = false;
                            }
                            else
                            {
                                Log.Error("file " + newname + " does exist already - will not move file");
                            }

                            break;
                        }

                    }//end all recordings
                    

                    endTime = endTime.AddDays(1.0); //add one day to make sure recordings are completed including postprocessing
                    if ((DateTime.Now > endTime)&&(nodeExists))
                    {
                        root.RemoveChild(nodejob);
                        Logdebug("Removing childnode due to timeout for " + programname);
                    }


                }

                renamexmldoc.Save(filename);

                if (root.HasChildNodes == false)
                {
                    File.Delete(filename);
                    Log.Debug("Deleting xml file as there are no entries");
                }


            }
            catch (Exception ee)
            {
                Log.Error("ProcessFileRenaming failed with exception message:");
                Log.Error(ee.Message);                
            }
            Log.Debug("ProcessFileRenaming completed");
            _ProcessFileRenamingRunning = false;

        }



        public void epgwatch()
        {
            try
            {
                Logdebug("Epg watching started");
                //get new updated value for next epg check
                TvBusinessLayer layer = new TvBusinessLayer();
                Setting setting = null;
            

                // get DEBUG
                setting = layer.GetSetting("TvWishList_Debug", "false");
                DEBUG = false;
                Boolean.TryParse(setting.Value, out DEBUG);
                Log.DebugValue = DEBUG;
                _NextEpgTime = NextEpgTime();
                Log.Debug("epgwatch(): _NextEpgTime=" + _NextEpgTime.ToString());
            

                ////////////////////////////////////////////////
                //start epgprocessing here
                

                if (epgwatchclass == null)
                {
                    Log.Error("EpgParser could not be initiated - aborting operation");
                    return;
                }
                else
                {
                    try
                    {
                        epgwatchclass.SearchEPG(false); //Email & Record
                    
                    }
                    catch (Exception exc)
                    {
                        Log.Error("Parsing EPG data failed with exception message:");
                        Log.Error(exc.Message);
                        // Reset BUSY Flag after exception
                    
                        //set BUSY = false
                        setting = layer.GetSetting("TvWishList_BUSY", "false");
                        setting.Value = "false";
                        setting.Persist();
                    }               
                }
            }
            catch (Exception exc)
            {
                Log.Error("epgwatch failed with exception message:");
                Log.Error(exc.Message);
            }
        }  

        private void SetStandbyAllowed(bool allowed)
        {
            try
            {
                //use IEPG handler to prevent shutdown
                if (GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
                {
                    GlobalServiceProvider.Instance.Get<IEpgHandler>().SetStandbyAllowed(this, allowed, 1800);//30 minutes timeout           
                    Logdebug("Telling PowerScheduler standby is: "+allowed.ToString()+", timeout is 30 minutes");

                    
                }
            }
            catch (Exception exc)
            {
                Log.Error("SetStandbyAllowed failed with exception message:");
                Log.Error(exc.Message);
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            try
            {
                Logdebug("SystemEvents_PowerModeChanged =" + e.Mode.ToString());
                if (e.Mode == PowerModes.Suspend)
                {
                    Logdebug("SystemEvents Suspend");
                    if (receivethread != null)
                    {
                        try
                        {
                            TvBusinessLayer layer = new TvBusinessLayer();
                            Setting setting = null;
                            setting = layer.GetSetting("TvWishList_NextEpgDate", DateTime.Now.ToString("yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture));
                            setting.Value = _PreviousEpgTime.ToString("yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                            Logdebug("TvWishList_NextEpgDate=" + setting.Value);
                            setting.Persist();
                        }
                        catch (Exception exc)
                        {
                            Log.Error("Error in writing old epg time");
                            Log.Error(exc.Message);
                        }

                        Log.Error("Aborting receive thread because of system standby event");
                        receivethread.Abort();

                        //resetting next epg date
                        _NextEpgTime = _PreviousEpgTime;
                        Logdebug("TVSERVER TvWishList: Setting NextEpgTime =" + _NextEpgTime.ToString());


                        /*
                        //writing file
                        try
                        {
                            string[] all_lines = new string[] { _PreviousEpgTime.ToString("yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture) };
                            File.WriteAllLines(TV_USER_FOLDER + @"\TvWishList\NextEPG.txt", all_lines);
                        }
                        catch (Exception exc)
                        {
                            Logdebug("Error in writing old epg time file");
                            Log.Error("Error in writing old epg time file");
                            Log.Debug(exc.Message);
                            Log.Error(exc.Message);
                        }*/

                    }
                }
                else if (e.Mode == PowerModes.Resume)
                {
                    Logdebug("SystemEvents Resume");
                }
                else if (e.Mode == PowerModes.StatusChange)
                {
                    Logdebug("SystemEvents Status Change");
                }
            }
            catch (Exception exc)
            {
                Log.Error("SystemEvents_PowerModeChanged failed with exception message:");
                Log.Error(exc.Message);
            }
        }
        

        private void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
        {
            try
            {
                Logdebug("SystemEvents_SessionEnded =" + e.Reason.ToString());


                if (receivethread != null)
                {
                    try
                    {
                        TvBusinessLayer layer = new TvBusinessLayer();
                        Setting setting = null;
                        setting = layer.GetSetting("TvWishList_NextEpgDate", DateTime.Now.ToString("yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture));
                        setting.Value = _PreviousEpgTime.ToString("yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                        Logdebug("TvWishList_NextEpgDate=" + setting.Value);
                        setting.Persist();

                    }
                    catch (Exception exc)
                    {
                        Logdebug("Error in writing old epg time");
                        Log.Error("Error in writing old epg time");
                        Log.Debug(exc.Message);
                        Log.Error(exc.Message);
                    }


                    Logdebug("Aborting receive thread because of shutdown or logoff event");
                    Log.Error("Aborting receive thread because of shutdown or logoff event");
                    receivethread.Abort();

                    //resetting next epg date
                    _NextEpgTime = _PreviousEpgTime;
                    Logdebug("TVSERVER TvWishList: Setting NextEpgTime =" + _NextEpgTime.ToString());

                }
            }
            catch (Exception exc)
            {
                Log.Debug("SystemEvents_SessionEnded failed with exception message:");
                Log.Error("SystemEvents_SessionEnded failed with exception message:");
                Log.Debug(exc.Message);
                Log.Error(exc.Message);
            }
            
            
        }


        #region server pipe
        public void StartServer()
        {

            try
            {
                PipeServerActive = true;
                ServerThread = new Thread(ServerThreadRoutine);
                ServerThread.Start();

            }
            catch (Exception exc)
            {
                Log.Error("StartServer() exception=" + exc.Message);
            }
            Log.Debug("Pipeserver thread started on tvserver");
        }

        public void StopServer()
        {
            try
            {
                PipeServerActive = false;

                if (pipeServer != null)
                {
                    if (pipeServer.IsConnected)
                        pipeServer.Disconnect();

                    pipeServer.Close();
                    pipeServer.Dispose();
                    pipeServer = null;
                    Logdebug("Pipeserver closed");
                    Thread.Sleep(1000);
                }


                if (ServerThread != null)
                {
                    ServerThread.Abort();
                    ServerThread = null;
                }


                try
                {  //create a dummy pipe to fix error condition when stopping server
                    pipeClient = new NamedPipeClientStream(".", "TvWishListPipe",
                        PipeDirection.InOut, PipeOptions.None,
                        TokenImpersonationLevel.Impersonation);

                    pipeClient.Connect();
                }
                catch (Exception exc)
                { 
                    Logdebug("StopServer() dummy pipe exception=" + exc.Message); 
                }

                PipeServerBusy = false;
            }
            catch (Exception exc)
            {
                Log.Error("StopServer() exception=" + exc.Message);
            }
        }

        private void ServerThreadRoutine(object data)
        {
            string defaultTimeOutValuestring = TimeOutValuestring;
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting = null;

            while (PipeServerActive)
            {
                try
                {
                    var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                    var rule = new PipeAccessRule(sid, PipeAccessRights.ReadWrite,System.Security.AccessControl.AccessControlType.Allow);
                    var sec = new PipeSecurity();
                    sec.AddAccessRule(rule);

#if (MPTV2)
                    string pipeName = "MP2TvWishListPipe";
#else
                    string pipeName = "TvWishListPipe";
#endif

                    pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, sec);  //only 1 thread for pipe

                    int threadId = Thread.CurrentThread.ManagedThreadId;

                    // Wait for a client to connect
                    Logdebug("TvServer: Waiting for client to connect");
                    PipeServerBusy = false;

                    pipeServer.WaitForConnection();
                    ServerMessage = string.Empty;
                    Logdebug("Client connected on pipe server thread.");
                    PipeServerBusy = true; ;
                    // Read the request from the client. Once the client has
                    // written to the pipe its security token will be available.

                    //pipeServer.ReadTimeout = 5000; //timeout not supported for async streams

                    StreamString ss = new StreamString(pipeServer);

                    // Verify our identity to the connected client using a
                    // string that the client anticipates.


                    MessageFromClient = ss.ReadString();            //receive message from client first
                    //labelreceivedTextServer.Text = messagefromclient;
                    Logdebug("***** CLIENTMESSAGE=" + MessageFromClient);

                    //*******************************
                    //commandinterpretation 
                    //*******************************
                    if (MessageFromClient == PipeCommands.RequestTvVersion.ToString())
                    {
                        string response = PipeCommands.RequestTvVersion.ToString() + "=" + this.Version;
                        Logdebug("sending response " + response);
                        ss.WriteString(response);
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.StartEpg.ToString()) == true)
                    {
                        defaultTimeOutValuestring = TimeOutValuestring;
                        string[] tokens = MessageFromClient.Split(':');
                        if (tokens.Length > 1)
                        {
                            TimeOutValuestring = tokens[1];
                            Logdebug("Changed TimeOutValuestring=" + TimeOutValuestring);
                        }
                        Logdebug("Starting EPG Search from pipe command");
                        Thread StartEPGsearchThread = new Thread(StartEPGsearchCommand);
                        StartEPGsearchThread.Start();
                        

                        while ((ServerMessage.StartsWith(PipeCommands.Ready.ToString())==false) && (ServerMessage.StartsWith(PipeCommands.Error.ToString()) == false))
                        {
                            ServerTimeOutCounter = new Thread(ServerTimeOutError);
                            ServerTimeOutCounter.Start();
                            while ((NewServerMessage == false) || (ServerMessage==string.Empty))
                            {
                                Thread.Sleep(500);
                                Logdebug("waiting for new servermessage (ServerMessage="+ServerMessage);
                            }
                            Logdebug("Sending Servermessage=" + ServerMessage);
                            ss.WriteString(ServerMessage);                   //send response messages until done
                            ServerTimeOutCounter.Abort();
                            NewServerMessage = false;
                        }
                        TimeOutValuestring = defaultTimeOutValuestring;
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.Error_TimeOut.ToString()) == true)
                    {
                        TimeOutValuestring = MessageFromClient.Replace(PipeCommands.Error_TimeOut.ToString(), string.Empty);
                        Logdebug("new TimeOutValuestring="+TimeOutValuestring);
                        ss.WriteString(TimeOutValuestring);
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.ExportTvWishes.ToString()) == true)
                    {
                        defaultTimeOutValuestring = TimeOutValuestring;

                        string[] tokens = MessageFromClient.Split(':');
                        if (tokens.Length > 1)
                        {
                            TimeOutValuestring = tokens[1];
                            Log.Debug("Changed TimeOutValuestring=" + TimeOutValuestring);
                        }

                        if (MessageFromClient.Contains("VIEWONLY=TRUE") == true)
                        {
                            ServerMessage = ExportTvWishes(true);
                        }
                        else //Email & Record Mode
                        {
                            ServerMessage = ExportTvWishes(false);
                        }

                        ss.WriteString(ServerMessage);

                        TimeOutValuestring = defaultTimeOutValuestring;
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.ImportTvWishes.ToString()) == true)
                    {
                        defaultTimeOutValuestring = TimeOutValuestring;

                        string[] tokens = MessageFromClient.Split(':');
                        if (tokens.Length > 1)
                        {
                            TimeOutValuestring = tokens[1];
                            Logdebug("Changed TimeOutValuestring=" + TimeOutValuestring);
                        }

                        if (MessageFromClient.Contains("VIEWONLY=TRUE") == true)
                        {
                            ServerMessage = ImportTvWishes(true);
                        }
                        else //Email & Record Mode
                        {
                            ServerMessage = ImportTvWishes(false);
                        }

                        ss.WriteString(ServerMessage);

                        TimeOutValuestring = defaultTimeOutValuestring;
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.RemoveSetting.ToString()) == true)
                    {
                        defaultTimeOutValuestring = TimeOutValuestring;

                        string[] tokens = MessageFromClient.Split(':');
                        if (tokens.Length > 1)
                        {
                            TimeOutValuestring = tokens[1];
                            Logdebug("Changed TimeOutValuestring=" + TimeOutValuestring);
                        }

                        string tag = tokens[0].Replace(PipeCommands.RemoveSetting.ToString(), string.Empty);

                        
                        setting = layer.GetSetting(tag, string.Empty);
                        if (setting != null)
                        {
                            setting.Remove();
                            ServerMessage = "Setting " + tag + " removed";
                            Logdebug("Setting " + tag + " removed");
                        }
                        else
                        {
                            ServerMessage = "Setting " + tag + " could not be removed";
                            Logdebug("Eror: Setting " + tag + " could not be removed");
                 
                        }

                        ss.WriteString(ServerMessage);
                        TimeOutValuestring = defaultTimeOutValuestring;
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.RemoveRecording.ToString()) == true)
                    {//string command = Main_GUI.PipeCommands.RemoveRecording.ToString() + this.IdRecording.ToString() + ":10";
                        defaultTimeOutValuestring = TimeOutValuestring;

                        string[] tokens = MessageFromClient.Split(':');
                        if (tokens.Length > 1)
                        {
                            TimeOutValuestring = tokens[1];
                            Logdebug("Changed TimeOutValuestring=" + TimeOutValuestring);
                        }

                        string idRecordingString = tokens[0].Replace(PipeCommands.RemoveRecording.ToString(), string.Empty);
                        try
                        {
                            int idRecording = -1;
                            int.TryParse(idRecordingString, out idRecording);
                            Logdebug("idRecording=" + idRecording.ToString());
                            Recording myrecording = Recording.Retrieve(idRecording);
                            Logdebug("idRecording=" + myrecording.Title.ToString());
                            myrecording.Delete();
                            Logdebug("Recording deleted");
                            ServerMessage = "Recording deleted"; 
                        }
                        catch (Exception exc)
                        {
                            Logdebug("no recording found for idRecordingString = " + idRecordingString);
                            Logdebug("exception is " + exc.Message);
                            ServerMessage = "Error: Recording could not be deleted check the tvserver log file"; 
                        }

                        ss.WriteString(ServerMessage);
                        TimeOutValuestring = defaultTimeOutValuestring;
                    }
                    
                    else if (MessageFromClient.StartsWith(PipeCommands.RemoveLongSetting.ToString()) == true)
                    {
                        defaultTimeOutValuestring = TimeOutValuestring;

                        string[] tokens = MessageFromClient.Split(':');
                        if (tokens.Length > 1)
                        {
                            TimeOutValuestring = tokens[1];
                            Logdebug("Changed TimeOutValuestring=" + TimeOutValuestring);
                        }

                        //processing
                        string mysetting = string.Empty;
                        try
                        {
                            //cleanup work
                            mysetting = tokens[0].Replace(PipeCommands.RemoveLongSetting.ToString(), string.Empty);
                            Log.Debug("mysetting=" + mysetting);
                            for (int i = 1; i < 1000; i++)
                            {
                                setting = layer.GetSetting(mysetting + "_" + i.ToString("D3"), "_DOES_NOT_EXIST_");
                                Log.Debug("save_longsetting setting=" + setting.Value);
                                if (setting.Value == "_DOES_NOT_EXIST_")
                                {
                                    setting.Remove();
                                    break;
                                }
                                else
                                {
                                    string value = setting.Value;
                                    setting.Remove();

                                }
                            }
                            ServerMessage = "Long setting could be removed";
                        }
                        catch (Exception exc)
                        {
                            Logdebug("Longsetting could not be removed for mysetting= " + mysetting);
                            Logdebug("exception is " + exc.Message);
                            ServerMessage = "Error: Long setting could not be removed - check the tvserver log file";
                        }

                        ss.WriteString(ServerMessage);
                        TimeOutValuestring = defaultTimeOutValuestring;
                    }
#if (MPTV2)
                    else if (MessageFromClient.StartsWith(PipeCommands.WriteSetting.ToString()) == true)
                    {
                        string tag = MessageFromClient.Replace(PipeCommands.WriteSetting.ToString(), string.Empty);
                        string[] tags = tag.Split('\n');
                        ServiceAgents.Instance.SettingServiceAgent.SaveValue(tags[0], tags[1]);

                        ServerMessage = "SUCCESS";
                        ss.WriteString(ServerMessage);
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.ReadSetting.ToString()) == true)
                    {
                        string tag = MessageFromClient.Replace(PipeCommands.ReadSetting.ToString(), string.Empty);
                        Log.Debug("tag="+tag);
                        string value = ServiceAgents.Instance.SettingServiceAgent.GetValue(tag, string.Empty);
                        Log.Debug("value=" + value);
                        ServerMessage = value;
                        ss.WriteString(ServerMessage);
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.ReadAllCards.ToString()) == true)
                    {
                        defaultTimeOutValuestring = TimeOutValuestring;

                        string[] tokens = MessageFromClient.Split(':');
                        if (tokens.Length > 1)
                        {
                            TimeOutValuestring = tokens[1];
                            Logdebug("Changed TimeOutValuestring=" + TimeOutValuestring);
                        }

                        ServerMessage = string.Empty;
                        foreach (Card mycard in Card.ListAll())
                        {
                            ServerMessage += mycard.IdCard.ToString() + "\n" + mycard.Name + "\n";
                        }
                        //65000 max chars                        
                        ss.WriteString(ServerMessage);
                    }                  
                    else if (MessageFromClient.StartsWith(PipeCommands.ReadAllChannelsByGroup.ToString()) == true)
                    {
                        string groupIdString = MessageFromClient.Replace(PipeCommands.ReadAllChannelsByGroup.ToString(), string.Empty);
                        Log.Debug("groupIdString="+groupIdString);
                        int groupId = -1;
                        int.TryParse(groupIdString, out groupId);
                        Log.Debug("groupId=" + groupId.ToString());

                        ServerMessage = string.Empty;
                        foreach (Channel mychannel in Channel.ListAllByGroup(groupId))
                        {
                            ServerMessage += mychannel.IdChannel.ToString() + "\n" + mychannel.DisplayName + "\n";
                        }
                        Log.Debug("Groupchannels=" + ServerMessage);
                        //65000 max chars                        
                        ss.WriteString(ServerMessage);
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.ReadAllChannels.ToString()) == true)//must be after ReadAllChannelsByGroup
                    {
                        ServerMessage = string.Empty;
                        foreach (Channel mychannel in Channel.ListAll())
                        {
                            ServerMessage += mychannel.IdChannel.ToString() + "\n" + mychannel.DisplayName + "\n";
                        }
                        //65000 max chars                        
                        ss.WriteString(ServerMessage);
                    }
                    
                    else if (MessageFromClient.StartsWith(PipeCommands.ReadAllRadioChannelsByGroup.ToString()) == true)
                    {
                        string groupIdString = MessageFromClient.Replace(PipeCommands.ReadAllRadioChannelsByGroup.ToString(), string.Empty);
                        Log.Debug("radiogroupIdString=" + groupIdString);
                        int groupId = -1;
                        int.TryParse(groupIdString, out groupId);
                        Log.Debug("radiogroupId=" + groupId.ToString());

                        ServerMessage = string.Empty;
                        foreach (RadioChannel myradiochannel in RadioChannel.ListAllByGroup(groupId))
                        {
                            ServerMessage += myradiochannel.Id.ToString() + "\n" + myradiochannel.Name + "\n";
                        }
                        Log.Debug("radioGroupchannels=" + ServerMessage);
                        //65000 max chars                        
                        ss.WriteString(ServerMessage);
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.ReadAllRadioChannels.ToString()) == true)//must be after ReadAllRadioChannelsByGroup
                    {
                        ServerMessage = string.Empty;
                        foreach (RadioChannel myradiochannel in RadioChannel.ListAll())
                        {
                            ServerMessage += myradiochannel.Id.ToString() + "\n" + myradiochannel.Name + "\n";
                        }
                        //65000 max chars                        
                        ss.WriteString(ServerMessage);
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.ReadAllChannelGroups.ToString()) == true)
                    {
                        ServerMessage = string.Empty;
                        foreach (ChannelGroup mygroup in ChannelGroup.ListAll())
                        {
                            ServerMessage += mygroup.Id.ToString() + "\n" + mygroup.GroupName + "\n";
                        }
                        //65000 max chars                        
                        ss.WriteString(ServerMessage);
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.ReadAllRadioChannelGroups.ToString()) == true)
                    {
                        ServerMessage = string.Empty;
                        foreach (RadioChannelGroup myradiogroup in RadioChannelGroup.ListAll())
                        {
                            ServerMessage += myradiogroup.Id.ToString() + "\n" + myradiogroup.GroupName + "\n";
                        }
                        //65000 max chars                        
                        ss.WriteString(ServerMessage);
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.ReadAllRecordings.ToString()) == true)
                    {
                        ServerMessage = string.Empty;
                        foreach (Recording myrecording in Recording.ListAll())
                        {
                            ServerMessage += myrecording.IdRecording.ToString() + "\n" + myrecording.Title + "\n"+myrecording.FileName + "\n" +
                                             myrecording.IdChannel.ToString() + "\n" + myrecording.StartTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture) +
                                             "\n" + myrecording.EndTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture) + "\n";
                        }
                        //65000 max chars                        
                        ss.WriteString(ServerMessage);
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.ReadAllSchedules.ToString()) == true)
                    {
                        ServerMessage = string.Empty;
                        foreach (Schedule myschedule in Schedule.ListAll())
                        {
                            ServerMessage += myschedule.IdSchedule.ToString() + "\n" + myschedule.ProgramName + "\n" + 
                                             myschedule.IdChannel.ToString() + "\n" + myschedule.StartTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture) +
                                             "\n" + myschedule.EndTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture) + "\n" +
                                             myschedule.ScheduleType.ToString() + "\n" + myschedule.PreRecordInterval.ToString() + "\n" +
                                             myschedule.PostRecordInterval.ToString() + "\n" + myschedule.MaxAirings.ToString() + "\n" +
                                             myschedule.KeepDate.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture) + "\n" +
                                             myschedule.KeepMethod.ToString() + "\n" + myschedule.Priority.ToString() + "\n" +
                                             myschedule.PreRecordInterval.ToString() + "\n" + myschedule.Series.ToString() + "\n";
                        }
                        //65000 max chars                        
                        ss.WriteString(ServerMessage);
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.ScheduleDelete.ToString()) == true)
                    {
                        string scheduleIdString = MessageFromClient.Replace(PipeCommands.ScheduleDelete.ToString(), string.Empty);
                        Log.Debug("scheduleIdString=" + scheduleIdString);
                        int scheduleId = -1;
                        int.TryParse(scheduleIdString, out scheduleId);
                        Log.Debug("scheduleId=" + scheduleId.ToString());
                        Schedule.Delete(scheduleId);
                        
                        //65000 max chars                        
                        ss.WriteString("Deleted");
                    }
                    else if (MessageFromClient.StartsWith(PipeCommands.ScheduleNew.ToString()) == true)
                    {
                        string schedule = MessageFromClient.Replace(PipeCommands.ScheduleNew.ToString(), string.Empty);
                        string[] scheduletags = schedule.Split('\n');

                        int idChannel = -1;
                        int.TryParse(scheduletags[1], out idChannel);
                        DateTime start = DateTime.ParseExact(scheduletags[2], "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                        DateTime end = DateTime.ParseExact(scheduletags[3], "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                        Schedule myschedule = new Schedule(idChannel, scheduletags[0], start, end);
                        myschedule.Persist();
                        

                        ServerMessage = "SUCCESS";
                        ss.WriteString(ServerMessage);
                    }

#endif

                    else //Unknown command
                    {
                        Logdebug("sending response " + PipeCommands.UnknownCommand.ToString());
                        ss.WriteString(PipeCommands.UnknownCommand.ToString());
                    }
                    
                }
                // Catch the IOException that is raised if the pipe is broken
                // or disconnected.
                catch (IOException e)
                {
                    Log.Error("ServerThread ERROR: " + e.Message);
                }
                catch (Exception e)
                {
                    Log.Error("ServerThread ERROR: " + e.Message);
                }

                if (pipeServer != null)
                {
                    if (pipeServer.IsConnected)
                        pipeServer.Disconnect();

                    pipeServer.Close();
                    pipeServer.Dispose();
                    pipeServer = null;
                }
                Logdebug("Connection closed");

            }

            Logdebug("Pipe Server Thread Completed");
        }

        private void ServerTimeOutError()
        {
            int timeoutvalue = 10;
            try
            {
                timeoutvalue = Convert.ToInt32(TimeOutValuestring);
            }
            catch { }
            for (int i = 0; i < timeoutvalue; i++)
            {
                Thread.Sleep(1000);
            }
            Log.Error("Timeout Error Server - sending timeouterror" + timeoutvalue.ToString());
            ServerMessage = PipeCommands.Error_TimeOut.ToString() + timeoutvalue.ToString();
            NewServerMessage = true;

        }

        private void SendServerPipeMessage(string text, PipeCommands type)
        {
            Log.Debug("SendServerPipeMessage text="+text+" type="+type.ToString());
            if (PipeServerBusy)
            {
                

                ServerMessage = text;
                if ((type == PipeCommands.Ready) || (type == PipeCommands.Error))
                {
                    ServerMessage = type.ToString() + ServerMessage;
                    Log.Debug("ServerMessage=" + ServerMessage);
                }
                Logdebug("Preparing new epg message=" + text);
                Log.Debug("ServerMessage=" + ServerMessage);
              
                NewServerMessage = true;
            }
        }

        public void StartEPGsearchCommand()
        {
            try
            {
                Logdebug("Started  StartEPGsearchCommand");
                //disable filewatcher
                //StartEPGsearch.EnableRaisingEvents = false;

                //File.Delete(filewatcherstartepg);  use this file as a check for other processes

                //get new updated value for next epg check
                TvBusinessLayer layer = new TvBusinessLayer();
                Setting setting = null;

                // get DEBUG
                setting = layer.GetSetting("TvWishList_Debug", "false");
                DEBUG = false;
                Boolean.TryParse(setting.Value, out DEBUG);
                Log.DebugValue = DEBUG;


                ////////////////////////////////////////////////
                //start epgprocessing here


                if (epgwatchclass == null)
                {
                    Logdebug("EpgParser could not be initiated - aborting operation");
                    Log.Error("EpgParser could not be initiated - aborting operation");
                    return;
                }
                else
                {
                    try
                    {

                        if (MessageFromClient.Contains("VIEWONLY=TRUE") == true)
                        {
                            epgwatchclass.SearchEPG(true);
                        }
                        else //Email & Record Mode
                        {
                            epgwatchclass.SearchEPG(false);
                        }

                    }
                    catch (Exception exc)
                    {
                        Log.Error("Parsing EPG data failed with exception message:");
                        Log.Error(exc.Message);
                        // Reset BUSY Flag after exception

                        //set BUSY = false
                        setting = layer.GetSetting("TvWishList_BUSY", "false");
                        setting.Value = "false";
                        setting.Persist();
                    }

                }
                Log.Debug("epgwatchcommand completed ");
               
                //must end with "ready or "error"
                if ((ServerMessage.StartsWith(PipeCommands.Ready.ToString()) == false) && (ServerMessage.StartsWith(PipeCommands.Error.ToString()) == false))
                {
                    string errortext = "Epgwatch did end without error and not ready";
                    Logdebug(errortext);
                    SendServerPipeMessage(errortext, PipeCommands.Error);
                }

                //enable filewatcher
                //StartEPGsearch.EnableRaisingEvents = true;
                
            }
            catch (Exception exc)
            {
                Log.Error("filewatcher epgwatch failed with exception message:");
                Log.Error(exc.Message);
            }
        }

        public string ExportTvWishes(bool viewonly)
        {
            Log.Debug("Exporting Tv Wishes viewonly=" + viewonly.ToString());

            TvWishProcessing myTvWishes = new TvWishProcessing();
            myTvWishes.ViewOnlyMode = viewonly;
            XmlMessages mymessages = new XmlMessages("", "", false);

            // load data


            

            //old code from MP plugin reused
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
                string exportfile = "";
                string listviewdata = "";
                string messageString = "";


                if (myTvWishes.ViewOnlyMode == false)
                {
                    exportfile = TV_USER_FOLDER + @"\TvWishList\Tvwishes.txt";
                    listviewdata = myTvWishes.loadlongsettings("TvWishList_ListView");
                    messageString = myTvWishes.loadlongsettings("TvWishList_ListViewMessages");
                }
                else
                {
                    exportfile = TV_USER_FOLDER + @"\TvWishList\TvwishesViewOnly.txt";
                    listviewdata = myTvWishes.loadlongsettings("TvWishList_OnlyView");
                    messageString = myTvWishes.loadlongsettings("TvWishList_OnlyViewMessages");
                }                              

                if (File.Exists(exportfile) == true)
                    File.Move(exportfile, exportfile + timestamp + ".txt");

                File.WriteAllText(exportfile, listviewdata);
                int count = listviewdata.Split('\n').Length;

                //handle messages
                mymessages.readxmlfile(messageString, false);
                if (myTvWishes.ViewOnlyMode == false)
                {
                    string MESSAGEFILE = TV_USER_FOLDER + @"\TvWishList\Messages.xml";
                    if (File.Exists(MESSAGEFILE) == true)
                        File.Move(MESSAGEFILE, MESSAGEFILE.Substring(0, MESSAGEFILE.Length - 4) + timestamp + ".xml");

                    mymessages.filename = MESSAGEFILE;
                    mymessages.writexmlfile(true);
                }
                else
                {
                    string MESSAGEFILE = TV_USER_FOLDER + @"\TvWishList\ViewOnlyMessages.xml";

                    if (File.Exists(MESSAGEFILE) == true)
                        File.Move(MESSAGEFILE, MESSAGEFILE.Substring(0, MESSAGEFILE.Length - 4) + timestamp + ".xml");

                    mymessages.filename = TV_USER_FOLDER + @"\TvWishList\ViewOnlyMessages.xml";
                    mymessages.writexmlfile(true);
                }


                //Info, Export of {0} wishes completed
                //myTvWishes.MyMessageBox(4400, string.Format(PluginGuiLocalizeStrings.Get(1556), count.ToString()));
                Log.Debug("Export of " + count.ToString() + " wishes completed");
                return ("Export of " + count.ToString() + " wishes completed");
            }
            catch (Exception exc)
            {
                //myTvWishes.MyMessageBox(4305, 1557);  //Export failed
                Log.Error("Error in Exporting Tv wishes - Exception:" + exc.Message);
                return("Error: Export of Tv wishes failed");
            }
        }

        public string ImportTvWishes(bool viewonly)
        {
            Log.Debug("Importing Tv Wishes viewonly=" + viewonly.ToString());

            TvWishProcessing myTvWishes = new TvWishProcessing();
            myTvWishes.ViewOnlyMode = viewonly;
            XmlMessages mymessages = new XmlMessages("", "", false);

            // import data and save data

            try
                {
                    string importfile = "";

                    if (myTvWishes.ViewOnlyMode == false)
                    {
                        importfile = TV_USER_FOLDER + @"\TvWishList\Tvwishes.txt";
                        mymessages.filename = TV_USER_FOLDER + @"\TvWishList\Messages.xml";
                        mymessages.readxmlfile("", true);
                    }
                    else
                    {
                        importfile = TV_USER_FOLDER + @"\TvWishList\TvwishesViewOnly.txt";
                        mymessages.filename = TV_USER_FOLDER + @"\TvWishList\ViewOnlyMessages.xml";
                        mymessages.readxmlfile("", true);
                    }

                    myTvWishes.Clear();
                    string wishdata = File.ReadAllText(importfile);
                    int count1 = myTvWishes.ListAll().Count;
                    myTvWishes.LoadFromString(wishdata, false);
                    int count = myTvWishes.ListAll().Count-count1;


                    string messageString = mymessages.writexmlfile(false);

                    if (myTvWishes.ViewOnlyMode == true)
                    {
                        Log.Debug("VIEWONLY = true   TvWishList_OnlyView");
                        myTvWishes.save_longsetting(wishdata, "TvWishList_OnlyView");
                        myTvWishes.save_longsetting(messageString, "TvWishList_OnlyViewMessages");
                    }
                    else
                    {
                        Log.Debug("VIEWONLY = false  TvWishList_ListView ");
                        myTvWishes.save_longsetting(wishdata, "TvWishList_ListView");
                        myTvWishes.save_longsetting(messageString, "TvWishList_ListViewMessages");
                    }


                    Log.Debug("Import of " + count .ToString()+ " wishes completed");
                    return ("Import of " + count.ToString() + " wishes completed");
                    //myTvWishes.MyMessageBox(4400, string.Format(PluginGuiLocalizeStrings.Get(1558), count.ToString()));                                       
                
                }
                catch (Exception exc)
                {
                    //myTvWishes.MyMessageBox(4305, 1559);  //Import failed
                    Log.Error("Error in More Case 6 - Exception:" + exc.Message);
                    return ("Error: Import of Tv wishes failed");
                }
        }


        #endregion serverpipe


        void Logdebug(String text)
        {
            if (DEBUG==true)
                Log.Debug("TvWishList TvServer: "+ text);
        }

        #endregion
    }

    /*
    // Defines the data protocol for reading and writing strings on our stream
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = 0;

            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                Log.Error("******ERROR: Write string too large - cutting value");
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }*/

}
