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

using MediaPortal.Plugins.TvWishListMP2.Settings;

using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishListMP2.Models
{
    public class ConfigFilter_GUI : IWorkflowModel, IDisposable
    {

        #region Localized Resources

        //must be model ID defined in workflow and plugin.xml
        public const string CONFIG_FILTER_GUI_MODEL_ID_STR = "cc50b5a4-f6e1-4c14-97d1-59c27714ac2f";
        public readonly static Guid CONFIG_FILTER_GUI_MODEL_ID = new Guid(CONFIG_FILTER_GUI_MODEL_ID_STR);
        
        #endregion

        #region Global Variables

        //Register global Services
        //ILogger Log = ServiceRegistration.Get<ILogger>();
        
        #endregion Global Variables
         
        #region Properties for Skins

        /// <summary>
        /// This property holds a string that we will modify in this tutorial.
        /// </summary>
        
        protected readonly AbstractProperty _activeProperty;
        //protected readonly AbstractProperty _searchForProperty;
        protected readonly AbstractProperty _matchTypeProperty;
        protected readonly AbstractProperty _groupProperty;
        protected readonly AbstractProperty _recordTypeProperty;
        protected readonly AbstractProperty _actionProperty;
        protected readonly AbstractProperty _excludeProperty;
        //protected readonly AbstractProperty _viewedProperty;
        protected readonly AbstractProperty _preRecordProperty;
        protected readonly AbstractProperty _postRecordProperty;
        protected readonly AbstractProperty _episodeNameProperty;
        protected readonly AbstractProperty _episodePartProperty;
        protected readonly AbstractProperty _episodeNumberProperty;
        protected readonly AbstractProperty _seriesNumberProperty;
        protected readonly AbstractProperty _keepEpisodesProperty;
        protected readonly AbstractProperty _keepUntilProperty;
        protected readonly AbstractProperty _recommendedCardProperty;
        protected readonly AbstractProperty _priorityProperty;
        protected readonly AbstractProperty _afterTimeProperty;
        protected readonly AbstractProperty _beforeTimeProperty;
        protected readonly AbstractProperty _afterDaysProperty;
        protected readonly AbstractProperty _beforeDaysProperty;
        protected readonly AbstractProperty _channelProperty;
        protected readonly AbstractProperty _skipProperty;
        protected readonly AbstractProperty _nameProperty;
        protected readonly AbstractProperty _useFolderNameProperty;
        protected readonly AbstractProperty _withinNextHoursProperty;
        //protected readonly AbstractProperty _scheduledProperty;
        //protected readonly AbstractProperty _tvwishidProperty;
        //protected readonly AbstractProperty _recordedProperty;
        //protected readonly AbstractProperty _deletedProperty;
        //protected readonly AbstractProperty _emailedProperty;
        //protected readonly AbstractProperty _conflictsProperty;
        protected readonly AbstractProperty _episodeCriteriaProperty;
        protected readonly AbstractProperty _preferredGroupProperty;
        protected readonly AbstractProperty _includeRecordingsProperty;
        

        public bool Active
        {
            get { return (bool)_activeProperty.GetValue(); }
            set { _activeProperty.SetValue(value); }
        }
        /*
        public bool SearchFor
        {
            get { return (bool)_searchForProperty.GetValue(); }
            set { _searchForProperty.SetValue(value); }
        }*/
        public bool MatchType
        {
            get { return (bool)_matchTypeProperty.GetValue(); }
            set { _matchTypeProperty.SetValue(value); }
        }
        public bool Group
        {
            get { return (bool)_groupProperty.GetValue(); }
            set { _groupProperty.SetValue(value); }
        }
        public bool RecordType
        {
            get { return (bool)_recordTypeProperty.GetValue(); }
            set { _recordTypeProperty.SetValue(value); }
        }
        public bool Action
        {
            get { return (bool)_actionProperty.GetValue(); }
            set { _actionProperty.SetValue(value); }
        }
        public bool Exclude
        {
            get { return (bool)_excludeProperty.GetValue(); }
            set { _excludeProperty.SetValue(value); }
        }
        /*
        public bool Viewed
        {
            get { return (bool)_viewedProperty.GetValue(); }
            set { _viewedProperty.SetValue(value); }
        }*/
        public bool PreRecord
        {
            get { return (bool)_preRecordProperty.GetValue(); }
            set { _preRecordProperty.SetValue(value); }
        }
        public bool PostRecord
        {
            get { return (bool)_postRecordProperty.GetValue(); }
            set { _postRecordProperty.SetValue(value); }
        }
        public bool EpisodeName
        {
            get { return (bool)_episodeNameProperty.GetValue(); }
            set { _episodeNameProperty.SetValue(value); }
        }
        public bool EpisodePart
        {
            get { return (bool)_episodePartProperty.GetValue(); }
            set { _episodePartProperty.SetValue(value); }
        }
        public bool EpisodeNumber
        {
            get { return (bool)_episodeNumberProperty.GetValue(); }
            set { _episodeNumberProperty.SetValue(value); }
        }
        public bool SeriesNumber
        {
            get { return (bool)_seriesNumberProperty.GetValue(); }
            set { _seriesNumberProperty.SetValue(value); }
        }
        public bool KeepEpisodes
        {
            get { return (bool)_keepEpisodesProperty.GetValue(); }
            set { _keepEpisodesProperty.SetValue(value); }
        }
        public bool KeepUntil
        {
            get { return (bool)_keepUntilProperty.GetValue(); }
            set { _keepUntilProperty.SetValue(value); }
        }
        public bool RecommendedCard
        {
            get { return (bool)_recommendedCardProperty.GetValue(); }
            set { _recommendedCardProperty.SetValue(value); }
        }
        public bool Priority
        {
            get { return (bool)_priorityProperty.GetValue(); }
            set { _priorityProperty.SetValue(value); }
        }
        public bool AfterTime
        {
            get { return (bool)_afterTimeProperty.GetValue(); }
            set { _afterTimeProperty.SetValue(value); }
        }
        public bool BeforeTime
        {
            get { return (bool)_beforeTimeProperty.GetValue(); }
            set { _beforeTimeProperty.SetValue(value); }
        }
        public bool AfterDays
        {
            get { return (bool)_afterDaysProperty.GetValue(); }
            set { _afterDaysProperty.SetValue(value); }
        }
        public bool BeforeDays
        {
            get { return (bool)_beforeDaysProperty.GetValue(); }
            set { _beforeDaysProperty.SetValue(value); }
        }
        public bool Channel
        {
            get { return (bool)_channelProperty.GetValue(); }
            set { _channelProperty.SetValue(value); }
        }
        public bool Skip
        {
            get { return (bool)_skipProperty.GetValue(); }
            set { _skipProperty.SetValue(value); }
        }
        public bool Name
        {
            get { return (bool)_nameProperty.GetValue(); }
            set { _nameProperty.SetValue(value); }
        }
        public bool UseFolderName
        {
            get { return (bool)_useFolderNameProperty.GetValue(); }
            set { _useFolderNameProperty.SetValue(value); }
        }
        public bool WithinNextHours
        {
            get { return (bool)_withinNextHoursProperty.GetValue(); }
            set { _withinNextHoursProperty.SetValue(value); }
        }

        /*
        public bool Scheduled
        {
            get { return (bool)_scheduledProperty.GetValue(); }
            set { _scheduledProperty.SetValue(value); }
        }
        public bool Tvwishid
        {
            get { return (bool)_tvwishidProperty.GetValue(); }
            set { _tvwishidProperty.SetValue(value); }
        }
        public bool Recorded
        {
            get { return (bool)_recordedProperty.GetValue(); }
            set { _recordedProperty.SetValue(value); }
        }       
        public bool Deleted
        {
            get { return (bool)_deletedProperty.GetValue(); }
            set { _deletedProperty.SetValue(value); }
        }
        public bool Emailed
        {
            get { return (bool)_emailedProperty.GetValue(); }
            set { _emailedProperty.SetValue(value); }
        }
        public bool Conflicts
        {
            get { return (bool)_conflictsProperty.GetValue(); }
            set { _conflictsProperty.SetValue(value); }
        }*/

        public bool EpisodeCriteria
        {
            get { return (bool)_episodeCriteriaProperty.GetValue(); }
            set { _episodeCriteriaProperty.SetValue(value); }
        }
        public bool PreferredGroup
        {
            get { return (bool)_preferredGroupProperty.GetValue(); }
            set { _preferredGroupProperty.SetValue(value); }
        }
        public bool IncludeRecordings
        {
            get { return (bool)_includeRecordingsProperty.GetValue(); }
            set { _includeRecordingsProperty.SetValue(value); }
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
        public AbstractProperty ActiveProperty
        {
            get { return _activeProperty; }
        }
        /*
        public AbstractProperty SearchForProperty
        {
            get { return _searchForProperty; }
        }*/
        public AbstractProperty MatchTypeProperty
        {
            get { return _matchTypeProperty; }
        }
        public AbstractProperty GroupProperty
        {
            get { return _groupProperty; }
        }
        public AbstractProperty RecordTypeProperty
        {
            get { return _recordTypeProperty; }
        }
        public AbstractProperty ActionProperty
        {
            get { return _actionProperty; }
        }
        public AbstractProperty ExcludeProperty
        {
            get { return _excludeProperty; }
        }
        /*
        public AbstractProperty ViewedProperty
        {
            get { return _viewedProperty; }
        }*/
        public AbstractProperty PreRecordProperty
        {
            get { return _preRecordProperty; }
        }
        public AbstractProperty PostRecordProperty
        {
            get { return _postRecordProperty; }
        }
        public AbstractProperty EpisodeNameProperty
        {
            get { return _episodeNameProperty; }
        }
        public AbstractProperty EpisodePartProperty
        {
            get { return _episodePartProperty; }
        }
        public AbstractProperty EpisodeNumberProperty
        {
            get { return _episodeNumberProperty; }
        }
        public AbstractProperty SeriesNumberProperty
        {
            get { return _seriesNumberProperty; }
        }
        public AbstractProperty KeepEpisodesProperty
        {
            get { return _keepEpisodesProperty; }
        }
        public AbstractProperty KeepUntilProperty
        {
            get { return _keepUntilProperty; }
        }
        public AbstractProperty RecommendedCardProperty
        {
            get { return _recommendedCardProperty; }
        }
        public AbstractProperty PriorityProperty
        {
            get { return _priorityProperty; }
        }
        public AbstractProperty AfterTimeProperty
        {
            get { return _afterTimeProperty; }
        }
        public AbstractProperty BeforeTimeProperty
        {
            get { return _beforeTimeProperty; }
        }
        public AbstractProperty AfterDaysProperty
        {
            get { return _afterDaysProperty; }
        }
        public AbstractProperty BeforeDaysProperty
        {
            get { return _beforeDaysProperty; }
        }
        public AbstractProperty ChannelProperty
        {
            get { return _channelProperty; }
        }
        public AbstractProperty SkipProperty
        {
            get { return _skipProperty; }
        }
        public AbstractProperty NameProperty
        {
            get { return _nameProperty; }
        }
        public AbstractProperty UseFolderNameProperty
        {
            get { return _useFolderNameProperty; }
        }
        public AbstractProperty WithinNextHoursProperty
        {
            get { return _withinNextHoursProperty; }
        }
        /*
        public AbstractProperty ScheduledProperty
        {
            get { return _scheduledProperty; }
        }
        public AbstractProperty TvwishidProperty
        {
            get { return _tvwishidProperty; }
        }
        public AbstractProperty RecordedProperty
        {
            get { return _recordedProperty; }
        }
        public AbstractProperty DeletedProperty
        {
            get { return _deletedProperty; }
        }
        public AbstractProperty EmailedProperty
        {
            get { return _emailedProperty; }
        }
        public AbstractProperty ConflictsProperty
        {
            get { return _conflictsProperty; }
        }*/
        public AbstractProperty EpisodeCriteriaProperty
        {
            get { return _episodeCriteriaProperty; }
        }
        public AbstractProperty PreferredGroupProperty
        {
            get { return _preferredGroupProperty; }
        }
        public AbstractProperty IncludeRecordingsProperty
        {
            get { return _includeRecordingsProperty; }
        }

        //do not forget Wproperties in constructor!
        #endregion Properties for Skins

        #region Constructor and Dispose

        /// <summary>
        /// Constructor... this one is called by the WorkflowManager when this model is loaded due to a screen reference.
        /// </summary>
        public ConfigFilter_GUI()  //will be called when the screen is the first time loaded not the same as Init() !!!
        {
            Log.Debug("ConfigFilter_GUI: Constructor called");
            
            // In models, properties will always be WProperty instances. When using SProperties for screen databinding,            
            _activeProperty = new WProperty(typeof(bool),true);
            //_searchForProperty = new WProperty(typeof(bool), true); 
            _matchTypeProperty = new WProperty(typeof(bool),true);
            _groupProperty = new WProperty(typeof(bool),true);
            _recordTypeProperty = new WProperty(typeof(bool),true);
            _actionProperty = new WProperty(typeof(bool),true);
            _excludeProperty = new WProperty(typeof(bool),true);
            //_viewedProperty = new WProperty(typeof(bool),true);
            _preRecordProperty = new WProperty(typeof(bool),true);
            _postRecordProperty = new WProperty(typeof(bool),true);
            _episodeNameProperty = new WProperty(typeof(bool),true);
            _episodePartProperty = new WProperty(typeof(bool),true);
            _episodeNumberProperty = new WProperty(typeof(bool),true);
            _seriesNumberProperty = new WProperty(typeof(bool),true);
            _keepEpisodesProperty = new WProperty(typeof(bool),true);
            _keepUntilProperty = new WProperty(typeof(bool),true);
            _recommendedCardProperty = new WProperty(typeof(bool),true);
            _priorityProperty = new WProperty(typeof(bool),true);
            _afterTimeProperty = new WProperty(typeof(bool),true);
            _beforeTimeProperty = new WProperty(typeof(bool),true);
            _afterDaysProperty = new WProperty(typeof(bool),true);
            _beforeDaysProperty = new WProperty(typeof(bool),true);
            _channelProperty = new WProperty(typeof(bool),true);
            _skipProperty = new WProperty(typeof(bool),true);
            _nameProperty = new WProperty(typeof(bool),true);
            _useFolderNameProperty = new WProperty(typeof(bool),true);
            _withinNextHoursProperty = new WProperty(typeof(bool), true);
            /*
            _scheduledProperty = new WProperty(typeof(bool),true);
            _tvwishidProperty = new WProperty(typeof(bool),true);
            _recordedProperty = new WProperty(typeof(bool),true);
            _deletedProperty = new WProperty(typeof(bool),true);
            _emailedProperty = new WProperty(typeof(bool),true);
            _conflictsProperty = new WProperty(typeof(bool),true);
             */
            _episodeCriteriaProperty = new WProperty(typeof(bool),true);
            _preferredGroupProperty = new WProperty(typeof(bool),true);
            _includeRecordingsProperty = new WProperty(typeof(bool), true);
        }

        public void Dispose()
        {
            //seems to be usable for MP1 function DeInit()
            Log.Debug("ConfigFilter_GUI:Dispose() - disposing");
        }

        #endregion Constructor and Dispose

        #region IWorkflowModel Implementation
        // to use this you must have derived the class from IWorkflowModel, IDisposable
        // and you must have defined in plugin.xml a workflowstate
        // WorkflowModel="023c44f2-3329-4781-9b4a-c974444c0b0d"/> <!-- MyTestPlugin Model -->

        public Guid ModelId
        {
            get { return CONFIG_FILTER_GUI_MODEL_ID; }
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

        #endregion IWorkflowModel Implementation

        #region Public Methods

        public void SaveSettings()
        {
            Log.Debug("ConfigFilter_GUI: SaveSettings() called");
            ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
            TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();

            settings.Active = Active;
            //settings.SearchFor = SearchFor;
            settings.MatchType = MatchType;
            settings.Group = Group;
            settings.RecordType = RecordType;
            settings.Action = Action;
            settings.Exclude = Exclude;
            //settings.Viewed = Viewed;
            settings.PreRecord = PreRecord;
            settings.PostRecord = PostRecord;
            settings.EpisodeName = EpisodeName;
            settings.EpisodePart = EpisodePart;
            settings.EpisodeNumber = EpisodeNumber;
            settings.SeriesNumber = SeriesNumber;
            settings.KeepEpisodes = KeepEpisodes;
            settings.KeepUntil = KeepUntil;
            settings.RecommendedCard = RecommendedCard;
            settings.Priority = Priority;
            settings.AfterTime = AfterTime;
            settings.BeforeTime = BeforeTime;
            settings.AfterDays = AfterDays;
            settings.BeforeDays = BeforeDays;
            settings.Channel = Channel;
            settings.Skip = Skip;
            settings.Name = Name;
            settings.UseFolderName = UseFolderName;
            settings.WithinNextHours = WithinNextHours;
            //settings.Scheduled = Scheduled;
            //settings.Tvwishid = Tvwishid;
            //settings.Recorded = Recorded;
            //settings.Deleted = Deleted;
            //settings.Emailed = Emailed;
            //settings.Conflicts = Conflicts;
            settings.EpisodeCriteria = EpisodeCriteria;
            settings.PreferredGroup = PreferredGroup;
            settings.IncludeRecordings = IncludeRecordings;
            
            settingsManager.Save(settings);
        }

        public void LoadSettings()
        {
            Log.Debug("ConfigFilter_GUI: LoadSettings() called");
            ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
            TvWishListMP2Settings settings = settingsManager.Load<TvWishListMP2Settings>();

            Active = settings.Active;
            //SearchFor = settings.SearchFor;
            MatchType = settings.MatchType;
            Group = settings.Group;
            RecordType = settings.RecordType;
            Action = settings.Action;
            Exclude = settings.Exclude;
            //Viewed = settings.Viewed;
            PreRecord = settings.PreRecord;
            PostRecord = settings.PostRecord;
            EpisodeName = settings.EpisodeName;
            EpisodePart = settings.EpisodePart;
            EpisodeNumber = settings.EpisodeNumber;
            SeriesNumber = settings.SeriesNumber;
            KeepEpisodes = settings.KeepEpisodes;
            KeepUntil = settings.KeepUntil;
            RecommendedCard = settings.RecommendedCard;
            Priority = settings.Priority;
            AfterTime = settings.AfterTime;
            BeforeTime = settings.BeforeTime;
            AfterDays = settings.AfterDays;
            BeforeDays = settings.BeforeDays;
            Channel = settings.Channel;
            Skip = settings.Skip;
            Name = settings.Name;
            UseFolderName = settings.UseFolderName;
            WithinNextHours = settings.WithinNextHours;
            //Scheduled = settings.Scheduled;
            //Tvwishid = settings.Tvwishid;
            //Recorded = settings.Recorded;
            //Deleted = settings.Deleted;
            //Emailed = settings.Emailed;
            //Conflicts = settings.Conflicts;
            EpisodeCriteria = settings.EpisodeCriteria;
            PreferredGroup = settings.PreferredGroup;
            IncludeRecordings = settings.IncludeRecordings;            
        }

        public void All()
        {
            Log.Debug("ConfigFilter_GUI: All() called");

            Active = true;
            //SearchFor = true;
            MatchType = true;
            Group = true;
            RecordType = true;
            Action = true;
            Exclude = true;
            //Viewed = true;
            PreRecord = true;
            PostRecord = true;
            EpisodeName = true;
            EpisodePart = true;
            EpisodeNumber = true;
            SeriesNumber = true;
            KeepEpisodes = true;
            KeepUntil = true;
            RecommendedCard = true;
            Priority = true;
            AfterTime = true;
            BeforeTime = true;
            AfterDays = true;
            BeforeDays = true;
            Channel = true;
            Skip = true;
            Name = true;
            UseFolderName = true;
            WithinNextHours = true;
            //Scheduled = true;
            //Tvwishid = true;
            //Recorded = true;
            //Deleted = true;
            //Emailed = true;
            //Conflicts = true;
            EpisodeCriteria = true;
            PreferredGroup = true;
            IncludeRecordings = true;
        }

        public void None()
        {
            Log.Debug("ConfigFilter_GUI: None() called");
            Active = false;
            //SearchFor = false;
            MatchType = false;
            Group = false;
            RecordType = false;
            Action = false;
            Exclude = false;
            //Viewed = false;
            PreRecord = false;
            PostRecord = false;
            EpisodeName = false;
            EpisodePart = false;
            EpisodeNumber = false;
            SeriesNumber = false;
            KeepEpisodes = false;
            KeepUntil = false;
            RecommendedCard = false;
            Priority = false;
            AfterTime = false;
            BeforeTime = false;
            AfterDays = false;
            BeforeDays = false;
            Channel = false;
            Skip = false;
            Name = false;
            UseFolderName = false;
            WithinNextHours = false;
            //Scheduled = false;
            //Tvwishid = false;
            //Recorded = false;
            //Deleted = false;
            //Emailed = false;
            //Conflicts = false;
            EpisodeCriteria = false;
            PreferredGroup = false;
            IncludeRecordings = false;
        }

        public void Defaults()
        {
            Log.Debug("ConfigFilter_GUI: Defaults() called");
            Active = false;
            //SearchFor = true;
            MatchType = true;
            Group = true;
            RecordType = true;
            Action = true;
            Exclude = false;
            //Viewed = false;
            PreRecord = false;
            PostRecord = false;
            EpisodeName = false;
            EpisodePart = false;
            EpisodeNumber = false;
            SeriesNumber = false;
            KeepEpisodes = false;
            KeepUntil = false;
            RecommendedCard = false;
            Priority = false;
            AfterTime = false;
            BeforeTime = false;
            AfterDays = false;
            BeforeDays = false;
            Channel = true;
            Skip = true;
            Name = true;
            UseFolderName = false;
            WithinNextHours = false;
            //Scheduled = false;
            //Tvwishid = false;
            //Recorded = false;
            //Deleted = false;
            //Emailed = false;
            //Conflicts = false;
            EpisodeCriteria = false;
            PreferredGroup = false;
            IncludeRecordings = false;
        }

        #endregion Public Methods

    }    
}
