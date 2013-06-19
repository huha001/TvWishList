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

//using MediaPortal.Plugins.TvWishListMP2.MPExtended;

using MediaPortal.Plugins.TvWishListMP2.Settings; //needed for configuration setting loading

using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishListMP2.Models
{
    public class ConfigFormats_GUI : IWorkflowModel, IDisposable
    {

        #region Localized Resources

        //must be model ID defined in workflow and plugin.xml
        public const string CONFIG_FORMATS_GUI_MODEL_ID_STR = "227cad91-e7ab-4458-b15a-54cbd657b8fb";
        public readonly static Guid CONFIG_FORMATS_GUI_MODEL_ID = new Guid(CONFIG_FORMATS_GUI_MODEL_ID_STR);
        #endregion

        #region Global Variables

        //Register global Services
       //ILogger Log = ServiceRegistration.Get<ILogger>();
        
        #endregion Global Variables
         
        #region Properties for skins

        /// <summary>
        /// This property holds a string that we will modify in this tutorial.
        /// </summary>
        protected readonly AbstractProperty _dateAndTimeFormatProperty;
        protected readonly AbstractProperty _mainItemFormatProperty;
        protected readonly AbstractProperty _resultItemFormatProperty;
        protected readonly AbstractProperty _emailMainTextBoxFormatProperty;
        protected readonly AbstractProperty _emailResultsTextBoxFormatProperty;
        protected readonly AbstractProperty _viewMainTextBoxFormatProperty;
        protected readonly AbstractProperty _viewResultsTextBoxFormatProperty;
        
        /// <summary>
        /// This sample property will be accessed by the hello_world screen. Note that the data type must be the same
        /// as given in the instantiation of our backing property <see cref="_helloStringProperty"/>.
        /// </summary>
        public string DateAndTimeFormat
        {
            get { return (string)_dateAndTimeFormatProperty.GetValue(); }
            set { _dateAndTimeFormatProperty.SetValue(value); }
        }
        public string MainItemFormat
        {
            get { return (string)_mainItemFormatProperty.GetValue(); }
            set { _mainItemFormatProperty.SetValue(value); }
        }
        public string ResultItemFormat
        {
            get { return (string)_resultItemFormatProperty.GetValue(); }
            set { _resultItemFormatProperty.SetValue(value); }
        }
        public string EmailMainTextBoxFormat
        {
            get { return (string)_emailMainTextBoxFormatProperty.GetValue(); }
            set { _emailMainTextBoxFormatProperty.SetValue(value); }
        }
        public string EmailResultsTextBoxFormat
        {
            get { return (string)_emailResultsTextBoxFormatProperty.GetValue(); }
            set { _emailResultsTextBoxFormatProperty.SetValue(value); }
        }
        public string ViewMainTextBoxFormat
        {
            get { return (string)_viewMainTextBoxFormatProperty.GetValue(); }
            set { _viewMainTextBoxFormatProperty.SetValue(value); }
        }
        public string ViewResultsTextBoxFormat
        {
            get { return (string)_viewResultsTextBoxFormatProperty.GetValue(); }
            set { _viewResultsTextBoxFormatProperty.SetValue(value); }
        }

        public AbstractProperty DateAndTimeFormatProperty
        {
            get { return _dateAndTimeFormatProperty; }
        }
        public AbstractProperty MainItemFormatProperty
        {
            get { return _mainItemFormatProperty; }
        }
        public AbstractProperty ResultItemFormatProperty
        {
            get { return _resultItemFormatProperty; }
        }
        public AbstractProperty EmailMainTextBoxFormatProperty
        {
            get { return _emailMainTextBoxFormatProperty; }
        }
        public AbstractProperty EmailResultsTextBoxFormatProperty
        {
            get { return _emailResultsTextBoxFormatProperty; }
        }
        public AbstractProperty ViewMainTextBoxFormatProperty
        {
            get { return _viewMainTextBoxFormatProperty; }
        }
        public AbstractProperty ViewResultsTextBoxFormatProperty
        {
            get { return _viewResultsTextBoxFormatProperty; }
        }
        

        //do not forget Wproperties in constructor!
        #endregion Properties for skins

        #region #region Constructor and Dispose
        /// <summary>
        /// Constructor... this one is called by the WorkflowManager when this model is loaded due to a screen reference.
        /// </summary>
        public ConfigFormats_GUI()  //will be called when the screen is the first time loaded not the same as Init() !!!
        {
            Log.Debug("ConfigFormats_GUI: Constructor called");
            
            // In models, properties will always be WProperty instances. When using SProperties for screen databinding,            
            _dateAndTimeFormatProperty = new WProperty(typeof(string), string.Empty);
            _mainItemFormatProperty = new WProperty(typeof(string), string.Empty);
            _resultItemFormatProperty = new WProperty(typeof(string), string.Empty);
            _emailMainTextBoxFormatProperty = new WProperty(typeof(string), string.Empty);
            _emailResultsTextBoxFormatProperty = new WProperty(typeof(string), string.Empty);
            _viewMainTextBoxFormatProperty = new WProperty(typeof(string), string.Empty);
            _viewResultsTextBoxFormatProperty = new WProperty(typeof(string), string.Empty);                                 
        }

        public void Dispose()
        {
            //seems to be usable for MP1 function DeInit()
            Log.Debug("ConfigFormats_GUI: Dispose() - disposing");
        }
        #endregion Constructor and Dispose

        #region IWorkflowModel implementation
        // to use this you must have derived the class from IWorkflowModel, IDisposable
        // and you must have defined in plugin.xml a workflowstate
        // WorkflowModel="023c44f2-3329-4781-9b4a-c974444c0b0d"/> <!-- MyTestPlugin Model -->

        public Guid ModelId
        {
            get { return CONFIG_FORMATS_GUI_MODEL_ID; }
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
            Log.Debug("ConfigFormats_GUI: SaveSettings() called");
            ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
            TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();

            settings.DateAndTimeFormat = DateAndTimeFormat;
            settings.MainItemFormat = MainItemFormat;
            settings.ResultItemFormat = ResultItemFormat;
            settings.EmailMainTextBoxFormat = EmailMainTextBoxFormat;
            settings.EmailResultsTextBoxFormat = EmailResultsTextBoxFormat;
            settings.ViewMainTextBoxFormat = ViewMainTextBoxFormat;
            settings.ViewResultsTextBoxFormat = ViewResultsTextBoxFormat;
            
            settingsManager.Save(settings);
        }

        public void LoadSettings()
        {
            Log.Debug("ConfigFormats_GUI: LoadSettings() called");
            ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
            TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();

            DateAndTimeFormat = settings.DateAndTimeFormat;
            MainItemFormat = settings.MainItemFormat;
            ResultItemFormat = settings.ResultItemFormat;
            EmailMainTextBoxFormat = settings.EmailMainTextBoxFormat;
            EmailResultsTextBoxFormat = settings.EmailResultsTextBoxFormat;
            ViewMainTextBoxFormat = settings.ViewMainTextBoxFormat;
            ViewResultsTextBoxFormat = settings.ViewResultsTextBoxFormat;
        }

        public void HelpButton()
        {
            Log.Debug("ConfigFormats_GUI: Help() started");
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


        #endregion public methods

    }    
}
