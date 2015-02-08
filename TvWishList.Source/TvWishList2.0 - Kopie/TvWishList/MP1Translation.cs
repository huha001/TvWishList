using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Log = TvLibrary.Log.huha.Log;

namespace Mediaportal.TV.Server.MP1Translation
{
    class TvBusinessLayer
    {
        //ILogger Log = ServiceRegistration.Get<ILogger>();

        public TvBusinessLayer()
        {
            Log.Debug("TvBusinessLayer class created");
        }


        public Setting GetSetting(string tagName, string defaultValue)
        {
            Log.Debug("TvBusinessLayer:  GetSetting(string tagName=" + tagName + ", string defaultValue=" + defaultValue);
            Setting mysetting = new Setting();
            TVDatabase.Entities.Setting setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue(tagName, defaultValue);
            mysetting.Tag = setting.Tag;
            mysetting.Value = setting.Value;
            return mysetting;

        }

        
    }

    public class Setting 
    {
        //ILogger Log = ServiceRegistration.Get<ILogger>();
        #region Setting Member

        public string Tag { get; set; }

        public string Value { get; set; }

        #endregion

        public Setting()
        {

        }

        public void Persist()
        {
            Log.Debug("Setting:  Persist()");
            ServiceAgents.Instance.SettingServiceAgent.SaveValue(Tag, Value);
        }


        public void Remove()
        {
            Log.Debug("Setting:  Remove() not implemented yet");

            Log.Error("Setting:  Remove() not implemented yet");
            
        }

    }

}
