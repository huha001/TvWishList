﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MediaPortal.Plugins.TvWishListMP2.MPExtended;


namespace MediaPortal.Plugins.TvWishList.Items
{
    public class Recording : IRecording
    {
        #region Schedule Member

        public int IdRecording { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public int IdChannel { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        #endregion Schedule Member
    }
}
