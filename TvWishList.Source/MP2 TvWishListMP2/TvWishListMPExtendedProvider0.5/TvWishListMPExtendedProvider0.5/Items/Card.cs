using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MediaPortal.Plugins.TvWishListMP2.MPExtended;


namespace MediaPortal.Plugins.TvWishList.Items
{
    public class Card : ICard
    {
        #region Card Member

        public int Id { get; set; }

        public string Name { get; set; }

        #endregion Card Member
    }
}
