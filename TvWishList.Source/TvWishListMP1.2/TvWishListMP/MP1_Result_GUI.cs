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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using Log = TvLibrary.Log.huha.Log;

using Action = MediaPortal.GUI.Library.Action;
//using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;
using TvWishList;
using MediaPortal.Configuration;

using TvControl;
using TvDatabase;
using Gentle.Framework;
 
//using GUIKeyboard = MediaPortal.GUI.Library.huha.GUIKeyboard;

namespace MediaPortal.Plugins.TvWishList
{
    public partial class Result_GUI : GUIWindow 
    {
        [SkinControlAttribute(3)]
        protected GUIButtonControl buttonUpdate = null;
        [SkinControlAttribute(4)]
        protected GUISortButtonControl buttonSort = null;
        [SkinControlAttribute(5)]
        protected GUIButtonControl buttonFilter = null;

        [SkinControlAttribute(50)]
        protected GUIListControl myfacade = null;

        [SkinControlAttribute(70)]
        protected GUITextScrollUpControl mytextbox = null;

        

        int _guimainwindowid = 70943675;
        int _guilistwindowid = 70943676;
        int _guieditwindowid = 70943677;

        //Filter criteria
        public bool _Email = true;
        public bool _Deleted = true;
        public bool _Conflicts = true;
        public bool _Scheduled = false;
        public bool _Recorded = false;
        public bool _View = true;
        public bool _Unknown = false;
        int _filter = 0;
        public int _sort = 1;
        public bool _sortreverse = false;

        public Result_GUI()
        {
            Log.Debug("[TVWishListMP GUI_List]:GUI_List()");
            GetID = _guilistwindowid;
        }

        public override bool SupportsDelayedLoad
        {
            get { return false; }
        }

        public override bool Init()
     	{
            Log.Debug("[TVWishListMP GUI_List]:Init");

            _instance = this;
            
            PluginGuiLocalizeStrings.LoadMPlanguage();
            bool bResult = Load(GUIGraphicsContext.Skin + @"\TvWishListMP_2.xml");
            GetID = _guilistwindowid;

            /*
            _TextBoxFormat_16to9_EmailFormat = PluginGuiLocalizeStrings.Get(3900);
            _TextBoxFormat_4to3_EmailFormat = PluginGuiLocalizeStrings.Get(3902);
            _TextBoxFormat_16to9_ViewOnlyFormat = PluginGuiLocalizeStrings.Get(3901);
            _TextBoxFormat_4to3_ViewOnlyFormat = PluginGuiLocalizeStrings.Get(3903);
            */

            
            _TextBoxFormat_16to9_EmailFormat = PluginGuiLocalizeStrings.Get(3900);
            _TextBoxFormat_16to9_EmailFormat = _TextBoxFormat_16to9_EmailFormat.Replace(@"\n", "\n");
            _TextBoxFormat_16to9_EmailFormat = _TextBoxFormat_16to9_EmailFormat.Replace("<br>", "\n");
            _TextBoxFormat_16to9_EmailFormat = _TextBoxFormat_16to9_EmailFormat.Replace("<BR>", "\n");

            _TextBoxFormat_4to3_EmailFormat = PluginGuiLocalizeStrings.Get(3902);
            _TextBoxFormat_4to3_EmailFormat = _TextBoxFormat_4to3_EmailFormat.Replace(@"\n", "\n");
            _TextBoxFormat_4to3_EmailFormat = _TextBoxFormat_4to3_EmailFormat.Replace("<br>", "\n");
            _TextBoxFormat_4to3_EmailFormat = _TextBoxFormat_4to3_EmailFormat.Replace("<BR>", "\n");

            _TextBoxFormat_16to9_ViewOnlyFormat = PluginGuiLocalizeStrings.Get(3901);
            _TextBoxFormat_16to9_ViewOnlyFormat = _TextBoxFormat_16to9_ViewOnlyFormat.Replace(@"\n", "\n");
            _TextBoxFormat_16to9_ViewOnlyFormat = _TextBoxFormat_16to9_ViewOnlyFormat.Replace("<br>", "\n");
            _TextBoxFormat_16to9_ViewOnlyFormat = _TextBoxFormat_16to9_ViewOnlyFormat.Replace("<BR>", "\n");

            _TextBoxFormat_4to3_ViewOnlyFormat = PluginGuiLocalizeStrings.Get(3903);
            _TextBoxFormat_4to3_ViewOnlyFormat = _TextBoxFormat_4to3_ViewOnlyFormat.Replace(@"\n", "\n");
            _TextBoxFormat_4to3_ViewOnlyFormat = _TextBoxFormat_4to3_ViewOnlyFormat.Replace("<br>", "\n");
            _TextBoxFormat_4to3_ViewOnlyFormat = _TextBoxFormat_4to3_ViewOnlyFormat.Replace("<BR>", "\n");

            Log.Debug("[TVWishListMP GUI_List]:Init  xmlfile=" + _xmlfile);

            
            return bResult;
     	}

        


        protected override void OnPageLoad()
        {
            Log.Debug("[TVWishListMP GUI_List]:OnPageLoad xmlfile=" + _xmlfile);

            if ((this.PreviousWindowId != _guimainwindowid)&&(this.PreviousWindowId != _guieditwindowid))
            {
                //go back to main window
                Main_GUI mymainwindow = (Main_GUI)GUIWindowManager.GetWindow(_guimainwindowid);
                mymainwindow.TvserverdatabaseLoadSettings();
                GUIWindowManager.ActivateWindow(_guimainwindowid);
            }

            
            

            Log.Debug("[TVWishListMP GUI_List]:OnPageLoad  mymessages.filename=" + mymessages.filename);

            

            
            UpdateControls();
            UpdateListItems();
            //GUIListControl.FocusItemControl(_guilistwindowid, 50, 0);  green color
            buttonSort.SortChanged += new SortEventHandler(ButtonSortChanged);

            

            //GUIControl.FocusControl(_guilistwindowid, 50);
            Action myaction = new Action(Action.ActionType.ACTION_MOUSE_MOVE, 0, 0);
            GUIWindowManager.OnAction(myaction);

            base.OnPageLoad();
        }

        protected override void OnPageDestroy(int newWindowId)
        {
            buttonSort.SortChanged -= new SortEventHandler(ButtonSortChanged);
            Log.Debug("mp list window at page destroy: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
            Log.Debug("newWindowId=" + newWindowId.ToString());
            //save data if going to a new page
            if ((newWindowId != _guieditwindowid) && (newWindowId != _guimainwindowid))
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
                Log.Debug("14 mymainwindow.LOCKED=" + mymainwindow.LOCKED.ToString());
            }

            myfacade.Clear();
            //delete filter for single TvWish entry - by default search for all tvwish messages
            if (newWindowId != _guilistwindowid)
            {
                mymessages.FilterName = "";
            }
            base.OnPageDestroy(newWindowId);
        }

        public override bool OnMessage(GUIMessage message)
        {
            //Log.Info("[TVWishListMP GUI_List]:OnMessage" + message.ToString());
            //StatusLabel( "");
            switch (message.Message)
            {
                

                case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
                    

                    if (myfacade.SelectedListItemIndex >= 0)
                    {
                        try
                        {
                            

                            string _TextBoxFormat = "";
                            if (myTvWishes.ViewOnlyMode == false)
                            {
                                if (_UserEmailFormat != "")
                                {
                                    _TextBoxFormat = _UserEmailFormat;
                                }
                                else if ((2 * mytextbox.Height) > mytextbox.Width)
                                {
                                    _TextBoxFormat = _TextBoxFormat_16to9_EmailFormat;
                                }
                                else
                                {
                                    _TextBoxFormat = _TextBoxFormat_4to3_EmailFormat;
                                }
                            }
                            else
                            {
                                if (_UserViewOnlyFormat != "")
                                {
                                    _TextBoxFormat = _UserViewOnlyFormat;
                                }
                                else if ((2 *mytextbox.Height) > mytextbox.Width )
                                {
                                    
                                    _TextBoxFormat = _TextBoxFormat_16to9_ViewOnlyFormat;
                                }
                                else
                                {
                                    _TextBoxFormat = _TextBoxFormat_4to3_ViewOnlyFormat;
                                }
                            }

                            string messagetext = "";
                            if ((myfacade.SelectedListItemIndex >= 0) && (mymessages.ListAllTvMessagesFiltered().Count > 0))
                            {
                                xmlmessage mymessage = mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex);

                                messagetext = mymessages.FormatMessage(mymessage, _TextBoxFormat);
                                //string.Format(_TextBoxFormat, mymessages.TvMessagesFiltered[_focused].title, mymessages.TvMessagesFiltered[_focused].description, mymessages.TvMessages[_focused].type, mymessages.TvMessagesFiltered[_focused].start, mymessages.TvMessagesFiltered[_focused].end, mymessages.TvMessagesFiltered[_focused].channel);
                                GUIPropertyManager.SetProperty("#textbox.label", messagetext);

                                GUIPropertyManager.SetProperty("#title.label", mymessage.title);
                                GUIPropertyManager.SetProperty("#episodepart.label",mymessage.EpisodePart);
                                GUIPropertyManager.SetProperty("#episodename.label", mymessage.EpisodeName);
                                GUIPropertyManager.SetProperty("#channel.label", mymessage.channel);
                                GUIPropertyManager.SetProperty("#start.label", mymessages.FormatDateTime(mymessage.start));
                                //Log.Debug("#start.label="+mymessages.FormatDateTime(mymessage.start));

                                //mymessages.FormatDateTime(mymessages.GetTvMessageFilteredAtIndex(_focusedMessage).start);


                                GUIPropertyManager.SetProperty("#end.label", mymessages.FormatDateTime(mymessage.end));
                                //Log.Debug("#end.label=" + mymessages.FormatDateTime(mymessage.end));



                                TimeSpan duration = mymessage.end - mymessage.start;
                                //string durationFormat = String.Format(@"{0:hh\:mm}", duration);
                                //string durationFormat = duration.Hours+":"+duration.Minutes;
                                //string durationFormat = String.Format("{0:HH}:{1:mm}", duration.Hours, duration.Minutes);
                                string durationFormat = string.Empty;
                                if (duration.Hours.ToString().Length == 1)
                                {
                                    durationFormat = "0" + duration.Hours.ToString() + ":";
                                }
                                else
                                {
                                    durationFormat = duration.Hours.ToString() + ":";
                                }


                                if (duration.Minutes.ToString().Length == 1)
                                {
                                    durationFormat += "0" + duration.Minutes.ToString();
                                }
                                else
                                {
                                    durationFormat += duration.Minutes.ToString();
                                }



                                //Log.Debug("durationFormat=" + durationFormat);
                                GUIPropertyManager.SetProperty("#duration.label", durationFormat);
                                GUIPropertyManager.SetProperty("#type.label", mymessages.TypeTranslation(mymessage.type.ToString()));
                                
                            }
                            //Log.Debug("[TVWishListMP GUI_List]:OnMessage  index="+_focused.ToString()+" messagetext="+messagetext);
                            
                        }
                        catch (Exception exc)
                        {
                            Log.Error("[TVWishListMP GUI_List]:OnMessage Error in format string: Exception=" + exc.Message);
                        }

                        //StatusLabel( "GUI_MSG_ITEM_FOCUS_CHANGED=" + myfacade.SelectedListItemIndex.ToString());
                        //myfacade.DoUpdate();
                        
                    }
                    break;
            }
            

            return base.OnMessage(message);
        }
 
        public override void OnAction(Action action)
     	{
            try
            {

                //Log.Debug("[TVWishListMP GUI_List]:OnAction");

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

                    case MediaPortal.GUI.Library.Action.ActionType.ACTION_CONTEXT_MENU:
                    case MediaPortal.GUI.Library.Action.ActionType.ACTION_SHOW_INFO:
                        if (myfacade.IsFocused) 
                        {
                            //StatusLabel( "myfacade action=" + actionType.ToString() + " id=" + controlId.ToString());
                            ItemSelected();

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
                    
                    //Log.Debug("Action Id: {0}", action.wID);
                    switch (action.wID)
                    {
                        case MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM:
                        case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOUSE_CLICK:
                            ItemSelected();
                            break;
                    }
                }*/
            }
            catch
            {
            }

     	    base.OnAction(action);
     	}

        //GUI clicked
        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
        {
            Log.Debug("[TVWishListMP GUI_List]:OnClicked");

            if (control == myfacade)
            {
                //StatusLabel( "myfacade action=" + actionType.ToString() + " id=" + controlId.ToString());
                ItemSelected();

            }            
            else if (control == buttonUpdate)
            {
                UpdateListItems();
            }
            else if (control == buttonSort)
            {
                ButtonSort();               
            }
            else if (control == buttonFilter)
            {
                ButtonFilter();               
            }
                
            base.OnClicked(controlId, control, actionType);
        }

        protected void ButtonSort()
        {
            Log.Debug("[TVWishListMP GUI_List]:buttonSort");
            //StatusLabel( "start");


            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dlg.ShowQuickNumbers = false;
            dlg.SetHeading(PluginGuiLocalizeStrings.Get(3200));

            for (int i = 3201; i <= 3213; i++)
            {
                dlg.Add(PluginGuiLocalizeStrings.Get(i));
            }

            dlg.ShowQuickNumbers = false;
            dlg.DoModal(GUIWindowManager.ActiveWindow);
            _sort = dlg.SelectedId;
            Log.Debug("Sort=" + _sort.ToString());
            //StatusLabel( "Sort=" + _sort.ToString());
            UpdateListItems();
        }



        protected void ButtonFilter()
        {
                Log.Debug("[TVWishListMP GUI_List]:buttonFilter");
                _filter=0;
                while (_filter != -1)
                {
                    GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                    dlg.ShowQuickNumbers = false;
                    dlg.SetHeading(String.Format(PluginGuiLocalizeStrings.Get(3250))); //"Filter Menu");
                    if (_View == true)
                    {
                        dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3251))); //"View");
                    }
                    else
                    {
                        dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3252))); //"No View");
                    }

                    if (_Email == true)
                    {
                        dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3253))); //"Emailed");
                    }
                    else
                    {
                        dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3254))); //"No Emailed");
                    }

                    if (_Scheduled == true)
                    {
                        dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3255))); //"Scheduled");
                    }
                    else
                    {
                        dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3256))); //"No Scheduled");
                    }

                    if (_Recorded == true)
                    {
                        dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3257))); //"Recorded");
                    }
                    else
                    {
                        dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3258))); //"No Recorded");
                    }

                    if (_Deleted == true)
                    {
                        dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3259))); //"Deleted");
                    }
                    else
                    {
                        dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3260))); //"No Deleted");
                    }

                    if (_Conflicts == true)
                    {
                        dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3261))); //"Conflicts");
                    }
                    else
                    {
                        dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3262))); //"No Conflicts");
                    }

                    dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3263))); //"All");
                    dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3264))); //"None");

                    /*
                    if (_Unknown == true)
                    {
                        dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3263))); //Unknowns"");
                    }
                    else
                    {
                        dlg.Add(String.Format(PluginGuiLocalizeStrings.Get(3264))); //"No Unknowns");
                    }  */

                    dlg.DoModal(GUIWindowManager.ActiveWindow);

                    //evaluate selected
                    _filter = dlg.SelectedId;

                    if (_filter == 1)
                        _View = !_View;

                    else if (_filter == 2)
                        _Email = !_Email;

                    else if (_filter == 3)
                        _Scheduled = !_Scheduled;

                    else if (_filter == 4)
                        _Recorded = !_Recorded;

                    else if (_filter == 5)
                        _Deleted = !_Deleted;

                    else if (_filter == 6)
                        _Conflicts = !_Conflicts;

                    else if (_filter == 7) //All
                    {
                        _View = true;
                        _Email = true;
                        _Scheduled = true;
                        _Recorded = true;
                        _Deleted = true;
                        _Conflicts = true;
                    }

                    else if (_filter == 8) //None
                    {
                        _View = false;
                        _Email = false;
                        _Scheduled = false;
                        _Recorded = false;
                        _Deleted = false;
                        _Conflicts = false;
                    }

                    //StatusLabel( "Filter=" + _filter.ToString());
                    UpdateListItems();
                }

        }



        protected void UpdateControls()
        {
            Log.Debug("[TVWishListMP GUI_List]:UpdateControls");
            GUIPropertyManager.SetProperty("#headershort.label", String.Format(PluginGuiLocalizeStrings.Get(3000)));

            if (myTvWishes.ViewOnlyMode == false)
            {
                GUIPropertyManager.SetProperty("#header.label", String.Format(PluginGuiLocalizeStrings.Get(3000)) + "  " + String.Format(PluginGuiLocalizeStrings.Get(4200))); //"TVWishList - Main-Email&Record");        
                GUIPropertyManager.SetProperty("#modus.label", String.Format(PluginGuiLocalizeStrings.Get(4200))); //"Email&Record");
            }
            else
            {
                GUIPropertyManager.SetProperty("#header.label", String.Format(PluginGuiLocalizeStrings.Get(3000)) + "  " + String.Format(PluginGuiLocalizeStrings.Get(4201))); //"TVWishList - Main-Viewonly");            
                GUIPropertyManager.SetProperty("#modus.label", String.Format(PluginGuiLocalizeStrings.Get(4201))); //"Viewonly");            
            }

            GUIPropertyManager.SetProperty("#textbox.label", "");
            myTvWishes.StatusLabel("");

            GUIControl.SetControlLabel(_guilistwindowid, 2, String.Format(PluginGuiLocalizeStrings.Get(3100)));//TvWish
            GUIControl.SetControlLabel(_guilistwindowid, 3, String.Format(PluginGuiLocalizeStrings.Get(3101)));//Update
            GUIControl.SetControlLabel(_guilistwindowid, 4, String.Format(PluginGuiLocalizeStrings.Get(3102)));//Sort
            GUIControl.SetControlLabel(_guilistwindowid, 5, String.Format(PluginGuiLocalizeStrings.Get(3103)));//Filter
            
        }

        public void UpdateListItems()
        {
            Log.Debug("[TVWishListMP GUI_List]:UpdateListItems  xmlfile="+ _xmlfile);

            //StatusLabel( "sort=" + _sort.ToString() + "  _sortreverse=" + _sortreverse.ToString());
            Log.Debug("[TVWishListMP GUI_List]:UpdateListItems  mymessages.filename=" + mymessages.filename);
            // sort messages    
            Log.Debug("mp list window before sorting messages: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
            Log.Debug("mp list window before sorting messages: TvMessagesFiltered.Count=" + mymessages.ListAllTvMessagesFiltered().Count.ToString());
            mymessages.sortmessages(_sort,_sortreverse);
            Log.Debug("mp list window after sorting messages: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
            Log.Debug("mp list window after sorting messages: TvMessagesFiltered.Count=" + mymessages.ListAllTvMessagesFiltered().Count.ToString());
            //filter messages
            mymessages.filtermessages(_Email, _Deleted, _Conflicts, _Scheduled, _Recorded, _View, _Unknown);
            Log.Debug("mp list window after filtering messages: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
            Log.Debug("mp list window after filtering messages: TvMessagesFiltered.Count=" + mymessages.ListAllTvMessagesFiltered().Count.ToString());

            //display messages
            GUIControl.ClearControl(GetID, myfacade.GetID);

            //update status:
            
            if (mymessages.FilterName == "")
            {
                myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(4404)); //All Tv Wishes
            }
            else
            {
                TvWish mywish = null;
                try
                {
                    mywish = myTvWishes.RetrieveById(mymessages.FilterName);
                }
                catch
                {//nothing
                }

                if (mywish != null)
                {
                    myTvWishes.StatusLabel(String.Format(PluginGuiLocalizeStrings.Get(4405), mywish.name));
                }
                else
                {
                    Log.Error("Could not retrieve tvwish for mymessages.FilterName=" + mymessages.FilterName);
                }
            }
            
            GUIListItem myGuiListItem;

            myfacade.Clear();
            try
            {
                Log.Debug("[TVWishListMP GUI_List]:UpdateListItems message number found: mymessages.ListAllTvMessagesFiltered().Count=" + mymessages.ListAllTvMessagesFiltered().Count.ToString());

                foreach (xmlmessage mymessage in mymessages.ListAllTvMessagesFiltered())
                {
                    /*
                    string mydatum = mymessage.start.ToString("yyyy_MM_dd HH:mm", CultureInfo.InvariantCulture);
                    string listitem = mymessage.title + " - " + mymessage.channel + " - " + mydatum;
                    Log.Debug("listitem=" + listitem);
                    myGuiListItem = new GUIListItem(listitem);*/

                    string listitem = "";

                    if (_UserListItemFormat != "")
                        listitem = mymessages.FormatMessage(mymessage, _UserListItemFormat); //user defined listitem format
                    else if (myTvWishes.ViewOnlyMode == false)
                        listitem = mymessages.FormatMessage(mymessage, PluginGuiLocalizeStrings.Get(3904));  //Email listitemformat
                    else
                        listitem = mymessages.FormatMessage(mymessage, PluginGuiLocalizeStrings.Get(3905));  //View Only listitemformat

                    myGuiListItem = new GUIListItem();
                    listitem = listitem.Replace("||", "\n");
                    string[] labels = listitem.Split('\n');
                    Log.Debug("listitem=" + listitem);
                    Log.Debug("labels.Length=" + labels.Length.ToString());


                    if (labels.Length == 2)
                    {
                        myGuiListItem.Label = labels[0];
                        myGuiListItem.Label2 = labels[1];
                    }
                    else
                    {
                        myGuiListItem.Label = listitem;
                    }

                    myfacade.Add(myGuiListItem);
                }

                if (mymessages.ListAllTvMessagesFiltered().Count == 0)
                {
                    myGuiListItem = new GUIListItem(PluginGuiLocalizeStrings.Get(4301));  //"No items found"
                    myfacade.Add(myGuiListItem);

                    GUIPropertyManager.SetProperty("#textbox.label", string.Empty);
                    GUIPropertyManager.SetProperty("#title.label", string.Empty);
                    GUIPropertyManager.SetProperty("#episodepart.label", string.Empty);
                    GUIPropertyManager.SetProperty("#episodename.label", string.Empty);
                    GUIPropertyManager.SetProperty("#channel.label", string.Empty);
                    GUIPropertyManager.SetProperty("#start.label", string.Empty);
                    GUIPropertyManager.SetProperty("#end.label", string.Empty);
                    GUIPropertyManager.SetProperty("#duration.label", string.Empty);
                    GUIPropertyManager.SetProperty("#type.label", string.Empty);
                }
                Log.Debug("[TVWishListMP GUI_List]:UpdateListItems message number found: myfacade.SubItemCount=" + myfacade.SubItemCount.ToString());
                
            }
            catch (Exception exc)
            {
                myGuiListItem = new GUIListItem(PluginGuiLocalizeStrings.Get(4302));   //Error in creating item list
                myfacade.Add(myGuiListItem);
                Log.Error("Error in creating item list - exception was:" +exc.Message);
            }

            

        }




        private void ItemSelected()
        {
            Log.Debug("[TVWishListMP GUI_List]:ItemSelected  selected item=" + myfacade.SelectedListItemIndex.ToString());
            bool scheduleExists = false;
            List<string> menulist = new List<string>();
            string keyboardstring = "";

            /*
            //Debug facadeview
            Log.Debug("myfacade.ButtonFocusName="+myfacade.ButtonFocusName.ToString());
            Log.Debug("myfacade.ButtonNoFocusName=" + myfacade.ButtonNoFocusName.ToString());
            Log.Debug("myfacade.Count=" + myfacade.Count.ToString());
            //Log.Debug("myfacade.Data=" + myfacade.Data.ToString());
            Log.Debug("myfacade.Focus=" + myfacade.Focus.ToString());
            Log.Debug("myfacade.IsFocused=" + myfacade.IsFocused.ToString());
            Log.Debug("myfacade.ListItems=" + myfacade.ListItems.ToString());
            Log.Debug("myfacade.Selected=" + myfacade.Selected.ToString());
            Log.Debug("myfacade.SelectedItem=" + myfacade.SelectedItem.ToString());
            Log.Debug("myfacade.myfacade.SelectedListItem=" + myfacade.SelectedListItem.ToString());
            Log.Debug("myfacade.SelectedListItemIndex=" + myfacade.SelectedListItemIndex.ToString());
            Log.Debug("myfacade.SelectedRectangle=" + myfacade.SelectedRectangle.ToString());
            Log.Debug("myfacade.SubItemCount=" + myfacade.SubItemCount.ToString());
            Log.Debug("myfacade.Text3Content=" + myfacade.Text3Content.ToString());
            Log.Debug("myfacade.TexutureDownFocusName=" + myfacade.TexutureDownFocusName.ToString());
            Log.Debug("myfacade.TexutureDownName=" + myfacade.TexutureDownName.ToString());
            Log.Debug("myfacade.TexutureUpFocusName=" + myfacade.TexutureUpFocusName.ToString());
            Log.Debug("myfacade.TexutureUpName=" + myfacade.TexutureUpName.ToString());
            Log.Debug("myfacade.Type=" + myfacade.Type.ToString());
            Log.Debug("myfacade.TypeOfList=" + myfacade.TypeOfList.ToString());
           */

            try
            {
                if ((myfacade.SelectedListItemIndex < 0) || (mymessages.ListAllTvMessagesFiltered().Count <= 0))
                {
                    Log.Error("No selected item or no fitered messages available in More menu of List window");
                    return;
                }
                int tvMessageindex = mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex).unfiltered_index;
                xmlmessage mymessage = mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex);
                Log.Debug("tvMessageindex filtered=" + myfacade.SelectedListItemIndex.ToString());
                Log.Debug("tvMessageindex unfiltered=" + tvMessageindex.ToString());
   
                TvWish mywish = myTvWishes.RetrieveById(mymessage.tvwishid);
                // find out if schedule exists for selected item
                // find out if schedule exists for selected item
                
                Schedule foundSchedule = null;
                foreach (Schedule myschedule in Schedule.ListAll())
                {
                    if ((mymessage.title == myschedule.ProgramName) && (mymessage.channel_id == myschedule.IdChannel) && (mymessage.start == myschedule.StartTime) && (mymessage.end == myschedule.EndTime))
                    {
                        foundSchedule = myschedule;
                        Log.Debug("Schedule found title=" + foundSchedule.ProgramName);
                        break;
                    }
                }
                
                
                Recording foundRecording = null;
                foreach (Recording myrecording in Recording.ListAll())
                {
                    //if ((mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex).title == myrecording.Title) && (mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex).channel_id == myrecording.IdChannel) && (mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex).start == myrecording.StartTime) && (mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex).end == myrecording.EndTime))
                    if (mymessage.recordingid == myrecording.IdRecording.ToString())
                    {
                        foundRecording = myrecording;
                        break;
                    }
                }



                if (foundRecording != null)
                    Log.Debug("found Recording ID=" + mymessage.recordingid.ToString() + "  title=" + foundRecording.Title + " at " + foundRecording.StartTime.ToString());
                
                GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                dlg.ShowQuickNumbers = false;
                dlg.SetHeading(PluginGuiLocalizeStrings.Get(3200)); //Dialog header

                dlg.Add(PluginGuiLocalizeStrings.Get(3500));//Goto TV (1)

                dlg.Add(PluginGuiLocalizeStrings.Get(1053));//Create TvWish with Title in Email Mode 
                dlg.Add(PluginGuiLocalizeStrings.Get(1054));//Create TvWish with Title in ViewOnly Mode
                dlg.Add(PluginGuiLocalizeStrings.Get(1055));//Create TvWish with All in Email Mode
                dlg.Add(PluginGuiLocalizeStrings.Get(1056));//Create TvWish with All in ViewOnly Mode

                if ((mymessage.type == MessageType.Deleted.ToString()) || (mymessage.type == MessageType.Conflict.ToString()))
                {
                    dlg.Add(PluginGuiLocalizeStrings.Get(3503)); //Delete message
                }
                else if ((mymessage.type == MessageType.Emailed.ToString()) || (mymessage.type == MessageType.Viewed.ToString()) || (mymessage.type == MessageType.Scheduled.ToString()))
                {
                    dlg.Add(PluginGuiLocalizeStrings.Get(3503)); //Delete message

                    scheduleExists = false;
                    //check all schedules
                    Log.Debug("check all schedules:");
                    foreach (Schedule myschedule in Schedule.ListAll())
                    {
                        Log.Debug("mymessage.title=" + mymessage.title);
                        Log.Debug("myschedule.ProgramName=" + myschedule.ProgramName);
                        Log.Debug("mymessage.channel_id=" + mymessage.channel_id.ToString());
                        Log.Debug("myschedule.IdChannel=" + myschedule.IdChannel.ToString());
                        if ((mymessage.title == myschedule.ProgramName) && (mymessage.channel_id == myschedule.IdChannel) && (mymessage.start == myschedule.StartTime) && (mymessage.end == myschedule.EndTime))
                        {
                            scheduleExists = true;
                            Log.Debug("Schedule found");
                            break;
                        }
                    }
                    Log.Debug("scheduleExists=" + scheduleExists.ToString());
                    if (scheduleExists)
                    {//Delete Schedule
                        dlg.Add(PluginGuiLocalizeStrings.Get(3501)); //Delete Schedule
                    }
                    else
                    {
                        // add prerecord, postrecord, keepuntil, priority, 
                        // change public void SelectedItemChanged(ListItem selectedListItem) in Common_Edit_AND_ConfigDefaults.cs

                        //int index =Convert.ToInt32(mymessage.tvwishid);
                        //Log.Debug("index=" + index.ToString());
                        
                        Log.Debug(" mywish.searchfor=" + mywish.searchfor);
                        Log.Debug(" mywish.tvwishid=" + mywish.tvwishid);

                        myTvWishes.FocusedWishIndex = myTvWishes.GetIndex(mywish);
                        Log.Debug("myTvWishes.FocusedWishIndex=" + myTvWishes.FocusedWishIndex.ToString());
                        dlg.Add(PluginGuiLocalizeStrings.Get(3502)); //Schedule

                        string mystring = String.Format(PluginGuiLocalizeStrings.Get(2508), mywish.prerecord);
                        Log.Debug("initial: mystring=" + mystring);
                        dlg.Add(mystring); //Prerecord

                        mystring = String.Format(PluginGuiLocalizeStrings.Get(2509), mywish.postrecord);
                        Log.Debug("initial: mystring=" + mystring);
                        dlg.Add(mystring); //Postrecord

                        mystring = String.Format(PluginGuiLocalizeStrings.Get(2515), mywish.keepuntil);
                        Log.Debug("initial: mystring=" + mystring);
                        dlg.Add(mystring); //Keepuntil
                    }                   
                }
                else if (mymessage.type == MessageType.Recorded.ToString())
                {
                    
                    dlg.Add(PluginGuiLocalizeStrings.Get(3504));//Delete recording
                    dlg.Add(PluginGuiLocalizeStrings.Get(3505));//Goto recordings
                }

                dlg.DoModal(GUIWindowManager.ActiveWindow);

                switch (dlg.SelectedId)
                {

                    case 1: //Goto Tv main screen
                        try
                        {
                            Main_GUI GUI_Main_window = (Main_GUI)GUIWindowManager.GetWindow(_guimainwindowid);
                            if (GUI_Main_window.TvserverdatabaseSaveSettings() == true)
                            {
                                myTvWishes.MyMessageBox(4400, 1300);   //Info, TvWish data have been saved
                            }
                            else
                            {
                                myTvWishes.MyMessageBox(4305, 1301);   //Error, TvWish data could not be saved
                            }

                        }
                        catch (Exception exc)
                        {
                            Log.Error("saving tvwish data did fail - Exception error " + exc.Message);
                        }
                        GUIWindowManager.ActivateWindow(1);
                        break;

                    case 2://Create TvWish with Title in Email Mode
                        try
                        {
                            string parameter = Main_GUI.SkinCommands.NEWTVWISH_EMAIL.ToString() + "//";                            

                            parameter += "TITLE=" + mymessage.title;

                            Log.Debug("LIST: parameter="+parameter);

                            Main_GUI GUI_Main_window = (Main_GUI)GUIWindowManager.GetWindow(_guimainwindowid);
                            GUI_Main_window.ParameterEvaluation(parameter, _guilistwindowid, true);
                        }
                        catch (Exception exc)
                        {
                            Log.Error("Create Tvwish with Title failed - Exception error " + exc.Message);
                        }

                        break;

                    case 3://Create TvWish with Title in ViewOnly Mode
                        try
                        {
                            string parameter = Main_GUI.SkinCommands.NEWTVWISH_VIEWONLY.ToString() + "//";

                            parameter += "TITLE=" + mymessage.title;

                            Log.Debug("LIST: parameter=" + parameter);

                            Main_GUI GUI_Main_window = (Main_GUI)GUIWindowManager.GetWindow(_guimainwindowid);
                            GUI_Main_window.ParameterEvaluation(parameter, _guilistwindowid, true);
                        }
                        catch (Exception exc)
                        {
                            Log.Error("Create Tvwish with Title failed - Exception error " + exc.Message);
                        }

                        break;

                    case 4: //Create TvWish with All in Email Mode

                        try
                        {
                            string parameter = Main_GUI.SkinCommands.NEWTVWISH_ALL_EMAIL.ToString() + "//";
                                                        
                            parameter += "TITLE=" + mymessage.title + "//";
                            parameter += "NAME=" + mymessage.title + "//";
                            parameter += "CHANNEL=" + mymessage.channel + "//";
                            parameter += "EPISODEPART=" + mymessage.EpisodePart + "//";
                            parameter += "EPISODENAME=" + mymessage.EpisodeName + "//";
                            parameter += "SERIESNUMBER=" + mymessage.SeriesNum + "//";
                            parameter += "EPISODENUMBER=" + mymessage.EpisodeNum;

                            Log.Debug("LIST: parameter=" + parameter);
                            
                            Main_GUI GUI_Main_window = (Main_GUI)GUIWindowManager.GetWindow(_guimainwindowid);

                           

                            GUI_Main_window.ParameterEvaluation(parameter, _guilistwindowid, true);
                        }
                        catch (Exception exc)
                        {
                            Log.Error("Create Tvwish with All failed - Exception error " + exc.Message);
                        }
                        break;

                    case 5: //Create TvWish with All in ViewOnly Mode

                        try
                        {
                            string parameter = Main_GUI.SkinCommands.NEWTVWISH_ALL_VIEWONLY.ToString() + "//";

                            parameter += "TITLE=" + mymessage.title + "//";
                            parameter += "NAME=" + mymessage.title + "//";
                            parameter += "CHANNEL=" + mymessage.channel + "//";
                            parameter += "EPISODEPART=" + mymessage.EpisodePart + "//";
                            parameter += "EPISODENAME=" + mymessage.EpisodeName + "//";
                            parameter += "SERIESNUMBER=" + mymessage.SeriesNum + "//";
                            parameter += "EPISODENUMBER=" + mymessage.EpisodeNum;

                            Log.Debug("LIST: parameter=" + parameter);

                            Main_GUI GUI_Main_window = (Main_GUI)GUIWindowManager.GetWindow(_guimainwindowid);



                            GUI_Main_window.ParameterEvaluation(parameter, _guilistwindowid, true);
                        }
                        catch (Exception exc)
                        {
                            Log.Error("Create Tvwish with All failed - Exception error " + exc.Message);
                        }
                        break;

                    case 6: //optional sixth
                        if ((mymessage.type == MessageType.Deleted.ToString()) || (mymessage.type == MessageType.Conflict.ToString()) || (mymessage.type == MessageType.Emailed.ToString()) || (mymessage.type == MessageType.Viewed.ToString()) || (mymessage.type == MessageType.Viewed.ToString()))
                        {
                            //Delete message
                            try
                            {
                                mymessages.DeleteTvMessageAt(tvMessageindex);
                                myTvWishes.UpdateCounters(mymessages.ListAllTvMessages());
                                UpdateListItems();
                                myTvWishes.MyMessageBox(4400, 3604);   //Info, message has been deleted                            
                            }
                            catch (Exception exc)
                            {
                                Log.Error("Deleting message failed  - Exception error " + exc.Message);
                                myTvWishes.MyMessageBox(4305, 3605);   //Error, deleting message failed
                            }
                        }
                        else if (mymessage.type == MessageType.Recorded.ToString())
                        {
                            //Delete recording
                            if (foundRecording != null) //delete recording and found item
                            {
                                GUIDialogMenu dlg2 = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                                dlg2.ShowQuickNumbers = false;
                                dlg2.SetHeading(PluginGuiLocalizeStrings.Get(4401));  // Warning

                                dlg2.Add(PluginGuiLocalizeStrings.Get(3608)); //Do you really want to delete the file
                                dlg2.Add(mymessage.filename);
                                dlg2.Add("");
                                dlg2.Add(PluginGuiLocalizeStrings.Get(4402)); // Yes
                                dlg2.Add(PluginGuiLocalizeStrings.Get(4403)); // No


                                dlg2.DoModal(GUIWindowManager.ActiveWindow);
                                Log.Debug("dlg.SelectedId=" + dlg2.SelectedId.ToString());


                                if (dlg2.SelectedId == 4) //YES - delete recording
                                {
                                    try
                                    {
                                        try
                                        {
                                            File.Delete(mymessage.filename);
                                            Log.Info("File deleted: " + mymessage.filename);
                                        }
                                        catch { }

                                        Log.Debug("0");
                                        Log.Debug("tvMessageindex=" + tvMessageindex);
                                        Log.Debug("mymessages.ListAllTvMessages().Count=" + mymessages.ListAllTvMessages().Count.ToString());
                                        Log.Debug("mymessages.ListAllTvMessagesFiltered().Count=" + mymessages.ListAllTvMessagesFiltered().Count.ToString());
                                        Log.Debug("a");
                                        xmlmessage mydeletedmessage = mymessages.GetTvMessageAtIndex(tvMessageindex); //this is the unfiltered index
                                        Log.Debug("1");
                                        mydeletedmessage.type = MessageType.Deleted.ToString();
                                        Log.Debug("2");
                                        mydeletedmessage.message = String.Format(PluginGuiLocalizeStrings.Get(3651).ToString());  //File has been deleted;
                                        Log.Debug("3");
                                        mydeletedmessage.created = DateTime.Now;
                                        mymessages.ReplaceTvMessageAtIndex(tvMessageindex, mydeletedmessage);
                                        Log.Debug("4");


                                        
                                        //mymessages.DeleteTvMessageAt(tvMessageindex); not working
                                        Log.Debug("Delete Message modified");                                        
                                        foundRecording.Delete();
                                        Log.Debug("Recording deleted");
                                        myTvWishes.UpdateCounters(mymessages.ListAllTvMessages());
                                        Log.Debug("Counters updated");
                                        UpdateListItems();
                                        myTvWishes.MyMessageBox(4400, 3606);   //Info, Recording has been deleted   
                         
                                    }
                                    catch (Exception exc)
                                    {
                                        Log.Error("Recording deleting failed  - Exception error " + exc.Message);
                                        myTvWishes.MyMessageBox(4305, 3607);   //Error, Deleting recording failed
                                    }
                                }
                            }//end delete recording
                            
                        }

                        break;

                    case 7: //optional seventh
                        if ((mymessage.type == MessageType.Emailed.ToString()) || (mymessage.type == MessageType.Viewed.ToString()) || (mymessage.type == MessageType.Scheduled.ToString()) )
                        {
                            Log.Debug("scheduleExists=" + scheduleExists.ToString());
                            if (scheduleExists == false)//Schedule
                            {

                                try
                                {
                                    if ((myfacade.SelectedListItemIndex >= 0) && (mymessages.ListAllTvMessagesFiltered().Count > 0))
                                    {
                                        // messagetext = mymessages.FormatMessage(mymessages.TvMessagesFiltered[_focused], _TextBoxFormat);



                                        Schedule schedule = layer.AddSchedule(mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex).channel_id, mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex).title, mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex).start, mymessages.GetTvMessageFilteredAtIndex(myfacade.SelectedListItemIndex).end, 0);


                                        if (mywish == null)
                                        {
                                            int prerecord = 5;
                                            int postrecord = 10;
                                            try
                                            {
                                                prerecord = Convert.ToInt32(myTvWishes.DefaultValues[(int)TvWishEntries.prerecord]);
                                                postrecord = Convert.ToInt32(myTvWishes.DefaultValues[(int)TvWishEntries.postrecord]);

                                            }
                                            catch (Exception exc)
                                            {
                                                Log.Debug("More: Manual schedule prerecord or postrecord could not be evaluated - Exception error " + exc.Message);
                                            }

                                            schedule.PreRecordInterval = prerecord;
                                            schedule.PostRecordInterval = postrecord;
                                        }
                                        else //use tvwish parameters
                                        {
                                            schedule.MaxAirings = mywish.i_keepepisodes;
                                            schedule.KeepDate = mywish.D_keepuntil;
                                            if (mywish.i_keepuntil > 0)
                                            {
                                                schedule.KeepDate = DateTime.Now.AddDays(mywish.i_keepuntil);
                                                Log.Debug("Keepdate based on " + mywish.i_keepuntil.ToString() + " days changed to " + schedule.KeepDate.ToString());
                                            }

                                            schedule.KeepMethod = mywish.i_keepmethod;
                                            schedule.PostRecordInterval = mywish.i_postrecord;
                                            schedule.PreRecordInterval = mywish.i_prerecord;
                                            schedule.Priority = mywish.i_priority;
                                            schedule.RecommendedCard = mywish.i_recommendedcard;
                                            schedule.Series = mywish.b_series;
                                        }

                                        schedule.Persist();
                                        // Bug: must not generate new message for manual scheduling  (or use -1 for tvwishid)
                                        //string message = "TvWishList did schedule the program";
                                        //mymessages.addmessage(schedule, message, mywish.t_action, mywish.name, (int)XmlMessages.MessageEvents.SCHEDULE_FOUND, mywish.tvwishid);
                                        //myTvWishes.UpdateCounters(mymessages.ListAllTvMessages());
                                        //UpdateListItems();
                                        myTvWishes.MyMessageBox(4400, 3600);   //Info, Schedule succeeded
                                    }
                                }
                                catch (Exception exc)
                                {
                                    Log.Debug("Schedule adding failed  - Exception error " + exc.Message);
                                    myTvWishes.MyMessageBox(4305, 3601);   //Error, Schedule adding failed
                                }
                            }
                            else //delete schedule
                            {
                                try
                                {
                                    foundSchedule.Delete();
                                    Log.Debug("Schedule deleted");
                                    mymessages.DeleteTvMessageAt(tvMessageindex);
                                    myTvWishes.UpdateCounters(mymessages.ListAllTvMessages());
                                    UpdateListItems();
                                    myTvWishes.MyMessageBox(4400, 3602);   //Info, Schedule has been deleted                            
                                }
                                catch (Exception exc)
                                {
                                    Log.Error("Schedule deleting failed  - Exception error " + exc.Message);
                                    myTvWishes.MyMessageBox(4305, 3603);   //Error, Deleting Schedule failed
                                }
                            }
                        }
                        else if (mymessage.type == MessageType.Recorded.ToString())
                        {
                            
                            //Goto Recordings
                            GUIWindowManager.ActivateWindow(603);
                        }
                        break;

                    case 8: //Prerecord
                        Log.Debug("Case Prerecording");
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
                         ItemSelected();
                        break;

                    case 9: //Postrecord
                        Log.Debug("Case Postrecording");
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
                        ItemSelected();
                        break;

                    case 10: //Keepuntil
                        Log.Debug("Case Keepuntil");
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
                            string mystring = String.Format(PluginGuiLocalizeStrings.Get(2920), "3"); //{0} Days after recording
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
                            string mystring = String.Format(PluginGuiLocalizeStrings.Get(2921), "1"); //{0} Weeks after recording
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
                            string mystring = String.Format(PluginGuiLocalizeStrings.Get(2922), "1"); //{0} Months after recording
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
                        string checkedstring = PluginGuiLocalizeStrings.Get(4101);//defaultvalue
                        string errormessage = "";
                        int days = 0;
                        int keepMethod = 0;
                        DateTime mydate = DateTime.Now;
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
                        ItemSelected();
                        break;
                }
            }

            catch (Exception exc)
            {
                Log.Error("[TVWishListMP GUI_List]:ItemSelected failed exception: "+exc.Message);
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

        private void ButtonSortChanged(object sender, SortEventArgs e)
        {
            buttonSort.SortChanged -= new SortEventHandler(ButtonSortChanged);
            Log.Debug("[TVWishListMP GUI_List]:SortChanged  e.Order=" + e.Order.ToString());
            if (e.Order == SortOrder.Descending)  //2
            {
                _sortreverse = true; //descending  
            }
            else if (e.Order == SortOrder.Ascending) //1
            {
                _sortreverse = false; //ascending
            }
            else
            {
                _sortreverse = false; //ascending
            }
            Log.Debug("[TVWishListMP GUI_List]:SortChanged  _sortreverse=" + _sortreverse.ToString());

            //StatusLabel( "Sortevent=" + e.Order.ToString());
            UpdateListItems();
            buttonSort.SortChanged += new SortEventHandler(ButtonSortChanged);
        }
    }
}
