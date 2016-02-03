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
//*************************
//Version 0.0.0.11
//*************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml; 



#if (MP11 || MP12 || MP16)
using MediaPortal.GUI.Library;
using Log = TvLibrary.Log.huha.Log;
using TvControl;
using TvDatabase;
using Gentle.Framework;
#elif (MP2)
using MediaPortal.Common;
using MediaPortal.Common.Commands;
using MediaPortal.Common.General;
using MediaPortal.Common.Logging;
using MediaPortal.Common.Messaging;
using MediaPortal.Common.Runtime;
using MediaPortal.Common.Settings;
using MediaPortal.Common.Localization;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Workflow;
using MediaPortal.UI.Presentation.Screens;
using MediaPortal.UI.Control.InputManager;
using MediaPortal.UI.Presentation.UiNotifications;
using MediaPortal.UI.SkinEngine.ScreenManagement;
using MediaPortal.UI.SkinEngine.Controls.Visuals;

//using TvWishList;
using MediaPortal.Plugins.TvWishList.Items;
#else
using TvLibrary.Log.huha;



#if (MPTV2)
// native TV3.5 for MP2
//using Mediaportal.TV.Server.Plugins.Base.Interfaces;
//using Mediaportal.TV.Server.SetupControls;
//using Mediaportal.TV.Server.TVControl.Events;
//using Mediaportal.TV.Server.TVControl.Interfaces.Events;
//using Mediaportal.TV.Server.TVControl.Interfaces.Services;
//using Mediaportal.Common.Utils;
using MediaPortal.Plugins.TvWishList.Items;
#else
// MP1 TV server
using TvControl;
using TvDatabase;
using Gentle.Framework;
#endif



//using SetupTv; 
//using TvEngine;
//using TvEngine.Events;
//using TvLibrary.Interfaces;
//using TvLibrary.Implementations;

//using Mediaportal.Plugins;
//using TvEngine.PowerScheduler.Interfaces;
#endif



namespace MediaPortal.Plugins.TvWishList
{

    #region Declarations
    public struct xmlmessage
    {
        public string title;
        public string description;
        public string channel;
        public DateTime start;
        public DateTime end;
        public int channel_id;
        public string EpisodeName;
        public string EpisodeNum;
        public string EpisodeNumber;
        public string EpisodePart;
        public string Genre;
        public string Classification;
        public DateTime OriginalAirDate;
        public int ParentalRating;
        public string SeriesNum;
        public int StarRating;
        public string message;
        public string type; //emailed scheduled recorded deleted conflict viewed
        public string searchstring;
        public DateTime created;
        public bool processed;
        public int unfiltered_index;
        public string tvwishid;
        public string recordingid;
        public string filename;
    }

    public enum MessageType 
    {
        Emailed = 1,
        Scheduled,
        Both,
        Recorded,
        Deleted,
        Conflict,
        Viewed
    }
    #endregion

    #region ICompare
    public class titlecompare : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(x.title, y.title);
        }
    }

    public class titlecompare_r : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(y.title, x.title);
        }
    }

    public class startcompare : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return DateTime.Compare(x.start, y.start);
        }
    }
    public class startcompare_r : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return DateTime.Compare(y.start, x.start);
        }
    }

    public class createcompare : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return DateTime.Compare(x.created, y.created);
        }
    }
    public class createcompare_r : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return DateTime.Compare(y.created, x.created);
        }
    }

    public class genrecompare : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(x.Genre, y.Genre);
        }
    }
    public class genrecompare_r : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(y.Genre, x.Genre);
        }
    }

    public class classificationcompare : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(x.Classification, y.Classification);
        }
    }
    public class classificationcompare_r : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(y.Classification, x.Classification);
        }
    }

    public class parentalratingcompare : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            if (x.ParentalRating >= y.ParentalRating)
            {
                return x.ParentalRating;
            }
            else
            {
                return y.ParentalRating;
            }
        }
    }
    public class parentalratingcompare_r : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {        
            if (x.ParentalRating < y.ParentalRating)
            {
                return x.ParentalRating;
            }
            else
            {
                return y.ParentalRating;
            }
        }       
    }

    public class starratingcompare : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            if (x.StarRating >= y.StarRating)
            {
                return x.StarRating;
            }
            else
            {
                return y.StarRating;
            }
        }
    }
    public class starratingcompare_r : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            if (x.StarRating < y.StarRating)
            {
                return x.StarRating;
            }
            else
            {
                return y.StarRating;
            }
        }       
    }

    public class typecompare : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(x.type, y.type);
        }
    }
    public class typecompare_r : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(y.type, x.type);
        }
    }

    public class messagecompare : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(x.message, y.message);
        }
    }
    public class messagecompare_r : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(y.message, x.message);
        }
    }

    public class searchstringcompare : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(x.searchstring, y.searchstring);
        }
    }
    public class searchstringcompare_r : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(y.searchstring, x.searchstring);
        }
    }

    public class episodenamecompare : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(x.EpisodeName, y.EpisodeName);
        }
    }
    public class episodenamecompare_r : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(y.EpisodeName, x.EpisodeName);
        }
    }

    public class episodenumcompare : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(x.EpisodeNum, y.EpisodeNum);
        }
    }
    public class episodenumcompare_r : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(y.EpisodeNum, x.EpisodeNum);
        }
    }

    public class episodenumbercompare : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(x.EpisodeNumber, y.EpisodeNumber);
        }
    }
    public class episodenumbercompare_r : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(y.EpisodeNumber, x.EpisodeNumber);
        }
    }

    public class episodepartcompare : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(x.EpisodePart, y.EpisodePart);
        }
    }
    public class episodepartcompare_r : IComparer<xmlmessage>
    {
        public int Compare(xmlmessage x, xmlmessage y)
        {
            return String.Compare(y.EpisodePart, x.EpisodePart);
        }
    }

    #endregion ICompare

    #if (TV101 || TV11 || TV12)
        [CLSCompliant(false)]
    #endif
    public class XmlMessages
    {
        #region Declarations
        List<xmlmessage> TvMessages = new List<xmlmessage>();
        List<xmlmessage> TvMessagesFiltered = new List<xmlmessage>();
        TvBusinessLayer layer = new TvBusinessLayer();

#if (TV101 || TV11 || TV12)
        LanguageTranslation PluginGuiLocalizeStrings = new LanguageTranslation();
#endif

#if (MP2)
        //Register global Services
        ILogger Log = ServiceRegistration.Get<ILogger>();
#endif

        //Instance
        public static XmlMessages _instance = null;

        public static XmlMessages Instance
        {
            get { return (XmlMessages)_instance; }
        }

        string _filename = "NOT_DEFINED";
        string _emailformat = "Error: Email Format Not Defined";
        bool _debug = true;
        string _dateTimeFormat = "ERROR: NOTDEFINED";
        string _FilterName = "";

        int NetworkLoops = 5; // number of tries to read the message file
        int NetworkSleepTime = 1000; // sleep between successless read file trials

        //string[] _eventformats;


        //settings for Result Window
        private int _sort = 1;
        private bool _sortreverse = true;
        private bool _email = true;
        private bool _deleted = false;
        private bool _conflicts = false;
        private bool _scheduled = true;
        private bool _recorded = false;
        private bool _view = true;

        private string _userListItemFormat = string.Empty;
        private string _userEmailFormat = string.Empty;
        private string _userViewOnlyFormat = string.Empty;

        public enum LogSetting
        {
            DEBUG = 1,
            ERROR,
            ERRORONLY,
            INFO,
            ADDRESPONSE
        }


        public enum Sorting
        {
              Title = 1,
              Start,
              Created,
              Genre,
              Classification,
	          ParentalRating,
	          StarRating,
	          Type,
	          Message,
	          SearchString,
	          EpisodeName,
              EpisodeNum,
              EpisodeNumber,
              EpisodePart,
        }

        public enum MessageEvents
        {
              NO_VALID_EPG 			=0, //	conflict
	          MANUAL_CONFLICT 		=1, //	conflict
	          SCHEDULE_SKIPPED		=2, //	conflict
	          MAXFOUND_EXCEEDED 	=3, //	conflict
	          FAILED_ADDING			=4, //	conflict
	          FAILED_CREATE			=5, //	conflict
	          REPEATED_FOUND		=6, //	conflict
	          LOW_PRIORITY_DELETE	=7, //	conflict
              FILTER_MISMATCH       =8, //  conflict
              ALREADY_RECORDED      =9, //  conflict
	          SCHEDULE_FOUND		=10, //	actionstring success
              EMAIL_FOUND           =11, //	actionstring success
              RECORDING_FOUND       =12,//  updating messages (not included in eventformat!!!
              RECORDING_DELETED     =13,//  updating messages (not included in eventformat!!!
        }
        public int MessageEventsNumber = 10;


        #endregion

        #region Properties

        public string filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        public bool debug
        {
            get { return _debug; }
            set { _debug = value; }
        }

            /*
        public string[] eventnumbers
        {
            get { return _eventformats; }
            set { _eventformats = value; }
        }*/

        public string date_time_format
        {
            get { return _dateTimeFormat; }
            set { _dateTimeFormat = value; }
        }

        public string EmailFormat
        {
            get { return _emailformat; }
            set { _emailformat = value; }
        }

        public string FilterName
        {
            get { return _FilterName; }
            set { _FilterName = value; }
        }

        public int Sort
        {
            get { return _sort; }
            set { _sort = value; }
        }
        public bool SortReverse
        {
            get { return _sortreverse; }
            set { _sortreverse = value; }
        }
        public bool Email
        {
            get { return _email; }
            set { _email = value; }
        }
        public bool Deleted
        {
            get { return _deleted; }
            set { _deleted = value; }
        }
        public bool Conflicts
        {
            get { return _conflicts; }
            set { _conflicts = value; }
        }
        public bool Scheduled
        {
            get { return _scheduled; }
            set { _scheduled = value; }
        }
        public bool Recorded
        {
            get { return _recorded; }
            set { _recorded = value; }
        }
        public bool View
        {
            get { return _view; }
            set { _view = value; }
        }
        public string UserListItemFormat
        {
            get { return _userListItemFormat; }
            set { _userListItemFormat = value; }
        }
        public string UserEmailFormat
        {
            get { return _userEmailFormat; }
            set { _userEmailFormat = value; }
        }
        public string UserViewOnlyFormat
        {
            get { return _userViewOnlyFormat; }
            set { _userViewOnlyFormat = value; }
        }

        #endregion properties

        #region Constructor

        public XmlMessages(string datetimeformat, string emailformat, bool debug)
        {
           //initialize
            _instance = this;

            _filename = filename;
            _debug = debug;
            _emailformat = emailformat;
            _dateTimeFormat = datetimeformat;
            //LogDebug("_dateTimeFormat= " + datetimeformat.Replace('{','['), (int)LogSetting.DEBUG);         

            //load MP language
#if (MP11 || MP12 || MP16)
            PluginGuiLocalizeStrings.LoadMPlanguage();
#elif (TV101 || TV11 || TV12)
            PluginGuiLocalizeStrings.ReadLanguageFile();
#endif
        }

        #endregion

        #region methods



        public void readxmlfile(string dataString, bool readfromfile)

        {
            LogDebug("readxml started", (int)LogSetting.DEBUG);
            if ((File.Exists(_filename) == false) && (readfromfile == true))
            {
                LogDebug("Warning Readxmlfile: Filename " + _filename + " does not exist", (int)LogSetting.ERROR);
                return;
            }

            

            try
            {
                TvMessages.Clear();
                TvMessagesFiltered.Clear();

                XmlDocument doc = new XmlDocument();

                


                if (readfromfile) //use filename for reading xml file
                {
                    for (int i = 0; i < NetworkLoops; i++) //multiple retries to open the file and read the messages
                    {
                        try
                        {
                            FileStream fs = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            doc.Load(fs);
                            fs.Close();
                            LogDebug("Filename " + _filename + " could be read in iteration " + i.ToString(), (int)LogSetting.DEBUG);
                            break;
                        }
                        catch (Exception exc)
                        {
                            System.Threading.Thread.Sleep(NetworkSleepTime);
                            LogDebug("Failing to read xml file in iteration " + i.ToString() + " Exception: " + exc.Message, (int)LogSetting.ERROR);
                        }
                        System.Threading.Thread.Sleep(500);
                    }
                }
                else //use string for reading xml file
                {

                    if ((readfromfile == false) && (dataString == ""))
                    {
                        LogDebug("Warning: no data transferred - resulting in 0 messages", (int)LogSetting.DEBUG);
                        return;
                    }

                    try
                    {
                        doc.LoadXml(dataString);                      
                    }
                    catch (Exception exc)
                    {                       
                        LogDebug("Failing to read xml stream Exception: " + exc.Message, (int)LogSetting.ERROR);
                    }
                }
                
                
                XmlNodeList allmessages = doc.SelectNodes("/ROOT/XMLMESSAGE");
                LogDebug("Found Nodenumber=" + allmessages.Count.ToString(), (int)LogSetting.DEBUG);
                foreach (XmlNode xmlnode in allmessages)
                {
                    if (xmlnode.HasChildNodes)
                    {
                        xmlmessage newmessage = new xmlmessage();

                        //Defaultvalues
                        newmessage.channel = string.Empty;
                        newmessage.channel_id = 0;
                        newmessage.Classification = string.Empty;
                        newmessage.created = DateTime.ParseExact("1800-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                        newmessage.description = string.Empty;
                        newmessage.end = DateTime.ParseExact("1800-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                        newmessage.EpisodeName = string.Empty;
                        newmessage.EpisodeNum = string.Empty;
                        newmessage.EpisodeNumber = string.Empty;
                        newmessage.EpisodePart = string.Empty;
                        newmessage.Genre = string.Empty;
                        newmessage.message = string.Empty;
                        newmessage.OriginalAirDate = DateTime.ParseExact("1800-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                        newmessage.ParentalRating = -1;
                        newmessage.processed = false;
                        newmessage.searchstring = string.Empty;
                        newmessage.SeriesNum = string.Empty;
                        newmessage.StarRating = -1;
                        newmessage.start = DateTime.ParseExact("1800-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                        newmessage.title = string.Empty;
                        newmessage.type = string.Empty;
                        newmessage.unfiltered_index = -1;
                        newmessage.tvwishid = "-1";
                        newmessage.recordingid = "-1";
                        newmessage.filename = string.Empty;
                        

                        foreach (XmlNode xml_message_node in xmlnode)
                        {
                            //LogDebug("Node=" + xml_message_node.Name);
                            if (xml_message_node.Name.ToUpper() == "TITLE")
                            {
                                newmessage.title = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "DESCRIPTION")
                            {
                                newmessage.description = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "CHANNEL")
                            {
                                newmessage.channel = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "START")
                            {
                                newmessage.start = DateTime.ParseExact(xml_message_node.InnerText, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            }
                            else if (xml_message_node.Name.ToUpper() == "END")
                            {
                                newmessage.end = DateTime.ParseExact(xml_message_node.InnerText, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            }
                            else if (xml_message_node.Name.ToUpper() == "EPISODENAME")
                            {
                                newmessage.EpisodeName = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "EPISODENUM")
                            {
                                newmessage.EpisodeNum = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "EPISODENUMBER")
                            {
                                newmessage.EpisodeNumber = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "EPISODEPART")
                            {
                                newmessage.EpisodePart = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "GENRE")
                            {
                                newmessage.Genre = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "CASSIFICATION")
                            {
                                newmessage.Classification = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "ORIGINALAIRDATE")
                            {
                                newmessage.OriginalAirDate = DateTime.ParseExact(xml_message_node.InnerText, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            }
                            else if (xml_message_node.Name.ToUpper() == "PARENTALRATING")  
                            {
                                int parentalRating = -1;
                                int.TryParse(xml_message_node.InnerText, out parentalRating);
                                newmessage.ParentalRating = parentalRating;
                            }
                            else if (xml_message_node.Name.ToUpper() == "SERIESNUM")
                            {
                                newmessage.SeriesNum = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "STARRATING")
                            {
                                int starRating = -1;
                                int.TryParse(xml_message_node.InnerText, out starRating);
                                newmessage.StarRating = starRating;
                            }
                            else if (xml_message_node.Name.ToUpper() == "PROGRAM_ID")  //legacy bad name
                            {
                                int channel_id = -1;
                                int.TryParse(xml_message_node.InnerText, out channel_id);
                                newmessage.channel_id = channel_id;
                            }
                            else if (xml_message_node.Name.ToUpper() == "CHANNEL_ID")
                            {
                                int channel_id = -1;
                                int.TryParse(xml_message_node.InnerText, out channel_id);
                                newmessage.channel_id = channel_id;
                            }
                            else if (xml_message_node.Name.ToUpper() == "MESSAGE")
                            {
                                newmessage.message = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "TYPE")
                            {
                                newmessage.type = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "SEARCHSTRING")
                            {
                                newmessage.searchstring = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "CREATED")
                            {
                                newmessage.created = DateTime.ParseExact(xml_message_node.InnerText, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            }
                            else if (xml_message_node.Name.ToUpper() == "PROCESSED")
                            {
                                Boolean processed = false;
                                Boolean.TryParse(xml_message_node.InnerText, out processed);
                                newmessage.processed = processed;
                            }
                            else if (xml_message_node.Name.ToUpper() == "TVWISHID")
                            {
                                newmessage.tvwishid = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "RECORDINGID")
                            {
                                newmessage.recordingid = xml_message_node.InnerText;
                            }
                            else if (xml_message_node.Name.ToUpper() == "FILENAME")
                            {
                                newmessage.filename = xml_message_node.InnerText;
                            }
                            newmessage.unfiltered_index = -1;
                        }
                        TvMessages.Add(newmessage);
                        
                    }
               }
               LogDebug("readxml: TvMessages.Count=" + TvMessages.Count.ToString(), (int)LogSetting.DEBUG);

            }
            catch (Exception exc)
            {
                LogDebug("Error: reading xml file "+_filename+" failed with exception " + exc.Message,(int)LogSetting.ERROR);
                return;
            }
            //LogDebug("readxml completed", (int)LogSetting.DEBUG);
        }

        public string writexmlfile(bool writeToFile)
        {
            LogDebug("writexml started", (int)LogSetting.DEBUG);
            LogDebug("TvMessages.Count=" + TvMessages.Count.ToString(), (int)LogSetting.DEBUG);
            string dataString = "";

            
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlNode rootElement = doc.CreateElement("ROOT");
                XmlNode node;
                foreach (xmlmessage onemessage in TvMessages)
                {
                    XmlNode messagenode = doc.CreateElement("XMLMESSAGE");
                    node = doc.CreateElement("TITLE");
                    node.InnerText = onemessage.title;
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("DESCRIPTION");
                    node.InnerText = onemessage.description;
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("CHANNEL");
                    node.InnerText = onemessage.channel;
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("START");
                    node.InnerText = onemessage.start.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("END");
                    node.InnerText = onemessage.end.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("EPISODENAME");
                    node.InnerText = onemessage.EpisodeName;
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("EPISODENUM");
                    node.InnerText = onemessage.EpisodeNum;
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("EPISODENUMBER");
                    node.InnerText = onemessage.EpisodeNumber;
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("EPISODEPART");
                    node.InnerText = onemessage.EpisodePart;
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("GENRE");
                    node.InnerText = onemessage.Genre;
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("CLASSIFICATION");
                    node.InnerText = onemessage.Classification;
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("ORIGINALAIRDATE");
                    node.InnerText = onemessage.OriginalAirDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    messagenode.AppendChild(node);
                    
                    node = doc.CreateElement("PARENTALRATING");
                    node.InnerText = onemessage.ParentalRating.ToString();
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("SERIESNUM");
                    node.InnerText = onemessage.SeriesNum;
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("STARRATING");
                    node.InnerText = onemessage.StarRating.ToString();
                    messagenode.AppendChild(node);     

                    node = doc.CreateElement("CHANNEL_ID");
                    node.InnerText = onemessage.channel_id.ToString();
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("MESSAGE");
                    node.InnerText = onemessage.message;
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("TYPE");
                    node.InnerText = onemessage.type;
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("SEARCHSTRING");
                    node.InnerText = onemessage.searchstring;
                    messagenode.AppendChild(node);
                    
                    node = doc.CreateElement("CREATED");
                    node.InnerText = onemessage.created.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("PROCESSED");
                    node.InnerText = onemessage.processed.ToString();
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("TVWISHID");
                    node.InnerText = onemessage.tvwishid.ToString();
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("RECORDINGID");                    
                    node.InnerText = onemessage.recordingid.ToString();
                    messagenode.AppendChild(node);

                    node = doc.CreateElement("FILENAME");
                    node.InnerText = onemessage.filename.ToString();
                    messagenode.AppendChild(node);
                    
                    rootElement.AppendChild(messagenode);
                }
                doc.AppendChild(rootElement);
               
                if (writeToFile)
                {
                    if (File.Exists(_filename) == true)
                    {
                        File.Delete(_filename);
                    }

                    for (int i = 0; i < NetworkLoops; i++) //multiple retries to open the file and read the messages
                    {

                        try
                        {
                            FileStream fs = new FileStream(_filename, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                            doc.Save(fs);
                            fs.Close();
                            LogDebug("Filename " + _filename + " could be written in iteration " + i.ToString(), (int)LogSetting.DEBUG);
                            break;
                        }
                        catch (Exception exc)
                        {
                            System.Threading.Thread.Sleep(NetworkSleepTime);
                            LogDebug("Failing to write xml file in iteration " + i.ToString() + " Exception: " + exc.Message, (int)LogSetting.DEBUG);
                        }
                        System.Threading.Thread.Sleep(500);
                    }
                    
                }
                else //write to string
                {
                    try
                    {
                        StringWriter sw = new StringWriter();
                        XmlTextWriter xw = new XmlTextWriter(sw);
                        doc.WriteTo(xw);
                        dataString = sw.ToString();
                        //Log.Debug("dataString=" + dataString);
                    }
                    catch (Exception exc)
                    {
                        LogDebug("Failing to write xml file to string - Exception: " + exc.Message, (int)LogSetting.DEBUG);
                    }                   
                }                               
            }
            catch (Exception exc)
            {
                LogDebug("Error: writing messages failed  with exception " + exc.Message, (int)LogSetting.ERROR);
                return "";
            }
            //LogDebug("writexml completed", (int)LogSetting.DEBUG);
            return dataString;          
        }

        public bool ClearTvMessages()
        {
            TvMessages.Clear();
            TvMessagesFiltered.Clear();
            return true;
        }

        public bool DeleteTvMessageAt(int index)
        {
            try
            {
                TvMessages.RemoveAt(index);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool DeleteTvMessage(xmlmessage message)
        {
            try
            {
                TvMessages.Remove(message);
                return true;
            }
            catch
            {
                return false;
            }

        }

#if (TV101 || TV11 || TV12)
        [CLSCompliant(false)]

        public bool DeleteTVMessageByProgram(Program myprogram)
        { //by title & description & channel & start & end
            //LogDebug("deletemessage started", (int)LogSetting.DEBUG);
            try
            {
                int ctr = 0;
                foreach (xmlmessage onemessage in TvMessages)
                {
                    Channel mychannel = Channel.Retrieve(myprogram.IdChannel);
                    if ((onemessage.title == myprogram.Title) && (onemessage.description == myprogram.Description) && (onemessage.channel == mychannel.DisplayName) && (onemessage.start == myprogram.StartTime) && (onemessage.end == myprogram.EndTime))
                    {
                        LogDebug("Deleting message " + myprogram.Title, (int)LogSetting.DEBUG);

                        if (debug)
                            logmessage(onemessage);

                        TvMessages.RemoveAt(ctr);

                        break;
                    }
                    ctr++;
                }

            }
            catch (Exception exc)
            {
                LogDebug("Error: deleting message failed with exception " + exc.Message, (int)LogSetting.ERROR);
                return false;
            }
            //LogDebug("deletemessage completed", (int)LogSetting.DEBUG);
            return true;
        }

        [CLSCompliant(false)]
        public bool DeleteTVMessageBySchedule(Schedule myschedule)
        { //by title & description & channel & start & end
            //LogDebug("deletemessage started", (int)LogSetting.DEBUG);
            try
            {
                int ctr = 0;
                foreach (xmlmessage onemessage in TvMessages)
                {
                    Channel mychannel = Channel.Retrieve(myschedule.IdChannel);
                    if ((onemessage.title == myschedule.ProgramName) && (onemessage.channel == mychannel.DisplayName) && (onemessage.start == myschedule.StartTime) && (onemessage.end == myschedule.EndTime))
                    {
                        LogDebug("Deleting message " + myschedule.ProgramName, (int)LogSetting.DEBUG);

                        if (debug)
                            logmessage(onemessage);

                        TvMessages.RemoveAt(ctr);

                        break;
                    }
                    ctr++;
                }

            }
            catch (Exception exc)
            {
                LogDebug("Error: deleting message failed with exception " + exc.Message, (int)LogSetting.ERROR);
                return false;
            }
            //LogDebug("deletemessage completed", (int)LogSetting.DEBUG);
            return true;
        }

        [CLSCompliant(false)]
        public int GetTvMessageBySchedule(Schedule myschedule)
        {

            try
            {
                int ctr = 0;
                foreach (xmlmessage onemessage in TvMessages)
                {
                    Channel mychannel = Channel.Retrieve(myschedule.IdChannel);
                    if ((onemessage.title == myschedule.ProgramName) && (onemessage.channel == mychannel.DisplayName) && (onemessage.start == myschedule.StartTime) && (onemessage.end == myschedule.EndTime))
                    {
                        LogDebug("Found message " + myschedule.ProgramName, (int)LogSetting.DEBUG);

                        if (debug)
                        {
                            logmessage(onemessage);
                            LogDebug("Found index="+ctr.ToString(), (int)LogSetting.DEBUG);
                        }

                        return ctr;
                    }
                    ctr++;
                }

            }
            catch (Exception exc)
            {
                LogDebug("Error: finding message by schedule failed with exception " + exc.Message, (int)LogSetting.ERROR);
                return -1;
            }
            LogDebug("GetTvMessageBySchedule completed without finding results", (int)LogSetting.DEBUG);
            return -1;
        }

        [CLSCompliant(false)]
        public int GetTvMessageBySchedule(Schedule myschedule, MessageType type)
        {

            try
            {
                int ctr = 0;
                foreach (xmlmessage onemessage in TvMessages)
                {
                    Channel mychannel = Channel.Retrieve(myschedule.IdChannel);
                    if ((onemessage.title == myschedule.ProgramName) && (onemessage.channel == mychannel.DisplayName) && (onemessage.start == myschedule.StartTime) && (onemessage.end == myschedule.EndTime))
                    {
                        if ((type.ToString().ToLower() == "any") || (onemessage.type.ToLower() == type.ToString().ToLower()))
                        {

                            LogDebug("Found message " + myschedule.ProgramName, (int)LogSetting.DEBUG);

                            if (debug)
                            {
                                logmessage(onemessage);
                                LogDebug("Found index=" + ctr.ToString(), (int)LogSetting.DEBUG);
                            }

                            return ctr;
                        }
                    }
                    ctr++;
                }

            }
            catch (Exception exc)
            {
                LogDebug("Error: finding message by schedule failed with exception " + exc.Message, (int)LogSetting.ERROR);
                return -1;
            }
            LogDebug("GetTvMessageBySchedule completed without finding results", (int)LogSetting.DEBUG);
            return -1;
        }
#endif

        public xmlmessage GetTvMessageAtIndex(int index)
        {
            return TvMessages[index];
        }





        public List<xmlmessage> ListAllTvMessages()
        {
            return TvMessages;
        }

        public void ReplaceTvMessageAtIndex(int index, xmlmessage newMessage)
        {
            TvMessages[index] = newMessage;
        }



        public bool DeleteTvMessageFilteredAt(int index)
        {
            try
            {
                TvMessagesFiltered.RemoveAt(index);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool DeleteTvMessageFiltered(xmlmessage message)
        {
            try
            {
                TvMessagesFiltered.Remove(message);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public xmlmessage GetTvMessageFilteredAtIndex(int index)
        {
            return TvMessagesFiltered[index];
        }

        public List<xmlmessage> ListAllTvMessagesFiltered()
        {
            return TvMessagesFiltered;
        }

        public void ReplaceTvMessageFilteredAtIndex(int index, xmlmessage newMessage)
        {
            TvMessagesFiltered[index] = newMessage;
        }


#if (TV101 || TV11 || TV12)
        public void updatemessages(int deleteExpiration)  //check for Schedule.ListAll
        { 
            //Log.Debug("updatemessages(Epgmarker=" + Epgmarker + ", EpisodeName_b=" + EpisodeName_b.ToString() + ", EpisodeNumber_b=" + EpisodeNumber_b.ToString()+")");
            try
            {
                LogDebug("updatemessages(): TvMessages.Count=" + TvMessages.Count.ToString(), (int)LogSetting.DEBUG);
                List<xmlmessage> newTvMessages = new List<xmlmessage>();
                for (int i=0;i<TvMessages.Count;i++)
                {
                    xmlmessage mymessage = TvMessages[i];
               
                    if ((mymessage.start > DateTime.Now)&&((mymessage.type == MessageType.Emailed.ToString())||(mymessage.type == MessageType.Both.ToString())))
                    {//add emails if in the future
                        newTvMessages.Add(mymessage);
                    }
                    /*else if ((mymessage.start > DateTime.Now) && (mymessage.type == MessageType.Conflict.ToString()))  //do not use old conflicts, as they are newly generated during each epg run
                    { //add conflict if in the future
                        newTvMessages.Add(mymessage);
                    }*/
                    else if ((mymessage.start > DateTime.Now) && (mymessage.type == MessageType.Viewed.ToString()))
                    { //add viewed if in the future
                        newTvMessages.Add(mymessage);
                    }
                    else if ((mymessage.type == MessageType.Scheduled.ToString()) || (mymessage.type == MessageType.Both.ToString())) 
                    {// check for existing schedule in the future
                        foreach (Schedule myschedule in Schedule.ListAll())
                        {
                            if ((myschedule.ProgramName == mymessage.title) && (myschedule.StartTime == mymessage.start) && (myschedule.EndTime == mymessage.end) && (myschedule.IdChannel == mymessage.channel_id))
                            {// schedule still exists with same name, start, end and channel id
                                newTvMessages.Add(mymessage);
                                break;
                            }
                        }                       
                    }
                    else if (mymessage.type == MessageType.Recorded.ToString())
                    {//check for existing file

                        //Debug only
                        //Log.Debug("mymessage.title=" + mymessage.title);
                        //Log.Debug("mymessage.title.ToUpper()=" + mymessage.title.ToUpper());
                        //Log.Debug("mymessage.description=" + mymessage.description);
                        //Log.Debug("mymessage.description.ToUpper()=" + mymessage.description.ToUpper());

                        if ((mymessage.start == mymessage.end)||((mymessage.title == mymessage.title.ToUpper())&&(mymessage.description==mymessage.description.ToUpper()))) //delete old messages which where created during recording (this was an old bug!)
                        {
                            Log.Debug("Message "+mymessage.title+" at "+mymessage.start.ToString()+"found with identical start and end time or upper case letters- will be skipped");
                            continue;
                        }
                        Recording myrecording = null;
                        foreach (Recording onerecording in Recording.ListAll())
                        {
                            //if ((mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex).title == myrecording.Title) && (mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex).channel_id == myrecording.IdChannel) && (mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex).start == myrecording.StartTime) && (mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex).end == myrecording.EndTime))
                            if (mymessage.recordingid == onerecording.IdRecording.ToString())
                            {
                                myrecording=onerecording;
                                break;
                            }
                        }


                        /* not working !!! do not use retrieve
                        int id = Convert.ToInt32(mymessage.recordingid);
                        Log.Debug("id=" + id.ToString());
                        Recording myrecording = Recording.Retrieve(id);*/
                        
                        if (myrecording == null)
                        {
                            mymessage.type = MessageType.Deleted.ToString();
                            mymessage.recordingid = "-1";
                            LanguageTranslation lng = new LanguageTranslation();
                            lng.ReadLanguageFile();
                            mymessage.message = lng.TranslateString("File has been deleted", 3651);
                            
                            Log.Debug(mymessage.title + " " + mymessage.start.ToString() + " Type changed to deleted 1");
                            newTvMessages.Add(mymessage); //always add deleted
                        }
                        else
                        {
                            if (File.Exists(myrecording.FileName) == false)
                            {
                                mymessage.type = MessageType.Deleted.ToString();
                                mymessage.recordingid = "-1";
                                LanguageTranslation lng = new LanguageTranslation();
                                lng.ReadLanguageFile();
                                mymessage.message = lng.TranslateString("File has been deleted", 3651);
                                Log.Debug(mymessage.title + " " + mymessage.start.ToString() + " Type changed to deleted 2");
                                newTvMessages.Add(mymessage); //always add deleted
                            }
                            else //filename is valid => valid recording, filesize has been added before and does exist already
                            {
                                Log.Debug(mymessage.title + " " + mymessage.start.ToString() + " Type unchanged as recorded");
                                newTvMessages.Add(mymessage); //always add recorded
                            }
                        }                        
                    }
                    else if (mymessage.type == MessageType.Deleted.ToString())
                    {//check for expired deletion date 
                        if (mymessage.created.AddMonths(deleteExpiration) > DateTime.Now)
                        {
                            Log.Debug(mymessage.title + " " + mymessage.start.ToString() + " Type unchanged as deleted");
                            newTvMessages.Add(mymessage);//always add deleted or recorded
                        }
                        else
                        {
                            Log.Debug(mymessage.title + " " + mymessage.start.ToString() + " Message has been deleted due to expred deletion date");
                        }
                    }

                    
                }//end all messages

                TvMessages.Clear();
                TvMessages = newTvMessages;
                LogDebug("updatemessages(): new count is " + TvMessages.Count.ToString(), (int)LogSetting.DEBUG);
            }
            catch (Exception exc)
            {
                LogDebug("Error: deleting old messages failed with exception " + exc.Message, (int)LogSetting.ERROR);
                return;
            }
            //LogDebug("deleteold completed", (int)LogSetting.DEBUG);
        }


        [CLSCompliant(false)]

        public bool addmessage(Program newprogram, string message, MessageType type, string searchstring, int eventnumber, string tvwishid, string filename)
        {
            //LogDebug("addmessage1 started", (int)LogSetting.DEBUG);
            if (type == MessageType.Both)
            {//split both into two messages email and record
                bool ok1 = addmessage(newprogram, message, MessageType.Emailed, searchstring, eventnumber, tvwishid, filename);
                Log.Debug("Emailed type Added");
                bool ok2 = addmessage(newprogram, message, MessageType.Scheduled, searchstring, eventnumber, tvwishid, filename);
                Log.Debug("Scheduled type Added");
                return (ok1 && ok2);
            }



            if (existsmessage(newprogram, type) == true)
            {
                LogDebug("Message " + newprogram.Title + " exists already for type " + type, (int)LogSetting.DEBUG);
                return false;
            }
            try
            {
                xmlmessage newmessage = new xmlmessage();
                newmessage.title = newprogram.Title;
                newmessage.description = newprogram.Description;
                newmessage.channel_id = newprogram.IdChannel;
                Channel mychannel = Channel.Retrieve(newprogram.IdChannel);
                try
                {
                    newmessage.channel = mychannel.DisplayName;
                }
                catch
                {
                    newmessage.channel = string.Empty;
                }
                newmessage.start = newprogram.StartTime;
                newmessage.end = newprogram.EndTime;

#if (TV101|| MP11)
                newmessage.EpisodeName = "";                    
                newmessage.EpisodePart = "";
                newmessage.EpisodeNumber = "";
                
#elif ( TV11 || TV12 || MP12 || MP16)
                newmessage.EpisodeName = newprogram.EpisodeName;
                newmessage.EpisodePart = newprogram.EpisodePart;
                newmessage.EpisodeNumber = newprogram.EpisodeNumber;
                
#endif
                newmessage.EpisodeNum = newprogram.EpisodeNum;
                newmessage.Genre = newprogram.Genre;
                newmessage.Classification = newprogram.Classification;
                newmessage.OriginalAirDate = newprogram.OriginalAirDate;
                newmessage.ParentalRating = newprogram.ParentalRating;
                newmessage.SeriesNum = newprogram.SeriesNum;
                newmessage.StarRating = newprogram.StarRating;
                newmessage.message = message;
                newmessage.type = type.ToString();
                newmessage.searchstring = searchstring;
                newmessage.created = DateTime.Now;
                newmessage.processed = false;
                newmessage.tvwishid = tvwishid;
                newmessage.recordingid = "-1";
                newmessage.filename = filename;
                newmessage.unfiltered_index = -1;
                TvMessages.Add(newmessage);
                if (debug)
                    logmessage(newmessage);
                
            }
            catch (Exception exc)
            {
                LogDebug("Error: (1) adding new message failed with exception " + exc.Message, (int)LogSetting.ERROR);
                return false;
            }
            

            //LogDebug("addmessage1 completed", (int)LogSetting.DEBUG);
            return true;
        }

        [CLSCompliant(false)]
        public bool addmessage(Schedule newschedule, string message, MessageType type, string searchstring, int eventnumber, string tvwishid, string filename)
        {
            //LogDebug("addmessage2 started", (int)LogSetting.DEBUG);
            if (type == MessageType.Both)
            {//split both into two messages email and record
                bool ok1 = addmessage(newschedule, message, MessageType.Emailed, searchstring, eventnumber,tvwishid, filename);
                Log.Debug("Emailed type Added");
                bool ok2 = addmessage(newschedule, message, MessageType.Scheduled, searchstring, eventnumber,tvwishid, filename);
                Log.Debug("Scheduled type Added");
                return (ok1 && ok2);
            }


            if (existsmessage(newschedule,type)==true)
            {
                LogDebug("Message " + newschedule.ProgramName+" exists already for type "+type, (int)LogSetting.DEBUG);
                return false;
            }

            try
            {
                xmlmessage newmessage = new xmlmessage();
                newmessage.title = newschedule.ProgramName;
                
                newmessage.channel_id = newschedule.IdChannel;
                Channel mychannel = Channel.Retrieve(newschedule.IdChannel);
                try
                {
                    newmessage.channel = mychannel.DisplayName;
                }
                catch
                {
                    newmessage.channel = string.Empty;
                }
                newmessage.start = newschedule.StartTime;
                newmessage.end = newschedule.EndTime;

                try
                {                    
#if (TV101 || MP11)
                    Program newprogram = Program.RetrieveByTitleAndTimes(newmessage.title, newmessage.start, newmessage.end);
                    newmessage.EpisodeName = "";                    
                    newmessage.EpisodePart = "";
                    newmessage.EpisodeNumber = "";
#elif ( TV11 || TV12 || MP12 || MP16)
                    Program newprogram = Program.RetrieveByTitleTimesAndChannel(newmessage.title, newmessage.start, newmessage.end, newmessage.channel_id);
                    newmessage.EpisodeName = newprogram.EpisodeName;                    
                    newmessage.EpisodePart = newprogram.EpisodePart;
                    newmessage.EpisodeNumber = newprogram.EpisodeNumber;
#endif
                    newmessage.description = newprogram.Description;
                    newmessage.EpisodeNum = newprogram.EpisodeNum;                   
                    newmessage.Genre = newprogram.Genre;
                    newmessage.Classification = newprogram.Classification;
                    newmessage.OriginalAirDate = newprogram.OriginalAirDate;
                    newmessage.ParentalRating = newprogram.ParentalRating;
                    newmessage.SeriesNum = newprogram.SeriesNum;
                    newmessage.StarRating = newprogram.StarRating;
                }
                catch
                {
                    newmessage.description = "";
                    newmessage.EpisodeName = "";
                    newmessage.EpisodeNum = "";
                    newmessage.EpisodeNumber = "";
                    newmessage.EpisodePart = "";
                    newmessage.Genre = "";
                    newmessage.Classification = "";
                    newmessage.OriginalAirDate = DateTime.ParseExact("1800-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                    newmessage.ParentalRating = 0;
                    newmessage.SeriesNum = "";
                    newmessage.StarRating = 0;
                }

                newmessage.message = message;
                newmessage.type = type.ToString();
                newmessage.searchstring = searchstring;
                newmessage.created = DateTime.Now;
                newmessage.processed = false;
                newmessage.tvwishid = tvwishid;
                newmessage.recordingid = "-1";
                newmessage.filename = filename;
                newmessage.unfiltered_index = -1;
                TvMessages.Add(newmessage);
                if (debug)
                    logmessage(newmessage);
            }
            catch (Exception exc)
            {
                LogDebug("Error: (2) adding new message failed with exception " + exc.Message, (int)LogSetting.ERROR);
                return false;
            }
            //LogDebug("addmessage2 completed", (int)LogSetting.DEBUG);
            return true;
        }

        [CLSCompliant(false)]
        public bool addmessage(Recording newrecording, string message, MessageType type, string searchstring, int eventnumber, string tvwishid, string filename)
        {
            if (existsmessage(newrecording, type) == true)
            {
                LogDebug("Message " + newrecording.Title + " exists already for type recorded", (int)LogSetting.DEBUG);
                return false;
            }

            try
            {
                xmlmessage newmessage = new xmlmessage();
                newmessage.title = newrecording.Title;
                newmessage.channel_id = newrecording.IdChannel;
                Channel mychannel = Channel.Retrieve(newrecording.IdChannel);
                try
                {
                    newmessage.channel = mychannel.DisplayName;
                }
                catch
                {
                    newmessage.channel = string.Empty;
                }
                newmessage.start = newrecording.StartTime;
                newmessage.end = newrecording.EndTime;
                newmessage.EpisodeNum = "";
                newmessage.SeriesNum = "";
                try
                {
#if (TV101|| MP11)
                    newmessage.EpisodeName = "";                    
                    newmessage.EpisodePart = "";
                    newmessage.EpisodeNumber = "";
#elif ( TV11 || TV12 || MP12 || MP16)
                    newmessage.EpisodeName = newrecording.EpisodeName;
                    newmessage.EpisodePart = newrecording.EpisodePart;
                    newmessage.EpisodeNumber = newrecording.EpisodeNumber;
                    newmessage.EpisodeNum = newrecording.EpisodeNum;
                    newmessage.SeriesNum = newrecording.SeriesNum;
#endif
                    newmessage.description = newrecording.Description;

                    newmessage.Genre = newrecording.Genre;
                    newmessage.Classification = "";
                    newmessage.OriginalAirDate = DateTime.ParseExact("1800-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                    newmessage.ParentalRating = 0;

                    newmessage.StarRating = 0;
                }
                catch
                {
                    newmessage.description = "";
                    newmessage.EpisodeName = "";
                    newmessage.EpisodeNum = "";
                    newmessage.EpisodeNumber = "";
                    newmessage.EpisodePart = "";
                    newmessage.Genre = "";
                    newmessage.Classification = "";
                    newmessage.OriginalAirDate = DateTime.ParseExact("1800-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);
                    newmessage.ParentalRating = 0;
                    newmessage.SeriesNum = "";
                    newmessage.StarRating = 0;
                }
                newmessage.message = message;
                newmessage.type = type.ToString();
                newmessage.searchstring = searchstring;
                newmessage.created = DateTime.Now;
                newmessage.processed = false;
                newmessage.tvwishid = tvwishid;
                if (type == MessageType.Deleted)
                {
                    newmessage.recordingid = "-1";
                }
                else
                {
                    newmessage.recordingid = newrecording.IdRecording.ToString();
                }
                //Log.Debug("newmessage.recordingid=" + newmessage.recordingid);
                newmessage.filename = filename;
                newmessage.unfiltered_index = -1;
                TvMessages.Add(newmessage);
                if (debug)
                    logmessage(newmessage);

            }
            catch (Exception exc)
            {
                LogDebug("Error: (3) adding new message failed with exception " + exc.Message, (int)LogSetting.ERROR);
                return false;
            }
            LogDebug("addmessage recording completed", (int)LogSetting.DEBUG);
            return true;
        }

        [CLSCompliant(false)]
        public bool existsmessage(Program myprogram, MessageType myMessageType)
        {// by title & description & channel & start & end
            //LogDebug("existsmessage1 started", (int)LogSetting.DEBUG);
            string type = myMessageType.ToString();
            try
            {
                Channel mychannel = Channel.Retrieve(myprogram.IdChannel);
                string mychannelname = string.Empty;
                if (mychannel != null)
                {
                    mychannelname = mychannel.DisplayName;
                }

                for (int i=0;i<TvMessages.Count;i++)
                {
                    xmlmessage onemessage = TvMessages[i];
                    if ((onemessage.title == myprogram.Title) && (onemessage.channel == mychannelname) && (onemessage.start == myprogram.StartTime) && (onemessage.end == myprogram.EndTime))
                    {
                        if ((type.ToLower() == "any") || (onemessage.type.ToLower() == type.ToLower()))
                        {
                            //onemessage.tvwishid = tvwishid; //update in case message id belong to a different tvwish before
                            TvMessages[i] = onemessage;
                            //LogDebug("existsmessage1 completed tvwishid updated to " + TvMessages[i].tvwishid, (int)LogSetting.DEBUG);
                            return true;
                        }
                    }                   
                }                
            }
            catch (Exception exc)
            {
                LogDebug("Error: (1) searching messages failed with exception " + exc.Message, (int)LogSetting.ERROR);
                return false;
            }
            //LogDebug("existsmessage1 completed", (int)LogSetting.DEBUG);
            return false;
        }

        [CLSCompliant(false)]
        public bool existsmessage(Schedule myschedule, MessageType myMessageType)
        {// by title & description & channel & start & end
            //LogDebug("existsmessage2 started", (int)LogSetting.DEBUG);
            string type = myMessageType.ToString();
            try
            {
                Channel mychannel = Channel.Retrieve(myschedule.IdChannel);
                string mychannelname = string.Empty;
                if (mychannel != null)
                {
                    mychannelname = mychannel.DisplayName;
                }

                for (int i = 0; i < TvMessages.Count; i++)
                {
                    xmlmessage onemessage = TvMessages[i];
                    if ((onemessage.title == myschedule.ProgramName) && (onemessage.channel == mychannelname) && (onemessage.start == myschedule.StartTime) && (onemessage.end == myschedule.EndTime))
                    {
                        if ((type.ToLower() == "any") || (onemessage.type.ToLower() == type.ToLower()))
                        {
                            //onemessage.tvwishid = tvwishid; //update in case message id belong to a different tvwish before
                            TvMessages[i] = onemessage;
                            //LogDebug("existsmessage2 completed tvwishid updated to " + TvMessages[i].tvwishid, (int)LogSetting.DEBUG);
                            return true;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                LogDebug("Error: (2) searching messages failed with exception " + exc.Message, (int)LogSetting.ERROR);
                return false;
            }
            //LogDebug("existsmessage2 completed", (int)LogSetting.DEBUG);
            return false;
        }

        [CLSCompliant(false)]
        public bool existsmessage(Recording myrecording, MessageType myMessageType)            
        {// by title & description & channel & start & end
            string type = myMessageType.ToString();
            //LogDebug("existsmessage2 started", (int)LogSetting.DEBUG);
            try
            {
                Channel mychannel = Channel.Retrieve(myrecording.IdChannel);

                string mychannelname = string.Empty;
                if (mychannel != null)
                {
                    mychannelname = mychannel.DisplayName;
                }


                for (int i = 0; i < TvMessages.Count; i++)
                {
                    xmlmessage onemessage = TvMessages[i];
                    if ((onemessage.title == myrecording.Title) && (onemessage.channel == mychannelname) && (onemessage.start == myrecording.StartTime) && (onemessage.end == myrecording.EndTime))
                    {
                        if ((type.ToLower() == "any") || (onemessage.type.ToLower() == type.ToLower()))
                        {
                            //onemessage.tvwishid = tvwishid; //update in case message id belong to a different tvwish before
                            TvMessages[i] = onemessage;
                            //LogDebug("existsmessage3 completed tvwishid updated to " + TvMessages[i].tvwishid, (int)LogSetting.DEBUG);
                            return true;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                LogDebug("Error: (3) searching messages failed with exception " + exc.Message, (int)LogSetting.ERROR);
                return false;
            }
            //LogDebug("existsmessage2 completed", (int)LogSetting.DEBUG);
            return false;
        }
#endif

        public void sortmessages(int type,bool sortreverse)
        {
            LogDebug("sortmessage started type="+type.ToString()+"  sortreverse="+sortreverse.ToString(), (int)LogSetting.DEBUG);
            if (type == (int)Sorting.Title) //title
            {
                if (sortreverse == false)
                {
                    titlecompare myname = new titlecompare();
                    TvMessages.Sort(myname);
                    LogDebug("messages sorted by name", (int)LogSetting.DEBUG);
                }
                else
                {
                    titlecompare_r myname = new titlecompare_r();
                    TvMessages.Sort(myname);
                    LogDebug("messages reverse sorted by name", (int)LogSetting.DEBUG);
                }              
            }
            else if (type == (int)Sorting.Start) //start
            {
                if (sortreverse == false)
                {
                    startcompare myname = new startcompare();
                    TvMessages.Sort(myname);
                    LogDebug("messages sorted by start", (int)LogSetting.DEBUG);
                }
                else
                {
                    startcompare_r myname = new startcompare_r();
                    TvMessages.Sort(myname);
                    LogDebug("messages reverse sorted by start", (int)LogSetting.DEBUG);
                }
            }
            else if (type == (int)Sorting.Created) //created
            {
                if (sortreverse == false)
                {
                    createcompare myname = new createcompare();
                    TvMessages.Sort(myname);
                    LogDebug("messages sorted by create", (int)LogSetting.DEBUG);
                }
                else
                {
                    createcompare_r myname = new createcompare_r();
                    TvMessages.Sort(myname);
                    LogDebug("messages reverse sorted by create", (int)LogSetting.DEBUG);
                }
            }
            else if (type == (int)Sorting.Genre) 
            {
                if (sortreverse == false)
                {
                    genrecompare myname = new genrecompare();
                    TvMessages.Sort(myname);
                    LogDebug("messages sorted by genre", (int)LogSetting.DEBUG);
                }
                else
                {
                    genrecompare_r myname = new genrecompare_r();
                    TvMessages.Sort(myname);
                    LogDebug("messages reverse sorted by genre", (int)LogSetting.DEBUG);
                }
            }
            else if (type == (int)Sorting.Classification)
            {
                if (sortreverse == false)
                {
                    classificationcompare myname = new classificationcompare();
                    TvMessages.Sort(myname);
                    LogDebug("messages sorted by classification", (int)LogSetting.DEBUG);
                }
                else
                {
                    classificationcompare_r myname = new classificationcompare_r();
                    TvMessages.Sort(myname);
                    LogDebug("messages reverse sorted by classification", (int)LogSetting.DEBUG);
                }
            }
            else if (type == (int)Sorting.ParentalRating)
            {
                if (sortreverse == false)
                {
                    parentalratingcompare myname = new parentalratingcompare();
                    TvMessages.Sort(myname);
                    LogDebug("messages sorted by parentalrating", (int)LogSetting.DEBUG);
                }
                else
                {
                    parentalratingcompare_r myname = new parentalratingcompare_r();
                    TvMessages.Sort(myname);
                    LogDebug("messages reverse sorted by parentalrating", (int)LogSetting.DEBUG);
                }
            }
            else if (type == (int)Sorting.StarRating)
            {
                if (sortreverse == false)
                {
                    starratingcompare myname = new starratingcompare();
                    TvMessages.Sort(myname);
                    LogDebug("messages sorted by starrating", (int)LogSetting.DEBUG);
                }
                else
                {
                    starratingcompare_r myname = new starratingcompare_r();
                    TvMessages.Sort(myname);
                    LogDebug("messages reverse sorted by starrating", (int)LogSetting.DEBUG);
                }
            }
            else if (type == (int)Sorting.Type)
            {
                if (sortreverse == false)
                {
                    typecompare myname = new typecompare();
                    TvMessages.Sort(myname);
                    LogDebug("messages sorted by type", (int)LogSetting.DEBUG);
                }
                else
                {
                    typecompare_r myname = new typecompare_r();
                    TvMessages.Sort(myname);
                    LogDebug("messages reverse sorted by type", (int)LogSetting.DEBUG);
                }
            }
            else if (type == (int)Sorting.Message)
            {
                if (sortreverse == false)
                {
                    messagecompare myname = new messagecompare();
                    TvMessages.Sort(myname);
                    LogDebug("messages sorted by message", (int)LogSetting.DEBUG);
                }
                else
                {
                    messagecompare_r myname = new messagecompare_r();
                    TvMessages.Sort(myname);
                    LogDebug("messages reverse sorted by message", (int)LogSetting.DEBUG);
                }
            }
            else if (type == (int)Sorting.SearchString)
            {
                if (sortreverse == false)
                {
                    searchstringcompare myname = new searchstringcompare();
                    TvMessages.Sort(myname);
                    LogDebug("messages sorted by searchstring", (int)LogSetting.DEBUG);
                }
                else
                {
                    searchstringcompare_r myname = new searchstringcompare_r();
                    TvMessages.Sort(myname);
                    LogDebug("messages reverse sorted by searchstring", (int)LogSetting.DEBUG);
                }
            }
            else if (type == (int)Sorting.EpisodeName)
            {
                if (sortreverse == false)
                {
                    episodenamecompare myname = new episodenamecompare();
                    TvMessages.Sort(myname);
                    LogDebug("messages sorted by episodename", (int)LogSetting.DEBUG);
                }
                else
                {
                    episodenamecompare_r myname = new episodenamecompare_r();
                    TvMessages.Sort(myname);
                    LogDebug("messages reverse sorted by episodename", (int)LogSetting.DEBUG);
                }
            }
            else if (type == (int)Sorting.EpisodeNum)
            {
                if (sortreverse == false)
                {
                    episodenumcompare myname = new episodenumcompare();
                    TvMessages.Sort(myname);
                    LogDebug("messages sorted by episodenum", (int)LogSetting.DEBUG);
                }
                else
                {
                    episodenumcompare_r myname = new episodenumcompare_r();
                    TvMessages.Sort(myname);
                    LogDebug("messages reverse sorted by episodenum", (int)LogSetting.DEBUG);
                }
            }
            else if (type == (int)Sorting.EpisodeNumber)
            {
                if (sortreverse == false)
                {
                    episodenumbercompare myname = new episodenumbercompare();
                    TvMessages.Sort(myname);
                    LogDebug("messages sorted by episodenumber", (int)LogSetting.DEBUG);
                }
                else
                {
                    episodenumbercompare_r myname = new episodenumbercompare_r();
                    TvMessages.Sort(myname);
                    LogDebug("messages reverse sorted by episodenumber", (int)LogSetting.DEBUG);
                }
            }
            else if (type == (int)Sorting.EpisodePart)
            {
                if (sortreverse == false)
                {
                    episodepartcompare myname = new episodepartcompare();
                    TvMessages.Sort(myname);
                    LogDebug("messages sorted by episode part", (int)LogSetting.DEBUG);
                }
                else
                {
                    episodepartcompare_r myname = new episodepartcompare_r();
                    TvMessages.Sort(myname);
                    LogDebug("messages reverse sorted by episode part", (int)LogSetting.DEBUG);
                }
            }

            //LogDebug("sortmessage completed", (int)LogSetting.DEBUG);
        }


        public void filtermessages(bool _email,bool  _deleted, bool _conflicts, bool _scheduled, bool _recorded, bool _view, bool _unknown)
        { //"both" is included in filter
            Log.Debug("TVmessages before filermessages(): TvMessages.Count=" + TvMessages.Count.ToString());
            //Log.Debug("_email=" + _email.ToString());
            //Log.Debug("_unknown=" + _unknown.ToString());

            TvMessagesFiltered.Clear();
            for (int i=0;i<TvMessages.Count;i++)
            {
                xmlmessage onemessage = TvMessages[i];

                if ((onemessage.tvwishid == _FilterName) || (_FilterName == ""))
                {
                    
                        if (((onemessage.type == MessageType.Emailed.ToString()) && (_email)) || ((onemessage.type == MessageType.Deleted.ToString()) && (_deleted)) || ((onemessage.type == MessageType.Both.ToString()) && (_scheduled || _email)) || ((onemessage.type == MessageType.Conflict.ToString()) && (_conflicts)) || ((onemessage.type == MessageType.Scheduled.ToString()) && (_scheduled)) || ((onemessage.type == MessageType.Recorded.ToString()) && (_recorded)) || ((onemessage.type == MessageType.Viewed.ToString()) && (_view)))
                        {
                            onemessage.unfiltered_index = i;
                            TvMessagesFiltered.Add(onemessage);
                        }
                    
                }
            }
            Log.Debug("TVmessages after filermessages(): TvMessagesFiltered.Count=" + TvMessagesFiltered.Count.ToString());
        }

        public void FilterExistingWishes(List<TvWish> allWishes)
        {
            LogDebug("TVmessages before filer: TvMessages.Count=" + TvMessages.Count.ToString(), (int)LogSetting.DEBUG);

            TvMessagesFiltered.Clear();
            for (int i = 0; i < TvMessages.Count; i++)
            {
                xmlmessage onemessage = TvMessages[i];
                bool found=false;
                foreach (TvWish mywish in allWishes)
                {
                    if (onemessage.searchstring == mywish.searchfor)
                    {
                        found = true;
                        break;
                    }
                }
                if (found==true)
                {
                    onemessage.unfiltered_index = i;
                    TvMessagesFiltered.Add(onemessage);
                }
            }
            LogDebug("TVmessages after filer: TvMessagesFiltered.Count=" + TvMessagesFiltered.Count.ToString(), (int)LogSetting.DEBUG);

        }


        public void unreadall()
        {
            //LogDebug("unreadall started", (int)LogSetting.DEBUG);
            try
            {
                for ( int i=0; i<TvMessages.Count;i++)
                {
                    xmlmessage onemessage = TvMessages[i];
                    onemessage.processed = false;
                    TvMessages[i] = onemessage;
                }
            }
            catch (Exception exc)
            {
                LogDebug("Error: marking email messages failed with exception " + exc.Message, (int)LogSetting.ERROR);
            }
            //LogDebug("Unread completed", (int)LogSetting.DEBUG);
        }

        public string emailmessages(bool onlynew)
        {
            //LogDebug("filteremailmessages started", (int)LogSetting.DEBUG);

            string messagestring = String.Empty;

            try
            {
                for (int i = 0; i < TvMessagesFiltered.Count; i++)
                {
                    xmlmessage onemessage = TvMessagesFiltered[i];
                    if ((onemessage.processed == false) || (onlynew == false))
                    {                       
                            onemessage.processed = true;
                            TvMessages[onemessage.unfiltered_index] = onemessage; //mark unfiltered list array as processed as only unfiltered list will be weritten to file
                            messagestring += FormatMessage(onemessage, _emailformat);  //add new message to email text                                                
                    }
                }                
            }
            catch (Exception exc)
            {
                LogDebug("Error: filtering email messages failed with exception " + exc.Message, (int)LogSetting.ERROR);
            }
            //LogDebug("Filtering completed", (int)LogSetting.DEBUG);
            return messagestring;
        }




        public string FormatDateTime(DateTime mydate)
        {
            string formatted_datetime_string = "";
            try
            {
                formatted_datetime_string = String.Format(_dateTimeFormat,mydate.Year,mydate.Month,mydate.Day,mydate.Hour,mydate.Minute,mydate.Second,mydate.TimeOfDay,mydate.Date);
            }
            catch
            {
                formatted_datetime_string = "Error in format string:\n" + _dateTimeFormat;
            }

            //LogDebug("formatted_datetime_string= " + formatted_datetime_string.Replace('{', '['), (int)LogSetting.DEBUG); 

            //   {0}   year
            //   {1}   month
            //   {2}   day
            //   {3}   hour
            //   {4}   minute
            //   {5}   second
            //   {6}   timeofday
            //   {7}   date

            return formatted_datetime_string;
        }

        public string FormatMessage(xmlmessage onemessage, string format )
        {
            string _formattedstring="";
            //LogDebug("format= " + format.Replace('{', '_').Replace('}', '_'), (int)LogSetting.DEBUG); 
            format = format.Replace("{21}", onemessage.message);

            try
            {
                _formattedstring = String.Format(format, onemessage.title, onemessage.description, onemessage.channel,
                                FormatDateTime(onemessage.start), FormatDateTime(onemessage.end), onemessage.EpisodeName, onemessage.EpisodeNum, onemessage.EpisodeNumber, onemessage.EpisodePart,
                                onemessage.Genre, onemessage.Classification, FormatDateTime(onemessage.OriginalAirDate), onemessage.ParentalRating.ToString(), onemessage.SeriesNum.ToString(), onemessage.StarRating.ToString(),
                                TypeTranslation(onemessage.type.ToString()), onemessage.channel_id.ToString(), FormatDateTime(onemessage.created), onemessage.searchstring, onemessage.tvwishid, onemessage.recordingid);
                _formattedstring = _formattedstring.Replace(@"\n", "\n"); //new
            }
            catch
            {
                _formattedstring = "Error in format string:\n" + format;
            }
            
            //  {0}  Title
            //  {1}  Description
            //  {2}  Channel
            //  {3}  Start
            //  {4}  End
            //  {5}  EpisodeName
            //  {6}  EpisodeNum
            //  {7}  EpisodeNumber
            //  {8}  EpisodePart
            //  {9}  Genre
            //  {10} Classification
            //  {11} OriginalAirDate
            //  {12} ParentalRating
            //  {13} SeriesNum
            //  {14} StarRating
            //  {15} type
            //  {16} channel_id
            //  {17} created
            //  {18} searchstring
            //  {19} tvwishid
            //  {20} recordingid
            //  {21}  Message   must be last item!!! do not forget to update above!! and emailformat in tvserver plugin
            //  \n    new line


            //filename not included for formats otherwise need update in languagefiles

            LogDebug("_formattedstring=" + _formattedstring+"\n", (int)LogSetting.DEBUG); 
            return _formattedstring;
        }


        public string TypeTranslation(string englishType)
        {
            if (englishType == MessageType.Viewed.ToString())
                return PluginGuiLocalizeStrings.Get(3251);

             else if (englishType == MessageType.Emailed.ToString())
                return PluginGuiLocalizeStrings.Get(3253);

             else if (englishType == MessageType.Scheduled.ToString())
                return PluginGuiLocalizeStrings.Get(3255);

             else if (englishType == MessageType.Recorded.ToString())
                return PluginGuiLocalizeStrings.Get(3257);

             else if (englishType == MessageType.Deleted.ToString())
                return PluginGuiLocalizeStrings.Get(3259);

             else if (englishType == MessageType.Conflict.ToString())
                return PluginGuiLocalizeStrings.Get(3261);

            Log.Error("Unknown message type = " + englishType);
            return englishType;
        }


        public void logmessages()
        {
            LogDebug("count = " + TvMessages.Count.ToString(), (int)LogSetting.DEBUG);
            foreach (xmlmessage onemessage in TvMessages)
            {
                logmessage(onemessage);
            }
        }

        public void logmessage(xmlmessage onemessage)
        {           
                LogDebug("***************MESSAGE********************", (int)LogSetting.DEBUG);
                LogDebug("Title=" + onemessage.title, (int)LogSetting.DEBUG);
                LogDebug("Description=" + onemessage.description, (int)LogSetting.DEBUG);
                LogDebug("Channel=" + onemessage.channel, (int)LogSetting.DEBUG);
                LogDebug("Start=" + onemessage.start.ToString(), (int)LogSetting.DEBUG);
                LogDebug("End=" + onemessage.end.ToString(), (int)LogSetting.DEBUG);
                LogDebug("EpisodeName=" + onemessage.EpisodeName, (int)LogSetting.DEBUG);
                LogDebug("EpisodeNum=" + onemessage.EpisodeNum, (int)LogSetting.DEBUG);
                LogDebug("EpisodeNumber=" + onemessage.EpisodeNumber, (int)LogSetting.DEBUG);
                LogDebug("EpisodePart=" + onemessage.EpisodePart, (int)LogSetting.DEBUG);
                LogDebug("Genre=" + onemessage.Genre, (int)LogSetting.DEBUG);
                LogDebug("Classification=" + onemessage.Classification, (int)LogSetting.DEBUG);
                LogDebug("OriginalAirDate=" + onemessage.OriginalAirDate.ToString(), (int)LogSetting.DEBUG);
                LogDebug("ParentalRating=" + onemessage.ParentalRating.ToString(), (int)LogSetting.DEBUG);
                LogDebug("SeriesNum=" + onemessage.SeriesNum.ToString(), (int)LogSetting.DEBUG);
                LogDebug("StarRating=" + onemessage.StarRating.ToString(), (int)LogSetting.DEBUG);
                LogDebug("program_id=" + onemessage.channel_id.ToString(), (int)LogSetting.DEBUG);
                LogDebug("type=" + onemessage.type, (int)LogSetting.DEBUG);
                LogDebug("message=" + onemessage.message, (int)LogSetting.DEBUG);
                LogDebug("created=" + onemessage.created.ToString(), (int)LogSetting.DEBUG);
                LogDebug("searchstring=" + onemessage.searchstring, (int)LogSetting.DEBUG);
                LogDebug("tvwishid=" + onemessage.tvwishid, (int)LogSetting.DEBUG);
                LogDebug("recordingid=" + onemessage.recordingid, (int)LogSetting.DEBUG);
                LogDebug("filename=" + onemessage.filename, (int)LogSetting.DEBUG);
                LogDebug("unfiltered_index=" + onemessage.unfiltered_index.ToString(), (int)LogSetting.DEBUG);
                LogDebug("TvMessages.Count=" + TvMessages.Count.ToString(), (int)LogSetting.DEBUG);
                LogDebug("****************END MESSAGE********************************", (int)LogSetting.DEBUG);
                LogDebug("", (int)LogSetting.DEBUG);
            
        }


        //-------------------------------------------------------------------------------------------------------------        
        // output a single schedule with all parameters
        //------------------------------------------------------------------------------------------------------------- 
#if (TV101 || TV11 || TV12)
        [CLSCompliant(false)]
        public void outputscheduletoresponse(Schedule schedule, Int32 this_setting)
        {
            //_debug = true;
            
            bool restoreDebug = _debug;
            _debug = true;
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



                  //Debug only for 1.2
                  //LogDebug("BitRateMode=" + schedule.BitRateMode.ToString(), this_setting);
#if (!MPTV2)
                  LogDebug("CacheKey=" + schedule.CacheKey.ToString(), this_setting);
                  LogDebug("DoesUseEpisodeManagement=" + schedule.DoesUseEpisodeManagement.ToString(), this_setting);
                  LogDebug("IsChanged=" + schedule.IsChanged.ToString(), this_setting);
                  LogDebug("IsManual=" + schedule.IsManual.ToString(), this_setting);
                  LogDebug("IsPersisted=" + schedule.IsPersisted.ToString(), this_setting);
                  LogDebug("RecommendedCard=" + schedule.RecommendedCard.ToString(), this_setting);
                  LogDebug("SessionBroker=" + schedule.SessionBroker.ToString(), this_setting);
                  LogDebug("ValidationMessages=" + schedule.ValidationMessages.ToString(), this_setting);
#endif
                  LogDebug("Canceled=" + schedule.Canceled.ToString(), this_setting);
                  LogDebug("Directory=" + schedule.Directory.ToString(), this_setting);                  
                  LogDebug("IdSchedule=" + schedule.IdSchedule.ToString(), this_setting);                 
                  LogDebug("KeepDate=" + schedule.KeepDate.ToString(), this_setting);
                  LogDebug("KeepMethod=" + schedule.KeepMethod.ToString(), this_setting);
                  LogDebug("MaxAirings=" + schedule.MaxAirings.ToString(), this_setting);
                  LogDebug("PostRecordInterval=" + schedule.PostRecordInterval.ToString(), this_setting);
                  LogDebug("PreRecordInterval=" + schedule.PreRecordInterval.ToString(), this_setting);
                  LogDebug("Priority=" + schedule.Priority.ToString(), this_setting);
                  LogDebug("Quality=" + schedule.Quality.ToString(), this_setting);
                  //LogDebug("QualityType=" + schedule.QualityType.ToString(), this_setting);
                  
                  LogDebug("Series=" + schedule.Series.ToString(), this_setting);
                  
                  LogDebug("Channel ID=            " + schedule.IdChannel, this_setting);
                
                  //end debug

#if (TV11 || TV12 || MP11 || MP12 || MP16)
                  try
                  { 
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
                      LogDebug("IdParentSchedule=" + schedule.IdParentSchedule.ToString(), this_setting);
                   }
                   catch(Exception exc){}
#endif
            }
            catch (Exception exc)
            {
                LogDebug("Error in retrieving EPG data for program" + schedule.ProgramName, (int)LogSetting.ERROR);
                LogDebug("Exception message was " + exc.Message, (int)LogSetting.ERROR);
            }

            LogDebug("*****************************END SCHEDULE****************************************", this_setting);
            LogDebug("", this_setting);
            _debug = restoreDebug;
        }
#endif

        //-------------------------------------------------------------------------------------------------------------        
        // output a single program with all parameters
        //------------------------------------------------------------------------------------------------------------- 
#if (TV101 || TV11 || TV12)
        [CLSCompliant(false)]
        public void outputprogramresponse(Program program, Int32 this_setting)
        {
            bool restoreDebug = _debug;
            _debug = true;
            LogDebug("***************************PROGRAM********************************************", this_setting);
            LogDebug("ProgramName=           " + program.Title, this_setting);
            LogDebug("Description=           " + program.Description, this_setting);
            LogDebug("Genre=           " + program.Genre, this_setting);
            LogDebug("Classification=           " + program.Classification, this_setting);
            LogDebug("EpisodeNum=           " + program.EpisodeNum, this_setting);
#if(TV11 || TV12 || MP11 || MP12 || MP16)
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
            _debug = restoreDebug;
        }
#endif

        //-------------------------------------------------------------------------------------------------------------        
        // output a single recording with all parameters
        //------------------------------------------------------------------------------------------------------------- 
#if (TV101 || TV11 || TV12)
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

            bool restoreDebug = _debug;
            _debug = true;
            LogDebug("*************************RECORDING*******************************************", this_setting);
            LogDebug("ProgramName=           " + recording.Title, this_setting);
            LogDebug("Description=           " + recording.Description, this_setting);
            LogDebug("Genre=           " + recording.Genre, this_setting);
            LogDebug("FileName=           " + recording.FileName, this_setting);
            LogDebug("TimesWatched=           " + recording.TimesWatched.ToString(), this_setting);
            LogDebug("StopTime=           " + recording.StopTime, this_setting);

#if(TV11 || TV12 || MP11 || MP12 || MP16)
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
            LogDebug("IdChannel=           " + recording.IdChannel, this_setting);
#if (!MPTV2)
            LogDebug("IdServer=           " + recording.IdServer, this_setting);
#endif

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
            _debug = restoreDebug;
        }
#endif


        //-------------------------------------------------------------------------------------------------------------        
        //log handling for debug, error and addmessage (return mals)
        //-------------------------------------------------------------------------------------------------------------                
        public void LogDebug(string text, int field)
        {
            //trigger message event

            if (field == (int)LogSetting.INFO)
            {
                Log.Debug("[TvWishList MessageClass]: " + text);
                //if (newmessage != null)
                //    newmessage(text, field);

            }
            else if ((field == (int)LogSetting.DEBUG) && (_debug == true))
            {
                if (_debug == true)
                {
                    Log.Debug("[TvWishList MessageClass]: " + text);
                    //if (newmessage != null)
                    //    newmessage(text, field);
                }

            }
            else if (field == (int)LogSetting.ERROR)
            {
                Log.Error("[TvWishList MessageClass]: " + text);
                Log.Debug("[TvWishList MessageClass]: " + text);
            }
            else if (field == (int)LogSetting.ERRORONLY)
            {
                Log.Error("[TvWishList MessageClass]: " + text);
                //if (newmessage != null)
                //    newmessage(text, field);

            }
            else
            {
                //Log.Error("TvWishList Error MailClass: Unknown message Code " + field.ToString(), (int)LogSetting.ERROR);
            }
        }

        #endregion

    }
}