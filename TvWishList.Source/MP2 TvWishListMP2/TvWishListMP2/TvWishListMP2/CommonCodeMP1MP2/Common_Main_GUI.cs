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

using TvWishList;
using Log = TvLibrary.Log.huha.Log;

#if (MP12 || MP11)

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using Action = MediaPortal.GUI.Library.Action;
//using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;
using TvControl;
using TvDatabase;
using Gentle.Framework;
//using GUIKeyboard = MediaPortal.GUI.Library.huha.GUIKeyboard;

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

using MediaPortal.Plugins.TvWishListMP2.MPExtended;

using MediaPortal.Plugins.TvWishListMP2.Settings; 

#endif

//Version 0.0.0.1


#if (MP12 || MP11)
namespace TvWishList
#elif (MP2)
namespace MediaPortal.Plugins.TvWishListMP2.Models
#endif


{

    public partial class Main_GUI //: GUIWindow, ISetupForm
    {

        #region Common Global Definitions

        //Instance
        public static Main_GUI _instance = null;

        public static Main_GUI Instance
        {
            get { return (Main_GUI)_instance; }
        }


        bool _Debug = false;
        public bool Debug
        {
            get { return _Debug; }
            set
            {
                _Debug = value;
#if (!MP2)
                   Log.DebugValue = _Debug; 
#endif
            }

        }

        TvWishProcessing myTvWishes = null;
        XmlMessages mymessages = null;

        TvWish mynewwish = null;
        bool Single = true;

        char TvWishItemSeparator = ';';
        

        public string SectionName = "TvWishListMP";

        public bool LOCKED = false;
        public bool TIMEOUT = false;

        //pipe client

        NamedPipeClientStream pipeClient = null;
        string ClientMessage = string.Empty;
        string ReceivedMessage = "Error";
        //bool OldConnect = false;
        string TimeOutValueString = "60";               //60s default, will be read from config settings
        string HostComputer = "localhost";              //needs to be defined

        //Pipelocking mechanism
        System.Threading.Thread PipeRunThread;
        System.Threading.Thread PipeRunThreadTimeOutCounter = null;
        bool PipeRunThreadActive = false;

        bool TvServerLoadSettings_FAILED = false;   //avoids saving corrupted data after an incorrect load operation        

        // default format strings for text boxes at init() initialized from language file and customized user formats
        string _TextBoxFormat_4to3_EmailFormat_Org = "";
        string _TextBoxFormat_16to9_EmailFormat_Org = "";
        string _UserEmailFormat_Org = "";
        string _TextBoxFormat_4to3_ViewOnlyFormat_Org = "";
        string _TextBoxFormat_16to9_ViewOnlyFormat_Org = "";
        string _UserViewOnlyFormat_Org = "";

        string _TextBoxFormat_4to3_EmailFormat = "";
        string _TextBoxFormat_16to9_EmailFormat = "";
        string _UserEmailFormat = "";
        string _TextBoxFormat_4to3_ViewOnlyFormat = "";
        string _TextBoxFormat_16to9_ViewOnlyFormat = "";
        string _UserViewOnlyFormat = "";
        //string _UserDateTimeFormat = "";
        string _UserListItemFormat = "";
        //end formats

        public bool TvWishListQuickMenu = true;
        

        //Versions
        string TvVersion;
        string MpVersion;
        bool VersionMismatch = false;

        //string TV_USER_FOLDER = "NOT_DEFINED";
        //string TV_PROGRAM_FOLDER = "NOT_DEFINED";

        public string Version()
        {
            return "1.3.0.12";
        }

        public enum PipeCommands
        {
            RequestTvVersion = 1,
            StartEpg,
            ImportTvWishes,
            ExportTvWishes,
            RemoveSetting,
            RemoveRecording,
            RemoveLongSetting,
            Ready,
            Error,
            Error_TimeOut,
            UnknownCommand,
        }

        public enum SkinCommands
        {
            NEWTVWISH = 1,
            NEWTVWISH_ALL,
            NEWTVWISH_EMAIL,
            NEWTVWISH_ALL_EMAIL,
            NEWTVWISH_VIEWONLY,
            NEWTVWISH_ALL_VIEWONLY,
            GOTO_MAIN,
            GOTO_RESULTS,
            RUN_EPG_SINGLE,
            RUN_EPG_ALL,
            TVWISHLIST_QUICKMENU,
        }

        #endregion Common Global Definitions

        #region  private methods
        private string TextBoxFormatConversion(string oldFormat)
        {

            if (oldFormat == string.Empty)
            {
                Log.Debug("Empty format string in TextBoxFormatConversion(string oldFormat)");
            }


            //LogDebug("formatted_datetime_string= " + formatted_datetime_string.Replace('{', '['), (int)LogSetting.DEBUG);
            string debug = oldFormat;
            debug = debug.Replace('{', '_');
            debug = debug.Replace('}', '_');
            Log.Debug("Old Format=" + debug);

            string[] formats = oldFormat.Split('|');
            string newFormat = "";

            foreach (string token in formats)
            {
                for (int i = (int)TvWishEntries.active; i < (int)TvWishEntries.end; i++)
                {
                    if ((token.Contains("{" + i + "}") == true) && (myTvWishes._boolTranslator[i] == true))
                    {
                        //Log.Debug("Found new token=" + token);
                        newFormat += token;
                        break;
                    }

                }
            }
            //Log.Debug("New Format=" + newFormat.Replace('{', '['));




            return newFormat;
        }
       
        private void LoadFromTvServer()
        {//load TvwishlistFolder and file namesfrom TvServer
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;

            setting = layer.GetSetting("TvWishList_MachineName", "NONE");
            HostComputer = setting.Value;

            //load Tvserver Version
            setting = layer.GetSetting("TvWishList_TvServerVersion", "0.0.0.0");
            TvVersion = setting.Value;

            /*//TV_PROGRAM_FOLDER
            setting = layer.GetSetting("TvWishList_TV_PROGRAM_FOLDER", "NOT_DEFINED");
            TV_PROGRAM_FOLDER = setting.Value;*/
        }

        #region More Button
        private void MoreEvaluation(int number)
        {
            //button lock
            if (myTvWishes.ButtonActive)
            {
                myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Operation Is Still Running"
                return;
            }
            myTvWishes.ButtonActive = true; //lock buttons
             

            switch (number)
            {
                case 1: //All active
                    AllActive();
                    break;

                case 2:  //None Active
                    NoneActive();
                    break;

                case 3:  //Skip All
                    SkipAll();
                    break;

                case 4:  //Skip None
                    SkipNone();
                    break;

                case 5:  //Copy TvWishes from View Only to Email
                    CopyFromViewToEmail();
                    break;

                case 6:  //Copy TvWishes from Email to View Only
                    CopyFromEmailToView();
                    break;

                case 7:  //Copy schedules to tvwishlist
                    CopySchedulesToTvWishList();
                    break;

                case 8:  //Export tvwish and both message files
                    ExportTvWish();
                    break;

                case 9:  //Import
                    ImportTvWish();
                    break;
            }

            //StatusLabel( "Sort=" + _sort.ToString());
            UpdateListItems();

            myTvWishes.ButtonActive = false; //unlock buttons
        }

        private void AllActive()
        {
            //All active
            for (int i = 0; i < myTvWishes.ListAll().Count; i++)
            {
                TvWish tempwish = myTvWishes.GetAtIndex(i);
                tempwish.active = PluginGuiLocalizeStrings.Get(4000);
                tempwish.b_active = true;
                myTvWishes.ReplaceAtIndex(i, tempwish);
            }
        }

        private void NoneActive()
        {
            //None Active
            for (int i = 0; i < myTvWishes.ListAll().Count; i++)
            {
                TvWish tempwish = myTvWishes.GetAtIndex(i);
                tempwish.active = PluginGuiLocalizeStrings.Get(4001);
                tempwish.b_active = false;
                myTvWishes.ReplaceAtIndex(i, tempwish);
            }
        }

        private void SkipAll()
        {
            //Skip All
            for (int i = 0; i < myTvWishes.ListAll().Count; i++)
            {
                TvWish tempwish = myTvWishes.GetAtIndex(i);
                tempwish.skip = PluginGuiLocalizeStrings.Get(4000);
                tempwish.b_skip = true;
                myTvWishes.ReplaceAtIndex(i, tempwish);
            }
        }

        private void SkipNone()
        {//Skip None
            for (int i = 0; i < myTvWishes.ListAll().Count; i++)
            {
                TvWish tempwish = myTvWishes.GetAtIndex(i);
                tempwish.skip = PluginGuiLocalizeStrings.Get(4001);
                tempwish.b_skip = false;
                myTvWishes.ReplaceAtIndex(i, tempwish);
            }
        }

        private void CopyFromViewToEmail()
        {
            //save current data 

            try
            {
                int itemcount = 0;

                if (TvserverdatabaseSaveSettings() == false)
                {
                    Log.Error("1: TvserverdatabaseSaveSettings failed");
                    //throw new System.InvalidOperationException("1: TvserverdatabaseSaveSettings failed");
                }

                bool localVIEW = myTvWishes.ViewOnlyMode;


                //set viewmode=true and load data
                myTvWishes.ViewOnlyMode = true;

                if (TvserverdatabaseLoadSettings() == false)
                {
                    Log.Error("2: TvserverdatabaseLoadSettings failed");
                    //throw new System.InvalidOperationException("2: TvserverdatabaseLoadSettings failed");
                }

                List<TvWish> tempTvWishes = new List<TvWish>(myTvWishes.ListAll()); //copy data to temp list

                //set viewmode=false and load data
                myTvWishes.ViewOnlyMode = false;
                if (TvserverdatabaseLoadSettings() == false)
                {
                    Log.Error("3: TvserverdatabaseLoadSettings failed");
                }

                //copy data if not available yet  

                for (int i = 0; i < tempTvWishes.Count; i++)
                {
                    TvWish temp_wish = tempTvWishes[i];
                    bool found = false;
                    for (int j = 0; j < myTvWishes.ListAll().Count; j++)
                    {
                        TvWish actual_wish = myTvWishes.GetAtIndex(j);
                        if (temp_wish.searchfor == actual_wish.searchfor)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found == false)
                    {
                        temp_wish.action = PluginGuiLocalizeStrings.Get(2702);  //Email And Record
                        temp_wish.t_action = MessageType.Both;
                        temp_wish.viewed = "0";
                        temp_wish.i_viewed = 0;
                        temp_wish.recorded = "0";
                        temp_wish.i_recorded = 0;
                        temp_wish.scheduled = "0";
                        temp_wish.i_scheduled = 0;
                        temp_wish.deleted = "0";
                        temp_wish.i_deleted = 0;
                        temp_wish.conflicts = "0";
                        temp_wish.i_conflicts = 0;
                        temp_wish.emailed = "0";
                        temp_wish.i_emailed = 0;
                        //new id
                        myTvWishes.MaxTvWishId++;
                        temp_wish.tvwishid = myTvWishes.MaxTvWishId.ToString();

                        myTvWishes.Add(temp_wish);
                        itemcount++;
                        Log.Debug("New wish added: " + temp_wish.searchfor);
                    }

                }
                //save copied data
                if (TvserverdatabaseSaveSettings() == false)
                {
                    Log.Error("4: TvserverdatabaseSaveSettings failed");
                }

                //restore current data
                myTvWishes.ViewOnlyMode = localVIEW;
                if (TvserverdatabaseLoadSettings() == false)
                {
                    Log.Error("5: TvserverdatabaseLoadSettings failed");
                }

                //Info, Copying TvWishes from View Only to Email completed and found {0} wishes
                myTvWishes.MyMessageBox(4400, string.Format(PluginGuiLocalizeStrings.Get(1550), itemcount.ToString()));
            }
            catch (Exception exc)
            {
                myTvWishes.MyMessageBox(4305, 1551); //Copying TvWishes from View Only to Email failed
                Log.Debug("Error in More Case 6 - Exception:" + exc.Message);
            }
        }

        private void CopyFromEmailToView()
        {
            //save current data 

            try
            {
                int itemcount = 0;

                if (TvserverdatabaseSaveSettings() == false)
                {
                    Log.Error("1: TvserverdatabaseSaveSettings failed");
                    //throw new System.InvalidOperationException("1: TvserverdatabaseSaveSettings failed");
                }

                bool localVIEW = myTvWishes.ViewOnlyMode;


                //set viewmode=true and load data
                myTvWishes.ViewOnlyMode = false;

                if (TvserverdatabaseLoadSettings() == false)
                {
                    Log.Error("2: TvserverdatabaseLoadSettings failed");
                    //throw new System.InvalidOperationException("2: TvserverdatabaseLoadSettings failed");
                }

                List<TvWish> tempTvWishes = new List<TvWish>(myTvWishes.ListAll()); //copy data to temp list

                //set viewmode=false and load data
                myTvWishes.ViewOnlyMode = true;
                if (TvserverdatabaseLoadSettings() == false)
                {
                    Log.Error("3: TvserverdatabaseLoadSettings failed");
                }

                //copy data if not available yet  

                for (int i = 0; i < tempTvWishes.Count; i++)
                {
                    TvWish temp_wish = tempTvWishes[i];
                    bool found = false;
                    for (int j = 0; j < myTvWishes.ListAll().Count; j++)
                    {
                        TvWish actual_wish = myTvWishes.GetAtIndex(j);
                        if (temp_wish.searchfor == actual_wish.searchfor)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found == false)
                    {
                        temp_wish.action = PluginGuiLocalizeStrings.Get(2703);  //View
                        temp_wish.t_action = MessageType.Viewed;
                        temp_wish.skip = "False";
                        temp_wish.b_skip = false;
                        temp_wish.viewed = "0";
                        temp_wish.i_viewed = 0;
                        temp_wish.recorded = "0";
                        temp_wish.i_recorded = 0;
                        temp_wish.scheduled = "0";
                        temp_wish.i_scheduled = 0;
                        temp_wish.deleted = "0";
                        temp_wish.i_deleted = 0;
                        temp_wish.conflicts = "0";
                        temp_wish.i_conflicts = 0;
                        temp_wish.emailed = "0";
                        temp_wish.i_emailed = 0;
                        //new id
                        myTvWishes.MaxTvWishId++;
                        temp_wish.tvwishid = myTvWishes.MaxTvWishId.ToString();

                        myTvWishes.Add(temp_wish);
                        itemcount++;
                        Log.Debug("New wish added: " + temp_wish.searchfor);
                        Log.Debug("New wish action: " + temp_wish.action);
                        Log.Debug("New wish action: " + myTvWishes.GetAtIndex(myTvWishes.ListAll().Count - 1).action);
                    }

                }
                //save copied data
                if (TvserverdatabaseSaveSettings() == false)
                {
                    Log.Error("4: TvserverdatabaseSaveSettings failed");
                }

                //restore current data
                myTvWishes.ViewOnlyMode = localVIEW;
                if (TvserverdatabaseLoadSettings() == false)
                {
                    Log.Error("5: TvserverdatabaseLoadSettings failed");
                }


                //Info, Copying TvWishes from Email to View Only completed and found {0} wishes
                myTvWishes.MyMessageBox(4400, string.Format(PluginGuiLocalizeStrings.Get(1552), itemcount.ToString()));
            }
            catch (Exception exc)
            {
                myTvWishes.MyMessageBox(4305, 1553); //Error, Copying TvWishes from Email to View Only failed
                Log.Debug("Error in More Case 7 - Exception:" + exc.Message);
            }

        }

        private void CopySchedulesToTvWishList()
        {
            try
            {
                if (myTvWishes.ViewOnlyMode == true)
                {
                    myTvWishes.MyMessageBox(4305, 4310); //Copy Schedules Works only for Email Mode
                    return;
                }



                Log.Debug("mymessages.ListAllTvMessages().Count=" + mymessages.ListAllTvMessages().Count.ToString());

                mymessages.FilterExistingWishes(myTvWishes.ListAll());

                int count = 0;
                foreach (Schedule mySchedule in Schedule.ListAll())
                {
                    bool found = false;
                    foreach (xmlmessage mymessage in mymessages.ListAllTvMessagesFiltered())
                    {
                        if (mySchedule.ProgramName == mymessage.title)
                        {
                            Log.Debug("found=true mySchedule.ProgramName=" + mySchedule.ProgramName);
                            found = true;
                            break;
                        }
                    }

                    if (found == false)  //do not add if found=true it does exist already in a message
                    {
                        foreach (TvWish mywish in myTvWishes.ListAll())// check in TvWishList if already on
                        {
                            if (mywish.searchfor == mySchedule.ProgramName)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found == false)//delete schedule and add new TvWish
                        {
                            TvWish newWish = new TvWish();
                            newWish = myTvWishes.DefaultData();
                            newWish.searchfor = mySchedule.ProgramName;
                            newWish.name = mySchedule.ProgramName;
                            newWish.matchtype = PluginGuiLocalizeStrings.Get(2601);  //Exact Title
                            newWish.recordtype = PluginGuiLocalizeStrings.Get(2650);  //One Movie
                            myTvWishes.Add(newWish);
                            mySchedule.Delete();
                            count++;
                            Log.Debug("Moved new schedule to TvWishList title=" + newWish.searchfor);
                        }
                    }
                }

                //Info, Moving Schedules to TvWishList completed and found {0} wishes
                myTvWishes.MyMessageBox(4400, string.Format(PluginGuiLocalizeStrings.Get(1554), count.ToString()));
            }
            catch (Exception exc)
            {
                myTvWishes.MyMessageBox(4305, 1555); //Moving Schedules to TvWishList failed
                Log.Debug("Error in More Case 8 - Exception:" + exc.Message);
            }
        }

        private void ExportTvWish()
        {
            myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1107));  //Export
            TvserverdatabaseSaveSettings();

            if (myTvWishes.ViewOnlyMode)
            {
                ClientMessage = PipeCommands.ExportTvWishes.ToString() + "VIEWONLY=TRUE TIMEOUT:" + TimeOutValueString;
            }
            else
            {
                ClientMessage = PipeCommands.ExportTvWishes.ToString() + "VIEWONLY=FALSE TIMEOUT:" + TimeOutValueString;
            }

            string response = RunSingleCommand(ClientMessage);
            myTvWishes.MyMessageBox(4400, response);
        }

        private void ImportTvWish()
        {
            myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1108)); //Import


            if (myTvWishes.ViewOnlyMode)
            {
                ClientMessage = PipeCommands.ImportTvWishes.ToString() + "VIEWONLY=TRUE TIMEOUT:" + TimeOutValueString;
            }
            else
            {
                ClientMessage = PipeCommands.ImportTvWishes.ToString() + "VIEWONLY=FALSE TIMEOUT:" + TimeOutValueString;
            }

            string response = RunSingleCommand(ClientMessage);

            TvserverdatabaseLoadSettings();

            myTvWishes.MyMessageBox(4400, response);

        }
        #endregion More Button

        #endregion private methods

        #region public methods


        #region pog button for MP1 and MP2

        public void ParameterEvaluation(string parameter, int previousWindow, bool tvWishListQuickMenu)
        {
            Log.Debug("Evaluating Parameter loadparameter=" + parameter);
            Log.Debug("previousWindow=" + previousWindow);
            parameter = parameter.Replace(@"//", "\n");
            string[] tokenarray = parameter.Split('\n');
            Log.Debug("tokenarray.Length=" + tokenarray.Length.ToString());
            if (tokenarray.Length == 0)
            {
                Log.Debug("No parameter content found");
                return;
            }

            string arguments = string.Empty;
            for (int i = 1; i < tokenarray.Length; i++)
            {
                arguments += tokenarray[i]+"//";
            }
            if (arguments.Length >= 2) //remove last "//"
            {
                arguments = arguments.Substring(0, arguments.Length - 2);
            }
            Log.Debug("arguments=" + arguments);

            //get command
            string command = tokenarray[0].ToUpper();

            if (((!tvWishListQuickMenu) || (command.Contains(SkinCommands.TVWISHLIST_QUICKMENU.ToString()))) && (parameter.Contains("NOQUICKMENU")==false))
            {   //use dialog menu for command


#if (MP11 || MP12)
                GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                dlg.ShowQuickNumbers = false;
                dlg.SetHeading(PluginGuiLocalizeStrings.Get(1050)); //TvWishList Quick Menu

                dlg.Add(PluginGuiLocalizeStrings.Get(1060));//TvWishList Quick Menu - Create TvWish
                dlg.Add(PluginGuiLocalizeStrings.Get(1059));//TvWishList Quick Menu - Run EPG Single

                if (parameter.Contains("VIEWONLY=TRUE") == true)//VIEWONLY
                {
                    dlg.Add(PluginGuiLocalizeStrings.Get(1061)); //VIEWONLY=FALSE
                }
                else if (parameter.Contains("VIEWONLY=FALSE") == true)
                {
                    dlg.Add(PluginGuiLocalizeStrings.Get(1062)); //VIEWONLY=TRUE
                }
                else if (myTvWishes.ViewOnlyMode == true)
                {
                    dlg.Add(PluginGuiLocalizeStrings.Get(1061)); //VIEWONLY=FALSE
                    parameter += "//VIEWONLY=TRUE";
                }
                else if (myTvWishes.ViewOnlyMode == false)
                {
                    dlg.Add(PluginGuiLocalizeStrings.Get(1062)); //VIEWONLY=TRUE
                    parameter += "//VIEWONLY=FALSE";
                }

                dlg.Add(PluginGuiLocalizeStrings.Get(1057));//GOTO MainMenu
                dlg.Add(PluginGuiLocalizeStrings.Get(1058));//GOTO Results

                /*
                for (int i = 1053; i <= 1058; i++)
                {
                    dlg.Add(PluginGuiLocalizeStrings.Get(i));
                }*/

                dlg.DoModal(GUIWindowManager.ActiveWindow);
                Log.Debug("dlg.SelectedId=" + dlg.SelectedId.ToString());

                switch (dlg.SelectedId)
                {

                    case 1: //TvWishList Quick Menu - Create TvWish
                        command = SkinCommands.NEWTVWISH.ToString();
                        parameter = command + "//" + arguments;
                        parameter = parameter.Replace(@"//", "\n");
                        tokenarray = parameter.Split('\n');
                        break;

                    case 2: //TvWishList Quick Menu - Run EPG Single
                        command = SkinCommands.RUN_EPG_SINGLE.ToString();
                        parameter = command + "//" + arguments;
                        parameter = parameter.Replace(@"//", "\n");
                        tokenarray = parameter.Split('\n');
                        break;

                    case 3: //VIEWONLY
                        //Log.Debug("Command: before Parameter=" + parameter);
                        if (parameter.Contains("VIEWONLY=TRUE") == true)
                        {
                            parameter = parameter.Replace("VIEWONLY=TRUE", "VIEWONLY=FALSE");
                            //Log.Debug("Switching to false");
                        }
                        else
                        {
                            parameter = parameter.Replace("VIEWONLY=FALSE", "VIEWONLY=TRUE");
                            //Log.Debug("Switching to true");
                        }
                        //Log.Debug("Command: after Parameter=" + parameter);
                        ParameterEvaluation(parameter, -1, false); //use old parameter and force quickmenu
                        return;
                                            

                    case 4: //Goto TvWishList Main
                        command = SkinCommands.GOTO_MAIN.ToString();
                        parameter = command + "//" + arguments;
                        parameter = parameter.Replace(@"//", "\n");
                        tokenarray = parameter.Split('\n');
                        break;

                    case 5: //Goto TvWishList Results
                        command = SkinCommands.GOTO_RESULTS.ToString();
                        parameter = command + "//" + arguments;
                        parameter = parameter.Replace(@"//", "\n");
                        tokenarray = parameter.Split('\n');
                        break;
                }
            }
#else  //MP2
                _dialogMenuItemList.Clear();

                DialogHeader = PluginGuiLocalizeStrings.Get(1050); //TvWishList Quick Menu


                ListItem myitem1 = new ListItem(); //Create TvWish
                myitem1.SetLabel("Name", PluginGuiLocalizeStrings.Get(1060));//TvWishList Quick Menu - Create TvWish
                myitem1.Command = new MethodDelegateCommand(() =>
                {
                    command = SkinCommands.NEWTVWISH.ToString();
                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                    mycommandscreenManager.CloseTopmostDialog();
                    string newparameter = command + "//" + arguments;
                    ParameterEvaluation(newparameter, -1, true);
                });
                _dialogMenuItemList.Add(myitem1);


                ListItem myitem2 = new ListItem(); //Run EPG Single
                myitem2.SetLabel("Name", PluginGuiLocalizeStrings.Get(1059));//TvWishList Quick Menu - Run EPG Single
                myitem2.Command = new MethodDelegateCommand(() =>
                {
                    command = SkinCommands.RUN_EPG_SINGLE.ToString();
                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                    mycommandscreenManager.CloseTopmostDialog();
                    string newparameter = command + "//" + arguments;
                    ParameterEvaluation(newparameter, -1, true);
                });
                _dialogMenuItemList.Add(myitem2);


                

                ListItem myitem4 = new ListItem(); //VIEWONLY
                if (parameter.Contains("VIEWONLY=TRUE") == true)
                {
                    myitem4.SetLabel("Name", PluginGuiLocalizeStrings.Get(1061)); //VIEWONLY=FALSE
                }
                else if (parameter.Contains("VIEWONLY=FALSE") == true)
                {
                    myitem4.SetLabel("Name", PluginGuiLocalizeStrings.Get(1062)); //VIEWONLY=TRUE
                }
                else if (myTvWishes.ViewOnlyMode == true)
                {
                    myitem4.SetLabel("Name", PluginGuiLocalizeStrings.Get(1061)); //VIEWONLY=FALSE
                    parameter += "//VIEWONLY=TRUE";
                }
                else if (myTvWishes.ViewOnlyMode == false)
                {
                    myitem4.SetLabel("Name", PluginGuiLocalizeStrings.Get(1062)); //VIEWONLY=TRUE
                    parameter += "//VIEWONLY=FALSE";
                }

                //Log.Debug("myitem4.Labels[NAME]=" + myitem4.Labels["Name"].ToString());
                myitem4.Command = new MethodDelegateCommand(() =>
                {
                    //Log.Debug("Command: before Parameter=" + parameter);
                    if (parameter.Contains("VIEWONLY=TRUE") == true)
                    {
                        parameter = parameter.Replace("VIEWONLY=TRUE", "VIEWONLY=FALSE");
                        //Log.Debug("Switching to false");
                    }
                    else
                    {
                        parameter = parameter.Replace("VIEWONLY=FALSE", "VIEWONLY=TRUE");
                        //Log.Debug("Switching to true");
                    }
                    //Log.Debug("Command: after Parameter=" + parameter);
                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                    mycommandscreenManager.CloseTopmostDialog();                   
                    ParameterEvaluation(parameter, -1, false); //use old parameter and force quickmenu
                });
                _dialogMenuItemList.Add(myitem4);


                ListItem myitem5 = new ListItem(); //GOTO MainMenu
                myitem5.SetLabel("Name", PluginGuiLocalizeStrings.Get(1057));//TvWishList Quick Menu - GOTO MainMenu
                myitem5.Command = new MethodDelegateCommand(() =>
                {
                    command = SkinCommands.GOTO_MAIN.ToString();
                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                    mycommandscreenManager.CloseTopmostDialog();
                    string newparameter = command + "//" + arguments;
                    ParameterEvaluation(newparameter, -1, true);
                });
                _dialogMenuItemList.Add(myitem5);

                ListItem myitem6 = new ListItem(); //GOTO Results
                myitem6.SetLabel("Name", PluginGuiLocalizeStrings.Get(1058));//TvWishList Quick Menu - GOTO Results
                myitem6.Command = new MethodDelegateCommand(() =>
                {
                    command = SkinCommands.GOTO_RESULTS.ToString();
                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                    mycommandscreenManager.CloseTopmostDialog();
                    string newparameter = command + "//" + arguments;
                    ParameterEvaluation(newparameter, -1, true);
                });
                _dialogMenuItemList.Add(myitem6);

                //update for dialog skin
                DialogMenuItemList.FireChange();

                //will now call a dialogbox with a given menu            
                ScreenManager.ShowDialog(TVWISHLIST_MAIN_DIALOG_MENU_SCREEN);



                return;
            }
#endif
            //use skin command


            Log.Debug("command =" + command + " command.Length=" + command.Length.ToString());
            Log.Debug("parameter =" + parameter);

            //handle VIEWONLY first
            if (parameter.Contains("VIEWONLY=TRUE") == true)
            {
                if (!myTvWishes.ViewOnlyMode)
                    OnButtonViewOnlyMode();
            }
            else if (parameter.Contains("VIEWONLY=FALSE") == true)
            {
                if (myTvWishes.ViewOnlyMode)
                    OnButtonViewOnlyMode();
            }
            Log.Debug("myTvWishes.ViewOnlyMode=" + myTvWishes.ViewOnlyMode.ToString());


            if (command == SkinCommands.NEWTVWISH.ToString())
            {
                NewTvWishCreation(tokenarray, false); //only title
            }
            else if (command == SkinCommands.NEWTVWISH_ALL.ToString())
            {
                NewTvWishCreation(tokenarray, true); // all parameters
            }
            else if (command == SkinCommands.NEWTVWISH_EMAIL.ToString())
            {
                if (myTvWishes.ViewOnlyMode)
                    OnButtonViewOnlyMode();

                NewTvWishCreation(tokenarray, false); //only title
            }
            else if (command == SkinCommands.NEWTVWISH_ALL_EMAIL.ToString())
            {
                if (myTvWishes.ViewOnlyMode)
                    OnButtonViewOnlyMode();

                NewTvWishCreation(tokenarray, true); // all parameters
            }
            else if (command == SkinCommands.NEWTVWISH_VIEWONLY.ToString())
            {
                if (!myTvWishes.ViewOnlyMode)
                    OnButtonViewOnlyMode();

                NewTvWishCreation(tokenarray, false); //only title
            }
            else if (command == SkinCommands.NEWTVWISH_ALL_VIEWONLY.ToString())
            {
                if (!myTvWishes.ViewOnlyMode)
                    OnButtonViewOnlyMode();

                NewTvWishCreation(tokenarray, true); // all parameters
            }

            else if (command == SkinCommands.GOTO_MAIN.ToString())
            {
                //do nothing you are at main
            }
            else if (command == SkinCommands.GOTO_RESULTS.ToString())
            {
                //goto view results
#if (MP11 || MP12)
                GUIWindowManager.ActivateWindow(_guilistwindowid);
#else //MP2
                //push to Result Page
                IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
                Guid resultStatId = new Guid("eff5097b-a4a7-4934-a2e3-d8047d92cec7"); //from plugin.xml
                workflowManager.NavigatePush(resultStatId);
#endif
            }
            else if ((command == SkinCommands.RUN_EPG_SINGLE.ToString()) || (command == SkinCommands.RUN_EPG_ALL.ToString()) )
            {
                NewTvWishCreation(tokenarray, false); 
            }
            //add new commands here
            else //Error
            {
                Log.Error("Unknown command in skin parameter found - command=" + command);
            }
            Log.Debug("Skin parameter evaluation completed");

        }

        //pog button for MP1 and MP2
        protected void NewTvWishCreation(string[] tokenarray, bool all)
        {
            Log.Debug("NewTvWishCreation started - VIEWONLY=" + myTvWishes.ViewOnlyMode.ToString());
            Log.Debug("all=" + all.ToString());
            mynewwish = myTvWishes.DefaultData();
            Log.Debug("newwish.name=" + mynewwish.name);
            Log.Debug("newwish.searchfor=" + mynewwish.searchfor);
            Log.Debug("newwish.tvwishid=" + mynewwish.tvwishid);
            Log.Debug("myTvWishes.MaxTvWishId="+myTvWishes.MaxTvWishId.ToString());
            int ctr = 1;
            while (ctr < tokenarray.Length)
            {
                string expression = tokenarray[ctr].ToUpper();
                Log.Debug("expression=" + expression);

                if (expression.StartsWith("TITLE="))
                {
                    try
                    {
                        mynewwish.searchfor = tokenarray[ctr].Substring(6, tokenarray[ctr].Length - 6);
                        Log.Debug("newwish.searchfor=" + mynewwish.searchfor);
                    }
                    catch (Exception exc)
                    {
                        Log.Debug("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                        Log.Error("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                    }
                    if (mynewwish.name == string.Empty)
                    {
                        mynewwish.name = mynewwish.searchfor;
                    }


                }
                else if (expression.StartsWith("EXPRESSION="))
                {
                    try
                    {
                        mynewwish.searchfor = tokenarray[ctr].Substring(11, tokenarray[ctr].Length - 11);
                        Log.Debug("newwish.searchfor=" + mynewwish.searchfor);
                    }
                    catch (Exception exc)
                    {
                        Log.Debug("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                        Log.Error("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                    }
                    if (mynewwish.name == string.Empty)
                    {
                        mynewwish.name = mynewwish.searchfor;
                    }
                    mynewwish.matchtype = PluginGuiLocalizeStrings.Get(2606); //Expression
                }
                else if (expression.StartsWith("NAME=")) //must be after title and expression in scriptfile
                {
                    try
                    {
                        mynewwish.name = tokenarray[ctr].Substring(5, tokenarray[ctr].Length - 5);
                        Log.Debug("newwish.name=" + mynewwish.name);
                    }
                    catch (Exception exc)
                    {
                        Log.Debug("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                        Log.Error("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                    }
                }
                else if ((expression.StartsWith("CHANNEL=")) && (all))
                {
                    try
                    {
                        mynewwish.channel = tokenarray[ctr].Substring(8, tokenarray[ctr].Length - 8);
                        Log.Debug("newwish.channel=" + mynewwish.channel);
                    }
                    catch (Exception exc)
                    {
                        Log.Debug("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                        Log.Error("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                    }
                }
                else if ((expression.StartsWith("EPISODEPART=")) && (all))
                {
                    try
                    {
                        mynewwish.episodepart = tokenarray[ctr].Substring(12, tokenarray[ctr].Length - 12);
                        Log.Debug("newwish.episodepart=" + mynewwish.episodepart);
                    }
                    catch (Exception exc)
                    {
                        Log.Debug("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                        Log.Error("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                    }
                }
                else if ((expression.StartsWith("EPISODENAME=")) && (all))
                {
                    try
                    {
                        mynewwish.episodename = tokenarray[ctr].Substring(12, tokenarray[ctr].Length - 12);
                        Log.Debug("newwish.episodename=" + mynewwish.episodename);
                    }
                    catch (Exception exc)
                    {
                        Log.Debug("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                        Log.Error("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                    }
                }
                else if ((expression.StartsWith("SERIESNUMBER=")) && (all))
                {
                    try
                    {
                        if (tokenarray[ctr].Contains(@"/"))  //format SERIES.EPISODE/TOTALEPISODES eg. 2.3/12
                        {
                            string[] tot_array = tokenarray[ctr].Split('/');
                            if (tot_array[0].Contains("."))
                            {
                                string[] ser_array = tot_array[0].Split('.');
                                mynewwish.seriesnumber = ser_array[0].Substring(13, ser_array[0].Length - 13);
                                mynewwish.episodenumber = ser_array[1];
                                Log.Debug("format SERIES.EPISODE/TOTALEPISODES: newwish.seriesnumber=" + mynewwish.seriesnumber);
                                Log.Debug("format SERIES.EPISODE/TOTALEPISODES: newwish.episodenumber=" + mynewwish.episodenumber);
                            }
                        }
                        else
                        {
                            mynewwish.seriesnumber = tokenarray[ctr].Substring(13, tokenarray[ctr].Length - 13);
                            Log.Debug("newwish.seriesnumber=" + mynewwish.seriesnumber);
                        }
                    }
                    catch (Exception exc)
                    {
                        Log.Debug("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                        Log.Error("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                    }
                }
                else if ((expression.StartsWith("EPISODENUMBER=")) && (all))
                {
                    try
                    {
                        if (tokenarray[ctr].Contains(@"/"))  //format SERIES.EPISODE/TOTALEPISODES eg. 2.3/12
                        {
                            string[] tot_array = tokenarray[ctr].Split('/');
                            if (tot_array[0].Contains("."))
                            {
                                string[] ser_array = tot_array[0].Split('.');
                                mynewwish.episodenumber = ser_array[0].Substring(14, ser_array[0].Length - 14);
                                mynewwish.episodenumber = ser_array[1];
                                Log.Debug("format SERIES.EPISODE/TOTALEPISODES: newwish.seriesnumber=" + mynewwish.seriesnumber);
                                Log.Debug("format SERIES.EPISODE/TOTALEPISODES: newwish.episodenumber=" + mynewwish.episodenumber);
                            }
                        }
                        else
                        {
                            mynewwish.episodenumber = tokenarray[ctr].Substring(14, tokenarray[ctr].Length - 14);
                            Log.Debug("newwish.episodenumber=" + mynewwish.episodenumber);
                        }
                    }
                    catch (Exception exc)
                    {                       
                        Log.Error("Error in expression " + tokenarray[ctr] + "\n exception is " + exc.Message);
                    }
                }
                

                //add other tvwish arguments here
                else if (all) //error
                {
                    Log.Debug("Unknown expression in skin parameter found -expression=" + expression);
                    Log.Error("Unknown expression in skin parameter found -expression=" + expression);
                }
                ctr++;
            }

            //tvwish does exist?

            if (mynewwish.searchfor == string.Empty)
            {
                // no search due to no search criteria for new tv wish
                Log.Error("Tv wish had no search criteria - epg search abandoned");
                return;
            }


            int i = myTvWishes.RetrieveBySearchFor(mynewwish.searchfor);
            Log.Debug("Create new tvwish? TvWish i="+i.ToString());
            if (i == -1)//new tvwish must be created
            {
                myTvWishes.Add(mynewwish);
                Log.Debug("new tvwish added with name=" + mynewwish.name + " tvwishid=" + mynewwish.tvwishid.ToString());
                UpdateListItems();
            }
            else //new tvwish does exist
            {
                int j = myTvWishes.GetIndex(i.ToString());
                mynewwish.tvwishid = i.ToString();
                myTvWishes.ReplaceAtIndex(j, mynewwish);
                myTvWishes.MaxTvWishId--; //new to avoid leak
            }

            //get index
            myTvWishes.FocusedWishIndex = myTvWishes.GetIndex(mynewwish);
                

            Log.Debug("viewing results of existing wish mymessages.FilterName=" + mymessages.FilterName);



            if (tokenarray[0] == SkinCommands.RUN_EPG_SINGLE.ToString())
            {
                mymessages.FilterName = mynewwish.tvwishid.ToString(); //tvwishid of existing wish
                Single = true;
                Thread RunEpgCommandThread = new Thread(RunEpgCommand);
                RunEpgCommandThread.Start();
            }
            else if (tokenarray[0] == SkinCommands.RUN_EPG_ALL.ToString())
            {
                mymessages.FilterName = string.Empty; //all tv wishes
                Single = false;
                Thread RunEpgCommandThread = new Thread(RunEpgCommand);
                RunEpgCommandThread.Start();
            }
            else
            {
#if (MP11 || MP12)
                GUIWindowManager.ActivateWindow(_guieditwindowid);
#else
                //push to Edit Page
                    mymessages.FilterName = string.Empty; //all tv wishes
                    IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
                    Guid editStatId = new Guid("100e9845-54de-4e9b-9cf1-6101509b7c6d"); //from plugin.xml
                    workflowManager.NavigatePush(editStatId);
#endif
            }
        

            
                         
            



        }

        private void RunEpgCommand()
        {
            try
            {
                //store all tvwish.active elements

                Log.Debug("RunEpgCommand started");
                Log.Debug("newwish.name=" + mynewwish.name);
                Log.Debug("newwish.searchfor=" + mynewwish.searchfor);
                Log.Debug("newwish.tvwishid=" + mynewwish.tvwishid);
                //Log.Debug("i=" + i.ToString());
                Log.Debug("single=" + Single.ToString());
                

                int tvWishCount = myTvWishes.ListAll().Count;
                bool[] activebackup = new bool[tvWishCount];

                if (Single)
                {
                    for (int j = 0; j < tvWishCount; j++)
                    {
                        TvWish mywish = myTvWishes.GetAtIndex(j);
                        activebackup[j] = mywish.b_active;
                    }

                    // set all to inactive besides last created tvwish
                    Log.Debug("set all to inactive besides last created tvwish");
                    for (int j = 0; j < tvWishCount; j++)
                    {
                        TvWish mywish = myTvWishes.GetAtIndex(j);
                        if (mywish.tvwishid != mynewwish.tvwishid)
                        {
                            mywish.b_active = false;
                            mywish.active = PluginGuiLocalizeStrings.Get(4001);//False
                            myTvWishes.ReplaceAtIndex(j, mywish);
                        }
                    }
                }//end single


                mynewwish.active = PluginGuiLocalizeStrings.Get(4000);//True
                mynewwish.b_active = true;
               
                /*
                if (i > 0) //needed for existing tv wishes to reoplace at the right index!
                {
                    newwish.tvwishid = i.ToString();
                }*/

                if (!myTvWishes.ReplaceAtTvWishId(mynewwish.tvwishid, mynewwish))
                {
                    Log.Error("Error in replacing tvwish with id=" + mynewwish.tvwishid);
                }


                
                
#if (MP2)               
                IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
                //push to Result Page                    
                Guid resultStatId = new Guid("eff5097b-a4a7-4934-a2e3-d8047d92cec7"); //from plugin.xml
                workflowManager.NavigatePush(resultStatId);
#else
                GUIWindowManager.ActivateWindow(_guilistwindowid);
#endif

                //run epg search
                Log.Debug("run epg search");
                RunThreadProcessing();


                if (Single)
                {
                    mymessages.FilterName = mynewwish.tvwishid;
                    Log.Debug("mymessages.FilterName=" + mymessages.FilterName);
                    
                    //restore active in all tv wishes
                    Log.Debug("restore active in all tv wishes");
                    for (int j = 0; j < myTvWishes.ListAll().Count; j++)
                    {
                        TvWish mywish = myTvWishes.GetAtIndex(j);
                        if (activebackup[j])
                        {
                            mywish.b_active = true;
                            mywish.active = PluginGuiLocalizeStrings.Get(4000);//True
                        }
                        myTvWishes.ReplaceAtIndex(j, mywish);
                    }

                }//end single


                //update resultpage (common for both MP1 and MP2)
                if (Result_GUI.Instance != null)
                {
                    Log.Debug("Result_GUI.Instance.UpdateListItems");
                    //Result_GUI.Instance.MyMessages = mymessages;
                    //Result_GUI.Instance.TvWishes = myTvWishes;
#if (MP2) 
                    Result_GUI.Instance.UpdateListItems();
#else
                    //Result_GUI.Instance.UpdateListItems();
                    GUIWindowManager.ActivateWindow(_guilistwindowid);
#endif
                }

                
            }
            catch (Exception exc)
            {
                Log.Error("Error in RunEpgSingleCommand() -  exception is " + exc.Message);
            }
            
        }

        #endregion pog button for MP1 and MP2

        public bool TvserverdatabaseLoadSettings()
        {
            Log.Debug("Main_GUI: TvserverdatabaseLoadSettings() called");

            DateTime startTvserverdatabaseLoadSettings = DateTime.Now;

#if (!MP2)
            //connect to database if unconnected
            //myinterface.ConnectToDatabase();
#endif

            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;

            try
            {
                myTvWishes.Clear();

                setting = layer.GetSetting("TvWishList_ColumnSeparator", ";");
                TvWishItemSeparator = setting.Value[0];
                Log.Debug("TvWishItemSeparator=" + TvWishItemSeparator.ToString());

                //default pre and post record from general recording settings
                setting = layer.GetSetting("preRecordInterval", "5");
                string prerecord = setting.Value;

                setting = layer.GetSetting("postRecordInterval", "5");
                string postrecord = setting.Value;

                setting = layer.GetSetting("TvWishList_MaxTvWishId", "0");
                myTvWishes.MaxTvWishId = Convert.ToInt32(setting.Value);
                Log.Debug("LoadSettings: MaxTvWishId=" + myTvWishes.MaxTvWishId.ToString());

                setting = layer.GetSetting("TvWishList_TvServerVersion", "0.0.0.0");
                TvVersion = setting.Value;

                setting = layer.GetSetting("TvWishList_MachineName", "NONE");
                HostComputer = setting.Value;
                Log.Debug("HostComputer = " + setting.Value);

                IList<Channel> allChannels = Channel.ListAll();
                IList<ChannelGroup> allChannelGroups = ChannelGroup.ListAll();
                IList<RadioChannelGroup> allRadioChannelGroups = RadioChannelGroup.ListAll();
                IList<Card> allCards = Card.ListAll();

                myTvWishes.TvServerSettings(prerecord, postrecord, allChannelGroups, allRadioChannelGroups, allChannels, allCards, TvWishItemSeparator);


                string listviewdata = "";
                string messageString = "";
                if (myTvWishes.ViewOnlyMode == true)
                {
                    Log.Debug("VIEWONLY=true    TvWishList_OnlyView");
                    listviewdata = myTvWishes.loadlongsettings("TvWishList_OnlyView");
                    messageString = myTvWishes.loadlongsettings("TvWishList_OnlyViewMessages");
                }
                else
                {
                    Log.Debug("VIEWONLY=false   TvWishList_ListView");
                    listviewdata = myTvWishes.loadlongsettings("TvWishList_ListView");
                    messageString = myTvWishes.loadlongsettings("TvWishList_ListViewMessages");
                }

                myTvWishes.LoadFromString(listviewdata, true);

                Log.Debug("messagestring=" + messageString);

                Log.Debug("mp load settings before reading messages: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
                mymessages.readxmlfile(messageString, false);
                //Log.Debug("mp load settings after reading messages: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
                //mymessages.updatemessages();
                //Log.Debug("mp load settings after update messages: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
                myTvWishes.UpdateCounters(mymessages.ListAllTvMessages());
                Log.Debug("mp load settings after update counters: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
#if (MP2)
                if (Main_GUI.MyTvProvider != null)
                {
                    Main_GUI.MyTvProvider.DeInit();
                }
               
#endif
                TvServerLoadSettings_FAILED = false;
            }
            catch (Exception exc)
            {
                Log.Error("[TVWishListMP]:TvserverdatabaseLoadSettings: ****** LOADSETTING FAILED Exception " + exc.Message);
                TvServerLoadSettings_FAILED = true;
                return false;
            }
            return true;
        }

        public bool TvserverdatabaseSaveSettings()
        {
            Log.Debug("Main_GUI: TvserverdDatabaseSaveSettings() called");


            DateTime startTvserverdatabaseLoadSettings = DateTime.Now;
            try
            {
                if (TvServerLoadSettings_FAILED == true)
                {
                    Log.Error("Error TvserverdatabaseSaveSettings(): Did not save data because loadsettings did fail with exception or tv wishes could not be locked");
                    return false;
                }

#if (!MP2)
                //connect to database if unconnected
                //myinterface.ConnectToDatabase();
#endif
                string wishdata = myTvWishes.SaveToString();

                Log.Debug("VIEWONLY =" + myTvWishes.ViewOnlyMode.ToString());

                // save settings

                Log.Debug("mp save settings before writexml: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
                string dataString = mymessages.writexmlfile(false);
                Log.Debug("mp save settings after writexml: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
                if (myTvWishes.ViewOnlyMode == true)
                {
                    Log.Debug("VIEWONLY = true   TvWishList_OnlyView");
                    myTvWishes.save_longsetting(wishdata, "TvWishList_OnlyView");
                    Log.Debug("TvWishList_OnlyView saved");
                    myTvWishes.save_longsetting(dataString, "TvWishList_OnlyViewMessages");
                    Log.Debug("TvWishList_OnlyViewMessages saved");
                }
                else
                {
                    Log.Debug("VIEWONLY = false  TvWishList_ListView ");
                    myTvWishes.save_longsetting(wishdata, "TvWishList_ListView");
                    Log.Debug("TvWishList_ListView saved");
                    myTvWishes.save_longsetting(dataString, "TvWishList_ListViewMessages");
                    Log.Debug("TvWishList_ListViewMessages saved");
                }




                TvBusinessLayer layer = new TvBusinessLayer();
                Setting setting;
                Log.Debug("SaveSetings: MaxTvWishId=" + myTvWishes.MaxTvWishId.ToString());
                setting = layer.GetSetting("TvWishList_MaxTvWishId", "0");
                setting.Value = myTvWishes.MaxTvWishId.ToString();
                setting.Persist();
#if (MP2)
                if (Main_GUI.MyTvProvider != null)
                {
                    Main_GUI.MyTvProvider.DeInit();
                }
#endif
                TimeSpan diff = DateTime.Now.Subtract(startTvserverdatabaseLoadSettings);
                Log.Debug("Tv Server SaveSetting time was :" + diff.ToString());

            }
            catch (Exception exc)
            {
                Log.Error("[TVWishListMP]:TvserverdatabaseSaveSettings: ****** SAVESETTING FAILED Exception " + exc.Message);
                TvServerLoadSettings_FAILED = true;

                return false;
            }
            return true;
        }                  


        public string FormatTvWish(TvWish mywish, string format)  //formatting must match the order of TvWishEntries on Edit window
        {
            //format cannot be directly logged!!!!
            string debugstring = format;
            debugstring = debugstring.Replace('{', '_');
            debugstring = debugstring.Replace('}', '_');
            //Log.Debug("format="+debugstring);

            string _formattedstring = "";
            try
            {
                _formattedstring = String.Format(format, mywish.active, mywish.searchfor, mywish.matchtype, mywish.group, mywish.recordtype, mywish.action, mywish.exclude, mywish.viewed, mywish.prerecord, mywish.postrecord, mywish.episodename, mywish.episodepart, mywish.episodenumber, mywish.seriesnumber, mywish.keepepisodes, mywish.keepuntil, mywish.recommendedcard, mywish.priority, mywish.aftertime, mywish.beforetime, mywish.afterdays, mywish.beforedays, mywish.channel, mywish.skip, mywish.name, mywish.useFolderName, mywish.withinNextHours, mywish.scheduled, mywish.tvwishid, mywish.recorded, mywish.deleted, mywish.emailed, mywish.conflicts, mywish.episodecriteria, mywish.preferredgroup, mywish.includeRecordings);
            }
            catch
            {
                _formattedstring = "Error in format string:\n" + format;
            }

            //Log.Debug("_formattedstring ="+_formattedstring);

            _formattedstring = _formattedstring.Replace(@"\n", Environment.NewLine); //new

            //  {0}   active
            //  {1}   searchfor
            //  {2}   matchtype
            //  {3}   group
            //  {4}   recordtype
            //  {5}   action
            //  {6}   exclude
            //  {7}   viewed
            //  {8}   prerecord
            //  {9}   postrecord
            //  {10}  episodename
            //  {11}  episodepart
            //  {12}  episodenumber
            //  {13}  seriesnumber
            //  {14}  keepepisodes
            //  {15}  keepuntil
            //  {16}  recommendedcard
            //  {17}  priority
            //  {18}  aftertime
            //  {19}  beforetime
            //  {20}  afterdays
            //  {21}  beforedays
            //  {22}  channel
            //  {23}  skip
            //  {24}  name
            //  {25}  useNameFolders
            //  {26}  withinNextHours
            //  {27}  scheduled
            //  {28}  tvwishid
            //  {29}  recorded 
            //  {30}  deleted
            //  {31}  emailedd
            //  {32}  conflicts
            //  {33}  episodecriteria
            //  {34}  preferredgroup
            //  {35}  includerecordings
            //modify for listview table changes
            return _formattedstring;
        }

        public void OnButtonRun()
        {
            Log.Debug("[Main_GUI]:OnButtonRun");
                                  

            //button lock
            if (myTvWishes.ButtonActive)
            {
                myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Operation Is Still Running"
                return;
            }
            myTvWishes.ButtonActive = true; //lock buttons

            try
            {

                if (PipeRunThread == null)
                {
                    Log.Debug("[TVWishListMP]:OnButtonRun Run Thread = null - starting new thread");
                    PipeRunThread = new System.Threading.Thread(RunThreadProcessing);
                    PipeRunThread.Start();
                    //StatusLabel( "New Thread Started - was null");
                }
                else if (PipeRunThread.IsAlive == false)
                {
                    Log.Debug("[TVWishListMP]:OnButtonRun Run Thread not alive - starting new thread");
                    PipeRunThread = new System.Threading.Thread(RunThreadProcessing);
                    PipeRunThread.Start();
                    //StatusLabel( "New Thread Started - was alive");
                }
                else
                {
                    Log.Debug("[TVWishListMP]:OnButtonRun: Old Thread must be running - do nothing");

                    myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Search Is Still Running"
                }
            }
            catch (Exception ex)
            {
                Log.Error("Run Thread could not be started - exception message is\n" + ex.Message);
            }


        }

        public void OnButtonSave()
        {
            Log.Debug("[Main_GUI]:OnButtonSave");

            //button lock
            if (myTvWishes.ButtonActive)
            {
                myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Operation Is Still Running"
                return;
            }
            myTvWishes.ButtonActive = true; //lock buttons


            if (TvserverdatabaseSaveSettings() == true)
            {
                myTvWishes.MyMessageBox(4400, 1300);   //Info, TvWish data have been saved
            }
            else
            {
                myTvWishes.MyMessageBox(4305, 1301);   //Error, TvWish data could not be saved
            }

            myTvWishes.ButtonActive = false; //unlock buttons
        }

        public void OnButtonCancel()
        {

            Log.Debug("[Main_GUI]:OnButtonCancel");

            //button lock
            if (myTvWishes.ButtonActive)
            {
                myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Operation Is Still Running"
                return;
            }
            myTvWishes.ButtonActive = true; //lock buttons

            bool ok = false;
            try
            {
                ok = TvserverdatabaseLoadSettings();
                UpdateListItems();
            }
            catch (Exception exc)
            {
                ok = false;
                Log.Error("[TVWishListMP]:Could not load data. Exception error " + exc.Message);
            }

            if (ok)
            {
                myTvWishes.MyMessageBox(4400, 1400);   //Info, TvWish data have been reloaded
            }
            else
            {

                myTvWishes.MyMessageBox(4305, 1401); //Error, TvWish data could not be reloaded             
            }

            myTvWishes.ButtonActive = false; //unlock buttons
        }

        public void OnButtonViewOnlyMode()
        {

            Log.Debug("[TVWishListMP]:OnButtonViewOnlyMode VIEWONLY=" + myTvWishes.ViewOnlyMode.ToString());

            //button lock
            if (myTvWishes.ButtonActive)
            {
                myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Operation Is Still Running"
                return;
            }
            myTvWishes.ButtonActive = true; //lock buttons


            TvserverdatabaseSaveSettings();
            myTvWishes.ViewOnlyMode = !myTvWishes.ViewOnlyMode;
            Log.Debug("Switchingevent VIEWONLY=" + myTvWishes.ViewOnlyMode.ToString());
            TvserverdatabaseLoadSettings();  //viewfile is loaded, too
            UpdateControls();
            UpdateListItems();

            myTvWishes.ButtonActive = false; //unlock buttons
        }

          

        #endregion public methods

        #region client pipes


        private void RunThreadProcessing()
        {
            Log.Debug("[TVWishListMP]:OnButtonRun");


            TvserverdatabaseSaveSettings(); //save data first before unlocking


            //*****************************************************
            //unlock TvWishList
            if (LOCKED == true)
            {
                bool ok = myTvWishes.UnLockTvWishList();
                if (!ok)
                {
                    Log.Error("Could not unlock before starting tvserver processing=");
                    myTvWishes.ButtonActive = false; //unlock buttons
                    return;
                }
                LOCKED = false;
                Log.Debug("6 LOCKED=" + LOCKED.ToString());
                TvServerLoadSettings_FAILED = true; //do not save data anymore after unlocking
            }

            //start versioncommand  - not needed
            //ClientMessage = PipeCommands.RequestTvVersion.ToString();
            //ClientThreadRoutineRunSingleCommand();
            //Log.Debug("TvServer Version = " + ReceivedMessage);

            //start runepgmode command

            if (myTvWishes.ViewOnlyMode)
            {
                ClientMessage = PipeCommands.StartEpg.ToString() + "VIEWONLY=TRUE TIMEOUT:" + TimeOutValueString;
            }
            else
            {
                ClientMessage = PipeCommands.StartEpg.ToString() + "VIEWONLY=FALSE TIMEOUT:" + TimeOutValueString;
            }

            if (PipeRunThreadActive)
            {
                myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Operation Is Still Running"
                myTvWishes.ButtonActive = false; //unlock buttons
                return;
            }
            PipeRunThreadActive = true; //lock pipe after saving settings

            ClientRoutineRunEpg();

            PipeRunThreadActive = false; //unlock pipe

            //*****************************************************
            //Lock Tvwishes
            bool success2 = myTvWishes.LockTvWishList("TvWishListMP:Main");
            if (!success2)
            {
                myTvWishes.MyMessageBox(4305, 4311); //Tv wish list is being processed by another process<br>Try again later<br>If the other process hangs reboot the system or stop the tv server manually
                myTvWishes.ButtonActive = false; //unlock buttons
                LOCKED = false; //unlock tvwishlist
                Log.Debug("11 LOCKED=" + LOCKED.ToString());
                TvServerLoadSettings_FAILED = true;

#if (MP2)
                IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
                workflowManager.NavigatePop(1); //same as escape (takes one entry from the stack)
#else
                GUIWindowManager.ActivateWindow(1); //goto tv mainwindow to avoid a partial loop   
#endif
            }
            else
            {
                LOCKED = true;
                Log.Debug("12 LOCKED=" + LOCKED.ToString());
                TvserverdatabaseLoadSettings();
                UpdateListItems();
            }



            //myTvWishes.MyMessageBox(4400, 1204);  //Epg Search Completed
            //myTvWishes.StatusLabel(statusmessage.Replace(@"\n", "")); //overwrite with last status message in case an error occured on the server
            Log.Debug("[TVWishListMP]:RunThreadProcessing Completed");

            myTvWishes.ButtonActive = false; //unlock buttons

        }

        public void StopClient()
        {
            try
            {
                if (pipeClient != null)
                {
                    pipeClient.Close();
                    if (pipeClient != null)
                        pipeClient = null;
                    Log.Debug("Pipeclient closed");
                    Thread.Sleep(1000);
                }

                if (PipeRunThread != null)
                {
                    PipeRunThread.Abort();
                    PipeRunThread = null;
                }
            }
            catch (Exception exc)
            {
                Log.Debug("StopClient() exception=" + exc.Message);
            }
        }


        private void ClientRoutineRunEpg()
        {
            bool success = false;
            for (int i = 1; i <= 3; i++) //three retries for sending command
            {
                TIMEOUT = false;
                try
                {
#if MP2
                    //store tv server language for parent, switch and restore at the end
                    CultureInfo currentCulture = ServiceRegistration.Get<ILocalization>().CurrentCulture;                
#else //MP1
                    string language = PluginGuiLocalizeStrings.CurrentLanguage();
                    Log.Debug("language=" + language);
                    string cultureName = PluginGuiLocalizeStrings.GetCultureName(language);
                    Log.Debug("cultureName=" + cultureName);
                    

                    CultureInfo currentCulture = CultureInfo.CreateSpecificCulture(cultureName);                   
#endif

                    string languageFile = "strings_" + currentCulture.Parent.TwoLetterISOLanguageName.ToString() + ".xml";
                    Log.Debug("languageFile =" + languageFile);

                    TvBusinessLayer layer = new TvBusinessLayer();
                    Setting setting;
                    setting = layer.GetSetting("TvWishList_LanguageFile", "strings_en.xml");
                    //string backupLanguageFile = setting.Value;  do not restore back
                    setting.Value = languageFile;
                    setting.Persist();


                    string hostname = ".";
                    if (HostComputer != "localhost")
                    {
                        hostname = HostComputer;
                    }


                    pipeClient = new NamedPipeClientStream(hostname, "TvWishListPipe",
                            PipeDirection.InOut, PipeOptions.None,
                            TokenImpersonationLevel.Impersonation);

                    Log.Debug("ClientThreadRoutineRunEpg: Connecting to server " + HostComputer);
                    pipeClient.Connect();
                    Log.Debug("hostname=" + hostname);
                    StreamString ss = new StreamString(pipeClient);
                    //send clientmessage to server
                    Log.Debug("Writing command " + ClientMessage);
                    ss.WriteString(ClientMessage);

                    ReceivedMessage = "ClientThreadRoutineRunEpg Error";
                    while ((ReceivedMessage.StartsWith(PipeCommands.Ready.ToString())==false) && (ReceivedMessage.StartsWith(PipeCommands.Error.ToString()) == false))
                    {
                        PipeRunThreadTimeOutCounter = new Thread(ClientTimeOutError);
                        PipeRunThreadTimeOutCounter.Start();
                        ReceivedMessage = ss.ReadString();
                        PipeRunThreadTimeOutCounter.Abort();

                        //evaluate response from server
                        string Processedmessage = string.Empty;
                        if (ReceivedMessage.StartsWith(PipeCommands.Ready.ToString()) == true)
                        {
                            Processedmessage = ReceivedMessage.Substring(PipeCommands.Ready.ToString().Length);
                        }
                        else if (ReceivedMessage.StartsWith(PipeCommands.Error.ToString()) == true)
                        {
                            Processedmessage = ReceivedMessage.Substring(PipeCommands.Error.ToString().Length);
                        }
                        else
                        {
                            Processedmessage = ReceivedMessage;
                        }

                        myTvWishes.StatusLabel(Processedmessage);
                        Log.Debug("***** SERVERMESSAGE=" + ReceivedMessage);
                    }

                    Log.Debug("closing client pipe - command executed");
                    if (pipeClient != null)
                    {
                        pipeClient.Close();
                        if (pipeClient != null)
                            pipeClient = null;
                    }

                    //restore language file -  do not restore back
                    /*setting = layer.GetSetting("TvWishList_LanguageFile", "strings_en.xml");
                    setting.Value = backupLanguageFile;
                    setting.Persist();*/

                    success = true;
                    break;
                }
                catch (Exception exc)
                {
                    Log.Debug("Sending tv server command failed in iteration i=" + i.ToString());



                    Log.Debug("ClientThread() exception=" + exc.Message);
                    Thread.Sleep(2000);
                    if (pipeClient != null)
                    {
                        pipeClient.Close();
                        if (pipeClient != null)
                            pipeClient = null;
                    }

                    if (TIMEOUT)
                    {
                        TIMEOUT = false;
                        break;
                    }
                }
            }

            if (success == false)
            {
                myTvWishes.MyMessageBox(4305, 1206); //TvWishList MediaPortal Plugin Does Not Match To TvWishList TV Server Plugin
            }
            Log.Debug("ClientThreadRoutineRunEpg() Thread Completed");
            //OldConnect = false;
        }


        private void ClientTimeOutError()
        {
            TIMEOUT = false;
            int timeOutValue = 60;
            try
            {
                timeOutValue = Convert.ToInt32(TimeOutValueString);
            }
            catch { }

            for (int i = 0; i < timeOutValue; i++)
            {
                Thread.Sleep(1000);
            }
            TIMEOUT = true;
            Log.Debug("Timeout Error Client - stopping");
            StopClient();
            ReceivedMessage = PipeCommands.Error_TimeOut.ToString();
            myTvWishes.StatusLabel(ReceivedMessage);

            myTvWishes.MyMessageBox(4305, 4312); //Timeout Error: Try to reboot your server.\nIf it reoccurs try to increase the timeout value in the expert settings

            PipeRunThreadActive = false; //always needed because pipe is stopped

            if (ClientMessage.StartsWith(PipeCommands.StartEpg.ToString()))
                myTvWishes.ButtonActive = false; //unlock buttons only for StartEPG command
        }

        public string RunSingleCommand(string command)
        {
            if (PipeRunThreadActive)
            {
                return (PluginGuiLocalizeStrings.Get(1200)); //Waiting for old process to finish
            }
            PipeRunThreadActive = true;

            ClientMessage = command;
            try
            {
                PipeRunThread = new Thread(ClientThreadRoutineRunSingleCommand);
                PipeRunThread.Start();
            }
            catch (Exception exc)
            {
                Log.Error("RunSingleCommand: SendClient() exception=" + exc.Message);
            }

            //wait for finish
            while (PipeRunThread.IsAlive)
            {
                Log.Debug("RunSingleCommand: Waiting for client thread to finish");
                Thread.Sleep(100);
            }

            PipeRunThreadActive = false;
            return ReceivedMessage;
        }

        private void ClientThreadRoutineRunSingleCommand()
        {
            ReceivedMessage = "Error in ClientThreadRoutineRunSingleCommand";
            try
            {
                string hostname = ".";
                if (HostComputer != "localhost")
                {
                    hostname = HostComputer;
                }


                pipeClient = new NamedPipeClientStream(hostname, "TvWishListPipe",
                        PipeDirection.InOut, PipeOptions.None,
                        TokenImpersonationLevel.Impersonation);

                Log.Debug("ClientThreadRoutineRunSingleCommand: Connecting to server " + HostComputer);
                pipeClient.Connect();
                Log.Debug("hostname=" + hostname);
                StreamString ss = new StreamString(pipeClient);
                //Log.Debug("1 ClientMessage=" + ClientMessage);
                //send clientmessage to server
                ss.WriteString(ClientMessage);
                //Log.Debug("2");
                //pipeClient.ReadTimeout = 5000; //timeout not supported for async streams

                PipeRunThreadTimeOutCounter = new Thread(ClientTimeOutError);
                PipeRunThreadTimeOutCounter.Start();
                ReceivedMessage = ss.ReadString();
                PipeRunThreadTimeOutCounter.Abort();


                Log.Debug("ClientThreadRoutineRunSingleCommand: ***** SERVERMESSAGE=" + ReceivedMessage);


                Log.Debug("ClientThreadRoutineRunSingleCommand: closing client pipe - command executed");
            }
            catch (Exception exc)
            {
                Log.Debug("ClientThreadRoutineRunSingleCommand: ClientThread() exception=" + exc.Message);
            }

            if (pipeClient != null)
            {
                pipeClient.Close();
                //pipeClient = null;
            }

            Log.Debug("ClientThreadRoutineRunSingleCommand: Pipe Client Thread Completed");
            //OldConnect = false;
            return;
        }

        #endregion clientpipes

    }

    //needed for pipes:
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
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }

    //needed for pipes:
    // Contains the method executed in the context of the impersonated user
    public class ReadFileToStream
    {
        private string fn;
        private StreamString ss;

        public ReadFileToStream(StreamString str, string filename)
        {
            fn = filename;
            ss = str;
        }

        public void Start()
        {
            string contents = File.ReadAllText(fn);
            ss.WriteString(contents);
        }
    }

}
