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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using TvWishList;
using Log = TvLibrary.Log.huha.Log;

#if (MP12 || MP11)

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;

using Action = MediaPortal.GUI.Library.Action;
//using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;
using MediaPortal.Configuration;
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
    public partial class Edit_GUI
    {

        //Instance
        public static Edit_GUI _instance = null;

        public static Edit_GUI Instance
        {
            get { return (Edit_GUI)_instance; }
        }

        int _restore_selected_Tvwish = -1;

        


        bool ALLITEMS = false;

        //TvWishProcessing myTvWishes = null;
        XmlMessages mymessages = null;



        //int[] _listTranslator = new int[(int)TvWishEntries.end];

        


        #region properties
        public TvWishProcessing TvWishes
        {
            get { return myTvWishes; }
            set { myTvWishes = value; }
        }
        public XmlMessages MyMessages
        {
            get { return mymessages; }
            set { mymessages = value; }
        }
        #endregion

        #region private methods

        public void OnButtonSave()
        {
            Log.Debug("[TVWishListMP GUI_Edit]:OnButtonSave");

            bool ok = false;
            try
            {
                ok = Main_GUI.Instance.TvserverdatabaseSaveSettings();
            }
            catch (Exception exc)
            {
                ok = false;
                Log.Error("[TVWishListMP GUI_Edit]:Gui Main window could not be activated. Could not save data. Exception error " + exc.Message);
            }
            if (ok)
            {
                myTvWishes.MyMessageBox(4400, 2300);   //Info, TvWish data have been saved
            }
            else
            {
                myTvWishes.MyMessageBox(4305, 2301);   //Error, TvWish data could not be saved
            }
        }

        public void OnButtonCan()
        {
            Log.Debug("[TVWishListMP GUI_Edit]:OnButtonCancel");

            bool ok = false;
            try
            {
                //myfacade.Clear();
                ok = Main_GUI.Instance.TvserverdatabaseLoadSettings();
                if (myTvWishes.FocusedWishIndex >= myTvWishes.ListAll().Count)
                    myTvWishes.FocusedWishIndex = 0;

                UpdateListItems();
            }
            catch (Exception exc)
            {
                ok = false;
                Log.Error("[TVWishListMP GUI_Edit]:Gui Main window could not be activated. Could not load data. Exception error " + exc.Message);
            }

            if (ok)
            {
                myTvWishes.MyMessageBox(4400, 2400);   //Info, TvWish data have been reloaded
            }
            else
            {

                myTvWishes.MyMessageBox(4305, 2401); //Error, TvWish data could not be reloaded             
            }
        }       

        public void OnButtonNext()
        {
            Log.Debug("OnButtonNext");
            if (myTvWishes.ListAll().Count == 0)
                return;

            myTvWishes.FocusedWishIndex++;
            if (myTvWishes.FocusedWishIndex > myTvWishes.ListAll().Count - 1)
            {
                myTvWishes.FocusedWishIndex = 0;
            }

            UpdateListItems();
        }

        public void OnButtonPrevious()
        {
            Log.Debug("OnButtonNext");
            if (myTvWishes.ListAll().Count == 0)
                return;

            myTvWishes.FocusedWishIndex--;
            if (myTvWishes.FocusedWishIndex < 0)
            {
                myTvWishes.FocusedWishIndex = myTvWishes.ListAll().Count - 1;
            }

            UpdateListItems();
        }

        public void OnButtonDelete()
        {
            Log.Debug("[TVWishListMP GUI_Edit]:OnButtonDelete");

            if (myTvWishes.ListAll().Count == 0)
                return;

            try
            {

                myTvWishes.RemoveAtIndex(myTvWishes.FocusedWishIndex);
                if (myTvWishes.FocusedWishIndex > myTvWishes.ListAll().Count - 1)
                {
                    myTvWishes.FocusedWishIndex = myTvWishes.ListAll().Count - 1;
                }


                //update message.tvwishid if tvwish has been deleted or is unknown
                Log.Debug("mp list window before updating messages: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
                for (int i = mymessages.ListAllTvMessages().Count - 1; i >= 0; i--)
                {
                    xmlmessage mymessage = mymessages.GetTvMessageAtIndex(i);
                    TvWish mywish = myTvWishes.RetrieveById(mymessage.tvwishid);
                    if (mywish == null)
                    {
                        Log.Debug("deleting " + mymessage.title + " at " + mymessage.start.ToString() + " ID: " + mymessage.tvwishid);
                        mymessages.DeleteTvMessageAt(i);
                    }

                }
                Log.Debug("mp list window after updating messages: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());

                UpdateListItems();

                myTvWishes.MyMessageBox(4400, 2200);   //Info, TvWish data have been deleted


            }
            catch (Exception exc)
            {
                Log.Error("[TVWishListMP]:Could not delete data. Exception error " + exc.Message);
                myTvWishes.MyMessageBox(4305, 2201); //Error, TvWish data could not be deleted 
            }
        }


        protected void MoreButtonEvaluation(int number)
        {

            switch (number)
            {
                case 1: //User Defined Items / All Items                    
                    ALLITEMS = !ALLITEMS;
#if (MP2)
                    selected_edit_index = 0;
#endif
                    //Select edit items based on VIEW or EMAIL mode
                    UpdateEditItemTranslator();
                    UpdateListItems();
                    break;

                case 2:    //View results of a single TvWish
                    mymessages.FilterName = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).tvwishid;
#if (MP2)
                    IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
                    //now push to Result_GUI
                    string name = "Push to to Result_GUI";
                    string displayname = "[TvWishListMP2.1108]";
                    string mainscreen = "Result_GUI";
                    workflowManager.NavigatePushTransient(new WorkflowState(Guid.NewGuid(), name, displayname, true, mainscreen, false, true, RESULT_GUI_MODEL_ID, WorkflowType.Workflow), null);
#else
                    GUIWindowManager.ActivateWindow(_guilistwindowid);
#endif
                    break;

                case 3:  //Copy TvWishes from current mode to other mode
                    CopyToOtherMode();
                    break;

                case 4: //Clone TvWish
                    CloneTvWish();
                    break;
            }
        }

        protected void CopyToOtherMode() //Copy TvWishes from current mode to other mode
        {
            try
            {
                int itemcount = 0;

                if (Main_GUI.Instance.TvserverdatabaseSaveSettings() == false)
                {
                    Log.Error("B1: TvserverdatabaseSaveSettings failed");
                }
                List<TvWish> tempTvWishes = new List<TvWish>(myTvWishes.ListAll()); //copy data to temp list


                //invert viewmode and load data
                myTvWishes.ViewOnlyMode = !myTvWishes.ViewOnlyMode;
                if (Main_GUI.Instance.TvserverdatabaseLoadSettings() == false)
                {
                    Log.Error("B3: TvserverdatabaseLoadSettings failed");
                }

                //copy data if not available yet  


                TvWish temp_wish = tempTvWishes[myTvWishes.FocusedWishIndex];
                bool found = false;
                for (int j = 0; j < myTvWishes.ListAll().Count; j++)
                {
                    TvWish actual_wish = myTvWishes.GetAtIndex(j);
                    if (temp_wish.searchfor == actual_wish.searchfor)
                    {
                        Log.Debug("found = true  temp_wish.searchfor=" + temp_wish.searchfor);
                        found = true;
                        break;
                    }
                }
                if (found == false)
                {
                    temp_wish.action = PluginGuiLocalizeStrings.Get(2701);  //Email
                    //temp_wish.hits = "0";
                    myTvWishes.Add(temp_wish);
                    itemcount++;
                    Log.Debug("New wish added: " + temp_wish.searchfor);
                }


                //save copied data
                if (Main_GUI.Instance.TvserverdatabaseSaveSettings() == false)
                {
                    Log.Error("B4: TvserverdatabaseSaveSettings failed");
                }

                //restore current data
                myTvWishes.ViewOnlyMode = !myTvWishes.ViewOnlyMode;
                if (Main_GUI.Instance.TvserverdatabaseLoadSettings() == false)
                {
                    Log.Error("B5: TvserverdatabaseLoadSettings failed");
                }

                //Info, Copying TvWishes from View Only to Email completed and found {0} wishes
                myTvWishes.MyMessageBox(4400, string.Format(PluginGuiLocalizeStrings.Get(2454), itemcount.ToString())); //{0} Wish(es) Copied to Other Mode
            }
            catch (Exception exc)
            {
                myTvWishes.MyMessageBox(4305, 2455); //Failed to Copy Tv Wish to Other Mode
                Log.Debug("Error in More Case 3 - Exception:" + exc.Message);
            }
        }

        protected void CloneTvWish() //Clone TvWish
        {
            try
            {
                int i = Convert.ToInt32(myTvWishes.FocusedWishIndex);
                TvWish new_wish = myTvWishes.GetAtIndex(i);
                myTvWishes.MaxTvWishId++;
                new_wish.tvwishid = myTvWishes.MaxTvWishId.ToString();
                myTvWishes.Add(new_wish);
                myTvWishes.MyMessageBox(4400, PluginGuiLocalizeStrings.Get(2457)); //Tv wish cloned                   
            }
            catch (Exception exc)
            {
                myTvWishes.MyMessageBox(4305, 2458); //Failed to clone Tv Wish
                Log.Debug("Tv wish could not be cloned - Exception:" + exc.Message);
            }
        }

        protected void UpdateEditItemTranslator()
        {
            if (ALLITEMS == false)
            {
                for (int i = 0; i < myTvWishes._boolTranslator.Length; i++)
                {
                    myTvWishes._boolTranslator[i] = myTvWishes._boolTranslatorbackup[i];
                }
            }
            else
            {
                for (int i = 0; i < myTvWishes._boolTranslator.Length; i++)
                {
                    myTvWishes._boolTranslator[i] = true;
                }
            }

            //Select edit items based on VIEW or EMAIL mode
            if (myTvWishes.ViewOnlyMode == true) //do not use skip and action item
            {
                myTvWishes._boolTranslator[(int)TvWishEntries.searchfor] = true; //always searchfor
                myTvWishes._boolTranslator[(int)TvWishEntries.action] = false;
                //myTvWishes._boolTranslator[(int)TvWishEntries.postrecord] = false;
                //myTvWishes._boolTranslator[(int)TvWishEntries.prerecord] = false;
                myTvWishes._boolTranslator[(int)TvWishEntries.keepepisodes] = false;
                //myTvWishes._boolTranslator[(int)TvWishEntries.keepuntil] = false;
                myTvWishes._boolTranslator[(int)TvWishEntries.recordtype] = false;
                myTvWishes._boolTranslator[(int)TvWishEntries.recommendedcard] = false;
                myTvWishes._boolTranslator[(int)TvWishEntries.priority] = false;
                myTvWishes._boolTranslator[(int)TvWishEntries.skip] = false;
                myTvWishes._boolTranslator[(int)TvWishEntries.useFolderName] = false;
                myTvWishes._boolTranslator[(int)TvWishEntries.episodecriteria] = false;
                myTvWishes._boolTranslator[(int)TvWishEntries.preferredgroup] = false;
                myTvWishes._boolTranslator[(int)TvWishEntries.includerecordings] = false;
                myTvWishes._boolTranslator[(int)TvWishEntries.withinNextHours] = (myTvWishes._boolTranslatorbackup[(int)TvWishEntries.withinNextHours] || ALLITEMS);
                //modify for listview table changes
            }
            else
            {
                myTvWishes._boolTranslator[(int)TvWishEntries.searchfor] = true; //always searchfor
                myTvWishes._boolTranslator[(int)TvWishEntries.action] = (myTvWishes._boolTranslatorbackup[(int)TvWishEntries.action] || ALLITEMS);  //restore actiion and skip item from loadsettings
                //myTvWishes._boolTranslator[(int)TvWishEntries.postrecord] = (myTvWishes._boolTranslatorbackup[(int)TvWishEntries.postrecord] || ALLITEMS);
                //myTvWishes._boolTranslator[(int)TvWishEntries.prerecord] = (myTvWishes._boolTranslatorbackup[(int)TvWishEntries.prerecord] || ALLITEMS);
                myTvWishes._boolTranslator[(int)TvWishEntries.keepepisodes] = (myTvWishes._boolTranslatorbackup[(int)TvWishEntries.keepepisodes] || ALLITEMS);
                //myTvWishes._boolTranslator[(int)TvWishEntries.keepuntil] = (myTvWishes._boolTranslatorbackup[(int)TvWishEntries.keepuntil] || ALLITEMS);
                myTvWishes._boolTranslator[(int)TvWishEntries.recordtype] = (myTvWishes._boolTranslatorbackup[(int)TvWishEntries.recordtype] || ALLITEMS);
                myTvWishes._boolTranslator[(int)TvWishEntries.recommendedcard] = (myTvWishes._boolTranslatorbackup[(int)TvWishEntries.recommendedcard] || ALLITEMS);
                myTvWishes._boolTranslator[(int)TvWishEntries.priority] = (myTvWishes._boolTranslatorbackup[(int)TvWishEntries.priority] || ALLITEMS);
                myTvWishes._boolTranslator[(int)TvWishEntries.skip] = (myTvWishes._boolTranslatorbackup[(int)TvWishEntries.skip] || ALLITEMS);
                myTvWishes._boolTranslator[(int)TvWishEntries.useFolderName] = (myTvWishes._boolTranslatorbackup[(int)TvWishEntries.useFolderName] || ALLITEMS);
                myTvWishes._boolTranslator[(int)TvWishEntries.episodecriteria] = (myTvWishes._boolTranslatorbackup[(int)TvWishEntries.episodecriteria] || ALLITEMS);
                myTvWishes._boolTranslator[(int)TvWishEntries.preferredgroup] = (myTvWishes._boolTranslatorbackup[(int)TvWishEntries.preferredgroup] || ALLITEMS);
                myTvWishes._boolTranslator[(int)TvWishEntries.includerecordings] = (myTvWishes._boolTranslatorbackup[(int)TvWishEntries.includerecordings] || ALLITEMS);
                myTvWishes._boolTranslator[(int)TvWishEntries.withinNextHours] = false;
                //modify for listview table changes
            }

            // Log.Debug("ALLITEMS=" + ALLITEMS.ToString());
            // Log.Debug("VIEWONLY=" + myTvWishes.ViewOnlyMode.ToString());
            // Log.Debug("myTvWishes._boolTranslator[(int)TvWishEntries.action]=" + myTvWishes._boolTranslator[(int)TvWishEntries.action].ToString());
            // Log.Debug("myTvWishes._boolTranslator[(int)TvWishEntries.useFolderName]=" + myTvWishes._boolTranslator[(int)TvWishEntries.useFolderName].ToString());
            // Log.Debug("myTvWishes._boolTranslator[(int)TvWishEntries.withinNextHours]=" + myTvWishes._boolTranslator[(int)TvWishEntries.withinNextHours].ToString());
        }

        #endregion private methods

    }

}