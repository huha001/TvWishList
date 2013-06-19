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
using MediaPortal.Common.Logging;
using MediaPortal.Plugins.TvWishListMP2.Models;
using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishListMP2.MPExtended
{
    public class ChannelGroup : IChannelGroup
    {
        #region ChannelGroup Member

        public int Id { get; set; }

        public string GroupName { get; set; }

        #endregion ChannelGroup Member


        #region public methods
        public static IList<ChannelGroup> ListAll()
        {
            //Log.Debug("ChannelGroup ListAll");
            IList<IChannelGroup> allIchannelgroups = null;

            if (Main_GUI.MyTvProvider == null)
            {
                Main_GUI.MyTvProvider = ServiceRegistration.Get<ITvWishListTVProvider>();
                //Log.Debug("Service registered");
            }

            if (Main_GUI.MyTvProvider.Initialized == false)
            {
                Main_GUI.MyTvProvider.Init();
                //Log.Debug("Service initialized");
            }

            Main_GUI.MyTvProvider.ReadAllChannelGroups(out allIchannelgroups);
            //Log.Debug("Reading finished - count=" + allIchannelgroups.Count.ToString());

            IList<ChannelGroup> allchannelgroups = new List<ChannelGroup>();
            foreach (IChannelGroup myIchannelgroup in allIchannelgroups)
            {
                //Log.Debug("mygroupname = " + myIchannelgroup.GroupName);
                allchannelgroups.Add(new ChannelGroup { Id = myIchannelgroup.Id, GroupName = myIchannelgroup.GroupName });
            }

            //Log.Debug("ChannelGroup Count=" + allIchannelgroups.Count.ToString());

            //return allchannelgroups.ConvertAll(o => (ChannelGroup)o);
            return allchannelgroups;
        }
        #endregion public methods
    }

}
