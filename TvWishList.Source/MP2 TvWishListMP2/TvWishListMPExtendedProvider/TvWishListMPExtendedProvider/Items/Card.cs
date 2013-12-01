using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Plugins.TvWishList.Items;


namespace MediaPortal.Plugins.TvWishListMPExtendedProvider.Items
{
    public class Card : ICard
    {
        #region Card Member

        public int Id { get; set; }

        public string Name { get; set; }

        #endregion Card Member
    }
}
