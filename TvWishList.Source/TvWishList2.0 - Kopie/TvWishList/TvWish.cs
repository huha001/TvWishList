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
//Version 0.0.20
//*************************

using System;
using System.Globalization;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
//using System.Data;
//using System.Drawing;

using System.IO;
//using System.Linq;
//using System.Text;

using System.Xml;

using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Threading;


using MediaPortal.Plugins.TvWishList;

#if (MP11 || MP12 || MP16)
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
//using Log = TvLibrary.Log.huha.Log;
using TvDatabase;
#elif (MP2)

using MediaPortal.Common;
using MediaPortal.Common.Commands;
using MediaPortal.Common.General;
//using Mediaportal.Common.Logging;
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

using MediaPortal.Plugins.TvWishList.Items;
using MediaPortal.Plugins.TvWishListMP2.Models;
//using TvWishList;

#else


#if (MPTV2)
// native TV3.5 for MP2
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.Common.Utils;
using MediaPortal.Plugins.TvWishList.Items;

using Mediaportal.TV.Server.TVLibrary.Interfaces;


#else
// MP1 TV server
using TvDatabase;
using TvControl;
using TvEngine.Events;
using TvLibrary.Implementations;

using TvLibrary.Interfaces;
#endif




//using TvLibrary.Log.huha;

using MediaPortal.Plugins.TvWishList.Setup;
using TvEngine;



using MediaPortal.Plugins;
using TvEngine.PowerScheduler.Interfaces; 
#endif 
 

using Log = TvLibrary.Log.huha.Log;



namespace MediaPortal.Plugins.TvWishList
{
    public enum TvWishEntries
    {
        active = 0, 
        searchfor = 1,
        matchtype = 2,
        group = 3,
        recordtype = 4,
        action = 5,
        exclude = 6,
        viewed = 7,
        prerecord = 8,
        postrecord = 9,
        episodename = 10,
        episodepart = 11,
        episodenumber = 12,
        seriesnumber = 13,
        keepepisodes = 14,
        keepuntil = 15,
        recommendedcard = 16,
        priority = 17,
        aftertime = 18,
        beforetime = 19,
        afterdays = 20,
        beforedays = 21,
        channel = 22,
        skip = 23,
        name = 24,
        useFolderName = 25,
        withinNextHours = 26,
        scheduled = 27,
        tvwishid = 28,
        recorded=29,
        deleted = 30,
        emailed = 31,
        conflicts =32,
        episodecriteria = 33,
        preferredgroup = 34,
        includerecordings=35,
        end = 36 // last element
        //modify for listview table changes
    }

    public enum LogSetting
    {
        DEBUG = 1,
        ERROR,
        ERRORONLY,
        INFO,
        ADDRESPONSE
    }

    public class TvWish
    {
        public string active;               //0
        public bool b_active;
        public string searchfor;            //1
        public string matchtype;            //2
        public bool b_partialname;
        public bool b_wordmatch;
        public string s_SearchIn;
        public string group;                //3
        public string recordtype;           //4
        public string action;               //5
        public MessageType t_action;
        public string exclude;              //6
        public string viewed;               //7
        public int i_viewed;
        public string prerecord;            //8
        public int i_prerecord;
        public string postrecord;           //9
        public int i_postrecord;
        public string episodename;          //10
        public string episodepart;          //11
        public string episodenumber;        //12
        public int i_episodenumber;
        public string seriesnumber;         //13
        public int i_seriesnumber;
        public string keepepisodes;         //14
        public int i_keepepisodes;
        public string keepuntil;            //15
        public DateTime D_keepuntil;
        public int i_keepuntil;
        public int i_keepmethod;
        public string recommendedcard;      //16
        public int i_recommendedcard;
        public string priority;             //17
        public int i_priority;
        public string aftertime;            //18
        public int i_aftertime;
        public string beforetime;           //19
        public int i_beforetime;
        public string afterdays;            //20
        public int i_afterdays;
        public string beforedays;           //21
        public int i_beforedays;
        public string channel;              //22
        public string skip;                 //23   
        public bool b_skip;
        public string name;                 //24
        public string useFolderName;        //25
        public bool b_series;
        public bool b_useFoldername;
        public string withinNextHours;      //26
        public int i_withinNextHours;
        public string scheduled;            //27
        public int i_scheduled;
        public string tvwishid;             //28
        public string recorded;             //29
        public int i_recorded;
        public string deleted;              //30
        public int i_deleted;
        public string emailed;              //31
        public int i_emailed;
        public string conflicts;            //32
        public int i_conflicts;
        public string episodecriteria;      //33
        public bool b_episodecriteria_d;
        public bool b_episodecriteria_n;
        public bool b_episodecriteria_c;
        public string preferredgroup;       //34
        public string includeRecordings;    //35
        public bool b_includeRecordings;
    } //modify for listview table changes



    #if (TV101 || TV11 || TV12)
            [CLSCompliant(false)]
    #endif
    public class TvWishProcessing :IDisposable
    {

        #region Global Variables

#if (MP2)
        //Register global Services
        //ILogger Log = ServiceRegistration.Get<ILogger>();
        DialogCloseWatcher dialogCloseWatcher = null;
#endif

        //Instance
        public static TvWishProcessing _instance = null;
        public static TvWishProcessing Instance
        {
            get { return (TvWishProcessing)_instance; }
        }

        //Buttonlock
        private bool _buttonActive = false;
        public bool ButtonActive
        {
            get { return (bool)_buttonActive; }
            set { _buttonActive = value; }
        }

        //Buttonlock
        private bool _tvSetup = false;
        public bool TvSetup
        {
            get { return (bool)_tvSetup; }
            set { _tvSetup = value; }
        }

        List<TvWish> TvWishes = new List<TvWish>();

        //_bool translator is been set with user values after loadsettings in main screen at init() in MP loadsettings()
        public bool[] _boolTranslator;
        public bool[] _boolTranslatorbackup;

        bool _Debug = false;
        bool _DisableInfoWindow = true;
        bool _validSearchCriteria = true;
        //any changes in defaultvalues must be adopted in tv server plugin datagridview for null value!!!
        //string[] _DefaultValues = new string[] { "True", "", "Partial Title", "All Channels", "All", "Both", "", "0", "-1", "-1", "", "", "", "", "All", "Always", "Any", "0", "Any", "Any", "Any", "Any", "Any", "True", "", "None", "Any", "0", "-1", "0", "0", "0", "0", "Descr.+Name+Number", "All Channels", "False" };//modify for listview table changes
        string _DefaultFormatString = "True;;Partial Title;All Channels;All;Both;;0;-1;-1;;;;;All;Always;Any;0;Any;Any;Any;Any;Any;True;;Automatic;Any;0;-1;0;0;0;0;Descr.+Name+Number;All Channels;False";//modify for listview table changes      
        string[] _DefaultValues = null;

        int _FocusedWishIndex = 0;
        int _MaxTvWishId = 0;
        int _MaxNumber = 2147483647;
        bool _ViewOnlyMode = false;
        //string _DataCreatedBy = "NOT_DEFINED";
        string _DataLoadedBy = "NOT_DEFINED";
        string _LockingPluginname = "NONE";
        bool _EnableDowngrade = false;

        string PreRecord = "-1";
        string PostRecord = "-1";
        public IList<Channel> AllChannels = null;
        public IList<ChannelGroup> AllChannelGroups = null;
        public IList<RadioChannelGroup> AllRadioChannelGroups = null;
        public IList<Card> AllCards = null;
        char TvWishItemSeparator = ';';

#if (TV101 || TV11 || TV12)
        LanguageTranslation PluginGuiLocalizeStrings = new LanguageTranslation();
#endif

        #endregion

        #region Properties
        public bool Debug
        {
            get { return _Debug; }
            set { _Debug = value; 
#if (!MP2)
                   Log.DebugValue = _Debug; 
#endif
                 }
          
        }

        public bool ViewOnlyMode
        {
            get { return _ViewOnlyMode; }
            set { _ViewOnlyMode = value; }
        }

        public bool DisableInfoWindow
        {
            get { return _DisableInfoWindow; }
            set {_DisableInfoWindow = value; }
        }

        public bool EnableDowngrade
        {
            get { return _EnableDowngrade; }
            set { _EnableDowngrade = value; }
        }

        public bool ValidSearchCriteria
        {
            get { return _validSearchCriteria; }
            set { _validSearchCriteria = value; }
        }

        
        public int FocusedWishIndex
        {
            get { return _FocusedWishIndex; }
            set { _FocusedWishIndex = value; }
        }

        
        public int MaxTvWishId
        {
            get { return _MaxTvWishId; }
            set { _MaxTvWishId = value; }
        }

        public string[] DefaultValues
        {
            get { return _DefaultValues; }
            set { _DefaultValues = value; }
        }

        public string DefaultFormatString
        {
            get { return _DefaultFormatString; }           
        }

        public int MaxKeepEpisodes
        {
            get { return _MaxNumber; }

        }

        /*
        public string DataCreatedBy
        {
            get { return _DataCreatedBy; }
        }*/

        public string DataLoadedBy
        {
            get { return _DataLoadedBy; }
        }

        
        
        #endregion

        #region Constructor
        public TvWishProcessing() 
        {
           //initialize
            _instance = this;
            
            //Log.Debug("Initializing TvWishProcessing");

            //load MP language
            #if (MP11 || MP12 || MP16)
            PluginGuiLocalizeStrings.LoadMPlanguage();
            #elif (TV101 || TV11 || TV12)
            PluginGuiLocalizeStrings.ReadLanguageFile();
            #endif

            #if (MP2)
            Log.InitializeLogFile();
            #endif
            

            _boolTranslator = new bool[(int)TvWishEntries.end];
            _boolTranslatorbackup = new bool[(int)TvWishEntries.end];
            for (int i = 0; i < (int)TvWishEntries.end; i++)
            {
                _boolTranslator[i] = true;
                _boolTranslatorbackup[i] = true;
            }

            _DefaultValues = _DefaultFormatString.Split(TvWishItemSeparator);
        }

        public void Dispose()
        {
#if (MP2)
        //UnRegister 
            if (dialogCloseWatcher != null)
                dialogCloseWatcher.Dispose();
#endif
        }
        #endregion

      
        #region Methods
        //gets the default values from loadsetting of the Tvserver settings if available
        public void TvServerSettings(string preRecord, string postRecord, IList<ChannelGroup> allChannelGroups, IList<RadioChannelGroup> allRadioChannelGroups, IList<Channel> allChannels, IList<Card> allCards, char tvWishItemSeparator)

        {
            
            PreRecord = preRecord;
            _DefaultValues[(int)TvWishEntries.prerecord]=preRecord;
            PostRecord = postRecord;
            _DefaultValues[(int)TvWishEntries.postrecord] = postRecord;
            AllCards = allCards;
            AllChannelGroups = allChannelGroups;
            AllRadioChannelGroups = allRadioChannelGroups;
            AllChannels = allChannels;
            TvWishItemSeparator = tvWishItemSeparator;
        }

        public void Clear()
        {
            TvWishes.Clear();
        }

        public List<TvWish> ListAll()
        {
            return TvWishes;
        }

        public TvWish GetAtIndex(int index)
        {
            try
            {
                return TvWishes[index];
            }
            catch
            {
                return null;
            }
        }

        public int GetIndex(string tvwishid)
        {
            int index = -1;
            for (int i = 0; i < TvWishes.Count; i++)
            {
                
                if (tvwishid == TvWishes[i].tvwishid)
                {
                    return i;
                }
            }
            return index;
        }
        public int GetIndex(TvWish mywish)
        {
            int index = -1;
            for (int i = 0; i < TvWishes.Count; i++)
            {               
                if (mywish.tvwishid == TvWishes[i].tvwishid)
                {
                    return i;
                }
            }
            return index;
        }


        public TvWish GetAtTvWishId(int id)
        {
            int i = 0;
            foreach (TvWish mywish in TvWishes)
            {            
                try
                {
                    i = Convert.ToInt32(mywish.tvwishid);
                    if (i == id)
                    {
                        return mywish;
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public TvWish GetAtTvWishId(string id)
        {
            foreach (TvWish mywish in TvWishes)
            {
                if (mywish.tvwishid == id)
                {
                    return mywish;
                }
            }
            return null;
        }



        public TvWish RetrieveById(string id)
        {
            //Log.Debug("id=" + id);
            //Log.Debug("id.Length=" + id.Length.ToString());
            foreach (TvWish mywish in TvWishes)
            {
                //Log.Debug("checking mywish.tvwishid=" + mywish.tvwishid);
                //Log.Debug("checking mywish.tvwishid.Length=" + mywish.tvwishid.Length.ToString());
                if (mywish.tvwishid == id)
                {
                    //Log.Debug("retrieving mywish.tvwishid=" + mywish.tvwishid);
                    return mywish;
                }
            }
            //Log.Debug("retrieving null");
            return null;
        }

        public int RetrieveByName(string nameSearch)
        {
            foreach (TvWish mywish in TvWishes)
            {                
                if (mywish.name == nameSearch)
                {
                    int i = -1;
                    try
                    {
                        i = Convert.ToInt32(mywish.tvwishid);
                    }
                    catch
                    { }
                    return i;
                }
            }
            //Log.Debug("no match");
            return -1;
        }
        public int RetrieveBySearchFor(string searchforSearch)
        {
            foreach (TvWish mywish in TvWishes)
            {
                if (mywish.searchfor == searchforSearch)
                {
                    int i = -1;
                    try
                    {
                        i = Convert.ToInt32(mywish.tvwishid);
                    }
                    catch
                    { }
                    return i;
                }
            }
            Log.Debug("no match");
            return -1;
        }

        

        public void UpdateCounters(List<xmlmessage> xmlMessages)
        {
            foreach (TvWish mywish in TvWishes)
            {
                //reset counters
                mywish.i_viewed = 0;
                mywish.i_scheduled = 0;
                mywish.i_recorded = 0;
                mywish.i_deleted = 0;
                mywish.i_emailed = 0;
                mywish.i_conflicts = 0;
            }

            foreach (xmlmessage mymessage in xmlMessages)
            {
                TvWish mywish = RetrieveById(mymessage.tvwishid);
                if (mywish != null)
                {
                    //Log.Debug("mymessage.type="+mymessage.type.ToString()+" for id="+mywish.tvwishid);

                    if (mymessage.type == MessageType.Viewed.ToString())
                    {
                        mywish.i_viewed++;
                    }
                    else if (mymessage.type == MessageType.Scheduled.ToString())
                    {
                        mywish.i_scheduled++;
                        //Log.Debug("mywish.i_scheduled=" + mywish.i_scheduled.ToString());
                    }
                    else if (mymessage.type == MessageType.Recorded.ToString())
                    {
                        mywish.i_recorded++;
                    }
                    else if (mymessage.type == MessageType.Deleted.ToString())
                    {
                        mywish.i_deleted++;
                    }
                    else if (mymessage.type == MessageType.Emailed.ToString())
                    {
                        mywish.i_emailed++;
                    }
                    else if (mymessage.type == MessageType.Conflict.ToString())
                    {
                        mywish.i_conflicts++;
                    }
                    else if (mymessage.type == MessageType.Both.ToString())
                    {
                        mywish.i_scheduled++;
                        mywish.i_emailed++;
                    }

                    ReplaceAtTvWishId(mymessage.tvwishid, mywish);
                }
            }

            foreach (TvWish mywish in TvWishes)
            {
                //Update strings
                mywish.viewed = mywish.i_viewed.ToString();
                mywish.scheduled = mywish.i_scheduled.ToString();
                mywish.recorded = mywish.i_recorded.ToString();
                mywish.deleted = mywish.i_deleted.ToString();
                mywish.emailed = mywish.i_emailed.ToString();
                mywish.conflicts = mywish.i_conflicts.ToString();
            }
        }

        public void Add(TvWish newWish)
        {
            //Log.Debug("ADD newWish.name=" + newWish.name);
            TvWishes.Add(newWish);
        }
         
        public void ReplaceAtIndex(int index, TvWish newWish)
        {
            TvWishes[index] = newWish;
        }

        public bool ReplaceAtTvWishId(string tvwishid, TvWish newWish)
        {
            for (int i = 0; i < TvWishes.Count; i++)
            {
                if (TvWishes[i].tvwishid == tvwishid)
                {
                    TvWishes[i] = newWish;
                    return true;
                }
            }
            return false;
        }

        public void RemoveAtIndex(int index)
        {
            TvWishes.RemoveAt(index);
        }

        public string LegacyConversion(string row, char TvWishItemSeparator)
        { //convert into new names for version 1.2.0.17
            string[] columns = row.Split(TvWishItemSeparator);

            if (row == "")
                return "";

            try
            {
                //Log.Debug("LegacyConversion: columns[(int)TvWishEntries.recordtype]=" + columns[(int)TvWishEntries.recordtype]);
                //Log.Debug("LegacyConversion: columns[(int)TvWishEntries.useFolderName]=" + columns[(int)TvWishEntries.useFolderName]);

                if (columns[(int)TvWishEntries.recordtype] == "One Movie")
                {
                    columns[(int)TvWishEntries.recordtype] = "Only Once";
                }
                else if (columns[(int)TvWishEntries.recordtype] == "All Movies")
                {
                    columns[(int)TvWishEntries.recordtype] = "All";
                }
                else if (columns[(int)TvWishEntries.recordtype] == "One Episode")
                {
                    columns[(int)TvWishEntries.recordtype] = "Only Once";
                }
                else if (columns[(int)TvWishEntries.recordtype] == "All Episodes")
                {
                    columns[(int)TvWishEntries.recordtype] = "All";
                    columns[(int)TvWishEntries.useFolderName] = "Episode";
                }


                if (columns[(int)TvWishEntries.useFolderName].ToLower() == "true")
                {
                    columns[(int)TvWishEntries.useFolderName] = "Name";
                }
                else if (columns[(int)TvWishEntries.useFolderName].ToLower() == "false")
                {
                    columns[(int)TvWishEntries.useFolderName] = "None";
                }               
            }
            catch (Exception exc)
            {
                Log.Error("Error in legacy conversion of row \n"+row+"\n - Exception was:" + exc.Message);
            }
            return row;
        }

        public string CheckString(string alldatastring)
        {
            string[] rows = alldatastring.Split('\n');
            string checkedstring = string.Empty;
            foreach (string row in rows)
            {
                checkedstring += CheckRowEntries(row, TvWishItemSeparator) +"\n";
            }
            return checkedstring;
        }


        public string CheckRowEntries(string row, char TvWishItemSeparator)
        {
            string errorMessages = "";

            if (row == string.Empty)
            {
                return string.Empty;
            }

            row = CheckRowEntries(row, TvWishItemSeparator, ref errorMessages);           
            return row;
        }

        public string CheckRowEntries(string row, char TvWishItemSeparator, ref string errorMessages)
        {
            string[] columns = row.Split(TvWishItemSeparator);
            //Log.Debug("columns.Length="+columns.Length.ToString());


            string[] checked_columns = CheckColumnsEntries(columns, TvWishItemSeparator, ref errorMessages);
            //Log.Debug("checked_columns.Length=" + checked_columns.Length.ToString());

            /*//DEBUG ONLY !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            if (errorMessages != "")
            {
                MessageBox.Show(errorMessages, "Debug Error");
            }*/


            string checkedrow = "";

            checkedrow = checked_columns[0];
            for (int i = 1; i < checked_columns.Length; i++)
            {
                checkedrow += TvWishItemSeparator + checked_columns[i];
            }
            //Log.Debug("CheckRowEntries: checkedrow=" + checkedrow);
            return checkedrow;

        }

        public string[] CheckColumnsEntries(string[] columns, char TvWishItemSeparator,bool defaultformat)
        {
            try
            {
                if (defaultformat)
                    columns[(int)TvWishEntries.tvwishid] = "1";
            }
            catch { }
            string errorMessages = "";

            
            
            string[] checkedColumns = CheckColumnsEntries(columns, TvWishItemSeparator, ref errorMessages);
            /*
            //DEBUG ONLY !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            if (errorMessages != "")
            {
                MessageBox.Show(errorMessages, "Debug Error");                
            }*/
            return checkedColumns;
        }

        public string[] CheckColumnsEntries(string[] columns, char TvWishItemSeparator, ref string errorMessages)
        {
            //Log.Debug("CheckColumnsEntries(string[] columns, char TvWishItemSeparator, ref bool ok, ref int errorColumn, ref string errorItem, ref string errorFormat, int number)");

            //default values as placeholders for later real values
            string[] defaultcolumns = (string[])_DefaultValues.Clone();

            //Log.Debug("defaultcolumns.Length=" + defaultcolumns.Length.ToString());


            
            /*
            //for debug only
            int ctr = 0;
            foreach (string myvalue in defaultcolumns)
            {
                Log.Debug("ctr=" + ctr.ToString());
                Log.Debug("value=" + myvalue);
                ctr++;
            }
            Log.Debug("");

            ctr = 0;
            foreach (string myvalue in columns)
            {
                Log.Debug("ctr=" + ctr.ToString());
                Log.Debug("value=" + myvalue);
                ctr++;
            }
            Log.Debug("");*/

            try
            {
                
                int colcount;

                if (defaultcolumns.Length < columns.Length)
                    colcount = defaultcolumns.Length;
                else
                    colcount = columns.Length;

                //Log.Debug("colcount=" + colcount.ToString());
                errorMessages = "";

                if (colcount > 0)//Active
                {
                    bool dummy = false;
                    ConvertString2Bool(columns[0], ref defaultcolumns[0], ref dummy, "Active", ref errorMessages);            
                }

                if (colcount > 1) //SearchFor
                {
                    defaultcolumns[1] = columns[1];
                }

                if (colcount > 2)//MatchType
                {
                    bool dummyPartialname=false;
                    bool dummyWordmatch=false;
                    string searchIn="";
                    ConvertString2MatchType(columns[2], ref defaultcolumns[2], ref dummyPartialname, ref dummyWordmatch, ref searchIn, ref errorMessages);

                }
                if (colcount > 3)
                {
                    if ((AllChannelGroups != null) && (AllRadioChannelGroups != null))//Group
                    {
                        bool foundflag = false;

                        foreach (ChannelGroup channelgroup in AllChannelGroups)
                        {
                            if (channelgroup.GroupName == columns[3])
                            {
                                foundflag = true;
                            }
                        }
                        foreach (RadioChannelGroup radiochannelgroup in AllRadioChannelGroups)
                        {
                            if (radiochannelgroup.GroupName == columns[3])
                            {
                                foundflag = true;
                            }
                        }

                        if (foundflag == false)
                        {
                            errorMessages += "*Error: Unknown value=" + columns[3] + " \nfor keyword GROUP \n resetting to " + defaultcolumns[3] + "\n\n";
                            Log.Error("*Error: Unknown value=" + columns[3] + " \nfor keyword GROUP \n resetting to " + defaultcolumns[3] + "\n\n");
                        }
                        else
                        {
                            defaultcolumns[3] = columns[3];
                        }
                    }
                    else //no groups available cannot check
                    {
                        defaultcolumns[3] = columns[3];
                    }
                }

                if (colcount > 4)//RecordType
                {
                    if ((columns[4] != "Only Once") && (columns[4] != "All") && (columns[4] != "All Same Channel") && (columns[4] != "All Same Channel Day") && (columns[4] != "All Same Channel Time") && (columns[4] != "All Same Channel Day Time"))
                    {
                        errorMessages += "*Error: Unknown value=" + columns[4] + " \nfor keyword RECORDTYPE \n resetting to " + defaultcolumns[4] + "\n\n";
                        Log.Error("*Error: Unknown value=" + columns[4] + " \nfor keyword RECORDTYPE \n resetting to " + defaultcolumns[4] + "\n\n");                       
                    }
                    else
                    {
                        defaultcolumns[4] = columns[4];
                    }
                }

                if (colcount > 5)//Action
                {
                    MessageType dummyType = MessageType.Both;
                    //Log.Debug("columns[5]=" + columns[5]);
                    //Log.Debug("defaultcolumns[5]=" + defaultcolumns[5]);
                    ConvertString2Action(columns[5], ref defaultcolumns[5], ref dummyType, ref errorMessages);
                }

                if (colcount > 6) //Exclude
                {
                    defaultcolumns[6] = columns[6];
                }

                if (colcount > 7) //Viewed
                {
                    int dummy = 0;
                    ConvertString2Int(columns[7], ref defaultcolumns[7], ref dummy, 0, 0, _MaxNumber, "Viewed", "0", 0, ref errorMessages);
                }

                if (colcount > 8) //PreRecord
                {
                    int dummy = 0;
                    ConvertString2Int(columns[8], ref defaultcolumns[8], ref dummy, -1, -1, _MaxNumber, "PreRecord", "0", 0, ref errorMessages);
                }

                if (colcount > 9) //PostRecord
                {
                    int dummy = 0;
                    ConvertString2Int(columns[9], ref defaultcolumns[9], ref dummy, -1, -1, _MaxNumber, "PostRecord", "0", 0, ref errorMessages);
                }

                if (colcount > 10) //EpisodeName
                {
                    defaultcolumns[10] = columns[10];
                }

                if (colcount > 11) //EpisodePart
                {
                    defaultcolumns[11] = columns[11];
                }

                if (colcount > 12) //EpisodeNumber   
                {
                    int dummy = 0;
                    ConvertString2IntExpression(columns[12], ref defaultcolumns[12], ref dummy, -1, 1, _MaxNumber, "EpisodeNumber", "", -1, ref errorMessages);
                }

                if (colcount > 13) //SeriesNumber   
                {
                    int dummy = 0;
                    ConvertString2IntExpression(columns[13], ref defaultcolumns[13], ref dummy, -1, 1, _MaxNumber, "SeriesNumber", "", -1, ref errorMessages);
                }                

                if (colcount > 14) //KeepEpisodes
                {
                    int dummy = 0;
                    ConvertString2Int(columns[14], ref defaultcolumns[14], ref dummy, -1, 1, _MaxNumber, "KeepEpisodes", "All", _MaxNumber, ref errorMessages);
                }

                if (colcount > 15) //KeepUntil
                {
                    int dummy_i1 = 0;
                    int dummy_i2= 0;
                    DateTime dummy_d = DateTime.Now;
                    ConvertString2KeepUntil(columns[15], ref defaultcolumns[15], ref dummy_i1, ref dummy_i2, ref dummy_d, ref errorMessages);
                }

                if ((colcount > 16) && (AllCards != null)) //RecommendedCard
                {
                    int dummy = 0;
                    ConvertString2Int(columns[16], ref defaultcolumns[16], ref dummy, -1, 1, AllCards.Count, "RecommendedCard", "Any", -1, ref errorMessages);                   
                }

                if (colcount > 17) //Priority
                {
                    int dummy = 0;
                    ConvertString2Int(columns[17], ref defaultcolumns[17], ref dummy, 0, 0, 9, "Priority", "", 0, ref errorMessages);
                }                

                if (colcount > 18) //After Time
                {
                    int dummy = 0;
                    ConvertString2Time(columns[18], ref defaultcolumns[18], ref dummy, "After Time", ref errorMessages);
                }

                if (colcount > 19) //Before Time
                {
                    int dummy = 0;
                    ConvertString2Time(columns[19], ref defaultcolumns[19], ref dummy, "Before Time", ref errorMessages);                     
                }
                if (colcount > 20)//After day
                {
                    int dummy = 0;
                    ConvertString2Day(columns[20], ref defaultcolumns[20], ref dummy, "After Day", ref errorMessages);                    
                }
                if (colcount > 21)//Before day
                {
                    int dummy = 0;
                    ConvertString2Day(columns[21], ref defaultcolumns[21], ref dummy, "Before Day", ref errorMessages);                   
                }
                if (colcount > 22) //Channel
                {
                    defaultcolumns[22] = columns[22];
                }
                if (colcount > 23) //skip
                {
                    bool dummy = true;                   
                    ConvertString2Bool(columns[23], ref defaultcolumns[23], ref dummy, "Skip", ref errorMessages); 
                }
                if (colcount > 24) //Name
                {
                    defaultcolumns[24] = columns[24];
                    if ((defaultcolumns[24] == "") && (defaultcolumns[1] != ""))
                    {
                        defaultcolumns[24] = defaultcolumns[1]; //if empty same as SearchFor
                    }
                }
                if (colcount > 25) //UseNameFolder
                {
                    //Log.Debug("columns[25]=" + columns[25]);
                    //Log.Debug("defaultcolumns[25]=" + defaultcolumns[25]);
                    bool dummy_b1 = true;
                    bool dummy_b2 = true;
                    ConvertString2UseFolder(columns[25], ref defaultcolumns[25], ref dummy_b1, ref dummy_b2, ref errorMessages);
                }
                if (colcount > 26) //WithinNextHours
                {
                    int dummy = 0;
                    ConvertString2Int(columns[26], ref defaultcolumns[26], ref dummy, -1, 1, _MaxNumber, "WithinNextHours", "Any", -1, ref errorMessages);
                }

                if (colcount > 27) //Scheduled
                {
                    int dummy = 0;
                    ConvertString2Int(columns[27], ref defaultcolumns[27], ref dummy, 0, 0, _MaxNumber, "Scheduled", "0", 0, ref errorMessages);
                }

                if (colcount > 28) //ID
                {
                    
                    //Log.Debug("columns[28]=" + columns[28]);

                    try
                    {
                        int k = Convert.ToInt32(columns[28]);
                        if (k >= 1) 
                        {
                            defaultcolumns[28] = k.ToString();
                        }
                        else
                        {                               
                                _MaxTvWishId++;  //need to add new id for legacy conversion, but not for defaultformat
                                defaultcolumns[28] = _MaxTvWishId.ToString();
                                errorMessages += "*Error: Unknown value=" + columns[28] + " \nfor keyword ID \n- creating new id=" + defaultcolumns[28] + "\n\n";
                                Log.Error("*Error: Unknown value=" + columns[28] + " \nfor keyword ID \n- creating new id=" + defaultcolumns[28] + "\n\n");                           
                        }
                    }
                    catch //update id
                    {
                        
                            _MaxTvWishId++;
                            defaultcolumns[28] = _MaxTvWishId.ToString();
                            errorMessages += "*Error: Unknown value=" + columns[28] + " \nfor keyword ID \n resetting to " + defaultcolumns[28] + "\n\n";
                            Log.Error("*Error: Unknown value=" + columns[28] + " \nfor keyword ID \n resetting to " + defaultcolumns[28] + "\n\n");
                        
                    }

                    //Log.Debug("defaultcolumns[ID]=" + defaultcolumns[28].ToString());
                }

                if (colcount > 29) //Recorded
                {
                   int dummy = 0;
                   ConvertString2Int(columns[29], ref defaultcolumns[29], ref dummy, 0, 0, _MaxNumber, "Recorded", "0", 0, ref errorMessages);
                }

                if (colcount > 30) //Deleted
                {
                   int dummy = 0;
                   ConvertString2Int(columns[30], ref defaultcolumns[30], ref dummy, 0, 0, _MaxNumber, "Deleted", "0", 0, ref errorMessages);
                }
                
                if (colcount > 31) //Emailed
                {
                   int dummy = 0;
                   ConvertString2Int(columns[31], ref defaultcolumns[31], ref dummy, 0, 0, _MaxNumber, "Emailed", "0", 0, ref errorMessages);
                }

                
                if (colcount > 32) //Conflicts
                {
                   int dummy = 0;
                   ConvertString2Int(columns[32], ref defaultcolumns[32], ref dummy, 0, 0, _MaxNumber, "Conflicts", "0", 0, ref errorMessages);
                }
                
                if (colcount > 33) //EpisodeCriteria
                {
                    //bool dummy_b0 = true;
                    bool dummy_b1 = true;
                    bool dummy_b2 = true;
                    bool dummy_b3 = true;
                    //bool dummy_b4 = true;
                    //ConvertString2EpisodeCriteria(columns[33], ref defaultcolumns[33],ref dummy_b0, ref dummy_b1, ref dummy_b2, ref dummy_b3, ref dummy_b4, ref errorMessages);
                    ConvertString2EpisodeCriteria(columns[33], ref defaultcolumns[33], ref dummy_b1, ref dummy_b2, ref dummy_b3, ref errorMessages);
                }
                
                if (colcount > 34)
                {
                    if ((AllChannelGroups != null) && (AllRadioChannelGroups != null))//PreferredGroup
                    {
                        bool foundflag = false;

                        foreach (ChannelGroup channelgroup in AllChannelGroups)
                        {
                            if (channelgroup.GroupName == columns[34])
                            {
                                foundflag = true;
                            }
                        }
                        foreach (RadioChannelGroup radiochannelgroup in AllRadioChannelGroups)
                        {
                            if (radiochannelgroup.GroupName == columns[34])
                            {
                                foundflag = true;
                            }
                        }

                        if (foundflag == false)
                        {
                            errorMessages += "*Error: Unknown value=" + columns[34] + " \nfor keyword PREFERRED GROUP \n resetting to " + defaultcolumns[34] + "\n\n";
                            Log.Error("*Error: Unknown value=" + columns[34] + " \nfor keyword REFERRED GROUP \n resetting to " + defaultcolumns[34] + "\n\n");
                        }
                        else
                        {
                            defaultcolumns[34] = columns[34];
                        }
                    }
                    else //no groups available cannot check
                    {
                        defaultcolumns[34] = columns[34];
                    }
                }
                
                if (colcount > 35) //IncludeRecordings
                {
                    bool dummy = true;
                    ConvertString2Bool(columns[35], ref defaultcolumns[35], ref dummy, "IncludeRecordings", ref errorMessages);
                }


                //modify for listview table changes
               
            }
            catch (Exception exc)
            {
                errorMessages += "Error in checking row entry - Exception was:\n" + exc.Message+"\n\n";
                Log.Error("Error in checking row entry - Exception was:" + exc.Message + "\n\n");
            }

            return defaultcolumns;
        }
        
        public void ConvertString2Bool(string checkingValue, ref string stringValue, ref bool boolValue, string item, ref string errorMessage)
        {
            string defaultStringValue = stringValue; //initial stringvalue is default value

            if (checkingValue.ToLower() == "true") 
            {
                boolValue = true;
                stringValue = boolValue.ToString();
            }
            else if (checkingValue.ToLower() == "false")
            {
                boolValue = false;
                stringValue = boolValue.ToString();
            }
            else //not correct
            {
                if (defaultStringValue.ToLower() == "true") //try default if correct format
                {
                    boolValue = true;
                }
                else //use false for incorrect default format
                {
                    boolValue = false;
                }
                
                stringValue = boolValue.ToString();
                errorMessage += "*Error: Invalid value = " + checkingValue + " \nfor keyword " + item + " \n resetting to " + stringValue + "\n\n";
                Log.Error("*Error: Invalid defaultvalue = " + checkingValue + " \nfor keyword " + item + " \n resetting to " + stringValue + "\n\n");              
            }
            return;
        }

        public void ConvertString2Int(string checkingValue, ref string stringValue, ref int intValue, int defaultIntValue, int minValue, int maxValue, string item, string specialName, int specialValue, ref string errorMessage)
        {
            string defaultStringValue = stringValue; //initial stringvalue is default value
            try
            {
                if (checkingValue == specialName)
                {
                    stringValue = specialName;
                    intValue = specialValue;
                    return;
                }

                intValue = Convert.ToInt32(checkingValue);
                if ((intValue >= minValue)&&(intValue <= maxValue))
                {
                    stringValue = intValue.ToString();
                    return;
                }
                else
                {
                    intValue = defaultIntValue;
                    stringValue = defaultStringValue;
                    errorMessage += "*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n\n";
                    Log.Error("*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n with value = " + defaultIntValue.ToString()+"\n\n");
                    return;
                }
            }
            catch //do nothing and use default
            {
                intValue = defaultIntValue;
                stringValue = defaultStringValue;
                errorMessage += "*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n\n";
                Log.Error("*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n with value = " + defaultIntValue.ToString() + "\n\n");                   
                return;
            }
        }

        public void ConvertString2IntExpression(string checkingValue, ref string stringValue, ref int intValue, int defaultIntValue, int minValue, int maxValue, string item, string specialName, int specialValue, ref string errorMessage)
        {
            //Log.Debug("ConvertString2IntExpression start: errorMessage=" + errorMessage);
            string defaultStringValue = stringValue; //initial stringvalue is default value
            try
            {
                if (checkingValue == specialName)
                {
                    stringValue = specialName;
                    intValue = specialValue;
                    return;
                }


                //process expressions
                string[] expressionArray = checkingValue.Split(',');
                foreach (string myExpression in expressionArray)
                {
                    Log.Debug("myExpression=" + myExpression);
                    if (myExpression.Contains("-") == true)
                    {

                        Log.Debug("- case");
                        string[] numberarray = myExpression.Split('-');

                        Log.Debug("a");
                        Log.Debug("numberarray.Length=" + numberarray.Length.ToString());
                        Log.Debug("a2");
                        if (numberarray.Length == 2)
                        {
                            if ((numberarray[0] == string.Empty) || (numberarray[1] == string.Empty))
                            {  // a- or -a
                                Log.Debug("a- or -a case");
                                string temp = myExpression.Replace("-", string.Empty);
                                intValue = 0;
                                int.TryParse(temp, out intValue);

                                Log.Debug("+ Expression = " + checkingValue + " intValue=" + intValue.ToString());
                                if ((intValue >= minValue) && (intValue <= maxValue))
                                {//do nothing                           
                                }
                                else
                                {
                                    intValue = defaultIntValue;
                                    stringValue = defaultStringValue;
                                    errorMessage += "*Error: ConvertString2IntExpression() Expression is not matching min and max values for " + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n\n";
                                    Log.Error("*Error: ConvertString2IntExpression() Expression is not matching min and max values for " + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n with value = " + defaultIntValue.ToString() + "\n\n");
                                    return;
                                }


                            }
                            else //a-b
                            {
                                Log.Debug("a-b case");
                                intValue = 0;
                                int.TryParse(numberarray[0], out intValue);
                                Log.Debug("-2 Expression = " + checkingValue + " intValue1=" + intValue.ToString());
                                if ((intValue >= minValue) && (intValue <= maxValue))
                                {//do nothing                           
                                }
                                else
                                {
                                    intValue = defaultIntValue;
                                    stringValue = defaultStringValue;
                                    errorMessage += "*Error: Expression is not matching min and max values for " + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n\n";
                                    Log.Error("*Error: Expression is not matching min and max values for " + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n with value = " + defaultIntValue.ToString() + "\n\n");
                                    return;
                                }

                                intValue = 0;
                                int.TryParse(numberarray[1], out intValue);
                                Log.Debug("Expression = " + checkingValue + " intValue2=" + intValue.ToString());
                                if ((intValue >= minValue) && (intValue <= maxValue))
                                {//do nothing                           
                                }
                                else
                                {
                                    intValue = defaultIntValue;
                                    stringValue = defaultStringValue;
                                    errorMessage += "*Error: ConvertString2IntExpression() Expression is not matching min and max values for " + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n\n";
                                    Log.Error("*Error: ConvertString2IntExpression() Expression is not matching min and max values for " + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n with value = " + defaultIntValue.ToString() + "\n\n");
                                    return;
                                }
                            }
                        }
                        else
                        {
                            Log.Debug("expression error: numberarray.Length=" + numberarray.Length.ToString());
                            intValue = defaultIntValue;
                            stringValue = defaultStringValue;
                            errorMessage += "*Error: ConvertString2IntExpression() Unknown expression=" + numberarray + " in " + checkingValue + "\nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n\n";
                            Log.Error("*Error: ConvertString2IntExpression() Unknown expression=" + numberarray + " in " + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n with value = " + defaultIntValue.ToString() + "\n\n");
                            return;
                        }

                    }
                    else if (myExpression.Contains("+") == true)
                    {
                        Log.Debug("+ case");
                        string temp = myExpression.Replace("+", string.Empty);
                        intValue = 0;
                        int.TryParse(temp, out intValue);
                        Log.Debug("+ Expression = " + checkingValue + " intValue=" + intValue.ToString());
                        if ((intValue >= minValue) && (intValue <= maxValue))
                        {//do nothing                           
                        }
                        else
                        {
                            intValue = defaultIntValue;
                            stringValue = defaultStringValue;
                            errorMessage += "*Error: ConvertString2IntExpression() Expression is not matching min and max values for " + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n\n";
                            Log.Error("*Error: ConvertString2IntExpression() Expression is not matching min and max values for " + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n with value = " + defaultIntValue.ToString() + "\n\n");
                            return;
                        }
                    }
                    else //single number
                    {
                        Log.Debug("single number case");
                        intValue = 0;
                        int.TryParse(myExpression, out intValue);
                        Log.Debug("single number Expression = " + checkingValue + " intValue=" + intValue.ToString());
                    }
                    if ((intValue >= minValue) && (intValue <= maxValue))
                    {//do nothing                           
                    }
                    else
                    {
                        Log.Debug("error undefined case");
                        intValue = defaultIntValue;
                        stringValue = defaultStringValue;
                        errorMessage += "*Error: ConvertString2IntExpression() Expression is not matching min and max values for " + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n\n";
                        Log.Error("*Error: ConvertString2IntExpression() Expression is not matching min and max values for " + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n with value = " + defaultIntValue.ToString() + "\n\n");
                        return;
                    }
                }


                stringValue = checkingValue;
                //Log.Debug("ConvertString2IntExpression end: errorMessage=" + errorMessage);
                return;

            }
            catch //do nothing and use default
            {
                intValue = defaultIntValue;
                stringValue = defaultStringValue;
                errorMessage += "*Error: ConvertString2IntExpression() Unknown expression=" + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n\n";
                Log.Error("*Error: ConvertString2IntExpression() Unknown expression=" + checkingValue + " \nfor keyword " + item + " \n resetting to " + defaultStringValue + "\n with value = " + defaultIntValue.ToString() + "\n\n");
                return;
            }
        }

        public void ConvertString2Day(string checkingValue, ref string stringValue, ref int intValue, string item, ref string errorMessage)
        {
            string defaultStringValue = stringValue; //initial stringvalue is default value
            try 
            {
                stringValue = checkingValue; //asume correect value has been used

                if (checkingValue == "Any")
                {
                    intValue = -1;
                }
                else if (checkingValue == "Sunday")
                {
                    intValue = (int)DayOfWeek.Sunday;
                }
                else if (checkingValue == "Monday")
                {
                    intValue = (int)DayOfWeek.Monday;
                }
                else if (checkingValue == "Tuesday")
                {
                    intValue = (int)DayOfWeek.Tuesday;
                }
                else if (checkingValue == "Wednesday")
                {
                    intValue = (int)DayOfWeek.Wednesday;
                }
                else if (checkingValue == "Thursday")
                {
                    intValue = (int)DayOfWeek.Thursday;
                }
                else if (checkingValue == "Friday")
                {
                    intValue = (int)DayOfWeek.Friday;
                }
                else if (checkingValue == "Saturday")
                {
                    intValue = (int)DayOfWeek.Saturday;
                }
                else //not correct
                {
                    intValue = -1;
                    stringValue = defaultStringValue;
                    errorMessage += "*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + "\n  resetting to " + defaultStringValue + "\n\n";
                    Log.Error("*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + "\n  resetting to " + defaultStringValue + "\n with value = -1\n\n");
                }
            }
            catch //do nothing and use any (-1) default settings
            {
                intValue = -1;
                stringValue = defaultStringValue;
                errorMessage += "*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + "\n  resetting to " + defaultStringValue + "\n\n";
                Log.Error("*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + "\n  resetting to " + defaultStringValue + "\n with value = -1\n\n");
            }
            return;
        }

        public void ConvertString2Time(string checkingValue, ref string stringValue, ref int intValue, string item, ref string errorMessage)
        {
            string defaultStringValue = stringValue; //initial stringvalue is default value
            try
            {
                stringValue = checkingValue; //asume correct value has been used

                int hours = 0; // 1 or 2 digits
                int minutes = 0;
                string[] tokenarray = checkingValue.Split(':', '_');

                if (checkingValue == "Any")
                {
                    stringValue = checkingValue;
                    intValue = -1;
                }
                else if (tokenarray.Length >= 2)
                {
                    hours = 0;
                    int.TryParse(tokenarray[0], out hours);
                    minutes = 0;
                    int.TryParse(tokenarray[1], out minutes);
                    if ((hours >= 0) && (hours <= 23) && (minutes >= 0) && (minutes <= 59))
                    {
                        stringValue = hours.ToString("00") + ":" + minutes.ToString("00"); //ensure correct double digit format
                        intValue = hours * 60 + minutes;

                    }
                    else
                    {
                        errorMessage += "*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + "\n  resetting to " + defaultStringValue + "\n\n";
                        Log.Error("*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + "\n  resetting to " + defaultStringValue + "\n with value = -1\n\n");
                        stringValue = defaultStringValue;
                        intValue = -1;
                    }
                }
                else if (tokenarray.Length == 1)
                {
                    hours = 0;
                    int.TryParse(tokenarray[0], out hours);
                    minutes = 0;
                    if ((hours >= 0) && (hours <= 23) && (minutes >= 0) && (minutes <= 59))
                    {
                        stringValue = hours.ToString("00") + ":" + minutes.ToString("00"); //ensure correct double digit format
                        intValue = hours * 60 + minutes;

                    }
                    else
                    {
                        errorMessage += "*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + "\n  resetting to " + defaultStringValue + "\n\n";
                        Log.Error("*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + "\n  resetting to " + defaultStringValue + "\n with value = -1\n\n");
                        stringValue = defaultStringValue;
                        intValue = -1;
                    }
                }               
                else //error
                {
                    errorMessage += "*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + "\n  resetting to " + defaultStringValue + "\n\n";
                    Log.Error("*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + "\n  resetting to " + defaultStringValue + "\n with value = -1\n\n");
                    stringValue = defaultStringValue;
                    intValue = -1;
                }
            }
            catch //do nothing and use default
            {
                errorMessage += "*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + "\n  resetting to " + defaultStringValue + "\n\n";
                Log.Error("*Error: Unknown value=" + checkingValue + " \nfor keyword " + item + "\n  resetting to " + defaultStringValue + "\n with value = -1\n\n");
                stringValue = defaultStringValue;
                intValue = -1;
            }
            return;
        }

        public void ConvertString2MatchType(string checkingValue, ref string stringValue, ref bool partialname, ref bool wordmatch, ref string searchIn, ref string errorMessage)
        {
            string defaultStringValue = stringValue; //initial stringvalue is default value

            stringValue = checkingValue; //asume correct value has been used

            if (checkingValue == "Partial Title")
            {
                partialname = true;
                wordmatch = false;
                searchIn = "Title";
            }
            else if (checkingValue == "Exact Title")
            {
                partialname = false;
                wordmatch = false;
                searchIn = "Title";
            }
            else if (checkingValue == "Partial Text")
            {
                partialname = true;
                wordmatch = false;
                searchIn = "Text";
            }
            else if (checkingValue == "Word in Title")
            {
                partialname = true;
                wordmatch = true;
                searchIn = "Title";
            }
            else if (checkingValue == "Word in Text")
            {
                partialname = true;
                wordmatch = true;
                searchIn = "Text";
            }
            else if (checkingValue == "Word in Text/Title")
            {
                partialname = true;
                wordmatch = true;
                searchIn = "Both";
            }
            else if (checkingValue == "Expression")
            {
                partialname = false;
                wordmatch = false;
                searchIn = "Expression";
            }
            else if (checkingValue == "Partial Text/Title")
            {
                partialname = true;
                wordmatch = false;
                searchIn = "Both";
            }
            else //not correct
            {
                partialname = true;
                wordmatch = false;
                searchIn = "Title";
                stringValue = "Partial Title"; //use partial title and not default value
                errorMessage += "*Error: Unknown value=" + checkingValue + " \nfor keyword Match Type\n\n";
                Log.Error("*Error: Unknown value=" + checkingValue + " \nfor keyword Match Type\n  resetting to " + stringValue + "\n\n");
            }
        }

        public void ConvertString2UseFolder(string checkingValue, ref string stringValue, ref bool useFoldername, ref bool series, ref string errorMessage)
        {
            string defaultStringValue = stringValue; //initial stringvalue is default value
            //Log.Debug("defaultStringValue=" + defaultStringValue);
            //Log.Debug("stringValue=" + stringValue);

            stringValue = checkingValue; //asume correct value has been used

            if (checkingValue == "Episode")
            {
                series = true;
                useFoldername = false;
            }
            else if (checkingValue == "Name")
            {
                series = false;
                useFoldername = true;
            }
            else if (checkingValue == "None")
            {
                series = false;
                useFoldername = false;
            }
            else if (checkingValue == "Automatic")
            {
                series = false;
                useFoldername = false;
            }
            else //not correct
            {
                series = false;
                useFoldername = false;
                stringValue = "None"; //use not defaultvalue due to not initializing
                errorMessage += "*Error: Unknown value=" + checkingValue + " \n for keyword Use Folder\n resetting to " + stringValue + "\n\n";
                Log.Error("*Error: Unknown value=" + checkingValue + " \n for keyword Use Folder\n resetting to " + stringValue + "\n\n");
            }
        }

        //public void ConvertString2EpisodeCriteria(string checkingValue, ref string stringValue,ref bool  episodecriteria_a, ref bool episodecriteria_d, ref bool episodecriteria_p, ref bool episodecriteria_n, ref bool episodecriteria_c, ref string errorMessage)        
        public void ConvertString2EpisodeCriteria(string checkingValue, ref string stringValue,ref bool episodecriteria_d, ref bool episodecriteria_n, ref bool episodecriteria_c, ref string errorMessage)
        {
            string defaultStringValue = stringValue; //initial stringvalue is default value

            stringValue = checkingValue; //asume correct value has been used

            //string[] validVector = new string[] { "Automatic", "None", "Descr.", "Part", "Name", "Number", "Descr.+Part", "Descr.+Name", "Descr.+Number", "Part+Name", "Part+Number", "Name+Number", "Descr.+Part+Name", "Descr.+Part+Number", "Descr.+Name+Number", "Part+Name+Number", "Descr.+Part+Name+Number" };
            string[] validVector = new string[] { "None", "Descr.", "Name", "Number", "Descr.+Name", "Descr.+Number", "Name+Number", "Descr.+Name+Number" };


            bool found = false;
            for (int i = 0; i < validVector.Length; i++)
            {
                if (checkingValue == validVector[i])
                {
                    found = true;
                    break;
                }
            }

            if (found == false)
            {
                stringValue = defaultStringValue;
                errorMessage += "*Error: Unknown value=" + checkingValue + " \n for keyword Episode Criteria\n resetting to " + stringValue + "\n\n";
                Log.Error("*Error: Unknown value=" + checkingValue + " \n for keyword Episode Criteria\n resetting to " + stringValue + "\n\n");
            }


            /*if (stringValue=="Automatic")
            {
                //episodecriteria_a = true;
                episodecriteria_d = false;
                //episodecriteria_p = false;
                episodecriteria_n = false;
                episodecriteria_c = false;
            }
            else*/
            
            if (stringValue == "None")
            {
                //episodecriteria_a = false;
                episodecriteria_d = false;
                //episodecriteria_p = false;
                episodecriteria_n = false;
                episodecriteria_c = false;
            }
            else
            {
                if (stringValue.Contains("Descr."))
                    episodecriteria_d = true;
                else
                    episodecriteria_d = false;

                /*if (stringValue.Contains("Part"))
                    episodecriteria_p = true;
                else
                    episodecriteria_p = false;*/

                if (stringValue.Contains("Name"))
                    episodecriteria_n = true;
                else
                    episodecriteria_n = false;

                if (stringValue.Contains("Number")) //count
                    episodecriteria_c = true;
                else
                    episodecriteria_c = false;
            }
            
            
        }

        public void ConvertString2Action(string checkingValue, ref string stringValue, ref MessageType type, ref string errorMessage)
        {
            string defaultStringValue = stringValue; //initial stringvalue is default value
            

            stringValue = checkingValue; //asume correct value has been used

            //Log.Debug("defaultStringValue=" + defaultStringValue);
            //Log.Debug("stringValue=" + stringValue);


            if (checkingValue == "Record")
            {
                type=MessageType.Scheduled;
            }
            else if (checkingValue == "Email")
            {
                type=MessageType.Emailed;
            }
            else if (checkingValue == "Both")
            {
                type=MessageType.Both;
            }
            else if (checkingValue == "View")
            {
                type=MessageType.Viewed;
            }
            else //not correct
            {
                stringValue = defaultStringValue;
                if (defaultStringValue == "Record")
                {
                    type=MessageType.Scheduled;
                }
                else if (defaultStringValue == "Email")
                {
                    type=MessageType.Emailed;
                }
                else if (defaultStringValue == "Both")
                {
                    type=MessageType.Both;
                }
                else if (defaultStringValue == "View")
                {
                    type = MessageType.Viewed;
                }
                else //not correct
                {
                    stringValue = "Both";
                    type=MessageType.Both;
                }
            
                errorMessage += "*Error: Unknown value=" + checkingValue + " \n for keyword Action\n resetting to " + stringValue + "\n\n";
                Log.Error("*Error: Unknown value=" + checkingValue + " \n for keyword Action\n resetting to " + stringValue + "\n\n");
            }
        }

        public void ConvertString2KeepUntil(string checkingValue, ref string stringValue, ref int keepMethod, ref int keepDays, ref DateTime keepDate, ref string errorMessage)
        {
            //keepmethod 0=Until Space needed
            //keepmethod 1=Until Watched
            //keepmethod 2=Until Date
            //keepmethod 3=Always

            string defaultStringValue = stringValue; //initial stringvalue is default value

            stringValue = checkingValue; //asume correct value has been used


            if (checkingValue == "Always")
            {
                keepDays = -1;
                keepMethod = 3;
                keepDate = DateTime.ParseExact("9000-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);	//keep for ever;
            }
            else if (checkingValue == "Watched")
            {
                keepDays = -1;
                keepMethod = 1;
                keepDate = DateTime.ParseExact("9000-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);	//keep for ever;
            }
            else if (checkingValue == "Space")
            {
                keepDays = -1;
                keepMethod = 0;
                keepDate = DateTime.ParseExact("9000-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);	//keep for ever;
            }
            else if ((checkingValue.Contains("days"))||(checkingValue.Contains("Days")))
            {
                try
                {
                    checkingValue = checkingValue.Replace("days", "");
                    checkingValue = checkingValue.Replace("Days", "");
                    keepDays = 0;
                    int.TryParse(checkingValue, out keepDays);
                    keepMethod = 2;
                    Log.Debug(keepDays.ToString()+"days identified");
                }
                catch //do nothing and use default
                {
                    keepDays = -1;
                    keepMethod = 3;
                    keepDate = DateTime.ParseExact("9000-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);	//keep for ever;
                    errorMessage += "*Error: Unknown value=" + stringValue + " \n for keyword Keep Until \n resetting to Always\n\n";
                    Log.Error("*Error days: Unknown value=" + stringValue + " \n for keyword Keep Until\n resetting to Always\n\n");
                    stringValue = "Always";
                   
                }
            }
            else if ((checkingValue.Contains("weeks")) || (checkingValue.Contains("Weeks")))
            {
                try
                {
                    checkingValue = checkingValue.Replace("weeks", "");
                    checkingValue = checkingValue.Replace("Weeks", "");
                    keepDays = 0;
                    int.TryParse(checkingValue, out keepDays);
                    keepDays = keepDays * 7;
                    keepMethod = 2;
                    Log.Debug(keepDays.ToString() + "weeks identified");
                }
                catch //do nothing and use default
                {
                    keepDays = -1;
                    keepMethod = 3;
                    keepDate = DateTime.ParseExact("9000-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);	//keep for ever;
                    errorMessage += "*Error: Unknown value=" + stringValue + " \n for keyword Keep Until \n resetting to Always\n\n";
                    Log.Error("*Error months: Unknown value=" + stringValue + " \n for keyword Keep Until\n resetting to Always\n\n");
                    stringValue = "Always";
                }
            }
            else if ((checkingValue.Contains("months")) || (checkingValue.Contains("Months")))
            {
                try
                {
                    checkingValue = checkingValue.Replace("months", "");
                    checkingValue = checkingValue.Replace("Months", "");
                    keepDays = 0;
                    int.TryParse(checkingValue, out keepDays);
                    keepDays = keepDays * 31;
                    keepMethod = 2;
                    Log.Debug(keepDays.ToString() + "months identified");
                }
                catch //do nothing and use default
                {
                    keepDays = -1;
                    keepMethod = 3;
                    keepDate = DateTime.ParseExact("9000-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);	//keep for ever;
                    errorMessage += "*Error: Unknown value=" + stringValue + " \n for keyword Keep Until \n resetting to Always\n\n";
                    Log.Error("*Error years: Unknown value=" + stringValue + " \n for keyword Keep Until\n resetting to Always\n\n");
                    stringValue = "Always";
                }
            }
            else if (checkingValue.Contains("-"))
            {
                try
                {
                    DateTime k = DateTime.ParseExact(checkingValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    if (k > DateTime.Now)
                    {
                        stringValue = checkingValue;
                        Log.Debug(k.ToString() + "date identified");
                        keepDays = -1;
                        keepMethod = 2;
                        keepDate = k;
                    }
                    else
                    {
                        keepDays = -1;
                        keepMethod = 3;
                        keepDate = DateTime.ParseExact("9000-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);	//keep for ever;
                        errorMessage += "*Error: Unknown value=" + stringValue + " \n for keyword Keep Until \n resetting to Always\n\n";
                        Log.Error("*Error date: Unknown value=" + stringValue + " \n for keyword Keep Until\n resetting to Always\n\n");
                        stringValue = "Always";
                    }
                }
                catch //do nothing and use default
                {
                    keepDays = -1;
                    keepMethod = 3;
                    keepDate = DateTime.ParseExact("9000-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);	//keep for ever;
                    errorMessage += "*Error: Unknown value=" + stringValue + " \n for keyword Keep Until \n resetting to Always\n\n";
                    Log.Error("*Error date: Unknown value=" + stringValue + " \n for keyword Keep Until\n resetting to Always\n\n");
                    stringValue = "Always";
                }
            }
            else //incorrect user input
            {
                keepDays = -1;
                keepMethod = 3;
                keepDate = DateTime.ParseExact("9000-01-01_00:00", "yyyy-MM-dd_HH:mm", CultureInfo.InvariantCulture);	//keep for ever;
                errorMessage += "*Error: Unknown value=" + stringValue + " \n for keyword Keep Until \n resetting to Always\n\n";
                Log.Error("*Error any: Unknown value=" + stringValue + " \n for keyword Keep Until\n resetting to Always\n\n");
                stringValue = "Always";
            }
        }


        public void DebugTvWish(TvWish mywish)
        {
            Log.Debug("***************TVWISH********************");
            Log.Debug("mywish.active=" + mywish.active);
            Log.Debug("   mywish.b_active=" + mywish.b_active.ToString());
            Log.Debug("mywish.searchfor=" + mywish.searchfor);
            Log.Debug("mywish.matchtype=" + mywish.matchtype);
            Log.Debug("   mywish.b_partialname=" + mywish.b_partialname.ToString());
            Log.Debug("   mywish.b_wordmatch=" + mywish.b_wordmatch.ToString());
            Log.Debug("   mywish.s_SearchIn=" + mywish.s_SearchIn);
            Log.Debug("mywish.group=" + mywish.group);
            Log.Debug("mywish.recordtype=" + mywish.recordtype);
            Log.Debug("mywish.action=" + mywish.action);
            Log.Debug("   mywish.t_action=" + mywish.t_action.ToString());
            Log.Debug("mywish.exclude=" + mywish.exclude);
            Log.Debug("mywish.viewed=" + mywish.viewed);
            Log.Debug("   mywish.i_hits=" + mywish.i_viewed.ToString());
            Log.Debug("mywish.prerecord=" + mywish.prerecord);
            Log.Debug("   mywish.i_prerecord=" + mywish.i_prerecord.ToString());
            Log.Debug("mywish.postrecord=" + mywish.postrecord);
            Log.Debug("   mywish.i_postrecord=" + mywish.i_postrecord.ToString());
            Log.Debug("mywish.episodename=" + mywish.episodename);
            Log.Debug("mywish.episodepart=" + mywish.episodepart);
            Log.Debug("mywish.episodenumber=" + mywish.episodenumber);
            Log.Debug("   mywish.i_episodenumber=" + mywish.i_episodenumber.ToString());
            Log.Debug("mywish.seriesnumber=" + mywish.seriesnumber);
            Log.Debug("   mywish.i_seriesnumber=" + mywish.i_seriesnumber.ToString());
            Log.Debug("mywish.keepepisodes=" + mywish.keepepisodes);
            Log.Debug("   mywish.i_keepepisodes=" + mywish.i_keepepisodes.ToString());
            Log.Debug("mywish.keepuntil=" + mywish.keepuntil);
            Log.Debug("   mywish.i_keepuntil=" + mywish.i_keepuntil.ToString());
            Log.Debug("   mywish.i_keepmethod=" + mywish.i_keepmethod.ToString());
            Log.Debug("   mywish.D_keepuntil=" + mywish.D_keepuntil.ToString());
            Log.Debug("mywish.recommendedcard=" + mywish.recommendedcard);
            Log.Debug("   mywish.i_recommendedcard=" + mywish.i_recommendedcard.ToString());
            Log.Debug("mywish.priority=" + mywish.priority);
            Log.Debug("   mywish.i_priority=" + mywish.i_priority.ToString());
            Log.Debug("mywish.aftertime=" + mywish.aftertime);
            Log.Debug(   "mywish.i_aftertime=" + mywish.i_aftertime.ToString());
            Log.Debug("mywish.beforetime=" + mywish.beforetime);
            Log.Debug("   mywish.i_beforetime=" + mywish.i_beforetime.ToString());
            Log.Debug("mywish.afterdays=" + mywish.afterdays);
            Log.Debug("   mywish.i_afterdays=" + mywish.i_afterdays.ToString());
            Log.Debug("mywish.beforedays=" + mywish.beforedays);
            Log.Debug("   mywish.i_beforedays=" + mywish.i_beforedays.ToString());
            Log.Debug("mywish.channel=" + mywish.channel);
            Log.Debug("mywish.skip=" + mywish.skip);
            Log.Debug("   mywish.b_skip=" + mywish.b_skip.ToString());
            Log.Debug("mywish.name=" + mywish.name);
            Log.Debug("mywish.useFolderName=" + mywish.useFolderName);
            Log.Debug("   mywish.b_useFoldername=" + mywish.b_useFoldername.ToString());
            Log.Debug("   mywish.b_series=" + mywish.b_series.ToString());
            Log.Debug("mywish.withinNextHours=" + mywish.withinNextHours);           
            Log.Debug("mywish.scheduled=" + mywish.scheduled);
            Log.Debug("   mywish.i_scheduled=" + mywish.i_scheduled.ToString());
            Log.Debug("mywish.tvwishid=" + mywish.tvwishid);
            Log.Debug("mywish.recorded=" + mywish.recorded);
            Log.Debug("   mywish.i_recorded=" + mywish.i_recorded.ToString());
            Log.Debug("mywish.deleted=" + mywish.deleted);
            Log.Debug("   mywish.i_deleted=" + mywish.i_deleted.ToString());
            Log.Debug("mywish.emailed=" + mywish.emailed);
            Log.Debug("   mywish.i_emailed=" + mywish.i_emailed.ToString());
            Log.Debug("mywish.conflicts=" + mywish.conflicts);
            Log.Debug("   mywish.i_conflicts=" + mywish.i_conflicts.ToString());
            Log.Debug("mywish.episodecriteria=" + mywish.episodecriteria);
            //Log.Debug("   mywish.b_episodecriteria_a=" + mywish.b_episodecriteria_a.ToString());
            Log.Debug("   mywish.b_episodecriteria_d=" + mywish.b_episodecriteria_d.ToString());
            //Log.Debug("   mywish.b_episodecriteria_p=" + mywish.b_episodecriteria_p.ToString());
            Log.Debug("   mywish.b_episodecriteria_n=" + mywish.b_episodecriteria_n.ToString());
            Log.Debug("   mywish.b_episodecriteria_c=" + mywish.b_episodecriteria_c.ToString());
            Log.Debug("mywish.preferredgroup=" + mywish.preferredgroup);
            Log.Debug("mywish.includeRecordings=" + mywish.includeRecordings);
            Log.Debug("   mywish.b_includeRecordings=" + mywish.b_includeRecordings.ToString());
            Log.Debug("***************END TVWISH********************");
            Log.Debug("");
        }//modify for listview table changes


        
        public TvWish DefaultData()
        {
            //Log.Debug("DefaultData");
            //add a new Tvwish filled with defaultdata

            string[] columndata = (string[])_DefaultValues.Clone();
            //Log.Debug("columndata.Length=" + columndata.Length.ToString());
            TvWish mywish = new TvWish();

            try
            {

                mywish = CreateTvWish(true, columndata);
                
                if (mywish.prerecord == "-1")
                    mywish.prerecord = PreRecord;

                if (mywish.postrecord == "-1")
                    mywish.postrecord = PostRecord;

                //do not use columndata[28] as the id must be unique for deafult data, will be overwritten later
                MaxTvWishId++;
                mywish.tvwishid=MaxTvWishId.ToString();  //do not use columndata[28] as the id must be unique for deafult data
                //modify for listview table changes


                //reset to 0 to avoid overflow for very large numbers  MaxTvWishId must be a unique number id for each wish 
                if (MaxTvWishId > 10000000)
                {
                    int ctr = 0;
                    foreach (TvWish tempwish in ListAll())
                    {
                        ctr++;
                        tempwish.tvwishid = ctr.ToString();
                    }
                    MaxTvWishId = ctr + 1;
                    Log.Error("Too many tv wishes resetting all tvwish IDs and recounting");
                }//end reset


                if (_ViewOnlyMode == true)
                {
                    mywish.skip = mywish.skip.Replace("False", PluginGuiLocalizeStrings.Get(4001));
                }

                //DebugTvWish(mywish);
                
            }
            catch (Exception exc)
            {
                Log.Error("[TVWishListMP]:TvserverdatabaseLoadSettings: ****** Exception " + exc.Message);
                Log.Error("[TVWishListMP]:TvserverdatabaseLoadSettings:row=" + DefaultFormatString);
            }

            return mywish;
        }

        public TvWish CreateTvWish(bool createDefaultData, string[] itemData)
        {
            //Log.Debug("CreateTvWish");
            TvWish mywish = new TvWish();
            int errorCtr = 0;
            try
            {
                //Log.Debug("itemData.Length="+itemData.Length.ToString());
                //Log.Debug("_DefaultValues.Length=" + _DefaultValues.Length.ToString());


                int maxlength=itemData.Length;
                

                string errorMessages = ""; //errormessages will be just stored in log file
                mywish.active = _DefaultValues[0];
                if (maxlength > 0)
                    ConvertString2Bool(itemData[0], ref mywish.active, ref mywish.b_active, "active", ref errorMessages);

                errorCtr++;
                mywish.searchfor = _DefaultValues[1];
                if (maxlength > 1)
                    mywish.searchfor = itemData[1];

                errorCtr++;
                mywish.matchtype = _DefaultValues[2];
                if (maxlength > 2)
                    ConvertString2MatchType(itemData[2], ref mywish.matchtype, ref mywish.b_partialname, ref mywish.b_wordmatch, ref mywish.s_SearchIn, ref errorMessages);

                errorCtr++;
                mywish.group = _DefaultValues[3];
                if (maxlength > 3)
                    mywish.group = itemData[3];

                errorCtr++;
                mywish.recordtype = _DefaultValues[4];
                if (maxlength > 4)
                    mywish.recordtype = itemData[4];

                errorCtr++;
                mywish.action = _DefaultValues[5];
                if (maxlength > 5)
                    ConvertString2Action(itemData[5], ref mywish.action, ref mywish.t_action, ref errorMessages);

                
                //double check for incorrect default data in Email and ViewOnly Mode
                if ((_ViewOnlyMode == true) && (createDefaultData))
                    mywish.action = "View";  //only "view" is allowed
                else if ((_ViewOnlyMode == false) && (createDefaultData))
                {
                    if (mywish.action == "View")
                        mywish.action = "Both";  // "view is not allowed for email and record mode => change to "Both"
                }

                errorCtr++;
                mywish.exclude = _DefaultValues[6];
                if (maxlength > 6)
                    mywish.exclude = itemData[6];

                errorCtr++;
                mywish.viewed = _DefaultValues[7];
                if (maxlength > 7)
                    ConvertString2Int(itemData[7], ref mywish.viewed, ref mywish.i_viewed, 0, 0, _MaxNumber, "viewed", "", 0, ref errorMessages);

                errorCtr++;
                mywish.prerecord = _DefaultValues[8];
                if (maxlength > 8)
                    ConvertString2Int(itemData[8], ref mywish.prerecord, ref mywish.i_prerecord, -1, 0, _MaxNumber, "prerecord", "", 0, ref errorMessages);

                errorCtr++;
                mywish.postrecord = _DefaultValues[9];
                if (maxlength > 9)
                    ConvertString2Int(itemData[9], ref mywish.postrecord, ref mywish.i_postrecord, -1, 0, _MaxNumber, "postrecord", "", 0, ref errorMessages);

                errorCtr++;
                mywish.episodename = _DefaultValues[10];
                if (maxlength > 10)
                    mywish.episodename = itemData[10];

                errorCtr++;
                mywish.episodepart = _DefaultValues[11];
                if (maxlength > 11)
                    mywish.episodepart = itemData[11];

                errorCtr++;
                mywish.episodenumber = _DefaultValues[12];
                if (maxlength > 12)
                    ConvertString2IntExpression(itemData[12], ref mywish.episodenumber, ref mywish.i_episodenumber, -1, 1, _MaxNumber, "EpisodeNumber", "", -1, ref errorMessages);

                errorCtr++;
                mywish.seriesnumber = _DefaultValues[13];
                if (maxlength > 13)
                    ConvertString2IntExpression(itemData[13], ref mywish.seriesnumber, ref mywish.i_seriesnumber, -1, 1, _MaxNumber, "SeriesNumber", "", -1, ref errorMessages);

                errorCtr++;
                mywish.keepepisodes = _DefaultValues[14];
                if (maxlength > 14)
                    ConvertString2Int(itemData[14], ref mywish.keepepisodes, ref mywish.i_keepepisodes, -1, 1, _MaxNumber, "KeepEpisodes", "All", _MaxNumber, ref errorMessages);

                errorCtr++;
                mywish.keepuntil = _DefaultValues[15];
                if (maxlength > 15)
                    ConvertString2KeepUntil(itemData[15], ref mywish.keepuntil, ref mywish.i_keepmethod, ref mywish.i_keepuntil, ref mywish.D_keepuntil, ref errorMessages);

                errorCtr++;

                //Log.Debug("errorCtr=" + errorCtr.ToString());
                //Log.Debug("_DefaultValues[16]=" + _DefaultValues[16]);

                mywish.recommendedcard = _DefaultValues[16];
                if (maxlength > 16)
                {
                    //Log.Debug("AllCards.Count=" + AllCards.Count.ToString());
                    ConvertString2Int(itemData[16], ref mywish.recommendedcard, ref mywish.i_recommendedcard, -1, 1, AllCards.Count, "RecommendedCard", "Any", -1, ref errorMessages);
                }

                errorCtr++;
                //Log.Debug("errorCtr=" + errorCtr.ToString());
                mywish.priority = _DefaultValues[17];
                if (maxlength > 17)
                ConvertString2Int(itemData[17], ref mywish.priority, ref mywish.i_priority, 0, 0, 9, "Priority", "", 0, ref errorMessages);

                errorCtr++;
                mywish.aftertime = _DefaultValues[18];
                if (maxlength > 18)
                    ConvertString2Time(itemData[18], ref mywish.aftertime, ref mywish.i_aftertime, "After Time", ref errorMessages);

                errorCtr++;
                mywish.beforetime = _DefaultValues[19];
                if (maxlength > 19)
                    ConvertString2Time(itemData[19], ref mywish.beforetime, ref mywish.i_beforetime, "Before Time", ref errorMessages);

                errorCtr++;
                mywish.afterdays = _DefaultValues[20];
                if (maxlength > 20)
                    ConvertString2Day(itemData[20], ref mywish.afterdays, ref mywish.i_afterdays, "After Day", ref errorMessages);

                errorCtr++;
                mywish.beforedays = _DefaultValues[21];
                if (maxlength > 21)
                    ConvertString2Day(itemData[21], ref mywish.beforedays, ref mywish.i_beforedays, "Before Day", ref errorMessages);

                errorCtr++;
                mywish.channel = _DefaultValues[22];
                if (maxlength > 22)
                    mywish.channel = itemData[22];

                errorCtr++;
                mywish.skip = _DefaultValues[23];
                if (maxlength > 23)
                    ConvertString2Bool(itemData[23], ref mywish.skip, ref mywish.b_skip, "Skip", ref errorMessages);

                errorCtr++;
                mywish.name = _DefaultValues[24];
                if (maxlength > 24)
                    mywish.name = itemData[24];

                errorCtr++;
                mywish.useFolderName = _DefaultValues[25];
                if (maxlength > 25)
                    ConvertString2UseFolder(itemData[25], ref mywish.useFolderName, ref mywish.b_useFoldername, ref mywish.b_series, ref errorMessages);

                errorCtr++;
                mywish.withinNextHours = _DefaultValues[26];
                if (maxlength > 26)
                    ConvertString2Int(itemData[26], ref mywish.withinNextHours, ref mywish.i_withinNextHours, -1, 1, _MaxNumber, "WithinNextHours", "Any", -1, ref errorMessages);

                errorCtr++;
                mywish.scheduled = _DefaultValues[27];
                if (maxlength > 27)
                    ConvertString2Int(itemData[27], ref mywish.scheduled, ref mywish.i_scheduled, 0, 0, _MaxNumber, "Scheduled", "", 0, ref errorMessages);

                errorCtr++;
                mywish.tvwishid = _DefaultValues[28];
                if (maxlength > 28)
                    mywish.tvwishid = itemData[28];

                //double checking if _MaxTvWishId is screwed up
                try
                {
                    int number = 0;
                    int.TryParse(mywish.tvwishid, out number);
                    if (_MaxTvWishId < number)
                    {
                        Log.Error("Fatal Error: mywish.tvwishid=" + mywish.tvwishid);
                        Log.Error("Fatal Error: _MaxTvWishId=" + _MaxTvWishId.ToString());
                        _MaxTvWishId = number;
                        Log.Error("Resetting _MaxTvWishId to " + _MaxTvWishId.ToString());
                    }
                }
                catch (Exception exc)
                {
                    Log.Error("Could not convert tvwishID to number for " + mywish.tvwishid);
                    Log.Error("Exception " + exc.Message);
                }

                errorCtr++;
                mywish.recorded = _DefaultValues[29];
                if (maxlength > 29)
                    ConvertString2Int(itemData[29], ref mywish.recorded, ref mywish.i_recorded, 0, 0, _MaxNumber, "Recorded", "", 0, ref errorMessages);

                errorCtr++;
                mywish.deleted = _DefaultValues[30];
                if (maxlength > 30)
                    ConvertString2Int(itemData[30], ref mywish.deleted, ref mywish.i_deleted, 0, 0, _MaxNumber, "Deleted", "", 0, ref errorMessages);

                errorCtr++;
                mywish.emailed = _DefaultValues[31];
                if (maxlength > 31)
                    ConvertString2Int(itemData[31], ref mywish.emailed, ref mywish.i_emailed, 0, 0, _MaxNumber, "Emailed", "", 0, ref errorMessages);

                errorCtr++;
                mywish.conflicts = _DefaultValues[32];
                if (maxlength > 32)
                    ConvertString2Int(itemData[32], ref mywish.conflicts, ref mywish.i_conflicts, 0, 0, _MaxNumber, "Conflicts", "", 0, ref errorMessages);

                errorCtr++;
                mywish.episodecriteria = _DefaultValues[33];
                if (maxlength > 33)
                    //ConvertString2EpisodeCriteria(itemData[33], ref mywish.episodecriteria, ref mywish.b_episodecriteria_a, ref mywish.b_episodecriteria_d, ref mywish.b_episodecriteria_p, ref mywish.b_episodecriteria_n, ref mywish.b_episodecriteria_c, ref errorMessages);
                    ConvertString2EpisodeCriteria(itemData[33], ref mywish.episodecriteria,  ref mywish.b_episodecriteria_d, ref mywish.b_episodecriteria_n, ref mywish.b_episodecriteria_c, ref errorMessages);

                errorCtr++;
                mywish.preferredgroup = _DefaultValues[34];
                if (maxlength > 34)
                    mywish.preferredgroup = itemData[34];

                errorCtr++;
                mywish.includeRecordings = _DefaultValues[35];
                if (maxlength > 35)
                    ConvertString2Bool(itemData[35], ref mywish.includeRecordings, ref mywish.b_includeRecordings, "IncludeRecordings", ref errorMessages);
                //modify for listview table changes


                TvWishLanguageTranslation(ref mywish); //must come afte data                
            
                
            }
            catch (Exception exc)
            {
                Log.Error("CreateTvWish: ****** Exception in item " + errorCtr.ToString()+ " - Message is" + exc.Message);
            }
            return mywish;
        }


        public TvWish CreateTvWishNoCheckingNoTranslation(bool createDefaultData, string[] itemData)
        {
            //Log.Debug("CreateTvWish");
            TvWish mywish = new TvWish();
            int ctr=0;
            try
            {
                Log.Debug("itemData.Length=" + itemData.Length.ToString());
                int maxlength = itemData.Length;
                

                mywish.active = _DefaultValues[0];  
                if (maxlength > 0)
                    mywish.active = itemData[0];
                
                ctr++;
                mywish.searchfor = _DefaultValues[1];  
                if (maxlength > 1)
                    mywish.searchfor = itemData[1];

                ctr++;
                mywish.matchtype = _DefaultValues[2]; 
                if (maxlength > 2)
                    mywish.matchtype = itemData[2];

                ctr++;
                mywish.group = _DefaultValues[3];  
                if (maxlength > 3)
                    mywish.group = itemData[3];

                ctr++;
                mywish.recordtype = _DefaultValues[4];
                if (maxlength > 4)
                    mywish.recordtype = itemData[4];

                ctr++;
                mywish.action = _DefaultValues[5]; 
                if (maxlength > 5)
                    mywish.action = itemData[5];

                ctr++;
                mywish.exclude = _DefaultValues[6];
                if (maxlength > 6)
                    mywish.exclude = itemData[6];

                ctr++;
                mywish.viewed = _DefaultValues[7];
                if (maxlength > 7)
                    mywish.viewed = itemData[7];

                ctr++;
                mywish.prerecord = _DefaultValues[8];
                if (maxlength > 8)
                    mywish.prerecord = itemData[8];
                
                mywish.postrecord = _DefaultValues[9];
                if (maxlength > 9)
                    mywish.postrecord = itemData[9];

                ctr++;
                mywish.episodename = _DefaultValues[10];
                if (maxlength > 10)
                    mywish.episodename = itemData[10];

                ctr++;
                mywish.episodepart = _DefaultValues[11];
                if (maxlength > 11)
                    mywish.episodepart = itemData[11];

                ctr++;
                mywish.episodenumber = _DefaultValues[12];
                if (maxlength > 12)
                    mywish.episodenumber = itemData[12];

                ctr++;
                mywish.seriesnumber = _DefaultValues[13];
                if (maxlength > 13)
                    mywish.seriesnumber = itemData[13];

                mywish.keepepisodes = _DefaultValues[14];
                if (maxlength > 14)
                    mywish.keepepisodes = itemData[14];

                mywish.keepuntil = _DefaultValues[15];
                if (maxlength > 15)
                    mywish.keepuntil = itemData[15];

                ctr++;
                mywish.recommendedcard = _DefaultValues[16];
                if (maxlength > 16)
                    mywish.recommendedcard = itemData[16];
                
                ctr++;
                mywish.priority = _DefaultValues[17];
                if (maxlength > 17)
                    mywish.priority = itemData[17];

                ctr++;
                mywish.aftertime = _DefaultValues[18];
                if (maxlength > 18)
                    mywish.aftertime = itemData[18];

                ctr++;
                mywish.beforetime = _DefaultValues[19];
                if (maxlength > 19)
                    mywish.beforetime = itemData[19];

                ctr++;
                mywish.afterdays = _DefaultValues[20];
                if (maxlength > 20)
                    mywish.afterdays = itemData[20];

                ctr++;
                mywish.beforedays = _DefaultValues[21];
                if (maxlength > 21)
                    mywish.beforedays =itemData[21];

                ctr++;
                mywish.channel = _DefaultValues[22];
                if (maxlength > 22)
                    mywish.channel = itemData[22];
                
                ctr++;
                mywish.skip = _DefaultValues[23];
                if (maxlength > 23)
                    mywish.skip = itemData[23];

                ctr++;
                mywish.name = _DefaultValues[24];
                if (maxlength > 24)
                    mywish.name = itemData[24];

                ctr++;
                mywish.useFolderName = _DefaultValues[25];
                if (maxlength > 25)
                    mywish.useFolderName = itemData[25];

                ctr++;
                mywish.withinNextHours = _DefaultValues[26];
                if (maxlength > 26)
                    mywish.withinNextHours = itemData[26];

                ctr++;
                mywish.scheduled = _DefaultValues[27];
                if (maxlength > 27)
                    mywish.scheduled = itemData[27];

                ctr++;
                mywish.tvwishid = _DefaultValues[28];
                if (maxlength > 28)
                    mywish.tvwishid = itemData[28];

                ctr++;
                mywish.recorded = _DefaultValues[29];
                if (maxlength > 29)
                    mywish.recorded = itemData[29];

                ctr++;
                mywish.deleted = _DefaultValues[30];
                if (maxlength > 30)
                    mywish.deleted = itemData[30];

                ctr++;
                mywish.emailed = _DefaultValues[31];
                if (maxlength > 31)
                    mywish.emailed = itemData[31];

                ctr++;
                mywish.conflicts = _DefaultValues[32];
                if (maxlength > 32)
                    mywish.conflicts = itemData[32];

                ctr++;
                mywish.episodecriteria = _DefaultValues[33];
                if (maxlength > 33)
                    mywish.episodecriteria = itemData[33];

                ctr++;
                mywish.preferredgroup = _DefaultValues[34];
                if (maxlength > 34)
                    mywish.preferredgroup = itemData[34];

                ctr++;
                mywish.includeRecordings = _DefaultValues[35];
                if (maxlength > 35)
                    mywish.includeRecordings = itemData[35];
                //modify for listview table changes

            }
            catch (Exception exc)
            {
                Log.Error("CreateTvWishNoCheckingNoTranslation: ****** Exception for item "+ctr.ToString()+" Message is" + exc.Message);
            }
            return mywish;
        }


        public void LoadFromString(string listviewdata, bool allowsamename)
        {


            Log.Debug("LoadFromString: listviewdata=" + listviewdata);
            
            
            

            string[] rowdata = listviewdata.Split('\n');
            foreach (string row in rowdata)
            {
                //Log.Debug("row=" + row);

                string legacyrow = LegacyConversion(row, TvWishItemSeparator);
                Log.Debug("legacyrow=" + legacyrow);

                string[] columndata = legacyrow.Split(TvWishItemSeparator);
                Log.Debug("columndata.Length=" + columndata.Length.ToString());
                if (columndata.Length > 5)//avoid dummy rows
                {
                    //check before creating Tvwish
                    Log.Debug("x before row =" + row);
                    string errorMessages = "";
                    string checkedrow = CheckRowEntries(row, TvWishItemSeparator, ref errorMessages);
                    Log.Debug("xx after row =" + checkedrow);

                    try
                    {
                        TvWish mywish = new TvWish();
                        mywish = CreateTvWish(false, columndata);

                        //check for same name in list already
                        if (allowsamename == false)
                        {
                            foreach (TvWish testwish in TvWishes)
                            {
                                if (testwish.name == mywish.name)
                                {
                                    Log.Debug("Repeated name " + testwish.name + " found and skipped row");
                                    continue;
                                }
                            }
                        }


                        //check for valid search criteria
                        if ((mywish.searchfor != "") || (mywish.name != "") || (mywish.episodename != "") || (mywish.episodepart != "") || (mywish.episodenumber != "") || (mywish.seriesnumber != "") || (_validSearchCriteria ==false))
                        {

                            Add(mywish);
                            DebugTvWish(mywish);

                        }
                    }
                    catch (Exception exc)
                    {
                        Log.Error("[TVWishListMP]:TvserverdatabaseLoadSettings: ****** Exception " + exc.Message);
                        Log.Error("[TVWishListMP]:TvserverdatabaseLoadSettings:row=" + row);
                    }
                }
            }
        }


        public void LoadFromStringNoChecking(string listviewdata, bool allowsamename)
        {
            Log.Debug("LoadFromStringNoChecking: listviewdata=" + listviewdata);

            string[] rowdata = listviewdata.Split('\n');
            foreach (string row in rowdata)
            {
                
                string[] columndata = row.Split(TvWishItemSeparator);
                Log.Debug("columndata.Length=" + columndata.Length.ToString());
                if (columndata.Length > 5)//avoid dummy rows
                {
                    
                    try
                    {
                        TvWish mywish = new TvWish();
                        mywish = CreateTvWish(false, columndata);

                        //check for same name in list already
                        if (allowsamename == false)
                        {
                            foreach (TvWish testwish in TvWishes)
                            {
                                if (testwish.name == mywish.name)
                                {
                                    Log.Debug("Repeated name " + testwish.name + " found and skipped row");
                                    continue;
                                }
                            }
                        }


                        //check for valid search criteria
                        if ((mywish.searchfor != "") || (mywish.name != "") || (mywish.episodename != "") || (mywish.episodepart != "") || (mywish.episodenumber != "") || (mywish.seriesnumber != "") || (_validSearchCriteria == false))
                        {

                            Add(mywish);
                            DebugTvWish(mywish);                            
                        }
                    }
                    catch (Exception exc)
                    {
                        Log.Error("[TVWishListMP]:TvserverdatabaseLoadSettings: ****** Exception " + exc.Message);
                        Log.Error("[TVWishListMP]:TvserverdatabaseLoadSettings:row=" + row);
                    }
                }
            }
        }




        public string SaveToStringNoTranslation(ref TvWish mywish)
        {
            //DebugTvWish(mywish);
            string row = "";

            row += mywish.active + TvWishItemSeparator.ToString();
            row += mywish.searchfor + TvWishItemSeparator.ToString();
            row += mywish.matchtype + TvWishItemSeparator.ToString();
            row += mywish.group + TvWishItemSeparator.ToString();
            row += mywish.recordtype + TvWishItemSeparator.ToString();
            row += mywish.action + TvWishItemSeparator.ToString();
            row += mywish.exclude + TvWishItemSeparator.ToString();
            row += mywish.viewed + TvWishItemSeparator.ToString();
            row += mywish.prerecord + TvWishItemSeparator.ToString();
            row += mywish.postrecord + TvWishItemSeparator.ToString();
            row += mywish.episodename + TvWishItemSeparator.ToString();
            row += mywish.episodepart + TvWishItemSeparator.ToString();
            row += mywish.episodenumber + TvWishItemSeparator.ToString();
            row += mywish.seriesnumber + TvWishItemSeparator.ToString();
            row += mywish.keepepisodes + TvWishItemSeparator.ToString();
            row += mywish.keepuntil + TvWishItemSeparator.ToString();
            row += mywish.recommendedcard + TvWishItemSeparator.ToString();
            row += mywish.priority + TvWishItemSeparator.ToString();
            row += mywish.aftertime + TvWishItemSeparator.ToString();
            row += mywish.beforetime + TvWishItemSeparator.ToString();
            row += mywish.afterdays + TvWishItemSeparator.ToString();
            row += mywish.beforedays + TvWishItemSeparator.ToString();
            row += mywish.channel + TvWishItemSeparator.ToString();
            row += mywish.skip + TvWishItemSeparator.ToString();
            if (mywish.name != "")
                row += mywish.name + TvWishItemSeparator.ToString();
            else
                row += mywish.searchfor + TvWishItemSeparator.ToString();

            row += mywish.useFolderName + TvWishItemSeparator.ToString();
            row += mywish.withinNextHours + TvWishItemSeparator.ToString();
            row += mywish.scheduled + TvWishItemSeparator.ToString();
            row += mywish.tvwishid + TvWishItemSeparator.ToString();
            row += mywish.recorded + TvWishItemSeparator.ToString();
            row += mywish.deleted + TvWishItemSeparator.ToString();
            row += mywish.emailed + TvWishItemSeparator.ToString();
            row += mywish.conflicts + TvWishItemSeparator.ToString();
            row += mywish.episodecriteria + TvWishItemSeparator.ToString();
            row += mywish.preferredgroup + TvWishItemSeparator.ToString();
            row += mywish.includeRecordings + TvWishItemSeparator.ToString();

            return row;
        }

        public string SaveToString(ref TvWish mywish)
        {
            //DebugTvWish(mywish);
            string row = "";


            //language translations for mediaportal plugin - must come first
            //Log.Debug(" SaveToString mywish.matchtype=" + mywish.matchtype);

            ReverseTvWishLanguageTranslation(ref mywish);

            //Log.Debug(" SaveToString mywish.matchtype=" + mywish.matchtype);

            row += mywish.active + TvWishItemSeparator.ToString();
            row += mywish.searchfor + TvWishItemSeparator.ToString();
            row += mywish.matchtype + TvWishItemSeparator.ToString();
            row += mywish.group + TvWishItemSeparator.ToString();
            row += mywish.recordtype + TvWishItemSeparator.ToString();
            row += mywish.action + TvWishItemSeparator.ToString();
            row += mywish.exclude + TvWishItemSeparator.ToString();
            row += mywish.viewed + TvWishItemSeparator.ToString();
            row += mywish.prerecord + TvWishItemSeparator.ToString();
            row += mywish.postrecord + TvWishItemSeparator.ToString();
            row += mywish.episodename + TvWishItemSeparator.ToString();
            row += mywish.episodepart + TvWishItemSeparator.ToString();
            row += mywish.episodenumber + TvWishItemSeparator.ToString();
            row += mywish.seriesnumber + TvWishItemSeparator.ToString();
            row += mywish.keepepisodes + TvWishItemSeparator.ToString();
            row += mywish.keepuntil + TvWishItemSeparator.ToString();
            row += mywish.recommendedcard + TvWishItemSeparator.ToString();
            row += mywish.priority + TvWishItemSeparator.ToString();
            row += mywish.aftertime + TvWishItemSeparator.ToString();
            row += mywish.beforetime + TvWishItemSeparator.ToString();
            row += mywish.afterdays + TvWishItemSeparator.ToString();
            row += mywish.beforedays + TvWishItemSeparator.ToString();
            row += mywish.channel + TvWishItemSeparator.ToString();
            row += mywish.skip + TvWishItemSeparator.ToString();
            if (mywish.name != "")
                row += mywish.name + TvWishItemSeparator.ToString();
            else
                row += mywish.searchfor + TvWishItemSeparator.ToString();

            row += mywish.useFolderName + TvWishItemSeparator.ToString();
            row += mywish.withinNextHours + TvWishItemSeparator.ToString();
            row += mywish.scheduled + TvWishItemSeparator.ToString();
            row += mywish.tvwishid + TvWishItemSeparator.ToString();
            row += mywish.recorded + TvWishItemSeparator.ToString();
            row += mywish.deleted + TvWishItemSeparator.ToString();
            row += mywish.emailed + TvWishItemSeparator.ToString();
            row += mywish.conflicts + TvWishItemSeparator.ToString();
            row += mywish.episodecriteria + TvWishItemSeparator.ToString();
            row += mywish.preferredgroup + TvWishItemSeparator.ToString();
            row += mywish.includeRecordings + TvWishItemSeparator.ToString();

            //translate back for later usage!
            TvWishLanguageTranslation(ref mywish);
            return row;
        }

        public string SaveToString()
        {
            Log.Debug("SaveToString");

            string wishdata = "";
            for (int i = 0; i < ListAll().Count; i++)
            {
                TvWish mywish = TvWishes[i];

                string row = SaveToString(ref mywish);

                //modify for listview table changes

                string errorMessages = "";

                Log.Debug("x before row =" + row);
                row = CheckRowEntries(row, TvWishItemSeparator, ref errorMessages);
                Log.Debug("xx after row =" + row);

                wishdata += row + "\n";

            }

            if (wishdata.Length >= 1)
                wishdata = wishdata.Substring(0, wishdata.Length - 1);

            
            return wishdata;


        }

        public string SaveToStringNoChecking()
        {
            Log.Debug("SaveToString");

            string wishdata = "";
            for (int i = 0; i < ListAll().Count; i++)
            {
                TvWish mywish = TvWishes[i];

                string row = SaveToString(ref mywish);

                wishdata += row + "\n";

            }

            if (wishdata.Length >= 1)
                wishdata = wishdata.Substring(0, wishdata.Length - 1);


            return wishdata;
        }


        public void ReverseTvWishLanguageTranslation(ref TvWish mywish)
        {
            mywish.active = mywish.active.Replace(PluginGuiLocalizeStrings.Get(4000), "True");
            mywish.active = mywish.active.Replace(PluginGuiLocalizeStrings.Get(4001), "False");
            mywish.matchtype = mywish.matchtype.Replace(PluginGuiLocalizeStrings.Get(2600), "Partial Title");
            mywish.matchtype = mywish.matchtype.Replace(PluginGuiLocalizeStrings.Get(2601), "Exact Title");
            mywish.matchtype = mywish.matchtype.Replace(PluginGuiLocalizeStrings.Get(2602), "Partial Text");
            mywish.matchtype = mywish.matchtype.Replace(PluginGuiLocalizeStrings.Get(2605), "Word in Text/Title"); //must come first
            mywish.matchtype = mywish.matchtype.Replace(PluginGuiLocalizeStrings.Get(2603), "Word in Title");
            mywish.matchtype = mywish.matchtype.Replace(PluginGuiLocalizeStrings.Get(2604), "Word in Text");           
            mywish.matchtype = mywish.matchtype.Replace(PluginGuiLocalizeStrings.Get(2606), "Expression");
            mywish.group = mywish.group.Replace(PluginGuiLocalizeStrings.Get(4104), "All Channels");
            
            string org = mywish.recordtype;
            //mywish.recordtype = mywish.recordtype.Replace("Only Once", PluginGuiLocalizeStrings.Get(2650)); //Order is important due to same name ALL
            if (mywish.recordtype == org)
                mywish.recordtype = mywish.recordtype.Replace(PluginGuiLocalizeStrings.Get(2650), "Only Once"); //Order is important due to same name ALL
            if (mywish.recordtype == org)
                mywish.recordtype = mywish.recordtype.Replace(PluginGuiLocalizeStrings.Get(2655), "All Same Channel Day Time");
            if (mywish.recordtype == org)
                mywish.recordtype = mywish.recordtype.Replace(PluginGuiLocalizeStrings.Get(2653), "All Same Channel Day");
            if (mywish.recordtype == org)
                mywish.recordtype = mywish.recordtype.Replace(PluginGuiLocalizeStrings.Get(2654), "All Same Channel Time");
            if (mywish.recordtype == org)
                mywish.recordtype = mywish.recordtype.Replace(PluginGuiLocalizeStrings.Get(2652), "All Same Channel");
            if (mywish.recordtype == org)
                mywish.recordtype = mywish.recordtype.Replace(PluginGuiLocalizeStrings.Get(2651), "All");
            
            mywish.action = mywish.action.Replace(PluginGuiLocalizeStrings.Get(2702), "Both"); //must be first
            mywish.action = mywish.action.Replace(PluginGuiLocalizeStrings.Get(2700), "Record");
            mywish.action = mywish.action.Replace(PluginGuiLocalizeStrings.Get(2701), "Email");
            mywish.action = mywish.action.Replace(PluginGuiLocalizeStrings.Get(2703), "View");
            mywish.keepepisodes = mywish.keepepisodes.Replace(PluginGuiLocalizeStrings.Get(4105), "All");
            mywish.keepuntil = mywish.keepuntil.Replace(PluginGuiLocalizeStrings.Get(2900), "Always");
            mywish.keepuntil = mywish.keepuntil.Replace(PluginGuiLocalizeStrings.Get(2901), "Days");
            mywish.keepuntil = mywish.keepuntil.Replace(PluginGuiLocalizeStrings.Get(2902), "Weeks");
            mywish.keepuntil = mywish.keepuntil.Replace(PluginGuiLocalizeStrings.Get(2903), "Months");
            mywish.keepuntil = mywish.keepuntil.Replace(PluginGuiLocalizeStrings.Get(2904), "Date");
            mywish.keepuntil = mywish.keepuntil.Replace(PluginGuiLocalizeStrings.Get(2905), "Watched");
            mywish.keepuntil = mywish.keepuntil.Replace(PluginGuiLocalizeStrings.Get(2906), "Space");
            mywish.recommendedcard = mywish.recommendedcard.Replace(PluginGuiLocalizeStrings.Get(4100), "Any");
            mywish.aftertime = mywish.aftertime.Replace(PluginGuiLocalizeStrings.Get(4100), "Any");
            mywish.beforetime = mywish.beforetime.Replace(PluginGuiLocalizeStrings.Get(4100), "Any");
            mywish.afterdays = mywish.afterdays.Replace(PluginGuiLocalizeStrings.Get(4100), "Any");
            mywish.afterdays = mywish.afterdays.Replace(PluginGuiLocalizeStrings.Get(2750), "Monday");
            mywish.afterdays = mywish.afterdays.Replace(PluginGuiLocalizeStrings.Get(2751), "Tuesday");
            mywish.afterdays = mywish.afterdays.Replace(PluginGuiLocalizeStrings.Get(2752), "Wednesday");
            mywish.afterdays = mywish.afterdays.Replace(PluginGuiLocalizeStrings.Get(2753), "Thursday");
            mywish.afterdays = mywish.afterdays.Replace(PluginGuiLocalizeStrings.Get(2754), "Friday");
            mywish.afterdays = mywish.afterdays.Replace(PluginGuiLocalizeStrings.Get(2755), "Saturday");
            mywish.afterdays = mywish.afterdays.Replace(PluginGuiLocalizeStrings.Get(2756), "Sunday");
            mywish.beforedays = mywish.beforedays.Replace(PluginGuiLocalizeStrings.Get(4100), "Any");
            mywish.beforedays = mywish.beforedays.Replace(PluginGuiLocalizeStrings.Get(2750), "Monday");
            mywish.beforedays = mywish.beforedays.Replace(PluginGuiLocalizeStrings.Get(2751), "Tuesday");
            mywish.beforedays = mywish.beforedays.Replace(PluginGuiLocalizeStrings.Get(2752), "Wednesday");
            mywish.beforedays = mywish.beforedays.Replace(PluginGuiLocalizeStrings.Get(2753), "Thursday");
            mywish.beforedays = mywish.beforedays.Replace(PluginGuiLocalizeStrings.Get(2754), "Friday");
            mywish.beforedays = mywish.beforedays.Replace(PluginGuiLocalizeStrings.Get(2755), "Saturday");
            mywish.beforedays = mywish.beforedays.Replace(PluginGuiLocalizeStrings.Get(2756), "Sunday");
            mywish.channel = mywish.channel.Replace(PluginGuiLocalizeStrings.Get(4100), "Any");
            mywish.skip = mywish.skip.Replace(PluginGuiLocalizeStrings.Get(4000), "True");
            mywish.skip = mywish.skip.Replace(PluginGuiLocalizeStrings.Get(4001), "False");
            mywish.useFolderName = mywish.useFolderName.Replace(PluginGuiLocalizeStrings.Get(2850), "Episode");
            mywish.useFolderName = mywish.useFolderName.Replace(PluginGuiLocalizeStrings.Get(2851), "Name");
            mywish.useFolderName = mywish.useFolderName.Replace(PluginGuiLocalizeStrings.Get(2852), "None");
            mywish.useFolderName = mywish.useFolderName.Replace(PluginGuiLocalizeStrings.Get(2853), "Automatic");
            mywish.withinNextHours = mywish.withinNextHours.Replace(PluginGuiLocalizeStrings.Get(4100), "Any");
            mywish.episodecriteria = mywish.episodecriteria.Replace(PluginGuiLocalizeStrings.Get(2960), "Descr.");
            mywish.episodecriteria = mywish.episodecriteria.Replace(PluginGuiLocalizeStrings.Get(2961), "Name");
            mywish.episodecriteria = mywish.episodecriteria.Replace(PluginGuiLocalizeStrings.Get(2962), "Number");
            mywish.preferredgroup = mywish.preferredgroup.Replace(PluginGuiLocalizeStrings.Get(4104), "All Channels");
            mywish.episodecriteria = mywish.episodecriteria.Replace(PluginGuiLocalizeStrings.Get(3264), "None");
            mywish.includeRecordings = mywish.includeRecordings.Replace(PluginGuiLocalizeStrings.Get(4000), "True");
            mywish.includeRecordings = mywish.includeRecordings.Replace(PluginGuiLocalizeStrings.Get(4001), "False");
            
        }

        public void TvWishLanguageTranslation( ref TvWish mywish)
        {
            //language translations for MediaPortal plugin
                mywish.active = mywish.active.Replace("True", PluginGuiLocalizeStrings.Get(4000));
                mywish.active = mywish.active.Replace("False", PluginGuiLocalizeStrings.Get(4001));
                mywish.matchtype = mywish.matchtype.Replace("Partial Title", PluginGuiLocalizeStrings.Get(2600));
                mywish.matchtype = mywish.matchtype.Replace("Exact Title", PluginGuiLocalizeStrings.Get(2601));
                mywish.matchtype = mywish.matchtype.Replace("Partial Text", PluginGuiLocalizeStrings.Get(2602));
                mywish.matchtype = mywish.matchtype.Replace("Word in Text/Title", PluginGuiLocalizeStrings.Get(2605));//must go first
                mywish.matchtype = mywish.matchtype.Replace("Word in Title", PluginGuiLocalizeStrings.Get(2603));
                mywish.matchtype = mywish.matchtype.Replace("Word in Text", PluginGuiLocalizeStrings.Get(2604));               
                mywish.matchtype = mywish.matchtype.Replace("Expression", PluginGuiLocalizeStrings.Get(2606));
                mywish.group = mywish.group.Replace("All Channels", PluginGuiLocalizeStrings.Get(4104));

                string org = mywish.recordtype;
                mywish.recordtype = mywish.recordtype.Replace("Only Once", PluginGuiLocalizeStrings.Get(2650)); //Order is important due to same name ALL
                if (mywish.recordtype==org)
                    mywish.recordtype = mywish.recordtype.Replace("All Same Channel Day Time", PluginGuiLocalizeStrings.Get(2655));
                if (mywish.recordtype == org)
                    mywish.recordtype = mywish.recordtype.Replace("All Same Channel Time", PluginGuiLocalizeStrings.Get(2654));
                if (mywish.recordtype == org)
                    mywish.recordtype = mywish.recordtype.Replace("All Same Channel Day", PluginGuiLocalizeStrings.Get(2653));
                if (mywish.recordtype == org)
                    mywish.recordtype = mywish.recordtype.Replace("All Same Channel", PluginGuiLocalizeStrings.Get(2652));
                if (mywish.recordtype == org)
                    mywish.recordtype = mywish.recordtype.Replace("All", PluginGuiLocalizeStrings.Get(2651));
                
                mywish.action = mywish.action.Replace("Both", PluginGuiLocalizeStrings.Get(2702)); //must be first
                mywish.action = mywish.action.Replace("Record", PluginGuiLocalizeStrings.Get(2700));
                mywish.action = mywish.action.Replace("Email", PluginGuiLocalizeStrings.Get(2701));
                mywish.action = mywish.action.Replace("View", PluginGuiLocalizeStrings.Get(2703));
                mywish.keepepisodes = mywish.keepepisodes.Replace("All", PluginGuiLocalizeStrings.Get(4105));
                mywish.keepuntil = mywish.keepuntil.Replace("Always", PluginGuiLocalizeStrings.Get(2900));
                mywish.keepuntil = mywish.keepuntil.Replace("Days", PluginGuiLocalizeStrings.Get(2901));
                mywish.keepuntil = mywish.keepuntil.Replace("days", PluginGuiLocalizeStrings.Get(2901));
                mywish.keepuntil = mywish.keepuntil.Replace("Weeks", PluginGuiLocalizeStrings.Get(2902));
                mywish.keepuntil = mywish.keepuntil.Replace("weeks", PluginGuiLocalizeStrings.Get(2902));
                mywish.keepuntil = mywish.keepuntil.Replace("Months", PluginGuiLocalizeStrings.Get(2903));
                mywish.keepuntil = mywish.keepuntil.Replace("months", PluginGuiLocalizeStrings.Get(2903));
                mywish.keepuntil = mywish.keepuntil.Replace("Date", PluginGuiLocalizeStrings.Get(2904)); 
                mywish.keepuntil = mywish.keepuntil.Replace("Watched", PluginGuiLocalizeStrings.Get(2905));
                mywish.keepuntil = mywish.keepuntil.Replace("Space", PluginGuiLocalizeStrings.Get(2906));
                mywish.recommendedcard = mywish.recommendedcard.Replace("Any", PluginGuiLocalizeStrings.Get(4100));
                mywish.aftertime = mywish.aftertime.Replace("Any", PluginGuiLocalizeStrings.Get(4100));
                Log.Debug("mywish.beforetime=" + mywish.beforetime);
                mywish.beforetime = mywish.beforetime.Replace("Any", PluginGuiLocalizeStrings.Get(4100));               
                mywish.afterdays = mywish.afterdays.Replace("Any", PluginGuiLocalizeStrings.Get(4100));
                mywish.afterdays = mywish.afterdays.Replace("Monday", PluginGuiLocalizeStrings.Get(2750));
                mywish.afterdays = mywish.afterdays.Replace("Tuesday", PluginGuiLocalizeStrings.Get(2751));
                mywish.afterdays = mywish.afterdays.Replace("Wednesday", PluginGuiLocalizeStrings.Get(2752));
                mywish.afterdays = mywish.afterdays.Replace("Thursday", PluginGuiLocalizeStrings.Get(2753));
                mywish.afterdays = mywish.afterdays.Replace("Friday", PluginGuiLocalizeStrings.Get(2754));
                mywish.afterdays = mywish.afterdays.Replace("Saturday", PluginGuiLocalizeStrings.Get(2755));
                mywish.afterdays = mywish.afterdays.Replace("Sunday", PluginGuiLocalizeStrings.Get(2756));
                mywish.beforedays = mywish.beforedays.Replace("Any", PluginGuiLocalizeStrings.Get(4100));
                mywish.beforedays = mywish.beforedays.Replace("Monday", PluginGuiLocalizeStrings.Get(2750));
                mywish.beforedays = mywish.beforedays.Replace("Tuesday", PluginGuiLocalizeStrings.Get(2751));
                mywish.beforedays = mywish.beforedays.Replace("Wednesday", PluginGuiLocalizeStrings.Get(2752));
                mywish.beforedays = mywish.beforedays.Replace("Thursday", PluginGuiLocalizeStrings.Get(2753));
                mywish.beforedays = mywish.beforedays.Replace("Friday", PluginGuiLocalizeStrings.Get(2754));
                mywish.beforedays = mywish.beforedays.Replace("Saturday", PluginGuiLocalizeStrings.Get(2755));
                mywish.beforedays = mywish.beforedays.Replace("Sunday", PluginGuiLocalizeStrings.Get(2756));
                mywish.channel = mywish.channel.Replace("Any", PluginGuiLocalizeStrings.Get(4100));
                mywish.skip = mywish.skip.Replace("True", PluginGuiLocalizeStrings.Get(4000));
                mywish.skip = mywish.skip.Replace("False", PluginGuiLocalizeStrings.Get(4001));
                mywish.useFolderName = mywish.useFolderName.Replace("Episode", PluginGuiLocalizeStrings.Get(2850));
                mywish.useFolderName = mywish.useFolderName.Replace("Name", PluginGuiLocalizeStrings.Get(2851));
                mywish.useFolderName = mywish.useFolderName.Replace("None", PluginGuiLocalizeStrings.Get(2852));
                mywish.useFolderName = mywish.useFolderName.Replace("Automatic", PluginGuiLocalizeStrings.Get(2853));
                mywish.withinNextHours = mywish.withinNextHours.Replace("Any", PluginGuiLocalizeStrings.Get(4100));
                mywish.episodecriteria = mywish.episodecriteria.Replace("None", PluginGuiLocalizeStrings.Get(3264));
                mywish.episodecriteria = mywish.episodecriteria.Replace("Descr.", PluginGuiLocalizeStrings.Get(2960));
                mywish.episodecriteria = mywish.episodecriteria.Replace("Name", PluginGuiLocalizeStrings.Get(2961));
                mywish.episodecriteria = mywish.episodecriteria.Replace("Number", PluginGuiLocalizeStrings.Get(2962));
                mywish.preferredgroup = mywish.preferredgroup.Replace("All Channels", PluginGuiLocalizeStrings.Get(4104));
                mywish.includeRecordings = mywish.includeRecordings.Replace("True",PluginGuiLocalizeStrings.Get(4000));
                mywish.includeRecordings = mywish.includeRecordings.Replace("False",PluginGuiLocalizeStrings.Get(4001)); 
        }


#if  (MP11 || MP12 || MP16)
        public void MyMessageBox(int header, string text)
        {
            if (text == string.Empty)
            {
                GUIPropertyManager.SetProperty("#status.label", text);
                return;
            }

            text = text.Replace("<BR>", "\n");
            text = text.Replace("<br>", "\n");
            text = text.Replace(@"\n", "\n");
            string[] tokens = text.Split('\n');
            text = text.Replace("\n", "");

            GUIPropertyManager.SetProperty("#status.label", text);

            if ((_DisableInfoWindow == true) && (header == 4400))
                return;

            
            GUIDialogMenu dlg2 = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dlg2.ShowQuickNumbers = false;
            dlg2.SetHeading(header);
            
            foreach (string item in tokens)
            {
                dlg2.Add(item);
            }
            
            dlg2.DoModal(GUIWindowManager.ActiveWindow);
        }
        public void MyMessageBox(int header, int textNumber)
        {
            string text = PluginGuiLocalizeStrings.Get(textNumber);

            if (text == string.Empty)
            {
                GUIPropertyManager.SetProperty("#status.label", text);
                return;
            }

            text = text.Replace("<BR>", "\n");
            text = text.Replace("<br>", "\n");
            text = text.Replace(@"\n", "\n");
            string[] tokens = text.Split('\n');
            text = text.Replace("\n", "");

            GUIPropertyManager.SetProperty("#status.label",text);

            if ((_DisableInfoWindow == true) && (header == 4400))
                return;

            
            GUIDialogMenu dlg3 = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dlg3.ShowQuickNumbers = false;
            dlg3.SetHeading(PluginGuiLocalizeStrings.Get(header));

            
            foreach (string item in tokens)
            {
                dlg3.Add(item);
            }
            dlg3.DoModal(GUIWindowManager.ActiveWindow);
        }


        public void StatusLabel(string text)
        {
            if (text.Length > 0)
            {
                text = text.Replace("\n", "");
                text = text.Replace(@"\n", "");
            }
            
            GUIPropertyManager.SetProperty("#status.label", text);
        }

#elif (MP2)
        //This processes in general a dialog box for MP2. The thread processing ensures that the dialog box can also be used within the routines of iWorkflowmodel e.g. EnterModelContext or ExitModelContext
        //multiple message boxes must be delayed till the first one is processed
        private int _textNumber = 0;
        private int _headerNumber = 0;
        private string _dialogheader = string.Empty;
        private string _dialogtext = string.Empty;
        Thread dialogthread = null;

        public void MyMessageBox(int header, int textNumber)
        {
            if (dialogthread != null)
            {
                while (dialogthread.IsAlive)
                {
                    Log.Debug("Waiting for old Dialog to close");
                    Thread.Sleep(500);
                }
            }
            string textstring = PluginGuiLocalizeStrings.Get(textNumber);
            _textNumber = textNumber;
            _headerNumber = header;

            MyMessageBox(header, textstring);
        }

        public void MyMessageBox(int header, string textstring)
        {
            string headerstring = PluginGuiLocalizeStrings.Get(header);
            MyMessageBox(headerstring, textstring);
        }

        public void MyMessageBox(string headerstring, string textstring)
        {
            if (textstring == string.Empty)
            {
                return;
            }

            textstring = textstring.Replace("<BR>", "\n");
            textstring = textstring.Replace("<br>", "\n");
            textstring = textstring.Replace(@"\n", "\n");
            string[] tokens = textstring.Split('\n');
            textstring = textstring.Replace("\n", "");

            StatusLabel(textstring);

            if ((_DisableInfoWindow == true) && (_headerNumber == 4400))
                return;

            Log.Debug("_DisableInfoWindow=" + _DisableInfoWindow.ToString());
            Log.Debug("_textNumber=" + _textNumber.ToString());
            Log.Debug("_headerNumber=" + _headerNumber.ToString());

            _dialogheader = headerstring;
            Log.Debug("_dialogheader=" + _dialogheader);
            _dialogtext = textstring;
            Log.Debug("_dialogtext=" + _dialogtext);

           

            //create thread and delay dialog!!!
            dialogthread = new Thread(DialogThread);
            dialogthread.Start();

        }

        public void DialogThread()
        {
            Log.Debug(" DialogThread started");
            //dialogwindow
            Thread.Sleep(200);
            Log.Debug("Before OkDialog"); //not waiting for the dialog -> use a DialogCloseWatcher for that
            IDialogManager mydialog = ServiceRegistration.Get<IDialogManager>();
            Guid mydialogId = mydialog.ShowDialog(_dialogheader, _dialogtext, DialogType.OkDialog, false, DialogButtonType.Ok);
            Log.Debug("After OkDialog"); //not waiting for the dialog -> use a DialogCloseWatcher for that

            if (_textNumber == 4306) //4306: TvWishList MediaPortal Plugin Does Not Match To TvWishList TV Server Plugin
            {
                dialogCloseWatcher = new DialogCloseWatcher(this, mydialogId, dialogResult =>
                { //this is watching the result of the dialog box and displaying in the Status label of the screen (do not forget to dispose)
                    IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
                    workflowManager.NavigatePop(1); //same as escape (takes one entry from the stack)
                    return;
                });
            }
            else if (_textNumber == 4311) //4311: Tv wish list is being processed by another process<br>Try again later<br>If the other process hangs reboot the system or stop the tv server manually
            {
                dialogCloseWatcher = new DialogCloseWatcher(this, mydialogId, dialogResult =>
                { //this is watching the result of the dialog box and displaying in the Status label of the screen (do not forget to dispose)
                    IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
                    workflowManager.NavigatePop(1); //same as escape (takes one entry from the stack)
                    return;
                });
            }          
            else
            {
                //do nothing
            }

            _textNumber = 0;
            _headerNumber = 0;
            Log.Debug(" DialogThread finished");

        }
        
/*
        public void MyMessageBox(int header, string text)
        {
            if (Main_GUI.Instance != null)
            {
                if (Main_GUI.Instance.Active)
                {
                    Main_GUI.Instance.Status = text;
                }
            }

            if (Edit_GUI.Instance != null)
            {
                if (Edit_GUI.Instance.Active)
                {
                    Edit_GUI.Instance.Status = text;
                }
            }

            if (Result_GUI.Instance != null)
            {
                if (Result_GUI.Instance.Active)
                {
                    Result_GUI.Instance.Status = text;
                }
            }

            if (text == string.Empty)
            {
                return;
            }

            text = text.Replace("<BR>", "\n");
            text = text.Replace("<br>", "\n");
            text = text.Replace(@"\n", "\n");
            string[] tokens = text.Split('\n');
            text = text.Replace("\n", "");            

            if ((_DisableInfoWindow == true) && (header == 4400))
                return;

            //dialogwindow
            IDialogManager mydialog = ServiceRegistration.Get<IDialogManager>();
            Guid mydialogId = mydialog.ShowDialog(PluginGuiLocalizeStrings.Get(header), text, DialogType.OkDialog, true, DialogButtonType.Ok);
            Log.Debug("After the OkDialog"); //not waiting for the dialog -> use a DialogCloseWatcher for that

            if (MessageBoxAction == 0)
            {
               //do nothing
            }
            else
            {
                DialogCloseWatcher dialogCloseWatcher = new DialogCloseWatcher(this, mydialogId, dialogResult =>
                { //this is watching the result of the dialog box and displaying in the Status label of the screen (do not forget to dispose)
                    DialogFollowUp(MessageBoxAction);
                    return true;
                });
            }

        }
*/
      

        public void StatusLabel(string text)
        {
            if (text.Length > 0)
            {
                text = text.Replace("\n", "");
                text = text.Replace(@"\n", "");
            }

            if (Main_GUI.Instance != null)
            {
                if (Main_GUI.Instance.Active)
                {
                    Main_GUI.Instance.Status = text;
                    Log.Debug("StatusLabel=" + text);
                }
            }

            if (Edit_GUI.Instance != null)
            {
                if (Edit_GUI.Instance.Active)
                {
                    Edit_GUI.Instance.Status = text;
                }
            }

            if (Result_GUI.Instance != null)
            {
                if (Result_GUI.Instance.Active)
                {
                    Result_GUI.Instance.Status = text;
                }
            }
        }
#endif




        public bool save_longsetting(string mystring, string mysetting)
        {
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;
            int STRINGLIMIT = 4000; //is 4096  string limit for settings - split if larger

            // tell TvWishCreator
            setting = layer.GetSetting("TvWishList_TvWishDataCreator", "0.0.0.0");
            if (setting.Value != _DataLoadedBy)
            {
                Log.Error("Fatal data consistency error: Data have been loaded by " + _DataLoadedBy + " and were overwritten by " + setting.Value+" will overwrite again!");
            }
            setting.Value = _LockingPluginname;//new data created now
            _DataLoadedBy = _LockingPluginname;
            setting.Persist();


            // tell TvWish Version
            setting = layer.GetSetting("TvWishList_TvWishVersion", "0.0.0.0");
            setting.Value = TvWishVersion();
            setting.Persist();


            


#if (MP2) 
            //send pipe command for speeding up savesettings
            string response = string.Empty;
            string command = Main_GUI.PipeCommands.RemoveLongSetting.ToString() + mysetting + ":10";
            Log.Debug("command=" + command);
            for (int i = 1; i < 120; i++)
            {
                response = PipeClient.Instance.RunSingleCommand(command);
                if (response == PluginGuiLocalizeStrings.Get(1200))  //Waiting for old process to finish
                {
                    Log.Debug("Waiting for old process to finish");
                    Thread.Sleep(1000);
                }
                else
                {
                    break;
                }


            }
            Log.Debug("Server response=" + response);

            if (response.StartsWith("Error") )
            {//error occured
                Log.Error("Error in pipecommand response="+response);
            }
#elif(MPTV2)
            //Log.Debug("TvWishListSetup.Setup=" + TvWishListSetup.Setup.ToString());
            Log.Debug("TvSetup = " + TvSetup.ToString());

            if (TvSetup)
            {// Command comes from setup form as TvWishList setup is activated
                //send pipe command for speeding up savesettings
                string response = string.Empty;
                string command = PipeCommands.RemoveLongSetting.ToString() + mysetting + ":10";
                Log.Debug("command=" + command);
                for (int i = 1; i < 120; i++)
                {

                    response = PipeClient.Instance.RunSingleCommand(command);
                    if (response == PluginGuiLocalizeStrings.Get(1200))  //Waiting for old process to finish
                    {
                        Log.Debug("Waiting for old process to finish");
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }


                }
                Log.Debug("Server response=" + response);

                if (response.StartsWith("Error"))
                {//error occured
                    Log.Error("Error in pipecommand response=" + response);
                }
            }
            else  //command comes from tvserver as TvWishList setup is deactivated
            {
                Setting.DeleteSettings(mysetting);
            }
#else


            //cleanup work
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

                    //if (value == string.Empty)  was needed for MP2 only before pipecommand
                    //    break; //will be cleaned up later by tv server                 

                }
            }
#endif

            // split string if too large  !!! Limit of 4096 characters in tv server

            try
            {
                if (mystring.Length > STRINGLIMIT)
                {

                    string partial_string = mystring.Substring(0, STRINGLIMIT);
                    setting = layer.GetSetting(mysetting, "");
                    setting.Value = partial_string;
                    //LogDebug("partial string  " + mysetting + "  =" + partial_string, (int)LogSetting.DEBUG);
                    setting.Persist();
                    int ctr = 1;
                    while (ctr * STRINGLIMIT <= mystring.Length)
                    {
                        if ((ctr + 1) * STRINGLIMIT < mystring.Length)
                        {
                            partial_string = mystring.Substring(ctr * STRINGLIMIT, STRINGLIMIT);
                            setting = layer.GetSetting(mysetting + "_" + ctr.ToString("D3"), "");
                            setting.Value = partial_string;
                            //LogDebug("partial string  " + mysetting + "_" + ctr.ToString("D3") + "  =" + partial_string, (int)LogSetting.DEBUG);
                            setting.Persist();

                        }
                        else
                        {
                            partial_string = mystring.Substring(ctr * STRINGLIMIT, mystring.Length - ctr * STRINGLIMIT);
                            setting = layer.GetSetting(mysetting + "_" + ctr.ToString("D3"), "");
                            setting.Value = partial_string;
                            //LogDebug("partial listviewstring  " + mysetting + "_" + ctr.ToString("D3") + "  =" + partial_string, (int)LogSetting.DEBUG);
                            setting.Persist();
                            ctr++;
                            setting = layer.GetSetting(mysetting + "_" + ctr.ToString("D3"), "");
                            setting.Value = "";
                            setting.Persist();

                        }
                        ctr++;

                        if (ctr > 999)
                        {
                            Log.Debug("!!!!!!!!!!!!!!!!!!!! Fatal Error: Too many data entries - skipping data", (int)LogSetting.ERROR);
                            break;
                        }
                    }

                }
                else //do not split string - small enough
                {
                    setting = layer.GetSetting(mysetting, "");
                    setting.Value = mystring;
                    setting.Persist();
                    int ctr = 1;
                    //LogDebug("string  " + mysetting + "=" + mystring, (int)LogSetting.DEBUG);
                    setting = layer.GetSetting(mysetting + "_" + ctr.ToString("D3"), ""); //needed for detecting in loadlongsettings
                    setting.Value = "";
                    setting.Persist();

                }
                return true;
            }
            catch (Exception exc)
            {
                Log.Debug("Adding long setting failed with message \n" + exc.Message, (int)LogSetting.ERROR);
                return false;
            }

        }


        public string loadlongsettings(string name)
        {

            //splits long setting strings in multiple parts
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;

            // tell TvWishCreator
            setting = layer.GetSetting("TvWishList_TvWishDataCreator", "0.0.0.0");
            _DataLoadedBy =  setting.Value;
            

            //LogDebug("Longsetting TvWishList_ListView", (int)LogSetting.DEBUG);
            string stringdata = layer.GetSetting(name, "").Value;
            //LogDebug("listviewdata TvWishList_ListView=" + listviewdata, (int)LogSetting.DEBUG);
            int count = 1;
            string partial = layer.GetSetting(name + "_" + count.ToString("D3"), "").Value;
            //LogDebug("partial " + "TvWishList_ListView_" + count.ToString("D3") + "=" + partial, (int)LogSetting.DEBUG);
            while (partial != "")
            {
                stringdata += partial;
                count++;
                partial = layer.GetSetting(name + "_" + count.ToString("D3"), "").Value;
                //LogDebug("partial " + name + "_" + count.ToString("D3") + "=" + partial, (int)LogSetting.DEBUG);
            }


            //LogDebug("Merged" + "TvWishList_ListView Length =" + listviewdata.Length.ToString(), (int)LogSetting.DEBUG);
            //LogDebug("Merged" + "TvWishList_ListView =" + listviewdata, (int)LogSetting.DEBUG);

            return stringdata;
        }
        
        #region API_For_External_Users
        public bool LockTvWishList(string pluginName)
        {
            bool BUSY = false;
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;
            Log.Debug("LockTvWishList: Trying to lock Tvwishes by " + pluginName);

            for (int i = 0; i < 5; i++) //5 retries
            {
                setting = layer.GetSetting("TvWishList_BUSY", "false");
                try
                {
                    BUSY = Convert.ToBoolean(setting.Value);
                }
                catch
                {
                    BUSY = true;
                    Log.Debug("Could not convert TvWishList_BUSY - setting BUSY to true", (int)LogSetting.ERROR);
                }

                if (BUSY == false)
                    break;

                Thread.Sleep(1000);  //retry every second
            }

            if (BUSY == true)
            {
                Log.Debug("BUSY=true - other process is running", (int)LogSetting.DEBUG);
                setting = layer.GetSetting("TvWishList_TimeStampLock", "1999-12-31 11:59:00");
                DateTime lockTime = DateTime.ParseExact(setting.Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                DateTime checkTime = lockTime.AddHours(1.0); //ignore existing lock after 1 hour
                Log.Debug("lockTime=" + lockTime.ToString());
                Log.Debug("checkTime=" + checkTime.ToString());
                Log.Debug("DateTime.Now=" + DateTime.Now.ToString());

                setting = layer.GetSetting("TvWishList_LockingPluginname", "Not Defined");
                Log.Debug("TvWishList_LockingPluginname=" + setting.Value);
                if ((setting.Value.Contains(pluginName))&&(setting.Value.Contains(Environment.MachineName)) )
                {
                    Log.Error("Locking data because plugin and host are the same - data were locked by " + setting.Value);
                }
                else if (DateTime.Now < checkTime)
                {// data are locked return
                    return false;
                }
                else //data lock is too old - ignore
                {
                    Log.Error("Locking data because timestamp is expired - data were locked by " + setting.Value);
                }
            }

            //set BUSY = true
            setting = layer.GetSetting("TvWishList_BUSY", "true");
            setting.Value = "true";
            setting.Persist();

            //tell plugin
            setting = layer.GetSetting("TvWishList_LockingPluginname", "Not Defined");
            setting.Value = Environment.MachineName + ":" + pluginName + ":" + DateTime.Now.ToString() + " by version " + TvWishVersion();
            _LockingPluginname = setting.Value;
            Log.Debug("TvWishList has been locked by " + _LockingPluginname, (int)LogSetting.DEBUG);
            setting.Persist();

            //tell timestamp
            setting = layer.GetSetting("TvWishList_TimeStampLock", "1999-12-31 11:59:00");
            setting.Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            setting.Persist();

            return true;

        }

        public bool UnLockTvWishList()
        {
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;
            Log.Debug("UnLockTvWishList: Trying to unlock Tvwishes");

            try
            {
                //tell pluginversion
                setting = layer.GetSetting("TvWishList_LockingPluginname", "NONE");
                string owner=setting.Value;
                if (owner == _LockingPluginname)
                {
                    setting.Value = "NONE";
                    _LockingPluginname = "NONE";
                    setting.Persist();

                    //create old timestamp
                    setting = layer.GetSetting("TvWishList_TimeStampLock", "1999-12-31 11:59:00");
                    setting.Value = "1999-12-31 11:59:00";
                    setting.Persist();

                    //set BUSY = false
                    setting = layer.GetSetting("TvWishList_BUSY", "false");
                    setting.Value = "false";
                    setting.Persist();

                    Log.Debug("TvWishList has been sucessfully unlocked by "+owner, (int)LogSetting.DEBUG);
                    return true;
                }
                else
                {
                    Log.Debug("TvWishList could not be unlocked - owner is " + setting.Value, (int)LogSetting.ERROR);
                    return false;
                }
            }
            catch (Exception exc)
            {
                Log.Debug("TvWishList could not be unlocked - exception: "+exc.Message, (int)LogSetting.ERROR);
                return false;
            }
        }


        public string TvWishVersion()
        {
            return ("1.4.0.3");
        }


        enum CompareFileVersion
        {
            Newer = 1,
            Older = -1,
            Equal = 0,
            Error = 89,
            Version1Error = 90,
            Version1String = 91,
            Version2Error = 92,
            Version2String = 93
        };

        

        #endregion API_For_External_Users

        #endregion methods


    }
    
}
