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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
//using System.Threading;
//using System.IO;
using System.Diagnostics;

using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
//using MediaPortal.Plugins;

using TvControl;
using TvDatabase;
using Gentle.Framework;
using TvWishList;
using Log = TvLibrary.Log.huha.Log;


namespace TvWishList
{
    public partial class TvWishListSetup : Form
    {
        public static string SectionName = "TvWishListMP";
        string[] Array_Defaultformats;
        int Defaultformat_select = -1;
        ServiceInterface myinterface;
        char TvWishItemSeparator = ';';
        string TimeOutDefault = "60"; //60s default

        TvWishProcessing myTvWishes = null;
        ServiceInterface myconnect = new ServiceInterface();
        string prerecord = "-1";
        string postrecord = "-1";

        public TvWishListSetup()
        {
            InitializeComponent();

            myTvWishes = new TvWishProcessing();
            PluginGuiLocalizeStrings.LoadMPlanguage();
            //initialize TV database
            myinterface = new ServiceInterface();
            //try to shorten wait time when switching to the page by opening the connection at the beginning
            myinterface.ConnectToDatabase();

            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;
            //default pre and post record from general recording settings
            setting = layer.GetSetting("preRecordInterval", "5");
            prerecord = setting.Value;

            setting = layer.GetSetting("postRecordInterval", "5");
            postrecord = setting.Value;

            IList<Channel> allChannels = Channel.ListAll();
            IList<ChannelGroup> allChannelGroups = ChannelGroup.ListAll();
            IList<RadioChannelGroup> allRadioChannelGroups = RadioChannelGroup.ListAll();
            IList<Card> allCards = Card.ListAll();

            Log.Debug("allCards.Count=" + allCards.Count.ToString());

            myTvWishes.TvServerSettings(prerecord, postrecord, allChannelGroups, allRadioChannelGroups, allChannels, allCards, TvWishItemSeparator);

            LanguageTranslation();

            LoadSettings();

        }

        public void LanguageTranslation()
        {
            //load MP language
            PluginGuiLocalizeStrings.LoadMPlanguage();

            tabControl1.TabPages[0].Text = PluginGuiLocalizeStrings.Get(5000); //Main
            label2.Text = PluginGuiLocalizeStrings.Get(5051); //Before you use the MediaPortal plugin you must have 
            label3.Text = PluginGuiLocalizeStrings.Get(5052); //configured and tested the TvWishList Tvserver plugin
            groupBox4.Text = PluginGuiLocalizeStrings.Get(1109); //Mode
            radioButtonEasy.Text = PluginGuiLocalizeStrings.Get(10101);//Easy
            radioButtonExpert.Text = PluginGuiLocalizeStrings.Get(10102);//Expert
            groupBox5.Text = PluginGuiLocalizeStrings.Get(10160);//Settings
            checkBoxDebug.Text = PluginGuiLocalizeStrings.Get(5200);//Debug
            checkBoxDisableQuickMenu.Text = PluginGuiLocalizeStrings.Get(5201);//Disable TvWish Quick Menu
            checkBoxDisableInfoWindow.Text = PluginGuiLocalizeStrings.Get(5206);//Disable Message Window
            label18.Text = PluginGuiLocalizeStrings.Get(5202);//Timeout
            label19.Text = PluginGuiLocalizeStrings.Get(5203);//Change Timeout 60 only if you experience time out errors
            buttonhelp.Text = PluginGuiLocalizeStrings.Get(5102);//Help
            label1.Text = PluginGuiLocalizeStrings.Get(5208); //Save/Load Client Settings on/from Tv Server
            buttonTvserverSave.Text = PluginGuiLocalizeStrings.Get(5211); //Save On TV
            buttonTvserverLoad.Text = PluginGuiLocalizeStrings.Get(5212); //Load From TV   

            tabControl1.TabPages[1].Text = PluginGuiLocalizeStrings.Get(6000); //Filter
            label6.Text = PluginGuiLocalizeStrings.Get(6100); //Select the filter items you want to edit
            checkBoxActive.Text = PluginGuiLocalizeStrings.Get(2800); //Active
            checkBoxName.Text = PluginGuiLocalizeStrings.Get(2824); //Name
            checkBoxMatchType.Text = PluginGuiLocalizeStrings.Get(2802); //Match Type
            checkBoxExclude.Text = PluginGuiLocalizeStrings.Get(2806); //Exclude
            checkBoxRecordtype.Text = PluginGuiLocalizeStrings.Get(2804); //Record Type
            checkBoxAction.Text = PluginGuiLocalizeStrings.Get(2805); //Action
            checkBoxWithinNextHours.Text = PluginGuiLocalizeStrings.Get(2826); //Show Only Within The Next Hour(s)
            checkBoxGroup.Text = PluginGuiLocalizeStrings.Get(2803); //Group
            checkBoxChannel.Text = PluginGuiLocalizeStrings.Get(2822); //Channel
            checkBoxAfterDays.Text = PluginGuiLocalizeStrings.Get(2820); //After Day
            checkBoxBeforeDay.Text = PluginGuiLocalizeStrings.Get(2821); //Before Day
            checkBoxAfterTime.Text = PluginGuiLocalizeStrings.Get(2818); //After Time
            checkBoxBeforeTime.Text = PluginGuiLocalizeStrings.Get(2819); //Before Time
            checkBoxEpisodeCriteria.Text = PluginGuiLocalizeStrings.Get(2833); //Change Episode Match Criteria
            checkBoxEpisodeName.Text = PluginGuiLocalizeStrings.Get(2810); //Episode Name
            checkBoxEpisodePart.Text = PluginGuiLocalizeStrings.Get(2811); //Episode Part
            checkBoxEpisodeNumber.Text = PluginGuiLocalizeStrings.Get(2812); //Episode Number
            checkBoxSeriesNumber.Text = PluginGuiLocalizeStrings.Get(2813); //Series Number
            checkBoxPrerecord.Text = PluginGuiLocalizeStrings.Get(2808); //Pre Recording Time
            checkBoxPostrecord.Text = PluginGuiLocalizeStrings.Get(2809); //Post Recording Time
            checkBoxPreferredGroup.Text = PluginGuiLocalizeStrings.Get(2834); //Preferred Channel Group
            checkBoxKeepEpisodes.Text = PluginGuiLocalizeStrings.Get(2814); //Keep Episodes
            checkBoxKeepUntil.Text = PluginGuiLocalizeStrings.Get(2815); //Keep Until
            checkBoxPriority.Text = PluginGuiLocalizeStrings.Get(2817); //Priority
            checkBoxRecommendedCard.Text = PluginGuiLocalizeStrings.Get(2816); //Recommended Card
            checkBoxSkip.Text = PluginGuiLocalizeStrings.Get(2823); //Skip
            checkBoxUseFolderNames.Text = PluginGuiLocalizeStrings.Get(2825); //Move Recordings to Folder
            checkBoxIncludeRecordings.Text = PluginGuiLocalizeStrings.Get(2835); //Including Recordings
            buttonAllActive.Text = PluginGuiLocalizeStrings.Get(10111); //All Active
            buttonAllInActive.Text = PluginGuiLocalizeStrings.Get(10112); //All Inactive
            buttonDefault.Text = PluginGuiLocalizeStrings.Get(6202); //Defaults
            label24.Text = PluginGuiLocalizeStrings.Get(7000); //Defaults Configuration
            buttonCheck.Text = PluginGuiLocalizeStrings.Get(7002); //Check

            tabControl1.TabPages[2].Text = PluginGuiLocalizeStrings.Get(8000); //Formats
            label16.Text = PluginGuiLocalizeStrings.Get(8110); //Formats should be changed by experts only. 
            label17.Text = PluginGuiLocalizeStrings.Get(8111); //Empty fields will use defaults from the language file
            label11.Text = PluginGuiLocalizeStrings.Get(8100); //Date And Time Formats
            groupBox3.Text = PluginGuiLocalizeStrings.Get(8106); //List Item Formats
            label9.Text = PluginGuiLocalizeStrings.Get(8101); //Main Menu Item Format
            label13.Text = PluginGuiLocalizeStrings.Get(8102); //Results Menu Item Format
            groupBox1.Text = PluginGuiLocalizeStrings.Get(8107); //Email And Record Modus
            label7.Text = PluginGuiLocalizeStrings.Get(8103); //Main Menu Textbox Format
            label8.Text = PluginGuiLocalizeStrings.Get(8104); //Results Menu Textbox Format
            groupBox2.Text = PluginGuiLocalizeStrings.Get(8108); //View Only Modus
            label15.Text = PluginGuiLocalizeStrings.Get(8103); //Main Menu Textbox Format
            label14.Text = PluginGuiLocalizeStrings.Get(8104); //Results Menu Textbox Format

            int ctr = 0;
            for (int i = 0; i <= 35; i++)
            {
                if ((i == 7) || ((i >= 27) && (i <= 32)))
                {
                    continue;
                }
                comboBoxDefaultFormats.Items[ctr] = PluginGuiLocalizeStrings.Get(i+2800);
                ctr++;
            }

            //missing 7,27,28,29,30,31,32  total 29 items  29 in list  36-7 = 29

        }

        public void LoadSettings()
        {
            
            try
            {
                using (var reader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "TvWishListMP.xml")))
                {
                    //DEBUG first
                    checkBoxDebug.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxDebug", "false"));
                    myTvWishes.Debug = checkBoxDebug.Checked;
                    Log.DebugValue = checkBoxDebug.Checked;


                    checkBoxAction.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxAction", "true"));
                    checkBoxActive.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxActive", "false"));
                    checkBoxAfterDays.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxAfterDays", "false"));
                    checkBoxAfterTime.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxAfterTime", "false"));
                    checkBoxBeforeDay.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxBeforeDay", "false"));
                    checkBoxBeforeTime.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxBeforeTime", "false"));
                    checkBoxChannel.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxChannel", "true"));
                    checkBoxEpisodeName.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxEpisodeName", "false"));
                    checkBoxEpisodeNumber.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxEpisodeNumber", "false"));
                    checkBoxEpisodePart.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxEpisodePart", "false"));
                    checkBoxExclude.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxExclude", "true"));
                    checkBoxGroup.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxGroup", "true"));
                    //checkBoxHits.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxHits", "true"));
                    checkBoxKeepEpisodes.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxKeepEpisodes", "false"));
                    checkBoxKeepUntil.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxKeepUntil", "false"));
                    checkBoxMatchType.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxMatchType", "true"));
                    checkBoxName.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxName", "true"));
                    checkBoxPostrecord.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxPostrecord", "false"));
                    checkBoxPrerecord.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxPrerecord", "false"));
                    checkBoxPriority.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxPriority", "false"));
                    checkBoxRecommendedCard.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxRecommendedCard", "false"));
                    checkBoxRecordtype.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxRecordtype", "true"));
                    // SearchFor is always true;
                    checkBoxSeriesNumber.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxSeriesNumber", "false"));
                    checkBoxSkip.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxSkip", "true"));
                    checkBoxUseFolderNames.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxUseFolderNames", "false"));
                    checkBoxWithinNextHours.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxRecordtype", "false"));
                    checkBoxEpisodeCriteria.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxEpisodeCriteria", "false"));
                    checkBoxPreferredGroup.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxPreferredGroup", "false"));
                    checkBoxIncludeRecordings.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxIncludeRecordings", "false"));
                    //modify for listview table changes


                    checkBoxDisableInfoWindow.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxDisableInfoWindow", "false"));
                    checkBoxDisableQuickMenu.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxDisableQuickMenu", "false"));

                    radioButtonEasy.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "radioButtonEasy", "true"));
                    radioButtonExpert.Checked = Convert.ToBoolean(reader.GetValueAsString(SectionName, "radioButtonExpert", "false"));

                    //filenametextBox.Text=reader.GetValueAsString(SectionName, "TvwishlistFolder", "");
                    //Log.Debug("[TVWishListMP] TvWishListSetup: loadsettings: " + filenametextBox.Text);

                    textBoxDateTimeFormat.Text = reader.GetValueAsString(SectionName, "DateTimeFormat", "");
                    textBoxMainItemFormat.Text = reader.GetValueAsString(SectionName, "MainItemFormat", "");
                    textBoxEmailMainFormat.Text = reader.GetValueAsString(SectionName, "EmailMainFormat", "");
                    textBoxEmailResultsFormat.Text = reader.GetValueAsString(SectionName, "EmailResultsFormat", "");
                    textBoxResultsItemFormat.Text = reader.GetValueAsString(SectionName, "ResultsItemFormat", "");
                    textBoxViewMainFormat.Text = reader.GetValueAsString(SectionName, "ViewMainFormat", "");
                    textBoxViewResultsFormat.Text = reader.GetValueAsString(SectionName, "ViewResultsFormat", "");

                    textBoxTimeOut.Text = reader.GetValueAsString(SectionName, "TimeOut", "60");

                    //load defaultformats
                    string defaultformatstring = reader.GetValueAsString(SectionName, "DefaultFormats", ""); //Complete User default string in English
                    LoadDefaultFormatsFromString(defaultformatstring);
                }
            }
            catch (Exception exc)
            {
                Log.Debug("[TVWishListMP] TvWishListSetup: Error LoadSettings: Exception " + exc.Message);
            }
        }

        public void SaveSettings()
        {
            try
            {
                using (var reader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "TvWishListMP.xml")))
                {
                    reader.SetValue(SectionName, "checkBoxDebug", checkBoxDebug.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxAction", checkBoxAction.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxActive", checkBoxActive.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxAfterDays", checkBoxAfterDays.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxAfterTime", checkBoxAfterTime.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxBeforeDay", checkBoxBeforeDay.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxBeforeTime", checkBoxBeforeTime.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxChannel", checkBoxChannel.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxEpisodeName", checkBoxEpisodeName.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxEpisodeNumber", checkBoxEpisodeNumber.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxEpisodePart", checkBoxEpisodePart.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxExclude", checkBoxExclude.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxGroup", checkBoxGroup.Checked.ToString());
                    //reader.SetValue(SectionName, "checkBoxHits", checkBoxHits.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxKeepEpisodes", checkBoxKeepEpisodes.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxKeepUntil", checkBoxKeepUntil.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxMatchType", checkBoxMatchType.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxName", checkBoxName.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxPostrecord", checkBoxPostrecord.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxPrerecord", checkBoxPrerecord.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxPriority", checkBoxPriority.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxRecommendedCard", checkBoxRecommendedCard.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxRecordtype", checkBoxRecordtype.Checked.ToString());                    
                    reader.SetValue(SectionName, "checkBoxSkip", checkBoxSkip.Checked.ToString());                    
                    reader.SetValue(SectionName, "checkBoxSeriesNumber", checkBoxSeriesNumber.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxUseFolderNames", checkBoxUseFolderNames.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxWithinNextHours", checkBoxWithinNextHours.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxEpisodeCriteria", checkBoxEpisodeCriteria.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxPreferredGroup", checkBoxPreferredGroup.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxIncludeRecordings", checkBoxIncludeRecordings.Checked.ToString());
                    //modify for listview table changes

                    reader.SetValue(SectionName, "checkBoxDisableInfoWindow", checkBoxDisableInfoWindow.Checked.ToString());
                    reader.SetValue(SectionName, "checkBoxDisableQuickMenu", checkBoxDisableQuickMenu.Checked.ToString());
                    
                    reader.SetValue(SectionName, "radioButtonEasy", radioButtonEasy.Checked.ToString());
                    reader.SetValue(SectionName, "radioButtonExpert", radioButtonExpert.Checked.ToString());
                  
                    //reader.SetValue(SectionName, "TvwishlistFolder", filenametextBox.Text);

                    reader.SetValue(SectionName, "DateTimeFormat", textBoxDateTimeFormat.Text);
                    reader.SetValue(SectionName, "MainItemFormat", textBoxMainItemFormat.Text);
                    reader.SetValue(SectionName, "EmailMainFormat", textBoxEmailMainFormat.Text);
                    reader.SetValue(SectionName, "EmailResultsFormat", textBoxEmailResultsFormat.Text);
                    reader.SetValue(SectionName, "ResultsItemFormat", textBoxResultsItemFormat.Text);
                    reader.SetValue(SectionName, "ViewMainFormat", textBoxViewMainFormat.Text);
                    reader.SetValue(SectionName, "ViewResultsFormat", textBoxViewResultsFormat.Text);
                    try
                    {
                        int test = Convert.ToInt32(textBoxTimeOut.Text);
                    }
                    catch
                    {
                        textBoxTimeOut.Text = TimeOutDefault;
                    }
                    reader.SetValue(SectionName, "TimeOut", textBoxTimeOut.Text);

                    

                    string defaultformats = SaveDefaultFormatsToString();
                    
                    reader.SetValue(SectionName, "DefaultFormats", defaultformats);     //saving checked complete default user string              
                }
            }
            catch (Exception exc)
            {
                Log.Debug("[TVWishListMP] TvWishListSetup: Error SaveSettings: Exception " + exc.Message);
            }
        }

        private void LoadDefaultFormatsFromString(string defaultformatstring)
        {
            string[] defaultformats = defaultformatstring.Split(TvWishItemSeparator);
            if (defaultformats[(int)TvWishEntries.prerecord] == "-1")
            {
                defaultformats[(int)TvWishEntries.prerecord] = prerecord;    //replace -1 from hardcoded vector
            }
            if (defaultformats[(int)TvWishEntries.postrecord] == "-1")
            {
                defaultformats[(int)TvWishEntries.postrecord] = postrecord;  //replace -1 from hardcoded vector
            }
            if (defaultformats[(int)TvWishEntries.tvwishid] == "-1")
            {
                defaultformats[(int)TvWishEntries.tvwishid] = "1";           //replace -1 from hardcoded vector
            }
            defaultformatstring = SaveToStringNoLngTranslation(defaultformats);
            Log.Debug("LoadSettings(): defaultformatstring=" + defaultformatstring);
            string checkedrow = myTvWishes.CheckRowEntries(defaultformatstring, TvWishItemSeparator); //complete user default in English and checked
            Log.Debug("LoadSettings(): checked defaultformatstring=" + checkedrow);
            TvWish userCountryhwish = new TvWish();
            userCountryhwish = myTvWishes.CreateTvWish(true, checkedrow.Split(TvWishItemSeparator));  //user default tvwish in Country language complete and checked
            Log.Debug("userEnglishwish including translation");
            myTvWishes.DebugTvWish(userCountryhwish);
            //myTvWishes.TvWishLanguageTranslation(ref userEnglishwish); //user default Tvwish in in country language
            string userCountryDefaultsCompleteString = myTvWishes.SaveToStringNoTranslation(ref userCountryhwish);  //complete user default country string
            Log.Debug("LoadSettings(): userCountryDefaultsCompleteString=" + userCountryDefaultsCompleteString);
            Array_Defaultformats = Defaults2TvWishArray(userCountryDefaultsCompleteString.Split(TvWishItemSeparator));  //partial user default country array
            Log.Debug("Array_Defaultformats.Length=" + Array_Defaultformats.Length.ToString());

            //preselect first element
            textBoxDefaultFormats.Text = Array_Defaultformats[0];
            Defaultformat_select = 0;//must be before comboBoxDefaultFormats.Text = ;
            comboBoxDefaultFormats.Text = comboBoxDefaultFormats.Items[0].ToString();

            if (Array_Defaultformats.Length != (int)TvWishEntries.end - 7)
            {
                MessageBox.Show("Invalid Default Format Array", "Error");
                Log.Error("Invalid Default Format Array");
                Log.Error("Array_Defaultformats.Length=" + Array_Defaultformats.Length.ToString());
                Log.Error("(int)TvWishEntries.skip=" + ((int)TvWishEntries.skip).ToString());
            }
        }

        private string SaveDefaultFormatsToString()
        {
            //store default formats
            if (Defaultformat_select >= 0)
            {
                Log.Debug("Save: Defaultformat_select=" + Defaultformat_select.ToString());
                Array_Defaultformats[Defaultformat_select] = textBoxDefaultFormats.Text;  // store last selected element
            }

            //SavePart DefaultFormats
            string[] userCountryDefaultsComplete = TvWishArray2Defaults(Array_Defaultformats);  //complete user default Formats array in country language from screen
            string userCountryDefaultsCompleteString = SaveToStringNoLngTranslation(userCountryDefaultsComplete); //complete user default Formats string in country language from screen
            Log.Debug("SaveSettings(): userCountryDefaultsCompleteString=" + userCountryDefaultsCompleteString);
            TvWish usercountrywish = new TvWish();
            usercountrywish = myTvWishes.CreateTvWishNoCheckingNoTranslation(true, userCountryDefaultsCompleteString.Split(TvWishItemSeparator));  //user default tvwish in country language, because input has full array size                    
            Log.Debug("userEnglishwish before translation");
            myTvWishes.DebugTvWish(usercountrywish);
            usercountrywish.tvwishid = "1";
            string userEnglishDefaultsCompleteString = myTvWishes.SaveToString(ref usercountrywish); //complete default user string in english (includes reverse language translation to string)
            Log.Debug("SaveSettings(): userEnglishDefaultsCompleteString=" + userEnglishDefaultsCompleteString);
            string defaultformats = myTvWishes.CheckRowEntries(userEnglishDefaultsCompleteString, TvWishItemSeparator); //complete default user string in english and checked                   
            Log.Debug("Save: after checking defaultformats=" + defaultformats);
            return defaultformats;
        }

        private void buttonAllActive_Click(object sender, EventArgs e)
        {
            checkBoxAction.Checked = true;
            checkBoxActive.Checked = true;
            checkBoxAfterDays.Checked = true;
            checkBoxAfterTime.Checked = true;
            checkBoxBeforeDay.Checked = true;
            checkBoxBeforeTime.Checked = true;
            checkBoxChannel.Checked = true;
            checkBoxEpisodeName.Checked = true;
            checkBoxEpisodeNumber.Checked = true;
            checkBoxEpisodePart.Checked = true;
            checkBoxExclude.Checked = true;
            checkBoxGroup.Checked = true;
            //checkBoxHits.Checked = true;
            checkBoxKeepEpisodes.Checked = true;
            checkBoxKeepUntil.Checked = true;
            checkBoxMatchType.Checked = true;
            checkBoxName.Checked = true;
            checkBoxPostrecord.Checked = true;
            checkBoxPrerecord.Checked = true;
            checkBoxPriority.Checked = true;
            checkBoxRecommendedCard.Checked = true;
            checkBoxRecordtype.Checked = true;
            checkBoxSeriesNumber.Checked = true;
            checkBoxSkip.Checked = true;
            checkBoxUseFolderNames.Checked = true;
            checkBoxWithinNextHours.Checked = true;
            checkBoxEpisodeCriteria.Checked = true;
            checkBoxPreferredGroup.Checked = true;
            checkBoxIncludeRecordings.Checked = true;
            //modify for listview table changes
        }

        private void buttonAllInActive_Click(object sender, EventArgs e)
        {
            checkBoxAction.Checked = false;
            checkBoxActive.Checked = false;
            checkBoxAfterDays.Checked = false;
            checkBoxAfterTime.Checked = false;
            checkBoxBeforeDay.Checked = false;
            checkBoxBeforeTime.Checked = false;
            checkBoxChannel.Checked = false;
            checkBoxEpisodeName.Checked = false;
            checkBoxEpisodeNumber.Checked = false;
            checkBoxEpisodePart.Checked = false;
            checkBoxExclude.Checked = false;
            checkBoxGroup.Checked = false;
            //checkBoxHits.Checked = false;
            checkBoxKeepEpisodes.Checked = false;
            checkBoxKeepUntil.Checked = false;
            checkBoxMatchType.Checked = false;
            checkBoxName.Checked = false;
            checkBoxPostrecord.Checked = false;
            checkBoxPrerecord.Checked = false;
            checkBoxPriority.Checked = false;
            checkBoxRecommendedCard.Checked = false;
            checkBoxRecordtype.Checked = false;
            checkBoxSeriesNumber.Checked = false;
            checkBoxSkip.Checked = false;
            checkBoxUseFolderNames.Checked = false;
            checkBoxWithinNextHours.Checked = false;
            checkBoxEpisodeCriteria.Checked = false;
            checkBoxPreferredGroup.Checked = false;
            checkBoxIncludeRecordings.Checked = false;
            //modify for listview table changes
        }

        private void buttonDefault_Click(object sender, EventArgs e)
        {
            switch (MessageBox.Show(PluginGuiLocalizeStrings.Get(6203), PluginGuiLocalizeStrings.Get(4401), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
            {
                case DialogResult.Yes:
                    {
                        //Tv server tab
                        textBoxTimeOut.Text = "60";

                        //Filter Items tab
                        checkBoxAction.Checked = true;
                        checkBoxActive.Checked = false;
                        checkBoxAfterDays.Checked = false;
                        checkBoxAfterTime.Checked = false;
                        checkBoxBeforeDay.Checked = false;
                        checkBoxBeforeTime.Checked = false;
                        checkBoxChannel.Checked = true;
                        checkBoxEpisodeName.Checked = false;
                        checkBoxEpisodeNumber.Checked = false;
                        checkBoxEpisodePart.Checked = false;
                        checkBoxExclude.Checked = false;
                        checkBoxGroup.Checked = true;
                        //checkBoxHits.Checked = true;
                        checkBoxKeepEpisodes.Checked = false;
                        checkBoxKeepUntil.Checked = false;
                        checkBoxMatchType.Checked = true;
                        checkBoxName.Checked = true;
                        checkBoxPostrecord.Checked = false;
                        checkBoxPrerecord.Checked = false;
                        checkBoxPriority.Checked = false;
                        checkBoxRecommendedCard.Checked = false;
                        checkBoxRecordtype.Checked = true;
                        checkBoxSeriesNumber.Checked = false;
                        checkBoxSkip.Checked = true;
                        checkBoxUseFolderNames.Checked = false;
                        checkBoxWithinNextHours.Checked = false;
                        checkBoxEpisodeCriteria.Checked = false;
                        checkBoxPreferredGroup.Checked = false;
                        checkBoxIncludeRecordings.Checked = false;
                        //modify for listview table changes

                        //load hardcoded Defaults from TvWishes
                        LoadDefaultFormatsFromString(myTvWishes.DefaultFormatString);

                        checkBoxDisableInfoWindow.Checked = true;
                        checkBoxDisableQuickMenu.Checked = false;

                        //Formats tab
                        textBoxDateTimeFormat.Text = "";
                        textBoxEmailMainFormat.Text = "";
                        textBoxEmailResultsFormat.Text = "";
                        textBoxMainItemFormat.Text = "";
                        textBoxResultsItemFormat.Text = "";
                        textBoxViewMainFormat.Text = "";
                        textBoxViewResultsFormat.Text="";

                    }
                    break;
            }
            

        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Close();
        }

        private void buttonCancelExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBoxEvent_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (Defaultformat_select >= 0)
                {
                    Array_Defaultformats[Defaultformat_select] = textBoxDefaultFormats.Text;  // store last selected element
                }
                textBoxDefaultFormats.Text = Array_Defaultformats[comboBoxDefaultFormats.SelectedIndex];  //load selected element
                Defaultformat_select = comboBoxDefaultFormats.SelectedIndex; //store last selected element index
            }
            catch (Exception exc)
            {
                Log.Error("Error in combobox selection changed\nException mesage was: " + exc.Message);
            }
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            Log.Debug("Check button pressed");
            Log.Debug("Current directory = " + System.Environment.CurrentDirectory.ToString());

            //saving and reloading will include checking
            string defaultformatsString = SaveDefaultFormatsToString();
            LoadDefaultFormatsFromString(defaultformatsString);
        }

        private string[] Defaults2TvWishArray(string[] Defaults)
        {
            string[] tvWishArray = new string[(int)TvWishEntries.end - 7]; // 6 cunters and tvwishid removed
            int ctr = 0;
            for (int i = 0; i < (int)TvWishEntries.end; i++)
            {
                if ((i != (int)TvWishEntries.tvwishid) && (i != (int)TvWishEntries.viewed) && (i != (int)TvWishEntries.emailed) && (i != (int)TvWishEntries.conflicts) && (i != (int)TvWishEntries.deleted) && (i != (int)TvWishEntries.recorded) && (i != (int)TvWishEntries.scheduled))
                {
                    tvWishArray[ctr] = Defaults[i];
                    ctr++;
                }
            }
            return tvWishArray;
        }

        private string[] TvWishArray2Defaults(string[] TvWishArray)
        {
            string[] defaultFormats = new string[(int)TvWishEntries.end];
            int ctr = 0;
            for (int i = 0; i < (int)TvWishEntries.end; i++)
            {
                //Log.Debug("i="+i.ToString()  );
                //Log.Debug("ctr=" + ctr.ToString());

                //id must be -1
                if (i == (int)TvWishEntries.tvwishid)
                    defaultFormats[i] = "-1";

                //counter defaults must be 0
                else if (i == (int)TvWishEntries.viewed)
                    defaultFormats[i] = "0";

                else if (i == (int)TvWishEntries.emailed)
                    defaultFormats[i] = "0";

                else if (i == (int)TvWishEntries.conflicts)
                    defaultFormats[i] = "0";

                else if (i == (int)TvWishEntries.deleted)
                    defaultFormats[i] = "0";

                else if (i == (int)TvWishEntries.recorded)
                    defaultFormats[i] = "0";

                else if (i == (int)TvWishEntries.scheduled)
                    defaultFormats[i] = "0";

                else
                {
                    try
                    {
                        defaultFormats[i] = Array_Defaultformats[ctr];
                        //Log.Debug("Array_Defaultformats[ctr]=" + Array_Defaultformats[ctr]);
                        ctr++;
                    }
                    catch (Exception exc)
                    {
                        Log.Error("Error in TvWishArray2Defaults for i="+i.ToString()+": Exception="+exc.Message);
                    }
                    
                }
            }
            return defaultFormats;
        }
        
        private string SaveToStringNoLngTranslation(string[] mywisharray)
        {
            if (mywisharray.Length != (int)TvWishEntries.end)
            {
                Log.Error("Fatal Error in SaveToStringNoLngTranslation: elements in mywisharray do not match TvWishEntries - returning empty string");
                return string.Empty;
            }

            string row = "";

            for (int i = 0; i < (int)TvWishEntries.end; i++)
            {
                row += mywisharray[i] + TvWishItemSeparator.ToString(); ;
            }

            return row;
        }
        
        private void radioButtonEasy_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonEasy.Checked == true)
            {
                EasyMode();
            }
            
        }

        private void radioButtonExpert_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonExpert.Checked == true)
            {
                AdvancedMode();
            }
        }

        private void EasyMode()
        {
            if (tabControl1.TabPages.Contains(tabPage2))
                tabControl1.TabPages.Remove(tabPage2);
            if (tabControl1.TabPages.Contains(tabPage3))
                tabControl1.TabPages.Remove(tabPage3);

            label18.Hide();
            textBoxTimeOut.Hide();
            label19.Hide();
        }

        private void AdvancedMode()
        {
            if (!tabControl1.TabPages.Contains(tabPage2))
                tabControl1.TabPages.Add(tabPage2);
            if (!tabControl1.TabPages.Contains(tabPage3))
                tabControl1.TabPages.Add(tabPage3);

            label18.Show();
            textBoxTimeOut.Show();
            label19.Show();
        }

        private void buttonhelp_Click(object sender, EventArgs e)
        {
            //Help
            Process proc = new Process();
            ProcessStartInfo procstartinfo = new ProcessStartInfo();
            procstartinfo.FileName = "TvWishList.pdf";
            procstartinfo.WorkingDirectory = @"Docs";
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

        private void buttonTvserverSave_Click(object sender, EventArgs e)
        {
            //switch (MessageBox.Show("Do you want to Save the client setting on the Tv server and overwrite the previous settings?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
            switch (MessageBox.Show(PluginGuiLocalizeStrings.Get(5209), PluginGuiLocalizeStrings.Get(4401), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
            
            {
                case DialogResult.Yes:
                    {
                        try
                        {
                            SaveSettingsToTvServer();
                            MessageBox.Show(PluginGuiLocalizeStrings.Get(5215), PluginGuiLocalizeStrings.Get(4400));
                        }
                        catch (Exception exc)
                        {
                            Log.Debug("[TVWishListMP] TvWishListSetup: Error SaveSettings: Exception " + exc.Message);
                            MessageBox.Show(PluginGuiLocalizeStrings.Get(5216), PluginGuiLocalizeStrings.Get(4305));
                        }
                        break;
                    }
            }
        }

        private void buttonTvserverLoad_Click(object sender, EventArgs e)
        {
            //switch (MessageBox.Show("Do you want to Load the client setting from the Tv server and overwrite the current client settings?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
            switch (MessageBox.Show(PluginGuiLocalizeStrings.Get(5210), PluginGuiLocalizeStrings.Get(4401), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
            
            {
                case DialogResult.Yes:
                    {
                        try
                        {
                            LoadSettingsFromTvServer();
                            MessageBox.Show(PluginGuiLocalizeStrings.Get(5213), PluginGuiLocalizeStrings.Get(4400));
                        }
                        catch (Exception exc)
                        {
                            Log.Debug("[TVWishListMP] TvWishListSetup: Error LoadSettingsFromTvServer: Exception " + exc.Message);
                            MessageBox.Show(PluginGuiLocalizeStrings.Get(5214), PluginGuiLocalizeStrings.Get(4305));
                        }
                        break;
                    }
            }
        }

        private void SaveSettingsToTvServer()
        {
            //save client settings tom Tv server
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;

            
            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxDebug","false");
            setting.Value = checkBoxDebug.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxAction", "false");
            setting.Value = checkBoxAction.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxActive", "false");
            setting.Value = checkBoxActive.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxAfterDays", "false");
            setting.Value = checkBoxAfterDays.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxAfterTime", "false");
            setting.Value = checkBoxAfterTime.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxBeforeDay", "false");
            setting.Value = checkBoxBeforeDay.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxBeforeTime", "false");
            setting.Value = checkBoxBeforeTime.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxChannel", "false");
            setting.Value = checkBoxChannel.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodeName", "false");
            setting.Value = checkBoxEpisodeName.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodeNumber", "false");
            setting.Value = checkBoxEpisodeNumber.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodePart", "false");
            setting.Value = checkBoxEpisodePart.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxExclude", "false");
            setting.Value = checkBoxExclude.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxGroup", "false");
            setting.Value = checkBoxGroup.Checked.ToString();
            setting.Persist();

            //setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxHits", "false");checkBoxHits.Checked.ToString();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxKeepEpisodes", "false");
            setting.Value = checkBoxKeepEpisodes.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxKeepUntil", "false");
            setting.Value = checkBoxKeepUntil.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxMatchType", "false");
            setting.Value = checkBoxMatchType.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxName", "false");
            setting.Value = checkBoxName.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxPostrecord", "false");
            setting.Value = checkBoxPostrecord.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxPrerecord", "false");
            setting.Value = checkBoxPrerecord.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxPriority", "false");
            setting.Value = checkBoxPriority.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxRecommendedCard", "false");
            setting.Value = checkBoxRecommendedCard.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxRecordtype", "false");
            setting.Value = checkBoxRecordtype.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxSkip", "false");
            setting.Value = checkBoxSkip.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxSeriesNumber", "false");
            setting.Value = checkBoxSeriesNumber.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxUseFolderNames", "false");
            setting.Value = checkBoxUseFolderNames.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxWithinNextHours", "false");
            setting.Value = checkBoxWithinNextHours.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodeCriteria", "false");
            setting.Value = checkBoxEpisodeCriteria.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxPreferredGroup", "false");
            setting.Value = checkBoxPreferredGroup.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxIncludeRecordings", "false");
            setting.Value = checkBoxIncludeRecordings.Checked.ToString();
            setting.Persist();

            //modify for listview table changes

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxDisableInfoWindow", "false");
            setting.Value = checkBoxDisableInfoWindow.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_checkBoxDisableQuickMenu", "false");
            setting.Value = checkBoxDisableQuickMenu.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_radioButtonEasy", "false");
            setting.Value = radioButtonEasy.Checked.ToString();
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_radioButtonExpert", "false");
            setting.Value = radioButtonExpert.Checked.ToString();
            setting.Persist();

            //setting = layer.GetSetting("TvWishList_ClientSetting_TvwishlistFolder", "false");filenametextBox.Text);

            setting = layer.GetSetting("TvWishList_ClientSetting_DateTimeFormat", "");
            setting.Value = textBoxDateTimeFormat.Text;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_MainItemFormat", "");
            setting.Value = textBoxMainItemFormat.Text;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_EmailMainFormat", "");
            setting.Value = textBoxEmailMainFormat.Text;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_EmailResultsFormat", "");
            setting.Value = textBoxEmailResultsFormat.Text;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_ResultsItemFormat", "");
            setting.Value = textBoxResultsItemFormat.Text;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_ViewMainFormat", "");
            setting.Value = textBoxViewMainFormat.Text;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ClientSetting_ViewResultsFormat", "");
            setting.Value = textBoxViewResultsFormat.Text;
            setting.Persist();

            try
            {
                int test = Convert.ToInt32(textBoxTimeOut.Text);
            }
            catch
            {
                textBoxTimeOut.Text = TimeOutDefault;
            }
            setting = layer.GetSetting("TvWishList_ClientSetting_TimeOut", textBoxTimeOut.Text);
            setting.Persist();

            //store default formats
            string defaultformats = SaveDefaultFormatsToString();

            setting = layer.GetSetting("TvWishList_ClientSetting_DefaultFormats", "");
            setting.Value = defaultformats;
            setting.Persist();

                            
        }

        private bool LoadSettingsFromTvServer()
        {
            
            //get client settings from Tv server
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;

            //debug test
            setting = layer.GetSetting("TvWishList_test", "false");
            setting.Value = "true";
            setting.Persist();

            setting = layer.GetSetting("TvWishList_test", "false");
            Log.Debug("testsetting is "+setting.Value);

            if (setting.Value == "false")
            {
                Log.Error("no connection to tv server database");
                return false;
            }

            setting.Value = "false";
            setting.Persist();
            setting = layer.GetSetting("TvWishList_test", "false");
            Log.Debug("false testsetting is " + setting.Value);
            //end debug test
                
            checkBoxDebug.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxDebug", "false").Value);
            checkBoxAction.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxAction", "true").Value);
            checkBoxActive.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxActive", "false").Value);
            checkBoxAfterDays.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxAfterDays", "false").Value);
            checkBoxAfterTime.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxAfterTime", "false").Value);
            checkBoxBeforeDay.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxBeforeDay", "false").Value);
            checkBoxBeforeTime.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxBeforeTime", "false").Value);
            checkBoxChannel.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxChannel", "true").Value);
            checkBoxEpisodeName.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodeName", "false").Value);
            checkBoxEpisodeNumber.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodeNumber", "false").Value);
            checkBoxEpisodePart.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodePart", "false").Value);
            checkBoxExclude.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxExclude", "true").Value);
            checkBoxGroup.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxGroup", "true").Value);
            //checkBoxHits.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxHits", "true").Value);
            checkBoxKeepEpisodes.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxKeepEpisodes", "false").Value);
            checkBoxKeepUntil.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxKeepUntil", "false").Value);
            checkBoxMatchType.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxMatchType", "true").Value);
            checkBoxName.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxName", "true").Value);
            checkBoxPostrecord.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxPostrecord", "false").Value);
            checkBoxPrerecord.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxPrerecord", "false").Value);
            checkBoxPriority.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxPriority", "false").Value);
            checkBoxRecommendedCard.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxRecommendedCard", "false").Value);
            checkBoxRecordtype.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxRecordtype", "true").Value);
            // SearchFor is always true;
            checkBoxSeriesNumber.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxSeriesNumber", "false").Value);
            checkBoxSkip.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxSkip", "true").Value);
            checkBoxUseFolderNames.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxUseFolderNames", "false").Value);
            checkBoxWithinNextHours.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxRecordtype", "false").Value);
            checkBoxEpisodeCriteria.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxEpisodeCriteria", "false").Value);
            checkBoxPreferredGroup.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxPreferredGroup", "false").Value);
            checkBoxIncludeRecordings.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxIncludeRecordings", "false").Value);
            //modify for listview table changes

            checkBoxDisableInfoWindow.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxDisableInfoWindow", "false").Value);
            checkBoxDisableQuickMenu.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_checkBoxDisableQuickMenu", "false").Value);

            radioButtonEasy.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_radioButtonEasy", "true").Value);
            radioButtonExpert.Checked = Convert.ToBoolean(layer.GetSetting("TvWishList_ClientSetting_radioButtonExpert", "false").Value);

            //filenametextBox.Text=layer.GetSetting("TvWishList_ClientSetting_TvwishlistFolder", "");
            //Log.Debug("[TVWishListMP] TvWishListSetup: loadsettings: " + filenametextBox.Text);

            textBoxDateTimeFormat.Text = layer.GetSetting("TvWishList_ClientSetting_DateTimeFormat", "").Value;
            textBoxMainItemFormat.Text = layer.GetSetting("TvWishList_ClientSetting_MainItemFormat", "").Value;
            textBoxEmailMainFormat.Text = layer.GetSetting("TvWishList_ClientSetting_EmailMainFormat", "").Value;
            textBoxEmailResultsFormat.Text = layer.GetSetting("TvWishList_ClientSetting_EmailResultsFormat", "").Value;
            textBoxResultsItemFormat.Text = layer.GetSetting("TvWishList_ClientSetting_ResultsItemFormat", "").Value;
            textBoxViewMainFormat.Text = layer.GetSetting("TvWishList_ClientSetting_ViewMainFormat", "").Value;
            textBoxViewResultsFormat.Text = layer.GetSetting("TvWishList_ClientSetting_ViewResultsFormat", "").Value;

            textBoxTimeOut.Text = layer.GetSetting("TvWishList_ClientSetting_TimeOut", "60").Value;

            //load defaultformats
            string defaultformatstring = layer.GetSetting("TvWishList_ClientSetting_DefaultFormats", "").Value; //Complete User default in English
            LoadDefaultFormatsFromString(defaultformatstring);

            return true;
        }
        

    }



}
