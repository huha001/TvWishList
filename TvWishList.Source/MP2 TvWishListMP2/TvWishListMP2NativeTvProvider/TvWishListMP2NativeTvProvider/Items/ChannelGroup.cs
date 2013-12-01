using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Plugins.TvWishList.Items;

namespace MediaPortal.Plugins.TvWishListMP2NativeTvProvider.Items
{
    public class ChannelGroup : IChannelGroup
    {
        #region ChannelGroup Member

        public int Id { get; set; }

        public string GroupName { get; set; }

        #endregion ChannelGroup Member



    }
}
