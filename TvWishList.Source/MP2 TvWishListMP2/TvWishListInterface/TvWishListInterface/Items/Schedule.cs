#region Copyright (C) 2007-2012 Team MediaPortal
/*
    Copyright (C) 2007-2012 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MediaPortal.Plugins.TvWishList.Items
{
    public interface ISchedule
    {
        #region Schedule Member

        int Id { get; set; }
        string ProgramName { get; set; }
        int IdChannel { get; set; }
        DateTime StartTime { get; set; }
        DateTime EndTime { get; set; }
        Int32 ScheduleType { get; set; }

        int PreRecordInterval { get; set; }
        int PostRecordInterval { get; set; }
        int MaxAirings { get; set; }
        DateTime KeepDate { get; set; }
        Int32 KeepMethod { get; set; }
        int Priority { get; set; }
        int RecommendedCard { get; set; }
        bool Series { get; set; }
                                       
        #endregion Schedule Member

        //ILogger Log = ServiceRegistration.Get<ILogger>();

        #region public methods
        //List<ISchedule> ListAll();
        

        //void Persist();


        //void Delete();

        //ISchedule GetNew();

        #endregion public methods
    }
}
