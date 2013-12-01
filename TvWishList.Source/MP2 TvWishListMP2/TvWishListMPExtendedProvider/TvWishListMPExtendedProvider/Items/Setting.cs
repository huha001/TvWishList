using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Plugins.TvWishList.Items;


namespace MediaPortal.Plugins.TvWishListMPExtendedProvider.Items
{
    public class Setting : ISetting
    {
        #region Setting Member

        public string Tag { get; set; }

        public string Value { get; set; }

        #endregion
    }


}
