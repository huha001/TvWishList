#region Copyright (C)
/* 
 *	Copyright (C) 2005-2012 Team MediaPortal
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
#endregion Copyright (C)

//*****************
//Version 1.0.2.0
//*****************

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
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;




namespace MediaPortal.Plugins
{
    //[CLSCompliant(false)]
    public delegate void textmessage(string text);
    public class InstallPaths
    {

        #region Properties


        //Main installation folders
        string _MP_PROGRAM_FOLDER = "NOT_DEFINED";
        string _MP_USER_FOLDER = "NOT_DEFINED";
        string _TV_PROGRAM_FOLDER = "NOT_DEFINED";
        string _TV_USER_FOLDER = "NOT_DEFINED";

        string _TV2_PROGRAM_FOLDER = "NOT_DEFINED";
        string _TV2_USER_FOLDER = "NOT_DEFINED";

        string _MP2_PROGRAM_FOLDER = "NOT_DEFINED";
        string _MP2_USER_FOLDER = "NOT_DEFINED";
        string _SV2_PROGRAM_FOLDER = "NOT_DEFINED";
        string _SV2_USER_FOLDER = "NOT_DEFINED";

        //All paths from MediaPortalDirs.xml
        string _DIR_Config = "NOT_DEFINED";
        string _DIR_Plugins = "NOT_DEFINED";
        string _DIR_LOG = "NOT_DEFINED";
        string _DIR_CustomInputDevice = "NOT_DEFINED";
        string _DIR_CustomInputDefault = "NOT_DEFINED";
        string _DIR_Skin = "NOT_DEFINED";
        string _DIR_Language = "NOT_DEFINED";
        string _DIR_Database = "NOT_DEFINED";
        string _DIR_Thumbs = "NOT_DEFINED";
        string _DIR_Weather = "NOT_DEFINED";
        string _DIR_Cache = "NOT_DEFINED";
        string _DIR_BurnerSupport = "NOT_DEFINED";
        string _FILE_MediaPortalDirs = "NOT_DEFINED";

        //All paths from MP2Server
        string _FILE_SV2_Paths = "NOT_DEFINED";
        string _DIR_SV2_Data = "NOT_DEFINED";
        string _DIR_SV2_LOG = "NOT_DEFINED";
        string _DIR_SV2_Config = "NOT_DEFINED";
        string _DIR_SV2_Database = "NOT_DEFINED";
        string _DIR_SV2_Plugins = "NOT_DEFINED";

        //All paths from MP2Client
        string _FILE_MP2_Paths = "NOT_DEFINED";
        string _DIR_MP2_Data = "NOT_DEFINED";
        string _DIR_MP2_LOG = "NOT_DEFINED";
        string _DIR_MP2_Config = "NOT_DEFINED";
        string _DIR_MP2_Plugins = "NOT_DEFINED";
               
        // other properties
        //bool USERPROFILE_EXISTS = false;
        //bool USERPROFILEMP2_EXISTS = false;
        //bool USERPROFILESV2_EXISTS = false;

        bool _DEBUG = false;
        string _STARTSWITH = "";
        string _LOG = "";


        public event textmessage newmessage;

        
        //Base Paths Properties
        public string TV_PROGRAM_FOLDER
        {
            get { return _TV_PROGRAM_FOLDER; }
            set { _TV_PROGRAM_FOLDER = value; }
        }

        public string TV_USER_FOLDER
        {
            get { return _TV_USER_FOLDER; }
            set { _TV_USER_FOLDER = value; }
        }

        public string TV2_PROGRAM_FOLDER
        {
            get { return _TV2_PROGRAM_FOLDER; }
            set { _TV2_PROGRAM_FOLDER = value; }
        }

        public string TV2_USER_FOLDER
        {
            get { return _TV2_USER_FOLDER; }
            set { _TV2_USER_FOLDER = value; }
        }

        public string MP_PROGRAM_FOLDER
        {
            get { return _MP_PROGRAM_FOLDER; }
            set { _MP_PROGRAM_FOLDER = value; }
        }

        public string MP_USER_FOLDER
        {
            get { return _MP_USER_FOLDER; }
            set { _MP_USER_FOLDER = value; }
        }

        public string MP2_PROGRAM_FOLDER
        {
            get { return _MP2_PROGRAM_FOLDER; }
            set { _MP2_PROGRAM_FOLDER = value; }
        }

        public string MP2_USER_FOLDER
        {
            get { return _MP2_USER_FOLDER; }
            set { _MP2_USER_FOLDER = value; }
        }

        public string SV2_PROGRAM_FOLDER
        {
            get { return _SV2_PROGRAM_FOLDER; }
            set { _SV2_PROGRAM_FOLDER = value; }
        }

        public string SV2_USER_FOLDER
        {
            get { return _SV2_USER_FOLDER; }
            set { _SV2_USER_FOLDER = value; }
        }


        //MP1 Property DIRs

        public string DIR_Config
        {
            get { return _DIR_Config; }
            set { _DIR_Config = value; }
        }

        public string DIR_Plugins
        {
            get { return _DIR_Plugins; }
            set { _DIR_Plugins = value; }
        }

        public string DIR_Log
        {
            get { return _DIR_LOG; }
            set { _DIR_LOG = value; }
        }

        public string DIR_CustomInputDevice
        {
            get { return _DIR_CustomInputDevice; }
            set { _DIR_CustomInputDevice = value; }
        }

        public string DIR_CustomInputDefault
        {
            get { return _DIR_CustomInputDefault; }
            set { _DIR_CustomInputDefault = value; }
        }

        public string DIR_Skin
        {
            get { return _DIR_Skin; }
            set { _DIR_Skin = value; }
        }

        public string DIR_Language
        {
            get { return _DIR_Language; }
            set { _DIR_Language = value; }
        }

        public string DIR_Database
        {
            get { return _DIR_Database; }
            set { _DIR_Database = value; }
        }

        public string DIR_Thumbs
        {
            get { return _DIR_Thumbs; }
            set { _DIR_Thumbs = value; }
        }

        public string DIR_Weather
        {
            get { return _DIR_Weather; }
            set { _DIR_Weather = value; }
        }

        public string DIR_Cache
        {
            get { return _DIR_Cache; }
            set { _DIR_Cache = value; }
        }

        public string DIR_BurnerSupport
        {
            get { return _DIR_BurnerSupport; }
            set { _DIR_BurnerSupport = value; }
        }

        public string FILE_MediaPortalDirs
        {
            get { return _FILE_MediaPortalDirs; }
            set { _FILE_MediaPortalDirs = value; }
        }

        //MP2 Client Properties
        public string FILE_SV2_Paths
        {
            get { return _FILE_SV2_Paths; }
            set { _FILE_SV2_Paths = value; }
        }

        public string DIR_SV2_Data
        {
            get { return _DIR_SV2_Data; }
            set { _DIR_SV2_Data = value; }
        }

        public string DIR_SV2_LOG
        {
            get { return _DIR_SV2_LOG; }
            set { _DIR_SV2_LOG = value; }
        }

        public string DIR_SV2_Config
        {
            get { return _DIR_SV2_Config; }
            set { _DIR_SV2_Config = value; }
        }

        public string DIR_SV2_Database
        {
            get { return _DIR_SV2_Database; }
            set { _DIR_SV2_Database = value; }
        }

        public string DIR_SV2_Plugins
        {
            get { return _DIR_SV2_Plugins; }
            set { _DIR_SV2_Plugins = value; }
        }

        //MP2 Server Properties
        public string FILE_MP2_Paths
        {
            get { return _FILE_MP2_Paths; }
            set { _FILE_MP2_Paths = value; }
        }

        public string DIR_MP2_Data
        {
            get { return _DIR_MP2_Data; }
            set { _DIR_MP2_Data = value; }
        }

        public string DIR_MP2_LOG
        {
            get { return _DIR_MP2_LOG; }
            set { _DIR_MP2_LOG = value; }
        }

        public string DIR_MP2_Config
        {
            get { return _DIR_MP2_Config; }
            set { _DIR_MP2_Config = value; }
        }

        public string DIR_MP2_Plugins
        {
            get { return _DIR_MP2_Plugins; }
            set { _DIR_MP2_Plugins = value; }
        }


        //other properties
        public bool DEBUG
        {
            get { return _DEBUG; }
            set { _DEBUG = value; }
        }

        public string STARTSWITH
        {
            get { return _STARTSWITH; }
            set { _STARTSWITH = value; }
        }

        public string LOG
        {
            get { return _LOG; }
            set { _LOG = value; }
        }

        #endregion Properties


        #region Methods


        public void GetInstallPaths()
        {
            // searches for MediaPortal and Tv server installation paths 
            // tries to autodetect the following folder
            //      string _MP_PROGRAM_FOLDER  (MediaPortal Program Base)
            //      string _MP_USER_FOLDER     (MediaPortal Configuration)    
            //      string _TV_PROGRAM_FOLDER  (TV Server Program Base)
            //      string _TV_USER_FOLDER      TV Server Configuration)
            // autodetection supports only English or German XP or Vista version 
            // if no valid folder is found the variable contains the value "NOT_DEFINED"
            //
            // autodetect uses the MediaPortal paths from MediaPortalDirs.xml
            // supports reading of MediaPortalDirs.xml from Userprofile Windows path and 
            // sets the variable USERPROFILE_EXISTS to true if such a file exists within the Userprofile
            // The following paths are set from the info of the MediaPortalDirs.xml file
            // If no file MediaPortalDirs.xml is found the paths are set to "NOT_DEFINED"
            //      string _DIR_Config
            //      string _DIR_Plugins
            //      string _DIR_LOG
            //      string _DIR_CustomInputDevice
            //      string _DIR_CustomInputDefault
            //      string _DIR_Skin
            //      string _DIR_Language
            //      string _DIR_Database
            //      string _DIR_Thumbs
            //      string _DIR_Weather
            //      string _DIR_BurnerSupport
            //      string DIR_MediaPortalDirs
            //
            // Other properties:
            //      string USERPROFILE_EXISTS
            //      string _DEBUG
            //      string _STARTSWITH
            //      string _LOG
            // 
            //
            // 
            //




            //************************************
            //try autodetect Mediaportal.exe
            //************************************
            try
            {

                //start with current directory
                string thisdir=System.IO.Directory.GetCurrentDirectory();

                //try to find "Team MediaPortal" folder - get _MP_PROGRAM_FOLDER
                string testpath="NOT_DEFINED";
                string testfile = "NOT_DEFINED";
                int pos = 0;
                if (thisdir.Contains(@"\Team MediaPortal\")==true)
                {
                    //try to find "MediaPortal\Mediaportal.exe" - get _MP_PROGRAM_FOLDER
                    pos = thisdir.IndexOf(@"\Team MediaPortal\");
                    testpath = thisdir.Substring(0,pos)+@"\Team MediaPortal\MediaPortal";
                    testfile = thisdir.Substring(0, pos) + @"\Team MediaPortal\MediaPortal\Mediaportal.exe";
                }
                 
                if (File.Exists(testfile)==true)
                {
                    if (testpath.StartsWith(_STARTSWITH) == true)
                    {
                        _MP_PROGRAM_FOLDER = testpath;
                        if (_DEBUG == true)
                        {
                                textoutput( "New thisdir path " + _MP_PROGRAM_FOLDER + " found for _MP_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }    
                //try to find "%PROGRAMFILES%" - get _MP_PROGRAM_FOLDER
                else if (File.Exists(Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Team MediaPortal\MediaPortal\Mediaportal.exe") == true)
                {
                    string path = Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Team MediaPortal\MediaPortal";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _MP_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput( "New %PROGRAMFILES% path " + _MP_PROGRAM_FOLDER + " found for _MP_PROGRAM_FOLDER" + "\n");
                        }                 
                    }                    
                }
                //try to find "%PROGRAMFILES% (x86)" - get _MP_PROGRAM_FOLDER
                else if (File.Exists(Environment.GetEnvironmentVariable("PROGRAMFILES") + @" (x86)\Team MediaPortal\MediaPortal\Mediaportal.exe") == true)
                {
                    string path = Environment.GetEnvironmentVariable("PROGRAMFILES") + @" (x86)\Team MediaPortal\MediaPortal";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _MP_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput("New %PROGRAMFILES%  (x86) path " + _MP_PROGRAM_FOLDER + " found for _MP_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "C:\Program Files" - get _MP_PROGRAM_FOLDER
                else if (File.Exists(@"C:\Program Files\Team MediaPortal\MediaPortal\Mediaportal.exe") == true)
                {
                    string path = @"C:\Program Files\Team MediaPortal\MediaPortal";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _MP_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput( @"New C:\Program Files path " + _MP_PROGRAM_FOLDER + " found for _MP_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "C:\Program Files (x86)" - get _MP_PROGRAM_FOLDER
                else if (File.Exists(@"C:\Program Files (x86)\Team MediaPortal\MediaPortal\Mediaportal.exe") == true)
                {
                    string path = @"C:\Program Files (x86)\Team MediaPortal\MediaPortal";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _MP_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput(@"New C:\Program Files (x86) path " + _MP_PROGRAM_FOLDER + " found for _MP_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "C:\Programme" - get _MP_PROGRAM_FOLDER
                else if (File.Exists(@"C:\Programme\Team MediaPortal\MediaPortal\Mediaportal.exe") == true)
                {
                    string path = @"C:\Programme\Team MediaPortal\MediaPortal";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _MP_PROGRAM_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput( @"New C:\Programme path " + _MP_PROGRAM_FOLDER + " found for _MP_PROGRAM_FOLDER" + "\n");
                            }
                        }
                    }
                }
                else 
                {
                    //registry
                    try
                    {
                        RegistryKey regkey = Registry.LocalMachine;
                        RegistryKey subkey = regkey.OpenSubKey(@"Software\Team MediaPortal\MediaPortal");
                        string path = subkey.GetValue("ApplicationDir").ToString();
                        regkey.Close();
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            if (path.StartsWith(_STARTSWITH) == true)
                            {
                                _MP_PROGRAM_FOLDER = path;
                                if (_DEBUG == true)
                                {
                                    textoutput( @"New registry path " + _MP_PROGRAM_FOLDER + " found for _MP_PROGRAM_FOLDER" + "\n");
                                }
                            }
                        }
                    }
                    catch (Exception ee)
                    {
                        textoutput( "Failed reading registry key ApplicationDir - Exception message is \n" + ee.Message + "\n");
                    }
                }


                if ((_DEBUG == true) && (_MP_PROGRAM_FOLDER == "NOT_DEFINED"))
                {
                    textoutput( "_MP_PROGRAM_FOLDER was not found" + "\n");
                }

                
            }
            catch (Exception ee)
            {
                textoutput( "Failed to autodetect _MP_PROGRAM_FOLDER - Exception message is \n" + ee.Message + "\n");
            }



            //************************************
            //try autodetect TvService.exe
            //************************************
            try
            {

                //start with current directory
                string thisdir = System.IO.Directory.GetCurrentDirectory();

                //try to find "Team MediaPortal" folder - get _MP_PROGRAM_FOLDER
                string testpath = "NOT_DEFINED";
                string testfile = "NOT_DEFINED";
                string mediaportaltestpath = "NOT_DEFINED";
                string mediaportaltestfile = "NOT_DEFINED";

                int pos = 0;
                if (thisdir.Contains(@"\Team MediaPortal\") == true)
                {
                    //try to find "MediaPortal TV server\TvService.exe" - get _TV_PROGRAM_FOLDER
                    pos = thisdir.IndexOf(@"\Team MediaPortal\");
                    testpath = thisdir.Substring(0, pos) + @"\Team MediaPortal\MediaPortal TV Server";
                    testfile = thisdir.Substring(0, pos) + @"\Team MediaPortal\MediaPortal TV Server\TvService.exe";
                }
                if (_MP_PROGRAM_FOLDER != "NOT_DEFINED")
                {
                    string testserverpath = _MP_PROGRAM_FOLDER;
                    pos = testserverpath.IndexOf(@"\Team MediaPortal\");
                    mediaportaltestpath = testserverpath.Substring(0, pos) + @"\Team MediaPortal\MediaPortal TV Server";
                    mediaportaltestfile = testserverpath.Substring(0, pos) + @"\Team MediaPortal\MediaPortal TV Server\TvService.exe";
                }

                if (File.Exists(testfile) == true)
                {
                    if (testpath.StartsWith(_STARTSWITH) == true)
                    {
                        _TV_PROGRAM_FOLDER = testpath;
                        if (_DEBUG == true)
                        {
                            textoutput( "New thisdir path " + _TV_PROGRAM_FOLDER + " found for _TV_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                else if (File.Exists(mediaportaltestfile)==true)
                {
                    if (testpath.StartsWith(_STARTSWITH) == true)
                    {
                        _TV_PROGRAM_FOLDER = mediaportaltestpath;
                        if (_DEBUG == true)
                        {
                            textoutput( "New mediaportal tv path " + _TV_PROGRAM_FOLDER + " found for _TV_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "%PROGRAMFILES%" - get _TV_PROGRAM_FOLDER
                else if (File.Exists(Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Team MediaPortal\MediaPortal TV server\TvService.exe") == true)
                {
                    string path = Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Team MediaPortal\MediaPortal TV server";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _TV_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput( "New %PROGRAMFILES% path " + _TV_PROGRAM_FOLDER + " found for _TV_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "%PROGRAMFILES% (x86)" - get _TV_PROGRAM_FOLDER
                else if (File.Exists(Environment.GetEnvironmentVariable("PROGRAMFILES") + @" (x86)\Team MediaPortal\MediaPortal TV server\TvService.exe") == true)
                {
                    string path = Environment.GetEnvironmentVariable("PROGRAMFILES") + @" (x86)\Team MediaPortal\MediaPortal TV server";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _TV_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput("New %PROGRAMFILES% (x86) path " + _TV_PROGRAM_FOLDER + " found for _TV_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "C:\Program Files" - get _TV_PROGRAM_FOLDER
                else if (File.Exists(@"C:\Program Files\Team MediaPortal\MediaPortal TV server\TvService.exe") == true)
                {
                    string path = @"C:\Program Files\Team MediaPortal\MediaPortal TV server";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _TV_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput( @"New C:\Program Files path " + _TV_PROGRAM_FOLDER + " found for _TV_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "C:\Program Files (x86)" - get _TV_PROGRAM_FOLDER
                else if (File.Exists(@"C:\Program Files (x86)\Team MediaPortal\MediaPortal TV server\TvService.exe") == true)
                {
                    string path = @"C:\Program Files (x86)\Team MediaPortal\MediaPortal TV server";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _TV_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput(@"New C:\Program Files (x86) path " + _TV_PROGRAM_FOLDER + " found for _TV_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "C:\Programme" - get _TV_PROGRAM_FOLDER
                else if (File.Exists(@"C:\Programme\Team MediaPortal\MediaPortal TV server\TvService.exe") == true)
                {
                    string path = @"C:\Programme\Team MediaPortal\MediaPortal TV server";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _TV_PROGRAM_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput( @"New C:\Programme path " + _TV_PROGRAM_FOLDER + " found for _TV_PROGRAM_FOLDER" + "\n");
                            }
                        }
                    }
                }

                if ((_DEBUG == true) && (_TV_PROGRAM_FOLDER == "NOT_DEFINED"))
                {
                    textoutput( "_TV_PROGRAM_FOLDER was not found" + "\n");
                }

            }
            catch (Exception ee)
            {
                textoutput( "Failed to autodetect _TV_PROGRAM_FOLDER - Exception message is \n" + ee.Message + "\n");
            }



            //*********************************************
            //try autodetect MediaPortal Application data
            //*********************************************
            try
            {
                //get environment variables
                string ALLUSERSPROFILE = Environment.GetEnvironmentVariable("ALLUSERSPROFILE");
                string PROGRAMDATA = Environment.GetEnvironmentVariable("PROGRAMDATA");
                string APPDATA = Environment.GetEnvironmentVariable("APPDATA");
                string[] array = APPDATA.Split('\\');
                string XP_PROGRAMDATA = ALLUSERSPROFILE + "\\" + array[array.Length-1];

                //try to find "%ALLUSERSPROFILE%" for VISTA - get _MP_USER_FOLDER
                if (Directory.Exists(ALLUSERSPROFILE + @"\Team MediaPortal\MediaPortal") == true)
                {
                    if (File.Exists(ALLUSERSPROFILE + @"\Team MediaPortal\MediaPortal\Mediaportal.exe") == false)
                    {
                        string path = ALLUSERSPROFILE + @"\Team MediaPortal\MediaPortal";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _MP_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput( "New %ALLUSERSPROFILE% path " + _MP_USER_FOLDER + " found for _MP_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find "%ALLUSERSPROFILE%\Anwendungsdaten" for XP German - get _MP_USER_FOLDER
                else if (Directory.Exists(ALLUSERSPROFILE + @"\Anwendungsdaten\Team MediaPortal\MediaPortal") == true)
                {
                    if (File.Exists(ALLUSERSPROFILE + @"\Anwendungsdaten\Team MediaPortal\MediaPortal\Mediaportal.exe") == false)
                    {
                        string path = ALLUSERSPROFILE + @"\Anwendungsdaten\Team MediaPortal\MediaPortal";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _MP_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput( @"New %ALLUSERSPROFILE%\Anwendungsdaten path " + _MP_USER_FOLDER + " found for _MP_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find "%ALLUSERSPROFILE%\Application Data" for XP English - get _MP_USER_FOLDER
                else if (Directory.Exists(ALLUSERSPROFILE + @"\Application Data\Team MediaPortal\MediaPortal") == true)
                {
                    if (File.Exists(ALLUSERSPROFILE + @"\Application Data\Team MediaPortal\MediaPortal\Mediaportal.exe") == false)
                    {
                        string path = ALLUSERSPROFILE + @"\Application Data\Team MediaPortal\MediaPortal";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _MP_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput( @"New %ALLUSERSPROFILE%\Application Data path " + _MP_USER_FOLDER + " found for _MP_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find "%APPDATA%" for XP - get _MP_USER_FOLDER
                else if (Directory.Exists(APPDATA + @"\Team MediaPortal\MediaPortal") == true)
                {
                    if (File.Exists(APPDATA + @"\Team MediaPortal\MediaPortal\Mediaportal.exe") == false)
                    {
                        string path = APPDATA + @"\Team MediaPortal\MediaPortal";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _MP_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput("New %APPDATA% path " + _MP_USER_FOLDER + " found for _MP_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find "%XP_PROGRAMDATA%" for XP - get _MP_USER_FOLDER
                else if (Directory.Exists(XP_PROGRAMDATA + @"\Team MediaPortal\MediaPortal") == true)
                {
                    if (File.Exists(XP_PROGRAMDATA + @"\Team MediaPortal\MediaPortal\Mediaportal.exe") == false)
                    {
                        string path = XP_PROGRAMDATA + @"\Team MediaPortal\MediaPortal";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _MP_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput("New %XP_PROGRAMDATA% path " + _MP_USER_FOLDER + " found for _MP_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find "%PROGRAMDATA%" for VISTA - get _MP_USER_FOLDER
                else if (Directory.Exists(PROGRAMDATA + @"\Team MediaPortal\MediaPortal") == true)
                {
                    if (File.Exists(PROGRAMDATA + @"\Team MediaPortal\MediaPortal\Mediaportal.exe") == false)
                    {
                        string path = PROGRAMDATA + @"\Team MediaPortal\MediaPortal";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _MP_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput("New %PROGRAMDATA% path " + _MP_USER_FOLDER + " found for _MP_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                else 
                {

                    //try registry
                    try
                    {
                        RegistryKey regkey = Registry.LocalMachine;
                        RegistryKey subkey = regkey.OpenSubKey(@"Software\Team MediaPortal\MediaPortal");
                        string path = subkey.GetValue("ConfigDir").ToString();
                        regkey.Close();
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            if (path.StartsWith(_STARTSWITH) == true)
                            {
                                _MP_USER_FOLDER = path;
                                if (_DEBUG == true)
                                {
                                    textoutput( @"New registry path " + _MP_USER_FOLDER + " found for _MP_USER_FOLDER" + "\n");
                                }
                            }
                        }
                    }
                    catch (Exception ee)
                    {
                        textoutput( "Failed reading registry key ConfigDir - Exception message is \n" + ee.Message + "\n");
                    }
                }
                
                
                


                //get install paths from "MediaPortalDirs.xml" - got all absolute DIR_...
                GetMediaPortalDirs();

                //if MP_USER is not defined and a defined path exists from _DIR_Config then use this path for _MP_USER_FOLDER
                if ((_DIR_Config != _MP_USER_FOLDER) && (_MP_USER_FOLDER != "NOT_DEFINED"))
                {
                    string path = _DIR_Config;
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _MP_USER_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput( @"New _DIR_Config Data path " + _MP_USER_FOLDER + " found for _MP_USER_FOLDER" + "\n");
                        }
                    }

                }
                


                if ((_DEBUG == true) && (_MP_USER_FOLDER == "NOT_DEFINED"))
                {
                        textoutput( "_MP_USER_FOLDER was not found" + "\n");
                }
                
                

            }
            catch (Exception ee)
            {
                textoutput( "Failed to autodetect _MP_USER_FOLDER - Exception message is \n" + ee.Message + "\n");
            }






            //*********************************************
            //try autodetect TV Server Application data
            //*********************************************

            try
            {

                //get environment variables
                string ALLUSERSPROFILE = Environment.GetEnvironmentVariable("ALLUSERSPROFILE");
                string PROGRAMDATA = Environment.GetEnvironmentVariable("PROGRAMDATA");
                string APPDATA = Environment.GetEnvironmentVariable("APPDATA");
                string[] array = APPDATA.Split('\\');
                string XP_PROGRAMDATA = ALLUSERSPROFILE + "\\" + array[array.Length-1];
                


                //try to find "%ALLUSERSPROFILE%" for VISTA - get _TV_USER_FOLDER
                if (Directory.Exists(ALLUSERSPROFILE + @"\Team MediaPortal\MediaPortal TV server") == true)
                {
                    if (File.Exists(ALLUSERSPROFILE + @"\Team MediaPortal\MediaPortal TV server\TvService.exe") == false)// check for avoiding program directory
                    {
                        string path = ALLUSERSPROFILE + @"\Team MediaPortal\MediaPortal TV server";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _TV_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput( "New %ALLUSERSPROFILE% path " + _TV_USER_FOLDER + " found for _TV_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find "%ALLUSERSPROFILE%\Anwendungsdaten" for XP German - get _TV_USER_FOLDER
                else if (Directory.Exists(ALLUSERSPROFILE + @"\Anwendungsdaten\Team MediaPortal\MediaPortal TV server") == true)
                {
                    if (File.Exists(ALLUSERSPROFILE + @"\Anwendungsdaten\Team MediaPortal\MediaPortal TV server\TvService.exe") == false)
                    {
                        string path = ALLUSERSPROFILE + @"\Anwendungsdaten\Team MediaPortal\MediaPortal TV server";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _TV_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput( @"New %ALLUSERSPROFILE%\Anwendungsdaten path " + _TV_USER_FOLDER + " found for _TV_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find "%ALLUSERSPROFILE%\Application Data" for XP English - get _TV_USER_FOLDER
                else if (Directory.Exists(ALLUSERSPROFILE + @"\Application Data\Team MediaPortal\MediaPortal TV server") == true)
                {
                    if (File.Exists(ALLUSERSPROFILE + @"\Application Data\Team MediaPortal\MediaPortal TV server\TvService.exe") == false)
                    {
                        string path = ALLUSERSPROFILE + @"\Application Data\Team MediaPortal\MediaPortal TV server";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _TV_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput( @"New %ALLUSERSPROFILE%\Application Data path " + _TV_USER_FOLDER + " found for _TV_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find  "%APPDATA%" for XP - get _TV_USER_FOLDER
                else if (Directory.Exists(APPDATA + @"\Team MediaPortal\MediaPortal TV server") == true)
                {
                    if (File.Exists(APPDATA + @"\Team MediaPortal\MediaPortal TV server\TvService.exe") == false)
                    {
                        string path = APPDATA + @"\Team MediaPortal\MediaPortal TV server";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _TV_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput("New %APPDATA% path " + _TV_USER_FOLDER + " found for _TV_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find  "%XP_PROGRAMDATA%" for XP - get _TV_USER_FOLDER
                else if (Directory.Exists(XP_PROGRAMDATA + @"\Team MediaPortal\MediaPortal TV server") == true)
                {
                    if (File.Exists(XP_PROGRAMDATA + @"\Team MediaPortal\MediaPortal TV server\TvService.exe") == false)
                    {
                        string path = XP_PROGRAMDATA + @"\Team MediaPortal\MediaPortal TV server";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _TV_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput("New %XP_PROGRAMDATA% path " + _TV_USER_FOLDER + " found for _TV_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find  "%PROGRAMDATA%" for VISTA - get _TV_USER_FOLDER
                else if (Directory.Exists(PROGRAMDATA + @"\Team MediaPortal\MediaPortal TV server") == true)
                {
                    if (File.Exists(PROGRAMDATA + @"\Team MediaPortal\MediaPortal TV server\TvService.exe") == false)
                    {
                        string path = PROGRAMDATA + @"\Team MediaPortal\MediaPortal TV server";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _TV_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput("New %PROGRAMDATA% path " + _TV_USER_FOLDER + " found for _TV_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }

                //try to find folder by derivation of MediaPortal User folder
                else if (Directory.Exists(_MP_USER_FOLDER + @" TV server") == true)//added name to MediaPortal:  MediaPortal TV server
                {
                    if (File.Exists(_MP_USER_FOLDER + @" TV server\TvService.exe") == false)
                    {
                        string path = _MP_USER_FOLDER + @" TV server";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _TV_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput( @"New derivative MP config Data path " + _TV_USER_FOLDER + " found for _TV_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }


                if ((_DEBUG == true) && (_TV_USER_FOLDER == "NOT_DEFINED"))
                {
                    textoutput( "_TV_USER_FOLDER was not found" + "\n");
                }
            }
            catch (Exception ee)
            {
                textoutput( "Failed to autodetect _TV_USER_FOLDER - Exception message is \n" + ee.Message + "\n");
            }

        }


        public void GetInstallPathsMP2()
        {
            

            //************************************
            //try autodetect MP2-Client.exe
            //************************************
            try
            {

                //start with current directory
                string thisdir = System.IO.Directory.GetCurrentDirectory();

                //try to find "Team MediaPortal" folder - get _MP2_PROGRAM_FOLDER
                string testpath = "NOT_DEFINED";
                string testfile = "NOT_DEFINED";
                int pos = 0;
                if (thisdir.Contains(@"\Team MediaPortal\") == true)
                {
                    //try to find "MediaPortal\Mediaportal.exe" - get _MP_PROGRAM_FOLDER
                    pos = thisdir.IndexOf(@"\Team MediaPortal\");
                    testpath = thisdir.Substring(0, pos) + @"\Team MediaPortal\MP2-Client";
                    testfile = thisdir.Substring(0, pos) + @"\Team MediaPortal\MP2-Client\MP2-Client.exe";
                }

                if (File.Exists(testfile) == true)
                {
                    if (testpath.StartsWith(_STARTSWITH) == true)
                    {
                        _MP2_PROGRAM_FOLDER = testpath;
                        if (_DEBUG == true)
                        {
                            textoutput("New thisdir path " + _MP2_PROGRAM_FOLDER + " found for _MP2_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "%PROGRAMFILES%" - get _MP2_PROGRAM_FOLDER
                else if (File.Exists(Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Team MediaPortal\MP2-Client\MP2-Client.exe") == true)
                {
                    string path = Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Team MediaPortal\MP2-Client";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _MP2_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput("New %PROGRAMFILES% path " + _MP2_PROGRAM_FOLDER + " found for _MP2_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "%PROGRAMFILES% (x86)" - get _MP2_PROGRAM_FOLDER
                else if (File.Exists(Environment.GetEnvironmentVariable("PROGRAMFILES") + @" (x86)\Team MediaPortal\MP2-Client\MP2-Client.exe") == true)
                {
                    string path = Environment.GetEnvironmentVariable("PROGRAMFILES") + @" (x86)\Team MediaPortal\MP2-Client";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _MP2_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput("New %PROGRAMFILES%  (x86) path " + _MP2_PROGRAM_FOLDER + " found for _MP2_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "C:\Program Files" - get _MP2_PROGRAM_FOLDER
                else if (File.Exists(@"C:\Program Files\Team MediaPortal\MP2-Client\MP2-Client.exe") == true)
                {
                    string path = @"C:\Program Files\Team MediaPortal\MP2-Client";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _MP2_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput(@"New C:\Program Files path " + _MP2_PROGRAM_FOLDER + " found for _MP2_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "C:\Program Files (x86)" - get _MP2_PROGRAM_FOLDER
                else if (File.Exists(@"C:\Program Files (x86)\Team MediaPortal\MP2-Client") == true)
                {
                    string path = @"C:\Program Files (x86)\Team MediaPortal\MP2-Client";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _MP2_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput(@"New C:\Program Files (x86) path " + _MP2_PROGRAM_FOLDER + " found for _MP2_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "C:\Programme" - get _MP2_PROGRAM_FOLDER
                else if (File.Exists(@"C:\Programme\Team MediaPortal\MP2-Client\MP2-Client.exe") == true)
                {
                    string path = @"C:\Programme\Team MediaPortal\MP2-Client";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _MP2_PROGRAM_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput(@"New C:\Programme path " + _MP2_PROGRAM_FOLDER + " found for _MP2_PROGRAM_FOLDER" + "\n");
                            }
                        }
                    }
                }
                else
                {
                    //registry
                    try
                    {
                        RegistryKey regkey = Registry.LocalMachine;
                        RegistryKey subkey = regkey.OpenSubKey(@"Software\Team MediaPortal\MP2-Client");
                        string path = subkey.GetValue("ApplicationDir").ToString();
                        regkey.Close();
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            if (path.StartsWith(_STARTSWITH) == true)
                            {
                                _MP2_PROGRAM_FOLDER = path;
                                if (_DEBUG == true)
                                {
                                    textoutput(@"New registry path " + _MP2_PROGRAM_FOLDER + " found for _MP2_PROGRAM_FOLDER" + "\n");
                                }
                            }
                        }
                    }
                    catch (Exception ee)
                    {
                        textoutput("Failed reading registry key ApplicationDir - Exception message is \n" + ee.Message + "\n");
                    }
                }


                if ((_DEBUG == true) && (_MP2_PROGRAM_FOLDER == "NOT_DEFINED"))
                {
                    textoutput("_MP2_PROGRAM_FOLDER was not found" + "\n");
                }


            }
            catch (Exception ee)
            {
                textoutput("Failed to autodetect _MP_PROGRAM_FOLDER - Exception message is \n" + ee.Message + "\n");
            }



            //************************************
            //try autodetect MP2-Server.exe
            //************************************
            try
            {

                //start with current directory
                string thisdir = System.IO.Directory.GetCurrentDirectory();

                //try to find "Team MediaPortal" folder - get _MP2_PROGRAM_FOLDER
                string testpath = "NOT_DEFINED";
                string testfile = "NOT_DEFINED";
                string mediaportaltestpath = "NOT_DEFINED";
                string mediaportaltestfile = "NOT_DEFINED";

                int pos = 0;
                if (thisdir.Contains(@"\Team MediaPortal\") == true)
                {
                    //try to find "MP2-Server\MP2-Server.exe" - get _SV2_PROGRAM_FOLDER
                    pos = thisdir.IndexOf(@"\Team MediaPortal\");
                    testpath = thisdir.Substring(0, pos) + @"\Team MediaPortal\MP2-Server";
                    testfile = thisdir.Substring(0, pos) + @"\Team MediaPortal\MP2-Server\MP2-Server.exe";
                }

                if (_MP2_PROGRAM_FOLDER != "NOT_DEFINED")
                {
                    string testserverpath = _MP2_PROGRAM_FOLDER;
                    pos = testserverpath.IndexOf(@"\Team MediaPortal\");
                    mediaportaltestpath = testserverpath.Substring(0, pos) + @"\Team MediaPortal\MP2-Server";
                    mediaportaltestfile = testserverpath.Substring(0, pos) + @"\Team MediaPortal\MP2-Server\MP2-Server.exe";
                }

                if (File.Exists(testfile) == true)
                {
                    if (testpath.StartsWith(_STARTSWITH) == true)
                    {
                        _SV2_PROGRAM_FOLDER = testpath;
                        if (_DEBUG == true)
                        {
                            textoutput("New thisdir path " + _SV2_PROGRAM_FOLDER + " found for _SV2_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                else if (File.Exists(mediaportaltestfile) == true)
                {
                    if (testpath.StartsWith(_STARTSWITH) == true)
                    {
                        _SV2_PROGRAM_FOLDER = mediaportaltestpath;
                        if (_DEBUG == true)
                        {
                            textoutput("New mediaportal tv path " + _SV2_PROGRAM_FOLDER + " found for _SV2_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "%PROGRAMFILES%" - get _SV2_PROGRAM_FOLDER
                else if (File.Exists(Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Team MediaPortal\MP2-Server\MP2-Server.exe") == true)
                {
                    string path = Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Team MediaPortal\MP2-Server";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _SV2_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput("New %PROGRAMFILES% path " + _SV2_PROGRAM_FOLDER + " found for _SV2_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "%PROGRAMFILES% (x86)" - get _SV2_PROGRAM_FOLDER
                else if (File.Exists(Environment.GetEnvironmentVariable("PROGRAMFILES") + @" (x86)\Team MediaPortal\MP2-Server\MP2-Server.exe") == true)
                {
                    string path = Environment.GetEnvironmentVariable("PROGRAMFILES") + @" (x86)\Team MediaPortal\MP2-Server";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _SV2_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput("New %PROGRAMFILES% (x86) path " + _SV2_PROGRAM_FOLDER + " found for _SV2_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "C:\Program Files" - get _SV2_PROGRAM_FOLDER
                else if (File.Exists(@"C:\Program Files\Team MediaPortal\MP2-Server\MP2-Server.exe") == true)
                {
                    string path = @"C:\Program Files\Team MediaPortal\MP2-Server";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _SV2_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput(@"New C:\Program Files path " + _SV2_PROGRAM_FOLDER + " found for _SV2_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "C:\Program Files (x86)" - get _SV2_PROGRAM_FOLDER
                else if (File.Exists(@"C:\Program Files (x86)\Team MediaPortal\MP2-Server\MP2-Server.exe") == true)
                {
                    string path = @"C:\Program Files (x86)\Team MediaPortal\MP2-Server";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _SV2_PROGRAM_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput(@"New C:\Program Files (x86) path " + _SV2_PROGRAM_FOLDER + " found for _SV2_PROGRAM_FOLDER" + "\n");
                        }
                    }
                }
                //try to find "C:\Programme" - get _SV2_PROGRAM_FOLDER
                else if (File.Exists(@"C:\Programme\Team MediaPortal\MP2-Server\MP2-Server.exe") == true)
                {
                    string path = @"C:\Programme\Team MediaPortal\MP2-Server";
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _SV2_PROGRAM_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput(@"New C:\Programme path " + _SV2_PROGRAM_FOLDER + " found for _SV2_PROGRAM_FOLDER" + "\n");
                            }
                        }
                    }
                }

                if ((_DEBUG == true) && (_SV2_PROGRAM_FOLDER == "NOT_DEFINED"))
                {
                    textoutput("_SV2_PROGRAM_FOLDER was not found" + "\n");
                }

            }
            catch (Exception ee)
            {
                textoutput("Failed to autodetect _SV2_PROGRAM_FOLDER - Exception message is \n" + ee.Message + "\n");
            }



            //*********************************************
            //try autodetect MediaPortal2 Client Application data
            //*********************************************
            try
            {
                //get environment variables
                string ALLUSERSPROFILE = Environment.GetEnvironmentVariable("ALLUSERSPROFILE");
                string PROGRAMDATA = Environment.GetEnvironmentVariable("PROGRAMDATA");
                string APPDATA = Environment.GetEnvironmentVariable("APPDATA");
                string[] array = APPDATA.Split('\\');
                string XP_PROGRAMDATA = ALLUSERSPROFILE + "\\" + array[array.Length - 1];

                //try to find "%ALLUSERSPROFILE%" for VISTA - get _MP2_USER_FOLDER
                if (Directory.Exists(ALLUSERSPROFILE + @"\Team MediaPortal\MP2-Client") == true)
                {
                    if (File.Exists(ALLUSERSPROFILE + @"\Team MediaPortal\MP2-Client\MP2-Client.exe") == false)
                    {
                        string path = ALLUSERSPROFILE + @"\Team MediaPortal\MP2-Client";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _MP2_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput("New %ALLUSERSPROFILE% path " + _MP2_USER_FOLDER + " found for _MP2_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find "%ALLUSERSPROFILE%\Anwendungsdaten" for XP German - get _MP2_USER_FOLDER
                else if (Directory.Exists(ALLUSERSPROFILE + @"\Anwendungsdaten\Team MediaPortal\MP2-Client") == true)
                {
                    if (File.Exists(ALLUSERSPROFILE + @"\Anwendungsdaten\Team MediaPortal\MP2-Client\MP2-Client.exe") == false)
                    {
                        string path = ALLUSERSPROFILE + @"\Anwendungsdaten\Team MediaPortal\MP2-Client";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _MP2_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput(@"New %ALLUSERSPROFILE%\Anwendungsdaten path " + _MP2_USER_FOLDER + " found for _MP2_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find "%ALLUSERSPROFILE%\Application Data" for XP English - get _MP2_USER_FOLDER
                else if (Directory.Exists(ALLUSERSPROFILE + @"\Application Data\Team MediaPortal\MP2-Client") == true)
                {
                    if (File.Exists(ALLUSERSPROFILE + @"\Application Data\Team MediaPortal\MP2-Client\MP2-Client.exe") == false)
                    {
                        string path = ALLUSERSPROFILE + @"\Application Data\Team MediaPortal\MP2-Client";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _MP2_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput(@"New %ALLUSERSPROFILE%\Application Data path " + _MP2_USER_FOLDER + " found for _MP2_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find "%APPDATA%" for XP - get _MP2_USER_FOLDER
                else if (Directory.Exists(APPDATA + @"\Team MediaPortal\MP2-Client") == true)
                {
                    if (File.Exists(APPDATA + @"\Team MediaPortal\MP2-Client\MP2-Client.exe") == false)
                    {
                        string path = APPDATA + @"\Team MediaPortal\MP2-Client";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _MP2_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput("New %APPDATA% path " + _MP2_USER_FOLDER + " found for _MP2_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find "%XP_PROGRAMDATA%" for XP - get _MP2_USER_FOLDER
                else if (Directory.Exists(XP_PROGRAMDATA + @"\Team MediaPortal\MP2-Client") == true)
                {
                    if (File.Exists(XP_PROGRAMDATA + @"\Team MediaPortal\MP2-Client\MP2-Client.exe") == false)
                    {
                        string path = XP_PROGRAMDATA + @"\Team MediaPortal\MP2-Client";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _MP2_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput("New %XP_PROGRAMDATA% path " + _MP2_USER_FOLDER + " found for _MP2_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find "%PROGRAMDATA%" for VISTA - get _MP_USER_FOLDER
                else if (Directory.Exists(PROGRAMDATA + @"\Team MediaPortal\MP2-Clientl") == true)
                {
                    if (File.Exists(PROGRAMDATA + @"\Team MediaPortal\MP2-Client\MP2-Client.exe") == false)
                    {
                        string path = PROGRAMDATA + @"\Team MediaPortal\MP2-Client";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            _MP2_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput("New %PROGRAMDATA% path " + _MP2_USER_FOLDER + " found for _MP2_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                else
                {

                    //try registry
                    try
                    {
                        RegistryKey regkey = Registry.LocalMachine;
                        RegistryKey subkey = regkey.OpenSubKey(@"Software\Team MediaPortal\MP2-Client");
                        string path = subkey.GetValue("ConfigDir").ToString();
                        regkey.Close();
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            if (path.StartsWith(_STARTSWITH) == true)
                            {
                                _MP2_USER_FOLDER = path;
                                if (_DEBUG == true)
                                {
                                    textoutput(@"New registry path " + _MP2_USER_FOLDER + " found for _MP2_USER_FOLDER" + "\n");
                                }
                            }
                        }
                    }
                    catch (Exception ee)
                    {
                        textoutput("Failed reading registry key ConfigDir - Exception message is \n" + ee.Message + "\n");
                    }
                }





                //get install paths from "Paths.xml" - got all absolute DIR_...
                GetMediaPortalDirsMP2();

                //if MP_USER is not defined and a defined path exists from _DIR_Config then use this path for _MP_USER_FOLDER
                if ((_DIR_MP2_Data != _MP2_USER_FOLDER) && (_MP2_USER_FOLDER != "NOT_DEFINED"))
                {
                    string path = _DIR_MP2_Data;
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        _MP2_USER_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput(@"New DIR_Data path " + _MP2_USER_FOLDER + " found for _MP2_USER_FOLDER" + "\n");
                        }
                    }

                }



                if ((_DEBUG == true) && (_MP2_USER_FOLDER == "NOT_DEFINED"))
                {
                    textoutput("_MP2_USER_FOLDER was not found" + "\n");
                }



            }
            catch (Exception ee)
            {
                textoutput("Failed to autodetect _MP2_USER_FOLDER - Exception message is \n" + ee.Message + "\n");
            }






            //*********************************************
            //try autodetect MP2 Server Application data
            //*********************************************

            try
            {

                //get environment variables
                string ALLUSERSPROFILE = Environment.GetEnvironmentVariable("ALLUSERSPROFILE");
                string PROGRAMDATA = Environment.GetEnvironmentVariable("PROGRAMDATA");
                string APPDATA = Environment.GetEnvironmentVariable("APPDATA");
                string[] array = APPDATA.Split('\\');
                string XP_PROGRAMDATA = ALLUSERSPROFILE + "\\" + array[array.Length - 1];

                //try to find "%ALLUSERSPROFILE%" for VISTA - get SV2_USER_FOLDER
                if (Directory.Exists(ALLUSERSPROFILE + @"\Team MediaPortal\MP2-Server") == true)
                {
                    if (File.Exists(ALLUSERSPROFILE + @"\Team MediaPortal\MP2-Server\MP2-Server.exe") == false)// check for avoiding program directory
                    {
                        string path = ALLUSERSPROFILE + @"\Team MediaPortal\MP2-Server";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            SV2_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput("New %ALLUSERSPROFILE% path " + SV2_USER_FOLDER + " found for SV2_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find "%ALLUSERSPROFILE%\Anwendungsdaten" for XP German - get SV2_USER_FOLDER
                else if (Directory.Exists(ALLUSERSPROFILE + @"\Anwendungsdaten\Team MediaPortal\MP2-Server") == true)
                {
                    if (File.Exists(ALLUSERSPROFILE + @"\Anwendungsdaten\Team MediaPortal\MP2-Server\MP2-Server.exe") == false)
                    {
                        string path = ALLUSERSPROFILE + @"\Anwendungsdaten\Team MediaPortal\MP2-Server";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            SV2_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput(@"New %ALLUSERSPROFILE%\Anwendungsdaten path " + SV2_USER_FOLDER + " found for SV2_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find "%ALLUSERSPROFILE%\Application Data" for XP English - get SV2_USER_FOLDER
                else if (Directory.Exists(ALLUSERSPROFILE + @"\Application Data\Team MediaPortal\MP2-Server") == true)
                {
                    if (File.Exists(ALLUSERSPROFILE + @"\Application Data\Team MediaPortal\MP2-Server\MP2-Server.exe") == false)
                    {
                        string path = ALLUSERSPROFILE + @"\Application Data\Team MediaPortal\MP2-Server";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            SV2_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput(@"New %ALLUSERSPROFILE%\Application Data path " + SV2_USER_FOLDER + " found for _TV_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find  "%APPDATA%" for XP - get SV2_USER_FOLDER
                else if (Directory.Exists(APPDATA + @"\Team MediaPortal\MP2-Server") == true)
                {
                    if (File.Exists(APPDATA + @"\Team MediaPortal\MP2-Server\MP2-Server.exe") == false)
                    {
                        string path = APPDATA + @"\Team MediaPortal\MP2-Server";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            SV2_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput("New %APPDATA% path " + SV2_USER_FOLDER + " found for SV2_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find  "%XP_PROGRAMDATA%" for XP - get SV2_USER_FOLDER
                else if (Directory.Exists(XP_PROGRAMDATA + @"\Team MediaPortal\MP2-Server") == true)
                {
                    if (File.Exists(XP_PROGRAMDATA + @"\Team MediaPortal\MP2-Server\MP2-Server.exe") == false)
                    {
                        string path = XP_PROGRAMDATA + @"\Team MediaPortal\MP2-Server";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            SV2_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput("New %XP_PROGRAMDATA% path " + SV2_USER_FOLDER + " found for SV2_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }
                //try to find  "%PROGRAMDATA%" for VISTA - get SV2_USER_FOLDER
                else if (Directory.Exists(PROGRAMDATA + @"\Team MediaPortal\MP2-Server") == true)
                {
                    if (File.Exists(PROGRAMDATA + @"\Team MediaPortal\MP2-Server\MP2-Server.exe") == false)
                    {
                        string path = PROGRAMDATA + @"\Team MediaPortal\MP2-Server";
                        if (path.StartsWith(_STARTSWITH) == true)
                        {
                            SV2_USER_FOLDER = path;
                            if (_DEBUG == true)
                            {
                                textoutput("New %PROGRAMDATA% path " + SV2_USER_FOLDER + " found for SV2_USER_FOLDER" + "\n");
                            }
                        }
                    }
                }


                //get install paths from "Paths.xml" - got all absolute DIR_...
                GetMediaPortalDirsMP2();

                //if MP_USER is not defined and a defined path exists from _DIR_Config then use this path for _MP_USER_FOLDER
                if ((_DIR_SV2_Data != SV2_USER_FOLDER) && (SV2_USER_FOLDER != "NOT_DEFINED"))
                {
                    string path = _DIR_SV2_Data;
                    if (path.StartsWith(_STARTSWITH) == true)
                    {
                        SV2_USER_FOLDER = path;
                        if (_DEBUG == true)
                        {
                            textoutput(@"New DIR_Data path " + SV2_USER_FOLDER + " found for SV2_USER_FOLDER" + "\n");
                        }
                    }

                }

                if ((_DEBUG == true) && (SV2_USER_FOLDER == "NOT_DEFINED"))
                {
                    textoutput("SV2_USER_FOLDER was not found" + "\n");
                }
            }
            catch (Exception ee)
            {
                textoutput("Failed to autodetect SV2_USER_FOLDER - Exception message is \n" + ee.Message + "\n");
            }


            

            //************************************
            //try autodetect MP2-Native TV Server
            //************************************
            try
            {
                GetMediaPortalDirsMP2();
                
                if (Directory.Exists(DIR_SV2_Plugins + @"\SlimTv.Service"))
                {
                    TV2_PROGRAM_FOLDER = DIR_SV2_Plugins + @"\SlimTv.Service";
                }
            }
            catch (Exception ee)
            {
                textoutput("Failed to autodetect TV2_PROGRAM_FOLDER - Exception message is \n" + ee.Message + "\n");
            }
            
            //*****************************************************
            //try autodetect MP2 Native TV Server Application data
            //*****************************************************

            try
            {
                if (Directory.Exists(SV2_USER_FOLDER + @"\SlimTVCore"))
                {
                    TV2_USER_FOLDER = SV2_USER_FOLDER + @"\SlimTVCore";
                }
            }
            catch (Exception ee)
            {
                textoutput("Failed to autodetect TV2_USER_FOLDER - Exception message is \n" + ee.Message + "\n");
            }

        }        


        public string ask_MP_PROGRAM()
        {

            if (File.Exists(_MP_PROGRAM_FOLDER + @"\Mediaportal.exe") == false)
                GetInstallPaths();

            if (File.Exists(_MP_PROGRAM_FOLDER + @"\Mediaportal.exe") == false)
            {
                ask_MP_PROGRAM_ALWAYS();              
            }
            return _MP_PROGRAM_FOLDER;
        }

        public string ask_MP_PROGRAM_ALWAYS()
        {
            string path = "";
            string foldertype = "_MP_PROGRAM_FOLDER";
            // let user select file
            OpenFileDialog openFileDialogMPPathProgram = new OpenFileDialog();
            openFileDialogMPPathProgram.Filter = "Mediaportal.exe |Mediaportal.exe";
            //openFileDialogMPPathProgram.InitialDirectory = "C:\\";
            openFileDialogMPPathProgram.Title = @"Select the File Mediaportal.exe in ..\Team MediaPortal\MediaPortal";
            if (openFileDialogMPPathProgram.ShowDialog() == DialogResult.OK)
                path = openFileDialogMPPathProgram.FileName;

            if ((File.Exists(path) == true) && (path.Length > 5))
            {
                // extract program folder ..\\Team MediaPortal\\MediaPortal TV Server
                try
                {
                    FileInfo myfileinfo = new FileInfo(path);
                    path = myfileinfo.DirectoryName;
                }
                catch (Exception exc)
                {
                    textoutput("Error: \n" + exc.Message + "\n\n");
                }

                if (path.StartsWith(_STARTSWITH) == false)
                {
                    textoutput("Error: Selected folder\n" + path + "  \n does not start with" + _STARTSWITH + "\n");
                    path = "NOT_DEFINED";
                }
                else
                {
                    _MP_PROGRAM_FOLDER = path;
                }
            }
            else
            {
                textoutput("Error: File Path Folder for " + foldertype + "  was not found in the file dialog");
                path = "NOT_DEFINED";
            }

            if (_DEBUG == true)
            {
                textoutput("New path " + path + " found for " + foldertype);
            }                
                
            return _MP_PROGRAM_FOLDER;
        }

        public string ask_MP_USER()
        {
            if (_MP_USER_FOLDER == "NOT_DEFINED")
                GetInstallPaths();

            if (_MP_USER_FOLDER == "NOT_DEFINED")
            {
                ask_MP_USER_ALWAYS();               
            }
            return _MP_USER_FOLDER;
        }

        public string ask_MP_USER_ALWAYS()
        {
            string path = "";
            string foldertype = "_MP_USER_FOLDER";
            // let user select file
            OpenFileDialog openFileDialogMPPathUser = new OpenFileDialog();
            openFileDialogMPPathUser.Filter = "*.* |*.*";
            //openFileDialogMPPathUser.InitialDirectory = "C:\\";
            openFileDialogMPPathUser.Title = @"Select any file in the user config folder in ..\Team MediaPortal\MediaPortal";
            if (openFileDialogMPPathUser.ShowDialog() == DialogResult.OK)
                path = openFileDialogMPPathUser.FileName;

            if ((File.Exists(path) == true) && (path.Length > 5))
            {
                // extract user folder ..\\Team MediaPortal\\MediaPortal

                try
                {
                    FileInfo myfileinfo = new FileInfo(path);
                    path = myfileinfo.DirectoryName;
                }
                catch (Exception exc)
                {
                    textoutput("Error: \n" + exc.Message);
                }


                if (path.StartsWith(_STARTSWITH) == false)
                {
                    textoutput("Error: Selected folder\n" + path + "  \n does not start with" + _STARTSWITH + "\n");
                    path = "NOT_DEFINED";
                }
                else
                {
                    _MP_USER_FOLDER = path;
                }
            }
            else
            {

                textoutput("Error: File Path Folder for " + foldertype + "  was not found in the file dialog");
                path = "NOT_DEFINED";
            }

            if (_DEBUG == true)
            {
                textoutput("New path " + path + " found for " + foldertype);
            }
            return _MP_USER_FOLDER;
        }

        public string ask_TV_PROGRAM()
        {
            if (File.Exists(_TV_PROGRAM_FOLDER + @"\TvService.exe") == false)
                GetInstallPaths();

            if (File.Exists(_TV_PROGRAM_FOLDER + @"\TvService.exe") == false)
            {
                ask_TV_PROGRAM_ALWAYS();
            }
            return _TV_PROGRAM_FOLDER;
        }

        public string ask_TV_PROGRAM_ALWAYS()
        {
            string path = "";
            string foldertype = "_TV_PROGRAM_FOLDER";
            // let user select file

            OpenFileDialog openFileDialogPathProgram = new OpenFileDialog();
            openFileDialogPathProgram.Filter = "TvService.exe |TvService.exe";
            //openFileDialogPathProgram.InitialDirectory = "C:\\";
            openFileDialogPathProgram.Title = @"Select the File TvService.exe in ..\Team MediaPortal\MediaPortal TV Server";
            if (openFileDialogPathProgram.ShowDialog() == DialogResult.OK)
                path = openFileDialogPathProgram.FileName;

            if ((File.Exists(path) == true) && (path.Length > 14))
            {
                // extract program folder ..\\Team MediaPortal\\MediaPortal TV Server
                try
                {
                    FileInfo myfileinfo = new FileInfo(path);
                    path = myfileinfo.DirectoryName;
                }
                catch (Exception exc)
                {
                    textoutput("Error: \n" + exc.Message);
                }

                if (path.StartsWith(_STARTSWITH) == false)
                {
                    textoutput("Error: Selected folder\n" + path + "  \n does not start with" + _STARTSWITH + "\n");
                    path = "NOT_DEFINED";
                }
                else
                {
                    _TV_PROGRAM_FOLDER = path;
                }
            }
            else
            {
                textoutput("Error: File Path Folder for " + foldertype + " was not found in the filedialog");
                path = "NOT_DEFINED";
            }

            if (_DEBUG == true)
            {
                textoutput("New path " + path + " found for " + foldertype);
            }
            return _TV_PROGRAM_FOLDER;
        }

        public string ask_TV_USER()
        {
            if (_TV_USER_FOLDER == "NOT_DEFINED")
                GetInstallPaths();

            if (_TV_USER_FOLDER == "NOT_DEFINED")
            {
                ask_TV_USER_ALWAYS();

            }
            return _TV_USER_FOLDER;
        }

        public string ask_TV_USER_ALWAYS()
        {

            string path = "";
            string foldertype = "_TV_USER_FOLDER";
            // let user select file
            OpenFileDialog openFileDialogPathUser = new OpenFileDialog();
            openFileDialogPathUser.Filter = "*.* |*.*";
            //openFileDialogPathUser.InitialDirectory = "C:\\";
            openFileDialogPathUser.Title = @"Select any file in the Tv user folder ..\Team MediaPortal\MediaPortal TV Server";
            if (openFileDialogPathUser.ShowDialog() == DialogResult.OK)
                path = openFileDialogPathUser.FileName;

            if ((File.Exists(path) == true) && (path.Length > 21))
            {
                // extract user folder ..\\Team MediaPortal\\MediaPortal TV Server

                try
                {
                    FileInfo myfileinfo = new FileInfo(path);
                    path = myfileinfo.DirectoryName;
                }
                catch (Exception exc)
                {
                    textoutput("Error: \n" + exc.Message);
                }

                if (path.StartsWith(_STARTSWITH) == false)
                {
                    textoutput("Error: Selected folder\n" + path + "  \n does not start with" + _STARTSWITH + "\n");
                    path = "NOT_DEFINED";
                }
                else
                {
                    _TV_USER_FOLDER = path;
                }
            }
            else
            {
                textoutput("Error: File Path Folder for " + foldertype + "  was not found in the file dialog");
                path = "NOT_DEFINED";
            }

            if (_DEBUG == true)
            {
                textoutput("New path " + path + " found for " + foldertype);
            }
            return _TV_USER_FOLDER;
        }




        public string ask_MP2_PROGRAM()
        {
            if (File.Exists(_MP2_PROGRAM_FOLDER + @"\MP2-Client.exe") == false)
                GetInstallPathsMP2();

            if (File.Exists(_MP2_PROGRAM_FOLDER + @"\MP2-Client.exe") == false)
            {
                ask_MP2_PROGRAM_ALWAYS();
            }
            return _MP2_PROGRAM_FOLDER;
        }

        public string ask_MP2_PROGRAM_ALWAYS()
        {
            string path = "";
            string foldertype = "_MP2_PROGRAM_FOLDER";
            // let user select file
            OpenFileDialog openFileDialogMPPathProgram = new OpenFileDialog();
            openFileDialogMPPathProgram.Filter = "MP2-Client.exe |MP2-Client.exe";
            //openFileDialogMPPathProgram.InitialDirectory = "C:\\";
            openFileDialogMPPathProgram.Title = @"Select the File MP2-Client.exe in ..\Team MediaPortal\MP2-Client";
            if (openFileDialogMPPathProgram.ShowDialog() == DialogResult.OK)
                path = openFileDialogMPPathProgram.FileName;

            if ((File.Exists(path) == true) && (path.Length > 5))
            {
                // extract program folder ..\\Team MediaPortal\\MediaPortal TV Server
                try
                {
                    FileInfo myfileinfo = new FileInfo(path);
                    path = myfileinfo.DirectoryName;
                }
                catch (Exception exc)
                {
                    textoutput("Error: \n" + exc.Message + "\n\n");
                }

                if (path.StartsWith(_STARTSWITH) == false)
                {
                    textoutput("Error: Selected folder\n" + path + "  \n does not start with" + _STARTSWITH + "\n");
                    path = "NOT_DEFINED";
                }
                else
                {
                    _MP2_PROGRAM_FOLDER = path;
                }
            }
            else
            {
                textoutput("Error: File Path Folder for " + foldertype + "  was not found in the file dialog");
                path = "NOT_DEFINED";
            }

            if (_DEBUG == true)
            {
                textoutput("New path " + path + " found for " + foldertype);
            }

            return _MP2_PROGRAM_FOLDER;
        }

        public string ask_MP2_USER()
        {
            if (_MP2_USER_FOLDER == "NOT_DEFINED")
                GetInstallPathsMP2();

            if (_MP2_USER_FOLDER == "NOT_DEFINED")
            {
                ask_MP2_USER_ALWAYS();
            }
            return _MP2_USER_FOLDER;
        }

        public string ask_MP2_USER_ALWAYS()
        {
            string path = "";
            string foldertype = "_MP2_USER_FOLDER";
            // let user select file
            OpenFileDialog openFileDialogMPPathUser = new OpenFileDialog();
            openFileDialogMPPathUser.Filter = "*.* |*.*";
            //openFileDialogMPPathUser.InitialDirectory = "C:\\";
            openFileDialogMPPathUser.Title = @"Select any file in the user config folder in ..\Team MediaPortal\MP2-Client";
            if (openFileDialogMPPathUser.ShowDialog() == DialogResult.OK)
                path = openFileDialogMPPathUser.FileName;

            if ((File.Exists(path) == true) && (path.Length > 5))
            {
                // extract user folder ..\\Team MediaPortal\\MP2-Client

                try
                {
                    FileInfo myfileinfo = new FileInfo(path);
                    path = myfileinfo.DirectoryName;
                }
                catch (Exception exc)
                {
                    textoutput("Error: \n" + exc.Message);
                }


                if (path.StartsWith(_STARTSWITH) == false)
                {
                    textoutput("Error: Selected folder\n" + path + "  \n does not start with" + _STARTSWITH + "\n");
                    path = "NOT_DEFINED";
                }
                else
                {
                    _MP2_USER_FOLDER = path;
                }
            }
            else
            {

                textoutput("Error: File Path Folder for " + foldertype + "  was not found in the file dialog");
                path = "NOT_DEFINED";
            }

            if (_DEBUG == true)
            {
                textoutput("New path " + path + " found for " + foldertype);
            }
            return _MP2_USER_FOLDER;
        }

        public string ask_SV2_PROGRAM()
        {
            if (File.Exists(_SV2_PROGRAM_FOLDER + @"\MP2-Server.exe") == false)
                GetInstallPathsMP2();

            if (File.Exists(_SV2_PROGRAM_FOLDER + @"\MP2-Server.exe") == false)
            {
                ask_SV2_PROGRAM_ALWAYS();
            }
            return _SV2_PROGRAM_FOLDER;
        }

        public string ask_SV2_PROGRAM_ALWAYS()
        {
            string path = "";
            string foldertype = "_SV2_PROGRAM_FOLDER";
            // let user select file

            OpenFileDialog openFileDialogPathProgram = new OpenFileDialog();
            openFileDialogPathProgram.Filter = "MP2-Server.exe |MP2-Server.exe";
            //openFileDialogPathProgram.InitialDirectory = "C:\\";
            openFileDialogPathProgram.Title = @"Select the File MP2-Server.exe in ..\Team MediaPortal\MP2-Server";
            if (openFileDialogPathProgram.ShowDialog() == DialogResult.OK)
                path = openFileDialogPathProgram.FileName;

            if ((File.Exists(path) == true) && (path.Length > 14))
            {
                // extract program folder ..\\Team MediaPortal\\MP2-Server
                try
                {
                    FileInfo myfileinfo = new FileInfo(path);
                    path = myfileinfo.DirectoryName;
                }
                catch (Exception exc)
                {
                    textoutput("Error: \n" + exc.Message);
                }

                if (path.StartsWith(_STARTSWITH) == false)
                {
                    textoutput("Error: Selected folder\n" + path + "  \n does not start with" + _STARTSWITH + "\n");
                    path = "NOT_DEFINED";
                }
                else
                {
                    _SV2_PROGRAM_FOLDER = path;
                }
            }
            else
            {
                textoutput("Error: File Path Folder for " + foldertype + " was not found in the filedialog");
                path = "NOT_DEFINED";
            }

            if (_DEBUG == true)
            {
                textoutput("New path " + path + " found for " + foldertype);
            }
            return _SV2_PROGRAM_FOLDER;
        }

        public string ask_SV2_USER()
        {
            if (SV2_USER_FOLDER == "NOT_DEFINED")
                GetInstallPathsMP2();

            if (SV2_USER_FOLDER == "NOT_DEFINED")
            {
                ask_SV2_USER_ALWAYS();

            }
            return SV2_USER_FOLDER;
        }

        public string ask_SV2_USER_ALWAYS()
        {

            string path = "";
            string foldertype = "SV2_USER_FOLDER";
            // let user select file
            OpenFileDialog openFileDialogPathUser = new OpenFileDialog();
            openFileDialogPathUser.Filter = "*.* |*.*";
            //openFileDialogPathUser.InitialDirectory = "C:\\";
            openFileDialogPathUser.Title = @"Select any file in the Tv user folder ..\Team MediaPortal\MP2-Server";
            if (openFileDialogPathUser.ShowDialog() == DialogResult.OK)
                path = openFileDialogPathUser.FileName;

            if ((File.Exists(path) == true) && (path.Length > 21))
            {
                // extract user folder ..\\Team MediaPortal\\MP2-Server

                try
                {
                    FileInfo myfileinfo = new FileInfo(path);
                    path = myfileinfo.DirectoryName;
                }
                catch (Exception exc)
                {
                    textoutput("Error: \n" + exc.Message);
                }

                if (path.StartsWith(_STARTSWITH) == false)
                {
                    textoutput("Error: Selected folder\n" + path + "  \n does not start with" + _STARTSWITH + "\n");
                    path = "NOT_DEFINED";
                }
                else
                {
                    SV2_USER_FOLDER = path;
                }
            }
            else
            {
                textoutput("Error: File Path Folder for " + foldertype + "  was not found in the file dialog");
                path = "NOT_DEFINED";
            }

            if (_DEBUG == true)
            {
                textoutput("New path " + path + " found for " + foldertype);
            }
            return SV2_USER_FOLDER;
        }


        public string ask_TV2_USER()
        {
            if (_TV2_USER_FOLDER == "NOT_DEFINED")
                GetInstallPathsMP2();

            if (_TV2_USER_FOLDER == "NOT_DEFINED")
            {
                ask_TV2_USER_ALWAYS();

            }
            return _TV2_USER_FOLDER;
        }

        public string ask_TV2_USER_ALWAYS()
        {

            string path = "";
            string foldertype = "_TV2_USER_FOLDER";
            // let user select file
            OpenFileDialog openFileDialogPathUser = new OpenFileDialog();
            openFileDialogPathUser.Filter = "*.* |*.*";
            //openFileDialogPathUser.InitialDirectory = "C:\\";
            openFileDialogPathUser.Title = @"Select any file in the Tv user folder ..\Team MediaPortal\MP2-Server\Plugins\SlimTv.Service";
            if (openFileDialogPathUser.ShowDialog() == DialogResult.OK)
                path = openFileDialogPathUser.FileName;

            if ((File.Exists(path) == true) && (path.Length > 21))
            {
                // extract user folder ..\\Team MediaPortal\\MediaPortal TV Server

                try
                {
                    FileInfo myfileinfo = new FileInfo(path);
                    path = myfileinfo.DirectoryName;
                }
                catch (Exception exc)
                {
                    textoutput("Error: \n" + exc.Message);
                }

                if (path.StartsWith(_STARTSWITH) == false)
                {
                    textoutput("Error: Selected folder\n" + path + "  \n does not start with" + _STARTSWITH + "\n");
                    path = "NOT_DEFINED";
                }
                else
                {
                    _TV2_USER_FOLDER = path;
                }
            }
            else
            {
                textoutput("Error: File Path Folder for " + foldertype + "  was not found in the file dialog");
                path = "NOT_DEFINED";
            }

            if (_DEBUG == true)
            {
                textoutput("New path " + path + " found for " + foldertype);
            }
            return _TV2_USER_FOLDER;
        }



        public void GetMediaPortalDirs()
        {
            
            //get default values for MediaPortalDirs.xml
            _DIR_Config = _MP_USER_FOLDER;
            _DIR_Plugins = _MP_PROGRAM_FOLDER+@"\plugins";
            _DIR_LOG = _MP_USER_FOLDER+@"\log";
            _DIR_CustomInputDevice = _MP_USER_FOLDER+@"\InputDeviceMappings";
            _DIR_CustomInputDefault = _MP_PROGRAM_FOLDER+@"\InputDeviceMappings\defaults";
            _DIR_Skin = _MP_PROGRAM_FOLDER + @"\skin";
            _DIR_Language = _MP_PROGRAM_FOLDER + @"\language";
            _DIR_Database = _MP_USER_FOLDER + @"\database";
            _DIR_Thumbs = _MP_USER_FOLDER + @"\thumbs";
            _DIR_Weather = _MP_PROGRAM_FOLDER + @"\weather";
            _DIR_Cache = _MP_USER_FOLDER + @"\cache";
            _DIR_BurnerSupport = _MP_PROGRAM_FOLDER + @"\Burner";
            _FILE_MediaPortalDirs = _MP_PROGRAM_FOLDER + @"\MediaPortalDirs.xml";

            //USERPROFILE_EXISTS = false;
            // read MediaPortalDirs.xml in User document directory
            if (File.Exists(Environment.GetEnvironmentVariable("APPDATA") + @"\Team MediaPortal\MediaPortalDirs.xml") == true)
            {
                _FILE_MediaPortalDirs = Environment.GetEnvironmentVariable("APPDATA") + @"\Team MediaPortal\MediaPortalDirs.xml";
                //USERPROFILE_EXISTS = true;
            }
            else if (File.Exists(Environment.GetEnvironmentVariable("USERPROFILE") + @"\Team MediaPortal\MediaPortalDirs.xml") == true)
            {
                _FILE_MediaPortalDirs = Environment.GetEnvironmentVariable("USERPROFILE") + @"\Team MediaPortal\MediaPortalDirs.xml";
                //USERPROFILE_EXISTS = true;
            }
            else if (File.Exists(_MP_PROGRAM_FOLDER + @"\MediaPortalDirs.xml") == true)
            {
                _FILE_MediaPortalDirs = _MP_PROGRAM_FOLDER + @"\MediaPortalDirs.xml";
                //USERPROFILE_EXISTS = true;
            }


            // read MediaPortalDirs.xml in MediaPortal program document directory


            if (File.Exists(_FILE_MediaPortalDirs) == true)
            {
                try
                {
                    StreamReader sfile = File.OpenText(_FILE_MediaPortalDirs);
                    String textline = null;

                    while ((textline = sfile.ReadLine()) != null)
                    {
                        InstallPathParser(ref _DIR_Config,"  <Dir id = \"Config\">",sfile,textline);

                        InstallPathParser(ref _DIR_Plugins,"  <Dir id = \"Plugins\">",sfile,textline);
                        if (_DIR_Plugins.ToLower() == _MP_PROGRAM_FOLDER.ToLower() + @"\plugins")  //special treatment of case sensitivity for standard installation
                        {
                            _DIR_Plugins=_MP_PROGRAM_FOLDER+ @"\plugins";
                        }

                        InstallPathParser(ref _DIR_LOG,"  <Dir id = \"Log\">",sfile,textline);
                        if (_DIR_LOG.ToLower() == _MP_USER_FOLDER.ToLower() + @"\log")  //special treatment of case sensitivity for standard installation
                        {
                            _DIR_LOG = _MP_USER_FOLDER + @"\log";
                        }

                        InstallPathParser(ref _DIR_CustomInputDevice,"  <Dir id = \"CustomInputDevice\">",sfile,textline);
                        if (_DIR_CustomInputDevice.ToLower() == _MP_USER_FOLDER.ToLower() + @"\inputdevicemappings")  //special treatment of case sensitivity for standard installation
                        {
                            _DIR_CustomInputDevice = _MP_USER_FOLDER + @"\InputDeviceMappings";
                        }

                        InstallPathParser(ref _DIR_CustomInputDefault,"  <Dir id = \"CustomInputDefault\">",sfile,textline);
                        if (_DIR_CustomInputDefault.ToLower() == _MP_PROGRAM_FOLDER.ToLower() + @"\inputdevicemappings\defaults")  //special treatment of case sensitivity for standard installation
                        {
                            _DIR_CustomInputDefault = _MP_PROGRAM_FOLDER + @"\InputDeviceMappings\defaults";
                        }
                        
                        InstallPathParser(ref _DIR_Skin,"  <Dir id = \"Skin\">",sfile,textline);
                        if (_DIR_Skin.ToLower() == _MP_PROGRAM_FOLDER.ToLower() + @"\skin")  //special treatment of case sensitivity for standard installation
                        {
                            _DIR_Skin = _MP_PROGRAM_FOLDER + @"\skin";
                        }

                        InstallPathParser(ref _DIR_Language,"  <Dir id = \"Language\">",sfile,textline);
                        if (_DIR_Language.ToLower() == _MP_PROGRAM_FOLDER.ToLower() + @"\language")  //special treatment of case sensitivity for standard installation
                        {
                            _DIR_Language = _MP_PROGRAM_FOLDER + @"\language";
                        }

                        InstallPathParser(ref _DIR_Database,"  <Dir id = \"Database\">",sfile,textline);
                        if (_DIR_Database.ToLower() == _MP_USER_FOLDER.ToLower() + @"\database")  //special treatment of case sensitivity for standard installation
                        {
                            _DIR_Database = _MP_USER_FOLDER + @"\database";
                        }

                        InstallPathParser(ref _DIR_Thumbs,"  <Dir id = \"Thumbs\">",sfile,textline);
                        if (_DIR_Thumbs.ToLower() == _MP_USER_FOLDER.ToLower() + @"\thumbs")  //special treatment of case sensitivity for standard installation
                        {
                            _DIR_Thumbs = _MP_USER_FOLDER + @"\thumbs";
                        }

                        InstallPathParser(ref _DIR_Weather,"  <Dir id = \"Weather\">",sfile,textline);
                        if (_DIR_Weather.ToLower() == _MP_PROGRAM_FOLDER.ToLower() + @"\weather")  //special treatment of case sensitivity for standard installation
                        {
                            _DIR_Weather = _MP_PROGRAM_FOLDER + @"\weather";
                        }

                        InstallPathParser(ref _DIR_Cache,"  <Dir id = \"Cache\">",sfile,textline);
                        if (_DIR_Cache.ToLower() == _MP_USER_FOLDER.ToLower() + @"\cache")  //special treatment of case sensitivity for standard installation
                        {
                            _DIR_Cache = _MP_USER_FOLDER + @"\cache";
                        }

                        InstallPathParser(ref _DIR_BurnerSupport,"  <Dir id = \"BurnerSupport\">",sfile,textline);
                        if (_DIR_BurnerSupport.ToLower() == _MP_PROGRAM_FOLDER.ToLower() + @"\burner")  //special treatment of case sensitivity for standard installation
                        {
                            _DIR_BurnerSupport = _MP_PROGRAM_FOLDER + @"\Burner";
                        }
                        
                    }
                    sfile.Close();
                }
                catch (Exception ee)
                {
                    textoutput("Error in parsing file  " + _FILE_MediaPortalDirs);
                    if (_DEBUG == true)
                        textoutput("Exception message is " + ee.Message);
                }

            }
            else
            {
                textoutput("Error: " + _MP_PROGRAM_FOLDER + @"\MediaPortalDirs.xml does not exist");
            }
        }


        public void GetMediaPortalDirsMP2()
        {
            //All paths from MP2Server
            _FILE_SV2_Paths = _SV2_PROGRAM_FOLDER + @"\Defaults\Paths.xml";
            _DIR_SV2_Data = SV2_USER_FOLDER;
            _DIR_SV2_LOG = SV2_USER_FOLDER + @"\Log";
            _DIR_SV2_Config = SV2_USER_FOLDER + @"\Config";
            _DIR_SV2_Database = SV2_USER_FOLDER;
            _DIR_SV2_Plugins = _SV2_PROGRAM_FOLDER + @"\Plugins";

            //All paths from MP2Client
            _FILE_MP2_Paths = _MP2_PROGRAM_FOLDER + @"\Defaults\Paths.xml";
            _DIR_MP2_Data = _MP2_USER_FOLDER;
            _DIR_MP2_LOG = _MP2_USER_FOLDER + @"\Log";
            _DIR_MP2_Config = _MP2_USER_FOLDER + @"\Config";
            _DIR_MP2_Plugins = _MP2_PROGRAM_FOLDER + @"\Plugins";

            //USERPROFILESV2_EXISTS = false;
            // read Paths.xml in User document directory
            if (File.Exists(Environment.GetEnvironmentVariable("APPDATA") + @"\Team MediaPortal\SV2-Client\Defaults\Paths.xml") == true)
            {
                _FILE_SV2_Paths = Environment.GetEnvironmentVariable("APPDATA") + @"\Team MediaPortal\SV2-Client\Defaults\Paths.xml";
                //USERPROFILESV2_EXISTS = true;
            }
            else if (File.Exists(Environment.GetEnvironmentVariable("USERPROFILE") + @"\Team MediaPortal\SV2-Client\Defaults\Paths.xml") == true)
            {
                _FILE_SV2_Paths = Environment.GetEnvironmentVariable("USERPROFILE") + @"\Team MediaPortal\SV2-Client\Defaults\Paths.xml";
                //USERPROFILESV2_EXISTS = true;
            }
            else if (File.Exists(_SV2_PROGRAM_FOLDER + @"\Defaults\Paths.xml") == true)
            {
                _FILE_SV2_Paths = _SV2_PROGRAM_FOLDER + @"\Defaults\Paths.xml";
                //USERPROFILESV2_EXISTS = true;
            }

            // read Paths.xml in MediaPortal Server  document directory
            if (File.Exists(_FILE_SV2_Paths) == true)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(_FILE_SV2_Paths);
                    XmlNodeList allpaths = doc.SelectNodes("/Paths/Path");
                    foreach (XmlNode mypath in allpaths)
                    {
                        string name = mypath.Attributes["name"].Value;
                        string value = mypath.Attributes["value"].Value;

                        value = value.Replace("<APPLICATION_ROOT>", _SV2_PROGRAM_FOLDER);
                        value = value.Replace("<DATA>", _DIR_SV2_Data);
                        string APPDATA = SV2_USER_FOLDER.Replace(@"\Team MediaPortal\MP2-Server","");
                        value = value.Replace("<COMMON_APPLICATION_DATA>",APPDATA);

                        if (name == "DATA")
                            _DIR_SV2_Data = value;
                        else if (name == "CONFIG")
                            _DIR_SV2_Config = value;
                        else if (name == "LOG")
                            _DIR_SV2_LOG = value;
                        else if (name == "PLUGINS")
                            _DIR_SV2_Plugins = value;
                        else if (name == "DATABASE")
                            _DIR_SV2_Database = value;
                    }

                    if (_DEBUG)
                    {
                        textoutput("_DIR_SV2_Data=" + _DIR_SV2_Data);
                        textoutput("_DIR_SV2_Config=" + _DIR_SV2_Config);
                        textoutput("_DIR_SV2_LOG=" + _DIR_SV2_LOG);
                        textoutput("_DIR_SV2_Plugins=" + _DIR_SV2_Plugins);
                        textoutput("_DIR_SV2_Database=" + _DIR_SV2_Database);
                    }

                }
                catch (Exception ee)
                {
                    textoutput("Error in parsing file  " + _FILE_SV2_Paths);
                    if (_DEBUG == true)
                        textoutput("Exception message is " + ee.Message);
                }

            }
            else
            {
                textoutput("Error: " + _FILE_SV2_Paths + " does not exist");
            }

            //USERPROFILEMP2_EXISTS = false;
            // read Paths.xml in User document directory
            if (File.Exists(Environment.GetEnvironmentVariable("APPDATA") + @"\Team MediaPortal\MP2-Client\Defaults\Paths.xml") == true)
            {
                _FILE_MP2_Paths = Environment.GetEnvironmentVariable("APPDATA") + @"\Team MediaPortal\MP2-Client\Defaults\Paths.xml";
                //USERPROFILEMP2_EXISTS = true;
            }
            else if (File.Exists(Environment.GetEnvironmentVariable("USERPROFILE") + @"\Team MediaPortal\MP2-Client\Defaults\Paths.xml") == true)
            {
                _FILE_MP2_Paths = Environment.GetEnvironmentVariable("USERPROFILE") + @"\Team MediaPortal\MP2-Client\Defaults\Paths.xml";
                //USERPROFILEMP2_EXISTS = true;
            }
            else if (File.Exists(_MP2_PROGRAM_FOLDER + @"\Defaults\Paths.xml") == true)
            {
                _FILE_MP2_Paths = _MP2_PROGRAM_FOLDER + @"\Defaults\Paths.xml"; 
                //USERPROFILEMP2_EXISTS = true;
            }


            // read Paths.xml in MediaPortal program document directory
            if (File.Exists(_FILE_MP2_Paths) == true)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(_FILE_MP2_Paths);

                    XmlNodeList allpaths = doc.SelectNodes("/Paths/Path");
                    foreach (XmlNode mypath in allpaths)
                    {
                        string name = mypath.Attributes["name"].Value;
                        string value = mypath.Attributes["value"].Value;
                        value = value.Replace("<APPLICATION_ROOT>", _MP2_PROGRAM_FOLDER);
                        value = value.Replace("<LOCAL_APPLICATION_DATA>", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                        value = value.Replace("<COMMON_APPLICATION_DATA>", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
                        value = value.Replace("<MY_DOCUMENTS>", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                        value = value.Replace("<DATA>", _DIR_MP2_Data);

                        /*
                        string APPDATA = _MP2_USER_FOLDER.Replace(@"\Team MediaPortal\MP2-Client", "");
                        value = value.Replace("<COMMON_APPLICATION_DATA>", APPDATA);
                        */

                        if (name=="DATA")
                            _DIR_MP2_Data = value;
                        else if (name == "CONFIG")
                            _DIR_MP2_Config = value;
                        else if (name == "LOG")
                            _DIR_MP2_LOG = value;
                        else if (name == "PLUGINS")
                            _DIR_MP2_Plugins =value;                                                
                    }
                    if (_DEBUG)
                    {
                        textoutput("_DIR_MP2_Data=" + _DIR_MP2_Data);
                        textoutput("_DIR_MP2_Config=" + _DIR_MP2_Config);
                        textoutput("_DIR_MP2_LOG=" + _DIR_MP2_LOG);
                        textoutput("_DIR_MP2_Plugins=" + _DIR_MP2_Plugins);
                    }

                }
                catch (Exception ee)
                {
                    textoutput("Error in parsing file  " + _FILE_MP2_Paths);
                    if (_DEBUG == true)
                        textoutput("Exception message is " + ee.Message);
                }

            }
            else
            {
                textoutput("Error: " + _FILE_MP2_Paths + " does not exist");
            }

        }


        private void InstallPathParser(ref string folder, string searchstring, StreamReader sfile, string textline)
        {
            if (textline == searchstring)
            {
                textline = sfile.ReadLine();
                folder = textline.Substring(10, textline.Length - 11 - 7);
                //substitute environment variables
                string[] array= folder.Split('\\');
                string newfolder = "";
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].Length > 2)
                    {
                        if (array[i].ToUpper() == "%PROGRAMDATA%")
                        {
                            array[i] = _MP_USER_FOLDER.Replace(@"\Team MediaPortal\MediaPortal", "");
                        }
                    }
                    newfolder += array[i] + "\\";
                    //textoutput(newfolder);
                }

                //complete path in case of relative names
                if (newfolder.Length<3)
                { 
                    //relative path - avoid exception later on
                    newfolder=_MP_PROGRAM_FOLDER+@"\"+newfolder;
                }
                else if ((newfolder[0] == '\\') && (newfolder[1] == '\\'))
                {
                    // path starts with server name use absolute filename
                }
                else if ((newfolder[1] == ':') && (newfolder[2] == '\\'))
                {
                    // path starts with drive letter use absolute file name
                }
                else
                {
                    //relative path
                    newfolder=_MP_PROGRAM_FOLDER+@"\"+newfolder;
                }
                folder = newfolder;

                //remove subsequent \
                if (folder.EndsWith("\\")==true)
                    folder=folder.Substring(0,folder.Length-1);

                if (_DEBUG == true)
                {
                    string token = searchstring.Substring(13, searchstring.Length - 13 - 2);
                    textoutput(token + "=" + folder);
                }
            }
        }

        public void textoutput(string text)
        {
            
            _LOG+=("InstallPaths:"+ text+"\n");

            if ((newmessage != null) && (DEBUG))
            {
                newmessage("InstallPaths:" + text + "\n");
            }
        }


        #endregion Methods
      
    }   
}


