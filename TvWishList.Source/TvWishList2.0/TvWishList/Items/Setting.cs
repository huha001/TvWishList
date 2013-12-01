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
using System.IO;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;

using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities.Cache;
using System.Data.Objects;

using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishList.Items
{

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
            //Log.Debug("Setting:  Persist() Tag=" + Tag + " Value=" + Value);
            ServiceAgents.Instance.SettingServiceAgent.SaveValue(Tag, Value);
        }


        public void Remove()
        {
            try
            {
                DeleteSettings(this.Tag);
            }
            catch (Exception exc)
            {
                Log.Error("Failed to remove setting "+this.Tag+", Value="+this.Value);
                Log.Error("Exception message was:" + exc.Message);
            }           
        }

        

        public static void DeleteSettings(string tagName)
        {
            using (ISettingsRepository settingRepository = new SettingsRepository(true)) 
            {    
                
                settingRepository.Delete<Mediaportal.TV.Server.TVDatabase.Entities.Setting>(s => s.Tag == tagName);
                settingRepository.UnitOfWork.SaveChanges();                
            }
        }

        public static IList<Setting> QuerySettings()
        {
            using (ISettingsRepository settingRepository = new SettingsRepository(true))
            {
                IQueryable<Mediaportal.TV.Server.TVDatabase.Entities.Setting> query = settingRepository.GetQuery<Mediaportal.TV.Server.TVDatabase.Entities.Setting>(s => s.Tag == "TvWishList_test");
                IList<Mediaportal.TV.Server.TVDatabase.Entities.Setting> newsettings = query.ToList();
                Log.Debug("Query Setting: number of found setting = " + newsettings.Count.ToString());

                IList<Setting> allsettings = new List<Setting>();
                foreach (Mediaportal.TV.Server.TVDatabase.Entities.Setting mysetting in newsettings)
                {
                    Setting newsetting = new Setting();
                    newsetting.Tag = mysetting.Tag;
                    newsetting.Value = mysetting.Value;
                    allsettings.Add(newsetting);
                    //Log.Debug("Tag=" + newsetting.Tag + "  Value=" + newsetting.Value);
                }

                return allsettings;
            }
        }

        public static IList<Setting> ListAllSettings()
        {
            using (ISettingsRepository settingsRepository = new SettingsRepository(true))
            {

                IQueryable<Mediaportal.TV.Server.TVDatabase.Entities.Setting> settings = settingsRepository.GetAll<Mediaportal.TV.Server.TVDatabase.Entities.Setting>();

                IList<Mediaportal.TV.Server.TVDatabase.Entities.Setting> newsettings = settings.ToList();

                Log.Debug("ListAllSettings: number of found setting = " + newsettings.Count.ToString());
                
                IList<Setting> allsettings = new List<Setting>();
                foreach (Mediaportal.TV.Server.TVDatabase.Entities.Setting mysetting in newsettings)
                {
                    Setting newsetting = new Setting();
                    newsetting.Tag = mysetting.Tag;
                    newsetting.Value = mysetting.Value;
                    allsettings.Add(newsetting);
                    //Log.Debug("Tag=" + newsetting.Tag + "  Value=" + newsetting.Value);
                }

                return allsettings;
            }
        }

    }

}