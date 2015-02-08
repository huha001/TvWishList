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
//using System.Linq;
//using System.Text;
//using Mediaportal.TV.Server.Plugins.Base.Interfaces;
//using Mediaportal.TV.Server.SetupControls;
//using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
//using Mediaportal.TV.Server.TVControl.Events;
//using Mediaportal.TV.Server.TVControl.Interfaces.Events;
//using Mediaportal.TV.Server.TVControl.Interfaces.Services;
//using Mediaportal.TV.Server;
using Log = TvLibrary.Log.huha.Log;

//using Mediaportal.TV.Server;
//using Mediaportal.TV.Server.TVDatabase;
//using Mediaportal.TV.Server.TVDatabase.Entities;
//using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
//using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
//using Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext;
//using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
//using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
//using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities.Cache;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using MediaPortal.Plugins.TvWishList.Setup;

namespace MediaPortal.Plugins.TvWishList.Items
{
    [CLSCompliant(false)]
    public class Channel  //:Channel
    {
        #region Channel Member        
        public int IdChannel { get; private set; }        
        public string DisplayName { get; set; }       
        #endregion Channel Member

        #region public methods
        public static IList<Channel> ListAll() 
        {
            
            //in tv server setup
            //Log.Debug("listing all channels");

            IList<Mediaportal.TV.Server.TVDatabase.Entities.Channel> rawchannels;
            if (TvWishListSetup.Setup)
                rawchannels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannelsByMediaType(Mediaportal.TV.Server.TVDatabase.Entities.Enums.MediaTypeEnum.TV);
            else
                rawchannels = ChannelManagement.ListAllChannelsByMediaType(Mediaportal.TV.Server.TVDatabase.Entities.Enums.MediaTypeEnum.TV);

            //Log.Debug(rawchannels.Count.ToString() + " channels found");          
            IList<Channel> allchannels = new List<Channel>();
            foreach (Mediaportal.TV.Server.TVDatabase.Entities.Channel mychannel in rawchannels)
            {
                Channel newchannel = new Channel();
                newchannel.IdChannel = mychannel.IdChannel;
                newchannel.DisplayName = mychannel.DisplayName;
                //add whatever you need here
                allchannels.Add(newchannel);
            }
            //Log.Debug(allchannels.Count.ToString() + " converted channels found");
            return allchannels;

        }

        public static IList<Channel> ListAllByGroup(int groupId)
        {


            IList<Mediaportal.TV.Server.TVDatabase.Entities.Channel> rawchannels;
            if (TvWishListSetup.Setup)
                rawchannels = ServiceAgents.Instance.ChannelServiceAgent.GetAllChannelsByGroupIdAndMediaType(groupId, Mediaportal.TV.Server.TVDatabase.Entities.Enums.MediaTypeEnum.TV);
            else
                rawchannels = ChannelManagement.GetAllChannelsByGroupIdAndMediaType(groupId, Mediaportal.TV.Server.TVDatabase.Entities.Enums.MediaTypeEnum.TV);

            IList<Channel> allchannels = new List<Channel>();
            foreach (Mediaportal.TV.Server.TVDatabase.Entities.Channel mychannel in rawchannels)
            {
                Channel newchannel = new Channel();
                newchannel.IdChannel = mychannel.IdChannel;
                newchannel.DisplayName = mychannel.DisplayName;
                //add whatever you need here
                allchannels.Add(newchannel);
            }
            return allchannels;

        }

        public static Channel Retrieve(int idChannel)
        {
            Mediaportal.TV.Server.TVDatabase.Entities.Channel mychannel;
            if (TvWishListSetup.Setup)
                mychannel = ServiceAgents.Instance.ChannelServiceAgent.GetChannel(idChannel);
            else
                mychannel = ChannelManagement.GetChannel(idChannel);

            Channel newchannel = new Channel();
            newchannel.IdChannel = mychannel.IdChannel;
            newchannel.DisplayName = mychannel.DisplayName;
            //add whatever you need here
            return newchannel;
        }
        #endregion public methods
    }
}
