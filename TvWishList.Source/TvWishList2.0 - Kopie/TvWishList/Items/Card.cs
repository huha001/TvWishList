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

namespace MediaPortal.Plugins.TvWishList.Items
{
  
    public class Card 
    {
        //
        //private int _idcard = -1;
        #region Card Member

        public int IdCard { get; private set; }

        public string Name { get; set; }
        //add whatever you need here

        #endregion Card Member

        //ILogger Log = ServiceRegistration.Get<ILogger>();

        #region public methods

        

        public static IList<Card> ListAll()
        {            
            IList<Mediaportal.TV.Server.TVDatabase.Entities.Card> rawcards = ServiceAgents.Instance.CardServiceAgent.ListAllCards();

            IList<Card> allcards = new List<Card>();
            foreach (Mediaportal.TV.Server.TVDatabase.Entities.Card mycard in rawcards)
            {
                Card newcard = new Card();
                newcard.IdCard = mycard.IdCard;
                newcard.Name = mycard.Name;
                //add whatever you need here


                //Log.Debug("mygroupname = " + myIchannelgroup.GroupName);
                allcards.Add(newcard);
            }
            return allcards;            
        }

        /*
        public bool canViewTvChannel(int IdChannel)
        {
            ServiceAgents.Instance.CardServiceAgent.
            return true;
        }*/

        #endregion public methods
    }
}
