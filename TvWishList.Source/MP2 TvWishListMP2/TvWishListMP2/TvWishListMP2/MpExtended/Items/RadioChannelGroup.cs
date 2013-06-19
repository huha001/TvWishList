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
using MediaPortal.Common;
using MediaPortal.Plugins.TvWishListMP2.Models;
using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishListMP2.MPExtended
{
    public class RadioChannelGroup : IRadioChannelGroup
    {
        #region RadioChannelGroup Member

        public int Id { get; set; }

        public string GroupName { get; set; }

        #endregion RadioChannelGroup Member


        #region public methods
        public static IList<RadioChannelGroup> ListAll()
        {
            IList<IRadioChannelGroup> allIradiochannelgroups = null;

            if (Main_GUI.MyTvProvider == null)
            {
                Main_GUI.MyTvProvider = ServiceRegistration.Get<ITvWishListTVProvider>();
            }

            if (Main_GUI.MyTvProvider.Initialized == false)
            {
                Main_GUI.MyTvProvider.Init();
            }

            Main_GUI.MyTvProvider.ReadAllRadioChannelGroups(out allIradiochannelgroups);

            IList<RadioChannelGroup> allradiochannelgroups = new List<RadioChannelGroup>();
            foreach (IRadioChannelGroup myIradiochannelgroup in allIradiochannelgroups)
            {
                //Log.Debug("mygroupname = " + myIradiochannelgroup.GroupName);
                allradiochannelgroups.Add(new RadioChannelGroup { Id = myIradiochannelgroup.Id, GroupName = myIradiochannelgroup.GroupName });
            }

            //Log.Debug("RadioChannelGroup Count=" + allradiochannelgroups.Count.ToString());
            return allradiochannelgroups;
            //return allradiochannelgroups.ConvertAll(o => (RadioChannelGroup)o);
        }
        #endregion public methods
    }

}
