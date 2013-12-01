using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Plugins.TvWishList.Items;

namespace MediaPortal.Plugins.TvWishListMP2NativeTvProvider.Items
{
    public class Channel : IChannel
    {
        #region Channel Member

        public int Id { get; set; }

        public string Name { get; set; }

        #endregion Channel Member
    }
}
