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

using System;
using System.Collections.Generic; 
using System.Globalization;
using System.Threading;

using TvWishList;

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

using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishListMP2.Models
{
    public class Common_Edit_AND_ConfigDefaults
    {
        #region Localized Resources

        //must be model ID defined in workflow and plugin.xml
        public const string CONFIG_DEFAULTS_GUI_MODEL_ID_STR = "2be92395-da83-4702-a0f7-eb9b13110fff";
        public readonly static Guid CONFIG_DEFAULTS_GUI_MODEL_ID = new Guid(CONFIG_DEFAULTS_GUI_MODEL_ID_STR);

        public string TVWISHLIST_EDIT_DIALOG_MENU_SCREEN = String.Empty;
        public string TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN = String.Empty;
        public string TVWISHLIST_EDIT_DIALOG_MENU2_SCREEN = String.Empty;
        #endregion

        #region Global Variables

        public int[] _listTranslator = new int[(int)TvWishEntries.end];
        public int CaseSelection = -1;
        public int _focusedEditItem = 0;
        public int EvaluateInputTextBoxCase = -1;
        public int KeepuntilCode = 2820;
        public int selected_edit_index = 0;
        public int SelectedDialogItem = 0;

        public int DialogSleep = 100;

        public TvWishProcessing myTvWishes = null;

        //Register global Services
        //public ILogger Log = ServiceRegistration.Get<ILogger>();
        IScreenManager ScreenManager = ServiceRegistration.Get<IScreenManager>();


        #endregion Global variables

        #region General Properties
        public string TvWishList_Dialog_Menu_Screen
        {
            get { return TVWISHLIST_EDIT_DIALOG_MENU_SCREEN; }
            set { TVWISHLIST_EDIT_DIALOG_MENU_SCREEN = value; }
        }
        public string TvWishList_Dialog_Menu2_Screen
        {
            get { return TVWISHLIST_EDIT_DIALOG_MENU2_SCREEN; }
            set { TVWISHLIST_EDIT_DIALOG_MENU2_SCREEN = value; }
        }
        public string TvWishList_Input_Textbox_Screen
        {
            get { return TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN; }
            set { TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN = value; }
        }
        #endregion General Properties


        #region Properties for Skins

        //Properties
        protected readonly AbstractProperty _dialogHeaderProperty;
        protected readonly AbstractProperty _inputHeaderProperty;
        protected readonly AbstractProperty _inputTextBoxSkinProperty;

        // Itemlist that is exposed to the skin (ListView in skin)
        protected ItemsList _skinItemList = new ItemsList();  //must be defined as new ItemsList() here !
        protected ItemsList _dialogMenuItemList = new ItemsList();  //must be defined as new ItemsList() here !
        protected ItemsList _dialogMenuItemList2 = new ItemsList();  //must be defined as new ItemsList() here !

        // Skin elements
        public string DialogHeader
        {
            get { return (string)_dialogHeaderProperty.GetValue(); }
            set { _dialogHeaderProperty.SetValue(value); }
        }
        public string InputTextBoxSkin
        {
            get { return (string)_inputTextBoxSkinProperty.GetValue(); }
            set { _inputTextBoxSkinProperty.SetValue(value); }
        }
        public string InputHeader
        {
            get { return (string)_inputHeaderProperty.GetValue(); }
            set { _inputHeaderProperty.SetValue(value); }
        }

        public ItemsList SkinItemList
        {
            get { return _skinItemList; }
            //set { _testlistProperty.SetValue(value); }
        }
        public ItemsList DialogMenuItemList
        {
            get { return _dialogMenuItemList; }
        }
        public ItemsList DialogMenuItemList2
        {
            get { return _dialogMenuItemList2; }
        }


        public AbstractProperty DialogHeaderProperty
        {
            get { return _dialogHeaderProperty; }
        }
        public AbstractProperty InputTextBoxSkinProperty
        {
            get { return _inputTextBoxSkinProperty; }
        }
        public AbstractProperty InputHeaderProperty
        {
            get { return _inputHeaderProperty; }
        }
        //do not forget Wproperties in constructor!

        #endregion Properties for Skins

        
        
        #region Constructor

        /// <summary>
        /// Constructor... this one is called by the WorkflowManager when this model is loaded due to a screen reference.
        /// </summary>
        public Common_Edit_AND_ConfigDefaults()  
        {
            Log.Debug("Common_Edit_GUI_AND_ConfigDefaults_GUI: Constructor called");
            
            // In models, properties will always be WProperty instances. When using SProperties for screen databinding,         
            _dialogHeaderProperty = new WProperty(typeof(string), String.Empty);
            _inputTextBoxSkinProperty = new WProperty(typeof(string), String.Empty);
            _inputHeaderProperty = new WProperty(typeof(string), String.Empty);

            
        }

        #endregion Constructor



        #region common ConfigDefaults_GUI
        protected void AddEditItem(int itemNumber, string itemString, ref int ctr)
        {
            Log.Debug("itemNumber=" + ((TvWishEntries)itemNumber).ToString());
            Log.Debug("itemString=" + itemString);

            if (myTvWishes._boolTranslator[itemNumber])
            {
                string mystring = String.Format(PluginGuiLocalizeStrings.Get(2500 + itemNumber), itemString);
                Log.Debug("true: mystring=" + mystring);
                ListItem myitem = new ListItem();
                myitem.SetLabel("Name", mystring);
                myitem.SetLabel("Index", ctr.ToString());
                Log.Debug("Index=" + myitem.Labels["Index"].ToString());
                myitem.Command = new MethodDelegateCommand(() =>
                {
                    //button lock
                    if (myTvWishes.ButtonActive)
                    {
                        myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Operation Is Still Running"
                        return;
                    }
                    myTvWishes.ButtonActive = true; //lock buttons

                    SelectedItemChanged(myitem);

                    myTvWishes.ButtonActive = false;
                });
                _skinItemList.Add(myitem);

                _listTranslator[ctr] = itemNumber;
                ctr++;
            }

        }

        public void UpdateListItems()
        {
            Log.Debug("EditGUI: UpdateListItems() started");

            //display edit items
            SkinItemList.Clear();
            int ctr = 0;

            Log.Debug("myTvWishes.ListAll().Count " + myTvWishes.ListAll().Count.ToString());
            //string[] edit_item_names = new string[24];

            Log.Debug("_listTranslator.Length=" + _listTranslator.Length.ToString());
            for (int i = 0; i < _listTranslator.Length; i++)
            {
                _listTranslator[i] = -1;
            }

            try
            {
                Log.Debug("myTvWishes.FocusedWishIndex=" + myTvWishes.FocusedWishIndex.ToString());
                TvWish mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);


                /* not needed
                //check mywish data for correct values
                TvWishProcessing checkTvWish = new TvWishProcessing();
                checkTvWish.
                */

                myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

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
            catch (Exception exc)
            {
                ListItem myitem = new ListItem();
                myitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(4302));   //Error in creating item list
                myitem.SetLabel("Index", "-1");
                _skinItemList.Add(myitem);
                Log.Error("Error in creating item list - exception was:" + exc.Message);
            }




            SkinItemList.FireChange();


        }

        public void FocusedItemChanged(ListItem focusedListItem)
        {
            if (focusedListItem == null)
            {
                Log.Debug("EditGUI: FocusedItemChanged: Focused item=null");
                return;
            }

            _focusedEditItem = Convert.ToInt32(focusedListItem.Labels["Index"].ToString());

            if (_focusedEditItem == -1)
                return;



            Log.Debug("EditGUI: FocusedItemChanged: Focused item=" + focusedListItem.Labels["Name"]);
        }

        public void SelectedItemChanged(ListItem selectedListItem)
        {
            //no button lock - already done

            if (selectedListItem == null)
            {
                Log.Debug("EditGUI: FocusedItemChanged: Focused item=null");
                return;
            }

            Log.Debug("EditGUI: SelectedItemChanged: Selected item number=" + selectedListItem.Labels["Name"]);

            //int selected_edit_index = Convert.ToInt32(selectedListItem.Labels["Index"].ToString());
            selected_edit_index = _focusedEditItem;
            Log.Debug("Selected item is " + selected_edit_index.ToString());
            Log.Debug("_listTranslator[selected_edit_index]=" + _listTranslator[selected_edit_index].ToString());
            CaseSelection = _listTranslator[selected_edit_index];
            Log.Debug("Tvwish caseSelection=" + CaseSelection.ToString());

            ItemProcessing();

        }

        public void ItemProcessing()
        {

            try
            {

                TvWish mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);//from main menu selected

                List<string> menulist = new List<string>();


                

                switch (CaseSelection)
                {
                    //no button lock - already done

                    case (int)TvWishEntries.active:

                        if (myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).b_active == true)
                        {
                            mywish.b_active = false;
                            mywish.active = PluginGuiLocalizeStrings.Get(4001);//False
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                        }
                        else
                        {
                            mywish.b_active = true;
                            mywish.active = PluginGuiLocalizeStrings.Get(4000);
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                        }
                        UpdateListItems();
                        Thread myselectthread = new Thread(SelectItemUpdate);
                        myselectthread.Start();

                        //Log.Debug("check mywish.b_active=" + mywish.b_active.ToString());
                        break;

                    case (int)TvWishEntries.searchfor:
                        InputHeader = PluginGuiLocalizeStrings.Get(2801); //Search For:
                        InputTextBoxSkin = mywish.searchfor;
                        EvaluateInputTextBoxCase = (int)TvWishEntries.searchfor;
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                        break;

                    case (int)TvWishEntries.matchtype:
                        _dialogMenuItemList.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2802); //Match Type


                        for (int i = 0; i <= 6; i++)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(i + 2600));
                            mydialogitem.SetLabel("Index", i.ToString());
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                Log.Debug("1 i=" + i.ToString());
                                Log.Debug("mydialogitem.Labels[Index].ToString()=" + mydialogitem.Labels["Index"].ToString());

                                int index = Convert.ToInt32(mydialogitem.Labels["Index"].ToString());
                                Log.Debug("2 index=" + index.ToString());
                                mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                Log.Debug("3");
                                mywish.matchtype = PluginGuiLocalizeStrings.Get(2600 + index);
                                myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();

                            });
                            _dialogMenuItemList.Add(mydialogitem);
                        }

                        //update for dialog skin
                        DialogMenuItemList.FireChange();

                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;

                    case (int)TvWishEntries.group:

                        _dialogMenuItemList.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2803);  //Group

                        ListItem mydialogitem_b = new ListItem();
                        mydialogitem_b.SetLabel("Name", PluginGuiLocalizeStrings.Get(4104));  //All Channels
                        mydialogitem_b.SetLabel("Index", "0");
                        mydialogitem_b.Command = new MethodDelegateCommand(() =>
                        {
                            mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                            mywish.group = PluginGuiLocalizeStrings.Get(4104);
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                            IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                            mycommandscreenManager.CloseTopmostDialog();
                            UpdateListItems();
                            myselectthread = new Thread(SelectItemUpdate);
                            myselectthread.Start();
                        });
                        _dialogMenuItemList.Add(mydialogitem_b);


                        int ctr = 1;
                        foreach (ChannelGroup channelgroup in myTvWishes.AllChannelGroups)
                        {
                            if (channelgroup.GroupName != "All Channels")
                            {
                                ListItem mydialogitem_c = new ListItem();
                                mydialogitem_c.SetLabel("Name", channelgroup.GroupName);
                                mydialogitem_c.SetLabel("Index", ctr.ToString());
                                mydialogitem_c.Command = new MethodDelegateCommand(() =>
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.group = mydialogitem_c.Labels["Name"].ToString();
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                    mycommandscreenManager.CloseTopmostDialog();
                                    UpdateListItems();
                                    myselectthread = new Thread(SelectItemUpdate);
                                    myselectthread.Start();
                                });
                                ctr++;
                                _dialogMenuItemList.Add(mydialogitem_c);

                            }
                        }
                        foreach (RadioChannelGroup radiochannelgroup in myTvWishes.AllRadioChannelGroups)
                        {
                            if (radiochannelgroup.GroupName != "All Channels")
                            {
                                ListItem mydialogitem_d = new ListItem();
                                mydialogitem_d.SetLabel("Name", radiochannelgroup.GroupName);
                                mydialogitem_d.SetLabel("Index", ctr.ToString());
                                mydialogitem_d.Command = new MethodDelegateCommand(() =>
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.group = mydialogitem_d.Labels["Name"].ToString();
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                    mycommandscreenManager.CloseTopmostDialog();
                                    UpdateListItems();
                                    myselectthread = new Thread(SelectItemUpdate);
                                    myselectthread.Start();
                                });
                                ctr++;
                                _dialogMenuItemList.Add(mydialogitem_d);
                            }
                        }
                        //update for dialog skin
                        DialogMenuItemList.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;

                    case (int)TvWishEntries.recordtype:
                        _dialogMenuItemList.Clear();
                        DialogHeader = PluginGuiLocalizeStrings.Get(2804); //Record Type

                        for (int i = 0; i <= 5; i++)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(i + 2650));
                            mydialogitem.SetLabel("Index", i.ToString());
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                int index = Convert.ToInt32(mydialogitem.Labels["Index"].ToString());
                                mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                mywish.recordtype = PluginGuiLocalizeStrings.Get(2650 + index);
                                if (index == 0) //If Only Once selected check other settings
                                {
                                    if (mywish.keepepisodes != PluginGuiLocalizeStrings.Get(4105)) //All
                                    {
                                        mywish.keepepisodes = PluginGuiLocalizeStrings.Get(4105); //All
                                        myTvWishes.MyMessageBox(4305, 4309); //Keep Episodes number had to be changed to Any
                                    }
                                }
                                myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();
                            });
                            _dialogMenuItemList.Add(mydialogitem);
                        }
                        //update for dialog skin
                        DialogMenuItemList.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;

                    case (int)TvWishEntries.action:
                        _dialogMenuItemList.Clear();
                        DialogHeader = PluginGuiLocalizeStrings.Get(2805); //Action

                        for (int i = 0; i <= 2; i++)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(i + 2700));
                            mydialogitem.SetLabel("Index", i.ToString());
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                int index = Convert.ToInt32(mydialogitem.Labels["Index"].ToString());
                                mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                mywish.action = PluginGuiLocalizeStrings.Get(2700 + index);
                                myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();
                            });
                            _dialogMenuItemList.Add(mydialogitem);
                        }
                        //update for dialog skin
                        DialogMenuItemList.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;

                    case (int)TvWishEntries.exclude:
                        InputHeader = PluginGuiLocalizeStrings.Get(2806); //Exclude
                        InputTextBoxSkin = mywish.exclude;
                        EvaluateInputTextBoxCase = (int)TvWishEntries.exclude;
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                        break;

                    case (int)TvWishEntries.prerecord:
                        _dialogMenuItemList.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2808);  //Prerecord

                        string[] allItemStrings = new string[] { "0", "5", "8", "10", "15", "30", PluginGuiLocalizeStrings.Get(4102) }; //Custom
                        foreach (string itemstring in allItemStrings)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", itemstring);
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                if (mydialogitem.Labels["Name"].ToString() == PluginGuiLocalizeStrings.Get(4102)) //Custom
                                {
                                    InputHeader = PluginGuiLocalizeStrings.Get(2808); //Prerecord
                                    InputTextBoxSkin = mywish.prerecord;
                                    EvaluateInputTextBoxCase = (int)TvWishEntries.prerecord;
                                    ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                                }
                                else
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.prerecord = mydialogitem.Labels["Name"].ToString();
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                }

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();
                            });
                            _dialogMenuItemList.Add(mydialogitem);
                        }
                        //update for dialog skin
                        DialogMenuItemList.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;

                    case (int)TvWishEntries.postrecord:
                        _dialogMenuItemList.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2809);  //Postrecord

                        allItemStrings = new string[] { "0", "5", "8", "10", "15", "30", PluginGuiLocalizeStrings.Get(4102) }; //Custom
                        foreach (string itemstring in allItemStrings)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", itemstring);
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                if (mydialogitem.Labels["Name"].ToString() == PluginGuiLocalizeStrings.Get(4102)) //Custom
                                {
                                    InputHeader = PluginGuiLocalizeStrings.Get(2809); //Postrecord
                                    InputTextBoxSkin = mywish.postrecord;
                                    EvaluateInputTextBoxCase = (int)TvWishEntries.postrecord;
                                    ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                                }
                                else
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.postrecord = mydialogitem.Labels["Name"].ToString();
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                }

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();
                            });
                            _dialogMenuItemList.Add(mydialogitem);
                        }
                        //update for dialog skin
                        DialogMenuItemList.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;

                    case (int)TvWishEntries.episodename:
                        InputHeader = PluginGuiLocalizeStrings.Get(2810); //Episodename
                        InputTextBoxSkin = mywish.episodename;
                        EvaluateInputTextBoxCase = (int)TvWishEntries.episodename;
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                        break;

                    case (int)TvWishEntries.episodepart:
                        InputHeader = PluginGuiLocalizeStrings.Get(2811); //Episodepart
                        InputTextBoxSkin = mywish.episodepart;
                        EvaluateInputTextBoxCase = (int)TvWishEntries.episodepart;
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                        break;

                    case (int)TvWishEntries.episodenumber:
                        InputHeader = PluginGuiLocalizeStrings.Get(2812); //Episodenumber
                        InputTextBoxSkin = mywish.episodenumber;
                        EvaluateInputTextBoxCase = (int)TvWishEntries.episodenumber;
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                        break;

                    case (int)TvWishEntries.seriesnumber:
                        InputHeader = PluginGuiLocalizeStrings.Get(2813); //Seriesnumber
                        InputTextBoxSkin = mywish.seriesnumber;
                        EvaluateInputTextBoxCase = (int)TvWishEntries.seriesnumber;
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                        break;

                    case (int)TvWishEntries.keepepisodes:
                        _dialogMenuItemList.Clear();
                        DialogHeader = PluginGuiLocalizeStrings.Get(2814);  //Keep Episodes
                        allItemStrings = new string[] { "3", "5", "8", "10", "15", PluginGuiLocalizeStrings.Get(4102) }; //Custom
                        foreach (string itemstring in allItemStrings)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", itemstring);
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                if (mydialogitem.Labels["Name"].ToString() == PluginGuiLocalizeStrings.Get(4102)) //Custom
                                {
                                    InputHeader = PluginGuiLocalizeStrings.Get(2814);  //Keep Episodes
                                    InputTextBoxSkin = mywish.keepepisodes;
                                    EvaluateInputTextBoxCase = (int)TvWishEntries.keepepisodes;
                                    ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                                }
                                else
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.keepepisodes = mydialogitem.Labels["Name"].ToString();

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

                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                }

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();
                            });
                            _dialogMenuItemList.Add(mydialogitem);
                        }
                        //update for dialog skin
                        DialogMenuItemList.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;

                    case (int)TvWishEntries.keepuntil:
                        _dialogMenuItemList.Clear();
                        DialogHeader = PluginGuiLocalizeStrings.Get(2815); //Keep Until

                        for (int i = 0; i <= 6; i++)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(i + 2900));//Always
                            mydialogitem.SetLabel("Index", i.ToString());
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                int index = Convert.ToInt32(mydialogitem.Labels["Index"].ToString());
                                Log.Debug("keepuntil index = " + index.ToString());
                                if (index == 0) //Always
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.keepuntil = PluginGuiLocalizeStrings.Get(2900); //Always
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                    UpdateListItems();
                                    myselectthread = new Thread(SelectItemUpdate);
                                    myselectthread.Start();
                                }
                                else if (index == 1) //days after recording
                                {
                                    DialogHeader = PluginGuiLocalizeStrings.Get(2901);  // Days After Recording

                                    _dialogMenuItemList2.Clear();
                                    allItemStrings = new string[] { "3", "5", "7", "10", "15", "30", PluginGuiLocalizeStrings.Get(4102) }; //Custom
                                    foreach (string itemstring in allItemStrings)
                                    {
                                        ListItem mydialogitem2 = new ListItem();
                                        mydialogitem2.SetLabel("Name", itemstring);
                                        Log.Debug("mydialogitem2=" + mydialogitem2.Labels["Name"].ToString());
                                        mydialogitem2.Command = new MethodDelegateCommand(() =>
                                        {
                                            Log.Debug("mydialogitem2.Labels[Name].ToString()=" + mydialogitem2.Labels["Name"].ToString());
                                            if (mydialogitem2.Labels["Name"].ToString() == PluginGuiLocalizeStrings.Get(4102)) //Custom
                                            {
                                                InputHeader = PluginGuiLocalizeStrings.Get(2901); // Days After Recording
                                                InputTextBoxSkin = "";
                                                KeepuntilCode = 2920; //days after recording
                                                EvaluateInputTextBoxCase = (int)TvWishEntries.keepuntil;
                                                ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                                            }
                                            else
                                            {
                                                mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                                int j = Convert.ToInt32(mydialogitem2.Labels["Name"].ToString());
                                                string newvalue = String.Format(PluginGuiLocalizeStrings.Get(2920), j);
                                                Log.Debug("newvalue=" + newvalue);
                                                mywish.keepuntil = newvalue;
                                                myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                            }

                                            IScreenManager mycommandscreenManager2 = ServiceRegistration.Get<IScreenManager>();
                                            mycommandscreenManager2.CloseTopmostDialog();
                                            UpdateListItems();
                                            myselectthread = new Thread(SelectItemUpdate);
                                            myselectthread.Start();
                                        });
                                        _dialogMenuItemList2.Add(mydialogitem2);
                                    }
                                    //update for dialog skin
                                    DialogMenuItemList2.FireChange();
                                    //will now call a dialogbox with a given menu            
                                    ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU2_SCREEN);

                                }//days after recording

                                else if (index == 2) //weeks after recording
                                {
                                    _dialogMenuItemList2.Clear();

                                    DialogHeader = PluginGuiLocalizeStrings.Get(2902);  // Weeks After Recording

                                    allItemStrings = new string[] { "3", "5", "7", "10", "15", "30", PluginGuiLocalizeStrings.Get(4102) }; //Custom
                                    foreach (string itemstring in allItemStrings)
                                    {
                                        ListItem mydialogitem2 = new ListItem();
                                        mydialogitem2.SetLabel("Name", itemstring);
                                        mydialogitem2.Command = new MethodDelegateCommand(() =>
                                        {
                                            if (mydialogitem2.Labels["Name"].ToString() == PluginGuiLocalizeStrings.Get(4102)) //Custom
                                            {
                                                InputHeader = PluginGuiLocalizeStrings.Get(2902);  // Weeks After Recording
                                                InputTextBoxSkin = "";
                                                KeepuntilCode = 2921; //weeks after recording
                                                EvaluateInputTextBoxCase = (int)TvWishEntries.keepuntil;
                                                ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                                            }
                                            else
                                            {
                                                mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                                int j = Convert.ToInt32(mydialogitem2.Labels["Name"].ToString());
                                                string newvalue = String.Format(PluginGuiLocalizeStrings.Get(2921), j); //weeks
                                                mywish.keepuntil = newvalue;
                                                myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                            }

                                            IScreenManager mycommandscreenManager2 = ServiceRegistration.Get<IScreenManager>();
                                            mycommandscreenManager2.CloseTopmostDialog();
                                            UpdateListItems();
                                            myselectthread = new Thread(SelectItemUpdate);
                                            myselectthread.Start();
                                        });
                                        _dialogMenuItemList2.Add(mydialogitem2);
                                    }
                                    //update for dialog skin
                                    DialogMenuItemList2.FireChange();
                                    //will now call a dialogbox with a given menu            
                                    ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU2_SCREEN);

                                }//weeks after recording
                                else if (index == 3) //months after recording
                                {
                                    _dialogMenuItemList2.Clear();

                                    DialogHeader = PluginGuiLocalizeStrings.Get(2903);  // Months After Recording

                                    allItemStrings = new string[] { "3", "5", "7", "10", "15", "30", PluginGuiLocalizeStrings.Get(4102) }; //Custom
                                    foreach (string itemstring in allItemStrings)
                                    {
                                        ListItem mydialogitem2 = new ListItem();
                                        mydialogitem2.SetLabel("Name", itemstring);
                                        mydialogitem2.Command = new MethodDelegateCommand(() =>
                                        {
                                            if (mydialogitem2.Labels["Name"].ToString() == PluginGuiLocalizeStrings.Get(4102)) //Custom
                                            {
                                                InputHeader = PluginGuiLocalizeStrings.Get(2903);  // Months After Recording
                                                InputTextBoxSkin = "";
                                                KeepuntilCode = 2922; //months after recording
                                                EvaluateInputTextBoxCase = (int)TvWishEntries.keepuntil;
                                                ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                                            }
                                            else
                                            {
                                                mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                                int j = Convert.ToInt32(mydialogitem2.Labels["Name"].ToString());
                                                string newvalue = String.Format(PluginGuiLocalizeStrings.Get(2922), j); //months
                                                mywish.keepuntil = newvalue;
                                                myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                            }

                                            IScreenManager mycommandscreenManager2 = ServiceRegistration.Get<IScreenManager>();
                                            mycommandscreenManager2.CloseTopmostDialog();
                                            UpdateListItems();
                                            myselectthread = new Thread(SelectItemUpdate);
                                            myselectthread.Start();
                                        });
                                        _dialogMenuItemList2.Add(mydialogitem2);
                                    }
                                    //update for dialog skin
                                    DialogMenuItemList2.FireChange();
                                    //will now call a dialogbox with a given menu            
                                    ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU2_SCREEN);

                                }//months after recording
                                else if (index == 4) //date
                                {
                                    InputHeader = PluginGuiLocalizeStrings.Get(2815) + " " + PluginGuiLocalizeStrings.Get(2923); //Keep Until date format
                                    InputTextBoxSkin = mywish.keepuntil;
                                    EvaluateInputTextBoxCase = (int)TvWishEntries.skip;// cheat with skip
                                    ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                                }
                                else if (index == 5) //until watched
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.keepuntil = PluginGuiLocalizeStrings.Get(2905); //watched
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                    UpdateListItems();
                                    myselectthread = new Thread(SelectItemUpdate);
                                    myselectthread.Start();
                                }
                                else if (index == 6) //until space needed
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.keepuntil = PluginGuiLocalizeStrings.Get(2906); //Space                                 
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                    UpdateListItems();
                                    myselectthread = new Thread(SelectItemUpdate);
                                    myselectthread.Start();
                                }
                                Log.Debug("mywish.keepuntil=" + mywish.keepuntil);

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                Log.Debug("First Dialog is closed");

                            });
                            _dialogMenuItemList.Add(mydialogitem);
                        }
                        //update for dialog skin
                        DialogMenuItemList.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);

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

                        _dialogMenuItemList.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2816);  //Recommended Card

                        ListItem mydialogitem_e = new ListItem();
                        mydialogitem_e.SetLabel("Name", PluginGuiLocalizeStrings.Get(4100));  //Any
                        mydialogitem_e.SetLabel("Index", "0");
                        mydialogitem_e.Command = new MethodDelegateCommand(() =>
                        {
                            mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                            mywish.recommendedcard = PluginGuiLocalizeStrings.Get(4100);  //Any
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                            IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                            mycommandscreenManager.CloseTopmostDialog();
                            UpdateListItems();
                            myselectthread = new Thread(SelectItemUpdate);
                            myselectthread.Start();
                        });
                        _dialogMenuItemList.Add(mydialogitem_e);

                        ctr = 1;
                        foreach (Card card in myTvWishes.AllCards)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", ctr.ToString() + ":" + card.Name);
                            mydialogitem.SetLabel("Index", ctr.ToString());
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                mywish.recommendedcard = mydialogitem.Labels["Index"].ToString();
                                myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();
                            });
                            ctr++;
                            _dialogMenuItemList.Add(mydialogitem);

                        }
                        //update for dialog skin
                        DialogMenuItemList.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;

                    case (int)TvWishEntries.priority:

                        _dialogMenuItemList.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2817);  //Priority

                        allItemStrings = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                        foreach (string itemstring in allItemStrings)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", itemstring);
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                mywish.priority = mydialogitem.Labels["Name"].ToString();
                                myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();
                            });
                            _dialogMenuItemList.Add(mydialogitem);
                        }
                        //update for dialog skin
                        DialogMenuItemList.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;

                    case (int)TvWishEntries.aftertime:

                        _dialogMenuItemList.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2818);  //After Time

                        allItemStrings = new string[] { PluginGuiLocalizeStrings.Get(4100), "20:00", "23:00", "02:00", "08:00", "12:00", "16:00", PluginGuiLocalizeStrings.Get(4102) }; //Any, Custom
                        foreach (string itemstring in allItemStrings)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", itemstring);
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                if (mydialogitem.Labels["Name"].ToString() == PluginGuiLocalizeStrings.Get(4102)) //Custom
                                {
                                    InputHeader = PluginGuiLocalizeStrings.Get(2818); //After Time
                                    InputTextBoxSkin = mywish.aftertime;
                                    EvaluateInputTextBoxCase = (int)TvWishEntries.aftertime;
                                    ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                                }
                                else
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.aftertime = mydialogitem.Labels["Name"].ToString();
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                }

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();
                            });
                            _dialogMenuItemList.Add(mydialogitem);
                        }
                        //update for dialog skin
                        DialogMenuItemList.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;

                    case (int)TvWishEntries.beforetime:

                        _dialogMenuItemList.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2819);  //Before Time

                        allItemStrings = new string[] { PluginGuiLocalizeStrings.Get(4100), "20:00", "23:00", "02:00", "08:00", "12:00", "16:00", PluginGuiLocalizeStrings.Get(4102) }; //Any, Custom
                        foreach (string itemstring in allItemStrings)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", itemstring);
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                if (mydialogitem.Labels["Name"].ToString() == PluginGuiLocalizeStrings.Get(4102)) //Custom
                                {
                                    InputHeader = PluginGuiLocalizeStrings.Get(2819);  //Before Time
                                    InputTextBoxSkin = mywish.beforetime;
                                    EvaluateInputTextBoxCase = (int)TvWishEntries.beforetime;
                                    ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                                }
                                else
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.beforetime = mydialogitem.Labels["Name"].ToString();
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                }

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();
                            });
                            _dialogMenuItemList.Add(mydialogitem);
                        }
                        //update for dialog skin
                        DialogMenuItemList.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;

                    case (int)TvWishEntries.afterdays:
                        _dialogMenuItemList.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2820); //After Day

                        ListItem mydialogitem_f = new ListItem();
                        mydialogitem_f.SetLabel("Name", PluginGuiLocalizeStrings.Get(4100));  //Any
                        mydialogitem_f.Command = new MethodDelegateCommand(() =>
                        {
                            mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                            mywish.afterdays = PluginGuiLocalizeStrings.Get(4100);  //Any
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                            IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                            mycommandscreenManager.CloseTopmostDialog();
                            UpdateListItems();
                            myselectthread = new Thread(SelectItemUpdate);
                            myselectthread.Start();
                        });
                        _dialogMenuItemList.Add(mydialogitem_f);

                        for (int i = 1; i <= 7; i++)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(i + 2749));
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                mywish.afterdays = mydialogitem.Labels["Name"].ToString();
                                myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();
                            });
                            _dialogMenuItemList.Add(mydialogitem);
                        }

                        //update for dialog skin
                        DialogMenuItemList.FireChange();

                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;

                    case (int)TvWishEntries.beforedays:
                        _dialogMenuItemList.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2821); //Before Day

                        ListItem mydialogitem_g = new ListItem();
                        mydialogitem_g.SetLabel("Name", PluginGuiLocalizeStrings.Get(4100));  //Any
                        mydialogitem_g.Command = new MethodDelegateCommand(() =>
                        {
                            mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                            mywish.beforedays = PluginGuiLocalizeStrings.Get(4100);  //Any
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                            IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                            mycommandscreenManager.CloseTopmostDialog();
                            UpdateListItems();
                            myselectthread = new Thread(SelectItemUpdate);
                            myselectthread.Start();
                        });
                        _dialogMenuItemList.Add(mydialogitem_g);

                        for (int i = 1; i <= 7; i++)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(i + 2749));
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                mywish.beforedays = mydialogitem.Labels["Name"].ToString();
                                myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();
                            });
                            _dialogMenuItemList.Add(mydialogitem);
                        }

                        //update for dialog skin
                        DialogMenuItemList.FireChange();

                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;


                    case (int)TvWishEntries.channel:
                        Log.Debug("Channel case");
                        _dialogMenuItemList.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2822);  //Channel

                        ListItem mydialogitem_h = new ListItem();
                        mydialogitem_h.SetLabel("Name", PluginGuiLocalizeStrings.Get(4100));  //Any
                        mydialogitem_h.SetLabel("Index", "0");
                        mydialogitem_h.Command = new MethodDelegateCommand(() =>
                        {
                            mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                            mywish.channel = PluginGuiLocalizeStrings.Get(4100);  //Any
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                            IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                            mycommandscreenManager.CloseTopmostDialog();
                            UpdateListItems();
                            myselectthread = new Thread(SelectItemUpdate);
                            myselectthread.Start();
                        });
                        _dialogMenuItemList.Add(mydialogitem_h);

                        ctr = 1;
                        IList<Channel> allChannelsInGroup = null;
                        if (mywish.group == PluginGuiLocalizeStrings.Get(4100))
                        {
                            allChannelsInGroup = myTvWishes.AllChannels;
                        }
                        else //find channelnames by group
                        {
                            allChannelsInGroup = new List<Channel>();
                            foreach (ChannelGroup channelgroup in myTvWishes.AllChannelGroups)
                            {
                                if (channelgroup.GroupName == mywish.group)  //groupname must match
                                {
                                    foreach (Channel channel in Channel.ListAllByGroup(channelgroup.Id))
                                    {
                                        allChannelsInGroup.Add(channel);
                                    }
                                }
                            }
                            foreach (RadioChannelGroup radiochannelgroup in myTvWishes.AllRadioChannelGroups)
                            {
                                if (radiochannelgroup.GroupName == mywish.group)  //groupname must match
                                {
                                    foreach (RadioChannel channel in RadioChannel.ListAllByGroup(radiochannelgroup.Id))
                                    {
                                        allChannelsInGroup.Add(new Channel {Name=channel.Name}); //need to create new channel from radiochannel
                                    }
                                }
                            }
                        }

                        foreach (Channel channel in allChannelsInGroup)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", channel.Name);
                            mydialogitem.SetLabel("Index", ctr.ToString());
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                mywish.channel = mydialogitem.Labels["Name"].ToString();
                                myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();
                            });
                            ctr++;
                            _dialogMenuItemList.Add(mydialogitem);
                        }

                        //update for dialog skin
                        DialogMenuItemList.FireChange();

                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;

                    case (int)TvWishEntries.skip:
                        if (myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).b_skip == true)
                        {
                            mywish.b_skip = false;
                            mywish.skip = PluginGuiLocalizeStrings.Get(4001);//False
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                        }
                        else
                        {
                            mywish.b_skip = true;
                            mywish.skip = PluginGuiLocalizeStrings.Get(4000);
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                        }
                        UpdateListItems();
                        myselectthread = new Thread(SelectItemUpdate);
                        myselectthread.Start();
                        break;

                    case (int)TvWishEntries.name:
                        InputHeader = PluginGuiLocalizeStrings.Get(2824); //Name
                        InputTextBoxSkin = mywish.name;
                        EvaluateInputTextBoxCase = (int)TvWishEntries.name;
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                        break;

                    case (int)TvWishEntries.useFolderName:

                        _dialogMenuItemList.Clear();
                        DialogHeader = PluginGuiLocalizeStrings.Get(2825); //UseFolderName

                        for (int i = 0; i <= 3; i++)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(i + 2850));
                            mydialogitem.SetLabel("Index", i.ToString());
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                int index = Convert.ToInt32(mydialogitem.Labels["Index"].ToString());
                                mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                mywish.useFolderName = PluginGuiLocalizeStrings.Get(2850 + index);
                                if (i > 0) //If  Epsisode Management is not selected check other settings
                                {
                                    if (mywish.keepepisodes != PluginGuiLocalizeStrings.Get(4105)) //All
                                    {
                                        mywish.keepepisodes = PluginGuiLocalizeStrings.Get(4105); //All
                                        myTvWishes.MyMessageBox(4305, 4309); //Keep Episodes number had to be changed to All
                                    }
                                }
                                myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();
                            });
                            _dialogMenuItemList.Add(mydialogitem);
                        }
                        //update for dialog skin
                        DialogMenuItemList.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;

                    case (int)TvWishEntries.withinNextHours:
                        _dialogMenuItemList.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2826);  //Within Next Hours

                        allItemStrings = new string[] { PluginGuiLocalizeStrings.Get(4100), "1", "2", "3", "4", "8", "16", PluginGuiLocalizeStrings.Get(4102) }; //Any, Custom
                        foreach (string itemstring in allItemStrings)
                        {
                            ListItem mydialogitem = new ListItem();
                            mydialogitem.SetLabel("Name", itemstring);
                            mydialogitem.Command = new MethodDelegateCommand(() =>
                            {
                                if (mydialogitem.Labels["Name"].ToString() == PluginGuiLocalizeStrings.Get(4102)) //Custom
                                {
                                    InputHeader = PluginGuiLocalizeStrings.Get(2826);  //Within Next Hours
                                    InputTextBoxSkin = mywish.withinNextHours;
                                    EvaluateInputTextBoxCase = (int)TvWishEntries.withinNextHours;
                                    ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
                                }
                                else
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.withinNextHours = mydialogitem.Labels["Name"].ToString();
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                }

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                UpdateListItems();
                                myselectthread = new Thread(SelectItemUpdate);
                                myselectthread.Start();
                            });
                            _dialogMenuItemList.Add(mydialogitem);
                        }

                        //update for dialog skin
                        DialogMenuItemList.FireChange();

                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;


                    case (int)TvWishEntries.episodecriteria:

                        EpisodeCriteriaDialog();

                        break;

                    case (int)TvWishEntries.preferredgroup:
                        _dialogMenuItemList.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2834);  //Preferred Group

                        ListItem mydialogitem_i = new ListItem();
                        mydialogitem_i.SetLabel("Name", PluginGuiLocalizeStrings.Get(4104));  //All Channels
                        mydialogitem_i.SetLabel("Index", "0");
                        mydialogitem_i.Command = new MethodDelegateCommand(() =>
                        {
                            mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                            mywish.preferredgroup = PluginGuiLocalizeStrings.Get(4104);
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                            IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                            mycommandscreenManager.CloseTopmostDialog();
                            UpdateListItems();
                            myselectthread = new Thread(SelectItemUpdate);
                            myselectthread.Start();
                        });
                        _dialogMenuItemList.Add(mydialogitem_i);

                        ctr = 1;
                        foreach (ChannelGroup channelgroup in myTvWishes.AllChannelGroups)
                        {
                            if (channelgroup.GroupName != "All Channels")
                            {
                                ListItem mydialogitem = new ListItem();
                                mydialogitem.SetLabel("Name", channelgroup.GroupName);
                                mydialogitem.SetLabel("Index", ctr.ToString());
                                mydialogitem.Command = new MethodDelegateCommand(() =>
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.preferredgroup = mydialogitem.Labels["Name"].ToString();
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                    mycommandscreenManager.CloseTopmostDialog();
                                    UpdateListItems();
                                    myselectthread = new Thread(SelectItemUpdate);
                                    myselectthread.Start();
                                });
                                ctr++;
                                _dialogMenuItemList.Add(mydialogitem);
                            }
                        }
                        foreach (RadioChannelGroup radiochannelgroup in myTvWishes.AllRadioChannelGroups)
                        {
                            if (radiochannelgroup.GroupName != "All Channels")
                            {
                                ListItem mydialogitem = new ListItem();
                                mydialogitem.SetLabel("Name", radiochannelgroup.GroupName);
                                mydialogitem.SetLabel("Index", ctr.ToString());
                                mydialogitem.Command = new MethodDelegateCommand(() =>
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.preferredgroup = mydialogitem.Labels["Name"].ToString();
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);

                                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                    mycommandscreenManager.CloseTopmostDialog();
                                    UpdateListItems();
                                    myselectthread = new Thread(SelectItemUpdate);
                                    myselectthread.Start();
                                });
                                ctr++;
                                _dialogMenuItemList.Add(mydialogitem);
                            }
                        }
                        //update for dialog skin
                        DialogMenuItemList.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
                        break;


                    case (int)TvWishEntries.includerecordings:

                        mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                        if (myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).b_includeRecordings == true)
                        {
                            mywish.b_includeRecordings = false;
                            mywish.includeRecordings = PluginGuiLocalizeStrings.Get(4001); //false
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                        }
                        else
                        {
                            mywish.b_includeRecordings = true;
                            mywish.includeRecordings = PluginGuiLocalizeStrings.Get(4000);
                            myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                        }

                        UpdateListItems();
                        myselectthread = new Thread(SelectItemUpdate);
                        myselectthread.Start();
                        //Log.Debug("check mywish.b_active=" + mywish.b_active.ToString());
                        break;

                    //modify for listview table changes


                }//end switch




            }

            catch (Exception exc)
            {

                Log.Error("[TVWishList GUI_Edit]:ItemSelectionChanged: ****** Exception " + exc.Message);

            }

        }

        public void EpisodeCriteriaDialog()
        {
            _dialogMenuItemList.Clear();

            DialogHeader = PluginGuiLocalizeStrings.Get(2833);  //EpisodeCriteria
            TvWish mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);


            ListItem mydialogitem_a = new ListItem();
            if (mywish.b_episodecriteria_d == true)
            {
                mydialogitem_a.SetLabel("Name", PluginGuiLocalizeStrings.Get(2950)); //"Description");
            }
            else
            {
                mydialogitem_a.SetLabel("Name", PluginGuiLocalizeStrings.Get(2951)); //"No Description");
            }
            mydialogitem_a.Command = new MethodDelegateCommand(() =>
            {
                TvWish mycommandwish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);

                if (mycommandwish.b_episodecriteria_d == false)
                {
                    mydialogitem_a.SetLabel("Name", PluginGuiLocalizeStrings.Get(2950)); //"Description");
                }
                else
                {
                    mydialogitem_a.SetLabel("Name", PluginGuiLocalizeStrings.Get(2951)); //"No Description");
                }
                mycommandwish.b_episodecriteria_d = !mycommandwish.b_episodecriteria_d;
                DialogMenuItemList.FireChange();
                EpisodeCriteriaDialogEvaluation(mycommandwish);
                SelectedDialogItem = 0;
                Thread myselectthread = new Thread(SelectItemUpdateEpisodeCriteriaDialog);
                myselectthread.Start();
            });
            _dialogMenuItemList.Add(mydialogitem_a);


            ListItem mydialogitem_b = new ListItem();
            if (mywish.b_episodecriteria_n == true)
            {
                mydialogitem_b.SetLabel("Name", PluginGuiLocalizeStrings.Get(2952)); //"Episode Names");
            }
            else
            {
                mydialogitem_b.SetLabel("Name", PluginGuiLocalizeStrings.Get(2953)); //"No Episode Names");
            }
            mydialogitem_b.Command = new MethodDelegateCommand(() =>
            {
                TvWish mycommandwish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);

                if (mycommandwish.b_episodecriteria_n == false)
                {
                    mydialogitem_b.SetLabel("Name", PluginGuiLocalizeStrings.Get(2952)); //"Episode Names");
                }
                else
                {
                    mydialogitem_b.SetLabel("Name", PluginGuiLocalizeStrings.Get(2953)); //"No Episode Names");
                }
                mydialogitem_b.Selected = true;
                mycommandwish.b_episodecriteria_n = !mycommandwish.b_episodecriteria_n;
                DialogMenuItemList.FireChange();
                EpisodeCriteriaDialogEvaluation(mycommandwish);
                SelectedDialogItem = 1;
                Thread myselectthread = new Thread(SelectItemUpdateEpisodeCriteriaDialog);
                myselectthread.Start();
            });
            _dialogMenuItemList.Add(mydialogitem_b);



            ListItem mydialogitem_c = new ListItem();
            if (mywish.b_episodecriteria_c == true)
            {
                mydialogitem_c.SetLabel("Name", PluginGuiLocalizeStrings.Get(2954)); //"Episode Numbers");
            }
            else
            {
                mydialogitem_c.SetLabel("Name", PluginGuiLocalizeStrings.Get(2955)); //"No Episode Numbers");
            }
            mydialogitem_c.Command = new MethodDelegateCommand(() =>
            {
                TvWish mycommandwish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);

                if (mycommandwish.b_episodecriteria_c == false)
                {
                    mydialogitem_c.SetLabel("Name", PluginGuiLocalizeStrings.Get(2954)); //"Episode Numbers");
                }
                else
                {
                    mydialogitem_c.SetLabel("Name", PluginGuiLocalizeStrings.Get(2955)); //"No Episode Numbers");
                }
                mydialogitem_c.Selected = true;
                mycommandwish.b_episodecriteria_c = !mycommandwish.b_episodecriteria_c;
                DialogMenuItemList.FireChange();
                EpisodeCriteriaDialogEvaluation(mycommandwish);
                SelectedDialogItem = 2;
                Thread myselectthread = new Thread(SelectItemUpdateEpisodeCriteriaDialog);
                myselectthread.Start();
            });
            _dialogMenuItemList.Add(mydialogitem_c);


            ListItem mydialogitem_d = new ListItem();
            mydialogitem_d.SetLabel("Name", PluginGuiLocalizeStrings.Get(2970)); //"All");
            mydialogitem_d.Command = new MethodDelegateCommand(() =>
            {
                TvWish mycommandwish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                _dialogMenuItemList[0].SetLabel("Name", PluginGuiLocalizeStrings.Get(2950)); //"Description");
                _dialogMenuItemList[1].SetLabel("Name", PluginGuiLocalizeStrings.Get(2952)); //"Episode Names");
                _dialogMenuItemList[2].SetLabel("Name", PluginGuiLocalizeStrings.Get(2954)); //"Episode Numbers");
                mycommandwish.b_episodecriteria_d = true;
                mycommandwish.b_episodecriteria_n = true;
                mycommandwish.b_episodecriteria_c = true;

                mydialogitem_d.Selected = true;

                DialogMenuItemList.FireChange();
                EpisodeCriteriaDialogEvaluation(mycommandwish);
                SelectedDialogItem = 3;
                Thread myselectthread = new Thread(SelectItemUpdateEpisodeCriteriaDialog);
                myselectthread.Start();
            });
            _dialogMenuItemList.Add(mydialogitem_d);


            ListItem mydialogitem_e = new ListItem();
            mydialogitem_e.SetLabel("Name", PluginGuiLocalizeStrings.Get(2971)); //"None");
            mydialogitem_e.Command = new MethodDelegateCommand(() =>
            {
                TvWish mycommandwish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                _dialogMenuItemList[0].SetLabel("Name", PluginGuiLocalizeStrings.Get(2951)); //"No Description");
                _dialogMenuItemList[1].SetLabel("Name", PluginGuiLocalizeStrings.Get(2953)); //"No Episode Names");
                _dialogMenuItemList[2].SetLabel("Name", PluginGuiLocalizeStrings.Get(2955)); //"No Episode Numbers");
                mycommandwish.b_episodecriteria_d = false;
                mycommandwish.b_episodecriteria_n = false;
                mycommandwish.b_episodecriteria_c = false;

                mydialogitem_e.Selected = true;

                DialogMenuItemList.FireChange();
                EpisodeCriteriaDialogEvaluation(mycommandwish);
                SelectedDialogItem = 4;
                Thread myselectthread = new Thread(SelectItemUpdateEpisodeCriteriaDialog);
                myselectthread.Start();
            });
            _dialogMenuItemList.Add(mydialogitem_e);


            //update for dialog skin
            DialogMenuItemList.FireChange();

            //will now call a dialogbox with a given menu            
            ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);

            //UpdateListItems();
        }

        public void EpisodeCriteriaDialogEvaluation(TvWish mywish)
        {
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
        }

        public void EvaluateInputTextBox()
        {
            //no button lock

            Log.Debug("EditGUI: EvaluateInputTextBox() started");

            TvWish mywish;
            switch (EvaluateInputTextBoxCase)
            {
                case (int)TvWishEntries.active: //cheated used for OnButtonNew()
                    TvWish newwish = myTvWishes.DefaultData();
                    newwish.searchfor = InputTextBoxSkin;
                    newwish.name = InputTextBoxSkin;
                    myTvWishes.Add(newwish);
                    myTvWishes.FocusedWishIndex = myTvWishes.ListAll().Count - 1;
                    break;

                case (int)TvWishEntries.searchfor:
                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    mywish.searchfor = InputTextBoxSkin;
                    if (mywish.name == "")
                        mywish.name = InputTextBoxSkin;
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

                case (int)TvWishEntries.exclude:
                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    mywish.exclude = InputTextBoxSkin;
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

                case (int)TvWishEntries.prerecord:
                    try
                    {
                        int k = Convert.ToInt32(InputTextBoxSkin);
                        //Log.Debug("k=" + k.ToString());
                    }
                    catch //do nothing and use default
                    {
                        InputTextBoxSkin = "5";
                    }
                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    mywish.prerecord = InputTextBoxSkin;
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

                case (int)TvWishEntries.postrecord:
                    try
                    {
                        int k = Convert.ToInt32(InputTextBoxSkin);
                        //Log.Debug("k=" + k.ToString());
                    }
                    catch //do nothing and use default
                    {
                        InputTextBoxSkin = "5";
                    }
                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    mywish.postrecord = InputTextBoxSkin;
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

                case (int)TvWishEntries.episodename:
                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    mywish.episodename = InputTextBoxSkin;
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

                case (int)TvWishEntries.episodepart:
                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    mywish.episodepart = InputTextBoxSkin;
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

                case (int)TvWishEntries.episodenumber:
                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    mywish.episodenumber = InputTextBoxSkin;
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

                case (int)TvWishEntries.seriesnumber:
                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    mywish.seriesnumber = InputTextBoxSkin;
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

                case (int)TvWishEntries.keepepisodes:
                    try
                    {
                        int k = Convert.ToInt32(InputTextBoxSkin);
                        //Log.Debug("k=" + k.ToString());
                    }
                    catch //do nothing and use default
                    {
                        InputTextBoxSkin = "5";
                    }
                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    mywish.keepepisodes = InputTextBoxSkin;
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
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

                case (int)TvWishEntries.keepuntil:
                    int keepuntil_i;
                    try
                    {
                        keepuntil_i = Convert.ToInt32(InputTextBoxSkin);

                    }
                    catch //do nothing and use default
                    {
                        keepuntil_i = -1;
                    }

                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    string newvalue = String.Format(PluginGuiLocalizeStrings.Get(KeepuntilCode), keepuntil_i);
                    mywish.keepuntil = newvalue;
                    if (keepuntil_i == -1)
                        mywish.keepuntil = PluginGuiLocalizeStrings.Get(2900);
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

                case (int)TvWishEntries.skip: //cheated used for entering date in keepuntil  YYYY-MM-DD
                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    mywish.keepuntil = InputTextBoxSkin;
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

                case (int)TvWishEntries.aftertime:
                    try
                    {
                        DateTime k = DateTime.ParseExact(InputTextBoxSkin, "HH:mm", CultureInfo.InvariantCulture);
                        //Log.Debug("k=" + k.ToString());
                    }
                    catch //do nothing and use default
                    {
                        InputTextBoxSkin = PluginGuiLocalizeStrings.Get(4100);//"Any"
                    }
                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    mywish.aftertime = InputTextBoxSkin;
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

                case (int)TvWishEntries.beforetime:
                    try
                    {
                        DateTime k = DateTime.ParseExact(InputTextBoxSkin, "HH:mm", CultureInfo.InvariantCulture);
                        //Log.Debug("k=" + k.ToString());
                    }
                    catch //do nothing and use default
                    {
                        InputTextBoxSkin = PluginGuiLocalizeStrings.Get(4100);//"Any"
                    }
                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    mywish.beforetime = InputTextBoxSkin;
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

                case (int)TvWishEntries.name:
                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    mywish.name = InputTextBoxSkin;
                    if (mywish.searchfor == "")
                        mywish.searchfor = InputTextBoxSkin;
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

                case (int)TvWishEntries.withinNextHours:
                    try
                    {
                        int k = Convert.ToInt32(InputTextBoxSkin);
                        //Log.Debug("k=" + k.ToString());
                    }
                    catch //do nothing and use default
                    {
                        InputTextBoxSkin = PluginGuiLocalizeStrings.Get(4100);//"Any"
                    }
                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                    mywish.withinNextHours = InputTextBoxSkin;
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    break;

            }

            UpdateListItems();
            Thread myselectthread = new Thread(SelectItemUpdate);
            myselectthread.Start();
        }

        public void SelectItemUpdate()
        {
            Thread.Sleep(DialogSleep);

            for (int i = 0; i < SkinItemList.Count; i++)
            {
                _skinItemList[i].Selected = false;
            }

            _skinItemList[selected_edit_index].Selected = true;
            SkinItemList.FireChange();
        }

        public void SelectItemUpdateEpisodeCriteriaDialog()
        {
            Log.Debug("SelectItemUpdateEpisodeCriteriaDialog()");
            Thread.Sleep(DialogSleep);

            for (int i = 0; i < _dialogMenuItemList.Count; i++)
            {
                _dialogMenuItemList[i].Selected = false;
            }
            //Log.Debug("SelectedDialogItem=" + SelectedDialogItem.ToString());
            //Log.Debug("_dialogMenuItemList.Count="+_dialogMenuItemList.Count.ToString());
            _dialogMenuItemList[SelectedDialogItem].Selected = true;
            DialogMenuItemList.FireChange();

            UpdateListItems();
        }
        #endregion common ConfigDefaults_GUI


    }
}
