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
using Mediaportal.TV.Server;
using Log = TvLibrary.Log.huha.Log;

namespace Mediaportal.TV.Server.MP1Translation
{
    public class GroupMap
    {

        public GroupMap(int idGroup, int idChannel, int sortOrder)
        {
            this.IdGroup = idGroup;
            this.IdChannel = idChannel;
            this.SortOrder = sortOrder;
        }
        public GroupMap(int idMap, int idGroup, int idChannel, int sortOrder)
        {
            this.IdMap = idMap;
            this.IdGroup = idGroup;
            this.IdChannel = idChannel;
            this.SortOrder = sortOrder;
        }

        #region GroupMap Member
        public int IdChannel { get; set; }
        public int IdGroup { get; set; }
        public int IdMap { get; set; }
        public bool IsChanged { get; set; }
        public int SortOrder { get; set; }

        //add whatever you need here

        #endregion Card Member

        //ILogger Log = ServiceRegistration.Get<ILogger>();

        #region public methods

        

        

        #endregion public methods
    }
}
