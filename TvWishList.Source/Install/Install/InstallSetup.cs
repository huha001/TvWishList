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


using System;
using System.Collections;
using System.Collections.Generic; 
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;

using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.Plugins;


namespace TvWishListInstall
{
    public partial class InstallSetup : Form
    {
        bool DEBUG=false;

        //string[] allskins = { @"\Default", @"\DefaultWide", @"\Blue3", @"\Blue3Wide", @"\PureVisionHD", @"\PureVisionHD 1080", @"\PureVisionHDXmas", @"\StreamedMP", @"\LCARS" }; //supported skins
        //string[] allskinmods = { @"\Default", @"\DefaultWide", @"\PureVisionHD", @"\PureVisionHD 1080", @"\PureVisionHDXmas", @"\StreamedMP", @"\LCARS" }; //supported skin mofications

        string[] allskins = { @"\Default", @"\DefaultWide", @"\Blue3", @"\Blue3Wide" }; //supported skins
        string[] allskinmods = { @"\Default", @"\DefaultWide" }; //supported skin mofications

        //main installation folders
        public string MP_PROGRAM_FOLDER = "NOT_DEFINED";
        public string MP_USER_FOLDER = "NOT_DEFINED";
        public string TV_PROGRAM_FOLDER = "NOT_DEFINED";
        public string TV_USER_FOLDER = "NOT_DEFINED";
        public string CENTRAL_DATABASE = "NOT_DEFINED";

        public string MP2_PROGRAM_FOLDER = "NOT_DEFINED";
        public string MP2_USER_FOLDER = "NOT_DEFINED";
        public string SV2_PROGRAM_FOLDER = "NOT_DEFINED";
        public string SV2_USER_FOLDER = "NOT_DEFINED";

        //other
        public bool UNINSTALL=false;
        public bool INSTALL = false;
        System.Timers.Timer m_timer = null;        
        InstallPaths instpaths = new InstallPaths();  //define new instance for folder detection
        
        string installdirectory = System.Environment.CurrentDirectory;

        enum CompareFileVersion
        {
            Newer = 1,
            Older = -1,
            Equal = 0,
            Error = 89,
            Version1Error = 90,
            Version1String = 91,
            Version2Error = 92,
            Version2String = 93
        };

        public InstallSetup(bool installflag, bool uninstallflag)
        {
            InitializeComponent();
            checkBoxInstallSkins.Checked = true;
            checkBoxSkinMods.Checked = true;

            AutoDetect();

            UpdatePathVariables();
            
            if ((Directory.Exists(TV_PROGRAM_FOLDER) == true) && (Directory.Exists(TV_USER_FOLDER) == true) && (TV_PROGRAM_FOLDER != "") && (TV_USER_FOLDER != ""))
            { //found TVserver 
                textoutputdebug("TV Server installation detected - TV Server plugin can be installed");
                checkBoxTV.Checked = true;
                
            }
            if ((Directory.Exists(MP_PROGRAM_FOLDER) == true) && (Directory.Exists(MP_USER_FOLDER) == true) && (MP_PROGRAM_FOLDER != "") && (MP_USER_FOLDER != ""))
            {//found Media Portal 
                textoutputdebug("Media Portal installation detected - Media Portal Plugin can be installed");
                checkBoxMP.Checked = true;
            }
            if ((Directory.Exists(MP2_PROGRAM_FOLDER) == true) && (Directory.Exists(MP2_USER_FOLDER) == true) && (MP2_PROGRAM_FOLDER != "") && (MP2_USER_FOLDER != ""))
            {//found Media Portal 
                textoutputdebug("Media Portal2 installation detected - Media Portal2 Plugin can be installed");
                checkBoxMP2C.Checked = true;
            }


            if (((Directory.Exists(MP_PROGRAM_FOLDER) == true) && (Directory.Exists(MP_USER_FOLDER) == true) && (MP_PROGRAM_FOLDER != "") && (MP_USER_FOLDER != "")) || ((Directory.Exists(TV_PROGRAM_FOLDER) == true) && (Directory.Exists(TV_USER_FOLDER) == true)&&(TV_PROGRAM_FOLDER != "")&&(TV_USER_FOLDER != "")))
            {//no error
                textoutputdebug("Check the folder paths and click the Install button");
            }
            else // no installation detected
            {
                textoutputdebug("No installation has been detected - please select installation paths manually");               
            }


            UpdateGUI();
             

            if (File.Exists(installdirectory + @"\Install.exe") == false)  //1st try current directory
            {//select install directory manually if current directory is not set to the extracted TvWishList folder
                if (File.Exists(MP_USER_FOLDER + @"\Installer\TvWishList\Install.exe") == true) //2nd try mpi installer directory                               
                {
                    installdirectory = MP_USER_FOLDER + @"\Installer\TvWishList";
                }
                else
                {
                    if (File.Exists(System.Environment.CurrentDirectory + @"\%Installer%\TvWishList\Install.exe") == true) //3rd try %installer% directory
                    {
                        installdirectory = System.Environment.CurrentDirectory + @"\%Installer%\TvWishList";
                    }

                    else
                    {
                        FolderBrowserDialog folderDialog = new FolderBrowserDialog();
                        folderDialog.Description = "Select Extracted TvWishList Release Folder (Contains Install.exe)";
                        folderDialog.ShowNewFolderButton = false;
                        if (folderDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (File.Exists(folderDialog.SelectedPath + @"\Install.exe") == true)
                            {
                                installdirectory = folderDialog.SelectedPath;

                            }
                            else
                            {
                                textoutputdebug("Install.exe does not exist in the selected folder \naborting installation\n" + folderDialog.SelectedPath + "\n");
                                return;
                            }
                        }
                        else
                        {
                            textoutputdebug("User selected invalid TvWishList folder or canceled \n aborting installation" + "\n");
                            return;
                        }
                    }
                }

            }

            textoutputdebug("Current installer directory is " + System.Environment.CurrentDirectory);


            if (installflag == true)
            {
                INSTALL = true;
            }
            else if (uninstallflag == true)
            {
                UNINSTALL=true;               
            }
            m_timer = new System.Timers.Timer(10); //close after 0.1s
            m_timer.Enabled = true;
            m_timer.Elapsed += new System.Timers.ElapsedEventHandler(autoinstallation);
        }

        public void AutoDetect()
        {
            //autodetect paths
            instpaths.DEBUG = DEBUG;

            instpaths.LOG = "";
            instpaths.GetInstallPaths();
            if (DEBUG)
                textoutputdebug(instpaths.LOG);

            textBoxMP1P.Text = instpaths.MP_PROGRAM_FOLDER;
            textBoxMP1U.Text = instpaths.MP_USER_FOLDER;
            textBoxTV1P.Text = instpaths.TV_PROGRAM_FOLDER;
            textBoxTV1U.Text = instpaths.TV_USER_FOLDER;

            instpaths.LOG = "";
            instpaths.GetInstallPathsMP2();
            if (DEBUG)
                textoutputdebug(instpaths.DIR_MP2_LOG);

            textBoxMP2P.Text = instpaths.MP2_PROGRAM_FOLDER;
            textBoxMP2U.Text = instpaths.MP2_USER_FOLDER;
            

        }

        public void UpdatePathVariables()
        {
            //kill emty strings for paths
            if (textBoxTV1P.Text == string.Empty)
                textBoxTV1P.Text = "NOT_DEFINED";

            if (textBoxTV1U.Text == string.Empty)
                textBoxTV1U.Text = "NOT_DEFINED";

            if (textBoxMP1P.Text == string.Empty)
                textBoxMP1P.Text = "NOT_DEFINED";

            if (textBoxMP1U.Text == string.Empty)
                textBoxMP1U.Text = "NOT_DEFINED";

            

            if (textBoxMP2P.Text == string.Empty)
                textBoxMP2P.Text = "NOT_DEFINED";

            if (textBoxMP2U.Text == string.Empty)
                textBoxMP2U.Text = "NOT_DEFINED";

            //update pasth variables
            MP_PROGRAM_FOLDER = textBoxMP1P.Text;
            MP_USER_FOLDER = textBoxMP1U.Text;
            TV_PROGRAM_FOLDER = textBoxTV1P.Text;
            TV_USER_FOLDER = textBoxTV1U.Text;

            MP2_PROGRAM_FOLDER = textBoxMP2P.Text;
            MP2_USER_FOLDER = textBoxMP2U.Text; ;
           


            //update instpaths variables a swell if not called for autodetect
            instpaths.MP_PROGRAM_FOLDER = textBoxMP1P.Text;
            instpaths.MP_USER_FOLDER = textBoxMP1U.Text;
            instpaths.TV_PROGRAM_FOLDER = textBoxTV1P.Text;
            instpaths.TV_USER_FOLDER = textBoxTV1U.Text;

            instpaths.MP2_PROGRAM_FOLDER = textBoxMP2P.Text;
            instpaths.MP2_USER_FOLDER = textBoxMP2U.Text; ;
            


            //get all install directories
            instpaths.LOG = "";
            instpaths.GetMediaPortalDirs();
            if (DEBUG)
                textoutputdebug(instpaths.LOG);

            instpaths.LOG = "";
            instpaths.GetMediaPortalDirsMP2();

            if (DEBUG)
                textoutputdebug(instpaths.LOG);


        }

        private void UpdateGUI()
        {
            if ((checkBoxTV.Checked == true) && ((TV_PROGRAM_FOLDER == "NOT_DEFINED") || (TV_USER_FOLDER == "NOT_DEFINED")))
            {
                MessageBox.Show("Unchecking Tv Server because paths are not correctly defined\nCheck the path configuration first", "Error");
                checkBoxTV.Checked = false;
            }

            if ((checkBoxMP.Checked == true) && ((MP_PROGRAM_FOLDER == "NOT_DEFINED") || (MP_USER_FOLDER == "NOT_DEFINED")))
            {
                MessageBox.Show("Unchecking MediaPortal1 because paths are not correctly defined\nCheck the path configuration first", "Error");
                checkBoxMP.Checked = false;
            }

            if ((checkBoxMP2C.Checked == true) && ((MP2_PROGRAM_FOLDER == "NOT_DEFINED") || (MP2_USER_FOLDER == "NOT_DEFINED")))
            {
                MessageBox.Show("Unchecking MediaPortal2 Client because paths are not correctly defined\nCheck the path configuration first", "Error");
                checkBoxMP2C.Checked = false;
            }

            

            if ((textBoxMP1P.Text != "NOT_DEFINED") && (textBoxMP1U.Text != "NOT_DEFINED"))
            {
                checkBoxMP.Show();
                checkBoxInstallSkins.Show();
                checkBoxSkinMods.Show();
            }
            else
            {
                checkBoxMP.Hide();
                checkBoxInstallSkins.Hide();
                checkBoxSkinMods.Hide();
            }

            if ((textBoxTV1P.Text != "NOT_DEFINED") && (textBoxTV1U.Text != "NOT_DEFINED"))
                checkBoxTV.Show();
            else
                checkBoxTV.Hide();

            if ((textBoxMP2P.Text != "NOT_DEFINED") && (textBoxMP2U.Text != "NOT_DEFINED"))
                checkBoxMP2C.Show();
            else
                checkBoxMP2C.Hide();

            
            if (!checkBoxTV.Checked && !checkBoxMP.Checked && !checkBoxMP2C.Checked)
                textoutputdebug("No Plugin Selection was found - Check the settings and/or installation paths");

        }

        public void autoinstallation(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (m_timer!=null)
                m_timer.Elapsed -= new System.Timers.ElapsedEventHandler(autoinstallation);

            if (INSTALL == true)
            {
                install();
                System.Threading.Thread.Sleep(2000);
                Application.Exit();
            }
            else if (UNINSTALL == true)
            {
                uninstall();
                System.Threading.Thread.Sleep(2000);
                Application.Exit();
            }
            
        }





        private void buttoninstall_Click(object sender, EventArgs e)
        {
            install();
        }

        private void install()
        {

            UpdatePathVariables();
            UpdateGUI();
           
            if (File.Exists(installdirectory + @"\Install.exe") == false)
            {
                textoutputdebug("Cannot find install.exe in install directory " + installdirectory + " - aborting\n");
                return;
            }
            else
            {
                System.Environment.CurrentDirectory = installdirectory;
            }            
            
            textoutputdebug("Current folder is " + installdirectory + "\n");

            if ((checkBoxTV.Checked) && (Directory.Exists(TV_PROGRAM_FOLDER) == true) && (Directory.Exists(TV_USER_FOLDER) == true) && (TV_PROGRAM_FOLDER != "") && (TV_USER_FOLDER != ""))
            { //install TVserver Plugin
                switch (MessageBox.Show("Do you want to install the TvWishList TV Server Plugin?\nThe Tv server will be stopped, so make sure you are not recording!\nYes is recommended ", "Tv Server TvWishList Plugin Installation",MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
                {
                    case DialogResult.Yes:
                        {
                            // "Yes" processing
                            textoutputdebug("\nInstalling TV Server Plugin TvWishList in ");
                            textoutputdebug(TV_PROGRAM_FOLDER + @"\Plugins"+"\n");
                            

                            //check for setupTv.exe
                            Process[] sprocs = Process.GetProcessesByName("SetupTv");
                            foreach (Process sproc in sprocs) // loop is only executed if Media portal is running
                            {
                                textoutputdebug("You need to close Tv Server Configuration before you install the plugin\n");
                                MessageBox.Show("You need to close Tv Server Configuration before you install the plugin", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }


                            Process[] tprocs = null;
                            tprocs= Process.GetProcessesByName("TVService");
                            bool TvserviceRunning=false;
                            foreach (Process tproc in tprocs)
                            {
                                TvserviceRunning = true;
                            }


                            //Stopping tv service
                            if (TvserviceRunning == true)
                            {
                                StopTvService();
                            }
                            else
                            {
                                textoutputdebug("TV Service is not running" + "\n");
                            }

                            TvServerInstall();
                            
                            //Starting tv service
                            if (TvserviceRunning == true)
                            {
                                StartTvService();
                            }
                           
                            break;
                        }
                    case DialogResult.No:
                        {
                            // "No" processing
                            textoutputdebug("Installation aborted by user\n");
                            break;

                        }
                }



            }


            if ((checkBoxMP.Checked) && (Directory.Exists(MP_PROGRAM_FOLDER) == true) && (Directory.Exists(MP_USER_FOLDER) == true) && (MP_PROGRAM_FOLDER != "") && (MP_USER_FOLDER != ""))
            {//install Media Portal Plugin
                switch (MessageBox.Show("Do you want to install the TvWishListMP Media Portal Plugin?\nYes is recommended ", "MediaPortal TvWishList Plugin Installation", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
                {
                    case DialogResult.Yes:
                        {
                            //check for MediaPortal.exe
                            Process[] sprocs = Process.GetProcessesByName("MediaPortal");
                            foreach (Process sproc in sprocs) // loop is only executed if Media portal is running
                            {
                                textoutputdebug("You need to close MediaPortal before you install the plugin\n");
                                MessageBox.Show("You need to close MediaPortal before you install the plugin", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            //check for Configuration.exe
                            sprocs = Process.GetProcessesByName("Configuration");
                            foreach (Process sproc in sprocs) // loop is only executed if Media portal is running
                            {
                                textoutputdebug("You need to close MediaPortal Configuration before you install the plugin\n");
                                MessageBox.Show("You need to close MediaPortal Configuration before you install the plugin", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            

                            // "Yes" processing
                            MP1Install(); 
                            
                            break;
                        }
                    case DialogResult.No:
                        {
                            // "No" processing
                            textoutputdebug("Installation aborted by user\n");
                            break;

                        }
                }
            }

            if ((checkBoxMP2C.Checked) && (Directory.Exists(MP2_PROGRAM_FOLDER) == true) && (Directory.Exists(MP2_USER_FOLDER) == true) && (MP2_PROGRAM_FOLDER != "") && (MP2_USER_FOLDER != ""))
            {//install Media Portal Plugin
                switch (MessageBox.Show("Do you want to install the TvWishListMP2 Media Portal2 Client Plugin?\nYes is recommended ", "MediaPortal2 Client TvWishList Plugin Installation", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
                {
                    case DialogResult.Yes:
                        {
                            //check for MP2-Client.exe
                            Process[] sprocs = Process.GetProcessesByName("MP2-Client");
                            foreach (Process sproc in sprocs) // loop is only executed if Media portal is running
                            {
                                textoutputdebug("You need to close the MediaPortal2 Client before you install the plugin\n");
                                MessageBox.Show("You need to close the MediaPortal2 Client you install the plugin", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            // "Yes" processing
                            MP2ClientInstall();
                            break;
                        }
                    case DialogResult.No:
                        {
                            // "No" processing
                            textoutputdebug("Installation aborted by user\n");
                            break;
                        }
                }
            }//end MP2 client installation

            if (((Directory.Exists(MP_PROGRAM_FOLDER) == true) && (Directory.Exists(MP_USER_FOLDER) == true) && (MP_PROGRAM_FOLDER != "") && (MP_USER_FOLDER != "")) || ((Directory.Exists(TV_PROGRAM_FOLDER) == true) && (Directory.Exists(TV_USER_FOLDER) == true)&&(TV_PROGRAM_FOLDER != "")&&(TV_USER_FOLDER != "")) || ((Directory.Exists(MP2_PROGRAM_FOLDER) == true) && (Directory.Exists(MP2_USER_FOLDER) == true) && (MP2_PROGRAM_FOLDER != "") && (MP2_USER_FOLDER != "")))
            {//no error
            }
            else
            {
                MessageBox.Show("No Tv Server Installation and no Media Portal installation was found\nCannot install any TvWishList plugin\nCheck the folder paths\n","Error");
            }


            textoutputdebug("Plugin installation finished\n");
        }

        private void TvServerInstall()
        {
            bool success = false;

            try  //copy TvWishList.dll
            {

                // first delete dll
                if (File.Exists(TV_PROGRAM_FOLDER + @"\Plugins\TvWishList.dll") == true)
                    File.Delete(TV_PROGRAM_FOLDER + @"\Plugins\TvWishList.dll");

                //fileversion tvservice used for recognizing different versions 
                FileVersionInfo tvserviceFileVersionInfo = FileVersionInfo.GetVersionInfo(TV_PROGRAM_FOLDER + @"\TvService.exe");
                if (FileVersionComparison(tvserviceFileVersionInfo.FileVersion.ToString(), "1.0.1.0") > (int)CompareFileVersion.Error)
                {
                    textoutputdebug("Error: TvService.exe File version could not be detected or is incorrect (Value=" + tvserviceFileVersionInfo.FileVersion.ToString() + ")");
                    textoutputdebug("Errorcode=" + FileVersionComparison(tvserviceFileVersionInfo.FileVersion.ToString(), "1.0.1.0").ToString());
                    return;
                }

                if (FileVersionComparison(tvserviceFileVersionInfo.FileVersion.ToString(), "1.0.1.0") == (int)CompareFileVersion.Older)  //MP1.0final version if older than 1.0.1.0
                {
                    textoutputdebug("MediaPortal 1.0.0 final is no more supported - please update your mediaPortal version first");
                    return;

                }
                else if (FileVersionComparison(tvserviceFileVersionInfo.FileVersion.ToString(), "1.0.3.0") == (int)CompareFileVersion.Older)  //1.0.1 or 1.0.2 version if TVservice is older than 1.0.3.0
                {
                    textoutputdebug("Installing 1.0.1 Plugin version");
                    File.Copy(@"TvWishList1.0.1\TvWishList.dll", TV_PROGRAM_FOLDER + @"\Plugins\TvWishList.dll", true);
                    textoutputdebug(@"TvWishList1.0.1\TvWishList.dll  copied to " + "\n" + TV_PROGRAM_FOLDER + @"\Plugins\TvWishList.dll" + "\n");

                }
                else if (FileVersionComparison(tvserviceFileVersionInfo.FileVersion.ToString(), "1.0.6.0") == (int)CompareFileVersion.Older)  //1.1RC1 version if TVservice is older than 1.0.6.0
                {
                    textoutputdebug("MediaPortal 1.1RC1 is no more supported - please update your mediaPortal version first");
                    return;

                }
                else if (FileVersionComparison(tvserviceFileVersionInfo.FileVersion.ToString(), "1.1.6.0") == (int)CompareFileVersion.Older)//1.1 version if older than 1.1.6.0
                {
                    textoutputdebug("Installing 1.1 Plugin version");
                    File.Copy(@"TvWishList1.1\TvWishList.dll", TV_PROGRAM_FOLDER + @"\Plugins\TvWishList.dll", true);
                    textoutputdebug(@"TvWishList1.1\TvWishList.dll  copied to " + "\n" + TV_PROGRAM_FOLDER + @"\Plugins\TvWishList.dll" + "\n");
                }
                else //latest 1.2 if newer than 1.1.6.0
                {
                    textoutputdebug("Installing 1.2 Plugin version");
                    File.Copy(@"TvWishList1.2\TvWishList.dll", TV_PROGRAM_FOLDER + @"\Plugins\TvWishList.dll", true);
                    textoutputdebug(@"TvWishList1.2\TvWishList.dll  copied to " + "\n" + TV_PROGRAM_FOLDER + @"\Plugins\TvWishList.dll" + "\n");
                }
                success = true;

            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could not copy TvWishList.dll to\n" + TV_PROGRAM_FOLDER + @"\Plugins\TvWishList.dll");
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                textoutputdebug("Try to uninstall first and then reinstall");
                textoutputdebug("If it does not help reboot your computer");
                return;
            }



            try  //copy TvWishList.pdf
            {

                // first create directory
                if (Directory.Exists(TV_USER_FOLDER + @"\TvWishList") == false)
                    Directory.CreateDirectory(TV_USER_FOLDER + @"\TvWishList");

                File.Copy("TvWishList.pdf", TV_USER_FOLDER + @"\TvWishList\TvWishList.pdf", true);
                textoutputdebug("TvWishList.pdf  copied to \n" + TV_USER_FOLDER + @"\TvWishList\TvWishList.pdf" + "\n");

            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could not copy TvWishList.pdf to\n" + TV_USER_FOLDER + @"\TvWishList\TvWishList.pdf");
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                return;
            }

            try //language
            {
                string destination = TV_USER_FOLDER+@"\TvWishList\Languages\";
                //DirectoryCopy(source, destination,filepattern,overwrite,verbose,recursive)                                
                DirectoryCopy(@"language\TvWishListMP", destination, "*", true, false, true);
                textoutputdebug("language directory  copied to \n" + destination + "\n");
            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could not copy language directory\n");
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                return;
            }

            if (success == true)
            {
                textoutputdebug("Plugin installation succeeded");
                textoutputdebug("You can start now the TV Server Configuration and enable the plugin\n");
            }
        }

        private void MP1Install()
        {
            textoutputdebug("Installing MediaPortal Plugin TvWishListMP in ");
            textoutputdebug(MP_PROGRAM_FOLDER + @"\plugins\Windows" + "\n");
            try //TvWishListMP.dll
            {
                //fileversion MediaPortal.exe used for recognizing different versions 
                FileVersionInfo MediaPortalFileVersionInfo = FileVersionInfo.GetVersionInfo(MP_PROGRAM_FOLDER + @"\MediaPortal.exe");
                if (FileVersionComparison(MediaPortalFileVersionInfo.FileVersion.ToString(), "1.1.6.0") == (int)CompareFileVersion.Older)//MP1.1 if older than 1.1.6.0
                {
                    textoutputdebug("Installing MP1.1 Plugin version" + "\n");
                    File.Copy(@"TvWishListMP1.1\TvWishListMP.dll", MP_PROGRAM_FOLDER + @"\plugins\Windows\TvWishListMP.dll", true);
                    textoutputdebug(@"TvWishListMP1.1\TvWishListMP.dll  copied to" + "\n" + MP_PROGRAM_FOLDER + @"\plugins\Windows\TvWishListMP.dll" + "\n");
                }
                else //MP1.2 if newer than 1.1.6.0
                {
                    textoutputdebug("Installing MP1.2 Plugin version" + "\n");
                    File.Copy(@"TvWishListMP1.2\TvWishListMP.dll", MP_PROGRAM_FOLDER + @"\plugins\Windows\TvWishListMP.dll", true);
                    textoutputdebug(@"TvWishListMP1.2\TvWishListMP.dll  copied to" + "\n" + MP_PROGRAM_FOLDER + @"\plugins\Windows\TvWishListMP.dll" + "\n");
                }

            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could not copy TvWishListMP.dll to\n" + MP_PROGRAM_FOLDER + @"\plugins\Windows\TvWishListMP.dll");
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                return;
            }

            try //language
            {
                string destination = instpaths.DIR_Language;
                //DirectoryCopy(source, destination,filepattern,overwrite,verbose,recursive)                                
                DirectoryCopy("language", destination, "*", true, false, true);
                textoutputdebug("language directory  copied to \n" + destination + "\n");
            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could not copy language directory to\n");
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                return;
            }

            //skins
            if (checkBoxInstallSkins.Checked == true)
            {
                try //skin
                {
                    foreach (string skin_name in allskins)
                    {
                        string destination = instpaths.DIR_Skin + skin_name;
                        //DirectoryCopy(source, destination,filepattern,overwrite,verbose,recursive) 
                        if (Directory.Exists(destination) == true)
                        {
                            DirectoryCopy("skin" + skin_name, destination, "*", true, false, true);
                            textoutputdebug("skin directory " + skin_name + " copied to \n" + destination + "\n");
                        }
                    }
                }
                catch (Exception exc)
                {
                    textoutputdebug("Error: Could not copy skin directory\n");
                    textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                    return;
                }
            }

            //skin modifications
            if (checkBoxSkinMods.Checked == true)
            {
                try //skinmods
                {
                    foreach (string skin_name in allskinmods)
                    {
                        string destination = instpaths.DIR_Skin + skin_name;
                        //DirectoryCopy(source, destination,filepattern,overwrite,verbose,recursive) 
                        if ((Directory.Exists("skinmods" + skin_name) == true) && (Directory.Exists(destination) == true))
                        {
                            DirectoryCopyBackupBefore("skinmods" + skin_name, destination, "*", true, false, true);
                            textoutputdebug("skin modifications for " + skin_name + " copied to \n" + destination + "\n");
                        }
                    }
                }
                catch (Exception exc)
                {
                    textoutputdebug("Error: Could not copy skin modifications\n");
                    textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                    return;
                }
            }

            try  //copy TvWishList.pdf
            {
                if (Directory.Exists(MP_PROGRAM_FOLDER + @"\Docs") == false)
                    Directory.CreateDirectory(MP_PROGRAM_FOLDER + @"\Docs");
                File.Copy("TvWishList.pdf", MP_PROGRAM_FOLDER + @"\Docs\TvWishList.pdf", true);
                textoutputdebug("TvWishList.pdf  copied to \n" + MP_PROGRAM_FOLDER + @"\Docs\TvWishList.pdf" + "\n");
            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could not copy TvWishList.pdf to\n" + MP_PROGRAM_FOLDER + @"\Docs\TvWishList.pdf");
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                return;
            }

            textoutputdebug("MediaPortal Plugin installation succeeded");
            textoutputdebug("You can start now the Media Portal Configuration and enable the plugin in the section \"Windows Plugins\" " + "\n");
                            
        }

        private void MP2ClientInstall()
        {
            //install TvWishListMP2 Client plugin
            string TvWishListMP2Folder = instpaths.DIR_MP2_Plugins + @"\TvWishListMP2";
            textoutputdebug("Installing MediaPortal Plugin TvWishListMP2 in ");
            textoutputdebug(TvWishListMP2Folder + "\n");

            try
            {
                if (Directory.Exists(TvWishListMP2Folder))
                {//Delete old files
                    DirectoryDelete(TvWishListMP2Folder);
                }
            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could delete folder TvWishListMP2 \n");
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                return;
            }

            try //copy plugin folder
            {
                DirectoryCopy("TvWishListMP2", TvWishListMP2Folder, "*", true, true, true);
            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could not copy folder TvWishListMp2 to\n" + TvWishListMP2Folder);
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                return;
            }

            //install TvWishListInterfaces Client plugin
            string TvWishListProviderMP2Folder = instpaths.DIR_MP2_Plugins + @"\TvWishListMPExtendedProvider0.5";
            string TvWishListProviderMP2Name = "TvWishListMPExtendedProvider0.5";

            textoutputdebug("Installing MediaPortal Plugin " + TvWishListProviderMP2Name + " in ");
            textoutputdebug(TvWishListProviderMP2Folder + "\n");

            try
            {
                if (Directory.Exists(TvWishListProviderMP2Folder))
                {//Delete old files
                    DirectoryDelete(TvWishListProviderMP2Folder);
                }
            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could delete folder " + TvWishListProviderMP2Folder + " \n");
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                return;
            }

            try //copy plugin folder
            {
                DirectoryCopy(TvWishListProviderMP2Name, TvWishListProviderMP2Folder, "*", true, true, true);
            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could not copy folder " + TvWishListProviderMP2Name + " to\n" + TvWishListProviderMP2Folder);
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                return;
            }

            try  //copy TvWishList.pdf
            {

                File.Copy("TvWishList.pdf", TvWishListMP2Folder + @"\TvWishList.pdf", true);
                textoutputdebug("TvWishList.pdf  copied to \n" + TvWishListMP2Folder + @"\TvWishList.pdf" + "\n");
            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could not copy TvWishList.pdf to\n" + TvWishListMP2Folder + @"\TvWishList.pdf");
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                return;
            }

            textoutputdebug("MediaPortal2 Plugin installation succeeded");
            
        }


        /// <summary>
        /// Stop the TvService
        /// </summary>
        /// <returns></returns>
        public static bool StopTvService()
        {
            try
            {
                using (ServiceController sc = new ServiceController("TvService"))
                {
                    switch (sc.Status)
                    {
                        case ServiceControllerStatus.Running:
                            sc.Stop();
                            break;
                        case ServiceControllerStatus.StopPending:
                            break;
                        case ServiceControllerStatus.Stopped:
                            return true;
                        default:
                            return false;
                    }
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));
                    return sc.Status == ServiceControllerStatus.Stopped;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ServiceHelper: Stopping tvservice failed. Please check your installation. \nError: "+ ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Starts the TvService
        /// </summary>
        /// <returns></returns>
        public static bool StartTvService()
        {
            return Start("TvService");
        }

        public static bool Start(string aServiceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(aServiceName))
                {
                    switch (sc.Status)
                    {
                        case ServiceControllerStatus.Stopped:
                            sc.Start();
                            break;
                        case ServiceControllerStatus.StartPending:
                            break;
                        case ServiceControllerStatus.Running:
                            return true;
                        default:
                            return false;
                    }
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
                    return sc.Status == ServiceControllerStatus.Running;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ServiceHelper: Starting "+aServiceName.ToString()+" failed. Please check your installation. \nError: "+ex.ToString());
                return false;
            }
        }

        /* old implementation
        private void StopTvService()
        {
            textoutputdebug("Stopping TV Service");
            Process proc = new Process();
            ProcessStartInfo startinfo = new ProcessStartInfo();
            startinfo.FileName = "cmd.exe";
            startinfo.WorkingDirectory = "";
            startinfo.Arguments = @"/c net stop tvservice";
            startinfo.UseShellExecute = false;
            startinfo.CreateNoWindow = true;
            startinfo.RedirectStandardError = true;
            startinfo.RedirectStandardInput = true;
            startinfo.RedirectStandardOutput = true;
            proc.StartInfo = startinfo;
            proc.EnableRaisingEvents = false;

            try
            {
                proc.Start();
            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could not execute command \n" + startinfo.FileName + " " + startinfo.Arguments);
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                return;
            }
            proc.WaitForExit(1000 * 60); //wait 1 minutes maximum
            if (proc.HasExited == true)
            {
                if (proc.ExitCode != 0)
                {
                    textoutputdebug("Tv Service error: Stopping Tv service caused an error code " + proc.ExitCode);

                    //textoutputdebug("Reboot and repeat installation\n");
                    //return;
                }
                else
                {
                    textoutputdebug("TV Service Stopped" + "\n");
                }
            }
            else
            {
                textoutputdebug("Tv Service timeout error: Could not stop Tv service within time limit");
                //textoutputdebug("Reboot and repeat installation\n");
                //return;
            }
        }

        private void StartTvService()
        {
            textoutputdebug("Starting TV Service");
            Process proc2 = new Process();
            ProcessStartInfo startinfo2 = new ProcessStartInfo();
            startinfo2.FileName = "cmd.exe";
            startinfo2.WorkingDirectory = "";
            startinfo2.Arguments = @"/c net start tvservice";
            startinfo2.WindowStyle = ProcessWindowStyle.Hidden;
            startinfo2.UseShellExecute = false;
            startinfo2.CreateNoWindow = true;
            startinfo2.RedirectStandardError = true;
            startinfo2.RedirectStandardInput = true;
            startinfo2.RedirectStandardOutput = true;

            proc2.StartInfo = startinfo2;
            proc2.EnableRaisingEvents = false;

            try
            {
                proc2.Start();
            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could not execute command \n" + startinfo2.FileName + " " + startinfo2.Arguments);
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                return;
            }
            proc2.WaitForExit(1000 * 60 * 3); //wait 3 minutes maximum
            if (proc2.HasExited == true)
            {
                if (proc2.ExitCode != 0)
                {
                    textoutputdebug("Tv Service error: Starting Tv service caused an error code " + proc2.ExitCode);

                    textoutputdebug("Reboot and check the TV service status from the TV server configuration tool\n");
                    return;
                }
            }
            else
            {
                textoutputdebug("Tv Service timeout error: Could not stop Tv service within time limit\n");
                textoutputdebug("Reboot and check the TV service status from the TV server configuration tool\n");
                return;
            }
            textoutputdebug("TV Service Started" + "\n");
        }
        */



        /// <summary>
        /// huha�s DirectoryCopy
        /// Copies a directory source including its subdirectories to a directory destination 
        /// If destination does not exists it will be created
        /// </summary>
        /// <param name="string source">string to the path of the source directory</param>
        /// <param name="string destination">string to the path of the destination directory</param>
        /// <param name="string filepattern">Filter for files (use "*" for all files)</param>
        /// <param name="bool overwrite">use "true" for overwriting existing files in the destination directory or otherwise "false" </param>
        /// <param name="bool verbose">use "true" for verbose output and logging</param>
        /// <param name="bool recursive">use "true" for including recursive directories</param>
        public void DirectoryCopy(string source, string destination, string filepattern, bool overwrite, bool verbose, bool recursive)
        {
            if (!File.Exists(destination))
            {
                try
                {
                    Directory.CreateDirectory(destination);
                }
                catch (Exception exc)
                {
                    textoutputdebug("DirectoryCopy Error: Could not create "+destination+" - Exception: "+exc.Message);
                }
            }

            // Copy files.
            DirectoryInfo sourceDir = new DirectoryInfo(source);
            FileInfo[] files = sourceDir.GetFiles(filepattern);
            foreach (FileInfo file in files)
            {
                
                    try
                    {
                        if (!File.Exists(destination + "\\" + file.Name))
                        { // file does not exist

                            File.Copy(file.FullName, destination + "\\" + file.Name, false);
                            if (verbose)
                            {
                                textoutputdebug("Copied: " + file.Name);
                            }

                        }
                        else if (overwrite == false) // and file does exist => do not copy
                        {
                            if (verbose)
                            {
                                textoutputdebug("Exists:" + file.Name);
                            }
                        }
                        else // file does exist
                        { // do overwrite files
                            // check for read only protection
                            FileAttributes attribute = File.GetAttributes(destination + "\\" + file.Name);

                            if ((attribute & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            {
                                if (verbose)
                                {
                                    textoutputdebug("ReadOnly: " + file.Name);
                                }
                            }
                            else if ((attribute & FileAttributes.Hidden) == FileAttributes.Hidden)
                            {
                                if (verbose)
                                {
                                    textoutputdebug("Hidden: " + file.Name);
                                }
                            }
                            else if ((attribute & FileAttributes.System) == FileAttributes.System)
                            {
                                if (verbose)
                                {
                                    textoutputdebug("System: " + file.Name);
                                }
                            }
                            else
                            {
                                File.Copy(file.FullName, destination + "\\" + file.Name, true);
                                if (verbose)
                                {
                                    textoutputdebug("Copied: " + file.Name);
                                }
                            }
                        }

                    }
                    catch (Exception exc)
                    {
                        textoutputdebug("<RED>DirectoryCopy Error: Could not copy file " + file.FullName + " to " + destination + "\\" + file.Name + " - Exception:" + exc.Message);
                    }
                
            }

            // subdirectories are being called recursively
            if (recursive)
            {
                DirectoryInfo sourceinfo = new DirectoryInfo(source);
                DirectoryInfo[] dirs = sourceinfo.GetDirectories();

                foreach (DirectoryInfo dir in dirs)
                {
                    string dirstring = dir.FullName;
                    FileAttributes attributes = dir.Attributes;

                    if (((attributes & FileAttributes.Hidden) != FileAttributes.Hidden) && ((attributes & FileAttributes.System) != FileAttributes.System))
                        DirectoryCopy(dirstring, destination + "\\" + dir, filepattern, overwrite, verbose, recursive);
                }
            }
        }

        public void DirectoryCopyBackupBefore(string source, string destination, string filepattern, bool overwrite, bool verbose, bool recursive)
        {
            if (!File.Exists(destination))
            {
                try
                {
                    Directory.CreateDirectory(destination);
                }
                catch (Exception exc)
                {
                    textoutputdebug("DirectoryCopy Error: Could not create " + destination + " - Exception: " + exc.Message);
                }
            }

            // Copy files.
            DirectoryInfo sourceDir = new DirectoryInfo(source);
            FileInfo[] files = sourceDir.GetFiles(filepattern);
            foreach (FileInfo file in files)
            {
                
                    try
                    {
                        if (!File.Exists(destination + "\\" + file.Name))
                        { // file does not exist

                            File.Copy(file.FullName, destination + "\\" + file.Name, false);
                            if (verbose)
                            {
                                textoutputdebug("Copied: " + file.Name);
                            }

                        }
                        else if (overwrite == false) // and file does exist => do not copy
                        {
                            if (verbose)
                            {
                                textoutputdebug("Exists:" + file.Name);
                            }
                        }
                        else // file does exist
                        { // do overwrite files
                            // check for read only protection
                            FileAttributes attribute = File.GetAttributes(destination + "\\" + file.Name);

                            if ((attribute & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            {
                                if (verbose)
                                {
                                    textoutputdebug("ReadOnly: " + file.Name);
                                }
                            }
                            else if ((attribute & FileAttributes.Hidden) == FileAttributes.Hidden)
                            {
                                if (verbose)
                                {
                                    textoutputdebug("Hidden: " + file.Name);
                                }
                            }
                            else if ((attribute & FileAttributes.System) == FileAttributes.System)
                            {
                                if (verbose)
                                {
                                    textoutputdebug("System: " + file.Name);
                                }
                            }
                            else
                            {
                                //backup before copy
                                string backupfile = file.Name.Replace(".xml",".xml.bak");

                                if (!File.Exists(destination + "\\" + backupfile))
                                {
                                    File.Move(destination + "\\" + file.Name , destination + "\\" + backupfile);
                                }

                                File.Copy(file.FullName, destination + "\\" + file.Name, true);
                                if (verbose)
                                {
                                    textoutputdebug("Copied: " + file.Name);
                                }
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        textoutputdebug("<RED>DirectoryCopy Error: Could not copy file " + file.FullName + " to " + destination + "\\" + file.Name + " - Exception:" + exc.Message);
                    }
                
            }

            // subdirectories are being called recursively
            if (recursive)
            {
                DirectoryInfo sourceinfo = new DirectoryInfo(source);
                DirectoryInfo[] dirs = sourceinfo.GetDirectories();

                foreach (DirectoryInfo dir in dirs)
                {
                    string dirstring = dir.FullName;
                    FileAttributes attributes = dir.Attributes;

                    if (((attributes & FileAttributes.Hidden) != FileAttributes.Hidden) && ((attributes & FileAttributes.System) != FileAttributes.System))
                        DirectoryCopy(dirstring, destination + "\\" + dir, filepattern, overwrite, verbose, recursive);
                }
            }
        }

        public void DirectoryDelete(string source)
        {
            // Copy files.
            DirectoryInfo sourceDir = new DirectoryInfo(source);
            FileInfo[] files = sourceDir.GetFiles("*");
            foreach (FileInfo file in files)
            {

                try
                {
                    File.Delete(file.FullName);
                    textoutputdebug("File " + file.FullName + " deleted");                 
                }
                catch (Exception exc)
                {
                    textoutputdebug("<RED>DirectoryDelete Error: Could not delete file " + file.FullName + " - Exception:" + exc.Message);
                }
                
            }

            // subdirectories are being called recursively
            
            DirectoryInfo sourceinfo = new DirectoryInfo(source);
            DirectoryInfo[] dirs = sourceinfo.GetDirectories();

            foreach (DirectoryInfo dir in dirs)
            {
                string dirstring = dir.FullName;
                FileAttributes attributes = dir.Attributes;

                if (((attributes & FileAttributes.Hidden) != FileAttributes.Hidden) && ((attributes & FileAttributes.System) != FileAttributes.System))
                    DirectoryDelete(dir.FullName);
            }

            Directory.Delete(source);
            
        }

        private int FileVersionComparison(string version1, string version2)
        {
            //valid return code: 
            //  1 version1 is newer than version2
            // -1 version1 is older than version2
            //  0 version1 is same as version2
            // 99 error occured

            //errorchecking version1
            string[] tokenarray1 = version1.Split('.');
            if (tokenarray1.Length != 4)
            {
                return (int)CompareFileVersion.Version1Error;
            }
            for (int i = 0; i < tokenarray1.Length; i++)
            {
                try
                {
                    int j = Convert.ToInt32(tokenarray1[i]);
                }
                catch
                {
                    return (int)CompareFileVersion.Version1String;
                }
            }

            //errorchecking version2
            string[] tokenarray2 = version2.Split('.');
            if (tokenarray2.Length != 4)
            {
                return (int)CompareFileVersion.Version2Error;
            }
            for (int i = 0; i < tokenarray2.Length; i++)
            {
                try
                {
                    int j = Convert.ToInt32(tokenarray2[i]);
                }
                catch
                {
                    return (int)CompareFileVersion.Version2String;
                }
            }

            if (Convert.ToInt32(tokenarray1[0]) > Convert.ToInt32(tokenarray2[0]))
            {
                return (int)CompareFileVersion.Newer;
            }
            else if (Convert.ToInt32(tokenarray1[0]) < Convert.ToInt32(tokenarray2[0]))
            {
                return (int)CompareFileVersion.Older;
            }
            else //same
            {
                if (Convert.ToInt32(tokenarray1[1]) > Convert.ToInt32(tokenarray2[1]))
                {
                    return (int)CompareFileVersion.Newer;
                }
                else if (Convert.ToInt32(tokenarray1[1]) < Convert.ToInt32(tokenarray2[1]))
                {
                    return (int)CompareFileVersion.Older;
                }
                else //same
                {
                    if (Convert.ToInt32(tokenarray1[2]) > Convert.ToInt32(tokenarray2[2]))
                    {
                        return (int)CompareFileVersion.Newer;
                    }
                    else if (Convert.ToInt32(tokenarray1[2]) < Convert.ToInt32(tokenarray2[2]))
                    {
                        return (int)CompareFileVersion.Older;
                    }
                    else //same
                    {
                        if (Convert.ToInt32(tokenarray1[3]) > Convert.ToInt32(tokenarray2[3]))
                        {
                            return (int)CompareFileVersion.Newer;
                        }
                        else if (Convert.ToInt32(tokenarray1[3]) < Convert.ToInt32(tokenarray2[3]))
                        {
                            return (int)CompareFileVersion.Older;
                        }
                        else //same
                        {
                            return (int)CompareFileVersion.Equal;
                        }
                    }
                }
            }

        }



        private void textoutputdebug(string textlines)
        {
            
            string text = "";

            if (listBox != null)
            {
                char[] splitterchars = { '\n' };  //split lines with \n
                string[] lines = textlines.Split(splitterchars);
                foreach (string line in lines)
                {
                    text = line;
                    while (text.Length > 70)   //split long lines
                    {
                        int linelength = 69;
                        for (int i = 69; i >= 4; i--)
                        {
                            if (text[i] == ' ')
                            {
                                linelength = i;
                                break;
                            }
                        }
                        string pretext = text.Substring(0, linelength);

                        listBox.Items.Add(pretext);

                        text = "+  " + text.Substring(linelength, text.Length - linelength);

                    }

                    listBox.Items.Add(text);
                    if (listBox.Items.Count > 10)
                        listBox.TopIndex = listBox.Items.Count - 9;

                    listBox.Update();
                }
            }
        }

        private void buttonuninstall_Click(object sender, EventArgs e)
        {
            uninstall();
        }

        private void uninstall()
        {
            UpdatePathVariables();
            UpdateGUI();


            if ((checkBoxTV.Checked) && (Directory.Exists(TV_PROGRAM_FOLDER) == true) && (Directory.Exists(TV_USER_FOLDER) == true) && (TV_PROGRAM_FOLDER != "") && (TV_USER_FOLDER != ""))
            {

                if (UNINSTALL == false)
                {

                    switch (MessageBox.Show("Do you want to uninstall the TvWishList plugin? This will delete all TvWishList informations like exported messages and tv wishes!", "TvWishList Plugin Deinstallation", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
                    {
                        case DialogResult.Yes:
                            {
                                // "Yes" processing
                                break;
                            }
                        case DialogResult.No:
                            {
                                // "No" processing
                                textoutputdebug("Uninstall aborted by user\n");
                                return;
                            }

                    }
                }



                //check for setupTv.exe
                Process[] sprocs = Process.GetProcessesByName("SetupTv");
                foreach (Process sproc in sprocs) // loop is only executed if Media portal is running
                {
                    textoutputdebug("You need to close Tv Server Configuration before you uninstall the plugin\n");
                    MessageBox.Show("You need to close Tv Server Configuration before you uninstall the plugin", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                //check for MediaPortal.exe
                sprocs = Process.GetProcessesByName("MediaPortal");
                foreach (Process sproc in sprocs) // loop is only executed if Media portal is running
                {
                    textoutputdebug("You need to close MediaPortal before you uninstall the plugin\n");
                    MessageBox.Show("You need to close MediaPortal before you uninstall the plugin", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //check for Configuration.exe
                sprocs = Process.GetProcessesByName("Configuration");
                foreach (Process sproc in sprocs) // loop is only executed if Media portal is running
                {
                    textoutputdebug("You need to close MediaPortal Configuration before you uninstall the plugin\n");
                    MessageBox.Show("You need to close MediaPortal Configuration before you uninstall the plugin", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                Process[] tprocs = null;
                tprocs = Process.GetProcessesByName("TVService");
                bool TvserviceRunning = false;
                foreach (Process tproc in tprocs)
                {
                    TvserviceRunning = true;
                }



                //Stopping tv service
                if (TvserviceRunning == true)
                {
                    StopTvService(); 
                }
                else
                {
                    textoutputdebug("TV Service is not running" + "\n");
                }


                TvServerUninstall();



                


                //Starting tv service
                if (TvserviceRunning == true)
                {
                    StartTvService(); 
                }

            }//end tvserver uninstall



            if ((checkBoxMP.Checked) && (Directory.Exists(MP_PROGRAM_FOLDER) == true) && (Directory.Exists(MP_USER_FOLDER) == true) && (MP_PROGRAM_FOLDER != "") && (MP_USER_FOLDER != ""))
            {
                if (UNINSTALL == false)
                {
                    switch (MessageBox.Show("Do you want to uninstall the TvWishListMP MediaPortal1 Client Plugin? ", "MediaPortal1 TvWishList Plugin Deinstallation", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
                    {
                        case DialogResult.Yes:
                            {
                                // "Yes" processing


                                break;
                            }
                        case DialogResult.No:
                            {
                                // "No" processing
                                textoutputdebug("Uninstall aborted by user\n");
                                return;
                            }
                    }
                }

                //check for MediaPortal.exe
                Process[] sprocs = Process.GetProcessesByName("MediaPortal");
                foreach (Process sproc in sprocs) // loop is only executed if Media portal is running
                {
                    textoutputdebug("You need to close MediaPortal before you uninstall the plugin\n");
                    MessageBox.Show("You need to close MediaPortal before you uninstall the plugin", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MP1Uninstall();

            }//end MP1 deinstallation


            if ((checkBoxMP2C.Checked) && (Directory.Exists(MP2_PROGRAM_FOLDER) == true) && (Directory.Exists(MP2_USER_FOLDER) == true) && (MP2_PROGRAM_FOLDER != "") && (MP2_USER_FOLDER != ""))
            {
                if (UNINSTALL == false)
                {
                    switch (MessageBox.Show("Do you want to uninstall the TvWishList MediaPortal2 Client Plugin and the TvWishListProvider Plugin? ", "MediaPortal2 Client TvWishList Plugin Deinstallation", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
                    {
                        case DialogResult.Yes:
                            {
                                // "Yes" processing


                                break;
                            }
                        case DialogResult.No:
                            {
                                // "No" processing
                                textoutputdebug("Uninstall aborted by user\n");
                                return;
                            }
                    }
                }

                //check for MP2-Client.exe
                Process[] sprocs = Process.GetProcessesByName("MP2-Client");
                foreach (Process sproc in sprocs) // loop is only executed if Media portal is running
                {
                    textoutputdebug("You need to close the MediaPortal2 Client before you uninstall the plugin\n");
                    MessageBox.Show("You need to close the MediaPortal2 Client before you uninstall the plugin", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MP2ClientUninstall();


            }//end MP2 Client deinstallation
            
        }

        private void TvServerUninstall()
        {
            try  //uninstall /TvWishList on Tvserver
            {

                if (File.Exists(TV_USER_FOLDER + @"\TvWishList\TvWishList.pdf") == true)
                {
                    File.Delete(TV_USER_FOLDER + @"\TvWishList\TvWishList.pdf");
                    textoutputdebug("Deleting " + TV_USER_FOLDER + @"\TvWishList\TvWishList.pdf" + " \n");
                }

                if (File.Exists(TV_USER_FOLDER + @"\TvWishList\Messages.xml") == true)
                {
                    File.Delete(TV_USER_FOLDER + @"\TvWishList\Messages.xml");
                    textoutputdebug("Deleting " + TV_USER_FOLDER + @"\TvWishList\Messages.xml" + " \n");
                }

                if (File.Exists(TV_USER_FOLDER + @"\TvWishList\Tvwishes.txt") == true)
                {
                    File.Delete(TV_USER_FOLDER + @"\TvWishList\Tvwishes.txt");
                    textoutputdebug("Deleting " + TV_USER_FOLDER + @"\TvWishList\Tvwishes.txt" + " \n");
                }

                if (File.Exists(TV_USER_FOLDER + @"\TvWishList\filewatchermessages.txt") == true)
                {
                    File.Delete(TV_USER_FOLDER + @"\TvWishList\filewatchermessages.txt");
                    textoutputdebug("Deleting " + TV_USER_FOLDER + @"\TvWishList\filewatchermessages.txt" + " \n");
                }

                if (File.Exists(TV_USER_FOLDER + @"\TvWishList\FileRenameXML.xml") == true)
                {
                    File.Delete(TV_USER_FOLDER + @"\TvWishList\FileRenameXML.xml");
                    textoutputdebug("Deleting " + TV_USER_FOLDER + @"\TvWishList\FileRenameXML.xml" + " \n");
                }

                if (File.Exists(TV_USER_FOLDER + @"\TvWishList\Version.txt") == true)
                {
                    File.Delete(TV_USER_FOLDER + @"\TvWishList\Version.txt");
                    textoutputdebug("Deleting " + TV_USER_FOLDER + @"\TvWishList\Version.txt" + " \n");
                }


                


                if (File.Exists(TV_PROGRAM_FOLDER + @"\Plugins\TvWishList.dll") == true)
                {
                    File.Delete(TV_PROGRAM_FOLDER + @"\Plugins\TvWishList.dll");
                    textoutputdebug("Deleting " + TV_PROGRAM_FOLDER + @"\Plugins\TvWishList.dll" + "  \n");
                }

                //uninstall log files
                string log_file = TV_USER_FOLDER + @"\log\TvWishList";
                try
                {
                    File.Delete(log_file + ".log");
                }
                catch (Exception exc)
                {
                    textoutputdebug("Error in deleting log file " + log_file + ".log" + " exception is: \n" + exc.Message + "\n");
                }
                try
                {
                    File.Delete(log_file + ".bak");
                }
                catch (Exception exc)
                {
                    textoutputdebug("Error in deleting log file " + log_file + ".bak" + " exception is: \n" + exc.Message + "\n");
                }

                //uninstall language files
                try
                {                    
                    string lang_dir = TV_USER_FOLDER + @"\TvWishList\Languages";
                    DirectoryInfo mydirinfo = new DirectoryInfo(lang_dir);
                    FileInfo[] files = mydirinfo.GetFiles("*.xml");
                    foreach (FileInfo file in files)
                    {
                        try
                        {
                            File.Delete(file.FullName);
                        }
                        catch (Exception exc)
                        {
                            textoutputdebug("Error in deleting language file " + file.FullName + " exception is: \n" + exc.Message + "\n");
                        }
                    }

                    try
                    {
                        Directory.Delete(lang_dir);
                        textoutputdebug("Language directory" + lang_dir + " deleted \n\n");
                    }
                    catch (Exception exc)
                    {
                        textoutputdebug("Error in deleting language folder " + lang_dir + " exception is: \n" + exc.Message + "\n");
                    }
                }
                catch (Exception exc)
                {
                    textoutputdebug("Error in uninstalling language files - exception is: \n" + exc.Message + "\n");
                }//end uninstall language files

                //uninstall .txt and xml files for tvwishes and messages
                try
                {
                    string lang_dir = TV_USER_FOLDER + @"\TvWishList";
                    DirectoryInfo mydirinfo = new DirectoryInfo(lang_dir);
                    FileInfo[] files = mydirinfo.GetFiles("*.xml");
                    foreach (FileInfo file in files)
                    {
                        try
                        {
                            File.Delete(file.FullName);
                        }
                        catch (Exception exc)
                        {
                            textoutputdebug("Error in deleting .xml file " + file.FullName + " exception is: \n" + exc.Message + "\n");
                        }
                    }

                    files = mydirinfo.GetFiles("*.txt");
                    foreach (FileInfo file in files)
                    {
                        try
                        {
                            File.Delete(file.FullName);
                        }
                        catch (Exception exc)
                        {
                            textoutputdebug("Error in deleting .txt file " + file.FullName + " exception is: \n" + exc.Message + "\n");
                        }
                    }
                }
                catch (Exception exc)
                {
                    textoutputdebug("Error in uninstalling xml or txt files - exception is: \n" + exc.Message + "\n");
                }//end uninstall xml and txt files


                try //must be last
                {
                    Directory.Delete(TV_USER_FOLDER + @"\TvWishList");
                }
                catch //do nothing
                {
                }

                if (Directory.Exists(TV_USER_FOLDER + @"\TvWishList") == false)
                {
                    textoutputdebug("Deleting " + TV_USER_FOLDER + @"\TvWishList");
                }
                else
                {
                    textoutputdebug("Directory " + TV_USER_FOLDER + @"\TvWishList  is not empty - not deleted" + "\n");
                }

                textoutputdebug("Tv Server Plugin deinstallation succeeded\n");

            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Uninstalling TvWishList caused exception \n" + exc.Message);
            }


        }

        private void MP1Uninstall()
        {
            try  //uninstall TvWishListMP on Mediaportal
            {

                if (File.Exists(MP_PROGRAM_FOLDER + @"\plugins\Windows\TvWishListMP.dll") == true)
                {
                    File.Delete(MP_PROGRAM_FOLDER + @"\plugins\Windows\TvWishListMP.dll");
                    textoutputdebug("Deleting " + MP_PROGRAM_FOLDER + @"\plugins\Windows\TvWishListMP.dll" + " \n");
                }

                if (File.Exists(MP_PROGRAM_FOLDER + @"\Docs\TvWishList.pdf") == true)
                {
                    File.Delete(MP_PROGRAM_FOLDER + @"\Docs\TvWishList.pdf");
                    textoutputdebug("Deleting " + MP_PROGRAM_FOLDER + @"\Docs\TvWishList.pdf" + " \n");
                }

                if (File.Exists(MP_USER_FOLDER + @"\TvWishListMP.xml") == true)
                {
                    File.Delete(MP_USER_FOLDER + @"\TvWishListMP.xml");
                    textoutputdebug("Deleting " + MP_USER_FOLDER + @"\TvWishListMP.xml" + " \n");
                }

                if (File.Exists(MP_USER_FOLDER + @"\TvWishListMP.xml.bak") == true)
                {
                    File.Delete(MP_USER_FOLDER + @"\TvWishListMP.xml.bak");
                    textoutputdebug("Deleting " + MP_USER_FOLDER + @"\TvWishListMP.xml.bak" + " \n");
                }

                //uninstall log files
                string log_file = instpaths.DIR_Log + @"\TvWishList";
                try
                {
                    File.Delete(log_file + ".log");
                }
                catch (Exception exc)
                {
                    textoutputdebug("Error in deleting log file " + log_file + ".log" + " exception is: \n" + exc.Message + "\n");
                }
                try
                {
                    File.Delete(log_file + ".bak");
                }
                catch (Exception exc)
                {
                    textoutputdebug("Error in deleting log file " + log_file + ".bak" + " exception is: \n" + exc.Message + "\n");
                }


                //uninstall language files

                try
                {
                    string lang_dir = instpaths.DIR_Language + @"\TvWishListMP";
                    DirectoryInfo mydirinfo = new DirectoryInfo(lang_dir);
                    FileInfo[] files = mydirinfo.GetFiles("*.xml");
                    foreach (FileInfo file in files)
                    {
                        try
                        {
                            File.Delete(file.FullName);
                        }
                        catch (Exception exc)
                        {
                            textoutputdebug("Error in deleting language file " + file.FullName + " exception is: \n" + exc.Message + "\n");
                        }
                    }

                    try
                    {
                        Directory.Delete(lang_dir);
                        textoutputdebug("Language directory" + lang_dir + " deleted \n\n");
                    }
                    catch (Exception exc)
                    {
                        textoutputdebug("Error in deleting language folder " + lang_dir + " exception is: \n" + exc.Message + "\n");
                    }
                }
                catch (Exception exc)
                {
                    textoutputdebug("Error in uninstalling language files - exception is: \n" + exc.Message + "\n");
                }


                //uninstall skins

                foreach (string skin_name in allskins)
                {
                    string destination = instpaths.DIR_Skin + skin_name;

                    if (Directory.Exists(destination) == true)
                    {
                        try
                        {
                            File.Delete(destination + @"\TvWishListMP.xml");
                            File.Delete(destination + @"\TvWishListMP_2.xml");
                            File.Delete(destination + @"\TvWishListMP_3.xml");
                            File.Delete(destination + @"\Media\hover_TvWishList.png");
                            File.Delete(destination + @"\Media\TvWishList.Icon.png");
                            textoutputdebug("TvWishListMP skin for " + skin_name + " has been uninstalled \n");
                        }
                        catch (Exception exc)
                        {
                            textoutputdebug("Error in deleting TvWishListMP file for skin " + skin_name + " exception is: \n" + exc.Message + "\n");
                        }


                    }
                }

                //uninstall skin mods

                foreach (string skin_name in allskinmods)
                {
                    string destination = instpaths.DIR_Skin + skin_name;

                    if (Directory.Exists(destination) == true)
                    {
                        try
                        {
                            if (File.Exists(destination + @"\dialogmenu.xml.bak"))
                            {
                                File.Delete(destination + @"\dialogmenu.xml");
                                File.Move(destination + @"\dialogmenu.xml.bak", destination + @"\dialogmenu.xml");
                            }
                            textoutputdebug("TvWishListMP skin for " + skin_name + " has been restored for skin mods \n");


                        }
                        catch (Exception exc)
                        {
                            textoutputdebug("Error in restoring files for skin " + skin_name + " exception is: \n" + exc.Message + "\n");
                        }


                    }
                }

                textoutputdebug("MediaPortal1 Plugin deinstallation succeeded\n");
            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Uninstalling TvWishList caused exception \n" + exc.Message);
            }
        }

        private void MP2ClientUninstall()
        {
            //Delete MP2 TvWishList Client plugin folder
            string TvWishListMP2Folder = instpaths.DIR_MP2_Plugins + @"\TvWishListMP2";
            textoutputdebug("Uninstalling MediaPortal2 Plugin TvWishListMP2 in \n" + TvWishListMP2Folder+"\n");
            
            try
            {
                if (Directory.Exists(TvWishListMP2Folder))
                {//Delete old files
                    DirectoryDelete(TvWishListMP2Folder);
                }
                
            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could delete folder TvWishListMP2 \n");
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                return;
            }

            //Delete MP2 TvWishListProvider plugin folder
            string TvWishListMP2ProviderFolder = instpaths.DIR_MP2_Plugins + @"\TvWishListMPExtendedProvider0.5";
            string TvWishListMP2ProviderName = "TvWishListMPExtendedProvider0.5";
            textoutputdebug("Uninstalling MediaPortal2 Plugin " + TvWishListMP2ProviderName + " in \n" + TvWishListMP2ProviderFolder + "\n");

            try
            {
                if (Directory.Exists(TvWishListMP2ProviderFolder))
                {//Delete old files
                    DirectoryDelete(TvWishListMP2ProviderFolder);
                }

            }
            catch (Exception exc)
            {
                textoutputdebug("Error: Could not delete folder " + TvWishListMP2ProviderName + " \n");
                textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                return;
            }

            //Delete setting file in user folders
            DirectoryInfo sourceinfo = new DirectoryInfo(instpaths.DIR_MP2_Config);
            DirectoryInfo[] dirs = sourceinfo.GetDirectories();

            foreach (DirectoryInfo dir in dirs)
            {
                string settingFile = instpaths.DIR_MP2_Config + @"\" + dir.FullName + @"\TvWishListMP2.xml";
                if (File.Exists(settingFile))
                {
                    try
                    {
                        File.Delete(settingFile);
                        textoutputdebug("File " + settingFile + " deleted");
                    }
                    catch (Exception exc)
                    {
                        textoutputdebug("Error: Could delete setting file " + settingFile);
                        textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                        return;
                    }
                }
                settingFile = instpaths.DIR_MP2_Config + @"\" + dir.FullName + @"\TvWishListMP2.xml.bak";
                if (File.Exists(settingFile))
                {
                    try
                    {
                        File.Delete(settingFile);
                        textoutputdebug("File " + settingFile + " deleted");
                    }
                    catch (Exception exc)
                    {
                        textoutputdebug("Error: Could delete setting file " + settingFile);
                        textoutputdebug("Exception message was:\n" + exc.Message + "\n");
                        return;
                    }
                }

                
            }

            textoutputdebug("MediaPortal2 Plugin deinstallation succeeded\n");
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        
        private void buttonMP1P_Click(object sender, EventArgs e)
        {
            instpaths.LOG = string.Empty;
            textBoxMP1P.Text = instpaths.ask_MP_PROGRAM_ALWAYS();
            if (instpaths.LOG != string.Empty)
            {
                MessageBox.Show(instpaths.LOG);
                instpaths.LOG = string.Empty;
            }
        }

        private void buttonMP1U_Click(object sender, EventArgs e)
        {
            instpaths.LOG = string.Empty;
            textBoxMP1U.Text = instpaths.ask_MP_USER_ALWAYS();
            if (instpaths.LOG != string.Empty)
            {
                MessageBox.Show(instpaths.LOG);
                instpaths.LOG = string.Empty;
            }
        }

        private void buttonTV1P_Click(object sender, EventArgs e)
        {
            instpaths.LOG = string.Empty;
            textBoxTV1P.Text = instpaths.ask_TV_PROGRAM_ALWAYS();
            if (instpaths.LOG != string.Empty)
            {
                MessageBox.Show(instpaths.LOG);
                instpaths.LOG = string.Empty;
            }
        }

        private void buttonTV1U_Click(object sender, EventArgs e)
        {
            instpaths.LOG = string.Empty;
            textBoxTV1U.Text = instpaths.ask_TV_USER_ALWAYS();
            if (instpaths.LOG != string.Empty)
            {
                MessageBox.Show(instpaths.LOG);
                instpaths.LOG = string.Empty;
            }
        }

        private void buttonMP2P_Click(object sender, EventArgs e)
        {
            instpaths.LOG = string.Empty;
            textBoxMP2P.Text = instpaths.ask_MP2_PROGRAM_ALWAYS();
            if (instpaths.LOG != string.Empty)
            {
                MessageBox.Show(instpaths.LOG);
                instpaths.LOG = string.Empty;
            }
        }

        private void buttonMP2U_Click(object sender, EventArgs e)
        {
            instpaths.LOG = string.Empty;
            textBoxMP2U.Text = instpaths.ask_MP2_USER_ALWAYS();
            if (instpaths.LOG != string.Empty)
            {
                MessageBox.Show(instpaths.LOG);
                instpaths.LOG = string.Empty;
            }
        }


        private void buttonDetect_Click(object sender, EventArgs e)
        {
            AutoDetect();
            UpdatePathVariables();
            UpdateGUI();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            //updating installation button visibility on main tab
            UpdateGUI();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            listBox.Items.Clear();
        }

        private void buttonReadme_Click_1(object sender, EventArgs e)
        {
            Process proc = new Process();
            ProcessStartInfo procstartinfo = new ProcessStartInfo();
            procstartinfo.FileName = "TvWishList.pdf";
            procstartinfo.WorkingDirectory = System.Environment.CurrentDirectory;
            proc.StartInfo = procstartinfo;
            try
            {
                proc.Start();
            }
            catch
            {
                MessageBox.Show("Could not open " + procstartinfo.WorkingDirectory + "\\" + procstartinfo.FileName, "Error");
            }
        }

        

        


    }
}
