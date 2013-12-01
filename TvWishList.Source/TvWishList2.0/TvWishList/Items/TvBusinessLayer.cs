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

namespace MediaPortal.Plugins.TvWishList.Items
{
    class TvBusinessLayer
    {
        //ILogger Log = ServiceRegistration.Get<ILogger>(); 

        public TvBusinessLayer()
        {
            //Log.Debug("TvBusinessLayer class created");
        }


        public Setting GetSetting(string tagName, string defaultValue)
        {
            
            Setting mysetting = new Setting();
            mysetting.Tag = tagName;
            
            //TVDatabase.Entities.Setting setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue(tagName, defaultValue);
            mysetting.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue(tagName, defaultValue);           
            //Log.Debug("TvBusinessLayer:  GetSetting(string tagName=" + mysetting.Tag + ", string Value=" + mysetting.Value + ", string default ="+defaultValue );
            return mysetting;
             
        }

        

        //Schedule schedule = layer.AddSchedule(minprogram.IdChannel, minprogram.Title, minprogram.StartTime, minprogram.EndTime, 0);
        public Schedule AddSchedule(int IdChannel, string Title, DateTime StartTime, DateTime EndTime, int ScheduleType)
        {
            Schedule newschedule = new Schedule(IdChannel, Title, StartTime, EndTime);
            newschedule.ScheduleType = ScheduleType;
            return newschedule;
        }
                                  
    }
}