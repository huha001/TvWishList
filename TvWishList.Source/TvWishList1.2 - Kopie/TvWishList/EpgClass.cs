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

// reusing email pop3 class with modifications from Media Portal Plugin MyMail from _Agree_ downloaded at
// http://mp-plugins.svn.sourceforge.net/viewvc/mp-plugins/trunk/plugins/MyMail/


using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
//using similaritymetrics;

using TvLibrary.Log.huha;
using TvControl;
using SetupTv;
using TvEngine;
using TvEngine.Events;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvDatabase;
using MediaPortal.Plugins;
using TvEngine.PowerScheduler.Interfaces;
using TvWishList;

using MySql.Data.MySqlClient;
using Gentle.Framework;
using StatementType = Gentle.Framework.StatementType;

namespace MyTVMail
{
  
  public delegate void setuplabelmessage(string text, PipeCommands type );
  public class EpgParser
  {
      //Global Definitions
      bool DEBUG = true;
      string RESPONSE = "";           //replying email
      char TV_WISH_COLUMN_SEPARATOR = ';';
      int MAXFOUND = 100;
      int ErrorWaitTime = 5000;
      bool VIEW_ONLY_MODE = false;
      bool HtmlFormat = false;
      //bool EpgMarkerExpression = false;

      string TV_USER_FOLDER = "";
      string FileRenameXML = "FileRenameXML.xml"; //global file containing all file renaming jobs

      
            

      //bool _checkBoxAllowDelete = false; no more used
      bool _emailreply = false;
      bool _automaticrecording = false;
      bool _checkBoxSSL=false;
      bool _scheduleconflicts = false;
      bool _emailonlynew = true;
      bool _DeleteTimeChanges = true;
      bool _skipDeleted = false;

      bool _filter_email=true;
      bool _filter_scheduled = true;
      bool _filter_conflicts = true;
      

      //string Emailformat = "NEW ENTRY<br>{21}<br>Title={0}<br>Description={1}<br>Channel={2}<br>Start={3}<br>End={4}<br>Type={15}<br>Created={17}<br><br><br>";

      bool _descendingsort = false;
      int _sort = 0;

      string _EpgMarker = "";
      string _TextBoxUserName = "";
      string _TextBoxPassword = "";
      string _TextBoxSmtpEmailAddress = "";

      string s_receiver = "";
      string _TextBoxSmtpServer = "";
      string s_subject = "TvWishList on "+System.Windows.Forms.SystemInformation.ComputerName;
      int _numericUpDownSmtpPort = 0;
      string message = "";
      //bool _useCredentials = false;

      public IList<EPGdata> epgdatalist = new List<EPGdata>();

      [CLSCompliant(false)]
      public event setuplabelmessage newlabelmessage;
      //string filewatchermessages = "";

      XmlMessages mymessage;
      TvWishProcessing myTvWishes;

      LanguageTranslation lng = new LanguageTranslation();

      /*
#if(TV100)
      IList conflictprograms = new List<Program>();
#elif(TV101 || TV11 || TV12)
      IList<Program> conflictprograms = new List<Program>();
#endif*/

      enum LogSetting
      {
          DEBUG = 1,
          ERROR,
          ERRORONLY,
          INFO,
          ADDRESPONSE
      }

      [CLSCompliant(false)]
      public struct EPGdata
      {
          public string Name;
          public string Description;
          public string GroupNames;
          public DateTime StartTime;
          public Program singleprogram;
      };

      

      public bool SearchEPG(bool view_only_mode)
      {
          Log.Info("[TvWishList]:SearchEPG started with view_only_mode=" + view_only_mode.ToString(), (int)LogSetting.DEBUG);
          VIEW_ONLY_MODE = view_only_mode;
          myTvWishes = new TvWishProcessing();
          // load globals from data base
          TvBusinessLayer layer = new TvBusinessLayer();
          Setting setting;
          
          /*
          //Debug only
          Log.Debug("Debug Schedules:");
          foreach (Schedule myschedule in Schedule.ListAll())
          {
              outputscheduletoresponse(myschedule, (int)LogSetting.DEBUG);
          }
          //end debug  
          */

          lng.ReadLanguageFile();
          Log.Debug("language file read");

          try
          {
              setting = layer.GetSetting("TvWishList_Debug", "false");

              if (BoolConversion(setting.Value,false))
              {
                  DEBUG = true;
              }
              else
              {
                  DEBUG = false;
              }
              Log.DebugValue = DEBUG;

              //TV User Folder
              setting = layer.GetSetting("TvWishList_TV_USER_FOLDER", "NOT_FOUND");
              TV_USER_FOLDER = setting.Value;

              /*// delete later - no more needed
              filewatchermessages = TV_USER_FOLDER + @"\TvWishList\filewatchermessages.txt";
              if (File.Exists(filewatchermessages) == true)
              {
                  File.Delete(filewatchermessages);
                  LogDebug("Deleting file " + filewatchermessages, (int)LogSetting.DEBUG);
              }*/
          }
          catch
          {
              string languagetext = lng.TranslateString("Fatal error - check the log file",24 );
              labelmessage(languagetext, PipeCommands.Error);
              LogDebug("Could not read from TV database", (int)LogSetting.ERROR);
              return false;
          }




          //*****************************************************
          //Lock TvWishList with timeout error
          bool success = false;
          int seconds = 60;
          for (int i = 0; i < seconds / 10; i++)
          {
              success = myTvWishes.LockTvWishList("TvWishList EpgSearch");
              if (success)
                  break;
              System.Threading.Thread.Sleep(10000); //sleep 10s to wait for BUSY=false
              
              string languagetext = lng.TranslateString("Waiting for old jobs {0}s to finish", 1, (seconds - i * 10).ToString());
              Log.Debug(languagetext, (int)LogSetting.DEBUG);
              labelmessage(languagetext, PipeCommands.StartEpg);
          }
          if (success == false)
          {
              string languagetext = lng.TranslateString("Error: TvWishList did not finish old jobs - reboot your computer", 2);
              LogDebug(languagetext, (int)LogSetting.ERROR);
              labelmessage(languagetext, PipeCommands.Error);
              return false;
          }

          try
          {

              string languagetext = lng.TranslateString("Reading data settings", 3);
              LogDebug(languagetext, (int)LogSetting.DEBUG);
              labelmessage(languagetext, PipeCommands.StartEpg);

              //checkboxes


              setting = layer.GetSetting("TvWishList_SkipDeleted", "false");
              if (BoolConversion(setting.Value, false))
                  _skipDeleted = true;
              else
                  _skipDeleted = false;

              setting = layer.GetSetting("TvWishList_EmailReply", "true");
              if (BoolConversion(setting.Value, true))
                  _emailreply = true;
              else
                  _emailreply = false;


              setting = layer.GetSetting("TvWishList_Schedule", "true");
              if (BoolConversion(setting.Value, true))
                  _automaticrecording = true;
              else
                  _automaticrecording = false;


              setting = layer.GetSetting("TvWishList_ScheduleConflicts", "false");
              if (BoolConversion(setting.Value, false))
                  _scheduleconflicts = true;
              else
                  _scheduleconflicts = false;

              setting = layer.GetSetting("TvWishList_EmailOnlyNew", "true");
              if (BoolConversion(setting.Value, true))
                  _emailonlynew = true;
              else
                  _emailonlynew = false;

              setting = layer.GetSetting("TvWishList_DeleteTimeChanges", "true");
              if (BoolConversion(setting.Value, true))
                  _DeleteTimeChanges = true;
              else
                  _DeleteTimeChanges = false;

              setting = layer.GetSetting("TvWishList_FilterEmail", "true");
              if (BoolConversion(setting.Value, true))
                  _filter_email = true;
              else
                  _filter_email = false;

              setting = layer.GetSetting("TvWishList_FilterRecord", "true");
              if (BoolConversion(setting.Value, true))
                  _filter_scheduled = true;
              else
                  _filter_scheduled = false;

              setting = layer.GetSetting("TvWishList_FilterConflicts", "true");
              if (BoolConversion(setting.Value, true))
                  _filter_conflicts = true;
              else
                  _filter_conflicts = false;

              setting = layer.GetSetting("TvWishList_DescendingSort", "false");
              if (BoolConversion(setting.Value, true))
                  _descendingsort = true;
              else
                  _descendingsort = false;

              //textboxes
              string Emailformat = myTvWishes.loadlongsettings("TvWishList_EmailFormat");
              if (Emailformat == string.Empty)
              {
                  Emailformat = lng.TranslateString(Emailformat, 90);
              }
              Emailformat = Emailformat.Replace(@"\n", "\n");
              Emailformat = Emailformat.Replace("<br>", "\n");
              Emailformat = Emailformat.Replace("<BR>", "\n");
              string myEmailformat=Emailformat.ToString().Replace('{', '_');
              myEmailformat=myEmailformat.ToString().Replace('}', '_');
              LogDebug("Emailformat :" + myEmailformat, (int)LogSetting.DEBUG);

              //datetimeformat
              setting = layer.GetSetting("TvWishList_DateTimeFormat", "");
              string DateTimeFormat = setting.Value.ToString();
              if (DateTimeFormat == string.Empty)
              {
                  DateTimeFormat = lng.TranslateString("{1:00}/{2:00} at {3:00}:{4:00}", 91);  
              }

              string myDateformat = DateTimeFormat.ToString().Replace('{', '_');
              myDateformat = myDateformat.ToString().Replace('}', '_');
              LogDebug("DateTimeFormat=" + myDateformat, (int)LogSetting.DEBUG);

              

              //initialize messages
              string messagedata = "";
              mymessage = new XmlMessages(DateTimeFormat, Emailformat, DEBUG);
              languagetext = lng.TranslateString("Loading Messages", 4);
              Log.Debug(languagetext);
              labelmessage(languagetext, PipeCommands.StartEpg);
              if (VIEW_ONLY_MODE == true)
              {
                  messagedata = ""; //start with a clean message list for viewonlymode
              }
              else
              {
                  //mymessage.filename = TV_USER_FOLDER + @"\TvWishList\Messages.xml";
                  messagedata = myTvWishes.loadlongsettings("TvWishList_ListViewMessages");

              }
              mymessage.readxmlfile(messagedata, false);
              Log.Debug("mymessage.TvMessages.Count=" + mymessage.ListAllTvMessages().Count.ToString());
              //mymessage.logmessages();  //DEBUG ONLY

              


              setting = layer.GetSetting("TvWishList_Sort", "Start");
              string sortstring = setting.Value;
              if (sortstring == "Title")
                  _sort = (int)XmlMessages.Sorting.Title;
              else if (sortstring == "Start")
                  _sort = (int)XmlMessages.Sorting.Start;
              else if (sortstring == "Created")
                  _sort = (int)XmlMessages.Sorting.Created;
              else if (sortstring == "Genre")
                  _sort = (int)XmlMessages.Sorting.Genre;
              else if (sortstring == "Classification")
                  _sort = (int)XmlMessages.Sorting.Classification;
              else if (sortstring == "ParentalRating")
                  _sort = (int)XmlMessages.Sorting.ParentalRating;
              else if (sortstring == "StarRating")
                  _sort = (int)XmlMessages.Sorting.StarRating;
              else if (sortstring == "Type")
                  _sort = (int)XmlMessages.Sorting.Type;
              else if (sortstring == "Message")
                  _sort = (int)XmlMessages.Sorting.Message;
              else if (sortstring == "SearchString")
                  _sort = (int)XmlMessages.Sorting.SearchString;
              else if (sortstring == "EpisodeName")
                  _sort = (int)XmlMessages.Sorting.EpisodeName;
              else if (sortstring == "EpisodeNum")
                  _sort = (int)XmlMessages.Sorting.EpisodeNum;
              else if (sortstring == "EpisodeNumber")
                  _sort = (int)XmlMessages.Sorting.EpisodeNumber;
              else if (sortstring == "EpisodePart")
                  _sort = (int)XmlMessages.Sorting.EpisodePart;


              //EPG marker
              setting = layer.GetSetting("TvWishList_EpgMarker", "");
              _EpgMarker = setting.Value;
              /*if (_EpgMarker.Contains(@"|"))
              {
                  EpgMarkerExpression = true;
              }
              else
              {
                  EpgMarkerExpression = false;
              }*/

              //textboxes
              setting = layer.GetSetting("TvWishList_UserName", "");
              _TextBoxUserName = setting.Value;

              setting = layer.GetSetting("TvWishList_Password", "");
              _TextBoxPassword = setting.Value;

              setting = layer.GetSetting("TvWishList_TestReceiver", "");
              s_receiver = setting.Value;

              setting = layer.GetSetting("TvWishList_SmtpEmailAddress", "");
              _TextBoxSmtpEmailAddress = setting.Value;


              //providerdata
              setting = layer.GetSetting("TvWishList_Providers_0", "_Last Setting;;0;False");
              string[] tokenarray = setting.Value.Split(';');
              if (tokenarray.Length != 4)
              {
                  LogDebug("Provider array has invalid number of elements: " + tokenarray.Length.ToString(), (int)LogSetting.ERROR);
              }
              else
              {
                  try
                  {
                      _TextBoxSmtpServer = tokenarray[1];
                      _numericUpDownSmtpPort = Convert.ToInt32(tokenarray[2]);
                      _checkBoxSSL = Convert.ToBoolean(tokenarray[3]);
                  }
                  catch (Exception ex)
                  {
                      LogDebug("Failed converting provider data with exception: " + ex.Message, (int)LogSetting.ERROR);
                      languagetext = lng.TranslateString("Fatal error - check the log file", 24);
                      labelmessage(languagetext, PipeCommands.StartEpg); //do not stop - do not flag as error
                      Thread.Sleep(ErrorWaitTime);
                  }
              }

              //maxfound
              setting = layer.GetSetting("TvWishList_MaxFound", "100");
              try
              {
                  MAXFOUND = Convert.ToInt32(setting.Value);
              }
              catch
              {
                  LogDebug("Max Found could not be converted to number  resetting to 100", (int)LogSetting.DEBUG);
                  MAXFOUND = 100;
                  languagetext = lng.TranslateString("Fatal error - check the log file", 24);
                  labelmessage(languagetext, PipeCommands.StartEpg); //do not stop - do not flag as error
                  Thread.Sleep(ErrorWaitTime);
              }


              setting = layer.GetSetting("TvWishList_MaxTvWishId", "0");
              myTvWishes.MaxTvWishId = Convert.ToInt32(setting.Value);
              Log.Debug("EpgClass: MaxTvWishId=" + myTvWishes.MaxTvWishId.ToString(), (int)LogSetting.DEBUG);


              //deleteExpiration in months
              int deleteExpiration = 12;
              setting = layer.GetSetting("TvWishList_DeleteExpiration", "12");
              try
              {
                  deleteExpiration = Convert.ToInt32(setting.Value);
              }
              catch
              {
                  LogDebug("Delete Expiration could not be converted to number  resetting to 12", (int)LogSetting.ERROR);
                  deleteExpiration = 12;
                  languagetext = lng.TranslateString("Fatal error - check the log file", 24);
                  labelmessage(languagetext, PipeCommands.StartEpg); //do not stop - do not flag as error
                  Thread.Sleep(ErrorWaitTime);
              }

              //listviewdata
              setting = layer.GetSetting("TvWishList_ColumnSeparator", ";");
              TV_WISH_COLUMN_SEPARATOR = setting.Value[0];

              //default pre and post record from general recording settings
              setting = layer.GetSetting("preRecordInterval", "5");
              string prerecord = setting.Value;
              setting = layer.GetSetting("postRecordInterval", "5");
              string postrecord = setting.Value;

              myTvWishes.TvServerSettings(prerecord, postrecord, ChannelGroup.ListAll(), RadioChannelGroup.ListAll(), Channel.ListAll(), Card.ListAll(), TV_WISH_COLUMN_SEPARATOR);

              languagetext = lng.TranslateString("Loading Tv wishes", 5);
              Log.Debug(languagetext);
              labelmessage(languagetext, PipeCommands.StartEpg);
              string listviewdata = "";
              if (VIEW_ONLY_MODE == true)
              {
                  listviewdata = myTvWishes.loadlongsettings("TvWishList_OnlyView");  //never change setting name must match to MP plugin and later savelongsetting             
              }
              else
              {
                  listviewdata = myTvWishes.loadlongsettings("TvWishList_ListView");
              }
              Log.Debug("listviewdata=" + listviewdata, (int)LogSetting.DEBUG);
              myTvWishes.Clear();
              myTvWishes.LoadFromString(listviewdata, true);



              RESPONSE = "";
              //conflictprograms.Clear();

              //update messages before conflict checking of EPG data
              Log.Debug("after reading messages: TvMessages.Count=" + mymessage.ListAllTvMessages().Count.ToString());
              mymessage.updatemessages(deleteExpiration);
              Log.Debug("after update messages: TvMessages.Count=" + mymessage.ListAllTvMessages().Count.ToString());


              //Debug for foxbenw issue
              Log.Debug("Outputting all schedules before schedule processing");
              foreach (Schedule oneschedule in Schedule.ListAll())
              {
                  mymessage.outputscheduletoresponse(oneschedule, (int)LogSetting.DEBUG);
              }
              Log.Debug("End of Outputting all schedules before schedule processing");



              //#if (MP11RC || MP12)
#if (TV11 || TV12)
              // check for conflicts between epg data and schedules (only for 1.1 final lor later)

              foreach (Schedule oneschedule in Schedule.ListAll())
              {

                  LogDebug("Schedule=" + oneschedule.ProgramName, (int)LogSetting.DEBUG);
                  LogDebug("Schedule Start Time: " + oneschedule.StartTime.ToString(), (int)LogSetting.DEBUG);
                  LogDebug("Schedule End Time: " + oneschedule.EndTime.ToString(), (int)LogSetting.DEBUG);
                  LogDebug("Schedule Channel: " + oneschedule.IdChannel.ToString(), (int)LogSetting.DEBUG);
                  LogDebug("Schedule ID: " + oneschedule.IdSchedule.ToString(), (int)LogSetting.DEBUG);
                  bool EpgTimeChanged = false;
                  Program testprogram = null;
                  Schedule testschedule = oneschedule;
                  try
                  {
                      testprogram = Program.RetrieveByTitleTimesAndChannel(oneschedule.ProgramName, oneschedule.StartTime, oneschedule.EndTime, oneschedule.IdChannel);
                  }
                  catch
                  {
                      testprogram = null;
                  }

                  if (_DeleteTimeChanges == true)
                  {
                      // check for valid EPG entry

                      if (testprogram == null)
                      {
                          // check for changed time on same channel
                          IList<Program> alternativeprograms = null;
                          alternativeprograms = Program.RetrieveEveryTimeOnThisChannel(oneschedule.ProgramName, oneschedule.IdChannel);
                          if (alternativeprograms != null)
                          {
                              // search for closest program to original start time
                              double minimumdifferenz = 10000000.0; //start with largest value > 4 weeks
                              Program minprogram = null;
                              foreach (Program altprogram in alternativeprograms)
                              {
                                  LogDebug("Alternate EPG=" + altprogram.Title, (int)LogSetting.DEBUG);
                                  LogDebug("Alternate Start Time: " + altprogram.StartTime.ToString(), (int)LogSetting.DEBUG);
                                  LogDebug("Alternate End Time: " + altprogram.EndTime.ToString(), (int)LogSetting.DEBUG);

                                  double totalminutes = (altprogram.StartTime - oneschedule.StartTime).TotalMinutes;
                                  if (totalminutes < 0)
                                      totalminutes = totalminutes * (-1);



                                  LogDebug("Differenz to Schedule totalminutes=" + totalminutes.ToString(), (int)LogSetting.DEBUG);

                                  // int differenz = oneschedule.StartTime.Subtract(altprogram.StartTime).Minutes;
                                  //DateTime.Compare(oneschedule.StartTime, altprogram.StartTime);


                                  if (totalminutes < minimumdifferenz)
                                  {
                                      minimumdifferenz = totalminutes;
                                      minprogram = altprogram;
                                      
                                  }
                              }
                              LogDebug("Minimum Differenz to Schedule =  " + minimumdifferenz.ToString(), (int)LogSetting.DEBUG);


                              if (minprogram != null)
                              {
                                  //alternative program found
                                  Schedule schedule = layer.AddSchedule(minprogram.IdChannel, minprogram.Title, minprogram.StartTime, minprogram.EndTime, 0);
                                  schedule.PreRecordInterval = oneschedule.PreRecordInterval;
                                  schedule.PostRecordInterval = oneschedule.PostRecordInterval;
                                  schedule.ScheduleType = oneschedule.ScheduleType;
                                  schedule.Series = oneschedule.Series;
                                  schedule.KeepDate = oneschedule.KeepDate;
                                  schedule.KeepMethod = oneschedule.KeepMethod;
                                  schedule.RecommendedCard = oneschedule.RecommendedCard;
                                  schedule.Priority = oneschedule.Priority;
                                  schedule.Persist();
                                  LogDebug("", (int)LogSetting.INFO);
                                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                                  message = lng.TranslateString("Schedule {0} had no valid EPG data but could be corrected", 50, oneschedule.ProgramName);
                                  LogDebug("Scheduled New= " + schedule.ProgramName, (int)LogSetting.INFO);
                                  LogDebug("New Start Time= " + schedule.StartTime.ToString(), (int)LogSetting.INFO);
                                  LogDebug("New End Time= " + schedule.EndTime.ToString(), (int)LogSetting.INFO);
                                  LogDebug("Deleted= " + oneschedule.ProgramName, (int)LogSetting.INFO);
                                  LogDebug("Old Start Time= " + oneschedule.StartTime.ToString(), (int)LogSetting.INFO);
                                  LogDebug("OLd End Time= " + oneschedule.EndTime.ToString(), (int)LogSetting.INFO);
                                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);

                                  EpgTimeChanged = true;

                                  //new
                                  // try to change schedule messages if they exist from old data                                 
                                  try
                                  {
                                      int k;
                                      k = mymessage.GetTvMessageBySchedule(oneschedule,MessageType.Scheduled);
                                      LogDebug("try to change schedule message: k="+k.ToString(), (int)LogSetting.DEBUG);
                                      xmlmessage scheduledmessage = mymessage.GetTvMessageAtIndex(k);
                                      LogDebug("schedule message retrieved", (int)LogSetting.DEBUG);
                                      bool ok = mymessage.addmessage(schedule, scheduledmessage.message, MessageType.Scheduled, scheduledmessage.searchstring, (int)XmlMessages.MessageEvents.SCHEDULE_FOUND, scheduledmessage.tvwishid, string.Empty);
                                      LogDebug("ok=" + ok.ToString(), (int)LogSetting.DEBUG);

                                      if (ok)
                                        mymessage.DeleteTvMessageAt(k);

                                      /*
                                      updatedmessage.start = schedule.StartTime;
                                      updatedmessage.end = schedule.EndTime;
                                      mymessage.ReplaceTvMessageAtIndex(k, updatedmessage);*/
                                      LogDebug("new schedule message has been added", (int)LogSetting.DEBUG);
                                  }
                                  catch
                                  {
                                      LogDebug("schedule message could not be found", (int)LogSetting.DEBUG);
                                  }

                                  //add new email message if it did exist
                                  try
                                  {
                                      int k;
                                      k = mymessage.GetTvMessageBySchedule(oneschedule, MessageType.Emailed);
                                      LogDebug("try to change email message: k=" + k.ToString(), (int)LogSetting.DEBUG);
                                      xmlmessage emailmessage = mymessage.GetTvMessageAtIndex(k);
                                      LogDebug("email message retrieved", (int)LogSetting.DEBUG);
                                      bool ok = mymessage.addmessage(schedule, emailmessage.message, MessageType.Emailed, emailmessage.searchstring, (int)XmlMessages.MessageEvents.EMAIL_FOUND, emailmessage.tvwishid, string.Empty);
                                      LogDebug("ok=" + ok.ToString(), (int)LogSetting.DEBUG);
                                      if (ok)
                                        mymessage.DeleteTvMessageAt(k);

                                      LogDebug("new  email message has been added", (int)LogSetting.DEBUG);
                                  }
                                  catch
                                  {
                                      LogDebug("Email message could not be found", (int)LogSetting.DEBUG);
                                  }

                                  //end new change


                                  Log.Debug("Deleting schedule "+oneschedule.ProgramName+" with id="+oneschedule.IdSchedule.ToString());
                                  oneschedule.Delete();
                                  //delete old schedule if possible
                                  mymessage.addmessage(oneschedule, message, MessageType.Conflict, "", (int)XmlMessages.MessageEvents.NO_VALID_EPG, "-1", string.Empty);

                                  //reassign testprogram and testschedule
                                  testprogram = minprogram;
                                  testschedule = schedule;
                                  
                              }
                              else
                              {
                                  LogDebug("", (int)LogSetting.INFO);
                                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                                  message = lng.TranslateString("Schedule {0} has no valid EPG data - check your schedules for conflicts",51,oneschedule.ProgramName);
                                  LogDebug(message, (int)LogSetting.INFO);
                                  LogDebug("Schedule start date = " + oneschedule.StartTime.ToString() + "\n", (int)LogSetting.INFO);
                                  LogDebug("Schedule end date= " + oneschedule.EndTime.ToString() + "\n\n", (int)LogSetting.INFO);                                 
                                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                                  mymessage.addmessage(oneschedule, message, MessageType.Conflict, "", (int)XmlMessages.MessageEvents.NO_VALID_EPG, "-1", string.Empty);
                              }

                          }
                          else  //no alternative program does exist - email warning
                          {
                              LogDebug("", (int)LogSetting.INFO);
                              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                              message = lng.TranslateString("Schedule {0} has no valid EPG data - check your schedules for conflicts",52,oneschedule.ProgramName);
                              LogDebug(message, (int)LogSetting.INFO);
                              LogDebug( "Schedule start date = " + oneschedule.StartTime.ToString() + "\n", (int)LogSetting.INFO);
                              LogDebug("Schedule end date= " + oneschedule.EndTime.ToString() + "\n\n", (int)LogSetting.INFO);                            
                              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                              mymessage.addmessage(oneschedule, message, MessageType.Conflict, "", (int)XmlMessages.MessageEvents.NO_VALID_EPG, "-1", string.Empty);   //do not use -1 bug!!! 

                          }

                      }// else: Schedule does match to EPG data - check next schedule

                  }//end epg changes


                 

                  //try to reschedule if episode or description epg data and message have been changed, but only if tvwish still exists
                 
                  int i;
                  try
                  {
                      i = mymessage.GetTvMessageBySchedule(testschedule,MessageType.Scheduled); // new bugfix: must be type scheduled
                  }
                  catch
                  {
                      i = -1;
                  }

                  Log.Debug("message index i=" + i.ToString());

                  if ((i >= 0) && (testprogram != null))//scheduled message does exist for schedule and program does exist for schedule
                  {
                      try
                      {
                          xmlmessage testmessage = mymessage.GetTvMessageAtIndex(i);
                          Log.Debug("retriefed testmessage.title=" + testmessage.title);

                          //get tvwish (which can  cause exception for unvalid entries)
                          TvWish mytestwish = myTvWishes.RetrieveById(testmessage.tvwishid);
                          Log.Debug("retrieved mytestwish.name=" + mytestwish.name);

                          bool ok = episodeManagementEmptyString(testprogram.Description, testmessage.description, testprogram.EpisodePart, testmessage.EpisodePart, testprogram.EpisodeName, testmessage.EpisodeName,
                          testprogram.SeriesNum, testmessage.SeriesNum, testprogram.EpisodeNum, testmessage.EpisodeNum, mytestwish.b_episodecriteria_d, mytestwish.b_episodecriteria_n, mytestwish.b_episodecriteria_c);

                          if ((ok == false) || (EpgTimeChanged))//epg episode data or epg time did change  //new: always try to reschedule if epg time changed because simultanous change of EPG data cannot be tracked
                          {
                              //conflict message
                              testmessage.type = MessageType.Conflict.ToString();
                              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                              message = lng.TranslateString("Epg data did change for Episode/Description or EPG Time changed- deleting current schedule and trying to reschedule",53);
                              LogDebug(message, (int)LogSetting.INFO);
                              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);

                              testmessage.message = message;
                              mymessage.ReplaceTvMessageAtIndex(i, testmessage);

                              //delete schedule
                              Log.Debug("Deleting testschedule " + testschedule.ProgramName + " with id=" + testschedule.IdSchedule.ToString());
                              testschedule.Delete();

                              if (mytestwish.b_active == false)
                              {//need to run now again for inactive tvwish
                                  mytestwish.b_active = true;
                                  Log.Debug("Setting Tvwish active and running sql query");
                                  //search for schedules
                                  SqlQueryPrograms(ref mytestwish, i);

                                  mytestwish.b_active = false;
                                  myTvWishes.ReplaceAtIndex(i, mytestwish);
                              }
                          }


                      }
                      catch //ignore errors
                      {
                      }

                  }//end of episode changes

              } //end all schedules
#endif

              //Debug for foxbenw issue
              Log.Debug("Outputting all schedules before tvwish processing");
              foreach (Schedule oneschedule in Schedule.ListAll())
              {
                  mymessage.outputscheduletoresponse(oneschedule, (int)LogSetting.DEBUG);
              }
              Log.Debug("End of Outputting all schedules before tvwish processing");




              // start processing all TvWishes


              TvWish mywish = null;

              for (int i = 0; i < myTvWishes.ListAll().Count; i++)
              {
                  mywish = myTvWishes.GetAtIndex(i);

                  if (DEBUG)
                  {
                      Log.Debug("Before Query:");
                      myTvWishes.DebugTvWish(mywish);
                  }


                  if ((mywish.name == "") && (mywish.searchfor == "") && (mywish.episodename == "") && (mywish.episodenumber == "") && (mywish.episodepart == ""))
                  {
                      Log.Debug("Skipping tvwish with id=" + mywish.tvwishid.ToString());
                      continue;
                  }

                  languagetext = lng.TranslateString("Searching for {0}", 6,mywish.name);
                  labelmessage(languagetext, PipeCommands.StartEpg);

                  //search for recordings and add messages only in email mode
                  if (VIEW_ONLY_MODE == false) //recording first to identify existing recordings
                      SqlQueryRecordings(mywish, i);


                  //search for schedules
                  SqlQueryPrograms(ref mywish, i);

                  myTvWishes.ReplaceAtIndex(i, mywish);

                  /*
                  if (DEBUG)
                  {
                      Log.Debug("After Query:");
                      myTvWishes.DebugTvWish(mywish);
                  }*/

                  

              }  //end all Tvwishes




              //check for remaining schedule conflicts


#if(TV11 || TV12) //only for 1.1 final

              IList<Schedule> allschedules = Schedule.ListAll();
              IList<Card> cards = Card.ListAll();


              // initialize conflicting schedules and assign all existing schedules to cards
              List<Schedule> conflicts = new List<Schedule>();


              if (cards.Count != 0)
              {


                  //LogDebug("GetConflictingSchedules: Cards.Count =" + cards.Count.ToString(), (int)LogSetting.DEBUG);

                  List<Schedule>[] cardSchedules = new List<Schedule>[cards.Count];
                  for (int i = 0; i < cards.Count; i++)
                  {
                      cardSchedules[i] = new List<Schedule>();
                  }
                  Schedule overlappingSchedule = null;

                  bool ok = false;
                  foreach (Schedule oneschedule in allschedules)
                  {
                      ok = AssignSchedulesToCard(oneschedule, cardSchedules, out overlappingSchedule, DEBUG);
                      if (ok == false)//conflict exists
                      {
                          LogDebug("", (int)LogSetting.INFO);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                          message = lng.TranslateString("Schedule conflict must be manually resolved",54);
                          LogDebug(message, (int)LogSetting.INFO);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                          outputscheduletoresponse(oneschedule, (int)LogSetting.INFO);
                          LogDebug("", (int)LogSetting.INFO);

                          mymessage.addmessage(oneschedule, message, MessageType.Conflict, "", (int)XmlMessages.MessageEvents.MANUAL_CONFLICT, "-1", string.Empty);  //Bug -1

                      }
                  }



              }
#endif


              //enable html email format
              if ((mymessage.EmailFormat.StartsWith("USE_HTML")) && (mymessage.EmailFormat.Length > 7))
              {
                  mymessage.EmailFormat = mymessage.EmailFormat.Substring(8);
                  HtmlFormat = true;
              }
              else
              {
                  HtmlFormat = false;
              }

              //Send all reply mails
              //update messages was done at the beginning
              mymessage.sortmessages(_sort, _descendingsort);
              mymessage.filtermessages(_filter_email, false, _filter_conflicts, _filter_scheduled, false, false, false);
              // old: mymessage.filtermessages(_filter_email,_filter_scheduled,_filter_conflicts,false,false,false);
              RESPONSE = mymessage.emailmessages(_emailonlynew);


              //store modified listviewdata and messages back

              //update counters before saving data
              myTvWishes.UpdateCounters(mymessage.ListAllTvMessages());

              //log messages
              mymessage.logmessages();

              string listviewstring = myTvWishes.SaveToString();

              //mymessage.logmessages(); //DEBUGONLY
              string dataString = mymessage.writexmlfile(false); //write xml file to string

              //LogDebug("Updated listview string: \n" + listviewstring, (int)LogSetting.DEBUG);
              if (VIEW_ONLY_MODE == true)
              {
                  myTvWishes.save_longsetting(listviewstring, "TvWishList_OnlyView");  //do never modify keywords must match MP plugin
                  myTvWishes.save_longsetting(dataString, "TvWishList_OnlyViewMessages");
              }
              else
              {
                  myTvWishes.save_longsetting(listviewstring, "TvWishList_ListView");   //do never modify keywords must match MP plugin
                  myTvWishes.save_longsetting(dataString, "TvWishList_ListViewMessages");
              }



              if ((_emailreply == true) && (RESPONSE != "") && (VIEW_ONLY_MODE == false))
              {
                  languagetext = lng.TranslateString("Sending email to {0}", 7, s_receiver);
                  LogDebug(languagetext, (int)LogSetting.DEBUG);
                  labelmessage(languagetext, PipeCommands.StartEpg);

                  // build the email message

                  /*
                  LogDebug("_TextBoxSmtpServer " + _TextBoxSmtpServer, (int)LogSetting.DEBUG);
                  LogDebug("_TextBoxUserName " + _TextBoxUserName, (int)LogSetting.DEBUG);
                  LogDebug("_TextBoxPassword " + _TextBoxPassword, (int)LogSetting.DEBUG);
                  LogDebug("_numericUpDownSmtpPort " + _numericUpDownSmtpPort.ToString(), (int)LogSetting.DEBUG);
                  LogDebug("_checkBoxSSL " + _checkBoxSSL.ToString(), (int)LogSetting.DEBUG);*/

                  if (_TextBoxSmtpServer == "")
                  {
                      LogDebug("Error: No Smtp Server defined - check and test configuration in TV Server Configuration", (int)LogSetting.ERROR);
                      languagetext = lng.TranslateString("Error: No Smtp Server defined", 8);
                      labelmessage(languagetext, PipeCommands.Error);
                      SearchEpgExit(); //includes setting BUSY = false;
                      return false;
                  }

                  /*
                  if ((_TextBoxUserName == "") && (_useCredentials))
                  {
                      LogDebug("Error: No user name defined - check and test configuration in TV Server Configuration", (int)LogSetting.ERROR);
                      
                      languagetext = lng.TranslateString("Error: No user name defined", 9);
                      labelmessage(languagetext, PipeCommands.Error);
                      SearchEpgExit(); //includes setting BUSY = false;
                      return false;
                  }

                  if ((_TextBoxPassword == "")&& (_useCredentials))
                  {
                      LogDebug("Error: No password defined - check and test configuration in TV Server Configuration", (int)LogSetting.ERROR);
                      
                      languagetext = lng.TranslateString("Error: No password defined", 10);
                      labelmessage(languagetext, PipeCommands.Error);
                      SearchEpgExit(); //includes setting BUSY = false;
                      return false;
                  }*/


                  if (_TextBoxSmtpEmailAddress == "")
                  {
                      _TextBoxSmtpEmailAddress = _TextBoxUserName;
                  }
                  if (s_receiver == "")
                  {
                      LogDebug("Error: No receiver emailaddress defined - check and test configuration in TV Server Configuration", (int)LogSetting.ERROR);
                      languagetext = lng.TranslateString("Error: No receiver emailaddress defined", 11);
                      labelmessage(languagetext, PipeCommands.Error);
                      SearchEpgExit(); //includes setting BUSY = false;
                      return false;
                  }

                  //load last settings and store it in providerstring [0]
                  setting = layer.GetSetting("TvWishList_Providers_0", "_Last Setting;;0;False;False;;0;False");
                  string[] array = setting.Value.Split(";".ToCharArray());
                  if (array.Length != 4)
                  {
                      LogDebug("TvWishList Error: Invalid provider string: " + setting.Value + "\n Count is " + array.Length, (int)LogSetting.ERROR);
                      languagetext = lng.TranslateString("Error: Invalid provider settings", 12);
                      labelmessage(languagetext, PipeCommands.Error);
                      SearchEpgExit(); //includes setting BUSY = false;
                      return false;
                  }

                  string ServerAddress = array[1];

                  if (ServerAddress == "")
                  {
                      LogDebug("Server address not specified - aborting email check", (int)LogSetting.ERROR);
                      languagetext = lng.TranslateString("Error: Server address not specified", 13);
                      labelmessage(languagetext, PipeCommands.Error);
                      SearchEpgExit(); //includes setting BUSY = false;
                      return false;
                  }

                  //wait for internet connection
                  bool InternetConnected = false;
                  for (int i = 1; i < 30; i++) //wait up to 300s and check every 10s
                  {
                      System.Threading.Thread.Sleep(10000); //sleep 10s to wait for internet connection after standby
                      //check for existing ip address

                      try
                      {
                          IPHostEntry hostIP = Dns.GetHostEntry(ServerAddress);
                          IPAddress[] addr = hostIP.AddressList;
                          LogDebug("POP3 Server exists", (int)LogSetting.DEBUG);
                          InternetConnected = true;
                          break;
                      }
                      catch
                      {//continue loop
                          LogDebug("Waiting for internet connection in iteration " + i.ToString(), (int)LogSetting.DEBUG);
                      }


                  }
                  if (InternetConnected == false)
                  {
                      LogDebug("Failed to get internet connection", (int)LogSetting.DEBUG);
                      languagetext = lng.TranslateString("Error: Failed to get internet connection to POP3 server", 14);
                      labelmessage(languagetext, PipeCommands.Error);
                      SearchEpgExit(); //includes setting BUSY = false;
                      return false;
                  }


                  int smtpport = 0;
                  try
                  {
                      smtpport = Convert.ToInt32(_numericUpDownSmtpPort);
                  }
                  catch
                  {
                      smtpport = 0;
                  }

                  if (smtpport == 0)
                  {
                      LogDebug("Error: No smtp port defined - check and test configuration in TV Server Configuration", (int)LogSetting.ERROR);
                      languagetext = lng.TranslateString("Error: No smtp port defined", 15);
                      labelmessage(languagetext, PipeCommands.Error);
                      SearchEpgExit(); //includes setting BUSY = false;
                      return false;
                  }

                  LogDebug("_TextBoxSmtpServer:" + _TextBoxSmtpServer, (int)LogSetting.DEBUG);
                  LogDebug("smtpport:" + smtpport, (int)LogSetting.DEBUG);
                  LogDebug("_checkBoxSSL:" + _checkBoxSSL.ToString(), (int)LogSetting.DEBUG);
                  LogDebug("_TextBoxUserName:" + _TextBoxUserName, (int)LogSetting.DEBUG);
                  LogDebug("_TextBoxSmtpEmailAddress:" + _TextBoxSmtpEmailAddress, (int)LogSetting.DEBUG);


                  SendTvServerEmail sendobject = new SendTvServerEmail(_TextBoxSmtpServer, smtpport, _checkBoxSSL, _TextBoxUserName, _TextBoxPassword, _TextBoxSmtpEmailAddress);
                  sendobject.Debug = DEBUG;
                  sendobject.HtmlFormat = HtmlFormat;



                  LogDebug("Send reply mail to " + s_receiver + " at " + DateTime.Now.ToString(), (int)LogSetting.DEBUG);
                  LogDebug("Subject:" + s_subject, (int)LogSetting.DEBUG);
                  LogDebug(RESPONSE, (int)LogSetting.DEBUG);
                  LogDebug("End of mail", (int)LogSetting.DEBUG);

                  bool ok = sendobject.SendNewEmail(s_receiver, s_subject, RESPONSE);

                  if (ok == true)
                  {
                      LogDebug("Sending return emails completed", (int)LogSetting.DEBUG);
                      //System.Threading.Thread.Sleep(2000); //wait 2s
                      languagetext = lng.TranslateString("Sending return emails completed", 16);
                      labelmessage(languagetext, PipeCommands.StartEpg);
                  }
                  else
                  {
                      if (_TextBoxUserName == string.Empty)
                      {
                          LogDebug("Sending return emails failed - check the username", (int)LogSetting.ERROR);
                          languagetext = lng.TranslateString("Error: Sending return emails failed - check the username", 17);
                          labelmessage(languagetext, PipeCommands.Error);
                      }
                      else if (_TextBoxPassword == string.Empty)
                      {
                          LogDebug("Sending return emails failed - check the password", (int)LogSetting.ERROR);
                          languagetext = lng.TranslateString("Error: Sending return emails failed - check the password", 18);
                          labelmessage(languagetext, PipeCommands.Error);
                      }
                      else
                      {
                          LogDebug("Sending return emails failed", (int)LogSetting.ERROR);
                          languagetext = lng.TranslateString("Error: Sending return emails failed", 19);
                          labelmessage(languagetext, PipeCommands.Error);
                      }
                      SearchEpgExit(); //includes setting BUSY = false;
                      return false;

                  }



              }

              //System.Threading.Thread.Sleep(2000); //wait 2s
              languagetext = lng.TranslateString("Ready", 20);
              labelmessage(languagetext, PipeCommands.Ready);
              Log.Info("SearchEPG ended successfully");


          }
          catch (Exception exc)
          {
              Log.Error("Error:Exception in SearchEPG: "+exc.Message);
              string languagetext = lng.TranslateString("Fatal error - check the log file",24 );
              labelmessage(languagetext, PipeCommands.Error);
              SearchEpgExit(); //includes setting BUSY = false;
              return false;
          }

          SearchEpgExit(); //includes setting BUSY = false;

          
          return true;
      }


      public bool BoolConversion(string value, bool defaultvalue)
      {
          try
          {
              bool result = Convert.ToBoolean(value);
              return result;
          }
          catch
          {
              return defaultvalue;
          }


      }


      public void SearchEpgExit()
      {
          //separate log file for each epg processing for foxbenw issue
          
          if (File.Exists(TV_USER_FOLDER + @"\TvWishList\SplitLog.txt"))
          {
              Log.Debug("DEBUG: SplitLog file=" + TV_USER_FOLDER + @"\TvWishList\SplitLog.txt");
              try
              {
                  File.Move(TV_USER_FOLDER + @"\log\TvWishList.log", TV_USER_FOLDER + @"\log\TvWishList" + DateTime.Now.ToString("yyyy_MM_dd___HH_mm_ss") + ".log");
              }
              catch (Exception exc)
              {
                  Log.Error("Error:Exception in moving log file: " + exc.Message);
              }
          }

          //unlock TvWishList
          myTvWishes.UnLockTvWishList();
      }



      //-------------------------------------------------------------------------------------------------------------        
      // Search epg data and find all matching programs 
      //-------------------------------------------------------------------------------------------------------------  
      // matching programs are searched based on available information for program name, start time, endtime, displayname or groupname
      // if dispalyname is defined, groupnames will be ignored
      // partial name = true will handle partial name matches from program name
      // the routine will return an IList<Program> matchingprograms which contains all EPG programs of Type Program
      // the routine will not do any checking for conflicts of the list
      //-------------------------------------------------------------------------------------------------------------                              
      public bool SqlQueryPrograms(ref TvWish mywish, int counter)
      {

          LogDebug("TvWishList: Autocomplete search for matching programs", (int)LogSetting.DEBUG);
          

          if (mywish.b_active == false)
          {
              Log.Debug("Tvwish is inactive - returning");
              return true;
          }

          string SearchIn = mywish.s_SearchIn.ToLower();
          String Expression = String.Empty;
          IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
          IList<Program> myprogramlist = null;

          /*  search names are case invariant (title SOKO equal to title soko)
                   * 
                   * LIKE
                   * NOT LIKE
                   % 	A substitute for zero or more characters
                   _ 	A substitute for exactly one character
                   [charlist] 	Any single character in charlist
                   [^charlist] or [!charlist] Any single character not in charlist
                   * 
                   * 
                   * 
                   * SELECT column_name(s)
                     FROM table_name
                     WHERE column_name operator value
                   
                   Operators Allowed in the WHERE Clause

                    With the WHERE clause, the following operators can be used:
                    Operator 	Description
                    = 	Equal
                    <> 	Not equal
                    > 	Greater than
                    < 	Less than
                    >= 	Greater than or equal
                    <= 	Less than or equal
                    BETWEEN 	Between an inclusive range
                    LIKE 	Search for a pattern
                    IN 	If you know the exact value you want to return for at least one of the columns
                   * 
                   * 
                   * Combine with brackets ()
                   * and with AND or OR

                    Note: In some versions of SQL the <> operator may be written as !=
           * 
           * 
           *       Program columns:
           *       title
           *       description
           *       classification
           *       
                   
         string Title 
         string Classification 
         string Description 
         DateTime StartTime 
         DateTime EndTime
         DateTime OriginalAirDate
         string EpisodeName 
         string EpisodeNum 
         string EpisodeNumber 
         string EpisodePart 
         string Genre 
         int ParentalRating 
         string SeriesNum 
         int StarRating
         int IdChannel 
         int IdProgram        
         
          
          
        
        */
          string EscapeName = EscapeSQLString(mywish.searchfor);
          if (SearchIn == "title") //no translation needed due to separation of original string
          {
              Expression += String.Format("(EndTime >= '{0}') AND ", DateTime.Now.ToString(GetDateTimeString(), mmddFormat));  //only programs after now
              if (mywish.b_partialname == true)  //partial title
              {
                  Expression += String.Format("( title like '%{0}%' ) ", EscapeName);                
              }
              else                      //exact title
              {
                  Expression += String.Format("( title = '{0}' ) ", EscapeName);
              }              
          }//end title
          else if (SearchIn == "text")  //search in description  //no translation needed due to separation of original string
          {
              Expression += String.Format("(EndTime >= '{0}') AND ", DateTime.Now.ToString(GetDateTimeString(), mmddFormat));  //only programs after now
              Expression += String.Format("( description like '%{0}%' ) ", EscapeName);
          }//end description
          else if (SearchIn == "both") //no translation needed due to separation of original string
          {
              Expression += String.Format("(EndTime >= '{0}') AND (", DateTime.Now.ToString(GetDateTimeString(), mmddFormat));  //only programs after now

              if (mywish.b_partialname == true)  //partial title
              {
                  Expression += String.Format("( title like '%{0}%' ) OR ", EscapeName);
              }
              else                      //exact title
              {
                  Expression += String.Format("( title = '{0}' ) OR ", EscapeName);
              }

              Expression += String.Format("( description like '%{0}%' ) )", EscapeName);

          }//end both

          if (SearchIn == "expression") //no translation needed due to separation of original string
          {
              Expression = mywish.searchfor;

              //split in schedule and recording expression
              Expression = Expression.Replace("<BR>", "\n");
              Expression = Expression.Replace("<br>", "\n");
              Expression = Expression.Replace(@"\n", "\n");
              Expression = Expression.Replace(@"\'", "'");  //needed for expressions
              string[] expArray = Expression.Split('\n');
              if (expArray.Length < 1)
              {
                  Log.Debug("No expression defined for searching schedules on tvwish " + mywish.name);
                  return true;
              }
              else
              {
                  Expression = expArray[0];
                  Log.Debug("Expression=" + Expression);
              }
          }

          //SQL Query
          LogDebug("SQL query for: " + Expression, (int)LogSetting.DEBUG);
          try
          {
              StringBuilder SqlSelectCommand = new StringBuilder();
              SqlSelectCommand.Append("select * from Program ");
              SqlSelectCommand.AppendFormat("where {0}", Expression);  //!!!!!!!!!!!!!!!!!!!!!!EscapeSQLString cannot be checked for expression
              SqlSelectCommand.Append(" order by StartTime");
              SqlStatement stmt = new SqlBuilder(StatementType.Select, typeof(Program)).GetStatement(true);
              SqlStatement ManualJoinSQL = new SqlStatement(StatementType.Select, stmt.Command, SqlSelectCommand.ToString(),typeof(Program));
              myprogramlist = ObjectFactory.GetCollection<Program>(ManualJoinSQL.Execute());
          }
          catch (Exception exc)
          {
              LogDebug("Error in SQL query for searching " + Expression, (int)LogSetting.ERROR);
              LogDebug("Exception message was " + exc.Message, (int)LogSetting.ERROR);

              string languagetext = lng.TranslateString("Error in SQL query - check the expression {0}", 25, Expression);
              labelmessage(languagetext, PipeCommands.StartEpg); //do not mark as an error as other commands are still coming - just delay message
              Thread.Sleep(ErrorWaitTime);
              return false;
          }
          // End of SQL Query

          Log.Debug(myprogramlist.Count.ToString()+" programs found");
         
          foreach (Program myprogram in myprogramlist)
          {
              Program oneprogram = myprogram;

              LogDebug("\n************************************************************", (int)LogSetting.DEBUG);
              LogDebug("****Next program in autocompletion:", (int)LogSetting.DEBUG);
              outputprogramresponse(myprogram, (int)LogSetting.DEBUG); //Debug only
              
              // process groupname
              if (mywish.group != lng.TranslateString("All Channels",4104))
              {
                  if ((IsChannelInGroup(oneprogram.IdChannel, mywish.group) == false) && (IsRadioChannelInGroup(oneprogram.IdChannel, mywish.group) == false))
                  {
                      LogDebug("!!!!!!!Groupname " + mywish.group + " not matching for title " + oneprogram.Title + " at " + oneprogram.StartTime.ToString(), (int)LogSetting.DEBUG);
                      continue;
                      
                  }
              }

              //process exclude
              if (mywish.exclude != string.Empty)
              {
                  string Exclude = mywish.exclude.ToUpper();
                  String Exclude1 = "";
                  String Exclude2 = "";
                  Exclude = Exclude.Replace("&&", "\n");
                  String[] Tokens = Exclude.Split('\n');
                  if (Tokens.Length >= 2)
                  {
                      Exclude2 = Tokens[1];
                  }

                  if (Tokens.Length >= 1)
                  {
                      Exclude1 = Tokens[0];
                  }
                  LogDebug("TvWishList: Exclude1=" + Exclude1, (int)LogSetting.DEBUG);
                  LogDebug("TvWishList: Exclude2=" + Exclude2, (int)LogSetting.DEBUG);

                  Boolean foundflag = false;

                  if ((Exclude1.StartsWith("*") == true) && (Exclude1.StartsWith("**") == false)) //exclude in description
                  {
                      string Exclude_mod = Exclude1.Substring(1, Exclude.Length - 1);
                      //LogDebug("Exclude_mod= " + Exclude_mod, (int)LogSetting.DEBUG);
                      if ((oneprogram.Description.ToUpper().Contains(Exclude_mod) == false) || (Exclude_mod == ""))
                      {
                          if ((oneprogram.Description.ToUpper().Contains(Exclude2) == false) || (Exclude2 == ""))
                          {
                              foundflag = true;
                              LogDebug("No Exclude of program for " + oneprogram.Title + " at " + oneprogram.StartTime.ToString(), (int)LogSetting.DEBUG);
                          }
                      }
                  }
                  else if (Exclude1.StartsWith("**") == true) //exclude in title or description
                  {
                      string Exclude_mod = Exclude1.Substring(2, Exclude.Length - 2);
                      //LogDebug("Exclude_mod= " + Exclude_mod, (int)LogSetting.DEBUG);
                      if (((oneprogram.Title.ToUpper().Contains(Exclude_mod) == false) && (oneprogram.Description.ToUpper().Contains(Exclude_mod) == false)) || (Exclude_mod == ""))
                      {
                          if (((oneprogram.Title.ToUpper().Contains(Exclude2) == false) && (oneprogram.Description.ToUpper().Contains(Exclude2) == false)) || (Exclude2 == ""))
                          {
                              foundflag = true;
                              LogDebug("No Exclude of program for title " + oneprogram.Title + " at " + oneprogram.StartTime.ToString(), (int)LogSetting.DEBUG);
                          }
                      }
                  }
                  else if ((oneprogram.Title.ToUpper().Contains(Exclude1) == false) || (Exclude1 == "")) //exclude only in title
                  {
                      if ((oneprogram.Title.ToUpper().Contains(Exclude2) == false) || (Exclude2 == ""))
                      {
                          foundflag = true;
                          LogDebug("No Exclude of program for title " + oneprogram.Title + " at " + oneprogram.StartTime.ToString(), (int)LogSetting.DEBUG);
                      }
                  }

                  if (foundflag == false)
                  {                      
                      LogDebug("", (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      message = lng.TranslateString("Excluding program {0} due to exclude filter condition {1}",55,oneprogram.Title,mywish.exclude);
                      LogDebug(message, (int)LogSetting.INFO);
                      LogDebug("Start=" + oneprogram.StartTime.ToString(), (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);

                      mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);
                      continue;
                  }

              }//end exclude

              // process valid word check
              if (mywish.b_wordmatch == true)
              {
                  if (SearchIn == "title") //no translation needed due to separation of original string
                  {
                      if (mywish.b_partialname == true)  //partial title
                      {
                          if (validwordcheck(oneprogram.Title,mywish.searchfor) == false)
                          {
                              LogDebug("", (int)LogSetting.INFO);
                              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                              message = lng.TranslateString("Excluding program {0} due to wordcheck filter condition",56,oneprogram.Title);
                              LogDebug(message, (int)LogSetting.INFO);
                              LogDebug("Start=" + oneprogram.StartTime.ToString(), (int)LogSetting.INFO);
                              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);

                              mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);
                              continue;
                          }
                      }
                  
                  }
                  else if (SearchIn == "text")  //search in description  //no translation needed due to separation of original string
                  {
                      if (validwordcheck(oneprogram.Description, mywish.searchfor) == false)
                      {
                          LogDebug("", (int)LogSetting.INFO);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                          message = lng.TranslateString("Excluding program {0} due to wordcheck filter condition ", 57, oneprogram.Title);
                          LogDebug(message, (int)LogSetting.INFO);
                          LogDebug("Start=" + oneprogram.StartTime.ToString(), (int)LogSetting.INFO);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);

                          mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);
                          continue;
                      }
                  }
                  else if (SearchIn == "both")  //search in either title or description  //no translation needed due to separation of original string
                  {
                      if ((validwordcheck(oneprogram.Title, mywish.searchfor) == false) && (validwordcheck(oneprogram.Description, mywish.searchfor) == false))
                      {
                          LogDebug("", (int)LogSetting.INFO);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                          message = lng.TranslateString("Excluding program {0} due to wordcheck filter condition ", 57, oneprogram.Title);
                          LogDebug(message, (int)LogSetting.INFO);
                          LogDebug("Start=" + oneprogram.StartTime.ToString(), (int)LogSetting.INFO);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);

                          mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);
                          continue;
                      }
                  }

              } //end processing valid wordcheck


                
                  string channelname = "Error_with_ID_" + oneprogram.IdChannel.ToString();
                  try
                  {
                      channelname = Channel.Retrieve(oneprogram.IdChannel).DisplayName;
                  }
                  catch//do nothing
                  {

                  }
                  string languagetext = lng.TranslateString("Processing EPG data {0} on {1} at {2}", 21, oneprogram.Title, channelname, oneprogram.StartTime.ToString());
                  labelmessage(languagetext, PipeCommands.StartEpg);
                      

                  if (mywish.i_scheduled >= MAXFOUND)
                  {
                      LogDebug("", (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      message = lng.TranslateString("Maximum number ({0}) of programs found - will ignore additional matches",58, MAXFOUND.ToString());
                      LogDebug(message, (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);

                      mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.MAXFOUND_EXCEEDED, mywish.tvwishid, string.Empty);

                      return true;
                  }

                  bool ok = addsingleschedule(oneprogram, ref mywish, counter);

                  //post processing and updating of Tv wish data by locking to existing found schedule
                  if (ok == true)
                  {
                      try  //update lock filters into tv server
                      {

                          if (mywish.recordtype == lng.TranslateString("Only Once",2650)) // stop after first successful program
                          {
                              LogDebug("Program " + mywish.name + " deactivated", (int)LogSetting.DEBUG);
                              mywish.b_active = false;
                              mywish.active = "False";
                              break; // stop after first successful program
                          }

                          //include all language strings with recordtype Channel
                          if (((mywish.recordtype == lng.Get(2652)) || (mywish.recordtype== lng.Get(2653)) || (mywish.recordtype== lng.Get(2654))|| (mywish.recordtype== lng.Get(2655)))&& (mywish.channel == lng.TranslateString("Any",4100)))
                          {
                              mywish.channel = channelname;
                              Log.Debug("Locked to channelfilter=" + channelname);
                          }

                          //include all language strings with recordtype Time
                          if (((mywish.recordtype == lng.Get(2654)) ||  (mywish.recordtype == lng.Get(2655)))&& (mywish.i_aftertime == -1) && (mywish.i_beforetime == -1))
                          {
                              mywish.i_aftertime = oneprogram.StartTime.Hour * 60 + oneprogram.StartTime.Minute;
                              mywish.aftertime = oneprogram.StartTime.Hour.ToString("D2") + ":" + oneprogram.StartTime.Minute.ToString("D2");
                              Log.Debug("Locked to i_aftertime=" + mywish.i_aftertime.ToString());

                              mywish.i_beforetime = oneprogram.EndTime.Hour * 60 + oneprogram.EndTime.Minute;
                              mywish.beforetime = oneprogram.EndTime.Hour.ToString("D2") + ":" + oneprogram.EndTime.Minute.ToString("D2");
                              Log.Debug("Locked to i_beforetime=" + mywish.i_beforetime.ToString());

                          }

                          //include all language strings with recordtype Day
                          if (((mywish.recordtype == lng.Get(2653)) ||  (mywish.recordtype == lng.Get(2655))) && (mywish.i_afterdays == -1) && (mywish.i_beforedays == -1))
                          {
                              mywish.i_afterdays = (int)oneprogram.StartTime.DayOfWeek;

                              if (mywish.i_afterdays == (int)DayOfWeek.Sunday)
                                  mywish.afterdays = "Sunday";
                              else if (mywish.i_afterdays == (int)DayOfWeek.Monday)
                                  mywish.afterdays = "Monday";
                              else if (mywish.i_afterdays == (int)DayOfWeek.Tuesday)
                                  mywish.afterdays = "Tuesday";
                              else if (mywish.i_afterdays == (int)DayOfWeek.Wednesday)
                                  mywish.afterdays = "Wednesday";
                              else if (mywish.i_afterdays == (int)DayOfWeek.Thursday)
                                  mywish.afterdays = "Thursday";
                              else if (mywish.i_afterdays == (int)DayOfWeek.Friday)
                                  mywish.afterdays = "Friday";
                              else if (mywish.i_afterdays == (int)DayOfWeek.Saturday)
                                  mywish.afterdays = "Saturday";

                              mywish.i_beforedays = (int)oneprogram.StartTime.DayOfWeek;

                              if (mywish.i_beforedays == (int)DayOfWeek.Sunday)
                                  mywish.beforedays = "Sunday";
                              else if (mywish.i_beforedays == (int)DayOfWeek.Monday)
                                  mywish.beforedays = "Monday";
                              else if (mywish.i_beforedays == (int)DayOfWeek.Tuesday)
                                  mywish.beforedays = "Tuesday";
                              else if (mywish.i_beforedays == (int)DayOfWeek.Wednesday)
                                  mywish.beforedays = "Wednesday";
                              else if (mywish.i_beforedays == (int)DayOfWeek.Thursday)
                                  mywish.beforedays = "Thursday";
                              else if (mywish.i_beforedays == (int)DayOfWeek.Friday)
                                  mywish.beforedays = "Friday";
                              else if (mywish.i_beforedays == (int)DayOfWeek.Saturday)
                                  mywish.beforedays = "Saturday";

                              Log.Debug("Locked to day=" + mywish.i_beforedays.ToString());
                          }
                      }
                      catch (Exception exc)
                      {
                          Log.Error("Error in updating lock filter - ex ception:" + exc.Message);
                          languagetext = lng.TranslateString("Fatal error - check the log file", 24);
                          labelmessage(languagetext, PipeCommands.StartEpg); //do not stop - do not flag as error
                          Thread.Sleep(ErrorWaitTime);
                      }
                  }  //end locking condition

              }//end search episode n
          //}

          LogDebug("TvWishList: Autocomplete finished", (int)LogSetting.DEBUG);
          return true;
      }


      //-------------------------------------------------------------------------------------------------------------        
      // Search epg data and find all matching programs 
      //-------------------------------------------------------------------------------------------------------------  
      // matching programs are searched based on available information for program name, start time, endtime, displayname or groupname
      // if dispalyname is defined, groupnames will be ignored
      // partial name = true will handle partial name matches from program name
      // the routine will return an IList<Program> matchingprograms which contains all EPG programs of Type Program
      // the routine will not do any checking for conflicts of the list
      //-------------------------------------------------------------------------------------------------------------                              
      public bool SqlQueryRecordings(TvWish mywish, int counter)
      {

          LogDebug("TvWishList: Autocomplete search for matching recordings", (int)LogSetting.DEBUG);
          
          if (mywish.b_active == false)
          {
              Log.Debug("Tvwish is inactive - returning");
              return true;
          }


          string SearchIn = mywish.s_SearchIn.ToLower();
          String Expression = String.Empty;
          IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
          IList<Recording> myRecordingList = null;

          /*  search names are case invariant (title SOKO equal to title soko)
                   * 
                   * LIKE
                   * NOT LIKE
                   % 	A substitute for zero or more characters
                   _ 	A substitute for exactly one character
                   [charlist] 	Any single character in charlist
                   [^charlist] or [!charlist] Any single character not in charlist
                   * 
                   * 
                   * 
                   * SELECT column_name(s)
                     FROM table_name
                     WHERE column_name operator value
                   
                   Operators Allowed in the WHERE Clause

                    With the WHERE clause, the following operators can be used:
                    Operator 	Description
                    = 	Equal
                    <> 	Not equal
                    > 	Greater than
                    < 	Less than
                    >= 	Greater than or equal
                    <= 	Less than or equal
                    BETWEEN 	Between an inclusive range
                    LIKE 	Search for a pattern
                    IN 	If you know the exact value you want to return for at least one of the columns
                   * 
                   * 
                   * Combine with brackets ()
                   * and with AND or OR

                    Note: In some versions of SQL the <> operator may be written as !=
           * 
           * 
           *       Recording columns(use lower case in sql quesry):
        public string Description { get; set; }
        public DateTime EndTime { get; set; }
        public string EpisodeName { get; set; }
        public string EpisodeNum { get; set; }
        public string EpisodeNumber { get; }
        public string EpisodePart { get; set; }
        public string FileName { get; set; }
        public string Genre { get; set; }
        public int IdChannel { get; set; }
        public int IdRecording { get; }
        public int Idschedule { get; set; }
        public int IdServer { get; set; }
        public bool IsChanged { get; }
        public bool IsManual { get; }
        public bool IsRecording { get; set; }
        public int KeepUntil { get; set; }
        public DateTime KeepUntilDate { get; set; }
        public string SeriesNum { get; set; }
        public bool ShouldBeDeleted { get; }
        public DateTime StartTime { get; set; }
        public int StopTime { get; set; }
        public int TimesWatched { get; set; }
        public string Title { get; set; }
         
          
          
        
        */
          string EscapeName = EscapeSQLString(mywish.searchfor);
          if (SearchIn == "title") //no translation needed due to separation of original string
          {
              //Expression += String.Format("(EndTime >= '{0}') AND ", DateTime.Now.ToString(GetDateTimeString(), mmddFormat));  //only programs after now
              if (mywish.b_partialname == true)  //partial title
              {
                  Expression += String.Format("( title like '%{0}%' ) ", EscapeName);
              }
              else                      //exact title
              {
                  Expression += String.Format("( title = '{0}' ) ", EscapeName);
              }
          }//end title
          else if (SearchIn == "text")  //search in description  //no translation needed due to separation of original string
          {
              //Expression += String.Format("(EndTime >= '{0}') AND ", DateTime.Now.ToString(GetDateTimeString(), mmddFormat));  //only programs after now
              Expression += String.Format("( description like '%{0}%' ) ", EscapeName);
          }//end description
          else if (SearchIn == "both") //no translation needed due to separation of original string
          {
              //Expression += String.Format("(EndTime >= '{0}') AND (", DateTime.Now.ToString(GetDateTimeString(), mmddFormat));  //only programs after now

              if (mywish.b_partialname == true)  //partial title
              {
                  Expression += String.Format("( title like '%{0}%' ) OR ", EscapeName);
              }
              else                      //exact title
              {
                  Expression += String.Format("( title = '{0}' ) OR ", EscapeName);
              }

              Expression += String.Format("( description like '%{0}%' )", EscapeName);

          }//end both

          if (SearchIn == "expression")  //no translation needed due to separation of original string
          {
              Expression = mywish.searchfor;

              //split in schedule and recording expression
              Expression = Expression.Replace("<BR>", "\n");
              Expression = Expression.Replace("<br>", "\n");
              Expression = Expression.Replace(@"\n", "\n");
              Expression = Expression.Replace(@"\'", "'");  //needed for expressions
              string[] expArray = Expression.Split('\n');
              if (expArray.Length != 2)
              {
                  Log.Debug("No expression defined for searching recordings on tvwish "+mywish.name);
                  return true;
              }
              else
              {                
                  Expression = expArray[1];
                  
              }
             
              Log.Debug("Expression=" + Expression);
          }

          //SQL Query
          LogDebug("SQL query for: " + Expression, (int)LogSetting.DEBUG);
          try
          {
              StringBuilder SqlSelectCommand = new StringBuilder();
              SqlSelectCommand.Append("select * from Recording ");
              SqlSelectCommand.AppendFormat(string.Format("where {0}", Expression));  //!!!!!!!!!!!!!!!!!!EscapeSQLString cannot bechecked for expression
              SqlStatement stmt = new SqlBuilder(StatementType.Select, typeof(Recording)).GetStatement(true);
              SqlStatement ManualJoinSQL = new SqlStatement(StatementType.Select, stmt.Command, SqlSelectCommand.ToString(), typeof(Recording));
              myRecordingList = ObjectFactory.GetCollection<Recording>(ManualJoinSQL.Execute());
              Log.Debug("SQL Result:myRecordingList.Count" + myRecordingList.Count.ToString());
          }
          catch (Exception exc)
          {
              LogDebug("Error in SQL query for searching " + Expression, (int)LogSetting.ERROR);
              LogDebug("Exception message was " + exc.Message, (int)LogSetting.ERROR);

              string languagetext = lng.TranslateString("Error in SQL query - check the expression {0}", 25, Expression);
              labelmessage(languagetext, PipeCommands.StartEpg); //do not mark as an error as other commands are still coming - just delay message
              Thread.Sleep(ErrorWaitTime);
              return false;
          }
          // End of SQL Query
          Log.Debug(myRecordingList.Count.ToString() + " recordings found after SQL query");

          foreach (Recording myrecording in myRecordingList)
          {
              
              LogDebug("****Next program in autocompletion:", (int)LogSetting.DEBUG);
              //outputrecordingresponse(myrecording, (int)LogSetting.DEBUG); //Debug only

              /*   // process groupname  BUG: not needed in recordings
              if (mywish.group != lng.TranslateString("All Channels", 4104))
              {
                  if ((IsChannelInGroup(myrecording.IdChannel, mywish.group) == false) && (IsRadioChannelInGroup(myrecording.IdChannel, mywish.group) == false))
                  {
                      LogDebug("!!!!!!!Groupname " + mywish.group + " not matching for title " + myrecording.Title + " at " + myrecording.StartTime.ToString(), (int)LogSetting.DEBUG);
                      continue;

                  }
              }*/

              //process exclude
              if (mywish.exclude != string.Empty)
              {
                  string Exclude = mywish.exclude.ToUpper();
                  String Exclude1 = "";
                  String Exclude2 = "";
                  Exclude = Exclude.Replace("&&", "\n");
                  String[] Tokens = Exclude.Split('\n');
                  if (Tokens.Length >= 2)
                  {
                      Exclude2 = Tokens[1];
                  }

                  if (Tokens.Length >= 1)
                  {
                      Exclude1 = Tokens[0];
                  }
                  LogDebug("TvWishList: Exclude1=" + Exclude1, (int)LogSetting.DEBUG);
                  LogDebug("TvWishList: Exclude2=" + Exclude2, (int)LogSetting.DEBUG);

                  Boolean foundflag = false;

                  if ((Exclude1.StartsWith("*") == true) && (Exclude1.StartsWith("**") == false)) //exclude in description
                  {
                      string Exclude_mod = Exclude1.Substring(1, Exclude.Length - 1);
                      //LogDebug("Exclude_mod= " + Exclude_mod, (int)LogSetting.DEBUG);
                      if ((myrecording.Description.ToUpper().Contains(Exclude_mod) == false) || (Exclude_mod == ""))
                      {
                          if ((myrecording.Description.ToUpper().Contains(Exclude2) == false) || (Exclude2 == ""))
                          {
                              foundflag = true;
                              LogDebug("No Exclude of program for " + myrecording.Title + " at " + myrecording.StartTime.ToString(), (int)LogSetting.DEBUG);
                          }
                      }
                  }
                  else if (Exclude1.StartsWith("**") == true) //exclude in title or description
                  {
                      string Exclude_mod = Exclude1.Substring(2, Exclude.Length - 2);
                      //LogDebug("Exclude_mod= " + Exclude_mod, (int)LogSetting.DEBUG);
                      if (((myrecording.Title.ToUpper().Contains(Exclude_mod) == false) && (myrecording.Description.ToUpper().Contains(Exclude_mod) == false)) || (Exclude_mod == ""))
                      {
                          if (((myrecording.Title.ToUpper().Contains(Exclude2) == false) && (myrecording.Description.ToUpper().Contains(Exclude2) == false)) || (Exclude2 == ""))
                          {
                              foundflag = true;
                              LogDebug("No Exclude of program for title " + myrecording.Title + " at " + myrecording.StartTime.ToString(), (int)LogSetting.DEBUG);
                          }
                      }
                  }
                  else if ((myrecording.Title.ToUpper().Contains(Exclude1) == false) || (Exclude1 == "")) //exclude only in title
                  {
                      if ((myrecording.Title.ToUpper().Contains(Exclude2) == false) || (Exclude2 == ""))
                      {
                          foundflag = true;
                          LogDebug("No Exclude of program for title " + myrecording.Title + " at " + myrecording.StartTime.ToString(), (int)LogSetting.DEBUG);
                      }
                  }

                  if (foundflag == false)
                  {
                      LogDebug("!!!!!!!exclude condition " + Exclude + " found for for title " + myrecording.Title + " at " + myrecording.StartTime.ToString(), (int)LogSetting.DEBUG);
                      continue;
                  }

              }//end exclude

              // process valid word check
              if (mywish.b_wordmatch == true)
              {
                  if (SearchIn == "title")  //no translation needed due to separation of original string
                  {
                      if (mywish.b_partialname == true)  //partial title
                      {
                          if (validwordcheck(myrecording.Title, mywish.searchfor) == false)
                          {
                              LogDebug("!!!!!!!wordcheck on title failed for title " + myrecording.Title + " at " + myrecording.StartTime.ToString(), (int)LogSetting.DEBUG);
                              continue;
                          }
                      }

                  }
                  else if (SearchIn == "text")  //search in description  //no translation needed due to separation of original string
                  {
                      if (validwordcheck(myrecording.Description, mywish.searchfor) == false)
                      {
                          LogDebug("!!!!!!!wordcheck on description failed for title " + myrecording.Title + " at " + myrecording.StartTime.ToString(), (int)LogSetting.DEBUG);
                          continue;
                      }
                  }
                  else if (SearchIn == "both")  //search in either title or description  //no translation needed due to separation of original string
                  {
                      if ((validwordcheck(myrecording.Title, mywish.searchfor) == false) && (validwordcheck(myrecording.Description, mywish.searchfor) == false))
                      {
                          LogDebug("!!!!!!!wordcheck on title and description failed for title " + myrecording.Title + " at " + myrecording.StartTime.ToString(), (int)LogSetting.DEBUG);
                          continue;
                      }
                  }

              } //end processing valid wordcheck


              
              //process episode name, part, number and series number
              
#if(TV100 || TV101)
                  if (1==1) //always true for 1.0.1
#elif(TV11 || TV12)
              if (((myrecording.EpisodeName.ToLower() == mywish.episodename.ToLower() == true) || (mywish.episodename == "")) && ((myrecording.EpisodePart.ToLower() == mywish.episodepart.ToLower() == true) || (mywish.episodepart == "")) && ((myrecording.SeriesNum.ToLower() == mywish.seriesnumber.ToLower()) || (mywish.seriesnumber == "")) && ((myrecording.EpisodeNum.ToLower() == mywish.episodenumber.ToLower()) || (mywish.episodenumber == "")))

                  //bug to fix with > number
#endif
              {
                  if ((myrecording.StartTime < myrecording.EndTime)&&(File.Exists(myrecording.FileName)))
                  {//during recording start time will be equal to end time and text all in capitol letters - in that case ignore and wait till recording is finished - schedule does exist in parallel during recording

                      //create new recording message
                      Log.Debug("myrecording.Title=" + myrecording.Title);
                      Log.Debug("myrecording.Description=" + myrecording.Description);
                      FileInfo myfileinfo = new FileInfo(myrecording.FileName);
                      //long filesize = myfileinfo.Length / (1024 * 1024);
                      //double gigabyte = Convert.ToDouble(filesize)/1024;
                      double filesize = Convert.ToDouble(myfileinfo.Length)/(1024*1024*1024);

                      string message =  lng.TranslateString("{0} with {1} GB",59,myrecording.FileName,filesize.ToString("F2"));
                      Log.Debug("recording message with filesize="+message);

                      //add new recorded message
                      mymessage.addmessage(myrecording, message, MessageType.Recorded, mywish.searchfor, (int)XmlMessages.MessageEvents.RECORDING_FOUND, mywish.tvwishid, myrecording.FileName);
                       


                      //remove deleted messages with same title/description/episode name/number
                      try
                      {
                          for (int i = mymessage.ListAllTvMessages().Count-1; i >=0 ; i--)
                          {
                              xmlmessage onemessage = mymessage.GetTvMessageAtIndex(i);
                              if ((onemessage.title == myrecording.Title) && (onemessage.description == myrecording.Description) && (onemessage.type == MessageType.Deleted.ToString()))
                              {
#if(TV100 || TV101)
                  if (1==1) //always true for 1.0.1
#elif(TV11 || TV12)
                                  if (((myrecording.EpisodeName.ToLower() == onemessage.EpisodeName.ToLower() == true) || (onemessage.EpisodeName == "")) && ((myrecording.EpisodePart.ToLower() == onemessage.EpisodePart.ToLower() == true) || (onemessage.EpisodePart == "")) && ((myrecording.SeriesNum.ToLower() == onemessage.SeriesNum.ToLower()) || (onemessage.SeriesNum == "")) && ((myrecording.EpisodeNum.ToLower() == onemessage.EpisodeNum.ToLower()) || (onemessage.EpisodeNum == "")))
#endif
                                  {
                                      //delete message
                                      Log.Debug("Deleting message " + onemessage.title+" at "+onemessage.start+" due to new recording");
                                      mymessage.DeleteTvMessageAt(i);             
                                  }
                              }
                          }
                      }
                      catch (Exception exc)
                      {
                          LogDebug("Error: searching deleted messages failed with exception " + exc.Message, (int)LogSetting.ERROR);
                          string languagetext2 = lng.TranslateString("Fatal error - check the log file", 24);
                          labelmessage(languagetext2, PipeCommands.StartEpg); //do not stop - do not flag as error
                          Thread.Sleep(ErrorWaitTime);
                          return false;
                      }

                              
                  }
                  
              }//end episode filter
              string languagetext = lng.TranslateString("Processing Recording {0} at {1}", 22, myrecording.Title, myrecording.StartTime.ToString());
              labelmessage(languagetext, PipeCommands.StartEpg);
          }//end all recordings
          

          LogDebug("TvWishList: Autocomplete finished", (int)LogSetting.DEBUG);
          return true;
      }



      public string GetDateTimeString()
      {
          string provider = ProviderFactory.GetDefaultProvider().Name.ToLowerInvariant();
          if (provider == "mysql")
          {
              return "yyyy-MM-dd HH:mm:ss";
          }
          return "yyyyMMdd HH:mm:ss";
      }

      public string EscapeSQLString(string original)
      {
          string provider = ProviderFactory.GetDefaultProvider().Name.ToLowerInvariant();
          if (provider == "mysql")
          {
              return original.Replace("'", "\\'");
          }
          else
          {
              return original.Replace("'", "''");
          }
      }

      // checks if the channel is within the group of groupname
      public bool IsChannelInGroup(int channelid, string groupname)
      {
          //LogDebug("IsChannelInGroup", (int)LogSetting.DEBUG);
          try
          {
              Channel mychannel = Channel.Retrieve(channelid);

#if(TV100)
                  IList maps = mychannel.ReferringGroupMap();
#elif(TV101)
                  IList<GroupMap> maps = mychannel.ReferringGroupMap();
#elif(TV11 || TV12)
              IList<GroupMap> maps = mychannel.ReferringGroupMap();
#endif

              //LogDebug("groupname=" + groupname.ToLower(), (int)LogSetting.DEBUG);
              //LogDebug("maps.count=" + maps.Count.ToString(), (int)LogSetting.DEBUG);
              foreach (GroupMap map in maps)
              {
                  
                  ChannelGroup channelgroupmap = map.ReferencedChannelGroup();
                  //LogDebug("groupmap: name=" + channelgroupmap.GroupName.ToLower(), (int)LogSetting.DEBUG);
                  if (channelgroupmap.GroupName.ToLower() == groupname.ToLower())
                  {
                      //LogDebug("return=true", (int)LogSetting.DEBUG);
                      return true;
                  }
              }
          }
          catch (Exception)
          {
              LogDebug("IsChannelInGroup for searching id=" + channelid + " could not be retrieved - groupname="+groupname, (int)LogSetting.DEBUG);
              //do not flag as severe error, as this can happen if recording channel names got changed and have a different id now
              //LogDebug("Exception message was " + exc.Message, (int)LogSetting.ERROR);
              //string languagetext = lng.TranslateString("Fatal error - check the log file", 24);
              //labelmessage(languagetext, PipeCommands.StartEpg); //do not stop - do not flag as error
              //Thread.Sleep(ErrorWaitTime);
          }
          //LogDebug("return=false", (int)LogSetting.DEBUG);
          return false;
      }

      // checks if the channel is within the group of radiogroupname
      public bool IsRadioChannelInGroup(int channelid, string radiogroupname)
      {
          //LogDebug("IsRadioChannelInGroup", (int)LogSetting.DEBUG);
          try
          {
              Channel mychannel = Channel.Retrieve(channelid);

#if(TV100)
                  IList radiomaps = mychannel.ReferringRadioGroupMap();
#elif(TV101)
                  IList<RadioGroupMap> radiomaps = mychannel.ReferringRadioGroupMap();
#elif(TV11 || TV12)
              IList<RadioGroupMap> radiomaps = mychannel.ReferringRadioGroupMap();
#endif
              //LogDebug("radiogroupname=" + radiogroupname.ToLower(), (int)LogSetting.DEBUG);
              //LogDebug("radiomaps.count=" + radiomaps.Count.ToString(), (int)LogSetting.DEBUG);
              foreach (RadioGroupMap radiomap in radiomaps)
              {
                  RadioChannelGroup radiochannelgroupmap = radiomap.ReferencedRadioChannelGroup();
                  //LogDebug("radiogroupmap: name=" + radiochannelgroupmap.GroupName.ToLower(), (int)LogSetting.DEBUG);
                  if (radiochannelgroupmap.GroupName.ToLower() == radiogroupname.ToLower())
                  {
                      //LogDebug("return=true", (int)LogSetting.DEBUG);
                      return true;
                  }
              }
          }
          catch (Exception)
          {
              LogDebug("IsRadioChannelInGroup for searching id=" + channelid + " could not be retrieved -  groupname=" + radiogroupname, (int)LogSetting.DEBUG);
              //do not flag as severe error, as this can happen if recording channel names got changed and have a different id now
              //LogDebug("Exception message was " + exc.Message, (int)LogSetting.ERROR);
              //string languagetext = lng.TranslateString("Fatal error - check the log file", 24);
              //labelmessage(languagetext, PipeCommands.StartEpg); //do not stop - do not flag as error
              //Thread.Sleep(ErrorWaitTime);
          }
          //LogDebug("return=false", (int)LogSetting.DEBUG);
          return false;
      }


      
      

      //-------------------------------------------------------------------------------------------------------------
      //checks if the string search is a complete word within text
      //-------------------------------------------------------------------------------------------------------------
      public bool validwordcheck(string text,string search)
      {
          text = text.ToLower();
          search = search.ToLower();
          // ensure that phrase matches to a whole word   
          //LogDebug("validwordcheck text="+text, (int)LogSetting.DEBUG);
          //LogDebug("validwordcheck search=" + search, (int)LogSetting.DEBUG);

              int pos = 0;
              int oldpos = 0;
              while (pos >= 0)
              {
                  pos = text.IndexOf(search, oldpos);
                 // LogDebug("pos=" + pos, (int)LogSetting.DEBUG);
                  if (pos != -1)  //word was found
                  {
                      bool word_begin = false;
                      // check for word conditions at beginning
                      if (pos > 0)
                      {
                          if (text[pos - 1] == ' ')
                          {
                              word_begin = true;
                              //LogDebug("wordbegin true\n\n", (int)LogSetting.DEBUG);
                          }
                      }
                      else
                      {// start valid for word
                          word_begin = true;
                          //LogDebug("wordbegin true\n\n", (int)LogSetting.DEBUG);
                      }


                      bool word_end = false;
                      // check for word conditions at end
                      if (pos + search.Length < text.Length)
                      {
                          if (text[pos + search.Length] == ' ')
                          {
                              word_end = true;
                              //LogDebug("wordend true\n\n", (int)LogSetting.DEBUG);
                          }
                      }
                      else
                      {// end valid for word
                          word_end = true;
                          //LogDebug("wordend true\n\n", (int)LogSetting.DEBUG);
                      }

                      if ((word_begin == true) && (word_end == true))
                      {
                          //LogDebug("true\n\n", (int)LogSetting.DEBUG);
                          return true;
                      }
                  }

                  oldpos = pos + 1;
                  if ((pos + 1) >= text.Length)
                  {
                      break; //break while loop because end of loop is reached
                  }
              }
              //LogDebug("false\n\n", (int)LogSetting.DEBUG);
              return false;
      }
      

      //-------------------------------------------------------------------------------------------------------------
      //schedule new single schedule with the parameters
      //-------------------------------------------------------------------------------------------------------------
      [CLSCompliant(false)]
      public bool addsingleschedule(Program oneprogram, ref TvWish mywish, int counter)
      {

          /*
          string programName = oneprogram.Title;
          string description = oneprogram.Description;
          Int32 idChannel = oneprogram.IdChannel;
          DateTime startTime = oneprogram.StartTime;
          DateTime endTime = oneprogram.EndTime;
          */

          
          TvBusinessLayer layer = new TvBusinessLayer();

          
#if(TV100)
          IList allschedules = Schedule.ListAll();
#elif(TV101 || TV11 || TV12)
          IList<Schedule> allschedules = Schedule.ListAll();
#endif
          int allschedules_before = allschedules.Count;
         
          // add new schedule
          Schedule schedule = null;
           
          try
          {
              schedule = layer.AddSchedule(oneprogram.IdChannel, oneprogram.Title, oneprogram.StartTime, oneprogram.EndTime, 0);
              schedule.PreRecordInterval = mywish.i_prerecord;
              schedule.PostRecordInterval = mywish.i_postrecord;
              schedule.ScheduleType = 0;             
              schedule.Series = mywish.b_series;

              if (mywish.useFolderName == lng.TranslateString("Automatic", 2853)) //automatic seriesname with MP management
              {

#if (TV12 || TV11)
                  if ((oneprogram.EpisodePart != string.Empty) || (oneprogram.EpisodeName != string.Empty) || (oneprogram.SeriesNum != string.Empty) || (oneprogram.EpisodeNum != string.Empty))
#else
                  if ((oneprogram.SeriesNum != string.Empty) || (oneprogram.EpisodeNum != string.Empty))
#endif


                  {
                      schedule.Series = true;
                      LogDebug("schedule.Series changed to true for automatic", (int)LogSetting.DEBUG);
                  }
                  else
                  {
                      schedule.Series = false;
                      LogDebug("schedule.Series changed to false for automatic", (int)LogSetting.DEBUG);
                  }
              }
            
              LogDebug("schedule.Series=" + schedule.Series.ToString(), (int)LogSetting.DEBUG);

              schedule.KeepDate = mywish.D_keepuntil;
              schedule.KeepMethod = mywish.i_keepmethod;
              schedule.MaxAirings = mywish.i_keepepisodes;             
              schedule.RecommendedCard = mywish.i_recommendedcard;
              schedule.Priority = mywish.i_priority;
              schedule.Persist();

              
          }
          catch
          {
              LogDebug("", (int)LogSetting.INFO);
              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
              message = lng.TranslateString("Failed to add program {0}",60,oneprogram.Title);
              LogDebug(message, (int)LogSetting.INFO);
              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
              mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FAILED_ADDING, mywish.tvwishid, string.Empty);
              return false;

          }
          LogDebug("schedule created", (int)LogSetting.DEBUG);
          if (schedule == null)
          {
              LogDebug("", (int)LogSetting.INFO);
              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
              message = lng.TranslateString("Failed to create schedule for program {0}", 61, oneprogram.Title);
              LogDebug(message, (int)LogSetting.INFO);
              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
              mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FAILED_CREATE, mywish.tvwishid, string.Empty);
              return false;
          }

          #region schedule exists
          int allschedules_after = Schedule.ListAll().Count;
          LogDebug("allschedule", (int)LogSetting.DEBUG);
          if (allschedules_after == allschedules_before)  //fatal BUG fixed:     &&(VIEW_ONLY_MODE==false)  existing schedule can be deleted afterewards
          {
              LogDebug("", (int)LogSetting.INFO);
              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
              message = lng.TranslateString("Program: {0} already scheduled", 62, oneprogram.Title);
              LogDebug(message, (int)LogSetting.INFO);
              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);

              //  old: mymessage.addmessage(schedule, message, mywish.t_action, mywish.name, (int)XmlMessages.MessageEvents.SCHEDULE_FOUND, mywish.tvwishid);  //only added if no other message did exist before do not change
              mymessage.addmessage(schedule, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FAILED_ADDING, mywish.tvwishid, string.Empty);
              
              //outputscheduletoresponse(schedule, (int)LogSetting.INFO);
              return false;
          }
          #endregion schedule exists

          #region filter days
          LogDebug("start filterfunctions", (int)LogSetting.DEBUG);
          // filter function only use schedules starting after after_days and ending before before_days
          if ((mywish.i_afterdays > -1) && (mywish.i_beforedays > -1))
          {


              LogDebug("after_days= " + mywish.i_afterdays.ToString(), (int)LogSetting.DEBUG);
              LogDebug("before_days= " + mywish.i_beforedays.ToString(), (int)LogSetting.DEBUG);

              Int32 start_day = (int)oneprogram.StartTime.DayOfWeek;           
              //changed to usage of start time only !!!!!!!!!!!!!!!
              //Int32 end_day = (int)endTime.DayOfWeek;
              Int32 end_day = (int)oneprogram.StartTime.DayOfWeek;
              LogDebug("start_day= " + start_day, (int)LogSetting.DEBUG);
              LogDebug("end_day= " + end_day, (int)LogSetting.DEBUG);

              if (mywish.i_beforedays >= mywish.i_afterdays)
               /*
                      ------I-------------------------------------------------I------
               *          after                                            before
               *                          start              end
               *                          
               *    start >= after  &&  end <= before  &&  end >= start
               *       
               */
              
              { //use regular timeinterval from after_days till before_days
                  if ((start_day < mywish.i_afterdays) || (end_day > mywish.i_beforedays) || (end_day < start_day))

                  {
                      LogDebug("", (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      message =lng.TranslateString("Skipping program {0} because it is not between {1} and {2}",63, oneprogram.Title, ((DayOfWeek)mywish.i_afterdays).ToString() , ((DayOfWeek)mywish.i_beforedays).ToString());                                       
                      LogDebug(message, (int)LogSetting.INFO);
                      LogDebug("\nStart=" + oneprogram.StartTime.ToString(), (int)LogSetting.INFO);    
                      LogDebug("End=" + oneprogram.EndTime.ToString(), (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);

                      mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);
                      schedule.Delete();
                      return false;
                  }

              }
              else if (mywish.i_beforedays < mywish.i_afterdays)
              /*                                    0:0
                      -----I-------------------------I------------------------I------
               *          after                                            before
               *          
               *     1)    I    start        end     I                        I               start >= after   &&   end >= after   &&   end >= start
               *          
               *          
               *     2)    I                         I       start     end    I                start <=before   &&   end <= before  &&   end >= start
               *     
               *     3)    I    start                I       end              I                start >= after   &&   end <= before  &&   end <= start
               *                          
               *    
               *       
               */
              {  //use timeinterval from before_days till after_days
                  bool doublecheck = false;
                  if (((start_day >= mywish.i_afterdays) && (end_day >= mywish.i_afterdays) && (end_day >= start_day)) || ((start_day <= mywish.i_beforedays) && (end_day <= mywish.i_beforedays) && (end_day >= start_day)) || ((start_day >= mywish.i_afterdays) && (end_day <= mywish.i_beforedays) && (end_day <= start_day)))
                  {
                      doublecheck = true;
                  }

                  if (((start_day < mywish.i_afterdays) || (end_day < mywish.i_afterdays) || (end_day < start_day)) && ((start_day > mywish.i_beforedays) || (end_day > mywish.i_beforedays) || (end_day < start_day)) && ((start_day < mywish.i_afterdays) || (end_day > mywish.i_beforedays)))
                  {
                      if (doublecheck == true)
                      {
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.ERROR);
                          LogDebug("doublecheck failed check equation for days - please post logfile and contact huha", (int)LogSetting.ERROR);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.ERROR);
                      }
                      LogDebug("", (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      message = lng.TranslateString("Skipping program {0} because it is not between {1} and {2}",63, oneprogram.Title, ((DayOfWeek)mywish.i_afterdays).ToString() , ((DayOfWeek)mywish.i_beforedays).ToString() );
                      LogDebug(message, (int)LogSetting.INFO);
                      LogDebug("\nStart=" + oneprogram.StartTime.ToString(), (int)LogSetting.INFO);                     
                      LogDebug("End=" + oneprogram.EndTime.ToString(), (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);
                      
                      schedule.Delete();
                      return false;
                  }
              }// if (before_days == after_days) do nothing
          }
          #endregion filter days

          #region filter time
          LogDebug("afterhours filter", (int)LogSetting.DEBUG);
          // filter function only use schedules starting after hh:mm and ending before hh:mm
          if ((mywish.i_aftertime > -1) && (mywish.i_beforetime > -1))
          {

              int start_time_minutes_all = oneprogram.StartTime.Hour * 60 + oneprogram.StartTime.Minute;
              int end_time_minutes_all = start_time_minutes_all;

              LogDebug("after_minutes= " + mywish.i_aftertime, (int)LogSetting.DEBUG);
              LogDebug("before_minutes= " + mywish.i_beforetime, (int)LogSetting.DEBUG);
              LogDebug("start_time_minutes= " + start_time_minutes_all, (int)LogSetting.DEBUG);
              LogDebug("end_time_minutes= " + end_time_minutes_all, (int)LogSetting.DEBUG);

              if (mywish.i_beforetime >= mywish.i_aftertime)
              /*
                      ------I-------------------------------------------------I------
               *          after                                            before
               *                          start              end
               *                          
               *    start >= after  &&  end <= before  &&  end >= start
               *       
               */
              { //use regular timeinterval from after_minutes till before_minutes
                  if ((start_time_minutes_all < mywish.i_aftertime) || (end_time_minutes_all > mywish.i_beforetime) || (end_time_minutes_all < start_time_minutes_all))  
                  
                  {

                      LogDebug("", (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      message = lng.TranslateString("Skipping program {0} because it is not between {1} and {2}",64, oneprogram.Title, mywish.aftertime , mywish.beforetime);
                      message+="\nStart=" + oneprogram.StartTime.ToString();
                      LogDebug(message, (int)LogSetting.INFO);
                      LogDebug("End=" + oneprogram.EndTime.ToString(), (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);
                      
                      schedule.Delete();
                      return false;
                  }

              }
              else if (mywish.i_beforetime < mywish.i_aftertime)
              /*                                    0:0
                      -----I-------------------------I------------------------I------
               *          after                                            before
               *          
               *     1)    I    start        end     I                        I               start >= after   &&   end >= after   &&   end >= start
               *          
               *          
               *     2)    I                         I       start     end    I                start <=before   &&   end <= before  &&   end >= start
               *     
               *     3)    I    start                I       end              I                start >= after   &&   end <= before  &&   end <= start
               *                          
               *    
               *       
               */
              {  //use timeinterval from after_minutes till 23:59 and from 00:00 till before_minutes
                  bool doublecheck = false;
                  if (((start_time_minutes_all >= mywish.i_aftertime) && (end_time_minutes_all >= mywish.i_aftertime) && (end_time_minutes_all >= start_time_minutes_all)) || ((start_time_minutes_all <= mywish.i_beforetime) && (end_time_minutes_all <= mywish.i_beforetime) && (end_time_minutes_all >= start_time_minutes_all)) || ((start_time_minutes_all >= mywish.i_aftertime) && (end_time_minutes_all <= mywish.i_beforetime) && (end_time_minutes_all <= start_time_minutes_all)))
                  {
                      doublecheck = true;
                  }
                  //if (((start_time_minutes_all < mywish.i_aftertime) && (start_time_minutes_all > mywish.i_beforetime)) || ((end_time_minutes_all > mywish.i_beforetime) && (end_time_minutes_all < mywish.i_aftertime)))
                  if ( ( (start_time_minutes_all < mywish.i_aftertime)||(end_time_minutes_all < mywish.i_aftertime)||(end_time_minutes_all < start_time_minutes_all) ) && ( (start_time_minutes_all > mywish.i_beforetime)||(end_time_minutes_all > mywish.i_beforetime)||(end_time_minutes_all < start_time_minutes_all) ) && ( (start_time_minutes_all < mywish.i_aftertime)||(end_time_minutes_all > mywish.i_beforetime) ) )
                  {
                      if (doublecheck == true)
                      {
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.ERROR);
                          LogDebug("doublecheck failed check equation for minutes - please post logfile and contact huha", (int)LogSetting.ERROR);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.ERROR);
                      }
                      LogDebug("", (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      message = lng.TranslateString("Skipping program {0} because it is not between {1} and {2}",64,oneprogram.Title, mywish.aftertime , mywish.beforetime);
                      message+="\nStart=" + oneprogram.StartTime.ToString();
                      LogDebug(message, (int)LogSetting.INFO);
                      LogDebug("End=" + oneprogram.EndTime.ToString(), (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);                      
                      
                      schedule.Delete();
                      return false;
                  }
              }// if (before_minutes == after_minutes) do nothing
          }
          #endregion filter time

          #region within next hours
          LogDebug("start WithinNextHours", (int)LogSetting.DEBUG);
          if (mywish.i_withinNextHours > 0)
          {
              DateTime myNextDateTime = DateTime.Now;
              myNextDateTime = myNextDateTime.AddHours(mywish.i_withinNextHours);

              if (oneprogram.StartTime > myNextDateTime)
              {
                  LogDebug("", (int)LogSetting.INFO);
                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                  lng.TranslateString(message = "Skipping program {0} because it is not within the next {1} hours", 65, oneprogram.Title,mywish.i_withinNextHours.ToString());
                  message+="\nStart=" + oneprogram.StartTime.ToString();
                  LogDebug(message, (int)LogSetting.INFO);
                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                  mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);                      
                      
                  schedule.Delete();
                  return false;
              }
          }
          #endregion within next hours

          #region channelfilter
          LogDebug("start channelfilter", (int)LogSetting.DEBUG);
          //filter one specific channel
          if (mywish.channel != "")
          {
              Channel channel = null;
              string channelname = "";
              try  //must use try due to in bug in map.ReferencedChannelGroup() - caused exception before
              {
                  //search for TV group match
                  channel = schedule.ReferencedChannel();
                  channelname = channel.DisplayName;
              }
              catch
              {
                  LogDebug("", (int)LogSetting.INFO);
                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                  LogDebug("Error: could not retrieve channel name for program " + schedule.ProgramName, (int)LogSetting.INFO);
                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                  channelname = "";
              }
              LogDebug("channelname=" + channelname, (int)LogSetting.DEBUG);
              LogDebug("channelfilter=" + mywish.channel, (int)LogSetting.DEBUG);

              if ((mywish.channel != channelname)&&(mywish.channel != lng.TranslateString("Any",4100)))
              {
                  LogDebug("", (int)LogSetting.INFO);
                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                  message = lng.TranslateString("Skipping program {0} because it does not match the required channel filter {1}",66, oneprogram.Title, mywish.channel);                
                  LogDebug(message, (int)LogSetting.INFO);
                  LogDebug("\nChannelname=" + channelname, (int)LogSetting.INFO);
                  LogDebug("\nStart=" + oneprogram.StartTime.ToString(), (int)LogSetting.INFO);
                  LogDebug("End=" + oneprogram.EndTime.ToString(), (int)LogSetting.INFO);
                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                  mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);                      
                  
                  schedule.Delete();
                  return false;
              }
          }
          #endregion channelfilter

#if (TV12)
          #region filter episode part
          //filter episode part
          LogDebug("start filterepisode part", (int)LogSetting.DEBUG);
          if (mywish.episodepart != "")
          {
              if (mywish.episodepart.ToUpper() != oneprogram.EpisodePart.ToUpper())
              {
                  LogDebug("", (int)LogSetting.INFO);
                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                  message = lng.TranslateString("Skipping program {0} because it does not match the required episode part {1}",67,oneprogram.Title,mywish.episodepart);
                  LogDebug(message, (int)LogSetting.INFO);
                  LogDebug("\nStart=" + oneprogram.StartTime.ToString(), (int)LogSetting.INFO);
                  LogDebug("\nEnd=" + oneprogram.EndTime.ToString(), (int)LogSetting.INFO);
                  LogDebug("\nEpisodepart=" + oneprogram.EpisodePart, (int)LogSetting.INFO);                 
                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                  mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);

                  schedule.Delete();
                  return false;
              }
          }
          #endregion filter episode part

          #region filter episode name
          //filter episode name
          LogDebug("start filterepisodename", (int)LogSetting.DEBUG);
          if (mywish.episodename != "")
          {
              if (mywish.episodename.ToUpper() != oneprogram.EpisodeName.ToUpper())
              {
                  LogDebug("", (int)LogSetting.INFO);
                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                  message = lng.TranslateString("Skipping program {0} because it does not match the required episode name {1}",68, oneprogram.Title, mywish.episodename);
                  LogDebug(message, (int)LogSetting.INFO);
                  LogDebug("\nStart=" + oneprogram.StartTime.ToString(), (int)LogSetting.INFO);
                  LogDebug("\nEnd=" + oneprogram.EndTime.ToString(), (int)LogSetting.INFO);
                  LogDebug("\nEpisodeName=" + oneprogram.EpisodeName, (int)LogSetting.INFO);                
                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                  mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);
                  
                  schedule.Delete();
                  return false;
              }
          }
          #endregion filter episode name

          

          #region filter series number
          //filter series number
          LogDebug("start filterseries", (int)LogSetting.DEBUG);
          if ((mywish.seriesnumber != "") && (oneprogram.SeriesNum != ""))
          {
              try
              {
                  string[] tokens = oneprogram.SeriesNum.Split('\\','/');
                  Log.Debug("tokens[0]=" + tokens[0]);
                  int programNumber = Convert.ToInt32(tokens[0]);
                  Log.Debug("programNumber=" + programNumber.ToString());
                  //process expressions
                  bool expression = false;
                  string[] expressionArray = mywish.seriesnumber.Split(',');
                  foreach (string myExpression in expressionArray)
                  {
                      if (myExpression.Contains("-") == true)
                      {
                          string[] numberarray = myExpression.Split('-');
                          Log.Debug("numberarray.Length=" + numberarray.Length.ToString());
                          if (numberarray.Length == 2)
                          {
                              if ((numberarray[0] == string.Empty) || (numberarray[1] == string.Empty))
                              {  // a- or -a
                                  Log.Debug("a- or -a case");
                                  string temp = myExpression.Replace("-", string.Empty);
                               
                                  int maxValue = Convert.ToInt32(temp);
                                  Log.Debug("Expression = " + mywish.seriesnumber + " maxValue=" + maxValue.ToString());
                                  if (programNumber <= maxValue)
                                  {
                                      expression = true;
                                      Log.Debug("Found match in expression " + myExpression);
                                      break;
                                  }    
                              }
                              else //a-b
                              {
                                  int minValue = Convert.ToInt32(numberarray[0]);
                                  Log.Debug("Expression = " + mywish.seriesnumber + " minValue=" + minValue.ToString());

                                  int maxValue = Convert.ToInt32(numberarray[1]);
                                  Log.Debug("Expression = " + mywish.seriesnumber + " maxValue=" + maxValue.ToString());

                                  if ((programNumber >= minValue) && (programNumber <= maxValue))
                                  {
                                      expression = true;
                                      Log.Debug("Found match in expression " + myExpression);
                                      break;
                                  }
                              }
                          }
                          else
                          {
                              Log.Error("Ignoring invalid expression " + myExpression);
                              break;
                          }                        
                      }
                      else if (myExpression.Contains("+") == true)
                      {
                          string temp = myExpression.Replace("+", string.Empty);
                          int minValue = Convert.ToInt32(temp);
                          Log.Debug("Expression = " + mywish.seriesnumber + " minValue=" + minValue.ToString());
                          if (programNumber >= minValue)
                          {
                              expression = true;
                              Log.Debug("Found match in expression " + myExpression);
                              break;
                          }                          
                      }                      
                      else
                      {
                          int minValue = Convert.ToInt32(myExpression);
                          Log.Debug("Expression = " + mywish.seriesnumber + " intValue=" + minValue.ToString());
                          if (programNumber == minValue)
                          {
                              expression = true;
                              Log.Debug("Found match in expression " + myExpression);
                              break;
                          }                          
                      }
                  }

                  if (expression == false) //skip incorrect numbers
                  {
                      LogDebug("", (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      message = lng.TranslateString("Skipping program {0} because it is not matching the required series number {1}",69, oneprogram.Title,mywish.seriesnumber);
                      LogDebug(message, (int)LogSetting.INFO);
                      LogDebug("Start=" + oneprogram.StartTime.ToString(), (int)LogSetting.INFO);
                      LogDebug("End=" + oneprogram.EndTime.ToString(), (int)LogSetting.INFO);
                      LogDebug("Seriesnumber=" + oneprogram.SeriesNum, (int)LogSetting.INFO);                     
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);

                      schedule.Delete();
                      return false;
                  }
              }
              catch (Exception exc)
              {
                  //ignore errors
                  LogDebug("Exception in filter series number - Message=" + exc.Message, (int)LogSetting.ERROR);
                  string languagetext = lng.TranslateString("Fatal error - check the log file", 24);
                  labelmessage(languagetext, PipeCommands.StartEpg); //do not stop - do not flag as error
                  Thread.Sleep(ErrorWaitTime);
              }
          }
          #endregion filter series number


          #region filter episode number
          //filter episode number
          LogDebug("start filter episode number", (int)LogSetting.DEBUG);
          if ((mywish.episodenumber != "") && (oneprogram.EpisodeNum != ""))
          {
              try
              {
                  string[] tokens = oneprogram.EpisodeNum.Split('\\', '/');
                  Log.Debug("tokens[0]=" + tokens[0]);
                  int programNumber = Convert.ToInt32(tokens[0]);
                  Log.Debug("programNumber=" + programNumber.ToString());
                  //process expressions
                  bool expression = false;
                  string[] expressionArray = mywish.episodenumber.Split(',');
                  foreach (string myExpression in expressionArray)
                  {
                      if (myExpression.Contains("-") == true)
                      {
                          string[] numberarray = myExpression.Split('-');
                          if (numberarray.Length == 2)
                          {
                              if ((numberarray[0] == string.Empty) || (numberarray[1] == string.Empty))
                              {  // a- or -a
                                  Log.Debug("a- or -a case");
                                  string temp = myExpression.Replace("-", string.Empty);

                                  int maxValue = Convert.ToInt32(temp);
                                  Log.Debug("Expression = " + mywish.episodenumber + " maxValue=" + maxValue.ToString());
                                  if (programNumber <= maxValue)
                                  {
                                      expression = true;
                                      Log.Debug("Found match in expression " + myExpression);
                                      break;
                                  }
                              }
                              else //a-b
                              {
                                  int minValue = Convert.ToInt32(numberarray[0]);
                                  Log.Debug("Expression = " + mywish.episodenumber + " minValue=" + minValue.ToString());

                                  int maxValue = Convert.ToInt32(numberarray[1]);
                                  Log.Debug("Expression = " + mywish.episodenumber + " maxValue=" + maxValue.ToString());

                                  if ((programNumber >= minValue) && (programNumber <= maxValue))
                                  {
                                      expression = true;
                                      Log.Debug("Found match in expression " + myExpression);
                                      break;
                                  }
                              }
                          }
                          else
                          {
                              Log.Error("Ignoring invalid expression " + myExpression);
                              break;
                          }
                      }                      
                      else if (myExpression.Contains("+") == true)
                      {
                          string temp = myExpression.Replace("+", string.Empty);
                          int minValue = Convert.ToInt32(temp);
                          Log.Debug("Expression = " + mywish.episodenumber + " intValue2=" + minValue.ToString());
                          if (programNumber >= minValue)
                          {                       
                              expression = true;
                              Log.Debug("Found match in expression " + myExpression);
                              break;
                          }
                      }
                      else
                      {
                          int minValue = Convert.ToInt32(myExpression);
                          Log.Debug("Expression = " + mywish.episodenumber + " intValue=" + minValue.ToString());
                          if (programNumber == minValue)
                          {
                              expression = true;
                              Log.Debug("Found match in expression " + myExpression);
                              break;
                          }
                      }
                  }

                  if (expression == false) //skip incorrect numbers
                  {
                      LogDebug("", (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      message = lng.TranslateString("Skipping program {0} because it is not matching the required episode number {1}",70, oneprogram.Title, mywish.episodenumber);
                      LogDebug(message, (int)LogSetting.INFO);
                      LogDebug("Start=" + oneprogram.StartTime.ToString(), (int)LogSetting.INFO);
                      LogDebug("End=" + oneprogram.EndTime.ToString(), (int)LogSetting.INFO);
                      LogDebug("Episodenumber=" + oneprogram.EpisodeNum, (int)LogSetting.INFO);                     
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);

                      schedule.Delete();
                      return false;
                  }
              }
              catch (Exception exc)
              {
                  //ignore errors
                  LogDebug("Exception in filter episode number - Message=" + exc.Message, (int)LogSetting.ERROR);
                  string languagetext = lng.TranslateString("Fatal error - check the log file", 24);
                  labelmessage(languagetext, PipeCommands.StartEpg); //do not stop - do not flag as error
                  Thread.Sleep(ErrorWaitTime);
              }
          }
          #endregion filter episode number


#endif

          
          //start checking for repeated and convert to upper
          string programNameRpt = ProcessEpgMarker(oneprogram.Title);
          string descriptionRpt = ProcessEpgMarker(oneprogram.Description);

          /*string programNameRpt = oneprogram.Title.ToUpper();
          try
          {
              programNameRpt = programNameRpt.Replace(_EpgMarker.ToUpper(), "");
          }
          catch
          {
              //ignore errors
          }
          string descriptionRpt = oneprogram.Description.ToUpper();
          
          try
          {
              descriptionRpt = descriptionRpt.Replace(_EpgMarker.ToUpper(), "");
          }
          catch
          {
              //ignore errors
          }*/

          
          LogDebug("programNameRpt=" + programNameRpt, (int)LogSetting.DEBUG);
          LogDebug("descriptionRpt=" + descriptionRpt, (int)LogSetting.DEBUG);

          #region recording exist
          //check for existing recording of same title and description 
          if (mywish.b_skip == true)
          {

              
              foreach (Recording onerecording in Recording.ListAll())
              {
                  string recordedTitle = ProcessEpgMarker(onerecording.Title);
                  string recordedDescription = ProcessEpgMarker(onerecording.Description);

                  /*string recordedTitle = onerecording.Title.ToUpper();
                  try
                  {
                      recordedTitle = recordedTitle.Replace(_EpgMarker.ToUpper(), "");
                  }
                  catch
                  {
                      //ignore errors
                  }
                  string recordedDescription = onerecording.Description.ToUpper();
                  try
                  {
                      recordedDescription = recordedDescription.Replace(_EpgMarker.ToUpper(), "");
                  }
                  catch
                  {
                      //ignore errors
                  }*/
                  //LogDebug("recordedDescription=" + recordedDescription, (int)LogSetting.DEBUG);
                  //LogDebug("descriptionRpt=" + descriptionRpt, (int)LogSetting.DEBUG);


                  if (recordedTitle != programNameRpt)
                      continue;        //new: added for speedup do not do episodemanagament if title does not match
                  

#if(TV100 || TV101)
                  //bool ok = episodeManagement(recordedDescription, descriptionRpt, "", "", "", "", "", oneprogram.SeriesNum, "", oneprogram.EpisodeNum, _episode_d, local_episode_n, local_episode_m);
                  bool ok = episodeManagement(recordedDescription, descriptionRpt, "", "", "", "", "", oneprogram.SeriesNum, "", oneprogram.EpisodeNum, mywish.b_episodecriteria_d, mywish.b_episodecriteria_n, mywish.b_episodecriteria_c);
                  
#elif(TV11)
                  //bool ok = episodeManagement(recordedDescription, descriptionRpt, onerecording.EpisodePart, oneprogram.EpisodePart, onerecording.EpisodeName, oneprogram.EpisodeName, onerecording.SeriesNum, oneprogram.SeriesNum, onerecording.EpisodeNum, oneprogram.EpisodeNum, _episode_d, local_episode_n, local_episode_m);
                  bool ok = episodeManagement(recordedDescription, descriptionRpt, onerecording.EpisodePart, oneprogram.EpisodePart, onerecording.EpisodeName, oneprogram.EpisodeName, onerecording.SeriesNum, oneprogram.SeriesNum, onerecording.EpisodeNum, oneprogram.EpisodeNum, mywish.b_episodecriteria_d, mywish.b_episodecriteria_n, mywish.b_episodecriteria_c);
                  
#elif(TV12)

                  //bool ok = episodeManagement(recordedDescription, descriptionRpt, onerecording.EpisodePart, oneprogram.EpisodePart, onerecording.EpisodeName, oneprogram.EpisodeName, onerecording.SeriesNum, oneprogram.SeriesNum, onerecording.EpisodeNum, oneprogram.EpisodeNum, mywish.b_episodecriteria_a, mywish.b_episodecriteria_d, mywish.b_episodecriteria_p, mywish.b_episodecriteria_n, mywish.b_episodecriteria_c);
                  bool ok = episodeManagement(recordedDescription, descriptionRpt, onerecording.EpisodePart, oneprogram.EpisodePart, onerecording.EpisodeName, oneprogram.EpisodeName, onerecording.SeriesNum, oneprogram.SeriesNum, onerecording.EpisodeNum, oneprogram.EpisodeNum, mywish.b_episodecriteria_d, mywish.b_episodecriteria_n, mywish.b_episodecriteria_c);
                  
#endif                    

                  if ((VIEW_ONLY_MODE == false) && (recordedTitle == programNameRpt) && (ok == true))
                  {
                      //check for preferred group

                      
                      bool old1 = IsChannelInGroup(onerecording.IdChannel, mywish.preferredgroup);
                      bool old2 = IsRadioChannelInGroup(onerecording.IdChannel, mywish.preferredgroup);
                      bool new1 = IsChannelInGroup(oneprogram.IdChannel, mywish.preferredgroup);
                      bool new2 = IsRadioChannelInGroup(oneprogram.IdChannel, mywish.preferredgroup);
                      bool expression = (!new1 && !new2) || old1 || old2;

                      Log.Debug("old1=" + old1.ToString(), (int)LogSetting.DEBUG);
                      Log.Debug("old2=" + old2.ToString(), (int)LogSetting.DEBUG);
                      Log.Debug("new1=" + new1.ToString(), (int)LogSetting.DEBUG);
                      Log.Debug("new2=" + new2.ToString(), (int)LogSetting.DEBUG);
                      Log.Debug("expression=" + expression.ToString(), (int)LogSetting.DEBUG);
                      Log.Debug("mywish.b_includeRecordings=" + mywish.b_includeRecordings.ToString(), (int)LogSetting.DEBUG);
                      
                      if (!mywish.b_includeRecordings || (mywish.preferredgroup==lng.TranslateString("All Channels",4104)) || expression)  //preferred group only if includeRecordings==true
                      {
                          LogDebug("", (int)LogSetting.INFO);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO); //do not send reply mail for already scheduled movies
                          message = lng.TranslateString("Program: {0} already recorded",71,oneprogram.Title);
                          LogDebug(message, (int)LogSetting.INFO);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                          outputscheduletoresponse(schedule, (int)LogSetting.INFO);
                          mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.ALREADY_RECORDED, mywish.tvwishid, string.Empty);
                  
                          schedule.Delete();
                          return false;
                      }
                      else
                      {
                          LogDebug("", (int)LogSetting.INFO);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO); //do not send reply mail for already scheduled movies
                          LogDebug("Warning: Program: " + oneprogram.Title + " already recorded, but not in preferred group - will try again", (int)LogSetting.INFO);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                                                   
                      }
                  }
              }
          }
          #endregion recording exist

          #region skip deleted entries in messages
          //skip deleted entries in messages if found again
          if ((mywish.b_skip == true) && (_skipDeleted == true))
          {
              foreach (xmlmessage singlemessage in mymessage.ListAllTvMessages())
              {
                  if (singlemessage.type == MessageType.Deleted.ToString())
                  {
                      string title = ProcessEpgMarker(singlemessage.title);
                      string messageDescription = ProcessEpgMarker(singlemessage.description);
                      
                      
                      /*string title = singlemessage.title.ToUpper();
                      try
                      {
                          title = title.Replace(_EpgMarker.ToUpper(), "");
                      }
                      catch
                      {
                          //ignore errors
                      }
                      string messageDescription = singlemessage.description.ToUpper();
                      try
                      {
                          messageDescription = messageDescription.Replace(_EpgMarker.ToUpper(), "");
                      }
                      catch
                      {
                          //ignore errors
                      }*/
                      //LogDebug("onerecording.Description=" + onerecording.Description, (int)LogSetting.DEBUG);
                      //LogDebug("onerecording.Description=" + onerecording.Description, (int)LogSetting.DEBUG);

                      if (title != programNameRpt)
                          continue;   //new. added for speedup - ignore if title does not match and do not do episodemanagement


#if(TV100 || TV101)
                      //bool ok = episodeManagement(messageDescription, descriptionRpt, "", "", "", "", "", oneprogram.SeriesNum, "", oneprogram.EpisodeNum, _episode_d, local_episode_n, local_episode_m);
                      bool ok = episodeManagement(messageDescription, descriptionRpt,  "", "", "", "", "", oneprogram.SeriesNum, "", oneprogram.EpisodeNum, mywish.b_episodecriteria_d, mywish.b_episodecriteria_n, mywish.b_episodecriteria_c);


#elif(TV11)
                      //bool ok = episodeManagement(messageDescription, descriptionRpt, singlemessage.EpisodePart, oneprogram.EpisodePart, singlemessage.EpisodeName, oneprogram.EpisodeName, singlemessage.SeriesNum, oneprogram.SeriesNum, singlemessage.EpisodeNum, oneprogram.EpisodeNum, _episode_d, local_episode_n, local_episode_m);
                      bool ok = episodeManagement(messageDescription, descriptionRpt, singlemessage.EpisodePart, oneprogram.EpisodePart, singlemessage.EpisodeName, oneprogram.EpisodeName, singlemessage.SeriesNum, oneprogram.SeriesNum, singlemessage.EpisodeNum, oneprogram.EpisodeNum, mywish.b_episodecriteria_d, mywish.b_episodecriteria_n, mywish.b_episodecriteria_c);

#elif(TV12)
                      //bool ok = episodeManagement(messageDescription, descriptionRpt, singlemessage.EpisodePart, oneprogram.EpisodePart, singlemessage.EpisodeName, oneprogram.EpisodeName, singlemessage.SeriesNum, oneprogram.SeriesNum, singlemessage.EpisodeNum, oneprogram.EpisodeNum, mywish.b_episodecriteria_a, mywish.b_episodecriteria_d, mywish.b_episodecriteria_p, mywish.b_episodecriteria_n, mywish.b_episodecriteria_c);
                      bool ok = episodeManagement(messageDescription, descriptionRpt, singlemessage.EpisodePart, oneprogram.EpisodePart, singlemessage.EpisodeName, oneprogram.EpisodeName, singlemessage.SeriesNum, oneprogram.SeriesNum, singlemessage.EpisodeNum, oneprogram.EpisodeNum, mywish.b_episodecriteria_d, mywish.b_episodecriteria_n, mywish.b_episodecriteria_c);

#endif

                      if ((VIEW_ONLY_MODE == false) && (title == programNameRpt) && (ok == true))
                      {
                          LogDebug("", (int)LogSetting.INFO);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO); //do not send reply mail for already scheduled movies
                          message = lng.TranslateString("Program: {0} has been already deleted and is skipped",72,oneprogram.Title);
                          LogDebug(message, (int)LogSetting.INFO);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                          outputscheduletoresponse(schedule, (int)LogSetting.INFO);
                          mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.REPEATED_FOUND, mywish.tvwishid, string.Empty);

                          schedule.Delete();
                          return false;
                      }
                  }

              }
          }//end skip deleted entries in messages if found again
          #endregion skip deleted entries in messages

          #region skip entries from the past
          //skip entries from the past
          if ((schedule.StartTime < DateTime.Now) && (schedule.ScheduleType == 0) && (VIEW_ONLY_MODE == false)) //old date, but only for type "ONCE" and Email/Recording mode  
          {

              LogDebug("", (int)LogSetting.INFO);
              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
              message = lng.TranslateString("Start time of program {0} is in the past - will skip schedule",73,oneprogram.Title);
              LogDebug(message, (int)LogSetting.INFO);
              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
              mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.FILTER_MISMATCH, mywish.tvwishid, string.Empty);

              schedule.Delete();
              return false;
          }
          #endregion skip entries from the past

          Schedule nonPreferredGroupSchedule = null;

          #region Check for repeated schedules (1st)
          if ((mywish.b_skip == true)&&(VIEW_ONLY_MODE == false)) // 1st
          {//Check for repeated schedules of same title and description
              foreach (Schedule myschedule in allschedules)
              {
                  string myscheduleProgramName = ProcessEpgMarker(myschedule.ProgramName);
                  

                  /*string myscheduleProgramName = myschedule.ProgramName.ToUpper();
                  try
                  {
                      myscheduleProgramName = myscheduleProgramName.Replace(_EpgMarker.ToUpper(), "");
                  }
                  catch
                  {
                      //ignore errors
                  }*/

                  //check for identical description
                  LogDebug("check for identical description", (int)LogSetting.DEBUG);
                  //retrieve Program from Schedule
#if(TV100 || TV101)
                          Program myprogram = Program.RetrieveByTitleAndTimes(myschedule.ProgramName, myschedule.StartTime, myschedule.EndTime);
#elif(TV11 || TV12)
                  Program myprogram = Program.RetrieveByTitleTimesAndChannel(myschedule.ProgramName, myschedule.StartTime, myschedule.EndTime, myschedule.IdChannel);
#endif

                  if (myprogram != null)
                  {
                      string myprogramDescription = ProcessEpgMarker(myprogram.Description);
                      /*string myprogramDescription = myprogram.Description.ToUpper();
                      try
                      {
                          myprogramDescription = myprogramDescription.Replace(_EpgMarker.ToUpper(), "");
                      }
                      catch
                      {
                          //ignore errors
                      }*/

                      //compare episodes


#if(TV100 || TV101)
                      bool ok = episodeManagement(myprogramDescription, descriptionRpt, "", "", "", "", myprogram.SeriesNum, oneprogram.SeriesNum, myprogram.EpisodeNum, oneprogram.EpisodeNum, mywish.b_episodecriteria_d, mywish.b_episodecriteria_n, mywish.b_episodecriteria_c);


#elif(TV11)
                      bool ok = episodeManagement(myprogramDescription, descriptionRpt, myprogram.EpisodePart, oneprogram.EpisodePart, myprogram.EpisodeName, oneprogram.EpisodeName, myprogram.SeriesNum, oneprogram.SeriesNum, myprogram.EpisodeNum, oneprogram.EpisodeNum, mywish.b_episodecriteria_d, mywish.b_episodecriteria_n, mywish.b_episodecriteria_c);

#elif(TV12)
                      bool ok = episodeManagement(myprogramDescription, descriptionRpt, myprogram.EpisodePart, oneprogram.EpisodePart, myprogram.EpisodeName, oneprogram.EpisodeName, myprogram.SeriesNum, oneprogram.SeriesNum, myprogram.EpisodeNum, oneprogram.EpisodeNum, mywish.b_episodecriteria_d, mywish.b_episodecriteria_n, mywish.b_episodecriteria_c);

#endif

                      if ((VIEW_ONLY_MODE == false) && (ok == true) && (myprogram.Title.ToUpper() == oneprogram.Title.ToUpper()) )  //old bug fixed need to compare title!!!
                      {//schedules do match -> repeated schedule  

                          Log.Debug("mywish.preferredgroup=" + mywish.preferredgroup, (int)LogSetting.DEBUG);

                          bool old1 = IsChannelInGroup(myschedule.IdChannel, mywish.preferredgroup);
                          bool old2 = IsRadioChannelInGroup(myschedule.IdChannel, mywish.preferredgroup);
                          bool new1 = IsChannelInGroup(oneprogram.IdChannel, mywish.preferredgroup);
                          bool new2 = IsRadioChannelInGroup(oneprogram.IdChannel, mywish.preferredgroup);
                          bool expression = (!new1 && !new2) || old1 || old2;

                          Log.Debug("old1=" + old1.ToString(), (int)LogSetting.DEBUG);
                          Log.Debug("old2=" + old2.ToString(), (int)LogSetting.DEBUG);
                          Log.Debug("new1=" + new1.ToString(), (int)LogSetting.DEBUG);
                          Log.Debug("new2=" + new2.ToString(), (int)LogSetting.DEBUG);
                          Log.Debug("expression=" + expression.ToString(), (int)LogSetting.DEBUG);

                          if ((mywish.preferredgroup == lng.TranslateString("All Channels",4104)) || expression)
                          {
                              LogDebug("", (int)LogSetting.INFO);
                              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                              message = lng.TranslateString("Repeated Schedule {0} found  - skipping",74,oneprogram.Title);
                              LogDebug(message, (int)LogSetting.INFO);
                              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                              LogDebug("Old Schedule:", (int)LogSetting.INFO);
                              outputscheduletoresponse(myschedule, (int)LogSetting.INFO);
                              mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.REPEATED_FOUND, mywish.tvwishid, string.Empty);

                              schedule.Delete();
                              return false;
                          }
                          else
                          {
                              LogDebug("", (int)LogSetting.INFO);
                              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                              LogDebug("Repeated Schedule " + oneprogram.Title + " found with same title, but not in preferred group - will try again", (int)LogSetting.INFO);
                              LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);


                              if ((_automaticrecording == true) && (mywish.action != lng.TranslateString("Email",2701)) && (mywish.action != lng.TranslateString("View",2703))) //schedule new recording
                              {
                                  
                                  Log.Debug("Storing as nonPreferredGroupSchedule old schedule not in preferred group");
                                  nonPreferredGroupSchedule = myschedule;  //will be deleted later
                                  
                              }
                          }

                      }//end schedules do match -> repeated schedule

                  }//end (myprogram != null)

              }//end all schedules loop
              
          }//end skip repeated schedules
          #endregion Check for repeated schedules (1st)

          #region Check For Conflicts  (2nd)
          // Check for conflicts (2nd)
          LogDebug("check for conflicts", (int)LogSetting.DEBUG);
          if ((VIEW_ONLY_MODE == false)&& (_scheduleconflicts == false))  //scheduleconflicts = true will trigger priority processing
          {
              IList<Schedule> conflict_schedules = GetAllConflictSchedules(schedule, nonPreferredGroupSchedule);
               
              if (conflict_schedules.Count > 0) 
              {
                  LogDebug("", (int)LogSetting.INFO);
                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO); //do not send reply mail for already scheduled movies
                  message = lng.TranslateString("Program: {0} has conflicts and will not be scheduled",75,oneprogram.Title);
                  LogDebug(message, (int)LogSetting.INFO);
                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                  outputscheduletoresponse(schedule, (int)LogSetting.INFO);
                  //conflictprograms.Add(oneprogram);
                  mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.MANUAL_CONFLICT, mywish.tvwishid, string.Empty);

                  schedule.Delete();
                  return false;
              }
          }
          #endregion Check For Conflicts  (2nd)

          #region PriorityProcessing (3rd)
          //priority processing 3rd if schedule conflicts==true, must come after conflicts
          LogDebug("start priorityprocessing", (int)LogSetting.DEBUG);
          if ((_scheduleconflicts == true) && (_automaticrecording == true) && (mywish.action != lng.TranslateString("Email", 2701)) && (VIEW_ONLY_MODE == false)) 
          {
              
              IList<Schedule> allconflicts = GetAllConflictSchedules(schedule, nonPreferredGroupSchedule);

              LogDebug("allconflicts.Count=" + allconflicts.Count.ToString(), (int)LogSetting.DEBUG);
              while (allconflicts.Count > 0)  //loop until all conflicts are resolved
              {
                  bool foundflag = false;
                  foreach (Schedule conflictschedule in allconflicts)  //only one conflict will be removed (break statements prevents complete loop)
                  {
                      LogDebug("schedule.Priority=" + schedule.Priority.ToString(), (int)LogSetting.DEBUG);
                      LogDebug("conflictschedule.ProgramName=" + conflictschedule.ProgramName.ToString(), (int)LogSetting.DEBUG);
                      LogDebug("conflictschedule.Priority=" + conflictschedule.Priority.ToString(), (int)LogSetting.DEBUG);

                      if (conflictschedule.Priority < schedule.Priority)
                      {
                          //delete conflicting schedule and send message
                          LogDebug("", (int)LogSetting.INFO);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                          message = lng.TranslateString("Deleting schedule {0} because of lower priority",76,conflictschedule.ProgramName);
                          LogDebug(message, (int)LogSetting.INFO);
                          LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                          mymessage.addmessage(conflictschedule, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.LOW_PRIORITY_DELETE, mywish.tvwishid, string.Empty);


                          

                          //checked above to avoid email or viewonly
                          Log.Debug("Changing message for old schedule to conflict");
                          int index = mymessage.GetTvMessageBySchedule(conflictschedule,MessageType.Scheduled);
                          if (index >= 0)
                          {
                              xmlmessage changemessage = mymessage.GetTvMessageAtIndex(index);
                              changemessage.message = "Deleting schedule with lower priority";
                              changemessage.type = MessageType.Conflict.ToString();
                              mymessage.ReplaceTvMessageAtIndex(index, changemessage);
                          }

                          Log.Debug("Deleting schedule with lower priority");
                          conflictschedule.Delete();
                          mywish.i_scheduled--;
                          foundflag = true;
                          break; //only one conflict deletion should fix it
                      }
                  }//end loop all conflicting schedules for priority processing
                  if (foundflag == false) //skip schedule due to not high enough priority
                  {
                      LogDebug("", (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      message = lng.TranslateString("Schedule {0} has not enough priority to delete conflicts - skipping",77,oneprogram.Title);
                      LogDebug(message, (int)LogSetting.INFO);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.INFO);
                      outputprogramresponse(oneprogram, (int)LogSetting.INFO);

                      mymessage.addmessage(oneprogram, message, MessageType.Conflict, mywish.name, (int)XmlMessages.MessageEvents.LOW_PRIORITY_DELETE, mywish.tvwishid, string.Empty);

                      schedule.Delete();
                      return false;
                  }
                  
                  //check for additional conflicts and rerun loop
                  allconflicts = GetAllConflictSchedules(schedule, nonPreferredGroupSchedule);
                  
              }//end conflicts do exist for priority processing  while (allconflicts.Count > 0)

          } //end priority processing
          #endregion PriorityProcessing (3rd)

          #region delete nonpreferred group schedule (4th)
          //delete nonpreferred schedule as it is now sure that the preferred group schedule will be scheduled
          if (nonPreferredGroupSchedule != null)
          {
              //delete message for nonpreferred group schedule
              Log.Debug("Changing message for old schedule to conflict");
              int index = mymessage.GetTvMessageBySchedule(nonPreferredGroupSchedule,MessageType.Scheduled);
              if (index >= 0)
              {
                  xmlmessage changemessage = mymessage.GetTvMessageAtIndex(index);
                  changemessage.message = lng.TranslateString("Better Schedule Found In Preferred Group",78);
                  changemessage.type = MessageType.Conflict.ToString();
                  mymessage.ReplaceTvMessageAtIndex(index, changemessage);
              }

              LogDebug("Deleting nonPreferredGroupSchedule title=" + nonPreferredGroupSchedule.ProgramName, (int)LogSetting.DEBUG);
              nonPreferredGroupSchedule.Delete();
              mywish.i_scheduled--;

          }
          #endregion delete nonpreferred schedule (4th)

          //do not insert new code here 

          #region success: schedule and/or email
          //success: schedule and/or email         
          if ((_automaticrecording == true) && (mywish.action != lng.TranslateString("Email", 2701)) && (mywish.action != lng.TranslateString("View", 2703))) //schedule new recording
          {
              LogDebug("", (int)LogSetting.INFO);
              LogDebug("*******************************************************************************\n", (int)LogSetting.INFO);
              message = lng.TranslateString("TvWishList found program from Tv Wish [{0}]: {1}\nTvWishList did schedule the program", 79, (counter + 1).ToString(), mywish.name);
              LogDebug("*******************************************************************************\n", (int)LogSetting.INFO);
              LogDebug(message, (int)LogSetting.INFO);
              mymessage.addmessage(schedule, message, mywish.t_action, mywish.name, (int)XmlMessages.MessageEvents.SCHEDULE_FOUND, mywish.tvwishid, string.Empty);
              outputscheduletoresponse(schedule, (int)LogSetting.DEBUG);
              LogDebug("End of new schedule", (int)LogSetting.INFO);
              //mywish.i_scheduled++;


              if (mywish.i_keepuntil > 0) //handle days for keepuntil and convert back to date after schedule has been found
              {
                  schedule.KeepDate = DateTime.Now.AddDays(mywish.i_keepuntil);
                  schedule.Persist();
              }


              if (mywish.b_useFoldername)  //add folder move of recording
              {
                  if ((schedule.ProgramName.Contains(@"\")) || (mywish.name.Contains(@"\")))
                  {
                      LogDebug("", (int)LogSetting.ADDRESPONSE);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.ADDRESPONSE);
                      LogDebug("Failed to add folder " + schedule.ProgramName + " because program and name must not contain \" \\\"", (int)LogSetting.ADDRESPONSE);
                      LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.ADDRESPONSE);
                  }
                  else
                  {

                      //create new xml file for renaming recorded filename
                      try
                      {
                          XmlDocument xmlDoc = new XmlDocument();
                          string filename = TV_USER_FOLDER + @"\TvWishList\" + FileRenameXML;

                          try
                          {
                              xmlDoc.Load(filename);
                          }
                          catch (System.IO.FileNotFoundException)
                          {
                              XmlTextWriter xmlWriter = new XmlTextWriter(filename, System.Text.Encoding.UTF8);
                              xmlWriter.Formatting = Formatting.Indented;
                              xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
                              xmlWriter.WriteStartElement("AllJobs");
                              xmlWriter.Close();
                              xmlDoc.Load(filename);
                          }

                          XmlNode alljobs = xmlDoc.DocumentElement;
                          //XmlNode alljobs = xmlDoc.SelectSingleNode("/AllJobs");
                          XmlNode node = xmlDoc.CreateElement("Job");
                          AddAttribute(node, "ScheduleName", schedule.ProgramName);
                          AddAttribute(node, "Folder", mywish.name);

                          //episode mode with separate folder has been disabled above already

                          Double number = Convert.ToDouble(schedule.PreRecordInterval) * (-1);
                          DateTime absoluteStartTime = oneprogram.StartTime.AddMinutes(number);
                          AddAttribute(node, "Start", absoluteStartTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture));
                          LogDebug("Start=" + absoluteStartTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture), (int)LogSetting.DEBUG);

                          number = Convert.ToDouble(schedule.PostRecordInterval);
                          DateTime absoluteEndTime = oneprogram.EndTime.AddMinutes(number);
                          AddAttribute(node, "End", absoluteEndTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture));
                          LogDebug("End=" + absoluteEndTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture), (int)LogSetting.DEBUG);

                          AddAttribute(node, "idChannel", schedule.IdChannel.ToString());

                          alljobs.AppendChild(node);


                          xmlDoc.Save(filename);
                      }
                      catch (Exception exc)
                      {
                          LogDebug("Could not update " + TV_USER_FOLDER + @"\TvWishList\" + FileRenameXML, (int)LogSetting.ERROR);
                          LogDebug("Exception message was " + exc.Message, (int)LogSetting.ERROR);
                          string languagetext = lng.TranslateString("Fatal error - check the log file", 24);
                          labelmessage(languagetext, PipeCommands.StartEpg); //do not stop - do not flag as error
                          Thread.Sleep(ErrorWaitTime);
                      }
                  }
              }


          }
          else
          {
              if (mymessage.existsmessage(schedule, mywish.t_action) == false)  //new reminder - send email
              {
                  LogDebug("", (int)LogSetting.INFO);
                  LogDebug("*******************************************************************************\n", (int)LogSetting.INFO);
                  message = lng.TranslateString("TvWishList found program from Tv Wish [{0}]: {1}\nTvWishList did not schedule but is reminding you of the program", 80, (counter + 1).ToString(), mywish.name);
                  LogDebug("*******************************************************************************\n", (int)LogSetting.INFO);
                  LogDebug(message, (int)LogSetting.INFO);
                  schedule.ScheduleType = 9;  //bug in 1.2.0.15
                  mymessage.addmessage(schedule, message, mywish.t_action, mywish.name, (int)XmlMessages.MessageEvents.EMAIL_FOUND, mywish.tvwishid, string.Empty);
                  outputscheduletoresponse(schedule, (int)LogSetting.DEBUG);
                  schedule.ScheduleType = 0; //end bug 1.2.0.15
                  LogDebug("End of reminder a", (int)LogSetting.DEBUG);
                  schedule.Delete();
                  //mywish.i_scheduled++;

              }
              else //message does exist already - do not send email but return
              {
                  LogDebug("", (int)LogSetting.DEBUG);
                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.DEBUG);
                  LogDebug("Message does exist already - not included in email", (int)LogSetting.DEBUG);
                  LogDebug("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", (int)LogSetting.DEBUG);
                  outputscheduletoresponse(schedule, (int)LogSetting.DEBUG);
                  LogDebug("End of old reminder b", (int)LogSetting.DEBUG);
                  schedule.Delete();

              }
          }
          #endregion success: schedule and/or email

          return true;
      }


      /* 
       * Replace epgmarker for repeated text strings.
       * The text must be converted to upper first.
       * EPG marker can have a multiple entries separated by "|" (request from Noctambulist)
       */
      public string ProcessEpgMarker(string text)
      {
          text = text.ToUpper();

          if (_EpgMarker.Contains("|")==false)
          {
              try
              {                  
                  text = text.Replace(_EpgMarker.ToUpper(), "");
              }
              catch
              {
              }
              return text;
          }
          else
          {
              try
              {
                  string[] tokenarray = _EpgMarker.Split('|');
                  foreach (string token in tokenarray)
                  {
                      text = text.Replace(token.ToUpper(), "");
                  }
              }
              catch
              {
              }
              return text;
          }
          
      }

      [CLSCompliant(false)]
      public IList<Schedule> GetAllConflictSchedules(Schedule schedule, Schedule nonPreferredGroupSchedule)
      {
          IList<Schedule> allconflict_schedules = GetConflictingSchedules(schedule);
          Log.Debug("GetAllConflictSchedules(Schedule schedule, Schedule nonPreferredGroupSchedule) allconflict_schedules.Count=" + allconflict_schedules.Count.ToString());
          //remove nonPreferredGroupSchedule from conflicts as it will be deleted later on if scheduled
          for (int i = allconflict_schedules.Count - 1; i >= 0; i--)
          {
              if (allconflict_schedules[i] == nonPreferredGroupSchedule)
              {
                  LogDebug("Removing nonPreferredGroupSchedule from conflicts", (int)LogSetting.DEBUG);
                  allconflict_schedules.RemoveAt(i);
                  break;
              }
          }

          return allconflict_schedules;
      }


      /// <summary>
      /// Adds or updates an attribute of the xml file
      /// </summary>
      /// <param name="XmlNode node">xml node</param>
      /// <param name="string tagName">attribute name</param>
      /// <param name="string tagValue">attribute valuee</param>
      public void AddAttribute(XmlNode node, string tagName, string tagValue)
      {
          XmlAttribute attr = node.OwnerDocument.CreateAttribute(tagName);
          attr.InnerText = tagValue;
          node.Attributes.Append(attr);
      }

      //public bool episodeManagement(string newDescription, string oldDescription, string newEpisodePart, string oldEpisodePart, string newEpisodeName, string oldEpisodeName, string newSeriesNumber, string oldSeriesNumber, string newEpisodeNumber, string oldEpisodeNumber, bool useAutomatic, bool useDescription, bool usePart, bool useName, bool useNumbers)      
      public bool episodeManagement(string newDescription, string oldDescription, string newEpisodePart, string oldEpisodePart, string newEpisodeName, string oldEpisodeName, string newSeriesNumber, string oldSeriesNumber, string newEpisodeNumber, string oldEpisodeNumber, bool useDescription,  bool useName, bool useNumbers)
      {
          Log.Debug("EpisodeManagement");
          Log.Debug("oldDescription=" + oldDescription);
          Log.Debug("oldEpisodePart=" + oldEpisodePart);
          Log.Debug("oldEpisodeName=" + oldEpisodeName);
          Log.Debug("oldSeriesNumber=" + oldSeriesNumber);
          Log.Debug("oldEpisodeNumber=" + oldEpisodeNumber);

          Log.Debug("newDescription=" + newDescription);
          Log.Debug("newEpisodePart=" + newEpisodePart);
          Log.Debug("newEpisodeName=" + newEpisodeName);
          Log.Debug("newSeriesNumber=" + newSeriesNumber);
          Log.Debug("newEpisodeNumber=" + newEpisodeNumber);

          Log.Debug("useDescription=" + useDescription.ToString());
          Log.Debug("useNames=" + useName.ToString());
          Log.Debug("useNumbers=" + useNumbers.ToString());

          //convert to numbers
          int newSeriesNumberInt = -1;
          int newEpisodeNumberInt = -1;
          int oldSeriesNumberInt = -1;
          int oldEpisodeNumberInt = -1;

          try
          {
              if (newSeriesNumber != string.Empty)
              {
                  string[] tokens = newSeriesNumber.Split('\\', '/');
                  newSeriesNumberInt = Convert.ToInt32(tokens[0]);
                  Log.Debug("newSeriesNumberInt=" + newSeriesNumberInt.ToString());
              }
              
              if (newEpisodeNumber != string.Empty)
              {
                  string[] tokens = newEpisodeNumber.Split('\\', '/');
                  newEpisodeNumberInt = Convert.ToInt32(tokens[0]);
                  Log.Debug("newEpisodeNumberInt=" + newEpisodeNumberInt.ToString());
              }

              if (oldSeriesNumber != string.Empty)
              {
                  string[] tokens = oldSeriesNumber.Split('\\', '/');
                  oldSeriesNumberInt = Convert.ToInt32(tokens[0]);
                  Log.Debug("oldSeriesNumberInt=" + oldSeriesNumberInt.ToString());
              }

              if (oldEpisodeNumber != string.Empty)
              {
                  string[] tokens = oldEpisodeNumber.Split('\\', '/');
                  oldEpisodeNumberInt = Convert.ToInt32(tokens[0]);
                  Log.Debug("oldEpisodeNumberInt=" + oldEpisodeNumberInt.ToString());
              }

          }
          catch (Exception exc)
          {
              Log.Error("Error in converting episode or series number to integer in episodemanagement - exception is "+exc.Message);
              Log.Debug("episodeManagement() false EpisodeNumber or SeriesNumber could not be converted to integer");
              string languagetext = lng.TranslateString("Fatal error - check the log file", 24);
              labelmessage(languagetext, PipeCommands.StartEpg); //do not stop - do not flag as error
              Thread.Sleep(ErrorWaitTime);
              return false;
          }


          if (useNumbers && useName && (newSeriesNumber != string.Empty) && (newEpisodeNumber != string.Empty) && (newEpisodeName != string.Empty))//check for seriesnumber and episodenumber and episodename only for true
          {
              if ((newEpisodeNumberInt == oldEpisodeNumberInt) && (newSeriesNumberInt == oldSeriesNumberInt) && (newEpisodeName == oldEpisodeName))
              {
                  Log.Debug("episodeManagement() true EpisodeNumber and SeriesNumber and EpisodeName");
                  return true; //series number and episode number and episode name do match
              }    
              //do not ask for mismatch, this will be covered by later criteria
          }

          if (useNumbers) //check for seriesnumber and episodenumber must be first
          {
              if ((newSeriesNumberInt == oldSeriesNumberInt) && (newSeriesNumber != string.Empty))
              {
                  if ((newEpisodeNumberInt == oldEpisodeNumberInt)&&(newEpisodeNumber != string.Empty) ) 
                  {
                      Log.Debug("episodeManagement() true EpisodeNumber and SeriesNumber");
                      return true; //series number and episode number do match
                  }
                  else if ((newEpisodeNumberInt != oldEpisodeNumberInt) && (newEpisodeNumber != string.Empty) && (oldEpisodeNumber != string.Empty))
                  {
                      Log.Debug("episodeManagement() false EpisodeNumber");
                      return false; //true mismatch of episodenumber
                  }
              }// if series number exists but there is no episode number it will not skip.
              else if ((newSeriesNumberInt != oldSeriesNumberInt) && (newSeriesNumber != string.Empty) && (oldSeriesNumber != string.Empty))
              {
                  Log.Debug("episodeManagement() false SeriesNumber");
                  return false; //true mismatch of series number
              }
          }

          if (useName) //check for episode name and episode part 
          {
              if ((newEpisodeName == oldEpisodeName) && (newEpisodeName != string.Empty))
              {

                  if ((newEpisodePart == string.Empty) && (oldEpisodePart != string.Empty)) //new if condition for empty episode parts
                  { //check for description
                      if ((useDescription) && (newDescription == oldDescription))
                      {
                          Log.Debug("episodeManagement() true description and episodename and empty episode part");
                          return true; //Description and Episode name is matching
                      }
                      else if ((useDescription) && (newDescription != oldDescription))
                      {
                          Log.Debug("episodeManagement() false description for true episode name and empty episode part");
                          return false; //Description is not matching
                      }
                      else
                      {
                          Log.Debug("episodeManagement() true episodename not checking description and empty episode part");
                          return true; //Episode name is matching
                      }
                  }//end new if condition
                  else if ((newEpisodePart == oldEpisodePart) || (newEpisodePart == string.Empty) || (oldEpisodePart == string.Empty))  //new || (oldEpisodePart == string.Empty)
                  {                     
                      Log.Debug("episodeManagement() true EpisodeName and EpisodePart");
                      return true;  //Episode name is matching and episode part is matching or is empty
                  }
                  else if ((newEpisodePart != oldEpisodePart) && (newEpisodePart != string.Empty) && (oldEpisodePart != string.Empty))
                  {
                      Log.Debug("episodeManagement() false EpisodePart");
                      return false; //true mismatch of episodepart
                  }
              }
              else if ((newEpisodeName != oldEpisodeName) && (newEpisodeName != string.Empty) && (oldEpisodeName != string.Empty))
              {
                  Log.Debug("episodeManagement() false EpisodeName");
                  return false; //true mismatch of episodename
              }
          }

          if (useDescription) //check for description must be last 
          {
              if ((newDescription == oldDescription) && (newDescription != string.Empty) )
              {
                  Log.Debug("episodeManagement() true description");
                  return true; //Description is matching
              }

          }

          if (!useNumbers && !useName && !useDescription) //None
          {
              Log.Debug("episodeManagement() true no selection of name, number or description (none)");
              return true; //enables the user to skip just for identical title if nothing is checked, skip repeated checkbox still needed
          }

          Log.Debug("episodeManagement() false left");
          return false;  //no unique matches or mismatches  do not know  
      }



      //public bool episodeManagement(string newDescription, string oldDescription, string newEpisodePart, string oldEpisodePart, string newEpisodeName, string oldEpisodeName, string newSeriesNumberInt, string oldSeriesNumberInt, string newEpisodeNumberInt, string oldEpisodeNumberInt, bool useAutomatic, bool useDescription, bool usePart, bool useName, bool useNumbers)      
      //same as episodeManagement but without any String.Empty
      public bool episodeManagementEmptyString(string newDescription, string oldDescription, string newEpisodePart, string oldEpisodePart, string newEpisodeName, string oldEpisodeName, string newSeriesNumberInt, string oldSeriesNumberInt, string newEpisodeNumberInt, string oldEpisodeNumberInt, bool useDescription, bool useName, bool useNumbers)
      {
          
          if (useNumbers && useName )//check for seriesnumber and episodenumber and episodename only for true
          {
              if ((newEpisodeNumberInt == oldEpisodeNumberInt) && (newSeriesNumberInt == oldSeriesNumberInt) && (newEpisodeName == oldEpisodeName))
              {
                  Log.Debug("episodeManagementEmptyString() true EpisodeNumber and SeriesNumber and EpisodeName");
                  return true; //series number and episode number and episode name do match
              }
              //do not ask for mismatch, this will be covered by later criteria
          }

          if (useNumbers) //check for seriesnumber and episodenumber must be first
          {
              if (newSeriesNumberInt == oldSeriesNumberInt) 
              {
                  if (newEpisodeNumberInt == oldEpisodeNumberInt) 
                  {
                      Log.Debug("episodeManagementEmptyString() true EpisodeNumber and SeriesNumber");
                      return true; //series number and episode number do match
                  }
                  else if (newEpisodeNumberInt != oldEpisodeNumberInt) 
                  {
                      Log.Debug("episodeManagementEmptyString() false EpisodeNumber");
                      return false; //true mismatch of episodenumber
                  }
              }// if series number exists but there is no episode number it will not skip.
              else if (newSeriesNumberInt != oldSeriesNumberInt) 
              {
                  Log.Debug("episodeManagementEmptyString() false SeriesNumber");
                  return false; //true mismatch of series number
              }
          }

          if (useName) //check for episode name and episode part 
          {
              if (newEpisodeName == oldEpisodeName) 
              {
                  if ((newEpisodePart == oldEpisodePart) || (newEpisodePart == string.Empty))
                  {
                      Log.Debug("episodeManagementEmptyString() true EpisodeName and EpisodePart");
                      return true;  //Episode name is matching and episode part is matching or is empty
                  }
                  else if (newEpisodePart != oldEpisodePart) 
                  {
                      Log.Debug("episodeManagementEmptyString() false EpisodePart");
                      return false; //true mismatch of episodepart
                  }
              }
              else if (newEpisodeName != oldEpisodeName) 
              {
                  Log.Debug("episodeManagementEmptyString() false EpisodeName");
                  return false; //true mismatch of episodename
              }
          }

          if (useDescription) //check for description must be last 
          {
              if (newDescription == oldDescription) 
              {
                  Log.Debug("episodeManagementEmptyString() true description");
                  return true; //Description is matching
              }

          }

          if (!useNumbers && !useName && !useDescription)
          {
              Log.Debug("episodeManagementEmptyString() true no selection of name, number or description (none)");
              return true; //enables the user to skip just for identical title if nothing is checked, skip repeated checkbox still needed
          }

          Log.Debug("episodeManagementEmptyString() false left");
          return false;  //no unique matches or mismatches  do not know  
      }


      
      //-------------------------------------------------------------------------------------------------------------        
      // output a single schedule with all parameters
      //------------------------------------------------------------------------------------------------------------- 
      [CLSCompliant(false)]        
      public void outputscheduletoresponse(Schedule schedule, Int32 this_setting)
      {
          LogDebug("*****************************SCHEDULE****************************************", this_setting);
          LogDebug("ProgramName=           " + schedule.ProgramName, this_setting);
          try
          {

              Channel channel = Channel.Retrieve(schedule.IdChannel);
              LogDebug("Channel=               " + channel.DisplayName, this_setting);

          }
          catch
          {
              LogDebug("Channel ID=            " + schedule.IdChannel, this_setting);
          }
          LogDebug("START_TIME=            " + schedule.StartTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture), this_setting);

          LogDebug("END_TIME=              " + schedule.EndTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture), this_setting);

          string schedtype = "";
          if (schedule.ScheduleType == 0)
              schedtype = "Once";
          else if (schedule.ScheduleType == 3)
              schedtype = "Always on this channel";
          else if (schedule.ScheduleType == 4)
              schedtype = "Always on all channels";
          else if (schedule.ScheduleType == 2)
              schedtype = "Weekly at this time";
          else if (schedule.ScheduleType == 1)
              schedtype = "Daily at this time";
          else if (schedule.ScheduleType == 6)
              schedtype = "Mon-Fri";
          else if (schedule.ScheduleType == 5)
              schedtype = "Weekends";
          else if (schedule.ScheduleType == 7)
              schedtype = "Weekly on this channel";
          else if (schedule.ScheduleType == 9)
              schedtype = "Not Scheduled";
          else
              schedtype = schedule.ScheduleType.ToString();

          LogDebug("ScheduleType=          " + schedtype + " number=" + schedule.ScheduleType.ToString(), this_setting);


          

          
              try
              {

#if (TV11 || TV12)

                  //Debug only for 1.2
                  LogDebug("BitRateMode=" + schedule.BitRateMode.ToString(), this_setting);
                  LogDebug("CacheKey=" + schedule.CacheKey.ToString(), this_setting);
                  LogDebug("Canceled=" + schedule.Canceled.ToString(), this_setting);
                  LogDebug("Directory=" + schedule.Directory.ToString(), this_setting);
                  LogDebug("DoesUseEpisodeManagement=" + schedule.DoesUseEpisodeManagement.ToString(), this_setting);
                  LogDebug("IdParentSchedule=" + schedule.IdParentSchedule.ToString(), this_setting);
                  LogDebug("IdSchedule=" + schedule.IdSchedule.ToString(), this_setting);
                  LogDebug("IsChanged=" + schedule.IsChanged.ToString(), this_setting);
                  LogDebug("IsManual=" + schedule.IsManual.ToString(), this_setting);
                  LogDebug("IsPersisted=" + schedule.IsPersisted.ToString(), this_setting);
                  LogDebug("KeepDate=" + schedule.KeepDate.ToString(), this_setting);
                  LogDebug("KeepMethod=" + schedule.KeepMethod.ToString(), this_setting);
                  LogDebug("MaxAirings=" + schedule.MaxAirings.ToString(), this_setting);
                  LogDebug("PostRecordInterval=" + schedule.PostRecordInterval.ToString(), this_setting);
                  LogDebug("PreRecordInterval=" + schedule.PreRecordInterval.ToString(), this_setting);
                  LogDebug("Priority=" + schedule.Priority.ToString(), this_setting);
                  LogDebug("Quality=" + schedule.Quality.ToString(), this_setting);
                  LogDebug("QualityType=" + schedule.QualityType.ToString(), this_setting);
                  LogDebug("RecommendedCard=" + schedule.RecommendedCard.ToString(), this_setting);
                  LogDebug("Series=" + schedule.Series.ToString(), this_setting);
                  LogDebug("SessionBroker=" + schedule.SessionBroker.ToString(), this_setting);
                  LogDebug("ValidationMessages=" + schedule.ValidationMessages.ToString(), this_setting);
                  //end debug


                  Program myprogram = Program.RetrieveByTitleTimesAndChannel(schedule.ProgramName,schedule.StartTime,schedule.EndTime,schedule.IdChannel);
                  LogDebug("Description=          " + myprogram.Description, this_setting);
                  LogDebug("Genre=           " + myprogram.Genre, this_setting);
                  LogDebug("Classification=           " + myprogram.Classification, this_setting);
                  LogDebug("EpisodeName=           " + myprogram.EpisodeName, this_setting);
                  LogDebug("EpisodeNumber=           " + myprogram.EpisodeNumber, this_setting);
                  LogDebug("EpisodeNum=           " + myprogram.EpisodeNum, this_setting);
                  LogDebug("EpisodePart=           " + myprogram.EpisodePart, this_setting);
                  LogDebug("OriginalAirDate=           " + myprogram.OriginalAirDate, this_setting);
                  LogDebug("ParentalRating=           " + myprogram.ParentalRating, this_setting);
                  LogDebug("SeriesNum=           " + myprogram.SeriesNum, this_setting);
                  LogDebug("StarRating=           " + myprogram.StarRating, this_setting);
#else
              LogDebug("Channel ID=            " + schedule.IdChannel, this_setting);
#endif
              }
              catch (Exception exc)
              {
                  LogDebug("Error in retrieving EPG data for program" + schedule.ProgramName,(int)LogSetting.ERROR );
                  LogDebug("Exception message was " + exc.Message,(int)LogSetting.ERROR );
              }

              LogDebug("*****************************END SCHEDULE****************************************", this_setting);
          LogDebug("", this_setting);
      }

      //-------------------------------------------------------------------------------------------------------------        
      // output a single schedule with all parameters
      //------------------------------------------------------------------------------------------------------------- 
      [CLSCompliant(false)]
      public void outputprogramresponse(Program program, Int32 this_setting)
      {
          LogDebug("***************************PROGRAM********************************************", this_setting);
          LogDebug("ProgramName=           " + program.Title, this_setting);
          LogDebug("Description=           " + program.Description, this_setting);
          LogDebug("Genre=           " + program.Genre, this_setting);
          LogDebug("Classification=           " + program.Classification, this_setting);
          LogDebug("EpisodeNum=           " + program.EpisodeNum, this_setting);
#if(TV11 || TV12)
          LogDebug("EpisodeName=           " + program.EpisodeName, this_setting);
          LogDebug("EpisodeNumber=           " + program.EpisodeNumber, this_setting);         
          LogDebug("EpisodePart=           " + program.EpisodePart, this_setting);
#endif
          LogDebug("OriginalAirDate=           " + program.OriginalAirDate, this_setting);
          LogDebug("ParentalRating=           " + program.ParentalRating, this_setting);
          LogDebug("SeriesNum=           " + program.SeriesNum, this_setting);
          LogDebug("StarRating=           " + program.StarRating, this_setting);
          try
          {
              Channel channel = Channel.Retrieve(program.IdChannel);
              LogDebug("Channel=               " + channel.DisplayName, this_setting);
          }
          catch
          {
              LogDebug("Channel ID=            " + program.IdChannel, this_setting);
          }
          LogDebug("START_TIME=            " + program.StartTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture), this_setting);

          LogDebug("END_TIME=              " + program.EndTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture), this_setting);

          LogDebug("***************************END PROGRAM********************************************", this_setting);

          LogDebug("", this_setting);
      }


      //-------------------------------------------------------------------------------------------------------------        
      // output a single recording with all parameters
      //------------------------------------------------------------------------------------------------------------- 
      [CLSCompliant(false)]
      public void outputrecordingresponse(Recording recording, Int32 this_setting)
      { 

         /* public string Description { get; set; }
        public DateTime EndTime { get; set; }
        public string EpisodeName { get; set; }
        public string EpisodeNum { get; set; }
        public string EpisodeNumber { get; }
        public string EpisodePart { get; set; }
        public string FileName { get; set; }
        public string Genre { get; set; }
        public int IdChannel { get; set; }
        public int IdRecording { get; }
        public int Idschedule { get; set; }
        public int IdServer { get; set; }
        public bool IsChanged { get; }
        public bool IsManual { get; }
        public bool IsRecording { get; set; }
        public int KeepUntil { get; set; }
        public DateTime KeepUntilDate { get; set; }
        public string SeriesNum { get; set; }
        public bool ShouldBeDeleted { get; }
        public DateTime StartTime { get; set; }
        public int StopTime { get; set; }
        public int TimesWatched { get; set; }
        public string Title { get; set; }*/


          LogDebug("*************************RECORDING*******************************************", this_setting);
          LogDebug("ProgramName=           " + recording.Title, this_setting);
          LogDebug("Description=           " + recording.Description, this_setting);
          LogDebug("Genre=           " + recording.Genre, this_setting);
          LogDebug("FileName=           " + recording.FileName, this_setting);
          
#if(TV11 || TV12)
          LogDebug("EpisodeNum=           " + recording.EpisodeNum, this_setting);
          LogDebug("EpisodeName=           " + recording.EpisodeName, this_setting);
          LogDebug("EpisodeNumber=           " + recording.EpisodeNumber, this_setting);
          LogDebug("EpisodePart=           " + recording.EpisodePart, this_setting);
          LogDebug("SeriesNum=           " + recording.SeriesNum, this_setting);
          LogDebug("Idschedule=           " + recording.Idschedule, this_setting);
#endif
          LogDebug("KeepUntil=           " + recording.KeepUntil, this_setting);
          LogDebug("KeepUntilDate=           " + recording.KeepUntilDate, this_setting);
          
          LogDebug("IdRecording=           " + recording.IdRecording, this_setting);
          
          try
          {
              Channel channel = Channel.Retrieve(recording.IdChannel);
              LogDebug("Channel=               " + channel.DisplayName, this_setting);
          }
          catch
          {
              LogDebug("Channel ID=            " + recording.IdChannel, this_setting);
          }
          LogDebug("START_TIME=            " + recording.StartTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture), this_setting);

          LogDebug("END_TIME=              " + recording.EndTime.ToString("yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture), this_setting);


          LogDebug("*************************END RECORDING*******************************************", this_setting);
          LogDebug("", this_setting);
      }


      //-------------------------------------------------------------------------------------------------------------        
      //log handling for debug, error and addmessage (return mals)
      //-------------------------------------------------------------------------------------------------------------                
      public void LogDebug(string text, int field)
      {
          //trigger message event

          if (field == (int)LogSetting.INFO)
          {
              Log.Debug("TvWishList EpgClass: " + text);
              //if (newmessage != null)
              //    newmessage(text, field);

          }
          else if ((field == (int)LogSetting.DEBUG) && (DEBUG == true))
          {
              if (DEBUG == true)
              {
                  Log.Debug("TvWishList EpgClass: " + text);
                  //if (newmessage != null)
                  //    newmessage(text, field);
              }

          }
          else if (field == (int)LogSetting.ERROR)
          {
              Log.Error("TvWishList EpgClass: " + text);
              Log.Debug("TvWishList EpgClass: " + text);
          }
          else if (field == (int)LogSetting.ERRORONLY)
          {
              Log.Error("TvWishList EpgClass: " + text);
              //if (newmessage != null)
              //    newmessage(text, field);

          }
          else
          {
              //Log.Error("TvWishList Error MailClass: Unknown message Code " + field.ToString(), (int)LogSetting.ERROR);
          }
      }



      


      //-------------------------------------------------------------------------------------------------------------        
      // collects all conflicts in LIST <Schedule> conflicts and returns the list for the existing schedule
      //------------------------------------------------------------------------------------------------------------- 
      [CLSCompliant(false)]
      public List<Schedule> GetConflictingSchedules(Schedule rec)
      {   
          LogDebug("GetConflictingSchedules: Schedule = " + rec.ToString(), (int)LogSetting.DEBUG);
#if(TV100)
          IList schedulesList = Schedule.ListAll();
          IList cards = Card.ListAll();
#elif(TV101 || TV11 || TV12)
          IList<Schedule> schedulesList = Schedule.ListAll();
          IList<Card> cards = Card.ListAll();
#endif

          List<Schedule> conflicts = new List<Schedule>();
          
          if (cards.Count == 0)
          {
              return conflicts;
          }
          //LogDebug("GetConflictingSchedules: Cards.Count =" + cards.Count.ToString(), (int)LogSetting.DEBUG);

          List<Schedule>[] cardSchedules = new List<Schedule>[cards.Count];
          for (int i = 0; i < cards.Count; i++)
          {
              cardSchedules[i] = new List<Schedule>();
          }

          // GEMX: Assign all already scheduled timers to cards. Assume that even possibly overlapping schedulues are ok to the user,
          // as he decided to keep them before. That's why they are in the db
          foreach (Schedule schedule in schedulesList)
          {

              //huha change 
              if (schedule == rec)
              {
                  continue;
              }
              //end huha 
               
              List<Schedule> episodes = GetRecordingTimes(schedule);
              foreach (Schedule episode in episodes)
              {
                  if (DateTime.Now > episode.EndTime)
                  {
                      continue;
                  }
                  if (episode.IsSerieIsCanceled(episode.StartTime))
                  {
                      continue;
                  }
                  Schedule overlapping;
                  AssignSchedulesToCard(episode, cardSchedules, out overlapping,DEBUG);
              }
          }

          List<Schedule> newEpisodes = GetRecordingTimes(rec);
          foreach (Schedule newEpisode in newEpisodes)
          {
              if (DateTime.Now > newEpisode.EndTime)
              {
                  continue;
              }
              if (newEpisode.IsSerieIsCanceled(newEpisode.StartTime))
              {
                  continue;
              }
              Schedule overlapping=null;
              if (!AssignSchedulesToCard(newEpisode, cardSchedules, out overlapping,DEBUG))
              {
                  LogDebug("GetConflictingSchedules: newEpisode can not be assigned to a card = " + newEpisode.ToString(), (int)LogSetting.DEBUG);
                  if (overlapping != null)
                  {
                      LogDebug("Overlapping schedule is:", (int)LogSetting.DEBUG);
                      outputscheduletoresponse(overlapping, (int)LogSetting.DEBUG);
                      conflicts.Add(overlapping);
                  }
                  else
                  {
                      LogDebug("Overlapping schedule is not defined", (int)LogSetting.DEBUG);
                  }                 
                    
              }

              
          }
          return conflicts;
      }


      //-------------------------------------------------------------------------------------------------------------        
      // assigns a single schedule to the card
      //------------------------------------------------------------------------------------------------------------- 
      private static bool AssignSchedulesToCard(Schedule schedule, List<Schedule>[] cardSchedules, out Schedule overlappingSchedule,bool Debug)
      {
          overlappingSchedule = null;
          //if (Debug==true)
          //    Log.Debug("AssignSchedulesToCard: schedule = " + schedule.ToString());


#if(TV100)
          IList cards = Card.ListAll();
#elif(TV101 || TV11 || TV12)
          IList<Card> cards = Card.ListAll();
#endif

          bool assigned = false;
          int count = 0;
          foreach (Card card in cards)
          {
              //if (Debug == true)
              //  Log.Debug("Working on card: "+card.IdCard.ToString()+" ID Channel="+schedule.IdChannel.ToString());
              if (card.canViewTvChannel(schedule.IdChannel))
              {
                  // checks if any schedule assigned to this cards overlaps current parsed schedule
                  bool free = true;
                 // if (Debug == true)
                 //   Log.Debug("card can view channel - free=true");
                  foreach (Schedule assignedSchedule in cardSchedules[count])
                  {
                      //if (Debug == true)
                      //    Log.Debug("AssignSchedulesToCard: card {0}, ID = {1} has schedule = " + assignedSchedule, count, card.IdCard);
                      if (schedule.IsOverlapping(assignedSchedule))
                      {
                          //if (Debug == true)
                          //    Log.Debug("schedule is overlapping - checking same transponder");

                          if (!(schedule.isSameTransponder(assignedSchedule) && card.supportSubChannels))
                          {
                              overlappingSchedule = assignedSchedule;
                              if (Debug == true)
                                  Log.Debug("AssignSchedulesToCard: overlapping with " + assignedSchedule + " on card {0}, ID = {1}", card.IdCard);
                              free = false;
                              break;
                          }

                      }
                  }
                  if (free)
                  {
                      if (Debug == true)
                          Log.Debug("AssignSchedulesToCard: free on card "+count.ToString()+", ID = "+ card.IdCard.ToString());
                      cardSchedules[count].Add(schedule);
                      assigned = true;
                      break;
                  }
              }
              count++;
          }
          if (!assigned)
          {
              return false;
          }

          return true;
      }

      [CLSCompliant(false)]
      //-------------------------------------------------------------------------------------------------------------        
      // gets the recording time of a schedule
      //------------------------------------------------------------------------------------------------------------- 
      public List<Schedule> GetRecordingTimes(Schedule rec)
      {
          return GetRecordingTimes(rec, 10);
      }


      [CLSCompliant(false)]
      //-------------------------------------------------------------------------------------------------------------        
      // gets the recording time of a schedule
      //-------------------------------------------------------------------------------------------------------------
      public List<Schedule> GetRecordingTimes(Schedule rec, int days)
      {
          TvBusinessLayer layer = new TvBusinessLayer();
          List<Schedule> recordings = new List<Schedule>();

          DateTime dtDay = DateTime.Now;
          if (rec.ScheduleType == (int)ScheduleRecordingType.Once)
          {
              recordings.Add(rec);
              return recordings;
          }

          if (rec.ScheduleType == (int)ScheduleRecordingType.Daily)
          {
              for (int i = 0; i < days; ++i)
              {
                  Schedule recNew = rec.Clone();
                  recNew.ScheduleType = (int)ScheduleRecordingType.Once;
                  recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute,
                                                  0);
                  if (rec.EndTime.Day > rec.StartTime.Day)
                  {
                      dtDay = dtDay.AddDays(1);
                  }
                  recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
                  if (rec.EndTime.Day > rec.StartTime.Day)
                  {
                      dtDay = dtDay.AddDays(-1);
                  }
                  recNew.Series = true;
                  if (recNew.StartTime >= DateTime.Now)
                  {
                      if (rec.IsSerieIsCanceled(recNew.StartTime))
                      {
                          recNew.Canceled = recNew.StartTime;
                      }
                      recordings.Add(recNew);
                  }
                  dtDay = dtDay.AddDays(1);
              }
              return recordings;
          }

          if (rec.ScheduleType == (int)ScheduleRecordingType.WorkingDays)
          {

#if (TV100 || TV101 || TV12)
              for (int i = 0; i < days; ++i)
              {
                  if (dtDay.DayOfWeek != DayOfWeek.Saturday && dtDay.DayOfWeek != DayOfWeek.Sunday)
#elif(TV11)
              WeekEndTool weekEndTool = Setting.GetWeekEndTool();
              for (int i = 0; i < days; ++i)
              {
                  if (weekEndTool.IsWorkingDay(dtDay.DayOfWeek))  
#endif             
                  {
                      Schedule recNew = rec.Clone();
                      recNew.ScheduleType = (int)ScheduleRecordingType.Once;
                      recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute,
                                                      0);
                      if (rec.EndTime.Day > rec.StartTime.Day)
                      {
                          dtDay = dtDay.AddDays(1);
                      }
                      recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
                      if (rec.EndTime.Day > rec.StartTime.Day)
                      {
                          dtDay = dtDay.AddDays(-1);
                      }
                      recNew.Series = true;
                      if (rec.IsSerieIsCanceled(recNew.StartTime))
                      {
                          recNew.Canceled = recNew.StartTime;
                      }
                      if (recNew.StartTime >= DateTime.Now)
                      {
                          recordings.Add(recNew);
                      }
                  }
                  dtDay = dtDay.AddDays(1);
              }
              return recordings;
          }

          if (rec.ScheduleType == (int)ScheduleRecordingType.Weekends)
          {
#if(TV100)
              IList progList = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(days), rec.ProgramName,rec.ReferencedChannel());
              foreach (Program prog in progList)
              {
                  if ((rec.IsRecordingProgram(prog, false)) &&
                      (prog.StartTime.DayOfWeek == DayOfWeek.Saturday || prog.StartTime.DayOfWeek == DayOfWeek.Sunday))
#elif(TV101 || TV12)
              IList<Program> progList = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(days), rec.ProgramName,rec.ReferencedChannel());
              foreach (Program prog in progList)
              {
                  if ((rec.IsRecordingProgram(prog, false)) &&
                      (prog.StartTime.DayOfWeek == DayOfWeek.Saturday || prog.StartTime.DayOfWeek == DayOfWeek.Sunday))

#elif(TV11)
              IList<Program> progList = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(days), rec.ProgramName, rec.ReferencedChannel());
              WeekEndTool weekEndTool = Setting.GetWeekEndTool();
              foreach (Program prog in progList)
              {
                  if ((rec.IsRecordingProgram(prog, false)) &&
                      (weekEndTool.IsWeekend(prog.StartTime.DayOfWeek)))
#endif
              

              
                  {
                      Schedule recNew = rec.Clone();
                      recNew.ScheduleType = (int)ScheduleRecordingType.Once;
                      recNew.StartTime = prog.StartTime;
                      recNew.EndTime = prog.EndTime;
                      recNew.Series = true;

                      if (rec.IsSerieIsCanceled(recNew.StartTime))
                      {
                          recNew.Canceled = recNew.StartTime;
                      }
                      recordings.Add(recNew);
                  }
              }
              return recordings;


              




          }
          if (rec.ScheduleType == (int)ScheduleRecordingType.Weekly)
          {
              for (int i = 0; i < days; ++i)
              {
                  if ((dtDay.DayOfWeek == rec.StartTime.DayOfWeek) && (dtDay.Date >= rec.StartTime.Date))
                  {
                      Schedule recNew = rec.Clone();
                      recNew.ScheduleType = (int)ScheduleRecordingType.Once;
                      recNew.StartTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.StartTime.Hour, rec.StartTime.Minute,
                                                      0);
                      if (rec.EndTime.Day > rec.StartTime.Day)
                      {
                          dtDay = dtDay.AddDays(1);
                      }
                      recNew.EndTime = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, rec.EndTime.Hour, rec.EndTime.Minute, 0);
                      if (rec.EndTime.Day > rec.StartTime.Day)
                      {
                          dtDay = dtDay.AddDays(-1);
                      }
                      recNew.Series = true;
                      if (rec.IsSerieIsCanceled(recNew.StartTime))
                      {
                          recNew.Canceled = recNew.StartTime;
                      }
                      if (recNew.StartTime >= DateTime.Now)
                      {
                          recordings.Add(recNew);
                      }
                  }
                  dtDay = dtDay.AddDays(1);
              }
              return recordings;
          }

#if(TV100)
          IList programs = rec.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnThisChannel
                                      ? layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(days), rec.ProgramName,
                                                                    rec.ReferencedChannel())
                                      : layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(days), rec.ProgramName, null);
#elif(TV101 || TV11 || TV12)
          IList<Program> programs = rec.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnThisChannel
                                      ? layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(days), rec.ProgramName,
                                                                    rec.ReferencedChannel())
                                      : layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(days), rec.ProgramName, null);
#endif

          foreach (Program prog in programs)
          {
              if (rec.IsRecordingProgram(prog, false))
              {
                  Schedule recNew = rec.Clone();
                  recNew.ScheduleType = (int)ScheduleRecordingType.Once;
                  recNew.IdChannel = prog.IdChannel;
                  recNew.StartTime = prog.StartTime;
                  recNew.EndTime = prog.EndTime;
                  recNew.Series = true;
                  if (rec.IsSerieIsCanceled(recNew.StartTime))
                  {
                      recNew.Canceled = recNew.StartTime;
                  }
                  recordings.Add(recNew);
              }
          }
          return recordings;
      }


      public void labelmessage(string text, PipeCommands type)
      {
          if (newlabelmessage != null)
          {

              LogDebug("Creating new event message: " + text + " for type=" + type.ToString(), (int)LogSetting.DEBUG);
              newlabelmessage(text, type);            
          }

      }


  }




  //**************************************************************************************************************************************

  //*****************************
  // the SendTvServerEmail class
  //*****************************

  public class SendTvServerEmail
  {
# region Declarations
      string s_userName = "";
      string s_passWord = "";
      string s_server = "";
      string s_from = "";
      string s_to = "";
      string s_subject = "";
      string s_body = "";
      bool s_busy = false;
      bool s_HtmlFormat = false;
      
      int s_serverPort = 0;
      bool s_secureSSL = true;

      
      bool s_error = false; 
      string s_errormessage = "";

      bool DEBUG = false;

      
      


      public bool Debug
      {
          get { return DEBUG; }
          set { DEBUG = value; }
      }

# endregion Declarations

# region Properties
      
        public string ServerAddress
        {
            get { return s_server; }
            set { s_server = value; }
        }

        public int Port
        {
            get { return s_serverPort; }
            set { s_serverPort = value;}
        }

        public string Username
        {
            get { return s_userName; }
            set { s_userName = value; }
        }

        public string Password
        {
            get { return s_passWord; }
            set { s_passWord = value;}
        }

        public string From
        {
            get { return s_from; }
            set { s_from = value;}
        }

        public bool SecureSSL
        {
            get { return s_secureSSL; }
            set { s_secureSSL = value;}
        }

        public string To
        {
            get { return s_to; }
            set { s_to = value; }
        }

        public string Subject
        {
            get { return s_subject; }
            set { s_subject = value; }
        }

        public string Body
        {
            get { return s_body; }
            set { s_body = value; }
        }

        public bool HtmlFormat
        {
            get { return s_HtmlFormat; }
            set { s_HtmlFormat = value; }
        }


        public string ErrorMessage
        {
            get { return s_errormessage; }
        }

        public bool Error
        {
            get { return s_error; }
        }




# endregion Properties

# region Constructor
    public SendTvServerEmail (string server, int port, bool secure,  string user, string password, string from)
    {
        s_server = server;
        s_serverPort = port;
        s_secureSSL = secure;
        s_userName = user;
        s_passWord = password;
        s_from = from;
    }

#endregion Constructor

#region Methods
    public bool SendNewEmail(string to, string subject, string body)
    {
        
        s_to = to;
        s_subject = subject;
        s_body = body;


        
        
        //wait for old sent
        while (s_busy == true)
        {
            System.Threading.Thread.Sleep(500); //wait 0.5s
            if (DEBUG==true)
                Log.Debug("Waiting for old email sent to complete");
        }


        if (DEBUG == true)
            Log.Debug("Starting email thread");
        s_busy = true;
        System.Threading.Thread th = new System.Threading.Thread(SendEmailThread);
        th.Start();


        for (int i = 1; i < 300; i++)  //max wait 150s
        {
            System.Threading.Thread.Sleep(500); //wait 0.5s
            
            if (s_busy == false)
            {
                if (s_errormessage == "NOERROR")
                {
                    if (DEBUG == true)
                        Log.Debug("Email Completed without errors");
                    return true;
                }
                else
                {

                    if (DEBUG == true)
                        Log.Debug("Email Completed with error");

                    System.Threading.Thread.Sleep(2000); //wait 2s
                    Log.Error("Email Error- " + s_errormessage);
                    
                    return false;
                }
            }
        }

        //timeout try to kill thread
        if (DEBUG == true)
            Log.Debug("Timeout for sending email - try to kill thread");
        th.Abort();
        System.Threading.Thread.Sleep(2000); //wait 2s
        Log.Error("Timeout error for sending email");

        s_busy = false;


        
        
        return false;
    }


    public void SendEmailThread()
    {
        s_error = true;
        s_errormessage = "TIMEOUT";
        try
        {
            
            Log.Error("begin debug");
            Log.Error("s_username is " + s_userName);
            //Log.Error("s_passWord is " + s_passWord);
            Log.Error("s_server is " + s_server);
            Log.Error("s_serverPort is " + s_serverPort.ToString());
            Log.Error("Email with subject is " + s_subject );
            Log.Error(" sent to " + s_to);
            Log.Error(" sent from " + s_from);
            Log.Error("EmailBody s:\n" + s_body + "\nEnd debug");
            Log.Error("char count=" + s_body.Length.ToString());
            Log.Error("end debug");
            

            SmtpClient SmtpMail = new SmtpClient(s_server, s_serverPort);
            SmtpMail.Credentials = new NetworkCredential(s_userName, s_passWord);
            SmtpMail.EnableSsl = s_secureSSL;


            //MailMessage(string from, string to, string subject, string body);
            MailMessage mymailmessage = new MailMessage(s_from, s_to, s_subject, s_body);

            

            mymailmessage.Body = s_body;
            mymailmessage.IsBodyHtml = s_HtmlFormat;

            /*
            mymailmessage.Subject = s_subject;

            MailAddress mymailaddress = new MailAddress(s_from);


            mymailmessage.Sender = new MailAddress(s_from);
            mymailmessage.From = new MailAddress(s_from);
            //mymailmessage.*/

            SmtpMail.Send(mymailmessage);
            
            
            if (DEBUG == true)
                Log.Debug("Email with subject " + s_subject + " sent to " + s_to);
            s_error = false;
            s_errormessage = "NOERROR";
            s_busy = false;
            //Log.Debug("Email sent " + subject + body + to + s_server + s_serverPort.ToString() + s_server + s_userName + s_passWord + s_secureSSL.ToString());
        }
        catch (Exception ee)
        {
            Log.Error("Fatal Error: Email with subject " + s_subject + " sent to " + s_to);
            Log.Error("Exception message is " + ee.Message);
            Log.Error("EmailBody s:\n" + s_body + "\nEnd debug");
            Log.Error("char count=" + s_body.Length.ToString());
            Log.Error("Sent from:" + s_from);

            Log.Debug("Fatal Error: Email with subject " + s_subject + " sent to " + s_to);
            Log.Debug("Exception message is " + ee.Message);
            Log.Debug("EmailBody s:\n" + s_body + "\nEnd debug");           
            Log.Debug("char count=" + s_body.Length.ToString());
            Log.Debug("Sent from:" + s_from);

            System.Threading.Thread.Sleep(2000); //wait 2s
            Log.Error("Error " + ee.Message);
            
            s_errormessage = ee.Message;
            s_busy = false;
            return;
        }

        

    }

      /*
    public void labelmessage(string text)
    {
        if (newlabelmessage != null)
        {
            newlabelmessage(text);
            
            //newlabelmessage("Processing Title " + columndata[0]+" out of "+rowdata.Length.ToString());
        }
    }*/
#endregion Methods


  }

}
