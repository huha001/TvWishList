﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MediaPortal.Plugins.TvWishListMP2.MPExtended;


namespace MediaPortal.Plugins.TvWishList.Items
{
    public class RadioChannel : IRadioChannel
    {
        #region RadioChannel Member

        public int Id { get; set; }

        public string Name { get; set; }

        #endregion RadioChannel Member
    }
}
