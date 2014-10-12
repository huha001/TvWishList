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

using MediaPortal.Plugins;
using MediaPortal;
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

using MediaPortal.Plugins.TvWishList.Items;

using MediaPortal.Plugins.TvWishListMP2.Settings; //needed for configuration setting loading

using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishListMP2.Models
{
    public class ConfigMain_GUI : IWorkflowModel, IDisposable
    {

        #region Localized Resources

        //must be model ID defined in workflow and plugin.xml
        public const string CONFIG_MAIN_GUI_MODEL_ID_STR = "85295da2-829b-4e38-bdf0-e76f819fd6f5";
        public readonly static Guid CONFIG_MAIN_GUI_MODEL_ID = new Guid(CONFIG_MAIN_GUI_MODEL_ID_STR);

        protected const string LOAD_QUESTION_RESOURCE = "[TvWishListMP2.LoadQuestion]";
        protected const string LOAD_COMPLETE_RESOURCE = "[TvWishListMP2.LoadComplete]";
        protected const string LOAD_ERROR_RESOURCE = "[TvWishListMP2.LoadError]";

        protected const string SAVE_QUESTION_RESOURCE = "[TvWishListMP2.SaveQuestion]";
        protected const string SAVE_COMPLETE_RESOURCE = "[TvWishListMP2.SaveComplete]";
        protected const string SAVE_ERROR_RESOURCE = "[TvWishListMP2.SaveError]";

        protected const string ERROR_RESOURCE = "[TvWishListMP2.4305]";
        protected const string WARNING_RESOURCE = "[TvWishListMP2.4401]";
        protected const string INFO_RESOURCE = "[TvWishListMP2.4400]";

        #endregion

        #region Global Variables

        //Register global Services
        //ILogger Log = ServiceRegistration.Get<ILogger>();
        int DialogSleep = 100;
        bool WideSkin = true;
        #endregion Global Variables
         
        #region Properties for skins
        protected readonly AbstractProperty _verboseProperty;
        protected readonly AbstractProperty _disableQuickMenuProperty;
        protected readonly AbstractProperty _disableInfoMenuProperty;
        protected readonly AbstractProperty _timeOutProperty;

        public bool Verbose
        {
            get { return (bool)_verboseProperty.GetValue(); }
            set { _verboseProperty.SetValue(value); }
        }
        public bool DisableQuickMenu
        {
            get { return (bool)_disableQuickMenuProperty.GetValue(); }
            set { _disableQuickMenuProperty.SetValue(value); }
        }
        public bool DisableInfoMenu
        {
            get { return (bool)_disableInfoMenuProperty.GetValue(); }
            set { _disableInfoMenuProperty.SetValue(value); }
        }
        public string TimeOut
        {
            get { return (string)_timeOutProperty.GetValue(); }
            set { _timeOutProperty.SetValue(value); }
        }

        public AbstractProperty VerboseProperty
        {
            get { return _verboseProperty; }
        }
        public AbstractProperty DisableQuickMenuProperty
        {
            get { return _disableQuickMenuProperty; }
        }
        public AbstractProperty DisableInfoMenuProperty
        {
            get { return _disableInfoMenuProperty; }
        }
        public AbstractProperty TimeOutProperty
        {
            get { return _timeOutProperty; }
        }

        //do not forget Wproperties in constructor!
        #endregion Properties for skins

        #region #region Constructor and Dispose
        /// <summary>
        /// Constructor... this one is called by the WorkflowManager when this model is loaded due to a screen reference.
        /// </summary>
        public ConfigMain_GUI()  //will be called when the screen is the first time loaded not the same as Init() !!!
        {
            Log.Debug("ConfigMain_GUI: Constructor called");
            
            // In models, properties will always be WProperty instances. When using SProperties for screen databinding,            
            _verboseProperty = new WProperty(typeof(bool), false);
            _disableQuickMenuProperty = new WProperty(typeof(bool), false);
            _disableInfoMenuProperty = new WProperty(typeof(bool), true);
            _timeOutProperty = new WProperty(typeof(string), "60");                      
        }

        public void Dispose()
        {
            //seems to be usable for MP1 function DeInit()
            Log.Debug("ConfigMain_GUI: Dispose() - disposing");
        }

        #endregion Constructor and Dispose

        #region IWorkflowModel implementation
        // to use this you must have derived the class from IWorkflowModel, IDisposable
        // and you must have defined in plugin.xml a workflowstate
        // WorkflowModel="023c44f2-3329-4781-9b4a-c974444c0b0d"/> <!-- MyTestPlugin Model -->

        public Guid ModelId
        {
            get { return CONFIG_MAIN_GUI_MODEL_ID; }
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
            Log.Debug("ConfigMain_GUI: SaveSettings() called");
            ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
            TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();

            settings.Verbose = Verbose;
            settings.DisableQuickMenu = DisableQuickMenu;
            settings.DisableInfoWindow = DisableInfoMenu; //not used by user yet
            settings.TimeOut = TimeOut;
            settings.DialogSleep = DialogSleep; //not used by user yet - hidden setting
            settings.WideSkin = WideSkin; //not used by user yet - hidden setting
            
             
            settingsManager.Save(settings);
        }

        public void LoadSettings()
        {
            Log.Debug("ConfigMain_GUI: LoadSettings() called");
            ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
            TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();

            Verbose = settings.Verbose;
            DisableQuickMenu = settings.DisableQuickMenu;
            DisableInfoMenu = settings.DisableInfoWindow;
            TimeOut = settings.TimeOut;
            DialogSleep = settings.DialogSleep; //not used by user yet - hidden setting
            WideSkin = settings.WideSkin; //not used by user yet - hidden setting
        }
        
        public void HelpButton()
        {
            Log.Debug("ConfigMain_GUI: Help() started");
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

        public void ButtonTvServerSaveSettings()
        {
            //This is an example for a dialog box

            IDialogManager mydialog = ServiceRegistration.Get<IDialogManager>();
            Guid mydialogId = mydialog.ShowDialog(WARNING_RESOURCE, SAVE_QUESTION_RESOURCE, DialogType.YesNoDialog, false, DialogButtonType.Yes);

            DialogCloseWatcher dialogCloseWatcher = new DialogCloseWatcher(this, mydialogId, dialogResult =>
            { //this is watching the result of the dialog box and displaying in the Status label of the screen (do not forget to dispose)
                Log.Debug("dialogResult.ToString()=" + dialogResult.ToString());

                if (dialogResult.ToString() == DialogButtonType.Yes.ToString())
                {
                    try
                    {
                        SaveSettingsToTvServer();
                        mydialog.ShowDialog(INFO_RESOURCE, SAVE_COMPLETE_RESOURCE, DialogType.OkDialog, false, DialogButtonType.Ok);
                    }
                    catch (Exception exc)
                    {
                        Log.Debug("[TVWishListMP2] buttonTvserverSave_Click: Error SaveSettings: Exception " + exc.Message);
                        mydialog.ShowDialog(ERROR_RESOURCE, SAVE_ERROR_RESOURCE, DialogType.OkDialog, false, DialogButtonType.Ok);
                    }
                }
                return;
            });

        }

        public void ButtonTvServerLoadSettings()
        {
            IDialogManager mydialog = ServiceRegistration.Get<IDialogManager>();
            Guid mydialogId = mydialog.ShowDialog(WARNING_RESOURCE, LOAD_QUESTION_RESOURCE, DialogType.YesNoDialog, false, DialogButtonType.Yes);

            DialogCloseWatcher dialogCloseWatcher = new DialogCloseWatcher(this, mydialogId, dialogResult =>
            { //this is watching the result of the dialog box and displaying in the Status label of the screen (do not forget to dispose)
                Log.Debug("dialogResult.ToString()=" + dialogResult.ToString());

                if (dialogResult.ToString() == DialogButtonType.Yes.ToString())
                {
                    try
                    {
                        LoadSettingsFromTvServer();
                        mydialog.ShowDialog(INFO_RESOURCE, LOAD_COMPLETE_RESOURCE, DialogType.OkDialog, false, DialogButtonType.Ok);
                    }
                    catch (Exception exc)
                    {
                        Log.Debug("[TVWishListMP2] buttonTvserverLoad_Click: Error LoadSettings: Exception " + exc.Message);
                        mydialog.ShowDialog(ERROR_RESOURCE, LOAD_ERROR_RESOURCE, DialogType.OkDialog, false, DialogButtonType.Ok);
                    }
                }
                return;
            });
        }

        #endregion public methods

        #region private methods


        private void SaveSettingsToTvServer()
        {
            //save client settings tom Tv server
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;

            ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
            TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxDebug", "false");
            setting.Value = settings.Verbose.ToString();
            Log.DebugValue = settings.Verbose;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxAction", "false");
            setting.Value = settings.Action.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxActive", "false");
            setting.Value = settings.Active.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxAfterDays", "false");
            setting.Value = settings.AfterDays.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxAfterTime", "false");
            setting.Value = settings.AfterTime.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxBeforeDay", "false");
            setting.Value = settings.BeforeDays.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxBeforeTime", "false");
            setting.Value = settings.BeforeTime.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxChannel", "false");
            setting.Value = settings.Channel.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodeName", "false");
            setting.Value = settings.EpisodeName.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodeNumber", "false");
            setting.Value = settings.EpisodeNumber.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodePart", "false");
            setting.Value = settings.EpisodePart.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxExclude", "false");
            setting.Value = settings.Exclude.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxGroup", "false");
            setting.Value = settings.Group.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxKeepEpisodes", "false");
            setting.Value = settings.KeepEpisodes.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxKeepUntil", "false");
            setting.Value = settings.KeepUntil.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxMatchType", "false");
            setting.Value = settings.MatchType.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxName", "false");
            setting.Value = settings.Name.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxPostrecord", "false");
            setting.Value = settings.PostRecord.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxPrerecord", "false");
            setting.Value = settings.PreRecord.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxPriority", "false");
            setting.Value = settings.Priority.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxRecommendedCard", "false");
            setting.Value = settings.RecommendedCard.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxRecordtype", "false");
            setting.Value = settings.RecordType.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxSkip", "false");
            setting.Value = settings.Skip.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxSeriesNumber", "false");
            setting.Value = settings.SeriesNumber.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxUseFolderNames", "false");
            setting.Value = settings.UseFolderName.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxWithinNextHours", "false");
            setting.Value = settings.WithinNextHours.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodeCriteria", "false");
            setting.Value = settings.EpisodeCriteria.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxPreferredGroup", "false");
            setting.Value = settings.PreferredGroup.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxIncludeRecordings", "false");
            setting.Value = settings.IncludeRecordings.ToString();
            setting.Persist();

            //modify for listview table changes

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxDisableInfoWindow", "false");
            setting.Value = settings.DisableInfoWindow.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxDisableQuickMenu", "false");
            setting.Value = settings.DisableQuickMenu.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_DateTimeFormat", "");
            setting.Value = settings.DateAndTimeFormat;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_MainItemFormat", "");
            setting.Value = settings.MainItemFormat;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_EmailMainFormat", "");
            setting.Value = settings.EmailMainTextBoxFormat;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_EmailResultsFormat", "");
            setting.Value = settings.EmailResultsTextBoxFormat;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_ResultsItemFormat", "");
            setting.Value = settings.ResultItemFormat;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_ViewMainFormat", "");
            setting.Value = settings.ViewMainTextBoxFormat;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_ViewResultsFormat", "");
            setting.Value = settings.ViewResultsTextBoxFormat;
            setting.Persist();


            setting = layer.GetSetting("TvWishList_ClientSetting_TimeOut", "60");
            setting.Value = settings.TimeOut.ToString();        
            setting.Persist();
            
            setting = layer.GetSetting("TvWishList_ClientSetting_DefaultFormats", "");
            setting.Value = settings.UserDefaultFormatsString;
            setting.Persist();
        }

        private bool LoadSettingsFromTvServer()
        {

            //get client settings from Tv server
            TvBusinessLayer layer = new TvBusinessLayer();

            ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
            TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();

            settings.Verbose = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxDebug", "false").Value);
            settings.Action = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxAction", "true").Value);
            settings.Active = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxActive", "false").Value);
            settings.AfterDays = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxAfterDays", "false").Value);
            settings.AfterTime = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxAfterTime", "false").Value);
            settings.BeforeDays = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxBeforeDay", "false").Value);
            settings.BeforeTime = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxBeforeTime", "false").Value);
            settings.Channel = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxChannel", "true").Value);
            settings.EpisodeName = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodeName", "false").Value);
            settings.EpisodeNumber = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodeNumber", "false").Value);
            settings.EpisodePart = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodePart", "false").Value);
            settings.Exclude = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxExclude", "true").Value);
            settings.Group = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxGroup", "true").Value);
            //settings.Hits = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxHits", "true").Value);
            settings.KeepEpisodes = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxKeepEpisodes", "false").Value);
            settings.KeepUntil = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxKeepUntil", "false").Value);
            settings.MatchType = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxMatchType", "true").Value);
            settings.Name = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxName", "true").Value);
            settings.PostRecord = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxPostrecord", "false").Value);
            settings.PreRecord = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxPrerecord", "false").Value);
            settings.Priority = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxPriority", "false").Value);
            settings.RecommendedCard = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxRecommendedCard", "false").Value);
            settings.RecordType = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxRecordtype", "true").Value);
            // SearchFor is always true;
            settings.SeriesNumber = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxSeriesNumber", "false").Value);
            settings.Skip = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxSkip", "true").Value);
            settings.UseFolderName = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxUseFolderNames", "false").Value);
            settings.WithinNextHours = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxRecordtype", "false").Value);
            settings.EpisodeCriteria = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodeCriteria", "false").Value);
            settings.PreferredGroup = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxPreferredGroup", "false").Value);
            settings.IncludeRecordings = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxIncludeRecordings", "false").Value);
            //modify for listview table changes

            settings.DisableInfoWindow = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxDisableInfoWindow", "false").Value);
            settings.DisableQuickMenu = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxDisableQuickMenu", "false").Value);

            

            //filenametextBox.Text=layer.GetSetting("TvWishList_ClientSetting_TvwishlistFolder", "");
            //Log.Debug("[TVWishListMP] TvWishListSetup: loadsettings: " + filenametextBox.Text);

            settings.DateAndTimeFormat = layer.GetSetting("TvWishList_ClientSetting_DateTimeFormat", "").Value;
            settings.MainItemFormat = layer.GetSetting("TvWishList_ClientSetting_MainItemFormat", "").Value;
            settings.EmailMainTextBoxFormat = layer.GetSetting("TvWishList_ClientSetting_EmailMainFormat", "").Value;
            settings.EmailResultsTextBoxFormat = layer.GetSetting("TvWishList_ClientSetting_EmailResultsFormat", "").Value;
            settings.ResultItemFormat = layer.GetSetting("TvWishList_ClientSetting_ResultsItemFormat", "").Value;
            settings.ViewMainTextBoxFormat = layer.GetSetting("TvWishList_ClientSetting_ViewMainFormat", "").Value;
            settings.ViewResultsTextBoxFormat = layer.GetSetting("TvWishList_ClientSetting_ViewResultsFormat", "").Value;
            settings.TimeOut = layer.GetSetting("TvWishList_ClientSetting_TimeOut", "60").Value;
            settings.UserDefaultFormatsString = layer.GetSetting("TvWishList_ClientSettingDefaultFormats", "").Value;
     
            settingsManager.Save(settings);

            LoadSettings(); //update skin now

            return true;
        }
        

        #endregion private methods


    }    
}
