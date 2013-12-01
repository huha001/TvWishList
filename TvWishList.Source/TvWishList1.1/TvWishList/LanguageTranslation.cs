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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 
using System.IO;

//using TvControl;
//using SetupTv;
//using TvEngine;
//using TvEngine.Events;
//using TvLibrary.Interfaces;
//using TvLibrary.Implementations;
#if (MPTV2)
using MediaPortal.Plugins.TvWishList.Items;
#else
using TvDatabase;
#endif
//using MediaPortal.Plugins;
//using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Log.huha;
using MediaPortal.Plugins;

namespace MediaPortal.Plugins.TvWishList
{
    public class LanguageTranslation
    {
        string[] Language = null;

        //constructor

        public LanguageTranslation()
        {

        }


        #region Language Customization

        public bool ReadLanguageFile()
        {
            Log.Debug("ReadLanguageFile started");
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting = null;
            setting = layer.GetSetting("TvWishList_TV_USER_FOLDER", "NOT_DEFINED");



            string TV_USER_FOLDER;
            TV_USER_FOLDER = layer.GetSetting("TvWishList_TV_USER_FOLDER", "NOT_FOUND").Value;
            if ((File.Exists(TV_USER_FOLDER + @"\TvService.exe") == true) || (Directory.Exists(TV_USER_FOLDER) == false))
            {
                //autodetect paths
                InstallPaths instpaths = new InstallPaths();  //define new instance for folder detection
#if (MPTV2) //Native MP2 Tv server
                instpaths.GetInstallPathsMP2();
                TV_USER_FOLDER = instpaths.TV2_USER_FOLDER;
#else
                    instpaths.GetInstallPaths();
                    TV_USER_FOLDER = instpaths.TV_USER_FOLDER;
#endif
                Log.Debug("TV server user folder detected at " + TV_USER_FOLDER);

                if ((File.Exists(TV_USER_FOLDER + @"\TvService.exe") == true) || (Directory.Exists(TV_USER_FOLDER) == false))
                {
                    Log.Error(@" TV server user folder does not exist - using C:\MediaPortal\TvWishList");
                    Log.Debug(@" TV server user folder does not exist - using C:\MediaPortal\TvWishList");
                    TV_USER_FOLDER = @"C:\MediaPortal";
                    if (Directory.Exists(TV_USER_FOLDER) == false)
                        Directory.CreateDirectory(TV_USER_FOLDER + @"\TvWishList");
                }
                else
                {//store found TV_USER_FOLDER
                    setting = layer.GetSetting("TvWishList_TV_USER_FOLDER", "NOT_FOUND");
                    setting.Value = TV_USER_FOLDER;
                    setting.Persist();
                }
            }





            if (Directory.Exists(setting.Value) == false)
            {
                Log.Error("Error: TV User Folder is not defined - cannot use language file");
                return false;
            }

            string foldername = setting.Value + @"\TvWishList\Languages\";

            setting = layer.GetSetting("TvWishList_LanguageFile", "strings_en.xml");

            string filename = foldername + setting.Value;
            if (File.Exists(filename) == false)
            {
                Log.Error("Could not find language file " + filename);
                filename = foldername + "strings_en.xml";                
                Log.Error("Switching to English default language file " + filename);
            }

            Log.Debug("Language file is " + filename);

            try
            {
                //read inputfile
                string[] inputlines = File.ReadAllLines(filename);
                Log.Debug(inputlines.Length.ToString()+" Lines read");
                int minimum = 0;
                int maximum = 0;
                int offset = 0;
                int offsetMinimum = 0;
                int offsetMaximum = 0;

                //process each line
                foreach (string line in inputlines)
                {
                    string myline = line.Replace("\t", string.Empty); //replace tab

                    //global replacements
                    if ((myline.Contains("<!-- TVSERVER VECTORSIZE="))||(myline.Contains("<!--TVSERVER VECTORSIZE=")))
                    {
                        myline = myline.Replace("<!-- TVSERVER VECTORSIZE=", string.Empty);
                        myline = myline.Replace("<!--TVSERVER VECTORSIZE=", string.Empty);
                        myline = myline.Replace("-->", string.Empty);

                        int size = 0;
                        int.TryParse(myline, out size);
                        Log.Debug("size = " + size.ToString());

                        Language = new string[size];

                        for (int i = 0; i < size; i++)
                        {
                            Language[i] = String.Empty;
                        }

                        //define at least true and false
                        try
                        {
                            Language[4000] = "true";
                            Language[4001] = "false";
                        }
                        catch //do nothing
                        {
                            Log.Error("Error in default initialization of true and false");
                        }
                        

                    }
                    else if ((myline.Contains("<!-- TVSERVER USE FROM=")) || (myline.Contains("<!--TVSERVER USE FROM=")) )
                    {
                        myline = myline.Replace("<!-- TVSERVER USE FROM=", string.Empty);
                        myline = myline.Replace("<!--TVSERVER USE FROM=", string.Empty);
                        myline = myline.Replace("-->", string.Empty);
                        myline = myline.Replace("OFFSET_FROM=", "\n"); //order is important
                        myline = myline.Replace("OFFSET_TO=", "\n");
                        myline = myline.Replace("TO=", "\n");
                        //myline = myline.Replace("OFFSET=", "\n");
                        
                        string[] tokenarray = myline.Split('\n');
                        if (tokenarray.Length != 4)
                        {
                            Log.Error("Invalid tv server command " + line.Replace("{","_") + " - cannot use language files");
                            return false;
                        }


                        minimum = 0;
                        int.TryParse(tokenarray[0], out minimum);
                        Log.Debug(" new minimum=" + minimum.ToString());
                        maximum = 0;
                        int.TryParse(tokenarray[1], out maximum);
                        Log.Debug(" new maximum=" + maximum.ToString());
                        offsetMinimum = 0;
                        int.TryParse(tokenarray[2], out offsetMinimum);
                        Log.Debug(" new offsetMinimum=" + offsetMinimum.ToString());
                        offsetMaximum = 0;
                        int.TryParse(tokenarray[3], out offsetMaximum);
                        Log.Debug(" new offsetMaximum=" + offsetMaximum.ToString());
                        offset = minimum - offsetMinimum;
                        Log.Debug(" new offset=" + offset.ToString());
                    }
                    else if (myline.Contains("<String id=\""))  // <String id="1107">Mehr</String> 
                    {
                        try
                        {
                            //remove leading spaces
                            int leadingSpaces = 0;
                            for (int i = 0; i < myline.Length; i++)
                            {
                                if (myline[i] == ' ')
                                {
                                    leadingSpaces++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            //textoutput("leadingSpaces=" + leadingSpaces.ToString());
                            myline = myline.Substring(leadingSpaces, myline.Length - leadingSpaces);//remove leading spaces

                            //remove trailing spaces
                            int trailingSpaces = 0;
                            for (int i = myline.Length - 1; i >= 0; i--)
                            {
                                if (myline[i] == ' ')
                                {
                                    trailingSpaces++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            //textoutput("trailingSpaces=" + trailingSpaces.ToString());
                            myline = myline.Substring(0, myline.Length - trailingSpaces);

                            myline = myline.Substring(12, myline.Length - 12);  //remove beginning comment and <String id=" 

                            

                            //now extract the numbers till the next "
                            int digits = 0;
                            for (int i = 0; i < myline.Length; i++)
                            {
                                if (myline[i] == '"')
                                {
                                    break;
                                }
                                else
                                {
                                    digits++;
                                }
                            }
                            //Log.Debug("digits=" + digits.ToString());

                            string digitString = myline.Substring(0, digits);
                            //Log.Debug("digitString=" + digitString);

                            if (myline.Length >= digits)
                                myline = myline.Substring(digits, myline.Length - digits);

                            myline = myline.Replace("\">", ""); //remove ending

                            

                            myline = myline.Replace("</String>", ""); //remove ending 

                            string valueString = myline;

                            
                            //Debug
                            //string output = valueString.Replace("{", "_");
                            //output = output.Replace("}", "_");
                            //Log.Debug("output ="+output);


                            int number = 0;
                            int.TryParse(digitString, out number);

                            //Log.Debug("number=" + number.ToString());

                            if ((number >= minimum) && (number <= maximum))
                            {
                                number = number - offset;
                                Language[number] = valueString;

                                
                                //Debug
                                //string output2 = Language[number].Replace("{", "_");
                                //output2 = output2.Replace("}", "_");                               
                                //Log.Debug("language[" + number.ToString() + "]=" + output2);
                            }

                        }
                        catch (Exception exc)
                        {
                            string output = line.Replace("{", "_");
                            output = output.Replace("}", "_");
                            Log.Error("Error: could not read string " + output + " Exception was " + exc.Message);
                        }

                    }//end expression


                }//end foreach line
            }
            catch (Exception exc)
            {
                Log.Error("Error: could not read language file " + filename + " Exception was " + exc.Message);
                return false;
            }


            



            Log.Debug("Language file read");
            return true;

        }


        //can be used compatible to MP1 with 
        public string Get(int number)
        {
            string label = TranslateString(string.Empty, number);
            return label;
        }

        public string TranslateString(string defaultvalue, int number)
        {
            string value;

            try
            {
                value = Language[number];
                //Log.Debug("translate: value=" + value);
            }
            catch
            {
                value = defaultvalue;
                Log.Debug("translate exception: number=" + number.ToString() + " defaultvalue=" + value);
            }

            return value;
        }

        public string TranslateString(string defaultvalue, int number, string text)
        {
            string value;

            try
            {
                value = String.Format(Language[number], text);
            }
            catch
            {
                value = String.Format(defaultvalue, text);
            }

            return value;
        }

        public string TranslateString(string defaultvalue, int number, string text1, string text2)
        {
            string value;

            try
            {
                value = String.Format(Language[number], text1, text2);
            }
            catch
            {
                value = String.Format(defaultvalue, text1, text2);
            }

            return value;
        }

        public string TranslateString(string defaultvalue, int number, string text1, string text2, string text3)
        {
            string value;

            try
            {
                value = String.Format(Language[number], text1, text2, text3);
            }
            catch
            {
                value = String.Format(defaultvalue, text1, text2, text3);
            }

            return value;
        }

        #endregion Language Customization

    }
}
