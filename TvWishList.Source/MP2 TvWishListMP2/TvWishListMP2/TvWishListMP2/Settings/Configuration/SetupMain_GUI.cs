#region Copyright (C) 2007-2011 Team MediaPortal

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


using MediaPortal.Common.Configuration.ConfigurationClasses;
using MediaPortal.Plugins.TvWishListMP2.Settings;
using MediaPortal.Plugins.TvWishListMP2.Models;
using MediaPortal.Common;
using MediaPortal.Common.General;
using MediaPortal.Common.Logging;
using System.Diagnostics;
using System.IO;

using MediaPortal.Plugins;



namespace MediaPortal.Plugins.TvWishListMP2.Settings.Configuration
{
    

    public class CustomConfigMain_GUI : CustomConfigSetting
    {
        /*
        ILogger Log = ServiceRegistration.Get<ILogger>();

        public void HelpButton()
        {
            Log.Debug("ConfigMain_GUI: Help() started");
            //Help
            Process proc = new Process();
            ProcessStartInfo procstartinfo = new ProcessStartInfo();
            procstartinfo.FileName = "TvWishList.pdf";
            InstallPaths instpaths = new InstallPaths();
            instpaths.GetInstallPathsMP2();
            instpaths.GetMediaPortalDirsMP2();
            procstartinfo.WorkingDirectory = instpaths.DIR_MP2_Plugins + @"\TvWishListMP2";
            proc.StartInfo = procstartinfo;
            try
            {
                proc.Start();
            }
            catch
            {
                Log.Error("Could not open " + procstartinfo.WorkingDirectory + "\\" + procstartinfo.FileName, "Error");
            }
        }*/
    }
}
