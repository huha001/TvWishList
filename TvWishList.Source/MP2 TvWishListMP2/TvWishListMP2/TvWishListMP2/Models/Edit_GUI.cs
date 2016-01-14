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
using System.Windows.Forms;
using System.Threading;

//using TvWishList;
using System.Runtime.InteropServices;

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

using MediaPortal.Plugins.TvWishList;
using MediaPortal.Plugins.TvWishList.Items;
using MediaPortal.Plugins.TvWishListMP2.Settings; //needed for configuration setting loading

using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishListMP2.Models 
{
    





    public partial class Edit_GUI : Common_Edit_AND_ConfigDefaults, IWorkflowModel, IDisposable
    {

        #region Localized Resources

        //must be model ID defined in workflow and plugin.xml
        public const string MAIN_GUI_MODEL_ID_STR = "46199691-8dc6-443d-9022-1315cee6152b";
        public readonly static Guid MAIN_GUI_MODEL_ID = new Guid(MAIN_GUI_MODEL_ID_STR);
        public const string EDIT_GUI_MODEL_ID_STR = "093c13ed-413e-4fc2-8db0-3eca69c09ad0";
        public readonly static Guid EDIT_GUI_MODEL_ID = new Guid(EDIT_GUI_MODEL_ID_STR);
        public const string RESULT_GUI_MODEL_ID_STR = "6e96da05-1c6a-4fed-8fed-b14ad114c4a2";
        public readonly static Guid RESULT_GUI_MODEL_ID = new Guid(RESULT_GUI_MODEL_ID_STR);

        new public const string TVWISHLIST_EDIT_DIALOG_MENU_SCREEN = "TvWishListEditDialogMenu";
        new public const string TVWISHLIST_EDIT_DIALOG_MENU2_SCREEN = "TvWishListEditDialogMenu2";
        new public const string TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN = "TvWishListEditInputTextBox";
        
        #endregion

        #region Global Variables and Services

        //Register global Services
        //ILogger Log = ServiceRegistration.Get<ILogger>();
        IScreenManager ScreenManager = ServiceRegistration.Get<IScreenManager>();

        string PreviousModel = string.Empty;

        

        //int EvaluateInputTextBoxCase = -1;
        //int KeepuntilCode = 2920;

        //int SelectedDialogItem = 0;

        private bool _active = false;
        public bool Active
        {
            get { return (bool)_active; }
        }

        //int _focusedEditItem = 0;

        //int selected_edit_index = 0;


        #endregion Global Variables and Services

        #region Properties for Skins

        //Properties
        protected readonly AbstractProperty _headerProperty;
        protected readonly AbstractProperty _modusProperty;
        protected readonly AbstractProperty _statusProperty;
        
        
        

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
        
        //do not forget Wproperties in constructor!

        #endregion Properties for Skins

        #region Constructor and Dispose
        /// <summary>
        /// Constructor... this one is called by the WorkflowManager when this model is loaded due to a screen reference.
        /// </summary>
        public Edit_GUI()  //will be called when the screen is the first time loaded not the same as Init() !!!
        {
            Log.Debug("Edit_GUI: Constructor called");
            _instance = this; //needed to ensure transfer from static function later on

            //update screens in base class
            this.TvWishList_Dialog_Menu_Screen = TVWISHLIST_EDIT_DIALOG_MENU_SCREEN;
            this.TvWishList_Dialog_Menu2_Screen = TVWISHLIST_EDIT_DIALOG_MENU2_SCREEN;
            this.TvWishList_Input_Textbox_Screen = TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN;

            // In models, properties will always be WProperty instances. When using SProperties for screen databinding,     
            _headerProperty = new WProperty(typeof(string), "[TvWishListMP2.2000]");
            _modusProperty = new WProperty(typeof(string), "[TvWishListMP2.4200]");
            _statusProperty = new WProperty(typeof(string), String.Empty);
            
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
        }

        public void Dispose()
        {
            //seems to be usable for MP1 function DeInit()
            Log.Debug("Edit_GUI: Dispose() - disposing");
            
        }

        #endregion Constructor and Dispose

        #region OnPage
        public void OnPageLoad(NavigationContext oldContext, NavigationContext newContext)
        {
            Log.Debug("[TVWishListMP2 GUI_Edit]:OnPageLoad  myTvWishes.FocusedWishIndex= " + myTvWishes.FocusedWishIndex.ToString());
            Log.Debug("Selected Tv wish name=" + myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).name);

            _active = true;

            if ((oldContext.WorkflowModelId != RESULT_GUI_MODEL_ID) && (oldContext.WorkflowModelId != MAIN_GUI_MODEL_ID))//TVWishList  EDIT or MAIN only
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
            DialogSleep = Main_GUI.Instance.DialogSleep; //neded for dialog selected elements

            //Select edit items based on VIEW or EMAIL mode
            UpdateEditItemTranslator();

            _restore_selected_Tvwish = myTvWishes.FocusedWishIndex;

            selected_edit_index = 0;

            UpdateControls();
            UpdateListItems(); 
        }

        public void OnPageDestroy(NavigationContext oldContext, NavigationContext newContext)
        {
            Log.Debug("[TVWishListMP2 GUI_Edit]:OnPageDestroy");
            Log.Debug("Tvwishes.Count=" + myTvWishes.ListAll().Count.ToString());
            //Log.Debug("FocusedEditIndex=" + myTvWishes.FocusedEditIndex.ToString());


            //save data if going to a new page
            if ((newContext.WorkflowModelId != RESULT_GUI_MODEL_ID) && (newContext.WorkflowModelId != MAIN_GUI_MODEL_ID))//TVWishList  EDIT or MAIN
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

                //*****************************************************
                //unlock TvWishList
                myTvWishes.UnLockTvWishList();
                Main_GUI.Instance.LOCKED = false;
                Log.Debug("13 mymainwindow.LOCKED=" + Main_GUI.Instance.LOCKED.ToString());
            }

            _active = false;
        }
        #endregion Onpage

        #region IWorkflowModel Implementation
        // to use this you must have derived the class from IWorkflowModel, IDisposable
        // and you must have defined in plugin.xml a workflowstate
        // WorkflowModel="023c44f2-3329-4781-9b4a-c974444c0b0d"/> <!-- MyTestPlugin Model -->

        public Guid ModelId
        {
            get { return EDIT_GUI_MODEL_ID; }
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
        public void UpdateControls()
        {
            Log.Debug("EditGUI: UpdateControls() started");
            myTvWishes.StatusLabel("");

            if (myTvWishes.ViewOnlyMode == false)
            {
                //Header = PluginGuiLocalizeStrings.Get(2000) + "  " + PluginGuiLocalizeStrings.Get(4200); //"TVWishList - Main-Email&Record");  
                Modus = PluginGuiLocalizeStrings.Get(4200); //"TVWishList - Main-Email&Record");
            }
            else
            {
                //Header = PluginGuiLocalizeStrings.Get(2000) + "  " + PluginGuiLocalizeStrings.Get(4201); //"TVWishList - Main-Viewonly"); 
                Modus = PluginGuiLocalizeStrings.Get(4201); //"TVWishList - Main-Viewonly");
            }
        }

        

        public void OnButtonNew()
        {
            Log.Debug("MainGUI: OnButtonNew() started");

            //no button lock
            InputHeader = PluginGuiLocalizeStrings.Get(102); //New TvWish: Search For:
            InputTextBoxSkin = string.Empty;
            EvaluateInputTextBoxCase = (int)TvWishEntries.active; //cheated - used for OnButtonNew()
            ScreenManager.ShowDialog(TVWISHLIST_EDIT_INPUT_TEXTBOX_SCREEN);
        }

        public void OnButtonMore()
        {
            Log.Debug("EditGUI: OnButtonMore() started");

            _dialogMenuItemList.Clear();

            DialogHeader = PluginGuiLocalizeStrings.Get(2107);

            ListItem myitem = new ListItem();
            if (ALLITEMS)
                myitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(2451));//User Defined Items
            else
                myitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(2450));//All Items

            myitem.Command = new MethodDelegateCommand(() =>
            {
                MoreButtonEvaluation(1); //starts at 0, but should be 1
                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                mycommandscreenManager.CloseTopmostDialog();
            });
            _dialogMenuItemList.Add(myitem);

            myitem = new ListItem();
            myitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(2452));//View Results
            myitem.Command = new MethodDelegateCommand(() =>
            {
                MoreButtonEvaluation(2); //starts at 0, but should be 1
                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                mycommandscreenManager.CloseTopmostDialog();
            });
            _dialogMenuItemList.Add(myitem);

            myitem = new ListItem();
            myitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(2453));//Copy wish to other mode
            myitem.Command = new MethodDelegateCommand(() =>
            {
                MoreButtonEvaluation(3); //starts at 0, but should be 1
                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                mycommandscreenManager.CloseTopmostDialog();
            });
            _dialogMenuItemList.Add(myitem);

            myitem = new ListItem();
            myitem.SetLabel("Name", PluginGuiLocalizeStrings.Get(2456));//clone tvwish
            myitem.Command = new MethodDelegateCommand(() =>
            {
                MoreButtonEvaluation(4); //starts at 0, but should be 1
                IScreenManager mycommandscreenManager = ServiceRegistration.Get<IScreenManager>();
                mycommandscreenManager.CloseTopmostDialog();
            });
            _dialogMenuItemList.Add(myitem);

            //update for dialog skin
            DialogMenuItemList.FireChange();

            //will now call a dialogbox with a given menu            
            ScreenManager.ShowDialog(TVWISHLIST_EDIT_DIALOG_MENU_SCREEN);
        }

        #endregion Public Methods


    }    
}
