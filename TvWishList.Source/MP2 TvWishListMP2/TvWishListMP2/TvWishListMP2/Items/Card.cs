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

namespace MediaPortal.Plugins.TvWishList.Items

{
    public class Card : ICard
    {
        #region Card Member

        public int Id { get; set; }

        public string Name { get; set; }

        #endregion Card Member

        //ILogger Log = ServiceRegistration.Get<ILogger>();

        #region public methods

        

        public static IList<Card> ListAll()
        {
            IList<ICard> allIcards = null;

            if ( Main_GUI.MyTvProvider == null)
            {
                Main_GUI.MyTvProvider = ServiceRegistration.Get<ITvWishListTVProvider>();
            }

            if (Main_GUI.MyTvProvider.Initialized == false)
            {
                Main_GUI.MyTvProvider.Init();
            }

            Main_GUI.MyTvProvider.ReadAllCards(out allIcards);

            IList<Card> allcards = new List<Card>();
            foreach (ICard mycard in allIcards)
            {
                //Log.Debug("mygroupname = " + myIchannelgroup.GroupName);
                allcards.Add(new Card { Id = mycard.Id, Name = mycard.Name });
            }
            return allcards;

            
        }

        #endregion public methods
    }
}
