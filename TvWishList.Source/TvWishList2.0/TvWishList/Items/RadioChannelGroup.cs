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
using Mediaportal.TV.Server;
using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishList.Items
{
    public class RadioChannelGroup 
    {
        #region RadioChannelGroup Member

        public int Id { get; private set; }

        public string GroupName { get; set; }

        #endregion RadioChannelGroup Member


        #region public methods

        public static IList<RadioChannelGroup> ListAll()
        {

            IList<Mediaportal.TV.Server.TVDatabase.Entities.ChannelGroup> rawradiochannelgroups = ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllChannelGroupsByMediaType(Mediaportal.TV.Server.TVDatabase.Entities.Enums.MediaTypeEnum.Radio);

            IList<RadioChannelGroup> allradiochannelgroups = new List<RadioChannelGroup>();
            foreach (Mediaportal.TV.Server.TVDatabase.Entities.ChannelGroup myradiochannelgroup in rawradiochannelgroups)
            {
                RadioChannelGroup newradiochannelgroup = new RadioChannelGroup();
                newradiochannelgroup.Id = myradiochannelgroup.IdGroup;
                newradiochannelgroup.GroupName = myradiochannelgroup.GroupName;
                //add whatever you need here

                Log.Debug("myradiogroupname = " + myradiochannelgroup.GroupName);
                allradiochannelgroups.Add(newradiochannelgroup);
            }
            return allradiochannelgroups;

        }

        #endregion public methods
    }

}
