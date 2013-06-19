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
using System.IO;
using System.Threading;
using System.Windows.Forms;

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
using MPExtended.Services.TVAccessService.Interfaces;

using MediaPortal.Plugins.TvWishListMP2.Settings; //needed for configuration setting loading

using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishListMP2.Models
{
    public partial class Result_GUI : IWorkflowModel, IDisposable
    {

        #region Localized Resources

        //must be model ID defined in workflow and plugin.xml
        public const string MAIN_GUI_MODEL_ID_STR = "46199691-8dc6-443d-9022-1315cee6152b";
        public readonly static Guid MAIN_GUI_MODEL_ID = new Guid(MAIN_GUI_MODEL_ID_STR);
        public const string EDIT_GUI_MODEL_ID_STR = "093c13ed-413e-4fc2-8db0-3eca69c09ad0";
        public readonly static Guid EDIT_GUI_MODEL_ID = new Guid(EDIT_GUI_MODEL_ID_STR);
        public const string RESULT_GUI_MODEL_ID_STR = "6e96da05-1c6a-4fed-8fed-b14ad114c4a2";
        public readonly static Guid RESULT_GUI_MODEL_ID = new Guid(RESULT_GUI_MODEL_ID_STR);

        public const string HOME_GUI_STATE_ID_STR = "7F702D9C-F2DD-42da-9ED8-0BA92F07787F";
        public readonly static Guid HOME_GUI_STATE_ID = new Guid(HOME_GUI_STATE_ID_STR);
                                                    
        public const string TV_GUI_STATE_ID_STR = "C7646667-5E63-48c7-A490-A58AC9518CFA";
        public readonly static Guid TV_GUI_STATE_ID = new Guid(TV_GUI_STATE_ID_STR);
        public const string TV_GUI_MODEL_ID_STR = "8BEC1372-1C76-484c-8A69-C7F3103708EC";
        public readonly static Guid TV_GUI_MODEL_ID = new Guid(TV_GUI_MODEL_ID_STR);

        public const string EPG_GUI_STATE_ID_STR = "7323BEB9-F7B0-48c8-80FF-8B59A4DB5385";
        public readonly static Guid EPG_GUI_STATE_ID = new Guid(EPG_GUI_STATE_ID_STR);
        public const string EPG_GUI_MODEL_ID_STR = "5054408D-C2A9-451f-A702-E84AFCD29C10";
        public readonly static Guid EPG_GUI_MODEL_ID = new Guid(EPG_GUI_MODEL_ID_STR);

        public const string VIDEO_GUI_STATE_ID_STR = "22ED8702-3887-4acb-ACB4-30965220AFF0";
        public readonly static Guid VIDEO_GUI_STATE_ID = new Guid(VIDEO_GUI_STATE_ID_STR);
        public const string VIDEO_GUI_MODEL_ID_STR = "4CDD601F-E280-43b9-AD0A-6D7B2403C856";
        public readonly static Guid VIDEO_GUI_MODEL_ID = new Guid(VIDEO_GUI_MODEL_ID_STR);

        public const string SERIES_GUI_STATE_ID_STR = "30F57CBA-459C-4202-A587-09FFF5098251";
        public readonly static Guid SERIES_GUI_STATE_ID = new Guid(VIDEO_GUI_STATE_ID_STR);
        public const string SERIES_GUI_MODEL_ID_STR = "4CDD601F-E280-43b9-AD0A-6D7B2403C856";
        public readonly static Guid SERIES_GUI_MODEL_ID = new Guid(VIDEO_GUI_MODEL_ID_STR);

        public const string TVWISHLIST_RESULT_DIALOG_MENU_SCREEN = "TvWishListResultDialogMenu";
        public const string TVWISHLIST_RESULT_DIALOG_MENU_SCREEN2 = "TvWishListResultDialogMenu2";
        public const string TVWISHLIST_RESULT_DIALOG_MENU_SCREEN3 = "TvWishListResultDialogMenu3";
        public const string TVWISHLIST_RESULT_INPUT_TEXTBOX_SCREEN = "TvWishListResultInputTextBox";
        
        #endregion

        #region Global Variables and Services

        //Register global Services
        //ILogger Log = ServiceRegistration.Get<ILogger>();
        IScreenManager ScreenManager = ServiceRegistration.Get<IScreenManager>();

        string PreviousModel = string.Empty;

        public bool WideSkin=true;

        //int DialogSleep = 100;

        int _focusedMessage = 0;
        xmlmessage mymessage;

        int SelectedDialogItem = 0;

        int CaseSelection = -1;
        int KeepuntilCode = -1;
       


        private bool _active = false;
        public bool Active
        {
            get { return (bool)_active; }
        }

        public enum MenuItems
        {
            GotoTv = 1,
            CreateTvWishEmail,
            CreateTvWishViewOnly,
            CreateTvWishAllEmail,
            CreateTvWishAllViewOnly,
            DeleteMessage,
            DeleteSchedule,
            Schedule,
            DeleteRecording,
            GotoVideo,
            Prerecord,
            Postrecord,
            Keepuntil,
        }

        #endregion Global Variables and Services

        #region Properties for Skins

        //Properties
        protected readonly AbstractProperty _headerProperty;
        protected readonly AbstractProperty _modusProperty;
        protected readonly AbstractProperty _statusProperty;
        protected readonly AbstractProperty _textBoxSkinProperty;
        protected readonly AbstractProperty _dialogHeaderProperty;
        protected readonly AbstractProperty _inputHeaderProperty;
        protected readonly AbstractProperty _inputTextBoxSkinProperty;
        protected readonly AbstractProperty _focusedTitleProperty;
        protected readonly AbstractProperty _focusedChannelProperty;
        protected readonly AbstractProperty _focusedStartProperty;
        protected readonly AbstractProperty _focusedEndProperty;
        protected readonly AbstractProperty _focusedDurationProperty;
        protected readonly AbstractProperty _focusedEpisodePartProperty;
        protected readonly AbstractProperty _focusedEpisodeNameProperty;
        protected readonly AbstractProperty _focusedTypeProperty;

        // Itemlist that is exposed to the skin (ListView in skin)
        protected ItemsList _skinItemList = new ItemsList();  //must be defined as new ItemsList() here !
        protected ItemsList _dialogMenuItemList = new ItemsList();  //must be defined as new ItemsList() here !
        protected ItemsList _dialogMenuItemList2 = new ItemsList();  //must be defined as new ItemsList() here !
        protected ItemsList _dialogMenuItemList3 = new ItemsList();  //must be defined as new ItemsList() here !

        // Skin elements
        public string Header
        {
            get { return (string)_headerProperty.GetValue(); }
            set { _headerProperty.SetValue(value); }
        }
        public string Modus
        {
            get { return (string)_modusProperty.GetValue(); }
            set { _modusProperty.SetValue(value); }
        }
        public string Status
        {
            get { return (string)_statusProperty.GetValue(); }
            set { _statusProperty.SetValue(value); }
        }
        public string TextBoxSkin
        {
            get { return (string)_textBoxSkinProperty.GetValue(); }
            set { _textBoxSkinProperty.SetValue(value); }
        }
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
        public string FocusedTitle
        {
            get { return (string)_focusedTitleProperty.GetValue(); }
            set { _focusedTitleProperty.SetValue(value); }
        }
        public string FocusedChannel
        {
            get { return (string)_focusedChannelProperty.GetValue(); }
            set { _focusedChannelProperty.SetValue(value); }
        }
        public string FocusedStart
        {
            get { return (string)_focusedStartProperty.GetValue(); }
            set { _focusedStartProperty.SetValue(value); }
        }
        public string FocusedEnd
        {
            get { return (string)_focusedEndProperty.GetValue(); }
            set { _focusedEndProperty.SetValue(value); }
        }
        public string FocusedDuration
        {
            get { return (string)_focusedDurationProperty.GetValue(); }
            set { _focusedDurationProperty.SetValue(value); }
        }
        public string FocusedEpisodePart
        {
            get { return (string)_focusedEpisodePartProperty.GetValue(); }
            set { _focusedEpisodePartProperty.SetValue(value); }
        }
        public string FocusedEpisodeName
        {
            get { return (string)_focusedEpisodeNameProperty.GetValue(); }
            set { _focusedEpisodeNameProperty.SetValue(value); }
        }
        public string FocusedType
        {
            get { return (string)_focusedTypeProperty.GetValue(); }
            set { _focusedTypeProperty.SetValue(value); }
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
        public ItemsList DialogMenuItemList3
        {
            get { return _dialogMenuItemList3; }
        }
        
        public AbstractProperty HeaderProperty
        {
            get { return _headerProperty; }
        }
        public AbstractProperty ModusProperty
        {
            get { return _modusProperty; }
        }
        public AbstractProperty StatusProperty
        {
            get { return _statusProperty; }
        }
        public AbstractProperty TextBoxSkinProperty
        {
            get { return _textBoxSkinProperty; }
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
        public AbstractProperty FocusedTitleProperty
        {
            get { return _focusedTitleProperty; }
        }
        public AbstractProperty FocusedChannelProperty
        {
            get { return _focusedChannelProperty; }
        }
        public AbstractProperty FocusedStartProperty
        {
            get { return _focusedStartProperty; }
        }
        public AbstractProperty FocusedEndProperty
        {
            get { return _focusedEndProperty; }
        }
        public AbstractProperty FocusedDurationProperty
        {
            get { return _focusedDurationProperty; }
        }
        public AbstractProperty FocusedEpisodePartProperty
        {
            get { return _focusedEpisodePartProperty; }
        }
        public AbstractProperty FocusedEpisodeNameProperty
        {
            get { return _focusedEpisodeNameProperty; }
        }
        public AbstractProperty FocusedTypeProperty
        {
            get { return _focusedTypeProperty; }
        }
        //do not forget Wproperties in constructor!

        #endregion Properties for Skins

        #region Constructor and Dispose

        /// <summary>
        /// Constructor... this one is called by the WorkflowManager when this model is loaded due to a screen reference.
        /// </summary>
        public Result_GUI()  //will be called when the screen is the first time loaded not the same as Init() !!!
        {
            Log.Debug("Result_GUI: Constructor called");
            _instance = this; //needed to ensure transfer from static function later on

            // In models, properties will always be WProperty instances. When using SProperties for screen databinding,
            _headerProperty = new WProperty(typeof(string), "[TvWishListMP2.3000]");
            _modusProperty = new WProperty(typeof(string), "[TvWishListMP2.4200]");
            _statusProperty = new WProperty(typeof(string), String.Empty);
            _textBoxSkinProperty = new WProperty(typeof(string), String.Empty);
            _dialogHeaderProperty = new WProperty(typeof(string), String.Empty);
            _inputTextBoxSkinProperty = new WProperty(typeof(string), String.Empty);
            _inputHeaderProperty = new WProperty(typeof(string), String.Empty);

            _focusedTitleProperty = new WProperty(typeof(string), String.Empty);
            _focusedChannelProperty = new WProperty(typeof(string), String.Empty);
            _focusedStartProperty = new WProperty(typeof(string), String.Empty);
            _focusedEndProperty = new WProperty(typeof(string), String.Empty);
            _focusedDurationProperty = new WProperty(typeof(string), String.Empty);
            _focusedEpisodePartProperty = new WProperty(typeof(string), String.Empty);
            _focusedEpisodeNameProperty = new WProperty(typeof(string), String.Empty);
            _focusedTypeProperty = new WProperty(typeof(string), String.Empty);

            //initialize MP1 plugin translation
            PluginGuiLocalizeStrings.MP2Section = "TvWishListMP2";

            //TvWishes Initialization
            if (TvWishProcessing.Instance == null)
            {                
                Log.Error("Fatal Error: TvWishprocessing instance did not exist in Result_GUI - going back");
                IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
                workflowManager.NavigatePop(1);
            }
            else
            {
                myTvWishes = TvWishProcessing.Instance;
            }

            
            //Message Initialization 
            if (XmlMessages.Instance == null)
            {
                Log.Error("Fatal Error: XmlMessages instance did not exist in Result_GUI - going back");
                IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
                workflowManager.NavigatePop(1);
            }
            else
            {
                mymessages = XmlMessages.Instance;
            }

            
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

           
        }

        public void Dispose()
        {
            //seems to be usable for MP1 function DeInit()
            Log.Debug("Result_GUI: Dispose() - disposing");
        }

        #endregion Constructor and Dispose

        #region IWorkflowModel Implementation
        // to use this you must have derived the class from IWorkflowModel, IDisposable
        // and you must have defined in plugin.xml a workflowstate
        // WorkflowModel="023c44f2-3329-4781-9b4a-c974444c0b0d"/> <!-- MyTestPlugin Model -->

        public Guid ModelId
        {
            get { return RESULT_GUI_MODEL_ID; }
        }

        public bool CanEnterState(NavigationContext oldContext, NavigationContext newContext)
        {
            return true;
        }

        public void EnterModelContext(NavigationContext oldContext, NavigationContext newContext)
        {
            OnPageLoad(oldContext, newContext);
        }

        public void ExitModelContext(NavigationContext oldContext, NavigationContext newContext)
        {
            OnPageDestroy(oldContext, newContext);            
        }
        
        public void ChangeModelContext(NavigationContext oldContext, NavigationContext newContext, bool push)
        {
        }

        public void Deactivate(NavigationContext oldContext, NavigationContext newContext)
        {
            OnPageDestroy(oldContext, newContext);
        }

        public void Reactivate(NavigationContext oldContext, NavigationContext newContext)
        {
            OnPageLoad(oldContext, newContext);
        }

        public void UpdateMenuActions(NavigationContext context, IDictionary<Guid, WorkflowAction> actions)
        {
        }

        public ScreenUpdateMode UpdateScreen(NavigationContext context, ref string screen)
        {
            return ScreenUpdateMode.AutoWorkflowManager;
        }

        #endregion Implementation

        #region OnPage

        public void OnPageLoad(NavigationContext oldContext, NavigationContext newContext)
        {
            Log.Debug("[TVWishListMP2 GUI_List]:OnPageLoad()  xmlfile=" + _xmlfile);
            Log.Debug("newModelId=" + newContext.WorkflowModelId.ToString());
            Log.Debug("oldModelId=" + oldContext.WorkflowModelId.ToString());

            _active = true;

            if ((oldContext.WorkflowModelId != EDIT_GUI_MODEL_ID) && (oldContext.WorkflowModelId != MAIN_GUI_MODEL_ID))//TVWishList  EDIT or MAIN only
            {
                //go back to previous window
                IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
                workflowManager.NavigatePop(1); //same as escape (takes one entry from the stack) 
                return; //do not forget return!
            }


            PreviousModel = oldContext.WorkflowModelId.ToString();

            if (Main_GUI.Instance == null)
            {
                Log.Error("Fatal Error: Main GUI instance is null on Result page - aborting without saving data and without unlocking");
                return;
            }
            //DialogSleep = Main_GUI.Instance.DialogSleep; //neded for dialog selected elements

            //in view only mode view must be turned on initially
            if ((myTvWishes.ViewOnlyMode == true) && (mymessages.View == false))
            {
               mymessages.View = true;
            }

            UpdateControls();
            UpdateListItems();
        }

        public void OnPageDestroy(NavigationContext oldContext, NavigationContext newContext)
        {
            Log.Debug("[TVWishListMP2 GUI_List]:OnPageDestroy()");
            Log.Debug("mp list window at page destroy: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
            Log.Debug("newModelId=" + newContext.WorkflowModelId.ToString());
            Log.Debug("oldModelId=" + oldContext.WorkflowModelId.ToString());

            //save data if going to a new page
            if ((newContext.WorkflowModelId != EDIT_GUI_MODEL_ID) && (newContext.WorkflowModelId != MAIN_GUI_MODEL_ID))//TVWishList  EDIT or MAIN
            {
                if (Main_GUI.Instance == null)
                {
                    Log.Error("Fatal Error: Main GUI instance is null on Result page - aborting without saving data and without unlocking");
                    return;
                }

                if (Main_GUI.Instance.TvserverdatabaseSaveSettings() == true)
                {
                    myTvWishes.MyMessageBox(4400, 1300);   //Info, TvWish data have been saved
                }
                else
                {
                    myTvWishes.MyMessageBox(4305, 1301);   //Error, TvWish data could not be saved
                }

                //Thread mythread = new Thread(SaveSettings);
                //mythread.Start(); 

                SaveSettings();
                Log.Debug("Settings have been saved");

                //*****************************************************
                //unlock TvWishList
                myTvWishes.UnLockTvWishList();
                Main_GUI.Instance.LOCKED = false;
                Log.Debug("14 mymainwindow.LOCKED=" + Main_GUI.Instance.LOCKED.ToString());
            }

            //delete filter for single TvWish entry - by default search for all tvwish messages
            mymessages.FilterName = "";

            _active = false;
        }


        public void SaveSettings()
        {
            Log.Debug("Main_GUI: SaveSettings() called");
            //Status = "Main_GUI: SaveSettings() called";

            try
            {
                ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
                TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();

                //settings.TvWishItemSeparator = TvWishItemSeparator;

                settings.Sort = mymessages.Sort;
                settings.SortReverse = mymessages.SortReverse;

                settings.Email = mymessages.Email;
                settings.Deleted = mymessages.Deleted;
                Log.Debug("mymessages.Deleted=" + mymessages.Deleted.ToString());
                settings.Conflicts = mymessages.Conflicts;
                settings.Scheduled = mymessages.Scheduled;
                settings.Recorded = mymessages.Recorded;
                Log.Debug("mymessages.Recorded=" + mymessages.Recorded.ToString());
                settings.View = mymessages.View;

                settingsManager.Save(settings);
                Log.Debug("Settings have been saved");
            }
            catch (Exception exc)
            {
                Log.Debug("Error SaveSettings: Exception " + exc.Message);
            }
        }


        #endregion OnPage

        #region Public Methods

        public void UpdateControls()
        {
            Log.Debug("ResultGUI: UpdateControls() started");
            
            myTvWishes.StatusLabel("");

            if (myTvWishes.ViewOnlyMode == false)
            {
                //Header = PluginGuiLocalizeStrings.Get(3000) + "  " + PluginGuiLocalizeStrings.Get(4200); //"TVWishList - Main-Email&Record");
                Modus = PluginGuiLocalizeStrings.Get(4200); //"TVWishList - Main-Email&Record"); 
            }
            else
            {
                //Header = PluginGuiLocalizeStrings.Get(3000) + "  " + PluginGuiLocalizeStrings.Get(4201); //"TVWishList - Main-Viewonly"); 
                Modus = PluginGuiLocalizeStrings.Get(4201); //"TVWishList - Main-Viewonly");
            }
            Log.Debug("Modus = " + Modus);
        }

        public void UpdateListItems()
        {
            Log.Debug("ResultGUI: UpdateListItems() started");

            
            //StatusLabel( "sort=" + _sort.ToString() + "  _sortreverse=" + _sortreverse.ToString());
            Log.Debug("[TVWishListMP GUI_List]:UpdateListItems  mymessages.filename=" + mymessages.filename);
            // sort messages    
            Log.Debug("mp list window before sorting messages: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
            mymessages.sortmessages(mymessages.Sort, mymessages.SortReverse);
            //filter messages
            mymessages.filtermessages(mymessages.Email, mymessages.Deleted, mymessages.Conflicts, mymessages.Scheduled, mymessages.Recorded, mymessages.View, false);
            Log.Debug("mp list window after filtering messages: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
            Log.Debug("mp list window after filtering messages: TvMessagesFiltered.Count=" + mymessages.ListAllTvMessagesFiltered().Count.ToString());

            //display messages
            

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

            _skinItemList.Clear();

            int ctr = 0;

            try
            {
                Log.Debug("[TVWishListMP GUI_List]:UpdateListItems message number found: mymessages.ListAllTvMessagesFiltered().Count=" + mymessages.ListAllTvMessagesFiltered().Count.ToString());

                foreach (xmlmessage mymessage in mymessages.ListAllTvMessagesFiltered())
                {
                    
                    string listitem = "";

                    if (_UserListItemFormat != "")
                        listitem = mymessages.FormatMessage(mymessage, _UserListItemFormat); //user defined listitem format
                    else if (myTvWishes.ViewOnlyMode == false)
                        listitem = mymessages.FormatMessage(mymessage, PluginGuiLocalizeStrings.Get(3904));  //Email listitemformat
                    else
                        listitem = mymessages.FormatMessage(mymessage, PluginGuiLocalizeStrings.Get(3905));  //View Only listitemformat

                    

                    listitem = listitem.Replace("||", "\n");
                    string[] labels = listitem.Split('\n');
                    Log.Debug("listitem=" + listitem);
                    Log.Debug("labels.Length=" + labels.Length.ToString());


                    string label = string.Empty;
                    string label2 = string.Empty;

                    if (labels.Length == 2)
                    {
                        label = labels[0];
                        label2 = labels[1];
                    }
                    else
                    {
                        label = listitem;
                    }

                    ListItem myitem = new ListItem();
                    myitem.SetLabel("Name", label);
                    myitem.SetLabel("Value", label2);
                    myitem.SetLabel("Index", ctr.ToString()); //index for myTvWishes starting with 0
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
                    ctr++;


                }

                if (mymessages.ListAllTvMessagesFiltered().Count == 0)
                {
                    ListItem myitem2 = new ListItem();
                    myitem2.SetLabel("Name", PluginGuiLocalizeStrings.Get(4301)); //"No items found"
                    _skinItemList.Add(myitem2);

                    //Skin properties update
                    TextBoxSkin = string.Empty;
                    FocusedTitle = string.Empty;
                    FocusedChannel = string.Empty;
                    FocusedStart = string.Empty;
                    FocusedEnd = string.Empty;
                    FocusedDuration = string.Empty;
                    FocusedEpisodeName = string.Empty;
                    FocusedEpisodePart = string.Empty;
                    FocusedType = string.Empty;                    
                }
                Log.Debug("[TVWishListMP]:UpdateListItems message number found:_skinItemList.Count=" + _skinItemList.Count.ToString());
            }
            catch (Exception exc)
            {
                ListItem myitem = new ListItem();
                myitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(4302));   //Error in creating item list
                _skinItemList.Add(myitem);
                Log.Error("Error in creating item list - exception was:" + exc.Message);
            }
            SkinItemList.FireChange();

        }

        public void FocusedItemChanged(ListItem focusedListItem)
        {
            if (focusedListItem == null)
            {
                //Log.Debug("ResultGUI: FocusedItemChanged: Focused item=null");
                return;
            }

            if (mymessages.ListAllTvMessagesFiltered().Count == 0)
            {
                //Log.Debug("ResultGUI: mymessages.ListAllTvMessagesFiltered().Count = 0");
                return;
            }

            try
            {
                _focusedMessage = Convert.ToInt32(focusedListItem.Labels["Index"].ToString());

                if (_focusedMessage == -1)
                    return;

                //Log.Debug("focusedMessage=" + _focusedMessage.ToString());

                
                string _TextBoxFormat = "";
                if (myTvWishes.ViewOnlyMode == false)
                {
                    if (_UserEmailFormat != "")
                    {
                        _TextBoxFormat = _UserEmailFormat;
                    }
                    else if (WideSkin)
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
                    else if (WideSkin)
                    {
                        _TextBoxFormat = _TextBoxFormat_16to9_ViewOnlyFormat;
                    }
                    else
                    {
                        _TextBoxFormat = _TextBoxFormat_4to3_ViewOnlyFormat;
                    }
                }

                string messagetext = "";
                if ((_focusedMessage >= 0) && (mymessages.ListAllTvMessagesFiltered().Count > 0))
                    messagetext = mymessages.FormatMessage(mymessages.GetTvMessageFilteredAtIndex(_focusedMessage), _TextBoxFormat);
                //string.Format(_TextBoxFormat, mymessages.TvMessagesFiltered[_focused].title, mymessages.TvMessagesFiltered[_focused].description, mymessages.TvMessages[_focused].type, mymessages.TvMessagesFiltered[_focused].start, mymessages.TvMessagesFiltered[_focused].end, mymessages.TvMessagesFiltered[_focused].channel);

                //Skin properties update
                TextBoxSkin = messagetext;

                FocusedTitle = mymessages.GetTvMessageFilteredAtIndex(_focusedMessage).title;
                FocusedChannel = mymessages.GetTvMessageFilteredAtIndex(_focusedMessage).channel;
                FocusedStart = mymessages.FormatDateTime(mymessages.GetTvMessageFilteredAtIndex(_focusedMessage).start);
                //Log.Debug("FocusedStart=" + FocusedStart);
                FocusedEnd = mymessages.FormatDateTime(mymessages.GetTvMessageFilteredAtIndex(_focusedMessage).end);
                //Log.Debug("FocusedEnd=" + FocusedEnd);
                TimeSpan duration = mymessages.GetTvMessageFilteredAtIndex(_focusedMessage).end - mymessages.GetTvMessageFilteredAtIndex(_focusedMessage).start;
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



                //Log.Debug("Duration=" + durationFormat);
                FocusedDuration = durationFormat;
                FocusedEpisodePart = mymessages.GetTvMessageFilteredAtIndex(_focusedMessage).EpisodePart;
                FocusedEpisodeName = mymessages.GetTvMessageFilteredAtIndex(_focusedMessage).EpisodeName;
                FocusedType = mymessages.TypeTranslation(mymessages.GetTvMessageFilteredAtIndex(_focusedMessage).type.ToString());
            }
            catch (Exception exc)
            {
                Log.Error("[TVWishListMP GUI_List]:OnMessage Error in format string: Exception=" + exc.Message);
            }  
        }

        public void SelectedItemChanged(ListItem selectedListItem)
        {
            //no button lock - already done
            if (selectedListItem == null)
            {
                Log.Debug("ResultGUI: FocusedItemChanged: Focused item=null");
                return;
            }

            if (mymessages.ListAllTvMessagesFiltered().Count == 0)
                return;

            Log.Debug("ResultGUI: SelectedItemChanged: Selected item number=" + selectedListItem.Labels["Name"]);

            if (_focusedMessage == -1)
                return;

            SelectedItemChangedprocessing();
        }


        public void SelectedItemChangedprocessing()
        {
            try
            {

                mymessage = mymessages.GetTvMessageFilteredAtIndex(_focusedMessage);
                Log.Debug("_focusedMessage=" + _focusedMessage.ToString());
                Log.Debug("mymessage.title="+mymessage.title);
                Log.Debug("mymessage.start=" + mymessage.start.ToString());
               

                _dialogMenuItemList.Clear();

                DialogHeader = PluginGuiLocalizeStrings.Get(3200);

                ListItem myitem1 = new ListItem();
                myitem1.SetLabel("Name", PluginGuiLocalizeStrings.Get(3500)); //Goto TV (1)
                myitem1.Command = new MethodDelegateCommand(() =>
                {
                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                    mycommandscreenManager.CloseTopmostDialog();
                    EvaluateSelectedItem(mymessage, (int)MenuItems.GotoTv);
                });
                _dialogMenuItemList.Add(myitem1);
                
                ListItem myitem2 = new ListItem();
                myitem2.SetLabel("Name", PluginGuiLocalizeStrings.Get(1053)); //Create TvWish with Title in Email Mode
                myitem2.Command = new MethodDelegateCommand(() =>
                {
                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                    mycommandscreenManager.CloseTopmostDialog();
                    EvaluateSelectedItem(mymessage, (int)MenuItems.CreateTvWishEmail);
                });
                _dialogMenuItemList.Add(myitem2);

                ListItem myitem3 = new ListItem();
                myitem3.SetLabel("Name", PluginGuiLocalizeStrings.Get(1054)); //Create TvWish with Title in ViewOnly Mode
                myitem3.Command = new MethodDelegateCommand(() =>
                {
                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                    mycommandscreenManager.CloseTopmostDialog();
                    EvaluateSelectedItem(mymessage, (int)MenuItems.CreateTvWishViewOnly);
                });
                _dialogMenuItemList.Add(myitem3);
                
                ListItem myitem4 = new ListItem();
                myitem4.SetLabel("Name", PluginGuiLocalizeStrings.Get(1055)); //Create TvWish with All in Email Mode
                myitem4.Command = new MethodDelegateCommand(() =>
                {
                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                    mycommandscreenManager.CloseTopmostDialog();
                    EvaluateSelectedItem(mymessage, (int)MenuItems.CreateTvWishAllEmail);
                });
                _dialogMenuItemList.Add(myitem4);
                
                ListItem  myitem5 = new ListItem();
                myitem5.SetLabel("Name", PluginGuiLocalizeStrings.Get(1056)); //Create TvWish with All in ViewOnly Mode
                myitem5.Command = new MethodDelegateCommand(() =>
                {
                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                    mycommandscreenManager.CloseTopmostDialog();
                    EvaluateSelectedItem(mymessage, (int)MenuItems.CreateTvWishAllViewOnly);
                });
                _dialogMenuItemList.Add(myitem5);
                
                if ((mymessage.type == MessageType.Deleted.ToString()) || (mymessage.type == MessageType.Conflict.ToString()))
                {
                    ListItem myitem6 = new ListItem();
                    myitem6.SetLabel("Name", PluginGuiLocalizeStrings.Get(3503)); //Delete message
                    myitem6.Command = new MethodDelegateCommand(() =>
                    {
                        IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                        mycommandscreenManager.CloseTopmostDialog();
                        EvaluateSelectedItem(mymessage, (int)MenuItems.DeleteMessage);
                    });
                    _dialogMenuItemList.Add(myitem6);
                    
                }
                else if ((mymessage.type == MessageType.Emailed.ToString()) || (mymessage.type == MessageType.Viewed.ToString()) || (mymessage.type == MessageType.Scheduled.ToString()))
                {
                    ListItem myitem6 = new ListItem();
                    myitem6.SetLabel("Name", PluginGuiLocalizeStrings.Get(3503)); //Delete message
                    myitem6.Command = new MethodDelegateCommand(() =>
                    {
                        IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                        mycommandscreenManager.CloseTopmostDialog();
                        EvaluateSelectedItem(mymessage, (int)MenuItems.DeleteMessage);
                    });
                    _dialogMenuItemList.Add(myitem6);
                    

                    bool scheduleExists = false;
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
                        ListItem myitem7 = new ListItem();
                        myitem7.SetLabel("Name", PluginGuiLocalizeStrings.Get(3501)); //Delete Schedule
                        myitem7.Command = new MethodDelegateCommand(() =>
                        {
                            IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                            mycommandscreenManager.CloseTopmostDialog();
                            EvaluateSelectedItem(mymessage, (int)MenuItems.DeleteSchedule);
                        });
                        _dialogMenuItemList.Add(myitem7);
                        
                    }
                    else
                    {//Schedule
                        // add prerecord, postrecord, keepuntil, priority, 
                        // change public void SelectedItemChanged(ListItem selectedListItem) in Common_Edit_AND_ConfigDefaults.cs

                        //int index =Convert.ToInt32(mymessage.tvwishid);
                        //Log.Debug("index=" + index.ToString());
                        TvWish mywish = myTvWishes.GetAtTvWishId(mymessage.tvwishid);
                        Log.Debug(" mywish.searchfor=" + mywish.searchfor);
                        Log.Debug(" mywish.tvwishid=" + mywish.tvwishid);

                        myTvWishes.FocusedWishIndex = myTvWishes.GetIndex(mywish);
                        Log.Debug("myTvWishes.FocusedWishIndex=" + myTvWishes.FocusedWishIndex.ToString());

                        ListItem myitem7 = new ListItem();
                        myitem7.SetLabel("Name", PluginGuiLocalizeStrings.Get(3502)); //Schedule
                        myitem7.Command = new MethodDelegateCommand(() =>
                        {
                            IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                            mycommandscreenManager.CloseTopmostDialog();
                            EvaluateSelectedItem(mymessage, (int)MenuItems.Schedule);
                        });
                        _dialogMenuItemList.Add(myitem7);
                        

                        ListItem myitem8 = new ListItem(); //Prerecord
                        string mystring = String.Format(PluginGuiLocalizeStrings.Get(2508), mywish.prerecord);
                        Log.Debug("initial: mystring=" + mystring);
                        myitem8.SetLabel("Name", mystring); //Prerecord
                        myitem8.Command = new MethodDelegateCommand(() =>
                        {
                            IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                            mycommandscreenManager.CloseTopmostDialog();
                            CaseSelection = (int)TvWishEntries.prerecord;
                            EvaluateSelectedItem(mymessage, (int)MenuItems.Prerecord);
                        });
                        _dialogMenuItemList.Add(myitem8);
                        

                        ListItem myitem9 = new ListItem(); //Postrecord
                        mystring = String.Format(PluginGuiLocalizeStrings.Get(2509), mywish.postrecord);
                        Log.Debug("initial: mystring=" + mystring);
                        myitem9.SetLabel("Name", mystring); //Postrecord
                        myitem9.Command = new MethodDelegateCommand(() =>
                        {
                            IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                            mycommandscreenManager.CloseTopmostDialog();
                            CaseSelection = (int)TvWishEntries.postrecord;
                            EvaluateSelectedItem(mymessage, (int)MenuItems.Postrecord);
                        });
                        _dialogMenuItemList.Add(myitem9);
                        

                        ListItem myitem10 = new ListItem(); //Keepuntil
                        mystring = String.Format(PluginGuiLocalizeStrings.Get(2515), mywish.keepuntil);
                        Log.Debug("initial: mystring=" + mystring);
                        myitem10.SetLabel("Name", mystring); //Keepuntil
                        myitem10.Command = new MethodDelegateCommand(() =>
                        {
                            IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                            mycommandscreenManager.CloseTopmostDialog();
                            CaseSelection = (int)TvWishEntries.keepuntil;
                            EvaluateSelectedItem(mymessage, (int)MenuItems.Keepuntil);
                        });
                        _dialogMenuItemList.Add(myitem10);
                        

                    }
                                       
                }
                else if (mymessage.type == MessageType.Recorded.ToString())
                {
                    ListItem myitem6 = new ListItem();
                    myitem6.SetLabel("Name", PluginGuiLocalizeStrings.Get(3504)); //Delete recording
                    myitem6.Command = new MethodDelegateCommand(() =>
                    {
                        IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                        mycommandscreenManager.CloseTopmostDialog();
                        EvaluateSelectedItem(mymessage, (int)MenuItems.DeleteRecording);
                    });
                    _dialogMenuItemList.Add(myitem6);
                    

                    ListItem myitem7 = new ListItem();
                    myitem7.SetLabel("Name", PluginGuiLocalizeStrings.Get(3505)); //Goto recordings
                    myitem7.Command = new MethodDelegateCommand(() =>
                    {
                        IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                        mycommandscreenManager.CloseTopmostDialog();
                        EvaluateSelectedItem(mymessage, (int)MenuItems.GotoVideo);
                    });
                    _dialogMenuItemList.Add(myitem7);
                    
                }

                //update for dialog skin
                DialogMenuItemList.FireChange();

                //will now call a dialogbox with a given menu            
                ScreenManager.ShowDialog(TVWISHLIST_RESULT_DIALOG_MENU_SCREEN);
            }
            catch (Exception exc)
            {
                Log.Error("[TVWishListMP2 GUI_List]:SelectedItemChanged failed exception: " + exc.Message);
            }

        }

        /*
        public void ItemProcessing(int caseSelection)
        {
            try
            {
                
                TvWish mywish = myTvWishes.GetAtTvWishId(myTvWishes.FocusedWishIndex);//from main menu selected

                //List<string> menulist2 = new List<string>();


                

                switch (caseSelection)
                {
                    //no button lock - already done
                    case (int)TvWishEntries.prerecord:
                        _dialogMenuItemList2.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2808);  //Prerecord

                        string[] allItemStrings = new string[] { "0", "5", "8", "10", "15", "30", PluginGuiLocalizeStrings.Get(4102) }; //Custom
                        foreach (string itemstring in allItemStrings)
                        {
                            ListItem mydialogitem2 = new ListItem();
                            mydialogitem2.SetLabel("Name", itemstring);
                            mydialogitem2.Command = new MethodDelegateCommand(() =>
                            {
                                if (mydialogitem2.Labels["Name"].ToString() == PluginGuiLocalizeStrings.Get(4102)) //Custom
                                {
                                    Log.Debug("x1");
                                    InputHeader = PluginGuiLocalizeStrings.Get(2808); //Prerecord
                                    Log.Debug("x2");
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    Log.Debug("x3");
                                    InputTextBoxSkin = mywish.prerecord;
                                    Log.Debug("x4");
                                    try
                                    {
                                        int k = Convert.ToInt32(InputTextBoxSkin);
                                        //Log.Debug("k=" + k.ToString());
                                    }
                                    catch //do nothing and use default
                                    {
                                        InputTextBoxSkin = "5";
                                    }
                                    Log.Debug("x5");
                                    mywish.prerecord = InputTextBoxSkin;

                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                    ScreenManager.ShowDialog(TVWISHLIST_RESULT_INPUT_TEXTBOX_SCREEN);
                                }
                                else
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.prerecord = mydialogitem2.Labels["Name"].ToString();
                                    Log.Debug("ItemProcessing(int caseSelection):   mywish.prerecord=" + mywish.prerecord);
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                    Log.Debug("ItemProcessing(int caseSelection):   mywish.prerecord=" + mywish.prerecord);



                                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                    mycommandscreenManager.CloseTopmostDialog();
                                    UpdateListItems();

                                    mycommandscreenManager.CloseTopmostDialog();

                                    SelectedItemChangedprocessing();
                                }

                                

                                //Thread myselectthread = new Thread(SelectItemUpdate);
                                //myselectthread.Start();
                            });
                            _dialogMenuItemList2.Add(mydialogitem2);
                        }
                        //update for dialog skin
                        DialogMenuItemList2.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_RESULT_DIALOG_MENU_SCREEN2);
                        break;


                }//end switch




            }

            catch (Exception exc)
            {

                Log.Error("[TVWishList GUI_Edit]:ItemSelectionChanged: ****** Exception " + exc.Message);

            }
        }
        */


        public void EvaluateInputTextBoxSave()
        {
            TvWish mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);

            switch (CaseSelection)
            {
                //no button lock - already done
                case (int)TvWishEntries.prerecord:

                    mywish.prerecord = InputTextBoxSkin.ToString();
                    Log.Debug("ItemProcessing(int caseSelection):   mywish.prerecord=" + mywish.prerecord);
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    Log.Debug("ItemProcessing(int caseSelection):   mywish.prerecord=" + mywish.prerecord);                 
                    break;

                case (int)TvWishEntries.postrecord:

                    mywish.postrecord = InputTextBoxSkin.ToString();
                    Log.Debug("ItemProcessing(int caseSelection):   mywish.postrecord=" + mywish.postrecord);
                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                    Log.Debug("ItemProcessing(int caseSelection):   mywish.postrecord=" + mywish.postrecord);
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
                
            }
            
            Thread mainDialogThread = new Thread(StartMainDialogThread);
            mainDialogThread.Start();
                    
        }

        public void EvaluateInputTextBoxAbort()
        {
            Thread mainDialogThread = new Thread(StartMainDialogThread);
            mainDialogThread.Start();
        }

        public void EvaluateSelectedItem(xmlmessage mymessage,int casenumber)
        {
            //button lock
            if (myTvWishes.ButtonActive)
            {
                myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Operation Is Still Running"
                return;
            }
            myTvWishes.ButtonActive = true; //lock buttons


            try
            {
                int tvMessageindex = mymessage.unfiltered_index;
                Log.Debug("tvMessageindex filtered=" + _focusedMessage.ToString());
                Log.Debug("tvMessageindex unfiltered=" + tvMessageindex.ToString());


                
                //possible bug can cause exception!!!!!!!!!!!!!!!!
                TvWish mywish = myTvWishes.RetrieveById(mymessage.tvwishid);

                //TvWish mywish = myTvWishes.GetAtTvWishId(myTvWishes.FocusedWishIndex);

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




                IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();

                switch (casenumber)
                {

                    case (int)MenuItems.GotoTv: //Goto Tv main screen

                        try
                        {
                            if (workflowManager.NavigatePopModel(TV_GUI_MODEL_ID))
                            {// do nothing

                            }
                            else
                            {
                                workflowManager.NavigatePopToState(HOME_GUI_STATE_ID, true);
                                workflowManager.NavigatePush(TV_GUI_STATE_ID);
                            }
                        }
                        catch
                        {
                            Log.Error("Workflow navigation to TV failed");
                        }





                        break;

                    case (int)MenuItems.CreateTvWishEmail://Create TvWish with Title in Email Mode
                        try
                        {
                            string parameter = Main_GUI.SkinCommands.NEWTVWISH_EMAIL.ToString() + "//";
                            parameter += "TITLE=" + mymessage.title;
                            Log.Debug("LIST: parameter=" + parameter);

                            if (Main_GUI.Instance == null)
                            {
                                Log.Error("Fatal Error: Main_GUI instance did not exist in Result_GUI case2 - going back");
                                return;
                            }
                            Main_GUI.Instance.Parameter = parameter;
                            Main_GUI.Instance.TvWishListQuickMenu = true;
                            if (PreviousModel == EDIT_GUI_MODEL_ID_STR)
                            {
                                workflowManager.NavigatePop(2); //2 stages to main
                            }
                            else
                            {
                                workflowManager.NavigatePop(1);//1 stage to main
                            }

                        }
                        catch (Exception exc)
                        {
                            Log.Error("Create Tvwish with Title failed - Exception error " + exc.Message);
                        }

                        break;

                    case (int)MenuItems.CreateTvWishViewOnly://Create TvWish with Title in ViewOnly Mode
                        try
                        {
                            string parameter = Main_GUI.SkinCommands.NEWTVWISH_VIEWONLY.ToString() + "//";
                            parameter += "TITLE=" + mymessage.title;
                            Log.Debug("LIST: parameter=" + parameter);

                            if (Main_GUI.Instance == null)
                            {
                                Log.Error("Fatal Error: Main_GUI instance did not exist in Result_GUI case2 - going back");
                                return;
                            }
                            Main_GUI.Instance.Parameter = parameter;
                            Main_GUI.Instance.TvWishListQuickMenu = true;
                            if (PreviousModel == EDIT_GUI_MODEL_ID_STR)
                            {
                                workflowManager.NavigatePop(2); //2 stages to main
                            }
                            else //must be main
                            {
                                workflowManager.NavigatePop(1);//1 stage to main
                            }
                        }
                        catch (Exception exc)
                        {
                            Log.Error("Create Tvwish with Title failed - Exception error " + exc.Message);
                        }

                        break;

                    case (int)MenuItems.CreateTvWishAllEmail: //Create TvWish with All in Email Mode

                        try
                        {
                            string parameter = Main_GUI.SkinCommands.NEWTVWISH_ALL_EMAIL.ToString() + "//";

                            parameter += "TITLE=" + mymessage.title + "//";
                            parameter += "CHANNEL=" + mymessage.channel + "//";
                            parameter += "EPISODEPART=" + mymessage.EpisodePart + "//";
                            parameter += "EPISODENAME=" + mymessage.EpisodeName + "//";
                            parameter += "SERIESNUMBER=" + mymessage.SeriesNum + "//";
                            parameter += "EPISODENUMBER=" + mymessage.EpisodeNum;

                            Log.Debug("LIST: parameter=" + parameter);

                            if (Main_GUI.Instance == null)
                            {
                                Log.Error("Fatal Error: Main_GUI instance did not exist in Result_GUI case2 - going back");
                                return;
                            }
                            Main_GUI.Instance.Parameter = parameter;
                            Main_GUI.Instance.TvWishListQuickMenu = true;
                            if (PreviousModel == EDIT_GUI_MODEL_ID_STR)
                            {
                                workflowManager.NavigatePop(2); //2 stages to main
                            }
                            else
                            {
                                workflowManager.NavigatePop(1);//1 stage to main
                            }
                        }
                        catch (Exception exc)
                        {
                            Log.Error("Create Tvwish with All failed - Exception error " + exc.Message);
                        }
                        break;

                    case (int)MenuItems.CreateTvWishAllViewOnly: //Create TvWish with All in ViewOnly Mode

                        try
                        {
                            string parameter = Main_GUI.SkinCommands.NEWTVWISH_ALL_VIEWONLY.ToString() + "//";

                            parameter += "TITLE=" + mymessage.title + "//";
                            parameter += "CHANNEL=" + mymessage.channel + "//";
                            parameter += "EPISODEPART=" + mymessage.EpisodePart + "//";
                            parameter += "EPISODENAME=" + mymessage.EpisodeName + "//";
                            parameter += "SERIESNUMBER=" + mymessage.SeriesNum + "//";
                            parameter += "EPISODENUMBER=" + mymessage.EpisodeNum;

                            Log.Debug("LIST: parameter=" + parameter);

                            if (Main_GUI.Instance == null)
                            {
                                Log.Error("Fatal Error: Main_GUI instance did not exist in Result_GUI case2 - going back");
                                return;
                            }
                            Main_GUI.Instance.Parameter = parameter;
                            Main_GUI.Instance.TvWishListQuickMenu = true;
                            if (PreviousModel == EDIT_GUI_MODEL_ID_STR)
                            {
                                workflowManager.NavigatePop(2); //2 stages to main
                            }
                            else
                            {
                                workflowManager.NavigatePop(1);//1 stage to main
                            }
                        }
                        catch (Exception exc)
                        {
                            Log.Error("Create Tvwish with All failed - Exception error " + exc.Message);
                        }
                        break;

                    case (int)MenuItems.DeleteMessage: //Deletemessage

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
                        break;


                    case (int)MenuItems.DeleteSchedule: //Delete Schedule
                        if (foundSchedule != null) //delete schedule and found item
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
                        break;

                    case (int)MenuItems.Schedule: //Schedule

                        try
                        {
                            if ((_focusedMessage >= 0) && (mymessages.ListAllTvMessagesFiltered().Count > 0))
                            {
                                // messagetext = mymessages.FormatMessage(mymessages.TvMessagesFiltered[_focused], _TextBoxFormat);

                                Log.Debug("Before Schedule added - Count=" + Schedule.ListAll().Count.ToString());

                                Schedule schedule = layer.AddSchedule(mymessage.channel_id, mymessage.title, mymessage.start, mymessage.end, 0);
                                

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
                                    //add manual recording message of type scheduled
                                }

                                schedule.Persist();
                                Log.Debug("Schedule added");
                                Log.Debug("Schedule added - Count="+Schedule.ListAll().Count.ToString());
                                // Old Bug: must not generate new message for manual scheduling  (or use -1 for tvwishid)
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
                        break;

                    case (int)MenuItems.DeleteRecording: //Delete recording
                        if (foundRecording != null) //delete recording and found item
                        {
                            IDialogManager mydialog = ServiceRegistration.Get<IDialogManager>();
                            Guid mydialogId = mydialog.ShowDialog(PluginGuiLocalizeStrings.Get(4401), PluginGuiLocalizeStrings.Get(3608).Replace(@"\n",string.Empty)+" "+mymessage.filename, DialogType.YesNoDialog, false, DialogButtonType.No);
                            DialogCloseWatcher dialogCloseWatcher = new DialogCloseWatcher(this, mydialogId, dialogResult =>
                            { //this is watching the result of the dialog box and displaying in the Status label of the screen (do not forget to dispose)
                                if (dialogResult.ToString() == DialogButtonType.Yes.ToString())
                                {
                                    try
                                    {
                                        try
                                        {
                                            File.Delete(mymessage.filename);
                                            Log.Info("File deleted: " + mymessage.filename);
                                        }
                                        catch { }//do nothing
                                        //MessageBox.Show("File deleted: " + mymessage.message);
                                        
                                        Log.Debug("tvMessageindex=" + tvMessageindex);
                                        Log.Debug("mymessages.ListAllTvMessages().Count=" + mymessages.ListAllTvMessages().Count.ToString());
                                        Log.Debug("mymessages.ListAllTvMessagesFiltered().Count=" + mymessages.ListAllTvMessagesFiltered().Count.ToString());
                                        xmlmessage mydeletedmessage = mymessages.GetTvMessageAtIndex(tvMessageindex); //this is the unfiltered index
                                        mydeletedmessage.type = MessageType.Deleted.ToString();
                                        mydeletedmessage.message = String.Format(PluginGuiLocalizeStrings.Get(3651));  //File has been deleted;
                                        mydeletedmessage.created = DateTime.Now;
                                        mymessages.ReplaceTvMessageAtIndex(tvMessageindex, mydeletedmessage);
                                        //mymessages.DeleteTvMessageAt(tvMessageindex);

                                        Log.Debug("Delete Message modified");


                                        //mymessages.DeleteTvMessageAt(tvMessageindex);
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

                                return true;
                            });

                        }//end delete recording



                        break;



                    case (int)MenuItems.GotoVideo:  //Goto Videos
                        try
                        {
                            if (workflowManager.NavigatePopModel(VIDEO_GUI_MODEL_ID))
                            {// do nothing

                            }
                            else
                            {
                                workflowManager.NavigatePopToState(HOME_GUI_STATE_ID, true);
                                workflowManager.NavigatePush(VIDEO_GUI_STATE_ID);
                            }
                        }
                        catch
                        {
                            Log.Error("Workflow navigation to Videos failed");
                        }

                        break;



                    case (int)MenuItems.Prerecord:  //prerecord
                        _dialogMenuItemList2.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2808);  //Prerecord

                        string[] allItemStrings = new string[] { "0", "5", "8", "10", "15", "30", PluginGuiLocalizeStrings.Get(4102) }; //Custom
                        foreach (string itemstring in allItemStrings)
                        {
                            ListItem mydialogitem2 = new ListItem();
                            mydialogitem2.SetLabel("Name", itemstring);
                            mydialogitem2.Command = new MethodDelegateCommand(() =>
                            {
                                if (mydialogitem2.Labels["Name"].ToString() == PluginGuiLocalizeStrings.Get(4102)) //Custom
                                {
                                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                    mycommandscreenManager.CloseTopmostDialog();
                                    InputHeader = PluginGuiLocalizeStrings.Get(2808); //Prerecord
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    InputTextBoxSkin = mywish.prerecord;
                                    try
                                    {
                                        int k = Convert.ToInt32(InputTextBoxSkin);
                                        //Log.Debug("k=" + k.ToString());
                                    }
                                    catch //do nothing and use default
                                    {
                                        InputTextBoxSkin = "5";
                                    }
                                    mywish.prerecord = InputTextBoxSkin;

                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                    ScreenManager.ShowDialog(TVWISHLIST_RESULT_INPUT_TEXTBOX_SCREEN);
                                }
                                else
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.prerecord = mydialogitem2.Labels["Name"].ToString();
                                    Log.Debug("ItemProcessing(int caseSelection):   mywish.prerecord=" + mywish.prerecord);
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                    Log.Debug("ItemProcessing(int caseSelection):   mywish.prerecord=" + mywish.prerecord);



                                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                    mycommandscreenManager.CloseTopmostDialog();
                                    UpdateListItems();

                                    

                                    //SelectedItemChangedprocessing();
                                    Thread mainDialogThread = new Thread(StartMainDialogThread);
                                    mainDialogThread.Start();
                                }

                                

                                //Thread myselectthread = new Thread(SelectItemUpdate);
                                //myselectthread.Start();
                            });
                            _dialogMenuItemList2.Add(mydialogitem2);
                        }
                        //update for dialog skin
                        DialogMenuItemList2.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_RESULT_DIALOG_MENU_SCREEN2);
                        break;



                    case (int)MenuItems.Postrecord:  //postrecord
                        _dialogMenuItemList2.Clear();

                        DialogHeader = PluginGuiLocalizeStrings.Get(2809);  //Postrecord

                        allItemStrings = new string[] { "0", "5", "8", "10", "15", "30", PluginGuiLocalizeStrings.Get(4102) }; //Custom
                        foreach (string itemstring in allItemStrings)
                        {
                            ListItem mydialogitem2 = new ListItem();
                            mydialogitem2.SetLabel("Name", itemstring);
                            mydialogitem2.Command = new MethodDelegateCommand(() =>
                            {
                                if (mydialogitem2.Labels["Name"].ToString() == PluginGuiLocalizeStrings.Get(4102)) //Custom
                                {
                                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                    mycommandscreenManager.CloseTopmostDialog();
                                    InputHeader = PluginGuiLocalizeStrings.Get(2809); //Postrecord
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    InputTextBoxSkin = mywish.postrecord;
                                    try
                                    {
                                        int k = Convert.ToInt32(InputTextBoxSkin);
                                        //Log.Debug("k=" + k.ToString());
                                    }
                                    catch //do nothing and use default
                                    {
                                        InputTextBoxSkin = "5";
                                    }
                                    mywish.postrecord = InputTextBoxSkin;

                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                    ScreenManager.ShowDialog(TVWISHLIST_RESULT_INPUT_TEXTBOX_SCREEN);
                                }
                                else
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.postrecord = mydialogitem2.Labels["Name"].ToString();
                                    Log.Debug("ItemProcessing(int caseSelection):   mywish.postrecord=" + mywish.postrecord);
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                    Log.Debug("ItemProcessing(int caseSelection):   mywish.postrecord=" + mywish.postrecord);



                                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                    mycommandscreenManager.CloseTopmostDialog();
                                    UpdateListItems();



                                    //SelectedItemChangedprocessing();
                                    Thread mainDialogThread = new Thread(StartMainDialogThread);
                                    mainDialogThread.Start();
                                }



                                //Thread myselectthread = new Thread(SelectItemUpdate);
                                //myselectthread.Start();
                            });
                            _dialogMenuItemList2.Add(mydialogitem2);
                        }
                        //update for dialog skin
                        DialogMenuItemList2.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_RESULT_DIALOG_MENU_SCREEN2);
                        break;


                    case (int)MenuItems.Keepuntil:  //keepuntil
                        _dialogMenuItemList2.Clear();
                         
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
                                    //myselectthread = new Thread(SelectItemUpdate);
                                    //myselectthread.Start();
                                }
                                else if (index == 1) //days after recording
                                {
                                    DialogHeader = PluginGuiLocalizeStrings.Get(2901);  // Days After Recording

                                    _dialogMenuItemList3.Clear();
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
                                                CaseSelection = (int)TvWishEntries.keepuntil;
                                                ScreenManager.ShowDialog(TVWISHLIST_RESULT_INPUT_TEXTBOX_SCREEN);
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
                                            //myselectthread = new Thread(SelectItemUpdate);
                                            //myselectthread.Start();
                                        });
                                        _dialogMenuItemList3.Add(mydialogitem2);
                                    }
                                    //update for dialog skin
                                    DialogMenuItemList3.FireChange();
                                    //will now call a dialogbox with a given menu            
                                    ScreenManager.ShowDialog(TVWISHLIST_RESULT_DIALOG_MENU_SCREEN3);

                                }//days after recording

                                else if (index == 2) //weeks after recording
                                {
                                    _dialogMenuItemList3.Clear();

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
                                                CaseSelection = (int)TvWishEntries.keepuntil;
                                                ScreenManager.ShowDialog(TVWISHLIST_RESULT_INPUT_TEXTBOX_SCREEN);
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
                                            //myselectthread = new Thread(SelectItemUpdate);
                                            //myselectthread.Start();
                                        });
                                        _dialogMenuItemList3.Add(mydialogitem2);
                                    }
                                    //update for dialog skin
                                    DialogMenuItemList3.FireChange();
                                    //will now call a dialogbox with a given menu            
                                    ScreenManager.ShowDialog(TVWISHLIST_RESULT_DIALOG_MENU_SCREEN3);

                                }//weeks after recording
                                else if (index == 3) //months after recording
                                {
                                    _dialogMenuItemList3.Clear();

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
                                                CaseSelection = (int)TvWishEntries.keepuntil;
                                                ScreenManager.ShowDialog(TVWISHLIST_RESULT_INPUT_TEXTBOX_SCREEN);
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
                                            //myselectthread = new Thread(SelectItemUpdate);
                                            //myselectthread.Start();
                                        });
                                        _dialogMenuItemList3.Add(mydialogitem2);
                                    }
                                    //update for dialog skin
                                    DialogMenuItemList3.FireChange();
                                    //will now call a dialogbox with a given menu            
                                    ScreenManager.ShowDialog(TVWISHLIST_RESULT_DIALOG_MENU_SCREEN3);

                                }//months after recording
                                else if (index == 4) //date
                                {
                                    InputHeader = PluginGuiLocalizeStrings.Get(2815) + " " + PluginGuiLocalizeStrings.Get(2923); //Keep Until date format
                                    InputTextBoxSkin = mywish.keepuntil;
                                    CaseSelection = (int)TvWishEntries.skip;// cheat with skip
                                    ScreenManager.ShowDialog(TVWISHLIST_RESULT_INPUT_TEXTBOX_SCREEN);
                                }
                                else if (index == 5) //until watched
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.keepuntil = PluginGuiLocalizeStrings.Get(2905); //watched
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                    UpdateListItems();
                                    //myselectthread = new Thread(SelectItemUpdate);
                                    //myselectthread.Start();
                                }
                                else if (index == 6) //until space needed
                                {
                                    mywish = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex);
                                    mywish.keepuntil = PluginGuiLocalizeStrings.Get(2906); //Space                                 
                                    myTvWishes.ReplaceAtIndex(myTvWishes.FocusedWishIndex, mywish);
                                    UpdateListItems();
                                    //myselectthread = new Thread(SelectItemUpdate);
                                    //myselectthread.Start();
                                }
                                Log.Debug("mywish.keepuntil=" + mywish.keepuntil);

                                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                                mycommandscreenManager.CloseTopmostDialog();
                                Log.Debug("First Dialog is closed");

                            });
                            _dialogMenuItemList2.Add(mydialogitem);
                        }
                        //update for dialog skin
                        DialogMenuItemList2.FireChange();
                        //will now call a dialogbox with a given menu            
                        ScreenManager.ShowDialog(TVWISHLIST_RESULT_DIALOG_MENU_SCREEN2);

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


                        
                }
                 
            }
            catch (Exception exc)
            {
                Log.Error("[TVWishListMP GUI_List]:ItemSelected failed exception: " + exc.Message);
            }

            myTvWishes.ButtonActive = false; //unlock buttons
        }

        

        public void StartMainDialogThread()
        {
            SelectedItemChangedprocessing();//start new main dialog
        }


        public void SortCriteria()
        {
            Log.Debug("ResultGUI: SortCriteria() started");

            //no button lock
     
            _dialogMenuItemList.Clear();
            DialogHeader = PluginGuiLocalizeStrings.Get(3200);

            for (int i = 1; i <= 13; i++)
            {
                ListItem myitem = new ListItem();
                // now we define numbers for changing the postrecord setting of the tv server database
                myitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(i + 3200));
                myitem.SetLabel("Index", i.ToString());
                myitem.Command = new MethodDelegateCommand(() =>
                {
                    //button lock
                    if (myTvWishes.ButtonActive)
                    {
                        myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Operation Is Still Running"
                        return;
                    }
                    myTvWishes.ButtonActive = true; //lock buttons

                    mymessages.Sort = Convert.ToInt32(myitem.Labels["Index"].ToString());
                    Log.Debug("_sort=" + mymessages.Sort.ToString());                    
                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                    mycommandscreenManager.CloseTopmostDialog();
                    UpdateListItems();
                    myTvWishes.ButtonActive = false;
                });
                _dialogMenuItemList.Add(myitem);
            }

            //update for dialog skin
            DialogMenuItemList.FireChange();

            //will now call a dialogbox with a given menu            
            ScreenManager.ShowDialog(TVWISHLIST_RESULT_DIALOG_MENU_SCREEN);           
        }

        public void SortOrder()
        {
            //button lock
            if (myTvWishes.ButtonActive)
            {
                myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Operation Is Still Running"
                return;
            }
            myTvWishes.ButtonActive = true; //lock buttons

            Log.Debug("ResultGUI: Sort() started");
            mymessages.SortReverse = !mymessages.SortReverse;
            UpdateListItems();

            if (!mymessages.SortReverse)
                Status = PluginGuiLocalizeStrings.Get(3652);  //3652 Sort order is ascending
            else
                Status = PluginGuiLocalizeStrings.Get(3653);  //3653 Sort order is descending

            myTvWishes.ButtonActive = false; //unlock buttons
        }
        
        public void Filter()
        {
            //button lock
            if (myTvWishes.ButtonActive)
            {
                myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Operation Is Still Running"
                return;
            }
            myTvWishes.ButtonActive = true; //lock buttons

            Log.Debug("ResultGUI: Filter() started");

            
            _dialogMenuItemList.Clear();

            DialogHeader = PluginGuiLocalizeStrings.Get(3250);

            ListItem mydialogitem0 = new ListItem();
            if (mymessages.View == true)
            {
                mydialogitem0.SetLabel("Name", PluginGuiLocalizeStrings.Get(3251)); //"View"
            }
            else
            {
                mydialogitem0.SetLabel("Name", PluginGuiLocalizeStrings.Get(3252)); //"No View"
            }
            mydialogitem0.Command = new MethodDelegateCommand(() =>
            {
                if (mymessages.View == false)
                {
                    _dialogMenuItemList[0].SetLabel("Name", PluginGuiLocalizeStrings.Get(3251)); //"View"
                }
                else
                {
                    _dialogMenuItemList[0].SetLabel("Name", PluginGuiLocalizeStrings.Get(3252)); //"No View"
                }
                mymessages.View = !mymessages.View;
                DialogMenuItemList.FireChange();
                //UpdateListItems();
                SelectedDialogItem = 0;
                Thread myselectthread = new Thread(SelectFilterItemUpdate);
                myselectthread.Start();
            });
            _dialogMenuItemList.Add(mydialogitem0);



            ListItem mydialogitem1 = new ListItem();
            if (mymessages.Email == true)
            {
                mydialogitem1.SetLabel("Name", PluginGuiLocalizeStrings.Get(3253)); //"Email"
            }
            else
            {
                mydialogitem1.SetLabel("Name", PluginGuiLocalizeStrings.Get(3254)); //"No Email"
            }
            mydialogitem1.Command = new MethodDelegateCommand(() =>
            {
                if (mymessages.Email == false)
                {
                    _dialogMenuItemList[1].SetLabel("Name", PluginGuiLocalizeStrings.Get(3253)); //"Email"
                }
                else
                {
                    _dialogMenuItemList[1].SetLabel("Name", PluginGuiLocalizeStrings.Get(3254)); //"No Email"
                }                            
                mymessages.Email = !mymessages.Email;
                DialogMenuItemList.FireChange();
                //UpdateListItems();
                SelectedDialogItem = 1;
                Thread myselectthread = new Thread(SelectFilterItemUpdate);
                myselectthread.Start();
            });
            _dialogMenuItemList.Add(mydialogitem1);


            ListItem mydialogitem2 = new ListItem();
            if (mymessages.Scheduled == true)
            {
                mydialogitem2.SetLabel("Name", PluginGuiLocalizeStrings.Get(3255)); //"Scheduled"
            }
            else
            {
                mydialogitem2.SetLabel("Name", PluginGuiLocalizeStrings.Get(3256)); //"No Scheduled"
            }
            mydialogitem2.Command = new MethodDelegateCommand(() =>
            {
                if (mymessages.Scheduled == false)
                {
                    _dialogMenuItemList[2].SetLabel("Name", PluginGuiLocalizeStrings.Get(3255)); //"Scheduled"
                }
                else
                {
                    _dialogMenuItemList[2].SetLabel("Name", PluginGuiLocalizeStrings.Get(3256)); //"No Scheduled"
                }
                mymessages.Scheduled = !mymessages.Scheduled;
                DialogMenuItemList.FireChange();
                //UpdateListItems();
                SelectedDialogItem = 2;
                Thread myselectthread = new Thread(SelectFilterItemUpdate);
                myselectthread.Start();
            });
            _dialogMenuItemList.Add(mydialogitem2);


            ListItem mydialogitem3 = new ListItem();
            if (mymessages.Recorded == true)
            {
                mydialogitem3.SetLabel("Name", PluginGuiLocalizeStrings.Get(3257)); //"Recorded"
            }
            else
            {
                mydialogitem3.SetLabel("Name", PluginGuiLocalizeStrings.Get(3258)); //"No Recorded"
            }
            mydialogitem3.Command = new MethodDelegateCommand(() =>
            {
                if (mymessages.Recorded == false)
                {
                    _dialogMenuItemList[3].SetLabel("Name", PluginGuiLocalizeStrings.Get(3257)); //"Recorded"
                }
                else
                {
                    _dialogMenuItemList[3].SetLabel("Name", PluginGuiLocalizeStrings.Get(3258)); //"No Recorded"
                }
                mymessages.Recorded = !mymessages.Recorded;
                DialogMenuItemList.FireChange();
                //UpdateListItems();
                SelectedDialogItem = 3;
                Thread myselectthread = new Thread(SelectFilterItemUpdate);
                myselectthread.Start();
            });
            _dialogMenuItemList.Add(mydialogitem3);


            ListItem mydialogitem4 = new ListItem();
            if (mymessages.Deleted == true)
            {
                mydialogitem4.SetLabel("Name", PluginGuiLocalizeStrings.Get(3259)); //"Deleted"
            }
            else
            {
                mydialogitem4.SetLabel("Name", PluginGuiLocalizeStrings.Get(3260)); //"No Deleted"
            }
            mydialogitem4.Command = new MethodDelegateCommand(() =>
            {
                if (mymessages.Deleted == false)
                {
                    _dialogMenuItemList[4].SetLabel("Name", PluginGuiLocalizeStrings.Get(3259)); //"Deleted"
                }
                else
                {
                    _dialogMenuItemList[4].SetLabel("Name", PluginGuiLocalizeStrings.Get(3260)); //"No Deleted"
                }
                mymessages.Deleted = !mymessages.Deleted;
                DialogMenuItemList.FireChange();
                //UpdateListItems();
                SelectedDialogItem = 4;
                Thread myselectthread = new Thread(SelectFilterItemUpdate);
                myselectthread.Start();
            });
            _dialogMenuItemList.Add(mydialogitem4);


            ListItem mydialogitem5 = new ListItem();
            if (mymessages.Conflicts == true)
            {
                mydialogitem5.SetLabel("Name", PluginGuiLocalizeStrings.Get(3261)); //"Conflicts"
            }
            else
            {
                mydialogitem5.SetLabel("Name", PluginGuiLocalizeStrings.Get(3262)); //"No Conflicts"
            }
            mydialogitem5.Command = new MethodDelegateCommand(() =>
            {
                if (mymessages.Conflicts == false)
                {
                    _dialogMenuItemList[5].SetLabel("Name", PluginGuiLocalizeStrings.Get(3261)); //"Conflicts"
                }
                else
                {
                    _dialogMenuItemList[5].SetLabel("Name", PluginGuiLocalizeStrings.Get(3262)); //"No Conflicts"
                }
                mymessages.Conflicts = !mymessages.Conflicts;
                DialogMenuItemList.FireChange();
                //UpdateListItems();
                SelectedDialogItem = 5;
                Thread myselectthread = new Thread(SelectFilterItemUpdate);
                myselectthread.Start();
            });
            _dialogMenuItemList.Add(mydialogitem5);


            ListItem mydialogitem6 = new ListItem();
            mydialogitem6.SetLabel("Name", PluginGuiLocalizeStrings.Get(3263)); //"All";
            mydialogitem6.Command = new MethodDelegateCommand(() =>
            {
                for (int i = 0; i < 6; i++)
                {
                    _dialogMenuItemList[i].SetLabel("Name", PluginGuiLocalizeStrings.Get(3251+i*2));
                }
                mymessages.View = true;
                mymessages.Email = true;
                mymessages.Scheduled = true;
                mymessages.Recorded = true;
                mymessages.Deleted = true;
                mymessages.Conflicts = true;
                DialogMenuItemList.FireChange();
                //UpdateListItems();
                SelectedDialogItem = 6;
                Thread myselectthread = new Thread(SelectFilterItemUpdate);
                myselectthread.Start();
            });
            _dialogMenuItemList.Add(mydialogitem6);



            ListItem mydialogitem7 = new ListItem();
            mydialogitem7.SetLabel("Name", PluginGuiLocalizeStrings.Get(3264)); //"None";
            mydialogitem7.Command = new MethodDelegateCommand(() =>
            {
                for (int i = 0; i < 6; i++)
                {
                    _dialogMenuItemList[i].SetLabel("Name", PluginGuiLocalizeStrings.Get(3252 + i * 2));
                }
                mymessages.View = false;
                mymessages.Email = false;
                mymessages.Scheduled = false;
                mymessages.Recorded = false;
                mymessages.Deleted = false;
                mymessages.Conflicts = false;
                DialogMenuItemList.FireChange();
                //UpdateListItems();
                SelectedDialogItem = 7;
                Thread myselectthread = new Thread(SelectFilterItemUpdate);
                myselectthread.Start();
            });
            _dialogMenuItemList.Add(mydialogitem7);
            


            //update for dialog skin
            DialogMenuItemList.FireChange();

            //will now call a dialogbox with a given menu            
            ScreenManager.ShowDialog(TVWISHLIST_RESULT_DIALOG_MENU_SCREEN);
                    
            myTvWishes.ButtonActive = false; //unlock buttons
        }

        public void SelectFilterItemUpdate()
        {
            //Thread.Sleep(DialogSleep);            

            
            for (int i = 0; i < _dialogMenuItemList.Count; i++)
            {
                _dialogMenuItemList[i].Selected = false;
            }

            _dialogMenuItemList[SelectedDialogItem].Selected = true;
            DialogMenuItemList.FireChange();
            Thread myselectthread = new Thread(UpdateListItems);
            myselectthread.Start();
            
        }
        #endregion Public Methods

    }    
}
