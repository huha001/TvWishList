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
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using Log = TvLibrary.Log.huha.Log;

using Action = MediaPortal.GUI.Library.Action;
//using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;
//using TvWishList;
using MediaPortal.Configuration;

using TvControl;
using TvDatabase;
using Gentle.Framework;
 
//using GUIKeyboard = MediaPortal.GUI.Library.huha.GUIKeyboard;


namespace MediaPortal.Plugins.TvWishList
{
    public partial class Edit_GUI : GUIWindow  
    {
        [SkinControlAttribute(3)]
        protected GUIButtonControl buttonPrev = null;
        [SkinControlAttribute(4)]
        protected GUIButtonControl buttonNext = null;
        [SkinControlAttribute(5)]
        protected GUIButtonControl buttonNew = null;
        [SkinControlAttribute(6)]
        protected GUIButtonControl buttonDelete = null;
        [SkinControlAttribute(7)]
        protected GUIButtonControl buttonSave = null;
        [SkinControlAttribute(8)]
        protected GUIButtonControl buttonCancel = null;
        [SkinControlAttribute(9)]
        protected GUIButtonControl buttonMore = null;

        
        [SkinControlAttribute(50)] protected GUIListControl myfacade = null;

        int _guimainwindowid = 70943675;
        int _guilistwindowid = 70943676;
        int _guieditwindowid = 70943677;

        TvWishProcessing myTvWishes = null;

        int[] _listTranslator = new int[(int)TvWishEntries.end];

        #region constructor
        public Edit_GUI()
     	{
            Log.Debug("[TVWishListMP GUI_Edit]:GUI_Edit()");

            _instance = this;

            GetID = _guieditwindowid;
     	}
        #endregion


        public override bool SupportsDelayedLoad
        {
            get { return false; }
        }

        public override bool Init()
     	{
            Log.Debug("[TVWishListMP GUI_Edit]:Init");            
            PluginGuiLocalizeStrings.LoadMPlanguage();

            

            bool bResult = Load(GUIGraphicsContext.Skin + @"\TvWishListMP_3.xml");
            GetID = _guieditwindowid;           
            return bResult;
     	}

       
        protected override void OnPageLoad()
        {
            Log.Debug("[TVWishListMP GUI_Edit]:OnPageLoad  myTvWishes.FocusedWishIndex= " + myTvWishes.FocusedWishIndex.ToString());
            Log.Debug("Selected Tv wish name="+myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).name);

            if ((this.PreviousWindowId != _guimainwindowid)&&(this.PreviousWindowId != _guilistwindowid))
            {
                //go back to main window
                Main_GUI mymainwindow = (Main_GUI)GUIWindowManager.GetWindow(_guimainwindowid);
                mymainwindow.TvserverdatabaseLoadSettings();
                GUIWindowManager.ActivateWindow(_guimainwindowid);
            }

            //Select edit items based on VIEW or EMAIL mode
            UpdateEditItemTranslator();
            
            
            _restore_selected_Tvwish = myTvWishes.FocusedWishIndex;     
            UpdateControls();
            UpdateListItems();
            GUIControl.FocusControl(_guieditwindowid, 50);
            base.OnPageLoad();
        }

        protected override void OnPageDestroy(int newWindowId)
        {
            Log.Debug("[TVWishListMP GUI_Edit]:OnPageDestroy");
            Log.Debug("Tvwishes.Count=" + myTvWishes.ListAll().Count.ToString());
            //Log.Debug("FocusedEditIndex=" + myTvWishes.FocusedEditIndex.ToString());

            //save data if going to a new page
            Log.Debug("newWindowId=" + newWindowId.ToString());
            if ((newWindowId != _guilistwindowid) && (newWindowId != _guimainwindowid))
            {
                Main_GUI mymainwindow = (Main_GUI)GUIWindowManager.GetWindow(_guimainwindowid);
                if (mymainwindow.TvserverdatabaseSaveSettings() == true)
                {
                    myTvWishes.MyMessageBox(4400, 1300);   //Info, TvWish data have been saved
                }
                else
                {
                    myTvWishes.MyMessageBox(4305, 1301);   //Error, TvWish data could not be saved
                }

                //*****************************************************
                //unlock TvWishList
                myTvWishes.UnLockTvWishList();
                mymainwindow.LOCKED = false;
                Log.Debug("13 mymainwindow.LOCKED=" + mymainwindow.LOCKED.ToString());
            }
            

            myfacade.Clear();
            base.OnPageDestroy(newWindowId);
        }

        
 
        public override void OnAction(Action action)
     	{
            //Log.Debug("[TVWishListMP GUI_Edit]:OnAction  (myfacade.IsFocused="+myfacade.IsFocused.ToString())

            switch (action.wID) //general actions
            {
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_PREVIOUS_MENU:
                    GUIWindowManager.ShowPreviousWindow();
                    return;

                case MediaPortal.GUI.Library.Action.ActionType.ACTION_EXIT:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_HIBERNATE:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_POWER_OFF:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_REBOOT:
                    Main_GUI mymainwindow = (Main_GUI)GUIWindowManager.GetWindow(_guimainwindowid);
                    if (mymainwindow.TvserverdatabaseSaveSettings() == true)
                    {
                        myTvWishes.MyMessageBox(4400, 1300);   //Info, TvWish data have been saved
                    }
                    else
                    {
                        myTvWishes.MyMessageBox(4305, 1301);   //Error, TvWish data could not be saved
                    }

                    break;

            }


            /*
    	    if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
     	    {
     	        GUIWindowManager.ShowPreviousWindow();
     	        return;
     	    }*/


            /*else if (action.wID == Action.ActionType.ACTION_MOUSE_MOVE)
            {
                //StatusLabel( "SelectedListItemIndex" + myfacade.SelectedListItemIndex.ToString() + "x=" + action.fAmount1.ToString() + "y=" + action.fAmount2.ToString() + "posx=" + myfacade.XPosition.ToString() + "width=" + myfacade.Width.ToString());
                if ((action.fAmount1 < myfacade.XPosition) || (action.fAmount1 > myfacade.XPosition + myfacade.Width) || (action.fAmount2 < myfacade.YPosition) || (action.fAmount2 > myfacade.YPosition + myfacade.Height))
                    _focused = -1;
                else
                    _focused = myfacade.SelectedListItemIndex;
            }*/

            /*
            if (myfacade.IsFocused)  
            {
                Log.Debug("Action Id: {0}", action.wID);
                switch (action.wID)
                {
                    case MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM:
                    case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOUSE_CLICK:
                        ItemSelectionChanged();                       
                        break;
                }
            }*/

     	    base.OnAction(action);
     	}

        //GUI clicked
        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
        {
            Log.Debug("[TVWishListMP GUI_Edit]:OnClicked");


            if (control == myfacade)
            {
                ItemSelectionChanged(); 
            }
            else if (control == buttonNew)
            {
                OnButtonNew();
            }
            else if (control == buttonCancel)
            {
                OnButtonCan();
            }
            else if (control == buttonNext)
            {
                OnButtonNext();
            }
            else if (control == buttonPrev)
            {
                OnButtonPrevious();
            }
            else if (control == buttonSave)
            {
                OnButtonSave();
            }
            else if (control == buttonDelete)
            {
                Main_GUI mymainwindow = (Main_GUI)GUIWindowManager.GetWindow(_guimainwindowid);              
                OnButtonDelete();  
            }
            else if (control == buttonMore)
            {
                OnButtonMore();
            }
            
            base.OnClicked(controlId, control, actionType);
        }

        protected void OnButtonNew()
        {
            Log.Debug("[TVWishListMP GUI_Edit]:OnButtonAdd");
            myTvWishes.Add(myTvWishes.DefaultData());
            myTvWishes.FocusedWishIndex = myTvWishes.ListAll().Count - 1;
            TvWish mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
            Log.Debug("myTvWishes.FocusedWishIndex=" + myTvWishes.FocusedWishIndex.ToString());
            string keyboardstring = mywish.searchfor;
            if (GetUserInputString(ref keyboardstring, false) == true)
            {
                mywish.searchfor = keyboardstring;
                mywish.name = keyboardstring;
                myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
            }
            UpdateListItems();

        }
        
        private void OnButtonMore()
        {
            Log.Debug("[TVWishListMP GUI_List]:OnButtonMore");


            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            
            dlg.SetHeading(PluginGuiLocalizeStrings.Get(2107));

            if (ALLITEMS)
                dlg.Add(PluginGuiLocalizeStrings.Get(2451));   //User Defined Items
            else
                dlg.Add(PluginGuiLocalizeStrings.Get(2450));   //All Items

            dlg.Add(PluginGuiLocalizeStrings.Get(2452));  //View Results
            dlg.Add(PluginGuiLocalizeStrings.Get(2453));  //Copy wish to other mode
            dlg.Add(PluginGuiLocalizeStrings.Get(2456));  //clone tvwish            

            dlg.ShowQuickNumbers = false;
            dlg.DoModal(GUIWindowManager.ActiveWindow);

            MoreButtonEvaluation(dlg.SelectedId);

        }

        protected void UpdateControls()
        {
            Log.Debug("[TVWishListMP GUI_Edit]:UpdateControls VIEWONLY=" + myTvWishes.ViewOnlyMode.ToString());
            GUIPropertyManager.SetProperty("#headershort.label", String.Format(PluginGuiLocalizeStrings.Get(2000)));

            if (myTvWishes.ViewOnlyMode == false)
            {
                GUIPropertyManager.SetProperty("#header.label", String.Format(PluginGuiLocalizeStrings.Get(2000)) + "  " + String.Format(PluginGuiLocalizeStrings.Get(4200))); //"TVWishList - Main-Email&Record");        
                GUIPropertyManager.SetProperty("#modus.label", String.Format(PluginGuiLocalizeStrings.Get(4200))); //"Email&Record");
            }
            else
            {
                GUIPropertyManager.SetProperty("#header.label", String.Format(PluginGuiLocalizeStrings.Get(2000)) + "  " + String.Format(PluginGuiLocalizeStrings.Get(4201))); //"TVWishList - Main-Viewonly");            
                GUIPropertyManager.SetProperty("#modus.label", String.Format(PluginGuiLocalizeStrings.Get(4201))); //"Viewonly");            
            }


            GUIPropertyManager.SetProperty("#textbox.label", "");
            myTvWishes.StatusLabel("");

            GUIControl.SetControlLabel(_guieditwindowid, 2, String.Format(PluginGuiLocalizeStrings.Get(2100)));//Back
            GUIControl.SetControlLabel(_guieditwindowid, 3, String.Format(PluginGuiLocalizeStrings.Get(2101)));//Prev
            GUIControl.SetControlLabel(_guieditwindowid, 4, String.Format(PluginGuiLocalizeStrings.Get(2102)));//Next
            GUIControl.SetControlLabel(_guieditwindowid, 5, String.Format(PluginGuiLocalizeStrings.Get(2103)));//New
            GUIControl.SetControlLabel(_guieditwindowid, 6, String.Format(PluginGuiLocalizeStrings.Get(2104)));//Delete
            GUIControl.SetControlLabel(_guieditwindowid, 7, String.Format(PluginGuiLocalizeStrings.Get(2105)));//Save
            GUIControl.SetControlLabel(_guieditwindowid, 8, String.Format(PluginGuiLocalizeStrings.Get(2106)));//Cancel
            GUIControl.SetControlLabel(_guieditwindowid, 9, String.Format(PluginGuiLocalizeStrings.Get(2107)));//More
        }

        protected void AddEditItem(int itemNumber, string itemString, ref int ctr)
        {
            if (myTvWishes._boolTranslator[itemNumber])
            {
                string mystring = String.Format(PluginGuiLocalizeStrings.Get(2500 + itemNumber), itemString);
                GUIListItem myGuiListItem = new GUIListItem(mystring);
                myfacade.Add(myGuiListItem);
                _listTranslator[ctr] = itemNumber;
                ctr++;
            }

        }

        protected void UpdateListItems()
        {
            Log.Debug("[TVWishListMP GUI_Edit]:UpdateListItems");
            //display edit items
            GUIControl.ClearControl(GetID, myfacade.GetID);            
            
            Log.Debug("myTvWishes.ListAll() " + myTvWishes.ListAll().ToString());
            //string[] edit_item_names = new string[24];
           
            Log.Debug("_listTranslator.Length=" + _listTranslator.Length.ToString());
            for (int i = 0; i < _listTranslator.Length;i++ )
            {
                _listTranslator[i] = -1;
            }
            try
            {
                myfacade.Clear();
                Log.Debug("myTvWishes.FocusedWishIndex=" + myTvWishes.FocusedWishIndex.ToString());
                TvWish mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                int ctr = 0;

                AddEditItem((int)TvWishEntries.active, mywish.active, ref ctr);
                AddEditItem((int)TvWishEntries.name, mywish.name, ref ctr);
                AddEditItem((int)TvWishEntries.searchfor, mywish.searchfor, ref ctr);
                AddEditItem((int)TvWishEntries.matchtype, mywish.matchtype, ref ctr);
                AddEditItem((int)TvWishEntries.exclude, mywish.exclude, ref ctr);
                AddEditItem((int)TvWishEntries.recordtype, mywish.recordtype, ref ctr);
                AddEditItem((int)TvWishEntries.action, mywish.action, ref ctr);
                AddEditItem((int)TvWishEntries.group, mywish.group, ref ctr);
                AddEditItem((int)TvWishEntries.channel, mywish.channel, ref ctr);
                AddEditItem((int)TvWishEntries.afterdays, mywish.afterdays, ref ctr);
                AddEditItem((int)TvWishEntries.beforedays, mywish.beforedays, ref ctr);
                AddEditItem((int)TvWishEntries.aftertime, mywish.aftertime, ref ctr);
                AddEditItem((int)TvWishEntries.beforetime, mywish.beforetime, ref ctr);
                AddEditItem((int)TvWishEntries.withinNextHours, mywish.withinNextHours, ref ctr);
                AddEditItem((int)TvWishEntries.preferredgroup, mywish.preferredgroup, ref ctr);
                AddEditItem((int)TvWishEntries.includerecordings, mywish.includeRecordings, ref ctr);
                AddEditItem((int)TvWishEntries.episodename, mywish.episodename, ref ctr);
                AddEditItem((int)TvWishEntries.episodepart, mywish.episodepart, ref ctr);
                AddEditItem((int)TvWishEntries.episodenumber, mywish.episodenumber, ref ctr);
                AddEditItem((int)TvWishEntries.seriesnumber, mywish.seriesnumber, ref ctr);
                AddEditItem((int)TvWishEntries.prerecord, mywish.prerecord, ref ctr);
                AddEditItem((int)TvWishEntries.postrecord, mywish.postrecord, ref ctr);
                AddEditItem((int)TvWishEntries.keepepisodes, mywish.keepepisodes, ref ctr);
                AddEditItem((int)TvWishEntries.keepuntil, mywish.keepuntil, ref ctr);
                AddEditItem((int)TvWishEntries.priority, mywish.priority, ref ctr);
                AddEditItem((int)TvWishEntries.recommendedcard, mywish.recommendedcard, ref ctr);
                AddEditItem((int)TvWishEntries.useFolderName, mywish.useFolderName, ref ctr);
                AddEditItem((int)TvWishEntries.skip, mywish.skip, ref ctr);
                AddEditItem((int)TvWishEntries.episodecriteria, mywish.episodecriteria, ref ctr);
                //modify for listview table changes       
            }
            catch
            {
                GUIListItem myGuiListItem = new GUIListItem(PluginGuiLocalizeStrings.Get(4302));   //Error in creating item list
                myfacade.Add(myGuiListItem);
                Log.Error("Error in creating item list");
            }
        }


        

        /// <summary>
        /// Gets a text input from the user using the virtual keyboard
        /// </summary>
        /// <param name="sString">String the user typed</param>
        /// <param name="password">Should the keyboard be presented as password</param>
        /// <param name="type">0=default, 1=sms, 2=web</param>
        /// <returns>True if user pressed ok, false if canceled</returns>
        public bool GetUserInputString(ref string sString, bool password)
        {
            VirtualKeyboard keyBoard = null;
            keyBoard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
            keyBoard.Reset();
            Log.Debug("keyBoard.MAX_CHARS=" + keyBoard.MAX_CHARS.ToString());
            keyBoard.IsSearchKeyboard = true;
            keyBoard.Text = sString;
            keyBoard.Password = password;
            keyBoard.DoModal(GetID); // show it...
            if (keyBoard.IsConfirmed)
            {
                sString = keyBoard.Text;
            }
            return keyBoard.IsConfirmed;
        }

        private void ItemSelectionChanged()
        {
            try
            {

                int selected_edit_index = myfacade.SelectedListItemIndex;

                Log.Debug("Selected item is " + selected_edit_index.ToString());
                Log.Debug("_listTranslator[selected_edit_index]=" + _listTranslator[selected_edit_index].ToString());
                string mystring;
                string keyboardstring="";
                TvWish mywish;
                List<string> menulist = new List<string>();
                GUIDialogMenu dlg;

                switch (_listTranslator[selected_edit_index])
                {
                    case (int)TvWishEntries.active:

                        if (myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).b_active == true)
                        {
                            mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                            mywish.b_active = false;
                            mywish.active = PluginGuiLocalizeStrings.Get(4001);//False
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                        }
                        else
                        {
                            mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                            mywish.b_active = true;
                            mywish.active = PluginGuiLocalizeStrings.Get(4000);
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                        }
                        //Log.Debug("check mywish.b_active=" + mywish.b_active.ToString());
                        break;

                    case (int)TvWishEntries.searchfor:
                        mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                        keyboardstring = mywish.searchfor;
                        if (GetUserInputString(ref keyboardstring, false)==true)
                        {
                            mywish.searchfor = keyboardstring;
                            if (mywish.name=="")
                                mywish.name = keyboardstring;
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                        }                        
                        break;

                    case (int)TvWishEntries.matchtype:
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2802)); //Match Type
                         dlg.ShowQuickNumbers = false;                       
                         for (int i=0;i<=6;i++)
                         {
                             dlg.Add(PluginGuiLocalizeStrings.Get(2600+i));
                         }
                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         Log.Debug("dlg.SelectedId=" + dlg.SelectedId.ToString());
                         if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.matchtype = PluginGuiLocalizeStrings.Get(2600 + dlg.SelectedId - 1);
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }                                 
                         break;

                    case (int)TvWishEntries.group:
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         if (menulist != null)
                             menulist.Clear();
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2803));  //Group
                         dlg.Add(PluginGuiLocalizeStrings.Get(4104)); //All Channels
                         menulist.Add(PluginGuiLocalizeStrings.Get(4104)); //All Channels
                         IList<ChannelGroup> allchannelgroups = ChannelGroup.ListAll();
                         foreach (ChannelGroup channelgroup in allchannelgroups)
                         {
                             if (channelgroup.GroupName != "All Channels")  //??? check with local language
                             {
                                 dlg.Add(channelgroup.GroupName);
                                 menulist.Add(channelgroup.GroupName);
                             }
                         }
                         IList<RadioChannelGroup> allradiochannelgroups = RadioChannelGroup.ListAll();
                         foreach (RadioChannelGroup radiochannelgroup in allradiochannelgroups)
                         {
                             if (radiochannelgroup.GroupName != "All Channels")
                             {
                                 dlg.Add(radiochannelgroup.GroupName.ToString());
                                 menulist.Add(radiochannelgroup.GroupName);
                             }
                         }

                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         if (dlg.SelectedLabel >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.group = menulist[dlg.SelectedLabel];
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.recordtype:
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2804)); //Record Type
                         for (int i = 0; i <= 5; i++)
                         {
                             dlg.Add(PluginGuiLocalizeStrings.Get(2650 + i));
                         }
                         
                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         Log.Debug("dlg.SelectedId=" + dlg.SelectedId.ToString());
                         if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.recordtype = PluginGuiLocalizeStrings.Get(2650 + dlg.SelectedId - 1);
                             if (dlg.SelectedId == 1) //If Only Once selected check other settings
                             {
                                 if (mywish.keepepisodes != PluginGuiLocalizeStrings.Get(4105)) //All
                                 {
                                     mywish.keepepisodes = PluginGuiLocalizeStrings.Get(4105); //All
                                     myTvWishes.MyMessageBox(4305, 4309); //Keep Episodes number had to be changed to Any
                                 }
                             }
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }

                         
                         break;

                    case (int)TvWishEntries.action:
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2805)); //Action
                         for (int i = 0; i <= 2; i++)  //do not use view
                         {
                             dlg.Add(PluginGuiLocalizeStrings.Get(2700 + i));
                         }                         
                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         Log.Debug("dlg.SelectedId=" + dlg.SelectedId.ToString());
                         if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.action = PluginGuiLocalizeStrings.Get(2700 + dlg.SelectedId - 1);
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.exclude:
                         mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                         keyboardstring = mywish.exclude;
                         if (GetUserInputString(ref keyboardstring, false) == true)
                         {
                             mywish.exclude = keyboardstring;
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.prerecord:

                         if (menulist != null)
                             menulist.Clear();
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2808));  //Prerecord
                         dlg.Add("0");
                         menulist.Add("0");
                         dlg.Add("5");
                         menulist.Add("5");
                         dlg.Add("8");
                         menulist.Add("8");
                         dlg.Add("10");
                         menulist.Add("10");
                         dlg.Add("15");
                         menulist.Add("15");
                         dlg.Add("30");
                         menulist.Add("30");
                         dlg.Add(PluginGuiLocalizeStrings.Get(4102));        //Custom
                         menulist.Add(PluginGuiLocalizeStrings.Get(4102));
                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         Log.Debug("dlg.SelectedId=" + dlg.SelectedId.ToString());

                         if (dlg.SelectedId == 7) //6+1
                         {
                             keyboardstring = "11";
                             if (GetUserInputString(ref keyboardstring, false) == true)
                             {
                                 try
                                 {
                                     int k = Convert.ToInt32(keyboardstring);
                                     Log.Debug("k=" + k.ToString());
                                 }
                                 catch //do nothing and use default
                                 {
                                     keyboardstring = "5";
                                 }

                                 mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                 mywish.prerecord = keyboardstring;
                                 myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                             }
                         }
                         else if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.prerecord = menulist[dlg.SelectedLabel];
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.postrecord:

                         if (menulist != null)
                             menulist.Clear();
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2809));  //Postrecord
                         dlg.Add("0");
                         menulist.Add("0");
                         dlg.Add("5");
                         menulist.Add("5");
                         dlg.Add("8");
                         menulist.Add("8");
                         dlg.Add("10");
                         menulist.Add("10");
                         dlg.Add("15");
                         menulist.Add("15");
                         dlg.Add("30");
                         menulist.Add("30");
                         dlg.Add(PluginGuiLocalizeStrings.Get(4102));        //Custom
                         menulist.Add(PluginGuiLocalizeStrings.Get(4102));
                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         Log.Debug("dlg.SelectedId=" + dlg.SelectedId.ToString());

                         if (dlg.SelectedId == 7) //6+1
                         {
                             keyboardstring = "11";
                             if (GetUserInputString(ref keyboardstring, false) == true)
                             {
                                 try
                                 {
                                     int k = Convert.ToInt32(keyboardstring);
                                     Log.Debug("k=" + k.ToString());
                                 }
                                 catch //do nothing and use default
                                 {
                                     keyboardstring = "5";
                                 }

                                 mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                 mywish.postrecord = keyboardstring;
                                 myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                             }
                         }
                         else if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.postrecord = menulist[dlg.SelectedLabel];
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;                 

                    case (int)TvWishEntries.episodename:
                         mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                         keyboardstring = mywish.episodename;    
                         if (GetUserInputString(ref keyboardstring, false) == true)
                         {
                             mywish.episodename = keyboardstring;
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.episodepart:
                         mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                         keyboardstring = mywish.episodepart;
                         if (GetUserInputString(ref keyboardstring, false) == true)
                         {
                             mywish.episodepart = keyboardstring;
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.episodenumber:
                         mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                         keyboardstring = mywish.episodenumber;
                         if (GetUserInputString(ref keyboardstring, false) == true)
                         {                            
                             mywish.episodenumber = keyboardstring;
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.seriesnumber:
                         mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                         keyboardstring = mywish.seriesnumber;
                         if (GetUserInputString(ref keyboardstring, false) == true)
                         {
                             mywish.seriesnumber = keyboardstring;
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    
                    case (int)TvWishEntries.keepepisodes:

                         if (menulist != null)
                             menulist.Clear();
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2814));  //Keep Episodes
                         dlg.Add(PluginGuiLocalizeStrings.Get(4105));  //All
                         menulist.Add(PluginGuiLocalizeStrings.Get(4105));  //All                        
                         dlg.Add("3");
                         menulist.Add("3");
                         dlg.Add("5");
                         menulist.Add("5");
                         dlg.Add("8");
                         menulist.Add("8");
                         dlg.Add("10");
                         menulist.Add("10");
                         dlg.Add("15");
                         menulist.Add("15");
                         dlg.Add(PluginGuiLocalizeStrings.Get(4102));        //Custom
                         menulist.Add(PluginGuiLocalizeStrings.Get(4102));
                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         Log.Debug("dlg.SelectedId=" + dlg.SelectedId.ToString());

                         if (dlg.SelectedId == 7) //5+1+1
                         {
                             keyboardstring = "5";
                             if (GetUserInputString(ref keyboardstring, false) == true)
                             {
                                 try
                                 {
                                     int k = Convert.ToInt32(keyboardstring);
                                     Log.Debug("k=" + k.ToString());
                                 }
                                 catch //do nothing and use default
                                 {
                                     keyboardstring = "5";
                                 }

                                 mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                 mywish.keepepisodes = keyboardstring;
                                 myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                             }
                         }
                         else if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.keepepisodes = menulist[dlg.SelectedLabel];
                             if (dlg.SelectedId != 1) //If  Epsisode Management is not selected check other settings
                             {
                                 if (mywish.useFolderName != PluginGuiLocalizeStrings.Get(2850)) //Episode
                                 {
                                     mywish.useFolderName = PluginGuiLocalizeStrings.Get(2850); //Episode
                                     myTvWishes.MyMessageBox(4305, 4307); //Use Folder name had to be changed to Episode
                                 }

                                 if (mywish.recordtype == PluginGuiLocalizeStrings.Get(2650)) //Only Once
                                 {
                                     mywish.recordtype = PluginGuiLocalizeStrings.Get(2651); //All
                                     myTvWishes.MyMessageBox(4305, 4308); //Record Type had to be changed to All
                                 }
                             }
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.keepuntil:
                         mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);

                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2815));  //Keep Until
                        
                         for (int i = 2900; i <= 2906; i++)
                         {
                             dlg.Add(PluginGuiLocalizeStrings.Get(i));
                             menulist.Add(PluginGuiLocalizeStrings.Get(i));
                         }
                         
                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         if (dlg.SelectedId == 1) //always
                         {
                             //myfacade[selected_edit_index].Label = String.Format(PluginGuiLocalizeStrings.Get(2515), menulist[dlg.SelectedLabel]); //Keep {0}
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.keepuntil = PluginGuiLocalizeStrings.Get(2900); //Always
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         else if (dlg.SelectedId == 2) //days after recording
                         {
                             if (menulist != null)
                                 menulist.Clear();
                             GUIDialogMenu dlg2 = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                             dlg2.ShowQuickNumbers = false;
                             dlg2.SetHeading(PluginGuiLocalizeStrings.Get(2901));  // Days After Recording
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2920), "3"); //{0} Days after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2920), "5"); //{0} Days after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2920), "7"); //{0} Days after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2920), "10"); //{0} Days after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2920), "15"); //{0} Days after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2920), "30"); //{0} Days after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             dlg2.Add(PluginGuiLocalizeStrings.Get(4102));        //Custom
                             menulist.Add(PluginGuiLocalizeStrings.Get(4102));
                             dlg2.DoModal(GUIWindowManager.ActiveWindow);
                             Log.Debug("dlg.SelectedId=" + dlg2.SelectedId.ToString());


                             if (dlg2.SelectedId == 7) //6+1  //Custom
                             {
                                 keyboardstring = "15";
                                 if (GetUserInputString(ref keyboardstring, false) == true)
                                 {
                                     try
                                     {
                                         int k = Convert.ToInt32(keyboardstring);
                                         Log.Debug("k=" + k.ToString());
                                     }
                                     catch //do nothing and use default
                                     {
                                         keyboardstring = "15";
                                     }

                                     //string listString = String.Format(PluginGuiLocalizeStrings.Get(2920), keyboardstring);

                                     mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                     mywish.keepuntil = String.Format(PluginGuiLocalizeStrings.Get(2920), keyboardstring); //{0} Days after recording
                                     myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                 }
                             }
                             else if (dlg2.SelectedId >= 0)
                             {
                                 mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                 mywish.keepuntil = menulist[dlg2.SelectedLabel];
                                 myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                             }

                         }//end days                       
                         else if (dlg.SelectedId == 3) //Weeks after recording
                         {
                             if (menulist != null)
                                 menulist.Clear();
                             GUIDialogMenu dlg2 = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                             dlg2.ShowQuickNumbers = false;
                             dlg2.SetHeading(PluginGuiLocalizeStrings.Get(2902));  // Months After Recording
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2921), "1"); //{0} Weeks after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2921), "2"); //{0} Weeks after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2921), "3"); //{0} Weeks after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2921), "4"); //{0} Weeks after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2921), "8"); //{0} Weeks after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2921), "12"); //{0} Weeks after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             dlg2.Add(PluginGuiLocalizeStrings.Get(4102));        //Custom
                             menulist.Add(PluginGuiLocalizeStrings.Get(4102));
                             dlg2.DoModal(GUIWindowManager.ActiveWindow);
                             Log.Debug("dlg.SelectedId=" + dlg2.SelectedId.ToString());


                             if (dlg2.SelectedId == 7) //6+1  //Custom
                             {
                                 keyboardstring = "15";
                                 if (GetUserInputString(ref keyboardstring, false) == true)
                                 {
                                     try
                                     {
                                         int k = Convert.ToInt32(keyboardstring);
                                         Log.Debug("k=" + k.ToString());
                                     }
                                     catch //do nothing and use default
                                     {
                                         keyboardstring = "15";
                                     }

                                     mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                     mywish.keepuntil = String.Format(PluginGuiLocalizeStrings.Get(2921), keyboardstring); //{0} Weeks after recording
                                     myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                 }
                             }
                             else if (dlg2.SelectedId >= 0)
                             {
                                  mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                 mywish.keepuntil = menulist[dlg2.SelectedLabel];
                                 myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                             }

                         }//end weeks
                         else if (dlg.SelectedId == 4) //Months after recording
                         {
                             if (menulist != null)
                                 menulist.Clear();
                             GUIDialogMenu dlg2 = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                             dlg2.ShowQuickNumbers = false;
                             dlg2.SetHeading(PluginGuiLocalizeStrings.Get(2903));  // Years After Recording
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2922), "1"); //{0} Months after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2922), "2"); //{0} Months after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2922), "5"); //{0} Months after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2922), "7"); //{0} Months after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2922), "9"); //{0} Months after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             mystring = String.Format(PluginGuiLocalizeStrings.Get(2922), "11"); //{0} Months after recording
                             dlg2.Add(mystring);
                             menulist.Add(mystring);
                             dlg2.Add(PluginGuiLocalizeStrings.Get(4102));        //Custom
                             menulist.Add(PluginGuiLocalizeStrings.Get(4102));
                             dlg2.DoModal(GUIWindowManager.ActiveWindow);
                             Log.Debug("dlg.SelectedId=" + dlg2.SelectedId.ToString());


                             if (dlg2.SelectedId == 7) //6+1  //Custom
                             {
                                 keyboardstring = "15";
                                 if (GetUserInputString(ref keyboardstring, false) == true)
                                 {
                                     try
                                     {
                                         int k = Convert.ToInt32(keyboardstring);
                                         Log.Debug("k=" + k.ToString());
                                     }
                                     catch //do nothing and use default
                                     {
                                         keyboardstring = "15";
                                     }

                                     mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                     mywish.keepuntil = String.Format(PluginGuiLocalizeStrings.Get(2922), keyboardstring); //{0} Months after recording
                                     myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                 }
                             }
                             else if (dlg2.SelectedId >= 0)
                             {
                                 mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                 mywish.keepuntil = menulist[dlg2.SelectedLabel];
                                 myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                             }

                         }//end months
                         else if (dlg.SelectedId == 5) //date
                         {
                             keyboardstring = PluginGuiLocalizeStrings.Get(2923); //YYYY-MM-DD
                             if (GetUserInputString(ref keyboardstring, false) == true)
                             {
                                 // check for timedate format
                                 mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                 mywish.keepuntil = keyboardstring;
                                 myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                             }

                         }
                         else if (dlg.SelectedId == 6) //until watched
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.keepuntil = PluginGuiLocalizeStrings.Get(2905); //watched
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         else if (dlg.SelectedId == 7) //until space needed
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.keepuntil = PluginGuiLocalizeStrings.Get(2906); //Space
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }

                         //final check:
                         Log.Debug("Before final check mywish.keepuntil=" + mywish.keepuntil);
                         string checkedstring=PluginGuiLocalizeStrings.Get(4101);//defaultvalue
                         string errormessage="";
                         int days=0;
                         int keepMethod = 0;
                         DateTime mydate=DateTime.Now;
                         myTvWishes.ReverseTvWishLanguageTranslation(ref mywish);
                         myTvWishes.ConvertString2KeepUntil(mywish.keepuntil, ref checkedstring, ref keepMethod, ref days, ref mydate, ref errormessage);
                         myTvWishes.TvWishLanguageTranslation(ref mywish);
                         Log.Debug("After final check mywish.keepuntil=" + mywish.keepuntil);
                      
                         break;

                    
                    case (int)TvWishEntries.recommendedcard:
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         if (menulist != null)
                             menulist.Clear();
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2816));  //Recommended Card
                         dlg.Add(PluginGuiLocalizeStrings.Get(4100));    //Any
                         menulist.Add(PluginGuiLocalizeStrings.Get(4100));    //Any
                         IList<Card> allcards = Card.ListAll();
                         foreach (Card mycard in allcards)
                         {
                             
                              dlg.Add(mycard.IdCard.ToString());
                              menulist.Add(mycard.IdCard.ToString());                    
                         }
                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.recommendedcard = menulist[dlg.SelectedLabel];
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.priority:
                         if (menulist != null)
                             menulist.Clear();
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2817));  //Priority
                         for (int i = 0; i <= 9; i++)
                         {
                             dlg.Add(i.ToString());
                             menulist.Add(i.ToString());
                         }
                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         Log.Debug("dlg.SelectedId=" + dlg.SelectedId.ToString());
                         if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.priority = menulist[dlg.SelectedLabel];
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.aftertime:
                         if (menulist != null)
                             menulist.Clear();
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2818));  //After Time
                         dlg.Add(PluginGuiLocalizeStrings.Get(4100));        //Any
                         menulist.Add(PluginGuiLocalizeStrings.Get(4100));
                         dlg.Add("20:00");
                         menulist.Add("20:00");
                         dlg.Add("23:00");
                         menulist.Add("23:00");
                         dlg.Add("02:00");
                         menulist.Add("02:00");
                         dlg.Add("08:00");
                         menulist.Add("08:00");
                         dlg.Add("12:00");
                         menulist.Add("12:00");
                         dlg.Add(PluginGuiLocalizeStrings.Get(4102));        //Custom
                         menulist.Add(PluginGuiLocalizeStrings.Get(4102));

                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         Log.Debug("dlg.SelectedId=" + dlg.SelectedId.ToString());

                         if (dlg.SelectedId == 7) //5+1+1
                         {
                             keyboardstring = "20:15";
                             if (GetUserInputString(ref keyboardstring, false) == true)
                             {
                                 try
                                 {
                                     DateTime k = DateTime.ParseExact(keyboardstring, "HH:mm", CultureInfo.InvariantCulture);
                                     Log.Debug("k=" + k.ToString());
                                 }
                                 catch //do nothing and use default
                                 {
                                     keyboardstring = PluginGuiLocalizeStrings.Get(4100);//"Any"
                                 }

                                 mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                 mywish.aftertime = keyboardstring;
                                 myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                             }
                         }
                         else if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.aftertime = menulist[dlg.SelectedLabel];
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.beforetime:

                         if (menulist != null)
                             menulist.Clear();
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2819));  //Before Time
                         dlg.Add(PluginGuiLocalizeStrings.Get(4100));        //Any
                         menulist.Add(PluginGuiLocalizeStrings.Get(4100));
                         dlg.Add("20:00");
                         menulist.Add("20:00");
                         dlg.Add("23:00");
                         menulist.Add("23:00");
                         dlg.Add("02:00");
                         menulist.Add("02:00");
                         dlg.Add("08:00");
                         menulist.Add("08:00");
                         dlg.Add("12:00");
                         menulist.Add("12:00");
                         dlg.Add(PluginGuiLocalizeStrings.Get(4102));        //Custom
                         menulist.Add(PluginGuiLocalizeStrings.Get(4102));

                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         Log.Debug("dlg.SelectedId=" + dlg.SelectedId.ToString());

                         if (dlg.SelectedId == 7) //5+1+1
                         {
                             keyboardstring = "20:15";
                             if (GetUserInputString(ref keyboardstring, false) == true)
                             {
                                 try
                                 {
                                     DateTime k = DateTime.ParseExact(keyboardstring, "HH:mm", CultureInfo.InvariantCulture);
                                     Log.Debug("k=" + k.ToString());
                                 }
                                 catch //do nothing and use default
                                 {
                                     keyboardstring = PluginGuiLocalizeStrings.Get(4100);//"Any"
                                 }

                                 mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                 mywish.beforetime = keyboardstring;
                                 myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                             }
                         }
                         else if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.beforetime = menulist[dlg.SelectedLabel];
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.afterdays:
                         if (menulist != null)
                             menulist.Clear();
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2820)); //After Day
                         dlg.Add(PluginGuiLocalizeStrings.Get(4100));        //Any
                         menulist.Add(PluginGuiLocalizeStrings.Get(4100));  //"Any"
                         for (int i = 0; i <= 6; i++)
                         {
                             dlg.Add(PluginGuiLocalizeStrings.Get(2750 + i));
                             menulist.Add(PluginGuiLocalizeStrings.Get(2750 + i));
                         }
                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         Log.Debug("dlg.SelectedId=" + dlg.SelectedId.ToString());
                         if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.afterdays = menulist[dlg.SelectedLabel];
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.beforedays:
                         if (menulist != null)
                             menulist.Clear();
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2821)); //Before Day
                         dlg.Add(PluginGuiLocalizeStrings.Get(4100));        //Any
                         menulist.Add(PluginGuiLocalizeStrings.Get(4100));  //"Any"
                         for (int i = 0; i <= 6; i++)
                         {
                             dlg.Add(PluginGuiLocalizeStrings.Get(2750 + i));
                             menulist.Add(PluginGuiLocalizeStrings.Get(2750 + i));
                         }
                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         Log.Debug("dlg.SelectedId=" + dlg.SelectedId.ToString());
                         if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.beforedays = menulist[dlg.SelectedLabel];
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.channel:
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         if (menulist != null)
                             menulist.Clear();
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2822));  //Channel
                         dlg.Add(PluginGuiLocalizeStrings.Get(4100));  //"Any"
                         menulist.Add(PluginGuiLocalizeStrings.Get(4100));  //"Any"

                         //build new channel list based on new group filter
                         mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                         foreach (ChannelGroup channelgroup in ChannelGroup.ListAll())
                         {
                             if (channelgroup.GroupName == mywish.group)  //groupname must match
                             {
                                 IList<GroupMap> allgroupmaps = channelgroup.ReferringGroupMap();

                                 foreach (GroupMap onegroupmap in allgroupmaps)
                                 {
                                     string channelname = onegroupmap.ReferencedChannel().DisplayName;
                                     dlg.Add(channelname);
                                     menulist.Add(channelname);

                                 }
                             }
                         }
                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.channel = menulist[dlg.SelectedLabel];
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.skip:
                         Log.Debug("case (int)TvWishEntries.skip:");
                         if (myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).b_skip == true)
                         {
                             Log.Debug("true");
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.b_skip = false;
                             mywish.skip = PluginGuiLocalizeStrings.Get(4001);
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                             Log.Debug("myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).b_skip.ToString()="+myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).b_skip.ToString());
                         }
                         else
                         {
                             Log.Debug("false");
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.b_skip = true;
                             mywish.skip = PluginGuiLocalizeStrings.Get(4000);
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                             Log.Debug("myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).b_skip.ToString()=" + myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).b_skip.ToString());
                         }
                         break;

                    case (int)TvWishEntries.name:
                         mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                         keyboardstring = mywish.name;
                         if (GetUserInputString(ref keyboardstring, false) == true)
                         {
                             mywish.name = keyboardstring;
                             if (mywish.searchfor == "")
                                 mywish.searchfor = keyboardstring;
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.useFolderName:
                         Log.Debug("case (int)TvWishEntries.useFolderName:");

                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2825)); //Record Type
                         for (int i = 0; i <= 3; i++)
                         {
                             dlg.Add(PluginGuiLocalizeStrings.Get(2850 + i));
                         }
                         
                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         Log.Debug("dlg.SelectedId=" + dlg.SelectedId.ToString());
                         if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.useFolderName = PluginGuiLocalizeStrings.Get(2850 + dlg.SelectedId - 1);
                             if (dlg.SelectedId != 1) //If  Epsisode Management is not selected check other settings
                             {
                                 if (mywish.keepepisodes != PluginGuiLocalizeStrings.Get(4105)) //All
                                 {
                                     mywish.keepepisodes = PluginGuiLocalizeStrings.Get(4105); //All
                                     myTvWishes.MyMessageBox(4305, 4309); //Keep Episodes number had to be changed to All
                                 }
                             }
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;


                    case (int)TvWishEntries.withinNextHours:

                         if (menulist != null)
                             menulist.Clear();
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2826));  //Within Next Hours
                         dlg.Add(PluginGuiLocalizeStrings.Get(4100));  //Any
                         menulist.Add(PluginGuiLocalizeStrings.Get(4100));  //Any                        
                         dlg.Add("1");
                         menulist.Add("1");
                         dlg.Add("2");
                         menulist.Add("2");
                         dlg.Add("4");
                         menulist.Add("4");
                         dlg.Add("8");
                         menulist.Add("8");
                         dlg.Add("16");
                         menulist.Add("16");
                         dlg.Add(PluginGuiLocalizeStrings.Get(4102));        //Custom
                         menulist.Add(PluginGuiLocalizeStrings.Get(4102));
                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         Log.Debug("dlg.SelectedId=" + dlg.SelectedId.ToString());

                         if (dlg.SelectedId == 7) //5+1+1
                         {
                             keyboardstring = "5";
                             if (GetUserInputString(ref keyboardstring, false) == true)
                             {
                                 try
                                 {
                                     int k = Convert.ToInt32(keyboardstring);
                                     Log.Debug("k=" + k.ToString());
                                 }
                                 catch //do nothing and use default
                                 { 
                                     keyboardstring = "5";
                                 }

                                 mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                 mywish.withinNextHours = keyboardstring;
                                 myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                             }
                         }
                         else if (dlg.SelectedId >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.withinNextHours = menulist[dlg.SelectedLabel];
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.episodecriteria:
                         
                         mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                         Log.Debug("[TVWishListMP GUI_List]:buttonFilter");
                         int _filter=0;
                         while (_filter != -1)
                         {
                             dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                             dlg.ShowQuickNumbers = false;
                             dlg.SetHeading(String.Format(PluginGuiLocalizeStrings.Get(2833))); //"Episode Criteria Menu");

                             if (mywish.b_episodecriteria_d == true)
                             {
                                 dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(2950))); //"Description");
                             }
                             else
                             {
                                 dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(2951))); //"No Description");
                             }

                             if (mywish.b_episodecriteria_n == true)
                             {
                                 dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(2952))); //"Episode Names");
                             }
                             else
                             {
                                 dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(2953))); //"No Episode Names");
                             }

                             if (mywish.b_episodecriteria_c == true)
                             {
                                 dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(2954))); //"Episode Numbers");
                             }
                             else
                             {
                                 dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(2955))); //"No Episode Numbers");
                             }
                             dlg.Add("   ");

                             dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(2970))); //"All");
                             dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(2971))); //"None");


                             dlg.DoModal(GUIWindowManager.ActiveWindow);

                             //evaluate selected
                             _filter = dlg.SelectedId;

                             if (_filter == 1)
                                 mywish.b_episodecriteria_d = !mywish.b_episodecriteria_d;

                             else if (_filter == 2)
                                 mywish.b_episodecriteria_n = !mywish.b_episodecriteria_n;

                             else if (_filter == 3)
                                 mywish.b_episodecriteria_c = !mywish.b_episodecriteria_c;

                             else if (_filter == 5) //All
                             {
                                 mywish.b_episodecriteria_d = true;
                                 mywish.b_episodecriteria_n = true;
                                 mywish.b_episodecriteria_c = true;
                             }

                             else if (_filter == 6) //None
                             {
                                 mywish.b_episodecriteria_d = false;
                                 mywish.b_episodecriteria_n = false;
                                 mywish.b_episodecriteria_c = false;
                             }

                             
                             //StatusLabel( "Filter=" + _filter.ToString());

                         }
                         string evaluationstring = "";
                         if (mywish.b_episodecriteria_d == true)
                         {
                             evaluationstring += PluginGuiLocalizeStrings.Get(2960) + "+";  //"Description"
                         }
                         if (mywish.b_episodecriteria_n == true)
                         {
                             evaluationstring += PluginGuiLocalizeStrings.Get(2961) + "+";  //"Episode Names"
                         }
                         if (mywish.b_episodecriteria_c == true)
                         {
                             evaluationstring += PluginGuiLocalizeStrings.Get(2962) + "+"; //"Episode Numbers"
                         }

                         if (evaluationstring == "")
                         {
                             evaluationstring = PluginGuiLocalizeStrings.Get(3264); //None
                         }
                         else
                         {
                             evaluationstring = evaluationstring.Substring(0, evaluationstring.Length - 1);
                         }
                         
                         mywish.episodecriteria = evaluationstring;
                         myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                         break;

                    case (int)TvWishEntries.preferredgroup:
                         dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                         dlg.ShowQuickNumbers = false;
                         if (menulist != null)
                             menulist.Clear();
                         dlg.SetHeading(PluginGuiLocalizeStrings.Get(2834));  //PreferredGroup
                         dlg.Add(PluginGuiLocalizeStrings.Get(4104)); //All Channels
                         menulist.Add(PluginGuiLocalizeStrings.Get(4104)); //All Channels
                         foreach (ChannelGroup channelgroup in ChannelGroup.ListAll())
                         {
                             if (channelgroup.GroupName != "All Channels")  //??? check with local language
                             {
                                 dlg.Add(channelgroup.GroupName);
                                 menulist.Add(channelgroup.GroupName);
                             }
                         }
                         foreach (RadioChannelGroup radiochannelgroup in RadioChannelGroup.ListAll())
                         {
                             if (radiochannelgroup.GroupName != "All Channels")
                             {
                                 dlg.Add(radiochannelgroup.GroupName.ToString());
                                 menulist.Add(radiochannelgroup.GroupName);
                             }
                         }

                         dlg.DoModal(GUIWindowManager.ActiveWindow);
                         if (dlg.SelectedLabel >= 0)
                         {
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.preferredgroup = menulist[dlg.SelectedLabel];
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                         }
                         break;

                    case (int)TvWishEntries.includerecordings:
                         if (myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).b_includeRecordings == true)
                         {
                             //Log.Debug("true");
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.b_includeRecordings = false;
                             mywish.includeRecordings = PluginGuiLocalizeStrings.Get(4001);
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                            //Log.Debug("myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).b_includeRecordings.ToString()=" + myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).b_includeRecordings.ToString());
                         }
                         else
                         {
                             //Log.Debug("false");
                             mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                             mywish.b_includeRecordings = true;
                             mywish.includeRecordings = PluginGuiLocalizeStrings.Get(4000);
                             myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                             Log.Debug("myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).b_includeRecordings.ToString()=" + myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).b_includeRecordings.ToString());
                         }
                         break;

                    //modify for listview table changes


                }//end switch

                UpdateListItems();
                
                //go back to last position
                for (int i=0; i< selected_edit_index;i++)
                {
                    Action myaction1 = new Action(Action.ActionType.ACTION_MOVE_DOWN, 0, 0);
                    GUIWindowManager.OnAction(myaction1);                   
                }               
            }

            catch (Exception exc)
            {

                Log.Error("[TVWishList GUI_Edit]:ItemSelectionChanged: ****** Exception " + exc.Message);

            }
        }
    }
}
