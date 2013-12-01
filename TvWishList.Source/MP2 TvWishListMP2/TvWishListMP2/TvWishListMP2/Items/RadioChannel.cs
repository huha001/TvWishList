﻿#region Copyright (C) 2007-2012 Team MediaPortal
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

namespace MediaPortal.Plugins.TvWishList.Items
{
    public class RadioChannel : IRadioChannel
    {
        #region RadioChannel Member

        public int Id { get; set; }

        public string Name { get; set; }

        #endregion RadioChannel Member

        #region public methods
        public static IList<RadioChannel> ListAll()
        {
            IList<IRadioChannel> allIradiochannels = null;

            if (Main_GUI.MyTvProvider == null)
            {
                Main_GUI.MyTvProvider = ServiceRegistration.Get<ITvWishListTVProvider>();
            }

            if (Main_GUI.MyTvProvider.Initialized == false)
            {
                Main_GUI.MyTvProvider.Init();
            }

            Main_GUI.MyTvProvider.ReadAllRadioChannels(out allIradiochannels);

            IList<RadioChannel> allradiochannels = new List<RadioChannel>();
            foreach (IRadioChannel myradiochannel in allIradiochannels)
            {
                allradiochannels.Add(new RadioChannel { Id = myradiochannel.Id, Name = myradiochannel.Name });
            }

            return allradiochannels;
        }

        public static IList<RadioChannel> ListAllByGroup(int groupId)
        {
            IList<IRadioChannel> allIradiochannels = null;

            if (Main_GUI.MyTvProvider == null)
            {
                Main_GUI.MyTvProvider = ServiceRegistration.Get<ITvWishListTVProvider>();
            }

            if (Main_GUI.MyTvProvider.Initialized == false)
            {
                Main_GUI.MyTvProvider.Init();
            }

            Main_GUI.MyTvProvider.ReadAllRadioChannelsByGroup(groupId, out allIradiochannels);

            IList<RadioChannel> allradiochannels = new List<RadioChannel>();
            foreach (IRadioChannel myradiochannel in allIradiochannels)
            {
                allradiochannels.Add(new RadioChannel { Id = myradiochannel.Id, Name = myradiochannel.Name });
            }

            return allradiochannels;
        }
        #endregion public methods
    }
}
