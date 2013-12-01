using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Plugins.TvWishList.Items;

namespace MediaPortal.Plugins.TvWishListMP2NativeTvProvider.Items
{
    public class Schedule : ISchedule
    {
        #region Schedule Member

        public int Id { get; set; }
        public string ProgramName { get; set; }
        public int IdChannel { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int ScheduleType { get; set; }

        public int PreRecordInterval { get; set; }
        public int PostRecordInterval { get; set; }
        public int MaxAirings { get; set; }
        public DateTime KeepDate { get; set; }
        public int KeepMethod { get; set; }
        public int Priority { get; set; }
        public int RecommendedCard { get; set; }
        public bool Series { get; set; }

        #endregion Schedule Member
    }
}
