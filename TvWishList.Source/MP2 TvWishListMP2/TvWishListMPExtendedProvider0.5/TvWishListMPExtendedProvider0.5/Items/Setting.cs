using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MediaPortal.Plugins.TvWishListMP2.MPExtended;


namespace MediaPortal.Plugins.TvWishList.Items
{
    public class Setting : ISetting
    {
        #region Setting Member

        public string Tag { get; set; }

        public string Value { get; set; }

        #endregion
    }


}
