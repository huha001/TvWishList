#region Copyright (C) 
/* 
 *	Copyright (C) 2006-2012 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion



/* Version History Install.exe
 * * 1.0.4. 4 
 *          install and uninstall flag implemented
 *          stopp tv service errors will not abort the plugin installation
 * 0.0.0.1 initial release
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using ShowLicense;
using MediaPortal.Plugins;
using TvLibrary.Log;

namespace TvWishListInstall
{
    static class Install
    {
        /// <summary>
        /// This program does install the plugin TvWishList for  Tv Server
        /// 
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            


            //check for single instance
            Process current = Process.GetCurrentProcess();
            Process[] allnamedprocesses = Process.GetProcessesByName(current.ProcessName);
            int k = allnamedprocesses.Length;
            if (k > 1)
            {
                //MessageBox.Show("More than one single instance running ("+k.ToString()+") - aborting","Error");
                return;
            }

            //setup
            bool install = false;
            bool uninstall = false;

            if ((args.Length == 1) && (args[0].ToLower() == "install"))
            {
                install = true;
            }
            else if ((args.Length == 1) && (args[0].ToLower() == "uninstall"))
            {
                uninstall = true;
            }

            InstallPaths instpaths = new InstallPaths();  //define new instance for folder detection
            instpaths.GetInstallPaths();
            string licensefile = "license.txt";
            if (File.Exists(licensefile) == false)
            {
                
                //Log.Debug("Current folder is " + installdirectory + "\n");  causes exception
                licensefile = System.Environment.CurrentDirectory + @"\license.txt";// 1st try in current directory
                if (File.Exists(licensefile) == false)
                {
                    //2nd try mpi installer
                    licensefile = instpaths.MP_USER_FOLDER + @"\Installer\TvWishList\license.txt";
                                      
                    if (File.Exists(licensefile) == false)
                    {
                        //3rd try in %Installer%
                        licensefile = System.Environment.CurrentDirectory + @"\%Installer%\TvWishList\license.txt";                 
                        if (File.Exists(licensefile) == false)
                        {
                            MessageBox.Show("License file " + licensefile + " not found - aborting installation", "Installation Error");
                            return;
                        }
                    }
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new License(licensefile));
            Application.Run(new InstallSetup(install, uninstall));
        }
    }
}
