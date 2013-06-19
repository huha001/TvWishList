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
using System.Diagnostics;
using System.IO;

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

using MediaPortal.Plugins.TvWishListMP2.Settings; //needed for configuration setting loading

using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishListMP2.Models
{
    public class ConfigDefaults_GUI : Common_Edit_AND_ConfigDefaults, IWorkflowModel, IDisposable
    {            

        #region Localized Resources

        //must be model ID defined in workflow and plugin.xml
        new public const string CONFIG_DEFAULTS_GUI_MODEL_ID_STR = "2be92395-da83-4702-a0f7-eb9b13110fff";
        new public readonly static Guid CONFIG_DEFAULTS_GUI_MODEL_ID = new Guid(CONFIG_DEFAULTS_GUI_MODEL_ID_STR);

        public const string TVWISHLIST_CONFIG_DEFAULTS_DIALOG_MENU_SCREEN = "TvWishListConfigDefaultsDialogMenu";             
        public const string TVWISHLIST_CONFIG_DEFAULTS_INPUT_TEXTBOX_SCREEN = "TvWishListConfigDefaultsInputTextBox";
        public const string TVWISHLIST_CONFIG_DEFAULTS_DIALOG_MENU2_SCREEN = "TvWishListConfigDefaultsDialogMenu2";
        #endregion

        #region Global Variables

        

        //Register global Services
        //ILogger Log = ServiceRegistration.Get<ILogger>();
       

        
        //Instance
        public static ConfigDefaults_GUI _instance = null;

        //TvWishProcessing myTvWishes = null;

        //int[] _listTranslator = new int[(int)TvWishEntries.end];

        

        #endregion Global Variables
        
        //Skin Properties are in Base Class

        //reusing most code of Edit_GUI via Base Class Common_Edit_AND_ConfigDefaults

        

        #region Constructor and Dispose

        /// <summary>
        /// Constructor... this one is called by the WorkflowManager when this model is loaded due to a screen reference.
        /// </summary>
        public ConfigDefaults_GUI()  
        {
            Log.Debug("ConfigDefaults_GUI: Constructor called");
            _instance = this; //needed to ensure transfer from static function later on

            //update screens in base class
            this.TvWishList_Dialog_Menu_Screen = TVWISHLIST_CONFIG_DEFAULTS_DIALOG_MENU_SCREEN;
            this.TvWishList_Dialog_Menu2_Screen = TVWISHLIST_CONFIG_DEFAULTS_DIALOG_MENU2_SCREEN;
            this.TvWishList_Input_Textbox_Screen = TVWISHLIST_CONFIG_DEFAULTS_INPUT_TEXTBOX_SCREEN;

            // WProperties are defined already in base class
            
            //define DefaultTvWish Class
            myTvWishes = new TvWishProcessing();

            IList<Channel> allChannels = Channel.ListAll();
            IList<ChannelGroup> allChannelGroups = ChannelGroup.ListAll();
            IList<RadioChannelGroup> allRadioChannelGroups = RadioChannelGroup.ListAll();
            IList<Card> allCards = Card.ListAll();

            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;

            setting = layer.GetSetting("TvWishList_ColumnSeparator", ";");
            char TvWishItemSeparator = setting.Value[0];

            setting = layer.GetSetting("preRecordInterval", "5");
            string prerecord = setting.Value;

            setting = layer.GetSetting("postRecordInterval", "5");
            string postrecord = setting.Value;

            myTvWishes.TvServerSettings(prerecord, postrecord, allChannelGroups, allRadioChannelGroups, allChannels, allCards, TvWishItemSeparator);                             
        }

        public void Dispose()
        {
            //seems to be usable for MP1 function DeInit()
            Log.Debug("ConfigDefaults_GUI:Dispose() - disposing");           
        }

        #endregion Constructor and Dispose

        #region IWorkflowModel implementation
        // to use this you must have derived the class from IWorkflowModel, IDisposable
        // and you must have defined in plugin.xml a workflowstate
        // WorkflowModel="023c44f2-3329-4781-9b4a-c974444c0b0d"/> <!-- MyTestPlugin Model -->

        public Guid ModelId
        {
            get { return CONFIG_DEFAULTS_GUI_MODEL_ID; }
        }

        public bool CanEnterState(NavigationContext oldContext, NavigationContext newContext)
        {
            return true;
        }

        public void EnterModelContext(NavigationContext oldContext, NavigationContext newContext)
        {
            LoadSettings();
        }

        public void ExitModelContext(NavigationContext oldContext, NavigationContext newContext)
        {
            
        }

        public void ChangeModelContext(NavigationContext oldContext, NavigationContext newContext, bool push)
        {
        }

        public void Deactivate(NavigationContext oldContext, NavigationContext newContext)
        {           
        }

        public void Reactivate(NavigationContext oldContext, NavigationContext newContext)
        {         
        }

        public void UpdateMenuActions(NavigationContext context, IDictionary<Guid, WorkflowAction> actions)
        {
        }

        public ScreenUpdateMode UpdateScreen(NavigationContext context, ref string screen)
        {
            return ScreenUpdateMode.AutoWorkflowManager;
        }

        


        #endregion

        #region public methods

        public void SaveSettings()
        {
            Log.Debug("ConfigDefaults_GUI: SaveSettings() called");
            ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
            TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();

            string defaultTvWishString = myTvWishes.SaveToString();
            Log.Debug("defaultTvWishString=" + defaultTvWishString);
            settings.UserDefaultFormatsString = defaultTvWishString;
            settingsManager.Save(settings);
        }


        public void LoadSettings()
        {
            Log.Debug("ConfigDefaults_GUI: LoadSettings() called");
            ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
            TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();           

            string defaultTvWishString = settings.UserDefaultFormatsString;
            Log.Debug("defaultTvWishString=" + defaultTvWishString);
            myTvWishes.Clear();
            if (defaultTvWishString == String.Empty)
            {
                TvWish mydefaultwish = myTvWishes.DefaultData();
               
                myTvWishes.Add(mydefaultwish);
                Log.Debug("New wish created");
            }
            else
            {
                myTvWishes.ValidSearchCriteria = false; //add defaultwish with empty search criteria
                myTvWishes.LoadFromString(defaultTvWishString, true);
            }

            DialogSleep = settings.DialogSleep;

            Log.Debug("mydefaultwish myTvWishes.ListAll().Count=" + myTvWishes.ListAll().Count);
            

            UpdateListItems();
        }


        public void HelpButton()
        {
            Log.Debug("ConfigDefaults_GUI: Help() started");
            //Help
            Process proc = new Process();
            ProcessStartInfo procstartinfo = new ProcessStartInfo();
            procstartinfo.FileName = "TvWishList.pdf";
            InstallPaths instpaths = new InstallPaths();
            instpaths.GetInstallPathsMP2();
            instpaths.GetMediaPortalDirsMP2();
            procstartinfo.WorkingDirectory = instpaths.DIR_MP2_Plugins + @"\TvWishListMP2";
            proc.StartInfo = procstartinfo;
            try
            {
                proc.Start();
            }
            catch
            {
                Log.Error("Could not open " + procstartinfo.WorkingDirectory + "\\" + procstartinfo.FileName, "Error");
            }
        }

        public void Defaults()
        {
            //define DefaultTvWish Class
             myTvWishes.Clear();
             TvWish mydefaultwish = myTvWishes.DefaultData();
             myTvWishes.Add(mydefaultwish);
             UpdateListItems();
             Log.Debug("Defaults restored");
        }


        #endregion public methods

    }    
}
