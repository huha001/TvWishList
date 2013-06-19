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

using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishListMP2.Models
{
    public partial class Main_GUI : IWorkflowModel, IDisposable
    {

        #region Localized Resources

        //must be model ID defined in workflow and plugin.xml
        public const string MAIN_GUI_MODEL_ID_STR = "46199691-8dc6-443d-9022-1315cee6152b";
        public readonly static Guid MAIN_GUI_MODEL_ID = new Guid(MAIN_GUI_MODEL_ID_STR);
        public const string EDIT_GUI_MODEL_ID_STR = "093c13ed-413e-4fc2-8db0-3eca69c09ad0";
        public readonly static Guid EDIT_GUI_MODEL_ID = new Guid(EDIT_GUI_MODEL_ID_STR);
        public const string RESULT_GUI_MODEL_ID_STR = "6e96da05-1c6a-4fed-8fed-b14ad114c4a2";
        public readonly static Guid RESULT_GUI_MODEL_ID = new Guid(RESULT_GUI_MODEL_ID_STR);

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

        public const string TVWISHLIST_MAIN_DIALOG_MENU_SCREEN = "TvWishListMainDialogMenu";
        public const string TVWISHLIST_MAIN_INPUT_TEXTBOX_SCREEN = "TvWishListMainInputTextBox";
        
        #endregion

        

        #region Global Variables and Services

        //Register global Services
        //ILogger Log = ServiceRegistration.Get<ILogger>();

        IScreenManager ScreenManager = ServiceRegistration.Get<IScreenManager>();

        static ITvWishListTVProvider _myTvProvider = null;
        public static ITvWishListTVProvider MyTvProvider
        {
            get { return (ITvWishListTVProvider)_myTvProvider; }
            set { _myTvProvider = value; }
        }

        DialogCloseWatcher DialogCloseWatcher = null;



        //Debug  
        bool DEBUG = false;

        bool WideSkin = true;

        int _dialogSleep = 100;

        private bool _active=false;
        public bool Active
        {
            get { return (bool)_active; }
        }

        public int DialogSleep
        {
            get { return (int)_dialogSleep; }
        }

        private string _parameter = string.Empty;
        public string Parameter
        {
            get { return (string)_parameter; }
            set { _parameter = value; }
        }

        int _focusedTvWish = 0;

        //paralmeters for skins
        string NAME;
        string TITLE;
        string EXPRESSION;
        string CHANNEL;
        string EPISODEPART;
        string EPISODENAME;
        string SERIESNUMBER;
        string EPISODENUMBER;

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
        
        // Itemlist that is exposed to the skin (ListView in skin)
        protected ItemsList _skinItemList = new ItemsList();  //must be defined as new ItemsList() here !
        protected ItemsList _dialogMenuItemList = new ItemsList();  //must be defined as new ItemsList() here !
        
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

        public ItemsList SkinItemList
        {
            get { return _skinItemList; }
        }
        public ItemsList DialogMenuItemList
        {
            get { return _dialogMenuItemList; }
        }

        /// <summary>
        /// This is the dependency property for our sample string. It is needed to propagate changes to the skin.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the screen databinds to the <see cref="HelloString"/> property in a binding mode which will propagate data
        /// changes from the model to the skin (OneWay, TwoWay), the SkinEngine will attach a change handler to this property
        /// and react to changes.
        /// </para>
        /// <para>
        /// In other words: For each property <c>Xyz</c>, which should be able to be attached to, there must be an
        /// <see cref="AbstractProperty"/> with name <c>XyzProperty</c>.
        /// Only if <c>XyzProperty</c> is present in the model, value changes can be propagated to the skin.
        /// </para> 
        /// </remarks>
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
        
        //do not forget Wproperties in constructor!
        #endregion Properties for Skins

        #region Constructor and Dispose
        /// <summary>
        /// Constructor... this one is called by the WorkflowManager when this model is loaded due to a screen reference.
        /// </summary>
        public Main_GUI()  //will be called when the screen is the first time loaded not the same as Init() !!!
        {
            Log.Debug("MainGui: Constructor called");
            _instance = this; //needed to ensure transfer from static function later on

            // In models, properties will always be WProperty instances. When using SProperties for screen databinding,
            _headerProperty = new WProperty(typeof(string), "[TvWishListMP2.1000]");
            _modusProperty = new WProperty(typeof(string), "[TvWishListMP2.4200]");
            _statusProperty = new WProperty(typeof(string), String.Empty);
            _textBoxSkinProperty = new WProperty(typeof(string), String.Empty);
            _dialogHeaderProperty = new WProperty(typeof(string), String.Empty);
            _inputTextBoxSkinProperty = new WProperty(typeof(string), String.Empty);
            _inputHeaderProperty = new WProperty(typeof(string), String.Empty);
            
            //initialize MP1 plugin translation
            PluginGuiLocalizeStrings.MP2Section = "TvWishListMP2";


            



            //Message Initialization after language
            mymessages = new XmlMessages(PluginGuiLocalizeStrings.Get(4900), "", false);  //user defined custom DateTimeFormat will overwrite default value later
            //Log.Debug("[TVWishListMP GUI_List]:Init  mymessages.filename=" + mymessages.filename);

            //TvWishes Initialization
            myTvWishes = new TvWishProcessing();           

            //Format Initialization
            _TextBoxFormat_16to9_EmailFormat_Org = PluginGuiLocalizeStrings.Get(1900);
            _TextBoxFormat_16to9_EmailFormat_Org = _TextBoxFormat_16to9_EmailFormat_Org.Replace(@"\n", "\n");
            _TextBoxFormat_16to9_EmailFormat_Org = _TextBoxFormat_16to9_EmailFormat_Org.Replace("<br>", "\n");
            _TextBoxFormat_16to9_EmailFormat_Org = _TextBoxFormat_16to9_EmailFormat_Org.Replace("<BR>", "\n");

            _TextBoxFormat_4to3_EmailFormat_Org = PluginGuiLocalizeStrings.Get(1902);
            _TextBoxFormat_4to3_EmailFormat_Org = _TextBoxFormat_4to3_EmailFormat_Org.Replace(@"\n", "\n");
            _TextBoxFormat_4to3_EmailFormat_Org = _TextBoxFormat_4to3_EmailFormat_Org.Replace("<br>", "\n");
            _TextBoxFormat_4to3_EmailFormat_Org = _TextBoxFormat_4to3_EmailFormat_Org.Replace("<BR>", "\n");

            _TextBoxFormat_16to9_ViewOnlyFormat_Org = PluginGuiLocalizeStrings.Get(1901);
            _TextBoxFormat_16to9_ViewOnlyFormat_Org = _TextBoxFormat_16to9_ViewOnlyFormat_Org.Replace(@"\n", "\n");
            _TextBoxFormat_16to9_ViewOnlyFormat_Org = _TextBoxFormat_16to9_ViewOnlyFormat_Org.Replace("<br>", "\n");
            _TextBoxFormat_16to9_ViewOnlyFormat_Org = _TextBoxFormat_16to9_ViewOnlyFormat_Org.Replace("<BR>", "\n");

            _TextBoxFormat_4to3_ViewOnlyFormat_Org = PluginGuiLocalizeStrings.Get(1903);
            _TextBoxFormat_4to3_ViewOnlyFormat_Org = _TextBoxFormat_4to3_ViewOnlyFormat_Org.Replace(@"\n", "\n");
            _TextBoxFormat_4to3_ViewOnlyFormat_Org = _TextBoxFormat_4to3_ViewOnlyFormat_Org.Replace("<br>", "\n");
            _TextBoxFormat_4to3_ViewOnlyFormat_Org = _TextBoxFormat_4to3_ViewOnlyFormat_Org.Replace("<BR>", "\n");

            //load MP2 data
            LoadSettings(); //must be done after initialization of windows
            LoadSettingsResultGui();

            //load TvwishlistFolder and filenames from TvServer
            LoadFromTvServer();

            MpVersion = Main_GUI._instance.Version();
            if (TvVersion != MpVersion) //version does not match
            {
                Log.Debug("TvVersion " + TvVersion + " does not match MpVersion " + MpVersion + " -Aborting plugin");
                VersionMismatch = true;
            }
            Log.Debug("TvWishList_Mp2Version =" + MpVersion);
            Log.Debug("TvWishList_TvServerVersion =" + TvVersion);
            
        }

        public void Dispose()
        {
            //seems to be usable for MP1 function DeInit()
            Log.Debug("Main_GUI:Dispose() - disposing");

            SaveSettings();

            //*****************************************************
            //save data and unlock TvWishList and 
            if (LOCKED == true)
            {
                TvserverdatabaseSaveSettings();
                myTvWishes.UnLockTvWishList();
                LOCKED = false;
                Log.Debug("5 LOCKED=" + LOCKED.ToString());
            }

            if (DialogCloseWatcher != null)
                DialogCloseWatcher.Dispose();

        }

        #endregion Constructor and Dispose

        #region OnPage
        private void OnPageLoad(NavigationContext oldContext, NavigationContext newContext)
        {
            Log.Debug("Main_GUI():OnPageLoad  VIEWONLY=" + myTvWishes.ViewOnlyMode.ToString());
            Log.Debug("newModelId=" + newContext.WorkflowModelId.ToString());
            Log.Debug("oldModelId=" + oldContext.WorkflowModelId.ToString());

            //define header
            _active = true;

            if (VersionMismatch) //bye
            {
                myTvWishes.MyMessageBox(4305, 4306); //TvWishList MediaPortal Plugin Does Not Match To TvWishList TV Server Plugin
                TvServerLoadSettings_FAILED = true; // do not save data after version mismatch   
                // workflowmanager is started in my message box with workflowManager.NavigatePop(1); //same as escape (takes one entry from the stack)
                return; //do not forget return!
            }



            Log.Debug("Previous ModelID=" + oldContext.WorkflowModelId.ToString());

            if ((oldContext.WorkflowModelId == EDIT_GUI_MODEL_ID) || (oldContext.WorkflowModelId == RESULT_GUI_MODEL_ID))//TVWishList  EDIT or RESULT
            {
                if ((LOCKED == false) && (TvServerLoadSettings_FAILED == false))
                {
                    //*****************************************************
                    //Lock Tvwishes
                    bool success = myTvWishes.LockTvWishList("TvWishListMP:Main");
                    if (!success)
                    {
                        Log.Debug("1 Lock failed");
                        myTvWishes.MyMessageBox(4305, 4311); //Tv wish list is being processed by another process<br>Try again later<br>If the other process hangs reboot the system or stop the tv server manually
                        LOCKED = false;
                        Log.Debug("1 LOCKED=" + LOCKED.ToString());
                        TvServerLoadSettings_FAILED = true;
                        // workflowmanager is started in my message box with workflowManager.NavigatePop(1); //same as escape (takes one entry from the stack)  
                        return; //do not forget return!
                    }
                    else
                    {
                        LOCKED = true;
                        Log.Debug("2 LOCKED=" + LOCKED.ToString());
                    }
                }
                //do not load settings            
            }
            else
            {
                //*****************************************************
                //Lock and load Tvwishes
                bool success = myTvWishes.LockTvWishList("TvWishListMP:Main");
                if (!success)
                {
                    myTvWishes.MyMessageBox(4305, 4311); //Tv wish list is being processed by another process<br>Try again later<br>If the other process hangs reboot the system or stop the tv server manually
                    LOCKED = false;
                    Log.Debug("3 LOCKED=" + LOCKED.ToString());
                    TvServerLoadSettings_FAILED = true;
                    // workflowmanager is started in my message box with workflowManager.NavigatePop(1); //same as escape (takes one entry from the stack)      
                    return; //do not forget return!
                }
                else
                {
                    LOCKED = true;
                    Log.Debug("4 LOCKED=" + LOCKED.ToString());
                }

                //load MP2 data to ensure config changes are used
                LoadSettings(); 

                TvserverdatabaseLoadSettings();
                //MP settings at mediaportal exit
            }
            Log.Debug("_Tvwishes.Count=" + myTvWishes.ListAll().Count.ToString());
            //trying new position for formatprocessing every time a page is loaded so that edit items are being updated when allitems are selected in edit window
            // Postprocess formats and keep only selected edit items in textboxformats (must be done after MP LoadSettings!)
            _TextBoxFormat_16to9_EmailFormat = TextBoxFormatConversion(_TextBoxFormat_16to9_EmailFormat_Org);
            _TextBoxFormat_4to3_EmailFormat = TextBoxFormatConversion(_TextBoxFormat_4to3_EmailFormat_Org);
            _UserEmailFormat = TextBoxFormatConversion(_UserEmailFormat_Org);

            _TextBoxFormat_16to9_ViewOnlyFormat = TextBoxFormatConversion(_TextBoxFormat_16to9_ViewOnlyFormat_Org);
            _TextBoxFormat_4to3_ViewOnlyFormat = TextBoxFormatConversion(_TextBoxFormat_4to3_ViewOnlyFormat_Org);
            _UserViewOnlyFormat = TextBoxFormatConversion(_UserViewOnlyFormat_Org);
            // end postprocessing formats
            UpdateControls();
            UpdateListItems();

            Log.Debug("OnPageLoad(): Parameter=" + Parameter);
            if (Parameter != String.Empty)
            {
                //start it as a thread for the dialog
                
                Thread parameterThread = new Thread(ParameterStarter);
                parameterThread.Start();
            }
        }

        private void OnPageDestroy(NavigationContext oldContext, NavigationContext newContext)
        {
            Log.Debug("Main_GUI:OnPageDestroy()");
            Log.Debug("VIEWONLY=" + myTvWishes.ViewOnlyMode.ToString());
            Log.Debug("newModelId=" + newContext.WorkflowModelId.ToString());
            Log.Debug("oldModelId=" + oldContext.WorkflowModelId.ToString());

            if (Main_GUI.MyTvProvider != null)
            {
                Main_GUI.MyTvProvider.DeInit();
            }
            _active = false;

            //save data if going to a new page
            if ((newContext.WorkflowModelId != RESULT_GUI_MODEL_ID) && (newContext.WorkflowModelId != EDIT_GUI_MODEL_ID))
            {
                if (TvserverdatabaseSaveSettings() == true)
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
                LOCKED = false;

            }
        }
        #endregion OnPage

        #region IWorkflowModel Implementation
        // to use this you must have derived the class from IWorkflowModel, IDisposable
        // and you must have defined in plugin.xml a workflowstate
        // WorkflowModel="023c44f2-3329-4781-9b4a-c974444c0b0d"/> <!-- MyTestPlugin Model -->

        public Guid ModelId
        {
            get { return MAIN_GUI_MODEL_ID; }
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

        #endregion IWorkflowModel Implementation

        #region Public Methods



        #region PogButtton
        public void ParameterStarter()
        {
            Log.Debug("ParameterStarter(): Parameter=" + Parameter);
            ParameterEvaluation(Parameter,-1, TvWishListQuickMenu);
            Parameter = "";
            Log.Debug("ParameterStarter() End: Parameter=" + Parameter);
        }
      
        public void Name(string parameter)
        {
            NAME = parameter;
        }
        public void Title(string parameter)
        {
            TITLE = parameter;
        }
        public void Expression(string parameter)
        {
            EXPRESSION = parameter;
        }
        public void ChannelName(string parameter)
        {
            CHANNEL = parameter;
        }
        public void EpisodePart(string parameter)
        {
            EPISODEPART = parameter;
        }
        public void EpisodeName(string parameter)
        {
            EPISODENAME = parameter;
        }
        public void SeriesNumber(string parameter)
        {
            SERIESNUMBER = parameter;
        }
        public void EpisodeNumber(string parameter)
        {
            EPISODENUMBER = parameter;
        }
        public void Command(string parameter)
        {
            try
            {               
                parameter = parameter.Replace(@"//", "\n");
                string[] tokens = parameter.Split('\n');
                string newparameter = tokens[0]+"//"; //COMMAND

                if (parameter.Contains("NAME"))
                {
                    newparameter += "NAME=" + NAME + "//";
                }

                if (parameter.Contains("TITLE"))
                {
                    newparameter += "TITLE=" + TITLE + "//";
                }

                if (parameter.Contains("EXPRESSION"))
                {
                    newparameter += "EXPRESSION=" + EXPRESSION + "//";
                }

                if (parameter.Contains("CHANNEL"))
                {
                    newparameter += "CHANNEL=" + CHANNEL + "//";
                }

                if (parameter.Contains("EPISODEPART"))
                {
                    newparameter += "EPISODEPART=" + EPISODEPART + "//";
                }

                if (parameter.Contains("EPISODENAME"))
                {
                    newparameter += "EPISODENAME=" + EPISODENAME + "//";
                }

                if (parameter.Contains("SERIESNUMBER"))
                {
                    newparameter += "SERIESNUMBER=" + SERIESNUMBER + "//";
                }

                if (parameter.Contains("EPISODENUMBER"))
                {
                    newparameter += "EPISODENUMBER=" + EPISODENUMBER + "//";
                }

                if (parameter.Contains("VIEWONLY=TRUE"))
                {
                    newparameter += "VIEWONLY=TRUE//";
                }
                else if (parameter.Contains("VIEWONLY=FALSE"))
                {
                    newparameter += "VIEWONLY=FALSE//";
                }

                Parameter = newparameter.Substring(0, newparameter.Length - 2); // remove last "//"
                Log.Debug("Parameter=" + newparameter);


                //push to Main Page
                IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
                Guid mainStatId = new Guid("eb91dc39-9735-48f6-9f31-39558ce2e861"); //from plugin.xml
                workflowManager.NavigatePush(mainStatId);

                //Parameter will start evaluation on main page via thread - do not start here

                
            }
            catch (Exception exc)
            {
                Log.Error("Error in Command() parameter= " + parameter + "\n exception is " + exc.Message);
            }

        }
        
        #endregion PogButton

        public void UpdateControls()
        {
            Log.Debug("MainGUI: UpdateControls() started");
            
            myTvWishes.StatusLabel("");

            
            if (myTvWishes.ViewOnlyMode == false)
            {
                Log.Debug("Header Email Mode");
                //Header = PluginGuiLocalizeStrings.Get(1000) + "  " + PluginGuiLocalizeStrings.Get(4200); //"TVWishList - Main-Email&Record"); 
                Modus = PluginGuiLocalizeStrings.Get(4200); //"TVWishList - Main-Email&Record"); 
            }
            else
            {
                Log.Debug("Header ViewOnly Mode");
                //Header = PluginGuiLocalizeStrings.Get(1000) + "  " + PluginGuiLocalizeStrings.Get(4201); //"TVWishList - Main-Viewonly");  
                Modus = PluginGuiLocalizeStrings.Get(4201); //"TVWishList - Main-Viewonly");
            }
            Log.Debug("Modus = " + Modus);
            
        }

        public void UpdateListItems()
        {
            Log.Debug("MainGUI: UpdateListItems() started");

            _skinItemList.Clear();
            

            try
            {
                int ctr = 0;
                foreach (TvWish mywish in myTvWishes.ListAll())
                {
                    string listitem = "";
                    Log.Debug("UpdateListItem name=" + mywish.name);
                    if (_UserListItemFormat != "")
                        listitem = FormatTvWish(mywish, _UserListItemFormat); //user defined listitem format
                    else if (myTvWishes.ViewOnlyMode == false)
                        listitem = FormatTvWish(mywish, PluginGuiLocalizeStrings.Get(1904));  //Email listitemformat
                    else
                        listitem = FormatTvWish(mywish, PluginGuiLocalizeStrings.Get(1905));  //View Only listitemformat

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
                        SelectedItemChanged(myitem);
                    });
                    _skinItemList.Add(myitem);
                    ctr++;

                }
                if (myTvWishes.ListAll().Count == 0)
                {
                    ListItem myitem = new ListItem();
                    myitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(4301)); //"No items found"
                    _skinItemList.Add(myitem);

                    //update skin
                    TextBoxSkin = string.Empty;
                }



                Log.Debug("_focusedTvWish=" + _focusedTvWish.ToString());
                Log.Debug("_skinItemList.Count=" + _skinItemList.Count.ToString());
                if ((_focusedTvWish >= 0) && (_focusedTvWish < _skinItemList.Count))
                {
                    Log.Debug("Selecting _focusedTvWish="+_focusedTvWish.ToString());
                    _skinItemList[_focusedTvWish].Selected = true;
                }


                Log.Debug("[TVWishListMP]:UpdateListItems message number found:_skinItemList.Count=" + _skinItemList.Count.ToString());
            }
            catch (Exception exc)
            {
                ListItem myitem = new ListItem();
                myitem.SetLabel("Name",PluginGuiLocalizeStrings.Get(4302));   //Error in creating item list
                myitem.SetLabel("Value", "-1");
                myitem.SetLabel("Index", "-1");
                _skinItemList.Add(myitem);
                Log.Error("Error in creating item list - exception was:" + exc.Message);
            }

            SkinItemList.FireChange();
        }
        

        public void FocusedItemChanged(ListItem focusedListItem)
        {
            //Log.Debug("Main_GUI: FocusedItemChanged()");

            if (focusedListItem == null)
                return;

            if (myTvWishes.ListAll().Count == 0)
                return;

            try
            {
                //Log.Debug("focusedListItem.Labels[Index].ToString()="+focusedListItem.Labels["Index"].ToString());

                _focusedTvWish = Convert.ToInt32(focusedListItem.Labels["Index"].ToString());

                if (_focusedTvWish == -1)
                    return;

                //Log.Debug("focusedTvWish="+_focusedTvWish.ToString());

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

                if ((_focusedTvWish >= 0) && (myTvWishes.ListAll().Count > 0))
                    messagetext = FormatTvWish(myTvWishes.GetAtIndex(_focusedTvWish), _TextBoxFormat);


                TextBoxSkin = messagetext;                
            }
            catch (Exception exc)
            {
                Log.Error("main_GUI:FocusedItemChanged(ListItem focusedListItem): Exception=" + exc.Message);
            }
        
        }

        public void SelectedItemChanged(ListItem selectedListItem)
        {
            Log.Debug("Main_GUI:ItemSelectionChanged()");

            if (selectedListItem == null)
                return;

            if (myTvWishes.ListAll().Count == 0)
                return;

            try
            {
                //myTvWishes.FocusedWishIndex = Convert.ToInt32(selectedListItem.Labels["Index"].ToString());
                myTvWishes.FocusedWishIndex = _focusedTvWish;
                Log.Debug("myTvWishes.FocusedWishIndex=" + myTvWishes.FocusedWishIndex.ToString());
            }
            catch (Exception exc)
            {
                myTvWishes.FocusedWishIndex = 0;
                Log.Error("Main_GUI:ItemSelectionChanged() Exception for myTvWishes.FocusedWishIndex");
                Log.Error("Exception=" + exc.Message);
                return;
            }

            Log.Debug("Selection changed to item: " + myTvWishes.FocusedWishIndex);

            if (myTvWishes.FocusedWishIndex >= 0)
            {
                //push to Edit Page
                IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
                Guid editStatId = new Guid("100e9845-54de-4e9b-9cf1-6101509b7c6d"); //from plugin.xml
                workflowManager.NavigatePush(editStatId);
            }
        }

        
        public void RightClick()
        {
            Log.Debug("Main_GUI:RightClick()");
            
            if (myTvWishes.ListAll().Count == 0)
                return;

            try
            {
                //Log.Debug("focusedListItem.Labels[Index].ToString()="+focusedListItem.Labels["Index"].ToString());

                if ((_focusedTvWish >= 0) && (myTvWishes.ListAll().Count > 0))
                {

                    mymessages.FilterName = myTvWishes.GetAtIndex(_focusedTvWish).tvwishid;
                    Log.Debug("_focusedTvWish=" + _focusedTvWish.ToString());
                    Log.Debug("mymessages.FilterName=" + mymessages.FilterName);
                    IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();

                    //push to Result Page                    
                    Guid resultStatId = new Guid("eff5097b-a4a7-4934-a2e3-d8047d92cec7"); //from plugin.xml
                    workflowManager.NavigatePush(resultStatId);
                }
            }
            catch (Exception exc)
            {
                Log.Error("Main_GUI:RightClick: Exception=" + exc.Message);
            }
        }


        public void DeleteItem()
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
                if (myTvWishes.ListAll().Count == 0)
                    return;

#if (MP2)
                int index = _focusedTvWish;
#else
                int index = myListView.SelectedListItemIndex;
#endif
                myTvWishes.RemoveAtIndex(index);
                //index = myListView.RemoveItem(index);
                //myListView.SelectedListItemIndex = index;

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

                if (_focusedTvWish > myTvWishes.ListAll().Count-1)
                {
                    _focusedTvWish = myTvWishes.ListAll().Count - 1;
                }

                UpdateListItems();
                //StatusLabel( "Deleted item: " + index.ToString());
            }
            catch (Exception exc)
            {
                Log.Error("Error DeletePlayListItem: Exception " + exc.Message);
            }
            

            myTvWishes.ButtonActive = false; //unlock buttons
        }

        public void MoveItemDown()
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
                if (myTvWishes.ListAll().Count == 0)
                    return;

#if (MP2)
                int index = _focusedTvWish;
#else
                int index = myListView.SelectedListItemIndex;
#endif

                if (myTvWishes.ListAll().Count > index + 1)
                {
                    TvWish tempwish = myTvWishes.GetAtIndex(index);
                    myTvWishes.ReplaceAtIndex(index, myTvWishes.GetAtIndex(index + 1));
                    myTvWishes.ReplaceAtIndex(index + 1, tempwish);
                }
                else
                {
                    TvWish tempwish = myTvWishes.GetAtIndex(index);
                    myTvWishes.ReplaceAtIndex(index, myTvWishes.GetAtIndex(0));
                    myTvWishes.ReplaceAtIndex(0, tempwish);
                }

                _focusedTvWish++;
                if (_focusedTvWish > myTvWishes.ListAll().Count - 1)
                {
                    _focusedTvWish = myTvWishes.ListAll().Count - 1;
                }

                UpdateListItems();
                //StatusLabel( "Move down item: " + index.ToString());
            }
            catch (Exception exc)
            {
                Log.Error("Error MovePlayListItemDown: Exception " + exc.Message);
            }

            myTvWishes.ButtonActive = false; //unlock buttons
        }

        public void MoveItemUp()
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
                if (myTvWishes.ListAll().Count == 0)
                    return;

#if (MP2)
                int index = _focusedTvWish;
#else
                int index = myListView.SelectedListItemIndex;
#endif

                if (index > 0)
                {
                    TvWish tempwish = myTvWishes.GetAtIndex(index);
                    myTvWishes.ReplaceAtIndex(index, myTvWishes.GetAtIndex(index - 1));
                    myTvWishes.ReplaceAtIndex(index - 1, tempwish);
                }
                else
                {
                    TvWish tempwish = myTvWishes.GetAtIndex(index);
                    myTvWishes.ReplaceAtIndex(index, myTvWishes.GetAtIndex(myTvWishes.ListAll().Count - 1));
                    myTvWishes.ReplaceAtIndex(myTvWishes.ListAll().Count - 1, tempwish);
                }

                _focusedTvWish--;
                if (_focusedTvWish < 0)
                {
                    _focusedTvWish = 0;
                }

                UpdateListItems();
                //StatusLabel( "Move up item: " + index.ToString());
            }
            catch (Exception exc)
            {
                Log.Error("Error MovePlayListItemUp: Exception " + exc.Message);
            }

            myTvWishes.ButtonActive = false; //unlock buttons
        }

        public void OnButtonMore()
        {
            Log.Debug("MainGUI: OnButtonMore() started");

            _dialogMenuItemList.Clear();

            DialogHeader = PluginGuiLocalizeStrings.Get(1107);

            for (int i = 0; i <= 8; i++)
            {
                ListItem myitem = new ListItem();
                // now we define numbers for changing the postrecord setting of the tv server database
                myitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(i+1500));
                myitem.SetLabel("Index",i.ToString());
                myitem.Command = new MethodDelegateCommand(() =>
                {
                    int number = Convert.ToInt32(myitem.Labels["Index"].ToString());
                    Log.Debug("number=" + number.ToString());
                    MoreEvaluation(number+1); //starts at 0, but should be 1
                    IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                    mycommandscreenManager.CloseTopmostDialog();
                });
                _dialogMenuItemList.Add(myitem);
            }


            //update for dialog skin
            DialogMenuItemList.FireChange();

            //will now call a dialogbox with a given menu            
            ScreenManager.ShowDialog(TVWISHLIST_MAIN_DIALOG_MENU_SCREEN);
        }

        public void OnButtonNew()
        {
            Log.Debug("MainGUI: OnButtonNew() started");

            //no button lock
            InputHeader = PluginGuiLocalizeStrings.Get(102); //New TvWish: Search For:
            InputTextBoxSkin = string.Empty;
            ScreenManager.ShowDialog(TVWISHLIST_MAIN_INPUT_TEXTBOX_SCREEN);
        }

        public void CreateNew()
        {
            //button lock
            if (myTvWishes.ButtonActive)
            {
                myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Operation Is Still Running"
                return;
            }
            myTvWishes.ButtonActive = true; //lock buttons

            Log.Debug("MainGUI: CreateNew() started");
            TvWish newwish = myTvWishes.DefaultData();
            newwish.searchfor = InputTextBoxSkin;
            newwish.name = InputTextBoxSkin;
            myTvWishes.Add(newwish);
            myTvWishes.FocusedWishIndex = myTvWishes.ListAll().Count - 1;
            UpdateListItems();
            myTvWishes.ButtonActive = false; //unlock buttons
        }

        public void LoadSettingsResultGui()
        {
            Log.Debug("Main_GUI: LoadSettings() called");           

            try
            {
                ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
                TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();
                //data for GUI_List_window 
                mymessages.Sort = settings.Sort;
                mymessages.SortReverse = settings.SortReverse;
                mymessages.Email = settings.Email;
                mymessages.Deleted = settings.Deleted;
                mymessages.Conflicts = settings.Conflicts;
                mymessages.Scheduled = settings.Scheduled;
                mymessages.Recorded = settings.Recorded;
                mymessages.View = settings.View;
            }
            catch (Exception exc)
            {
                Log.Error("Error LoadSettings: Exception " + exc.Message);
            }
        }

        public void LoadSettings()
        {
            Log.Debug("Main_GUI: LoadSettings() called");           

            try
            {
                ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
                TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();
                //Log.Debug("Settings have been loaded");

                _UserListItemFormat = settings.MainItemFormat;
                _UserListItemFormat = _UserListItemFormat.Replace("<br>", "\n");
                _UserListItemFormat = _UserListItemFormat.Replace("<BR>", "\n");

                _UserEmailFormat_Org = settings.EmailMainTextBoxFormat;
                _UserEmailFormat_Org = _UserEmailFormat_Org.Replace("<br>", "\n");
                _UserEmailFormat_Org = _UserEmailFormat_Org.Replace("<BR>", "\n");

                _UserViewOnlyFormat_Org = settings.EmailResultsTextBoxFormat;
                _UserViewOnlyFormat_Org = _UserViewOnlyFormat_Org.Replace("<br>", "\n");
                _UserViewOnlyFormat_Org = _UserViewOnlyFormat_Org.Replace("<BR>", "\n");

                DEBUG = settings.Verbose;
                myTvWishes.Debug = DEBUG;
                mymessages.debug = DEBUG;
                Log.DebugValue = DEBUG;

                TimeOutValueString = settings.TimeOut;
                TvWishItemSeparator = settings.TvWishItemSeparator;
                _dialogSleep = settings.DialogSleep;
                WideSkin = settings.WideSkin;

                //modify for listview table changes
                myTvWishes.DisableInfoWindow = settings.DisableInfoWindow;
                //Log.Debug("myTvWishes.DisableInfoWindow=" + myTvWishes.DisableInfoWindow.ToString());
                TvWishListQuickMenu = settings.DisableQuickMenu;

                //data for GUI_Edit_window
                myTvWishes._boolTranslator[(int)TvWishEntries.action] = settings.Action;
                myTvWishes._boolTranslator[(int)TvWishEntries.active] = settings.Active;
                myTvWishes._boolTranslator[(int)TvWishEntries.afterdays] = settings.AfterDays;
                myTvWishes._boolTranslator[(int)TvWishEntries.aftertime] = settings.AfterTime;
                myTvWishes._boolTranslator[(int)TvWishEntries.beforedays] = settings.BeforeDays;
                myTvWishes._boolTranslator[(int)TvWishEntries.beforetime] = settings.BeforeTime;
                myTvWishes._boolTranslator[(int)TvWishEntries.channel] = settings.Channel;
                myTvWishes._boolTranslator[(int)TvWishEntries.episodename] = settings.EpisodeName;
                myTvWishes._boolTranslator[(int)TvWishEntries.episodenumber] = settings.EpisodeNumber;
                myTvWishes._boolTranslator[(int)TvWishEntries.episodepart] = settings.EpisodePart;
                myTvWishes._boolTranslator[(int)TvWishEntries.exclude] = settings.Exclude;
                myTvWishes._boolTranslator[(int)TvWishEntries.group] = settings.Group;
                myTvWishes._boolTranslator[(int)TvWishEntries.keepepisodes] = settings.KeepEpisodes;
                myTvWishes._boolTranslator[(int)TvWishEntries.keepuntil] = settings.KeepUntil;
                myTvWishes._boolTranslator[(int)TvWishEntries.matchtype] = settings.MatchType;
                myTvWishes._boolTranslator[(int)TvWishEntries.name] = settings.Name;
                myTvWishes._boolTranslator[(int)TvWishEntries.postrecord] = settings.PostRecord;
                myTvWishes._boolTranslator[(int)TvWishEntries.prerecord] = settings.PreRecord;
                myTvWishes._boolTranslator[(int)TvWishEntries.priority] = settings.Priority;
                myTvWishes._boolTranslator[(int)TvWishEntries.recommendedcard] = settings.RecommendedCard;
                myTvWishes._boolTranslator[(int)TvWishEntries.recordtype] = settings.RecordType;
                myTvWishes._boolTranslator[(int)TvWishEntries.searchfor] = true;
                myTvWishes._boolTranslator[(int)TvWishEntries.seriesnumber] = settings.SeriesNumber;
                myTvWishes._boolTranslator[(int)TvWishEntries.skip] = settings.Skip;
                myTvWishes._boolTranslator[(int)TvWishEntries.useFolderName] = settings.UseFolderName;
                myTvWishes._boolTranslator[(int)TvWishEntries.withinNextHours] = settings.WithinNextHours;
                myTvWishes._boolTranslator[(int)TvWishEntries.episodecriteria] = settings.EpisodeCriteria;
                myTvWishes._boolTranslator[(int)TvWishEntries.preferredgroup] = settings.PreferredGroup;
                myTvWishes._boolTranslator[(int)TvWishEntries.includerecordings] = settings.IncludeRecordings;

                //create backup vector for usage after view only mode
                for (int i = 0; i < myTvWishes._boolTranslator.Length; i++)
                {
                    myTvWishes._boolTranslatorbackup[i] = myTvWishes._boolTranslator[i];
                }               

                //load user defined default formats
                string userDefaultFormatsString = settings.UserDefaultFormatsString;
                if (userDefaultFormatsString == string.Empty)
                    userDefaultFormatsString = "True";//all other defaults will be set from checking below

                Log.Debug("DefaultFormatsString=" + userDefaultFormatsString);
                string[] userDefaultFormats = myTvWishes.CheckColumnsEntries(userDefaultFormatsString.Split(TvWishItemSeparator), TvWishItemSeparator, true);

                for (int i = 0; i < userDefaultFormats.Length; i++)
                { //overwrite myTvWishes.DefaultValues with user defined default values
                    myTvWishes.DefaultValues[i] = userDefaultFormats[i];
                    Log.Debug(" myTvWishes.DefaultValues["+i.ToString()+"=" + userDefaultFormats[i]);
                }

                //id default must be -1 
                myTvWishes.DefaultValues[(int)TvWishEntries.tvwishid] = "-1";
                //counter defaults must be 0
                myTvWishes.DefaultValues[(int)TvWishEntries.viewed] = "0";
                myTvWishes.DefaultValues[(int)TvWishEntries.emailed] = "0";
                myTvWishes.DefaultValues[(int)TvWishEntries.conflicts] = "0";
                myTvWishes.DefaultValues[(int)TvWishEntries.deleted] = "0";
                myTvWishes.DefaultValues[(int)TvWishEntries.recorded] = "0";
                myTvWishes.DefaultValues[(int)TvWishEntries.scheduled] = "0";

                

                mymessages.UserListItemFormat = settings.ResultItemFormat;
                mymessages.UserListItemFormat = mymessages.UserListItemFormat.Replace("<br>", "\n");
                mymessages.UserListItemFormat = mymessages.UserListItemFormat.Replace("<BR>", "\n");

                mymessages.UserEmailFormat = settings.EmailResultsTextBoxFormat;
                mymessages.UserEmailFormat = mymessages.UserEmailFormat.Replace("<br>", "\n");
                mymessages.UserEmailFormat = mymessages.UserEmailFormat.Replace("<BR>", "\n");

                mymessages.UserViewOnlyFormat = settings.ViewResultsTextBoxFormat;
                mymessages.UserViewOnlyFormat = mymessages.UserViewOnlyFormat.Replace("<br>", "\n");
                mymessages.UserViewOnlyFormat = mymessages.UserViewOnlyFormat.Replace("<BR>", "\n");

                string _userDateTimeFormat = settings.DateAndTimeFormat;
                if (_userDateTimeFormat != "")
                {
                    mymessages.date_time_format = _userDateTimeFormat;
                    Log.Debug("GUI_List_window.mymessages.date_time_format changed to " + mymessages.date_time_format);
                }           
            }
            catch (Exception exc)
            {
                Log.Error("Error LoadSettings: Exception " + exc.Message);
            }

            //Status = groupid.ToString()+"Main_GUI: LoadSettings() called";
        }

        public void SaveSettings()
        {
            Log.Debug("Main_GUI: SaveSettings() called");
            //Status = "Main_GUI: SaveSettings() called";

            try
            {
                ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
                TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();

                settings.TvWishItemSeparator = TvWishItemSeparator;

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

        #endregion Public Methods

    }    
}
