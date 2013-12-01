using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Plugins.TvWishList.Items;

namespace MediaPortal.Plugins.TvWishListMP2NativeTvProvider.Items
{
    public class ServerName : IServerName
    {
        #region ServerName

        public string Name { get; set; }
        public int ServerIndex { get; set; }

        #endregion ServerName
    }
}
