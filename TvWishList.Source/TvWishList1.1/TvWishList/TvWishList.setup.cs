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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates; 
using System.Security.Cryptography;
using System.Net.Mail;
using System.Windows.Forms;
using System.Xml;
using similaritymetrics;

using TvLibrary.Log.huha;
using TvControl;
using SetupTv;
using TvEngine;
using TvEngine.Events;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvDatabase;
using MediaPortal.Plugins;
using TvEngine.PowerScheduler.Interfaces;

using MyTVMail;
using TvWishList;

/*
Automatic
None
Descr.
Part
Name
Number
Descr.+Part
Descr.+Name
Descr.+Number
Part+Name
Part+Number
Name+Number
Descr.+Part+Name
Descr.+Part+Number
Descr.+Name+Number
Part+Name+Number
Descr.+Part+Name+Number
 */


namespace SetupTv.Sections
{
    
    [CLSCompliant(false)]
    public partial class TvWishListSetup : SetupTv.SectionSettings
    {
        int ProviderLength = 200;  //max number of provider settings
        string[] providers = new string[20];
        string[] array_Eventformats ;
        
        static public string TV_USER_FOLDER = "NOT_FOUND";
        bool DEBUG = false;
        int SELECTED_COL = 1;
        int SELECTED_ROW = 1;
        public bool BUSY = false;
        char TV_WISH_COLUMN_SEPARATOR = ';';
        string PRERECORD = "5";
        string POSTRECORD = "5";

        bool LoadSettingError = false;

        

        //string DEFAULT_EMAILFORMAT = "NEW ENTRY<br>{19}<br>Title={0}<br>Description={1}<br>Channel={2}<br>Start={3}<br>End={4}<br>Type={15}<br>Created={17}<br><br><br>";
        //string DEFAULT_DATETIMEFORMAT = "{1:00}/{2:00} at {3:00}:{4:00}";
        XmlMessages mymessage;
        TvWishProcessing myTvWishes;
        LanguageTranslation lng = new LanguageTranslation();
        string[] ReverseLanguageFileTranslator_File;
        string[] ReverseLanguageFileTranslator_Language;

        

        enum LogSetting
        {
            DEBUG=1,
            ERROR,
            ERRORONLY,
            INFO,
            MESSAGE,
            ADDRESPONSE
        }

        InstallPaths instpaths = new InstallPaths();  //define new instance for folder detection
        EpgParser epgwatchclass = new EpgParser();
        
        #region Constructor


        public TvWishListSetup()
        {
            InitializeComponent();
            mymessage = new XmlMessages("", "",true);
            myTvWishes = new TvWishProcessing();
            Log.Debug("TvWishListSetup()");
        }
        #endregion Constructor

        public void dispose()
        {
            //unlock TvWishList
            Log.Debug("TvWishList Setup Dispose");
            myTvWishes.UnLockTvWishList();
        }

        #region SetupTv.SectionSettings
        public override void OnSectionActivated()
        {
            //set Debug flag first
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;
            setting = layer.GetSetting("TvWishList_Debug", "false");
            if (Convert.ToBoolean(setting.Value) == true)
                checkBoxDebug.Checked = true;
            else
                checkBoxDebug.Checked = false;

            DEBUG = checkBoxDebug.Checked;
            Log.DebugValue = DEBUG;

            LogDebug("TvWishList: Configuration activated", (int)LogSetting.DEBUG);
            if (epgwatchclass != null)
            {
                epgwatchclass.newlabelmessage += new setuplabelmessage(labelupdate);
            }
            //dataGridView1.RowsAdded += new DataGridViewRowsAddedEventHandler(dataGridView1_RowsAdded);

            
            // set actual groups from TV server


            DataGridViewComboBoxColumn mygroupcomboxcolumn = dataGridView1.Columns[3] as DataGridViewComboBoxColumn;
            DataGridViewComboBoxColumn mypreferredgroupcomboxcolumn = dataGridView1.Columns[34] as DataGridViewComboBoxColumn;
            //LogDebug("2) Group Item Count=" + mygroupcomboxcolumn.Items.Count.ToString(), (int)LogSetting.DEBUG);

            if (mygroupcomboxcolumn.Items.Count == 1)
            {
                foreach (ChannelGroup channelgroup in ChannelGroup.ListAll())
                {
                    if (channelgroup.GroupName != "All Channels")
                    {
                        mygroupcomboxcolumn.Items.AddRange(new object[] { channelgroup.GroupName });
                        mypreferredgroupcomboxcolumn.Items.AddRange(new object[] { channelgroup.GroupName });
                    }
                }
                foreach (RadioChannelGroup radiochannelgroup in RadioChannelGroup.ListAll())
                {
                    if (radiochannelgroup.GroupName != "All Channels")
                    {
                        mygroupcomboxcolumn.Items.AddRange(new object[] { radiochannelgroup.GroupName });
                        mypreferredgroupcomboxcolumn.Items.AddRange(new object[] { radiochannelgroup.GroupName });
                    }
                }
            }

            //channel group drop box at the bottom of the GUI
            if (comboBoxGroups.Items.Count == 1)
            {
                foreach (ChannelGroup channelgroup in ChannelGroup.ListAll())
                {
                    if (channelgroup.GroupName != "All Channels")
                    {
                        comboBoxGroups.Items.AddRange(new object[] { channelgroup.GroupName });
                    }
                }
                foreach (RadioChannelGroup radiochannelgroup in RadioChannelGroup.ListAll())
                {
                    if (radiochannelgroup.GroupName != "All Channels")
                    {
                        comboBoxGroups.Items.AddRange(new object[] { radiochannelgroup.GroupName });
                    }
                }
            }


            




            //LogDebug("3)After Group Item Count=" + mygroupcomboxcolumn.Items.Count.ToString(), (int)LogSetting.DEBUG);

            //dataGridView1.Columns.RemoveAt(3);
            //dataGridView1.Columns.Insert(3, mygroupcomboxcolumn);
            

            //set tv cards
#if(TV100)
          IList cards = Card.ListAll();
#elif(TV101)
          IList<Card> cards = Card.ListAll();
#elif(TV11)
            IList<Card> cards = Card.ListAll();
#elif(TV12)
            IList<Card> cards = Card.ListAll();
#endif

            DataGridViewComboBoxColumn mycomboxcolumn = dataGridView1.Columns[16] as DataGridViewComboBoxColumn;
            //LogDebug("Card Item Count=" + mycomboxcolumn.Items.Count.ToString(), (int)LogSetting.DEBUG);

            if (mycomboxcolumn.Items.Count == 1)
            {
                
                foreach (Card card in cards)
                {
                    mycomboxcolumn.Items.AddRange(new object[] { card.IdCard.ToString() });
                }
            }


            SetupLanguages(); //must come before loadsettings

            

            try
            {
                
                //*****************************************************
                //Lock TvWishList with timeout error
                bool success = false;
                //int seconds = 60;

                success = myTvWishes.LockTvWishList("TvWishList Setup"); 
                if (!success)
                {
                    setting = layer.GetSetting("TvWishList_LockingPluginname", "Not Defined");
                    MessageBox.Show(lng.TranslateString("Waiting for old jobs to finish from\n" + setting.Value + "\nTry again later\nIf jobs do not finish you have to reboot or manually stop the Tvserver", 250, setting.Value), lng.TranslateString("Closing Tv Configuration",251));
                    Log.Debug("Waiting for old jobs to finish from\n" + setting.Value + "\nTry again later\nIf jobs do not finish you have to reboot or manually stop the Tvserver");
                    Application.Exit();                    
                    //application is not closing immediately!!
                    return;
                }


                MyLoadSettings();
                

                //enable SetupTV process filewatcher loop after locking
                string filename = TV_USER_FOLDER + @"\TvWishList\SetupTvStarted.txt";
                Log.Debug("trying to create file "+filename);
                if (!File.Exists(filename))
                {
                    try
                    {
                        File.WriteAllText(filename, "SetupTvStarted");
                        Log.Debug("file created");
                    }
                    catch (Exception ex)
                    {
                        LogDebug("Fatal Error: Failed to delete file " + filename + " - exception message is\n" + ex.Message, (int)LogSetting.ERROR);
                    }
                }


            }
            catch (Exception ex)
            {
                LogDebug("Fatal Error: Failed to load settings - exception message is\n" + ex.Message, (int)LogSetting.ERROR);
                LogDebug("Trying to resave settings to data base", (int)LogSetting.ERROR);
                try
                {
                    MySaveSettings();
                    LogDebug("Saving settings succeeded", (int)LogSetting.ERROR);
                }
                catch
                {
                    LogDebug("Fatal Error: Faileed to save settings", (int)LogSetting.ERROR);
                }
            }


            //language file drop box
            Log.Debug("start comboBoxLanguage.Items.Count="+comboBoxLanguage.Items.Count.ToString());
            comboBoxLanguage.Items.Clear();
            string languageFolder = TV_USER_FOLDER + @"\TvWishList\Languages";
            if (Directory.Exists(languageFolder) == false)
            {
                Log.Error("language folder " + languageFolder + " does not exist");
                comboBoxLanguage.Items.Add("Error: No Language files found");
            }
            else
            {
                try
                {
                    DirectoryInfo dirinfo = new DirectoryInfo(languageFolder);
                    FileInfo[] allfiles = dirinfo.GetFiles("string*.xml");
                    ReverseLanguageFileTranslator_File = new string[allfiles.Length];
                    ReverseLanguageFileTranslator_Language = new string[allfiles.Length];
                    int ctr = 0;
                    foreach (FileInfo myfileinfo in allfiles)
                    {
                        ReverseLanguageFileTranslator_File[ctr] = myfileinfo.Name;
                        ReverseLanguageFileTranslator_Language[ctr] = LanguageFileTranslation(myfileinfo.Name);
                        comboBoxLanguage.Items.Add(ReverseLanguageFileTranslator_Language[ctr]);
                        ctr++;
                    }                  
                }
                catch (Exception exc)
                {
                    Log.Error("ProcessFolder Exception: " + exc.Message);
                }
            }
            Log.Debug("end comboBoxLanguage.Items.Count=" + comboBoxLanguage.Items.Count.ToString());


            


            base.OnSectionActivated();
            
        }

        public override void OnSectionDeActivated()
        {
            LogDebug("TvWishList: Configuration deactivated", (int)LogSetting.DEBUG);
            if (epgwatchclass != null)
            {
                epgwatchclass.newlabelmessage -= new setuplabelmessage(labelupdate);
            }
            //dataGridView1.RowsAdded -= new DataGridViewRowsAddedEventHandler(dataGridView1_RowsAdded);
            try
            {
                MySaveSettings();
            }
            catch (Exception ex)
            {
                LogDebug("Fatal Error: Failed to save settings - exception message is\n" + ex.Message, (int)LogSetting.ERROR);
            }

            //*****************************************************
            //unlock TvWishList
            myTvWishes.UnLockTvWishList();

            //disable SetupTV process filewatcher loop
            string filename = TV_USER_FOLDER + @"\TvWishList\SetupTvStarted.txt";
            if (File.Exists(filename))
            {
                try
                {
                    File.Delete(filename);
                }
                catch (Exception ex)
                {
                    LogDebug("Fatal Error: Failed to delete file "+filename+" - exception message is\n" + ex.Message, (int)LogSetting.ERROR);
                }
            }
        }

        public void SetupLanguages()
        {
            

            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;

            //check for installation folders first
            setting = layer.GetSetting("TvWishList_TV_USER_FOLDER", "NOT_FOUND");
            TV_USER_FOLDER = setting.Value;
            if ((File.Exists(TV_USER_FOLDER + @"\TvService.exe") == true) || (Directory.Exists(TV_USER_FOLDER) == false))
            {
                //autodetect paths
                instpaths.GetInstallPaths();
                TV_USER_FOLDER = instpaths.ask_TV_USER();
                LogDebug("TV server user folder detected at " + TV_USER_FOLDER, (int)LogSetting.DEBUG);

                if ((File.Exists(TV_USER_FOLDER + @"\TvService.exe") == true) || (Directory.Exists(TV_USER_FOLDER) == false))
                {
                    LogDebug(@" TV server user folder does not exist - using C:\MediaPortal", (int)LogSetting.ERROR);
                    TV_USER_FOLDER = @"C:\MediaPortal";
                    if (Directory.Exists(TV_USER_FOLDER) == false)
                        Directory.CreateDirectory(TV_USER_FOLDER + @"\TvWishList");
                }
            }

            //checkboxes
            setting = layer.GetSetting("TvWishList_Debug", "false");
            if (Convert.ToBoolean(setting.Value) == true)
                checkBoxDebug.Checked = true;
            else
                checkBoxDebug.Checked = false;

            DEBUG = checkBoxDebug.Checked;
            Log.DebugValue = DEBUG;

            LogDebug("Loadsettings Debug set", (int)LogSetting.DEBUG);

            if (lng.ReadLanguageFile() == false)
                return;

            tabControl1.TabPages[0].Text = lng.TranslateString("Main", 100);

            radioButtonEasy.Text = lng.TranslateString("Easy", 101);
            radioButtonExpert.Text = lng.TranslateString("Expert", 102);
            buttonUp.Text = lng.TranslateString("Up", 103);
            buttonDown.Text = lng.TranslateString("Down", 104);
            buttonDelete.Text = lng.TranslateString("Delete", 105);
            buttonCancel.Text = lng.TranslateString("Cancel", 106);
            buttonDefault.Text = lng.TranslateString("Default", 107);
            buttonhelp.Text = lng.TranslateString("Help", 108);
            checkBoxDebug.Text = lng.TranslateString("Debugmodus", 109);
            linkLabel1.Text = lng.TranslateString("Home Page", 110);
            buttonallactive.Text = lng.TranslateString("All Active", 111);
            buttonallinactive.Text = lng.TranslateString("All Inactive", 112);
            buttonSkipAll.Text = lng.TranslateString("Skip All", 113);
            buttonSkipNone.Text = lng.TranslateString("Skip None", 114);
            label11.Text = lng.TranslateString("Channel Filter", 115);

            tabControl1.TabPages[1].Text = lng.TranslateString("Email", 130);
            groupBox2.Text = lng.TranslateString("Account", 131);
            label16.Text = lng.TranslateString("User Name", 132);
            label18.Text = lng.TranslateString("Password", 133);
            label1.Text = lng.TranslateString("Repeat Password", 134);
            label4.Text = lng.TranslateString("Email Address", 135);
            label2.Text = lng.TranslateString("only if different than user name", 136);
            buttontest.Text = lng.TranslateString("Search Now", 137);
            groupBox1.Text = lng.TranslateString("Address", 138);
            label14.Text = lng.TranslateString("Email Receiver", 139);
            buttonTestSend.Text = lng.TranslateString("Test Send Mail", 140);
            groupBox4.Text = lng.TranslateString("SMTP", 141);
            label13.Text = lng.TranslateString("Provider", 142);
            label8.Text = lng.TranslateString("Port", 143);
            label9.Text = lng.TranslateString("SMTP Server", 144);
            checkBoxSSL.Text = lng.TranslateString("SSL", 145);
            checkBoxemailreply.Text = lng.TranslateString("Enable Email Reply", 146);
            checkBoxEmailOnlynew.Text = lng.TranslateString("Email Only New", 147);
            labelstatus.Text = lng.TranslateString("Ready", 20);
            labelmainstatus.Text = lng.TranslateString("Ready", 20);

            tabControl1.TabPages[2].Text = lng.TranslateString("EPG", 193);
            groupBox3.Text = lng.TranslateString("Check EPG", 161);
            checkBoxMon.Text = lng.TranslateString("Monday", 162);
            checkBoxTue.Text = lng.TranslateString("Tuesday", 163);
            checkBoxWed.Text = lng.TranslateString("Wednesday", 164);
            checkBoxThur.Text = lng.TranslateString("Thursday", 165);
            checkBoxFri.Text = lng.TranslateString("Friday", 166);
            checkBoxSat.Text = lng.TranslateString("Saturday", 167);
            checkBoxSun.Text = lng.TranslateString("Sunday", 168);
            checkBoxEvery.Text = lng.TranslateString("Every", 169);
            label5.Text = lng.TranslateString("Days", 170);
            label7.Text = lng.TranslateString("Hours", 171);
            label10.Text = lng.TranslateString("Minutes", 172);
            label26.Text = lng.TranslateString("Minutes", 172);
            label6.Text = lng.TranslateString("At", 173);
            checkBoxScheduleMinutes.Text = lng.TranslateString("Check EPG", 174);
            label27.Text = lng.TranslateString("before each schedule", 175);
            label15.Text = lng.TranslateString("Next Tv Server Checking Date:", 176);
            groupBox6.Text = lng.TranslateString("Match Criteria for Repeated Movies and Episodes besides the title", 177);
            label12.Text = lng.TranslateString("EPG Repeater Mark in Title or Description", 178);
            groupBox5.Text = lng.TranslateString("Other Options", 179);
            checkBoxschedule.Text = lng.TranslateString("Automatic Recording", 180);
            checkBoxscheduleconflicts.Text = lng.TranslateString("Schedule Conflicts", 181);
            checkBoxDeleteChangedEPG.Text = lng.TranslateString("Delete Changed EPG", 182);
            checkBoxResetEmailFormat.Text = lng.TranslateString("Restore Default Values", 183);
            checkBoxSlowCPU.Text = lng.TranslateString("Slow CPU", 184);
            checkBoxSkipDeleted.Text = lng.TranslateString("Skip Deleted", 185);
            label17.Text = lng.TranslateString("Deleted Expiration:", 186);
            label19.Text = lng.TranslateString("Months", 187);
            label32.Text = lng.TranslateString("Max Schedules Per Wish", 188);
            label28.Text = lng.TranslateString("Minutes", 172);
            label20.Text = lng.TranslateString("ComSkip Wait Time", 192);
            tabControl1.TabPages[3].Text = lng.TranslateString("Settings", 160);
            


            tabControl1.TabPages[4].Text = lng.TranslateString("Formats", 200);
            label23.Text = lng.TranslateString("Expert users can define customized formats for sent emails", 205);
            label25.Text = lng.TranslateString("Date and Time Format", 201);
            //label20.Text = lng.TranslateString("Email Format", 202);
            label24.Text = lng.TranslateString("Language File", 203);
            label21.Text = lng.TranslateString("Sort Criteria", 204);
            checkBoxdescendingSort.Text = lng.TranslateString("Descending Sort", 206);
            label22.Text = lng.TranslateString("Filter Criteria", 207);
            checkBoxFilterEmail.Text = lng.TranslateString("Email", 208);
            checkBoxFilterRecord.Text = lng.TranslateString("Record", 209);
            checkBoxFilterConflicts.Text = lng.TranslateString("Conflicts", 210);
            checkBoxDeletemessageFile.Text = lng.TranslateString("Delete All Messages", 211);


            //Match Type 2600-2606
            DataGridViewComboBoxColumn mygroupcomboxcolumn2 = dataGridView1.Columns[2] as DataGridViewComboBoxColumn;
            for (int i = 0; i <= 6; i++)
            {
                mygroupcomboxcolumn2.Items[i] = lng.Get(2600 + i);     
                //Log.Debug(" mygroupcomboxcolumn2.Items["+i.ToString()+"]="+ mygroupcomboxcolumn2.Items[i].ToString());
            }

            //Group 
            DataGridViewComboBoxColumn mygroupcomboxcolumn3 = dataGridView1.Columns[3] as DataGridViewComboBoxColumn;
            mygroupcomboxcolumn3.Items[0] = lng.Get(4104); //"All Channels"

            
            //Record type 2650-2655
            DataGridViewComboBoxColumn mygroupcomboxcolumn4 = dataGridView1.Columns[4] as DataGridViewComboBoxColumn;
            for (int i = 0; i <= 5; i++)
            {
                mygroupcomboxcolumn4.Items[i] = lng.Get(2650 + i);
            }

            //Action 2700-2702 // do not use view 2703!
            DataGridViewComboBoxColumn mygroupcomboxcolumn5 = dataGridView1.Columns[5] as DataGridViewComboBoxColumn;
            for (int i = 0; i <= 2; i++)
            {
                mygroupcomboxcolumn5.Items[i] = lng.Get(2700 + i);
            }

            //Recommendedcard Any
            DataGridViewComboBoxColumn mygroupcomboxcolumn16 = dataGridView1.Columns[16] as DataGridViewComboBoxColumn;
            mygroupcomboxcolumn16.Items[0] = lng.Get(4100); //Any
            
            //Days 2750-2756
            DataGridViewComboBoxColumn mygroupcomboxcolumn20 = dataGridView1.Columns[20] as DataGridViewComboBoxColumn;
            mygroupcomboxcolumn20.Items[0] = lng.Get(4100); //Any
            for (int i = 0; i <= 6; i++)
            {
                mygroupcomboxcolumn20.Items[i+1] = lng.Get(2750 + i);
            }
            DataGridViewComboBoxColumn mygroupcomboxcolumn21 = dataGridView1.Columns[21] as DataGridViewComboBoxColumn;
            mygroupcomboxcolumn21.Items[0] = lng.Get(4100); //Any
            for (int i = 0; i <= 6; i++)
            {
                mygroupcomboxcolumn21.Items[i+1] = lng.Get(2750 + i);
            }

            //Channel
            DataGridViewComboBoxColumn mygroupcomboxcolumn22 = dataGridView1.Columns[22] as DataGridViewComboBoxColumn;
            mygroupcomboxcolumn22.Items[0] = lng.Get(4100); //Any

            
            //Use name Folder 2850-2853
            DataGridViewComboBoxColumn mygroupcomboxcolumn25 = dataGridView1.Columns[25] as DataGridViewComboBoxColumn;
            for (int i = 0; i <= 3; i++)
            {
                mygroupcomboxcolumn25.Items[i] = lng.Get(2850 + i);
            }

            //Keep Until 2900-2906  --> not used a combobox in tv server

            //Change Episode Matching Criteria USE SHORT NAMES!
            DataGridViewComboBoxColumn mygroupcomboxcolumn33 = dataGridView1.Columns[33] as DataGridViewComboBoxColumn;
            
            mygroupcomboxcolumn33.Items[0] = lng.Get(3264);//bug do not use 4103, 3264 is used in tvWish.c for translation
            mygroupcomboxcolumn33.Items[1] = lng.Get(2960);
            mygroupcomboxcolumn33.Items[2] = lng.Get(2961);
            mygroupcomboxcolumn33.Items[3] = lng.Get(2962);
            mygroupcomboxcolumn33.Items[4] = lng.Get(2960) + "+" + lng.Get(2961);
            mygroupcomboxcolumn33.Items[5] = lng.Get(2960) + "+" + lng.Get(2962);
            mygroupcomboxcolumn33.Items[6] = lng.Get(2961) + "+" + lng.Get(2962);
            mygroupcomboxcolumn33.Items[7] = lng.Get(2960) + "+" + lng.Get(2961) + "+" + lng.Get(2962);
            /*Log.Debug("Episode Matching Criteria Items for Combobox");
            Log.Debug(lng.Get(3264));
            Log.Debug(lng.Get(2960));
            Log.Debug(lng.Get(2961));
            Log.Debug(lng.Get(2962));
            Log.Debug(lng.Get(2960) + "+" + lng.Get(2961));
            Log.Debug(lng.Get(2960) + "+" + lng.Get(2962));
            Log.Debug(lng.Get(2961) + "+" + lng.Get(2962));
            Log.Debug(lng.Get(2960) + "+" + lng.Get(2961) + "+" + lng.Get(2962));
            Log.Debug("End Episode Matching Criteria Items for Combobox");*/

            //PreferredGroup 
            DataGridViewComboBoxColumn mygroupcomboxcolumn34 = dataGridView1.Columns[34] as DataGridViewComboBoxColumn;
            mygroupcomboxcolumn34.Items[0] = lng.Get(4104); //"All Channels"

            //Column Headers 2800-2835
            dataGridView1.Columns[0].HeaderText = lng.TranslateString("Active", 2800);
            dataGridView1.Columns[1].HeaderText = lng.TranslateString("Search For", 2801);
            dataGridView1.Columns[2].HeaderText = lng.TranslateString("Match Type", 2802);
            dataGridView1.Columns[3].HeaderText = lng.TranslateString("Group", 2803);
            dataGridView1.Columns[4].HeaderText = lng.TranslateString("Record Type", 2804);
            dataGridView1.Columns[5].HeaderText = lng.TranslateString("Action", 2805);
            dataGridView1.Columns[6].HeaderText = lng.TranslateString("Exclude", 2806);
            dataGridView1.Columns[8].HeaderText = lng.TranslateString("Pre Recording Time", 2808);
            dataGridView1.Columns[9].HeaderText = lng.TranslateString("Post Recording Time", 2809);
            dataGridView1.Columns[10].HeaderText = lng.TranslateString("Episode Name", 2810);
            dataGridView1.Columns[11].HeaderText = lng.TranslateString("Episode Part", 2811);
            dataGridView1.Columns[12].HeaderText = lng.TranslateString("Episode Number", 2812);
            dataGridView1.Columns[13].HeaderText = lng.TranslateString("Series Number", 2813);
            dataGridView1.Columns[14].HeaderText = lng.TranslateString("Keep Episodes", 2814);
            dataGridView1.Columns[15].HeaderText = lng.TranslateString("Keep Until", 2815);
            dataGridView1.Columns[16].HeaderText = lng.TranslateString("Recommended Card", 2816);
            dataGridView1.Columns[17].HeaderText = lng.TranslateString("Priority", 2817);
            dataGridView1.Columns[18].HeaderText = lng.TranslateString("After Time", 2818);
            dataGridView1.Columns[19].HeaderText = lng.TranslateString("Before Time", 2819);
            dataGridView1.Columns[20].HeaderText = lng.TranslateString("After Day", 2820);
            dataGridView1.Columns[21].HeaderText = lng.TranslateString("Before Day", 2821);
            dataGridView1.Columns[22].HeaderText = lng.TranslateString("Channel", 2822);
            dataGridView1.Columns[23].HeaderText = lng.TranslateString("Skip", 2823);
            dataGridView1.Columns[24].HeaderText = lng.TranslateString("Name", 2824);
            dataGridView1.Columns[25].HeaderText = lng.TranslateString("Move Recordings to Folder", 2825);
            dataGridView1.Columns[26].HeaderText = lng.TranslateString("Show Only Within The Next Hour(s)", 2826);
            dataGridView1.Columns[33].HeaderText = lng.TranslateString("Change Episode Match Criteria", 2833);
            dataGridView1.Columns[34].HeaderText = lng.TranslateString("Preferred Channel Group", 2834);
            dataGridView1.Columns[35].HeaderText = lng.TranslateString("Including Recordings", 2835);


            //Tv Setup Format Column Box
            for (int i = 1; i <= 13; i++)
            {
                comboBoxSortCriteria.Items[i-1] = lng.Get(3200 + i);
            }
            comboBoxSortCriteria.Text = comboBoxSortCriteria.Items[0].ToString();
        }

        #endregion SetupTv.SectionSettings

        
        

        

        public string trimstring(string substring)
        {
            /* 
             * remove character 13 at end (line return)
             * remove leading and trailing tabs and spaces
            */
            if (substring.Length < 1)
                return substring;

            if (Convert.ToInt16(substring[substring.Length - 1]) == 13)
            {
                substring = substring.Substring(0, substring.Length - 1);
            }

            //remove leading spaces and tabs
            while ((substring.StartsWith(" ")) || (substring.StartsWith("\t")))
            {
                substring = substring.Substring(1, substring.Length - 1);
            }

            //remove trailing spaces and tabs
            while ((substring.EndsWith(" ")) || (substring.EndsWith("\t")))
            {
                substring = substring.Substring(0, substring.Length - 1);
            }
            return substring;



        }

        
        
        private void labelupdate(string message, PipeCommands type)
        {
            int number = 50;
            labelstatus2.Text = "";
            labelmainstatus2.Text = "";
            labelstatus2.Update();
            labelmainstatus2.Update();
            message = message.Replace(@"\n","   "); //no line returns here
            
            if (message.Length <= number-1)
            {
                labelstatus.Text = message;
                labelstatus.Update();
                labelmainstatus.Text = message;
                labelmainstatus.Update();
            }
            else
            {
                for (int i = number-1; i >= 1; i--)
                {
                    if (message[i] == ' ')
                    {
                        number = i;
                        break;
                    }
                }
                //LogDebug("number=" + number.ToString(), (int)LogSetting.DEBUG);
                labelstatus.Text = message.Substring(0, number);
                labelstatus2.Text = message.Substring(number + 1, message.Length - number-1);
                labelstatus.Update();
                labelstatus2.Update();

                labelmainstatus.Text = message.Substring(0, number);
                labelmainstatus2.Text = message.Substring(number + 1, message.Length - number - 1);
                labelmainstatus.Update();
                labelmainstatus2.Update();
            }
            
        }

        private void buttonTestSend_Click(object sender, EventArgs e)
        {
            if (BUSY == true)
            {
                MessageBox.Show(lng.TranslateString("Processing ongoing - please wait for completion",252),lng.TranslateString( "Warning",4401));
                return;
            }
            BUSY = true;
            System.Threading.Thread th = new System.Threading.Thread(TestSend);
            th.Start();
        }

        private void TestSend()
        {
            
            string testreceiver = TextBoxTestReceiver.Text;

            try
            {
                MySaveSettings();
            }
            catch (Exception ex)
            {
                LogDebug("Fatal Error: Failed to save settings - exception message is\n" + ex.Message, (int)LogSetting.ERROR);
                BUSY = false;
                return;
            }

            try
            {
                MyLoadSettings();
            }
            catch (Exception ex)
            {
                LogDebug("Fatal Error: Failed to load settings - exception message is\n" + ex.Message, (int)LogSetting.ERROR);
                BUSY = false;
                return;

            }

            if (TextBoxSmtpServer.Text == "")
            {
                LogDebug("No Smtp Provider specified", (int)LogSetting.ERROR);
                MessageBox.Show(lng.TranslateString("No Smtp Provider specified",008), lng.TranslateString("Error",4305));
                BUSY = false;
                return;
            }
            if (TextBoxUserName.Text == "")
            {
                LogDebug("No User Name specified", (int)LogSetting.ERROR);
                MessageBox.Show(lng.TranslateString("No User Name specified",009), lng.TranslateString("Error", 4305));
                BUSY = false;
                return;
            }
            if (testreceiver == "")
            {
                LogDebug("No receiver for testing specified", (int)LogSetting.ERROR);
                MessageBox.Show(lng.TranslateString("No receiver for testing specified",011), lng.TranslateString("Error", 4305));
                BUSY = false;
                return;
            }

            if (TextBoxPassword.Text != TextBoxPassword2.Text)
            {
                MessageBox.Show(lng.TranslateString("Error: Your password does not match and is reset - please enter password again",023), lng.TranslateString("Error", 4305));
                TextBoxPassword.Text = "";
                TextBoxPassword2.Text = "";
                BUSY = false;
                return;
            }


            if (TextBoxPassword.Text == "")
            {
                LogDebug("No Server Password specified", (int)LogSetting.ERROR);
                MessageBox.Show(lng.TranslateString("No Server Password specified",010), lng.TranslateString("Error", 4305));
                BUSY = false;
                return;
            }


            if (numericUpDownSmtpPort.Value == 0)
            {
                LogDebug("Serverport not defined", (int)LogSetting.ERROR);
                MessageBox.Show(lng.TranslateString("Serverport not defined",015), lng.TranslateString("Error", 4305));
                BUSY = false;
                return;
            }

            try
            {
                IPHostEntry hostIP = Dns.GetHostEntry(TextBoxSmtpServer.Text);
                IPAddress[] addr = hostIP.AddressList;
                LogDebug("SMTP Server exists", (int)LogSetting.DEBUG);
            }
            catch
            {
                LogDebug("SMTP Server does not exist - check the SMTP server name and your internet connection", (int)LogSetting.ERROR);

                MessageBox.Show(lng.TranslateString("Pop3 Server does not exist - check the POP3 server name and your internet connection",014), lng.TranslateString("Error", 4305));
                BUSY = false;
                return;
            }


            // build the email message
            labelstatus.Text = lng.TranslateString("Sending email to {0}",7,testreceiver); //Sending email to {0}
            labelstatus.Update();
            labelmainstatus.Text = lng.TranslateString("Sending email to {0}", 7, testreceiver); //Sending email to {0}
            labelmainstatus.Update();

            SendTvServerEmail sendobject = new SendTvServerEmail(TextBoxSmtpServer.Text, Convert.ToInt32(numericUpDownSmtpPort.Value), checkBoxSSL.Checked, TextBoxUserName.Text, TextBoxPassword.Text, textBoxSmtpEmailAdress.Text);
            sendobject.Debug = DEBUG;
            

            bool ok = sendobject.SendNewEmail(testreceiver, "Testmail from TvServer", "Test Send Mail Button was pressed on "+DateTime.Now.ToString());


            if (ok)
            {
                MessageBox.Show(lng.TranslateString("Email was successfully sent to \n" + testreceiver + "\nCheck your email account", 016), lng.TranslateString("Success", 253));
            }
            else if ((sendobject.ErrorMessage == "TIMEOUT") && (sendobject.Error == true))
            {
                MessageBox.Show(lng.TranslateString("Incorrect Portnumber", 012), lng.TranslateString("Email Send Failed", 254));
                LogDebug("Incorrect Portnumber - Email Send Failed", (int)LogSetting.ERROR);
            }
            else
            {
                MessageBox.Show(lng.TranslateString("Check your username and password\n Server Error was:\n{0}", 262, sendobject.ErrorMessage), lng.TranslateString("Email Send Failed with Errormessage:", 254));
                LogDebug("Check your username and password\n Server Error was:\n" + sendobject.ErrorMessage, (int)LogSetting.ERROR);
            }
            labelstatus.Text = lng.TranslateString("Ready",20);
            labelstatus.Update();
            labelmainstatus.Text = lng.TranslateString("Ready", 20);
            labelmainstatus.Update();
            BUSY = false;

        }

        
        public  void MySaveSettings()
        {
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;

            if (LoadSettingError == true)
            {
                LogDebug("No data saved because of loadsetting error", (int)LogSetting.ERROR);
                return;
            }

            //TV User Folder
            setting = layer.GetSetting("TvWishList_TV_USER_FOLDER", "NOT_FOUND");
            setting.Value = TV_USER_FOLDER;
            setting.Persist();
	        //checkboxes

            setting = layer.GetSetting("TvWishList_Debug", "false");
            if (checkBoxDebug.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            setting.Persist();

            setting = layer.GetSetting("TvWishList_SkipDeleted", "false");
            if (checkBoxSkipDeleted.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            setting.Persist();

	        setting = layer.GetSetting("TvWishList_EmailReply", "true");
            if (checkBoxemailreply.Checked == true)
        	        setting.Value = "true";
            else
        	        setting.Value = "false";
		    setting.Persist();

            setting = layer.GetSetting("TvWishList_Schedule", "true");
            if (checkBoxschedule.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            setting.Persist();

            setting = layer.GetSetting("TvWishList_ScheduleConflicts", "false");
            if (checkBoxscheduleconflicts.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            setting.Persist();

            setting = layer.GetSetting("TvWishList_EmailOnlyNew", "true");
            if (checkBoxEmailOnlynew.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            setting.Persist();

            setting = layer.GetSetting("TvWishList_DeleteTimeChanges", "true");
            if (checkBoxDeleteChangedEPG.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            setting.Persist();

            setting = layer.GetSetting("TvWishList_SlowCPU", "true");
            if (checkBoxSlowCPU.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            setting.Persist();

            setting = layer.GetSetting("TvWishList_FilterEmail", "true");
            if (checkBoxFilterEmail.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            setting.Persist();

            setting = layer.GetSetting("TvWishList_FilterRecord", "false");
            if (checkBoxFilterRecord.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            setting.Persist();

            setting = layer.GetSetting("TvWishList_FilterConflicts", "true");
            if (checkBoxFilterConflicts.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            setting.Persist();

            setting = layer.GetSetting("TvWishList_DescendingSort", "true");
            if (checkBoxdescendingSort.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            setting.Persist();

            setting = layer.GetSetting("TvWishList_Easy", "true");
            if (radioButtonEasy.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            setting.Persist();

            setting = layer.GetSetting("TvWishList_Expert", "false");
            if (radioButtonExpert.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            setting.Persist();

            
            //textboxes

            setting = layer.GetSetting("TvWishList_Sort", "Start");
            setting.Value = SortCriteriaReverseTranslation(comboBoxSortCriteria.Text);
            setting.Persist();

            setting = layer.GetSetting("TvWishList_DateTimeFormat", "{1:00}/{2:00} at {3:00}:{4:00}");
            setting.Value = textBoxDateTimeFormat.Text;
            setting.Persist();


            myTvWishes.save_longsetting(textBoxEmailFormat.Text, "TvWishList_EmailFormat");
            
            
            setting = layer.GetSetting("TvWishList_EpgMarker", "");
            setting.Value = textBoxEpgMark.Text;
            setting.Persist();


            setting = layer.GetSetting("TvWishList_UserName", "");
            setting.Value = trimstring(TextBoxUserName.Text);
		    setting.Persist();

		    if (TextBoxPassword.Text !=TextBoxPassword2.Text)
		    {
			    MessageBox.Show(lng.TranslateString("Error: Your password does not match and is reset - please enter password again",023),lng.TranslateString("Error: Passwords Do Not Match",4305));
			    TextBoxPassword.Text="";
			    TextBoxPassword2.Text="";
		    }

	        setting = layer.GetSetting("TvWishList_Password", "");
            setting.Value = trimstring(TextBoxPassword.Text);
		    setting.Persist();

            if (textBoxSmtpEmailAdress.Text == "")
            {
                textBoxSmtpEmailAdress.Text = TextBoxUserName.Text;
            }

            setting = layer.GetSetting("TvWishList_SmtpEmailAddress", "");
            setting.Value = trimstring(textBoxSmtpEmailAdress.Text);
            setting.Persist();

	        setting = layer.GetSetting("TvWishList_TestReceiver", "");
            setting.Value = trimstring(TextBoxTestReceiver.Text);
		    setting.Persist();

            //arrays        	
            try
            {
                try
                {
                    providerupdate(0);  //store last settings as first provider defaults
                }
                catch 
                {//ignore errors
			        LogDebug("Last user settings could not be stored", (int)LogSetting.ERROR);
                }
                for (int i=0; i< listBoxProvider2.Items.Count; i++)  //last settings are stored at position 0 for custom provider
	            {
                    setting = layer.GetSetting("TvWishList_Providers_"+i.ToString(), ";;;");
                    //LogDebug("provider_"+i.ToString()+" = "+providers[i]);
                    setting.Value = providers[i];
                    if (setting.Value == null)
                    {
                        setting.Value = ";;;";
                        
                    }
		            setting.Persist();
	            }
                
	        }
            catch (Exception ex)
            {
                LogDebug("Failed to write Provider ListBox to data base - exception message is\n" + ex.Message, (int)LogSetting.ERROR);
            }
            
            //maxfound

            //checkcombotextbox(ref ComboBox mycombobox,string format,int min, int max, string fieldname);
            checkcombotextbox(ref comboBoxmaxfound, "", 1, 1000000000, "Max Found");

            setting = layer.GetSetting("TvWishList_MaxFound", "100");
            setting.Value = comboBoxmaxfound.Text;
            setting.Persist();

            

            

            setting = layer.GetSetting("TvWishList_DeleteExpiration", "12");
            setting.Value = comboBoxDeleteExpiration.Text;
            setting.Persist();


            setting = layer.GetSetting("TvWishList_ChannelGroups", "Any");
            setting.Value = comboBoxGroups.Text;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_LanguageFile", "strings_en.xml");
            setting.Value = ReverseLanguageFileTranslation(comboBoxLanguage.Text);            
            setting.Persist();

            //saving messages
            //update message.tvwishid if tvwish has been deleted or is unknown
            Log.Debug("tv setup window before updating messages: TvMessages.Count=" + mymessage.ListAllTvMessages().Count.ToString());
            for (int i = mymessage.ListAllTvMessages().Count - 1; i >= 0; i--)
            {
                xmlmessage onemessage = mymessage.GetTvMessageAtIndex(i);
                //Log.Debug("onemessage.tvwishid="+onemessage.tvwishid);




                TvWish mywish = myTvWishes.RetrieveById(onemessage.tvwishid);
                if ((mywish == null)&&(onemessage.tvwishid!="-1"))  //allow -1 for general conflicts
                {
                    Log.Debug("deleting " + onemessage.title + " at " + onemessage.start.ToString() + " ID: " + onemessage.tvwishid);
                    mymessage.DeleteTvMessageAt(i);
                }
                /*if ((mywish == null) && (mymessage.tvwishid != "-1"))
                {
                    Log.Debug("Changing for " + mymessage.title + " at " + mymessage.start.ToString() + " from " + mymessage.tvwishid + " to -1");
                    mymessage.tvwishid = "-1";
                    mymessages.TvMessages[i] = mymessage;
                }*/
            }
            Log.Debug("tv setup window after updating messages: TvMessages.Count=" + mymessage.ListAllTvMessages().Count.ToString());
            string dataString = mymessage.writexmlfile(false); //write xml file to string
            myTvWishes.save_longsetting(dataString, "TvWishList_ListViewMessages");


            
            
            //listview data
            string listviewstring = "";
            myTvWishes.Clear();

            //convert dataGridView1 to tvwishes 
            for (int i=0;i<dataGridView1.Rows.Count-1;i++)
            {
                
                try //add new default row to enable automated upgrades for new formats with more items
                {
                    TvWish mywish = myTvWishes.DefaultData();
                    Log.Debug("i="+i.ToString());


                    Log.Debug("dataGridView1.RowCount=" + dataGridView1.RowCount.ToString());
                    Log.Debug("dataGridView1.ColumnCount=" + dataGridView1.ColumnCount.ToString());


                    try
                    {
                        mywish.active = dataGridView1[(int)TvWishEntries.active, i].Value.ToString();
                    } 
                    catch { }

                    try{
                        mywish.skip = dataGridView1[(int)TvWishEntries.skip, i].Value.ToString();
                    } 
                    catch { }

                    try{
                        mywish.includeRecordings = dataGridView1[(int)TvWishEntries.includerecordings, i].Value.ToString();
                    }
                    catch { }
                    try
                    {
                        mywish.searchfor = dataGridView1[(int)TvWishEntries.searchfor, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.matchtype = dataGridView1[(int)TvWishEntries.matchtype, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.group = dataGridView1[(int)TvWishEntries.group, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.recordtype = dataGridView1[(int)TvWishEntries.recordtype, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.action = dataGridView1[(int)TvWishEntries.action, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.exclude = dataGridView1[(int)TvWishEntries.exclude, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.prerecord = dataGridView1[(int)TvWishEntries.prerecord, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.postrecord = dataGridView1[(int)TvWishEntries.postrecord, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.episodename = dataGridView1[(int)TvWishEntries.episodename, i].Value.ToString();
                    }
                    catch { }


                    try
                    {
                        mywish.episodepart = dataGridView1[(int)TvWishEntries.episodepart, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.episodenumber = dataGridView1[(int)TvWishEntries.episodenumber, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.seriesnumber = dataGridView1[(int)TvWishEntries.seriesnumber, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.keepepisodes = dataGridView1[(int)TvWishEntries.keepepisodes, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.keepuntil = dataGridView1[(int)TvWishEntries.keepuntil, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.recommendedcard = dataGridView1[(int)TvWishEntries.recommendedcard, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.priority = dataGridView1[(int)TvWishEntries.priority, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.aftertime = dataGridView1[(int)TvWishEntries.aftertime, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.beforetime = dataGridView1[(int)TvWishEntries.beforetime, i].Value.ToString();
                    }
                    catch { }


                    try
                    {
                        mywish.afterdays = dataGridView1[(int)TvWishEntries.afterdays, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.beforedays = dataGridView1[(int)TvWishEntries.beforedays, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.channel = dataGridView1[(int)TvWishEntries.channel, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.name = dataGridView1[(int)TvWishEntries.name, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.useFolderName = dataGridView1[(int)TvWishEntries.useFolderName, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.withinNextHours = dataGridView1[(int)TvWishEntries.withinNextHours, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.episodecriteria = dataGridView1[(int)TvWishEntries.episodecriteria, i].Value.ToString();
                    }
                    catch { }

                    try
                    {
                        mywish.preferredgroup = dataGridView1[(int)TvWishEntries.preferredgroup, i].Value.ToString();
                    }
                    catch { }

                    
                    //add tvwishid
                    try
                    {
                        mywish.tvwishid = dataGridView1[(int)TvWishEntries.tvwishid, i].Value.ToString();
                        //decrease MaxTvWishId because new defaultwish id was not used - otherwise tvwishid would be increased during each save action
                        myTvWishes.MaxTvWishId--;
                    }
                    catch { }
                                      
                    myTvWishes.Add(mywish);
                    Log.Debug("Tv wish added");
                }
                catch (Exception exc)
                {
                    LogDebug("Reading row failed with message \n" + exc.Message, (int)LogSetting.ERROR);
                }              
                LogDebug("Tvwish finished", (int)LogSetting.DEBUG);
            } //End all rows in datagridview

            // save to string (includes backtransformation from language)
            listviewstring = myTvWishes.SaveToStringNoChecking();
            Log.Debug("savesettings: after language translation listviewstring=" + listviewstring);
            //check data
            listviewstring = myTvWishes.CheckString(listviewstring);

            Log.Debug("savesettings: after checking listviewstring=" + listviewstring);
            myTvWishes.save_longsetting(listviewstring, "TvWishList_ListView");

            //must be after listviewstring!
            LogDebug("SaveSetings: MaxTvWishId=" + myTvWishes.MaxTvWishId.ToString(), (int)LogSetting.DEBUG);
            setting = layer.GetSetting("TvWishList_MaxTvWishId", "0");
            setting.Value = myTvWishes.MaxTvWishId.ToString();
            setting.Persist();
           
                     

            //integer values
	        
         
	        setting = layer.GetSetting("TvWishList_ProviderSelected", "0");
            int j = listBoxProvider2.SelectedIndex;
            setting.Value = j.ToString();   //select actual item in listbox
		    setting.Persist();

            //comboboxes
            setting = layer.GetSetting("TvWishList_WaitComSkipMinutes", "60");
            try
            {
                int i = Convert.ToInt32(comboBoxComSkipWaitMinutes.Text);
            }
            catch
            {
                comboBoxComSkipWaitMinutes.Text = "60";
            }
            setting.Value = comboBoxComSkipWaitMinutes.Text;
            setting.Persist();

            // check comboboxes before saving
            //checkcombotextbox(ref ComboBox mycombobox,string format,int min, int max, string fieldname);
            checkcombotextbox(ref comboBoxdays, "D2" , 1, 14, "Days");

            //checkcombotextbox(ref ComboBox mycombobox,string format,int min, int max, string fieldname);
            checkcombotextbox(ref comboBoxhours, "D2", 0, 23, "Hours");

            //checkcombotextbox(ref ComboBox mycombobox,string format,int min, int max, string fieldname);
            checkcombotextbox(ref comboBoxminutes, "D2", 0, 59, "Minutes");

            
            


            //calculate new epg checking data
 
            // get old epg checking data
            string old_epg_check = "";
            setting = layer.GetSetting("TvWishList_CheckEpgDays", "07");
            old_epg_check += "days"+setting.Value;
            setting = layer.GetSetting("TvWishList_CheckEpgHours", "06");
            old_epg_check += "hours"+setting.Value;
            setting = layer.GetSetting("TvWishList_CheckEpgMinutes", "00");
            LogDebug("old epg checking data minutes=" + setting.Value, (int)LogSetting.DEBUG);
            old_epg_check += "minutes"+setting.Value;
            setting = layer.GetSetting("TvWishList_Monday", "false");
            old_epg_check += "Monday" + setting.Value;
            setting = layer.GetSetting("TvWishList_Tuesday", "false");
            old_epg_check += "Tuesday" + setting.Value;
            setting = layer.GetSetting("TvWishList_Wednesday", "false");
            old_epg_check += "Wednesday" + setting.Value;
            setting = layer.GetSetting("TvWishList_Thursday", "false");
            old_epg_check += "Thursday" + setting.Value;
            setting = layer.GetSetting("TvWishList_Friday", "false");
            old_epg_check += "Friday" + setting.Value;
            setting = layer.GetSetting("TvWishList_Saturday", "false");
            old_epg_check += "Saturday" + setting.Value;
            setting = layer.GetSetting("TvWishList_Sunday", "false");
            old_epg_check += "Sunday" + setting.Value;
            setting = layer.GetSetting("TvWishList_Every", "false");
            old_epg_check += "Every" + setting.Value;
            setting = layer.GetSetting("TvWishList_CheckEPGScheduleMinutes", "false");
            old_epg_check += "CheckEPGScheduleMinutes" + setting.Value;
            setting = layer.GetSetting("TvWishList_BeforeEPGMinutes", "00");
            old_epg_check += "BeforeEPGMinutes" + setting.Value;

            string new_epg_check = "";

            if ((checkBoxMon.Checked == false) && (checkBoxTue.Checked == false) && (checkBoxWed.Checked == false) && (checkBoxThur.Checked == false) && (checkBoxFri.Checked == false) && (checkBoxSat.Checked == false) && (checkBoxSun.Checked == false) && (checkBoxEvery.Checked == false))
            {
                checkBoxEvery.Checked = true;
                LogDebug("no information about checking days - using every " + comboBoxdays.Text + " days", (int)LogSetting.INFO);
            }

            setting = layer.GetSetting("TvWishList_CheckEpgDays", "07");
            setting.Value = comboBoxdays.Text;
            new_epg_check += "days" + setting.Value;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_CheckEpgHours", "06");
            setting.Value = comboBoxhours.Text;
            new_epg_check += "hours" + setting.Value;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_CheckEpgMinutes", "00");           
            setting.Value = comboBoxminutes.Text;
            LogDebug("new_epg_check minutes=" + setting.Value, (int)LogSetting.DEBUG);
            new_epg_check += "minutes" + setting.Value;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_Monday", "false");
            if (checkBoxMon.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            new_epg_check += "Monday" + setting.Value;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_Tuesday", "false");
            if (checkBoxTue.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            new_epg_check += "Tuesday" + setting.Value;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_Wednesday", "false");
            if (checkBoxWed.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            new_epg_check += "Wednesday" + setting.Value;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_Thursday", "false");
            if (checkBoxThur.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            new_epg_check += "Thursday" + setting.Value;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_Friday", "false");
            if (checkBoxFri.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            new_epg_check += "Friday" + setting.Value;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_Saturday", "false");
            if (checkBoxSat.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            new_epg_check += "Saturday" + setting.Value;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_Sunday", "false");
            if (checkBoxSun.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            new_epg_check += "Sunday" + setting.Value;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_Every", "false");
            if (checkBoxEvery.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            new_epg_check += "Every" + setting.Value;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_CheckEPGScheduleMinutes", "false");
            if (checkBoxScheduleMinutes.Checked == true)
                setting.Value = "true";
            else
                setting.Value = "false";
            new_epg_check += "CheckEPGScheduleMinutes" + setting.Value;
            setting.Persist();

            setting = layer.GetSetting("TvWishList_BeforeEPGMinutes", "00");
            setting.Value = comboBoxScheduleMinutes.Text;
            new_epg_check += "BeforeEPGMinutes" + setting.Value;
            setting.Persist();
            

            LogDebug("old_epg_check=" + old_epg_check, (int)LogSetting.DEBUG);
            LogDebug("new_epg_check=" + new_epg_check, (int)LogSetting.DEBUG);

            //update tv service if a new value is set

            

            if (old_epg_check != new_epg_check)
            {
                string nextEpgFileWatcherFile = TV_USER_FOLDER + @"\TvWishList\NextEPGCheck.txt";
                LogDebug("Writing Filewatcher file " + nextEpgFileWatcherFile, (int)LogSetting.DEBUG);
               //writing file
                try
                {
                    File.WriteAllText(nextEpgFileWatcherFile, "NextEPGCheckUpdate");
                }
                catch (Exception exc)
                {
                    LogDebug("Error in writing epg time file " + nextEpgFileWatcherFile, (int)LogSetting.ERROR);
                    LogDebug(exc.Message, (int)LogSetting.ERROR);                    
                }
            }    


        }

        public void MyLoadSettings()
        {
            
            LogDebug("Loadsettings started", (int)LogSetting.DEBUG);
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;

            try
            {
                
                setting = layer.GetSetting("TvWishList_SkipDeleted", "false");
                if (BoolConversion(setting.Value,false))
                    checkBoxSkipDeleted.Checked = true;
                else
                    checkBoxSkipDeleted.Checked = false;

                setting = layer.GetSetting("TvWishList_EmailReply", "true");
                if (BoolConversion(setting.Value, true))
                    checkBoxemailreply.Checked = true;
                else
                    checkBoxemailreply.Checked = false;


                setting = layer.GetSetting("TvWishList_Schedule", "true");
                if (BoolConversion(setting.Value, true))
                    checkBoxschedule.Checked = true;
                else
                    checkBoxschedule.Checked = false;

                setting = layer.GetSetting("TvWishList_ScheduleConflicts", "false");
                if (BoolConversion(setting.Value, false))
                    checkBoxscheduleconflicts.Checked = true;
                else
                    checkBoxscheduleconflicts.Checked = false;

                setting = layer.GetSetting("TvWishList_EmailOnlyNew", "true");
                if (BoolConversion(setting.Value, true))
                    checkBoxEmailOnlynew.Checked = true;
                else
                    checkBoxEmailOnlynew.Checked = false;

                setting = layer.GetSetting("TvWishList_SlowCPU", "true");
                if (BoolConversion(setting.Value, true))
                    checkBoxSlowCPU.Checked = true;
                else
                    checkBoxSlowCPU.Checked = false;

                setting = layer.GetSetting("TvWishList_Monday", "false");
                if (BoolConversion(setting.Value, false))
                    checkBoxMon.Checked = true;
                else
                    checkBoxMon.Checked = false;

                setting = layer.GetSetting("TvWishList_Tuesday", "false");
                if (BoolConversion(setting.Value, false))
                    checkBoxTue.Checked = true;
                else
                    checkBoxTue.Checked = false;

                setting = layer.GetSetting("TvWishList_Wednesday", "false");
                if (BoolConversion(setting.Value, false))
                    checkBoxWed.Checked = true;
                else
                    checkBoxWed.Checked = false;

                setting = layer.GetSetting("TvWishList_Thursday", "false");
                if (BoolConversion(setting.Value, false))
                    checkBoxThur.Checked = true;
                else
                    checkBoxThur.Checked = false;

                setting = layer.GetSetting("TvWishList_Friday", "false");
                if (BoolConversion(setting.Value, false))
                    checkBoxFri.Checked = true;
                else
                    checkBoxFri.Checked = false;

                setting = layer.GetSetting("TvWishList_Saturday", "false");
                if (BoolConversion(setting.Value, false))
                    checkBoxSat.Checked = true;
                else
                    checkBoxSat.Checked = false;

                setting = layer.GetSetting("TvWishList_Sunday", "false");
                if (BoolConversion(setting.Value, false))
                    checkBoxSun.Checked = true;
                else
                    checkBoxSun.Checked = false;

                setting = layer.GetSetting("TvWishList_Every", "false");
                if (BoolConversion(setting.Value, false))
                    checkBoxEvery.Checked = true;
                else
                    checkBoxEvery.Checked = false;

                setting = layer.GetSetting("TvWishList_DeleteTimeChanges", "false");
                if (BoolConversion(setting.Value, false))
                    checkBoxDeleteChangedEPG.Checked = true;
                else
                    checkBoxDeleteChangedEPG.Checked = false;

                setting = layer.GetSetting("TvWishList_FilterEmail", "true");
                if (BoolConversion(setting.Value, true))
                    checkBoxFilterEmail.Checked = true;
                else
                    checkBoxFilterEmail.Checked = false;

                setting = layer.GetSetting("TvWishList_FilterRecord", "false");
                if (BoolConversion(setting.Value, false))
                    checkBoxFilterRecord.Checked = true;
                else
                    checkBoxFilterRecord.Checked = false;

                setting = layer.GetSetting("TvWishList_FilterConflicts", "true");
                if (BoolConversion(setting.Value, true))
                    checkBoxFilterConflicts.Checked = true;
                else
                    checkBoxFilterConflicts.Checked = false;

                setting = layer.GetSetting("TvWishList_DescendingSort", "false");
                if (BoolConversion(setting.Value, false))
                    checkBoxdescendingSort.Checked = true;
                else
                    checkBoxdescendingSort.Checked = false;

                setting = layer.GetSetting("TvWishList_Easy", "true");
                if (BoolConversion(setting.Value, true))
                {
                    radioButtonEasy.Checked = true;
                }

                setting = layer.GetSetting("TvWishList_Expert", "false");
                if (BoolConversion(setting.Value, false))
                {
                    radioButtonExpert.Checked = true;
                }

                LogDebug("TvWishList_Expert=" + radioButtonExpert.Checked.ToString(), (int)LogSetting.DEBUG);

                setting = layer.GetSetting("TvWishList_DateTimeFormat", "{1:00}/{2:00} at {3:00}:{4:00}");
                textBoxDateTimeFormat.Text = setting.Value;
                //LogDebug("TvWishList_DateTimeFormat=" + setting.Value, (int)LogSetting.DEBUG);

                textBoxEmailFormat.Text = myTvWishes.loadlongsettings("TvWishList_EmailFormat");

                setting = layer.GetSetting("TvWishList_Sort", "Start");
                Log.Debug("TvWishList_Sort=" + setting.Value);
                comboBoxSortCriteria.Text = SortCriteriaTranslation(setting.Value);
                
                
                setting = layer.GetSetting("TvWishList_EpgMarker", "");
                textBoxEpgMark.Text = setting.Value;
                
                setting = layer.GetSetting("TvWishList_LanguageFile", "strings_en.xml");
                comboBoxLanguage.Text = LanguageFileTranslation(setting.Value);
                Log.Debug("TvWishList_LanguageFile="+setting.Value);
                
                setting = layer.GetSetting("TvWishList_UserName", "");
                TextBoxUserName.Text = setting.Value;
                
                setting = layer.GetSetting("TvWishList_Password", "");
                TextBoxPassword.Text = setting.Value;
                TextBoxPassword2.Text = setting.Value;
                
                setting = layer.GetSetting("TvWishList_TestReceiver", "");
                TextBoxTestReceiver.Text = setting.Value;
                
                setting = layer.GetSetting("TvWishList_SmtpEmailAddress", "");
                textBoxSmtpEmailAdress.Text = setting.Value;
                
                if (textBoxSmtpEmailAdress.Text == "")
                {
                    textBoxSmtpEmailAdress.Text = TextBoxUserName.Text;
                }
                //combobox
                
                setting = layer.GetSetting("TvWishList_WaitComSkipMinutes", "60");
                try
                {
                    int i = Convert.ToInt32(setting.Value);
                    comboBoxComSkipWaitMinutes.Text = setting.Value;
                }
                catch
                {
                    comboBoxComSkipWaitMinutes.Text = "60";
                }
                
                setting = layer.GetSetting("TvWishList_CheckEpgDays", "07");
                comboBoxdays.Text = setting.Value;
                //checkcombotextbox(ref ComboBox mycombobox,string format,int min, int max, string fieldname);
                checkcombotextbox(ref comboBoxdays, "D2", 1, 14, "Days");
                
                setting = layer.GetSetting("TvWishList_CheckEpgHours", "06");
                comboBoxhours.Text = setting.Value;
                //checkcombotextbox(ref ComboBox mycombobox,string format,int min, int max, string fieldname);
                checkcombotextbox(ref comboBoxhours, "D2", 0, 23, "Hours");
                
                setting = layer.GetSetting("TvWishList_CheckEpgMinutes", "00");
                comboBoxminutes.Text = setting.Value;
                //checkcombotextbox(ref ComboBox mycombobox,string format,int min, int max, string fieldname);
                checkcombotextbox(ref comboBoxminutes, "D2", 0, 59, "Minutes");
                
                setting = layer.GetSetting("TvWishList_BeforeEPGMinutes", "00");
                comboBoxScheduleMinutes.Text = setting.Value;
                //checkcombotextbox(ref ComboBox mycombobox,string format,int min, int max, string fieldname);
                checkcombotextbox(ref comboBoxScheduleMinutes, "D2", 0, 59, "Minutes");
                
                setting = layer.GetSetting("TvWishList_CheckEPGScheduleMinutes", "false");
                //Log.Debug("TvWishList_CheckEPGScheduleMinutes"+setting.Value);
                try
                {
                    checkBoxScheduleMinutes.Checked = Convert.ToBoolean(setting.Value);
                }
                catch
                {
                    checkBoxScheduleMinutes.Checked = false;
                }
                               
                try
                {
                    setting = layer.GetSetting("TvWishList_ChannelGroups", "Any");
                    comboBoxGroups.Text = setting.Value;
                }
                catch
                {
                    comboBoxGroups.Text = "Any";
                }
                
                //maxfound
                setting = layer.GetSetting("TvWishList_MaxFound", "100");
                comboBoxmaxfound.Text = setting.Value;
                //checkcombotextbox(ref ComboBox mycombobox,string format,int min, int max, string fieldname);
                checkcombotextbox(ref comboBoxmaxfound, "", 1, 1000000000, "Max Found");

                setting = layer.GetSetting("TvWishList_MaxTvWishId", "0");
                myTvWishes.MaxTvWishId = Convert.ToInt32(setting.Value);               
                LogDebug("LoadSettings: MaxTvWishId=" + myTvWishes.MaxTvWishId.ToString(), (int)LogSetting.DEBUG);

                setting = layer.GetSetting("TvWishList_DeleteExpiration", "12");
                comboBoxDeleteExpiration.Text = setting.Value;      
                

                //initialize messages
                string messagedata = "";
                //mymessage = new XmlMessages(mymessage.date_time_format, mymessage.EmailFormat, DEBUG);                
                messagedata = myTvWishes.loadlongsettings("TvWishList_ListViewMessages");                
                mymessage.readxmlfile(messagedata, false);
                Log.Debug(mymessage.ListAllTvMessages().Count.ToString()+" messages read");

                

                setting = layer.GetSetting("TvWishList_ColumnSeparator", ";");
                TV_WISH_COLUMN_SEPARATOR = setting.Value[0];

                //default pre and post record from general recording settings
                setting = layer.GetSetting("preRecordInterval", "5");
                PRERECORD = setting.Value;
                setting = layer.GetSetting("postRecordInterval", "5");
                POSTRECORD = setting.Value;

                //initialize tvserver settings in TvWish for checking of channels
                myTvWishes.TvServerSettings(PRERECORD, POSTRECORD, ChannelGroup.ListAll(), RadioChannelGroup.ListAll(), Channel.ListAll(), Card.ListAll(), TV_WISH_COLUMN_SEPARATOR);


                //listviewdata
                //Log.Debug("Turnuing off eventhandler for adding rows");
                //dataGridView1.RowsAdded -= new DataGridViewRowsAddedEventHandler(dataGridView1_RowsAdded);


                DataGridViewComboBoxColumn channelfilter = dataGridView1.Columns[22] as DataGridViewComboBoxColumn;
                string addnames = ";";
                for (int i = 0; i < channelfilter.Items.Count; i++)
                {
                    addnames += channelfilter.Items[i] + ";" as String;
                }

                //convert defaultdata with language translation
                string[] columndata = (string[])myTvWishes.DefaultValues.Clone();
                TvWish defaultwish = new TvWish();
                defaultwish = myTvWishes.CreateTvWish(true, columndata);
                //Log.Debug("defaultwish with Languagetranslation");
                myTvWishes.DebugTvWish(defaultwish);

               

                //define nullvalues for datagrid with language translation
                DataGridViewCheckBoxColumn active = dataGridView1.Columns[(int)TvWishEntries.active] as DataGridViewCheckBoxColumn;
                active.DefaultCellStyle.NullValue = defaultwish.b_active;
                DataGridViewCheckBoxColumn skip = dataGridView1.Columns[(int)TvWishEntries.skip] as DataGridViewCheckBoxColumn;
                skip.DefaultCellStyle.NullValue = defaultwish.b_skip;
                DataGridViewCheckBoxColumn includerecordings = dataGridView1.Columns[(int)TvWishEntries.includerecordings] as DataGridViewCheckBoxColumn;
                includerecordings.DefaultCellStyle.NullValue = defaultwish.b_includeRecordings;
                DataGridViewTextBoxColumn searchfor = dataGridView1.Columns[(int)TvWishEntries.searchfor] as DataGridViewTextBoxColumn;
                searchfor.DefaultCellStyle.NullValue = defaultwish.searchfor;
                DataGridViewComboBoxColumn matchtype = dataGridView1.Columns[(int)TvWishEntries.matchtype] as DataGridViewComboBoxColumn;
                matchtype.DefaultCellStyle.NullValue = defaultwish.matchtype;
                DataGridViewComboBoxColumn group = dataGridView1.Columns[(int)TvWishEntries.group] as DataGridViewComboBoxColumn;
                group.DefaultCellStyle.NullValue = defaultwish.group;
                DataGridViewComboBoxColumn recordtype = dataGridView1.Columns[(int)TvWishEntries.recordtype] as DataGridViewComboBoxColumn;
                recordtype.DefaultCellStyle.NullValue = defaultwish.recordtype;
                DataGridViewComboBoxColumn action = dataGridView1.Columns[(int)TvWishEntries.action] as DataGridViewComboBoxColumn;
                action.DefaultCellStyle.NullValue = defaultwish.action;
                DataGridViewTextBoxColumn exclude = dataGridView1.Columns[(int)TvWishEntries.exclude] as DataGridViewTextBoxColumn;
                exclude.DefaultCellStyle.NullValue = defaultwish.exclude;
                DataGridViewTextBoxColumn prerecord = dataGridView1.Columns[(int)TvWishEntries.prerecord] as DataGridViewTextBoxColumn;
                prerecord.DefaultCellStyle.NullValue = defaultwish.prerecord;
                DataGridViewTextBoxColumn postrecord = dataGridView1.Columns[(int)TvWishEntries.postrecord] as DataGridViewTextBoxColumn;
                postrecord.DefaultCellStyle.NullValue = defaultwish.postrecord;
                DataGridViewTextBoxColumn episodename = dataGridView1.Columns[(int)TvWishEntries.episodename] as DataGridViewTextBoxColumn;
                episodename.DefaultCellStyle.NullValue = defaultwish.episodename;
                DataGridViewTextBoxColumn episodepart = dataGridView1.Columns[(int)TvWishEntries.episodepart] as DataGridViewTextBoxColumn;
                episodepart.DefaultCellStyle.NullValue = defaultwish.episodepart;
                DataGridViewTextBoxColumn episodenumber = dataGridView1.Columns[(int)TvWishEntries.episodenumber] as DataGridViewTextBoxColumn;
                episodenumber.DefaultCellStyle.NullValue = defaultwish.episodenumber;
                DataGridViewTextBoxColumn seriesnumber = dataGridView1.Columns[(int)TvWishEntries.seriesnumber] as DataGridViewTextBoxColumn;
                seriesnumber.DefaultCellStyle.NullValue = defaultwish.seriesnumber;
                DataGridViewTextBoxColumn keepepisodes = dataGridView1.Columns[(int)TvWishEntries.keepepisodes] as DataGridViewTextBoxColumn;
                keepepisodes.DefaultCellStyle.NullValue = defaultwish.keepepisodes;
                DataGridViewTextBoxColumn keepuntil = dataGridView1.Columns[(int)TvWishEntries.keepuntil] as DataGridViewTextBoxColumn;
                keepuntil.DefaultCellStyle.NullValue = defaultwish.keepuntil;
                DataGridViewComboBoxColumn recommendedcard = dataGridView1.Columns[(int)TvWishEntries.recommendedcard] as DataGridViewComboBoxColumn;
                recommendedcard.DefaultCellStyle.NullValue = defaultwish.recommendedcard;
                DataGridViewComboBoxColumn priority = dataGridView1.Columns[(int)TvWishEntries.priority] as DataGridViewComboBoxColumn;
                priority.DefaultCellStyle.NullValue = defaultwish.priority;
                DataGridViewTextBoxColumn aftertime = dataGridView1.Columns[(int)TvWishEntries.aftertime] as DataGridViewTextBoxColumn;
                aftertime.DefaultCellStyle.NullValue = defaultwish.aftertime;
                DataGridViewTextBoxColumn beforetime = dataGridView1.Columns[(int)TvWishEntries.beforetime] as DataGridViewTextBoxColumn;
                beforetime.DefaultCellStyle.NullValue = defaultwish.beforetime;
                DataGridViewComboBoxColumn afterdays = dataGridView1.Columns[(int)TvWishEntries.afterdays] as DataGridViewComboBoxColumn;
                afterdays.DefaultCellStyle.NullValue = defaultwish.afterdays;
                DataGridViewComboBoxColumn beforedays = dataGridView1.Columns[(int)TvWishEntries.beforedays] as DataGridViewComboBoxColumn;
                beforedays.DefaultCellStyle.NullValue = defaultwish.beforedays;
                DataGridViewComboBoxColumn channel = dataGridView1.Columns[(int)TvWishEntries.channel] as DataGridViewComboBoxColumn;
                channel.DefaultCellStyle.NullValue = defaultwish.channel;
                DataGridViewTextBoxColumn name = dataGridView1.Columns[(int)TvWishEntries.name] as DataGridViewTextBoxColumn;
                name.DefaultCellStyle.NullValue = defaultwish.name;
                DataGridViewComboBoxColumn useFolderName = dataGridView1.Columns[(int)TvWishEntries.useFolderName] as DataGridViewComboBoxColumn;
                useFolderName.DefaultCellStyle.NullValue = defaultwish.useFolderName;
                DataGridViewTextBoxColumn withinNextHours = dataGridView1.Columns[(int)TvWishEntries.withinNextHours] as DataGridViewTextBoxColumn;
                withinNextHours.DefaultCellStyle.NullValue = defaultwish.withinNextHours;
                DataGridViewComboBoxColumn episodecriteria = dataGridView1.Columns[(int)TvWishEntries.episodecriteria] as DataGridViewComboBoxColumn;
                episodecriteria.DefaultCellStyle.NullValue = defaultwish.episodecriteria;
                DataGridViewComboBoxColumn preferredgroup = dataGridView1.Columns[(int)TvWishEntries.preferredgroup] as DataGridViewComboBoxColumn;
                preferredgroup.DefaultCellStyle.NullValue = defaultwish.preferredgroup;
                
               
                //load all tvwishes
                string listviewdata = myTvWishes.loadlongsettings("TvWishList_ListView");
                LogDebug("liestview=" + listviewdata, (int)LogSetting.DEBUG);
                myTvWishes.LoadFromString(listviewdata, true); //needed for later checking in savesettings()

                //fill datagridview
                LogDebug("initial datagrid rowcount=" + dataGridView1.Rows.Count.ToString(), (int)LogSetting.DEBUG);
                
                int newrow = dataGridView1.Rows.Count - 1;
                if (newrow == 0) //fill only the first time, datagrid is being remembered
                {
                    Log.Debug("myTvWishes.ListAll().Count="+myTvWishes.ListAll().Count.ToString());
                    foreach (TvWish mywish in myTvWishes.ListAll())
                    {
                        newrow = dataGridView1.Rows.Count - 1;
                        Log.Debug("searchfor" + mywish.searchfor);
                        Log.Debug("newrow=" + newrow.ToString());

                        if (addnames.Contains(";" + mywish.channel + ";") == false)//add new channel names
                        {
                            channelfilter.Items.Add(mywish.channel);
                            addnames += mywish.channel + ";";
                            //LogDebug("loadsetting addnames: " + addnames, (int)LogSetting.DEBUG);
                        }


                        LoadTvWishToDataGridRow(newrow, mywish);

                    }
                    Log.Debug(myTvWishes.ListAll().Count.ToString()+" Tvwishes added");
                }//end fill datagrid

                updatechannelnames();

                //dataGridView1.RowsAdded += new DataGridViewRowsAddedEventHandler(dataGridView1_RowsAdded);
                //Log.Debug("Turnuing on eventhandler for adding rows");

                //end listviewdata

                //load providervalues
                setdefaultproviders(false);

                //load last settings and store it in providerstring [0]
                setting = layer.GetSetting("TvWishList_Providers_0", "_Last Setting;;0;False");
                providers[0] = setting.Value;
                //LogDebug("Load settings: provider_0=" + providers[0]);
                
                try
                {
                    translateprovider(providers[0]);  //restore last user settings providers[0]
                }
                catch
                {
                    LogDebug("translateprovider(providers[0]= " + providers[0] + " failed", (int)LogSetting.DEBUG);
                }
                

                //labels
                DateTime NextEpgDate = DateTime.Now;
                setting = layer.GetSetting("TvWishList_NextEpgDate", DateTime.Now.ToString("yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture));
                try
                {
                    NextEpgDate = DateTime.ParseExact(setting.Value, "yyyy-MM-dd_HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception exc)
                {
                    LogDebug("NextEpgDate failed with exception: " + exc.Message, (int)LogSetting.ERROR);
                }

                Log.Debug("FormatConversion:");
                string mydate = NextEpgDate.ToString(lng.TranslateString(@"MM/dd/yyyy", 189), System.Globalization.CultureInfo.InvariantCulture);
                Log.Debug("mydate=" + mydate);
                string mytime = NextEpgDate.ToString(lng.TranslateString(@"HH:mm", 190), System.Globalization.CultureInfo.InvariantCulture);
                Log.Debug("mytime=" + mytime);
                labelCheckingdate.Text = lng.TranslateString("{0} at {1}",191,mydate,mytime);
                Log.Debug("labelCheckingdate.Text" + labelCheckingdate.Text);
                
                //integer values
                setting = layer.GetSetting("TvWishList_ProviderSelected", "0");
                int j = Convert.ToInt32(setting.Value);
                if ((j < 0) || (j > listBoxProvider2.Items.Count))
                {
                    j = 0;
                }
                try
                {
                    listBoxProvider2.SetSelected(0, true);   //select custom item in listbox
                }
                catch (Exception exc)
                {
                    LogDebug("listBoxProvider2.SetSelected failed with exception: " + exc.Message, (int)LogSetting.ERROR);
                }

                LogDebug("LoadSettings Completed", (int)LogSetting.DEBUG);
                LoadSettingError = false;
            }
            catch (Exception exc)
            {
                LogDebug("LoadSettings Error: Exception: "+exc.Message, (int)LogSetting.ERROR);
                LoadSettingError = true;
            }

            
        }

        public bool BoolConversion(string value, bool defaultvalue)
        {
            try
            {
                bool result = Convert.ToBoolean(value);
                return result;
            }
            catch
            {
                return defaultvalue;
            }


        }

        public string SortCriteriaTranslation(string englishText)
        {
            if (englishText == "Title")
                return lng.TranslateString("Title", 3201);

            else if (englishText == "Start")
                return lng.TranslateString("Start", 3202);

            else if (englishText == "Created")
                return lng.TranslateString("Created", 3203);

            else if (englishText == "Genre")
                return lng.TranslateString("Genre", 3204);

            else if (englishText == "Classification")
                return lng.TranslateString("Classification", 3205);

            else if (englishText == "ParentalRating")
                return lng.TranslateString("Parental Rating", 3206);

            else if (englishText == "StarRating")
                return lng.TranslateString("Star Rating", 3207);

            else if (englishText == "Type")
                return lng.TranslateString("Type", 3208);

            else if (englishText == "Message")
                return lng.TranslateString("Message", 3209);

            else if (englishText == "SearchString")
                return lng.TranslateString("Search String", 3210);

            else if (englishText == "EpisodeName")
                return lng.TranslateString("Episode Name", 3211);

            else if (englishText == "EpisodeNumber")
                return lng.TranslateString("Episode Number", 3212);

            else if (englishText == "EpisodePart")
                return lng.TranslateString("Episode Part", 3213);

            else
            {

                Log.Error("Incorrect criteria for sorting:" + englishText + " - changing to Title");
                return lng.TranslateString("Titel", 3201); ;
            }
            
        }

        public string SortCriteriaReverseTranslation(string localText)
        {
            if (localText == lng.TranslateString("Titel", 3201))
                return "Titel";

            else if (localText == lng.TranslateString("Start", 3202))
                return "Start";

            else if (localText == lng.TranslateString("Created", 3203))
                return "Created";

            else if (localText == lng.TranslateString("Genre", 3204))
                return "Genre";

            else if (localText == lng.TranslateString("Classification", 3205))
                return "Classification";

            else if (localText == lng.TranslateString("Parental Rating", 3206))
                return "ParentalRating";

            else if (localText == lng.TranslateString("Star Rating", 3207))
                return "StarRating";

            else if (localText == lng.TranslateString("Type", 3208))
                return "Type";

            else if (localText == lng.TranslateString("Message", 3209))
                return "Message";

            else if (localText == lng.TranslateString("Search String", 3210))
                return "SearchString";

            else if (localText == lng.TranslateString("Episode Name", 3211))
                return "EpisodeName";

            else if (localText == lng.TranslateString("Episode Number", 3212))
                return "EpisodeNumber";

            else if (localText == lng.TranslateString("Episode Part", 3213))
                return "EpisodePart";

            else
            {
                Log.Error("Incorrect criteria for sorting - changing to Title");
                return "Titel";
            }

        }

        public string LanguageFileTranslation(string languageStringFile)
        {
            //string file name e.g. strings_en.xml
            string languageLetters = languageStringFile.Replace("strings_", string.Empty);
            languageLetters = languageLetters.Replace(".xml",string.Empty);
            CultureInfo currentCulture = CultureInfo.CreateSpecificCulture(languageLetters);
            Log.Debug("languageLetters=" + languageLetters);
            Log.Debug("currentCulture.DisplayName=" + currentCulture.DisplayName.ToString());
            return currentCulture.DisplayName.ToString();
        }

        public string ReverseLanguageFileTranslation(string languageName)
        {
            for (int i = 0; i < ReverseLanguageFileTranslator_Language.Length; i++)
            {
                if (languageName == ReverseLanguageFileTranslator_Language[i])
                {
                    return ReverseLanguageFileTranslator_File[i];
                }
            }
            Log.Error("Could not find language file for " + languageName);

            return "strings_en.xml";
        }



        public void LoadTvWishToDataGridRow(int newrow, TvWish mywish)
        {
            try //add new default row to enable automated upgrades for new formats with more items
            {
                DataGridViewRow mydatagridviewrow = new DataGridViewRow();
                dataGridView1.Rows.Insert(newrow, mydatagridviewrow);
                if (mywish.b_active)
                {
                    dataGridView1[(int)TvWishEntries.active, newrow].Value = true;
                }
                else
                {
                    dataGridView1[(int)TvWishEntries.active, newrow].Value = false;
                }

                if (mywish.b_skip)
                {
                    dataGridView1[(int)TvWishEntries.skip, newrow].Value = true;
                }
                else
                {
                    dataGridView1[(int)TvWishEntries.skip, newrow].Value = false;
                }

                if (mywish.b_includeRecordings)
                {
                    dataGridView1[(int)TvWishEntries.includerecordings, newrow].Value = true;
                }
                else
                {
                    dataGridView1[(int)TvWishEntries.includerecordings, newrow].Value = false;
                }
                dataGridView1[(int)TvWishEntries.searchfor, newrow].Value = mywish.searchfor;
                dataGridView1[(int)TvWishEntries.matchtype, newrow].Value = mywish.matchtype;
                dataGridView1[(int)TvWishEntries.group, newrow].Value = mywish.group;
                dataGridView1[(int)TvWishEntries.recordtype, newrow].Value = mywish.recordtype;
                dataGridView1[(int)TvWishEntries.action, newrow].Value = mywish.action;
                dataGridView1[(int)TvWishEntries.exclude, newrow].Value = mywish.exclude;
                dataGridView1[(int)TvWishEntries.prerecord, newrow].Value = mywish.prerecord;
                dataGridView1[(int)TvWishEntries.postrecord, newrow].Value = mywish.postrecord;
                dataGridView1[(int)TvWishEntries.episodename, newrow].Value = mywish.episodename;
                dataGridView1[(int)TvWishEntries.episodepart, newrow].Value = mywish.episodepart;
                dataGridView1[(int)TvWishEntries.episodenumber, newrow].Value = mywish.episodenumber;
                dataGridView1[(int)TvWishEntries.seriesnumber, newrow].Value = mywish.seriesnumber;
                dataGridView1[(int)TvWishEntries.keepepisodes, newrow].Value = mywish.keepepisodes;
                dataGridView1[(int)TvWishEntries.keepuntil, newrow].Value = mywish.keepuntil;
                dataGridView1[(int)TvWishEntries.recommendedcard, newrow].Value = mywish.recommendedcard;
                dataGridView1[(int)TvWishEntries.priority, newrow].Value = mywish.priority;
                dataGridView1[(int)TvWishEntries.aftertime, newrow].Value = mywish.aftertime;
                dataGridView1[(int)TvWishEntries.beforetime, newrow].Value = mywish.beforetime;
                dataGridView1[(int)TvWishEntries.afterdays, newrow].Value = mywish.afterdays;
                dataGridView1[(int)TvWishEntries.beforedays, newrow].Value = mywish.beforedays;


                dataGridView1[(int)TvWishEntries.channel, newrow].Value = mywish.channel;
                dataGridView1[(int)TvWishEntries.name, newrow].Value = mywish.name;
                dataGridView1[(int)TvWishEntries.useFolderName, newrow].Value = mywish.useFolderName;
                dataGridView1[(int)TvWishEntries.withinNextHours, newrow].Value = mywish.withinNextHours;
                dataGridView1[(int)TvWishEntries.episodecriteria, newrow].Value = mywish.episodecriteria;
                dataGridView1[(int)TvWishEntries.preferredgroup, newrow].Value = mywish.preferredgroup;
                if ((string)dataGridView1[(int)TvWishEntries.name, newrow].Value == "") //avoid empty names
                {
                    dataGridView1[(int)TvWishEntries.name, newrow].Value = dataGridView1[(int)TvWishEntries.searchfor, newrow].Value;
                }

                //add tvwishid
                dataGridView1[(int)TvWishEntries.tvwishid, newrow].Value = mywish.tvwishid;



            }
            catch (Exception exc)
            {
                LogDebug("Adding row failed with message \n" + exc.Message, (int)LogSetting.ERROR);
            }
            LogDebug("Tvwish finished", (int)LogSetting.DEBUG);
                    
        }

        

        public void checkcombotextbox(ref ComboBox mycombobox,string format,int min, int max, string fieldname)
        {
            //LogDebug("Field " + fieldname + " before Checking: " + mycombobox.Text, (int)LogSetting.DEBUG);
            int number = min;
            try
            {
                number = Convert.ToInt32(mycombobox.Text);
            }
            catch (Exception exc)
            {
                LogDebug("Number could not be converted from text " + mycombobox.Text + " in field " + fieldname + " resetting to " + min.ToString(), (int)LogSetting.ERROR);
                LogDebug("Exception: " + exc.Message, (int)LogSetting.ERROR);
                number = min;
            }

            //min checking
            if (number < min)
            {
                MessageBox.Show(lng.TranslateString("Minimum "+fieldname+" is "+min.ToString(),255), lng.TranslateString("Warning",4401));
                LogDebug("Number " + mycombobox.Text + " is smaller than minimum value in field " + fieldname + "  resetting to " + min.ToString(), (int)LogSetting.ERROR);
                number = min;
            }


            //max checking
            if (number > max)
            {
                MessageBox.Show(lng.TranslateString("Maximum " + fieldname + " is " + max.ToString(), 256), lng.TranslateString("Warning", 4401));
                LogDebug("Number " + mycombobox.Text + " is larger than maximum value in field " + fieldname + "  resetting to " + max.ToString(), (int)LogSetting.ERROR);
                number = max;
            }

            //read numbers from combobox items
            try
            {
                int item_number = 0;
                int old_item_number = 0;

                int ctr = 0;



                foreach (string item in mycombobox.Items)
                {
                    

                    try
                    {
                        item_number = Convert.ToInt32(item);
                        if (item_number == number)
                        {
                            //LogDebug("Exact Combobox item found -done", (int)LogSetting.DEBUG);
                            mycombobox.Text = number.ToString(format);
                            return;
                        }

                        if (ctr > 0) //ignore first eleement to get interval
                        {
                            if ((number > old_item_number) && (number < item_number))
                            {
                                break;
                            }
                        }
                        else //ctr==0 check for first item insert
                        {
                            if (number < item_number)
                            {
                                break;
                            }
                        }
                        old_item_number = item_number;
                        ctr++;

                    }
                    catch (Exception exc)
                    {
                        LogDebug("Number could not be converted from comboboxfield " + ctr.ToString() + " for text " + item + " in field " + fieldname + " - will be skipped", (int)LogSetting.ERROR);
                        LogDebug("Exception: " + exc.Message, (int)LogSetting.ERROR);
                    }

                    

                }
                //insert new eleemnt to combobox
                //LogDebug("Inserting new combobox element " + number.ToString() + " at position " + ctr.ToString(), (int)LogSetting.DEBUG);
                mycombobox.Items.Insert(ctr, number.ToString(format));

            }
            catch (Exception exc)
            {
                LogDebug("Comboitems could not be processed in field " + fieldname , (int)LogSetting.ERROR);
                LogDebug("Exception: " + exc.Message, (int)LogSetting.ERROR);
            }


            mycombobox.Text = number.ToString(format);
            //LogDebug("Combobox After Checking: " + mycombobox.Text, (int)LogSetting.DEBUG);
        }


        public void translateprovider(string providerstring)
        {
            
	        TextBoxSmtpServer.Text="";
	        numericUpDownSmtpPort.Value=0;
	        checkBoxSSL.Checked=false;

            string[] array = providerstring.Split(";".ToCharArray());	        
            if (array.Length != 4)
	        {
                LogDebug("Invalid provider string: " + providerstring + "\n Count is " + array.Length, (int)LogSetting.ERROR);               
                MessageBox.Show(lng.TranslateString("Invalid provider string: " + providerstring + "\n Count is " + array.Length,012),lng.TranslateString("TvWishList Error",4305));
                return;
	        }
      
            TextBoxSmtpServer.Text = array[1].ToString();
            TextBoxSmtpServer.Update();
            numericUpDownSmtpPort.Value = Convert.ToInt32(array[2].ToString());
            numericUpDownSmtpPort.Update();
            checkBoxSSL.Checked = Convert.ToBoolean(array[3].ToString());
            checkBoxSSL.Update();
        }


        public void providerupdate(int selected)
        {
	        if (selected < providers.Length)
	        {
                providers[selected] = listBoxProvider2.Items[selected] + ";" +    TextBoxSmtpServer.Text + ";" + numericUpDownSmtpPort.Value.ToString() + ";" + checkBoxSSL.Checked.ToString();
                //LogDebug("Provider [i=" + selected + "] updated to " + providers[selected].ToString(), (int)LogSetting.DEBUG);
	        }
	        else
	        {
		        LogDebug("Invalid provider string select index: " + selected + "\n Maximum count is " + providers.Length, (int)LogSetting.ERROR);
            }

        }


        public void setdefaultproviders(bool silent)
        {
            
            //providers[0] reserved for last settings
            providers[0] = "_Last Setting;;0;False"; //custom stores always the last settings
            providers[1] = "1und1;smtp.1und1.de;25;True"; //1und1
            providers[2] = "gmx(free);mail.gmx.net;25;False"; //hotmail
            providers[3] = "google(free);smtp.googlemail.com;587;True"; //google
            providers[4] = "hotmail(free);smtp.live.com;25;True"; //hotmail           
            providers[5] = "web.de(free);smtp.web.de;25;True"; //web.de
	        ProviderLength=6; // is= array number+1
            
            listBoxProvider2.Items.Clear();
            for (int i = 0; i < ProviderLength; i++)
            {
                string[] array = providers[i].Split(";".ToCharArray());
                listBoxProvider2.Items.Add(array[0]);
            }


            //LogDebug("TvWishList Info: Provider settings set back to defaults", (int)LogSetting.DEBUG);
            
        }

        
        
        private void listBoxProvider2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int j = listBoxProvider2.SelectedIndex;
            //LogDebug("listBoxProvider2_SelectedIndexChanged =" + providers[j]);
            translateprovider(providers[j]);
        }
        
       

        

        public void LogDebug(string text, int field)
        {

            if (field == (int)LogSetting.INFO)
            {
                Log.Debug("TvWishList Setup: " + text);
                //textoutput(text, field);
            }
            else if (field == (int)LogSetting.ADDRESPONSE)
            {
                Log.Debug("TvWishList Setup: " + text);
                //textoutput(text, field);
            }
            else if ((field == (int)LogSetting.DEBUG) && (DEBUG == true))
            {
                if (DEBUG == true)
                {
                    Log.Debug("TvWishList Setup: " + text);
                    //textoutput(text, field);
                }
            }
            else if (field == (int)LogSetting.ERROR)
            {
                Log.Error("TvWishList Setup: " + text);
                Log.Debug("TvWishList Setup: " + text);
                //textoutput(text, field);
                
            }
            else if (field == (int)LogSetting.ERRORONLY)
            {
                Log.Error("TvWishList Setup: " + text);
                //textoutput(text, field);
            }
            else if (field == (int)LogSetting.MESSAGE)
            {
                Log.Debug("TvWishList Setup: " + text);
                //textoutput(text, field);
            }
            else
            {
                //textoutput("Error TvWishListSetup: Unknown message Code "+field.ToString(),(int)LogSetting.ERROR);
            }

            

        }


        private void buttontest_Click(object sender, EventArgs e)
        {
            
            
            if (BUSY == true)
            {
                MessageBox.Show(lng.TranslateString("Processing ongoing - please wait for completion",252), lng.TranslateString("Warning",4401));
                return;
            }
            BUSY = true;
            System.Threading.Thread th = new System.Threading.Thread(TestTvWishList);
            th.Start();
            
        }


        private void TestTvWishList()
        {
            LogDebug("EPG watching started", (int)LogSetting.DEBUG);
            try
            {
                MySaveSettings();
            }
            catch (Exception ex)
            {
                LogDebug("Fatal Error: Failed to save settings - exception message is\n" + ex.Message, (int)LogSetting.ERROR);
            }
            try
            {
                //*****************************************************
                //unlock TvWishList
                myTvWishes.UnLockTvWishList();

                //*****************************************************
                //run processing
                epgwatchclass.SearchEPG(false); //Email & Record

                

            }
            catch (Exception exc)
            {
                LogDebug("Parsing EPG data failed with exception message:", (int)LogSetting.ERROR);
                LogDebug(exc.Message, (int)LogSetting.ERROR);
                // Reset BUSY Flag
                TvBusinessLayer layer = new TvBusinessLayer();
                Setting setting = null;
                //set BUSY = false
                setting = layer.GetSetting("TvWishList_BUSY", "false");
                setting.Value = "false";
                setting.Persist();
                labelupdate("Parsing EPG data failed - Check the log file", PipeCommands.Error);
            }
            try
            {
                //*****************************************************
                //Lock TvWishList with timeout error
                bool success = false;
                int seconds = 60;
                for (int i = 0; i < seconds / 10; i++)
                {
                    success = myTvWishes.LockTvWishList("TvWishList Setup");
                    if (success)
                        break;
                    System.Threading.Thread.Sleep(10000); //sleep 10s to wait for BUSY=false
                    LogDebug("Waiting for old jobs " + (seconds - i * 10).ToString() + "s to finish", (int)LogSetting.DEBUG);

                }
                if (success == false)
                {
                    LogDebug("Timeout Error: TvWishList did not finish old jobs - reboot your computer ", (int)LogSetting.DEBUG);
                    MessageBox.Show(lng.TranslateString("Timeout Error: TvWishList did not finish old jobs - try to close the plugin again or reboot ",002));
                    LoadSettingError = true;
                }
                else
                {
                    MyLoadSettings();
                }
            }
            catch (Exception ex)
            {
                LogDebug("Fatal Error: Failed to load settings - exception message is\n" + ex.Message, (int)LogSetting.ERROR);
                LogDebug("Trying to resave settings to data base", (int)LogSetting.ERROR);
                try
                {
                    MySaveSettings();
                    LogDebug("Saving settings succeeded", (int)LogSetting.ERROR);
                }
                catch
                {
                    LogDebug("Fatal Error: Faileed to save settings", (int)LogSetting.ERROR);
                }
            }
            BUSY = false;
        }

        
        

        

        

        private void buttonDefault_Click_1(object sender, EventArgs e)
        {
            try
            {
                SELECTED_ROW = dataGridView1.CurrentCell.RowIndex;
                //Log.Debug("SELECTED_ROW=" + SELECTED_ROW.ToString());
                if (dataGridView1.RowCount > SELECTED_ROW+1)
                {
                    for (int i = 0; i < (int)TvWishEntries.end; i++)
                    {
                        if ((i == (int)TvWishEntries.active) || (i == (int)TvWishEntries.skip) || (i == (int)TvWishEntries.includerecordings))
                        {
                            dataGridView1[i, SELECTED_ROW].Value = Convert.ToBoolean(myTvWishes.DefaultValues[i]);
                        }
                        else if (i == (int)TvWishEntries.tvwishid)
                        {
                            // no entry for 28 as id is determined automatically and must not be changed
                        }
                        else
                        {
                            dataGridView1[i, SELECTED_ROW].Value = myTvWishes.DefaultValues[i];
                        }

                    }
                }
                //modify for listview table changes
            }
            catch
            {
                LogDebug("Fatal Error: Failed to delete dataview current row ", (int)LogSetting.ERROR);
                LogDebug("SELECTED_COL=" + SELECTED_COL.ToString() + "    " + "SELECTED_ROW=" + SELECTED_ROW.ToString(), (int)LogSetting.ERROR);
            }
            
        }

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("http://forum.team-mediaportal.com/tv-server-plugins-294/new-tv-server-plugin-tvwishlist-76506/");
            }
            catch (Exception exc)
            {
                LogDebug("Link failed with exception: " + exc.Message, (int)LogSetting.ERROR);
            }
        }

        private void buttonhelp_Click_1(object sender, EventArgs e)
        {//Help
            Process proc = new Process();
            ProcessStartInfo procstartinfo = new ProcessStartInfo();
            procstartinfo.FileName = "TvWishList.pdf";
            procstartinfo.WorkingDirectory = TV_USER_FOLDER + @"\TvWishList";
            proc.StartInfo = procstartinfo;
            try
            {
                proc.Start();
            }
            catch
            {
                MessageBox.Show(lng.TranslateString("Could not open " + procstartinfo.WorkingDirectory + "\\" + procstartinfo.FileName,257,procstartinfo.WorkingDirectory + "\\" + procstartinfo.FileName), lng.TranslateString("Error",4305));
            }
        }

        private void buttonCancel_Click_1(object sender, EventArgs e)
        {//Cancel
            try
            {
                MyLoadSettings();
            }
            catch (Exception ex)
            {
                LogDebug("Fatal Error: Failed to load settings - exception message is\n" + ex.Message, (int)LogSetting.ERROR);
                LogDebug("Trying to resave settings to data base", (int)LogSetting.ERROR);
                try
                {
                    MySaveSettings();
                    LogDebug("Saving settings succeeded", (int)LogSetting.ERROR);
                }
                catch
                {
                    LogDebug("Fatal Error: Faileed to save settings", (int)LogSetting.ERROR);
                }
            }
        }

        private void buttonDelete_Click_1(object sender, EventArgs e)
        {//Delete
            try
            {
                Log.Debug("dataGridView1.RowCount.ToString()="+dataGridView1.RowCount.ToString());
                Log.Debug("SELECTED_ROW.ToString()=" + SELECTED_ROW.ToString());
                
                SELECTED_COL = dataGridView1.CurrentCell.ColumnIndex;
                SELECTED_ROW = dataGridView1.CurrentCell.RowIndex;
                if (dataGridView1.RowCount > SELECTED_ROW+1)
                {
                    dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
                }
            }
            catch
            {
                LogDebug("Fatal Error: Failed to delete dataview current row ", (int)LogSetting.ERROR);
                LogDebug("SELECTED_COL=" + SELECTED_COL.ToString() + "    " + "SELECTED_ROW=" + SELECTED_ROW.ToString(), (int)LogSetting.ERROR);
            }

        }

        private void button2_Click_1(object sender, EventArgs e)
        {//Down
            SELECTED_COL = dataGridView1.CurrentCell.ColumnIndex;
            SELECTED_ROW = dataGridView1.CurrentCell.RowIndex;
            if ((SELECTED_ROW >= 0) && (SELECTED_ROW < dataGridView1.Rows.Count - 2))
            {
                DataGridViewRow selrow = dataGridView1.Rows[SELECTED_ROW];
                dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
                dataGridView1.Rows.Insert(SELECTED_ROW + 1, selrow);
                dataGridView1.CurrentCell = dataGridView1[SELECTED_COL,SELECTED_ROW+1];
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {//Up
            SELECTED_COL = dataGridView1.CurrentCell.ColumnIndex;
            SELECTED_ROW = dataGridView1.CurrentCell.RowIndex;
            if ((SELECTED_ROW > 0)&&(SELECTED_ROW < dataGridView1.Rows.Count - 1))
            {
                DataGridViewRow selrow = dataGridView1.Rows[SELECTED_ROW];
                dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
                dataGridView1.Rows.Insert(SELECTED_ROW - 1, selrow);
                dataGridView1.CurrentCell = dataGridView1[SELECTED_COL, SELECTED_ROW - 1];
            }
            
        }

        private void checkBoxEvery_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxEvery.Checked == true)
            {
                checkBoxMon.Checked = false;
                checkBoxTue.Checked = false;
                checkBoxWed.Checked = false;
                checkBoxThur.Checked = false;
                checkBoxFri.Checked = false;
                checkBoxSat.Checked = false;
                checkBoxSun.Checked = false;
            }
        }

        private void checkBoxMon_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxMon.Checked == true)
            {
                checkBoxEvery.Checked = false;
            }
        }

        private void checkBoxTue_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxTue.Checked == true)
            {
                checkBoxEvery.Checked = false;
            }
        }

        private void checkBoxWed_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxWed.Checked == true)
            {
                checkBoxEvery.Checked = false;
            }
        }

        private void checkBoxThur_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxThur.Checked == true)
            {
                checkBoxEvery.Checked = false;
            }
        }

        private void checkBoxFri_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxFri.Checked == true)
            {
                checkBoxEvery.Checked = false;
            }
        }

        private void checkBoxSat_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSat.Checked == true)
            {
                checkBoxEvery.Checked = false;
            }
        }

        private void checkBoxsun_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSun.Checked == true)
            {
                checkBoxEvery.Checked = false;
            }
        }

        private void buttonallactive_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i=0; i < dataGridView1.Rows.Count-1;i++)
                {
                    dataGridView1[0, i].Value = true;
                }
            }
            catch
            {
                LogDebug("Fatal Error: Faileed to set rows active ", (int)LogSetting.ERROR);
            }
        }

        
        private void buttonallinactive_Click_1(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    dataGridView1[0, i].Value = false;
                }
            }
            catch
            {
                LogDebug("Fatal Error: Faileed to set rows active ", (int)LogSetting.ERROR);
            }
        }

        private void buttonSkipAll_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    dataGridView1[23, i].Value = true;
                }
            }
            catch
            {
                LogDebug("Fatal Error: Faileed to set Skip Repeated Episode to false ", (int)LogSetting.ERROR);
            }
        }

        

        private void buttonSkipNone_Click_1(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    dataGridView1[23, i].Value = false;
                }
            }
            catch
            {
                LogDebug("Fatal Error: Faileed to set Skip Repeated Episode to false ", (int)LogSetting.ERROR);
            }
        }

        private void comboBoxGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            updatechannelnames();
        }

        private void updatechannelnames()
        {
            
            try
            {
                dataGridView1.Update();
                LogDebug("updatechannelnames", (int)LogSetting.DEBUG);
                //radio and tv channels item menue
                string newgroupname = comboBoxGroups.Text;
                DataGridViewComboBoxColumn channelfilter = dataGridView1.Columns[22] as DataGridViewComboBoxColumn;


                int length = channelfilter.Items.Count;
                //LogDebug("updatechannelnames: length =" + channelfilter.Items.Count.ToString(), (int)LogSetting.DEBUG);

                //add all existing channel names which are used in the channel column to a string and to the combobox at the end
                string addnames = ";Any;";
                channelfilter.Items.Add("Any");
                for (int i = 0; i < dataGridView1.Rows.Count-1; i++) //do not use last incomplete dummy row 
                {
                    //LogDebug("i =" + i.ToString(), (int)LogSetting.DEBUG);
                    string channelname = "Any";
                    dataGridView1.Update();
                    try
                    {
                        //LogDebug("dataGridView1[22, i].ValueType" + dataGridView1[22, i].ValueType.ToString(), (int)LogSetting.DEBUG); 
                        /*if (dataGridView1[22, i].Value.ToString().Length > 0)
                        {
                            LogDebug("dataGridView1[22, i].Value" + dataGridView1[22, i].Value.ToString(), (int)LogSetting.DEBUG);
                        }*/
                        channelname = dataGridView1[22, i].Value as String;
                    }
                    catch
                    {
                        LogDebug("Controlled Exception on channelname = dataGridView1[22, i].Value as String", (int)LogSetting.DEBUG);
                        //dataGridView1[22, i].Value = "Any";
                    }

                    
                    //LogDebug("channelname.length =" + channelname.Length.ToString(), (int)LogSetting.DEBUG);
                    /*if (channelname.Length > 0)
                    {
                        LogDebug("char =" + Convert.ToInt32(channelname[0]).ToString(), (int)LogSetting.DEBUG);
                    }*/

                    
                    
                    //LogDebug("channelname =" + channelname, (int)LogSetting.DEBUG);
                    if (addnames.Contains(";"+channelname+";") == false) //add new channel names
                    {
                        channelfilter.Items.Add(channelname);
                        addnames += channelname + ";";
                        //LogDebug("addnames: " + addnames, (int)LogSetting.DEBUG);
                    }
                }
                //LogDebug("updatechannelnames: end adding channels", (int)LogSetting.DEBUG);
                


                //remove all previous channel names  - do not sort channels!!!
                for (int i = 0; i < length; i++)
                {
                    channelfilter.Items.RemoveAt(0);
                }

                //LogDebug("updatechannelnames: removing channels completed", (int)LogSetting.DEBUG);
                //LogDebug("updatechannelnames: length =" + channelfilter.Items.Count.ToString(), (int)LogSetting.DEBUG);

                //build new channel list based on new group filter
                foreach (ChannelGroup channelgroup in ChannelGroup.ListAll())
                {
                    if (channelgroup.GroupName == newgroupname)
                    {
#if(TV100)
                        IList allgroupmaps = channelgroup.ReferringGroupMap();
#elif(TV101)
                        IList<GroupMap> allgroupmaps = channelgroup.ReferringGroupMap();
#elif(TV11)
                        IList<GroupMap> allgroupmaps = channelgroup.ReferringGroupMap();
#elif(TV12)
                        IList<GroupMap> allgroupmaps = channelgroup.ReferringGroupMap();
#endif




                        foreach (GroupMap onegroupmap in allgroupmaps)
                        {
                            string channelname = onegroupmap.ReferencedChannel().DisplayName;
                            if (channelname == "")
                            {
                                channelname = "Any";
                            }
                            if (addnames.Contains(";"+channelname+";") == false)//add new channel names
                            {
                                channelfilter.Items.Add(channelname);
                                addnames += channelname + ";";
                                //
                                //LogDebug("TV addnames: " + addnames, (int)LogSetting.DEBUG);
                            }

                        }
                        return;

                    }
                }
                foreach (RadioChannelGroup radiochannelgroup in RadioChannelGroup.ListAll())
                {
                    if (radiochannelgroup.GroupName == newgroupname)
                    {

#if(TV100)
                        IList allradiogroupmaps = radiochannelgroup.ReferringRadioGroupMap();
#elif(TV101)
                        IList<RadioGroupMap> allradiogroupmaps = radiochannelgroup.ReferringRadioGroupMap();
#elif(TV11)
                        IList<RadioGroupMap> allradiogroupmaps = radiochannelgroup.ReferringRadioGroupMap();
#elif(TV12)
                        IList<RadioGroupMap> allradiogroupmaps = radiochannelgroup.ReferringRadioGroupMap();
#endif


                        foreach (RadioGroupMap oneradiogroupmap in allradiogroupmaps)
                        {
                            string channelname = oneradiogroupmap.ReferencedChannel().DisplayName;
                            if (channelname == "")
                            {
                                channelname = "Any";
                            }
                            if (addnames.Contains(";"+channelname+";") == false)//add new channel names
                            {
                                channelfilter.Items.Add(channelname);
                                addnames += channelname + ";";
                                //LogDebug("Radio addnames: " + addnames, (int)LogSetting.DEBUG);
                            }


                        }
                        return;

                    }
                }
            }
            catch (Exception Exc)
            {
                LogDebug("Fatal Error: updatechannels exception is: "+Exc.Message, (int)LogSetting.ERROR);
            }
        }

        



        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            // set check boxes with initial value in case of added rows as the initialized null value is not recognized
            try
            {
                LogDebug("dataGridView1_RowsAdded  e.RowIndex="+e.RowIndex.ToString(), (int)LogSetting.DEBUG);
                LogDebug("dataGridView1_RowsAdded  e.RowCount=" + e.RowCount.ToString(), (int)LogSetting.DEBUG);
                LogDebug("dataGridView1_RowsAdded  dataGridView1.RowCount=" + dataGridView1.RowCount.ToString(), (int)LogSetting.DEBUG);
                LogDebug("", (int)LogSetting.DEBUG);

               
                //convert defaultdata with language translation
                string[] columndata = (string[])myTvWishes.DefaultValues.Clone();
                TvWish defaultwish = new TvWish();
                defaultwish = myTvWishes.CreateTvWish(true, columndata);
                Log.Debug("defaultwish with Languagetranslation created");
                //myTvWishes.DebugTvWish(defaultwish);


                LoadTvWishToDataGridRow(e.RowIndex - 1, defaultwish);


               /* dataGridView1[(int)TvWishEntries.active, e.RowIndex - 1].Value = true;
                dataGridView1[(int)TvWishEntries.skip, e.RowIndex - 1].Value = true;
                dataGridView1[(int)TvWishEntries.includerecordings, e.RowIndex - 1].Value = false;*/
               
                myTvWishes.MaxTvWishId++;
                dataGridView1[(int)TvWishEntries.tvwishid, e.RowIndex - 1].Value = myTvWishes.MaxTvWishId.ToString();
                
            }
            catch (Exception Exc)
            {
                LogDebug("dataGridView1_RowsAdded exception for e.RowIndex="+e.RowIndex.ToString()+" is: " + Exc.Message, (int)LogSetting.ERROR);
            }
        }

        private void checkBoxDeletemessageFile_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDeletemessageFile.Checked == true)
            {
                try
                {
                    switch (MessageBox.Show(lng.TranslateString("Do you want to delete all your messages? \nWARNING: This will erase your message history including deleted messages\n Make sure the TvWishListMP MediaPortal plugin is closed!",258), lng.TranslateString("Info: Deleting Message File",4400), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
                    {
                        case DialogResult.Yes:
                            {
                                // "Yes" processing

                                // create file backup
                                string dataString = myTvWishes.loadlongsettings("TvWishList_ListViewMessages");
                                mymessage.readxmlfile(dataString,false);
                                mymessage.filename = TV_USER_FOLDER + @"\TvWishList\Messages." + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture) + ".xml";
                                mymessage.writexmlfile(true);
                                //save_longsetting("<ROOT></ROOT>", "TvWishList_ListViewMessages");//delete settings
                                mymessage.ClearTvMessages();

                                //store empty messages for both modes
                                dataString = mymessage.writexmlfile(false); //write xml file to string
                                myTvWishes.save_longsetting(dataString, "TvWishList_OnlyViewMessages");
                                myTvWishes.save_longsetting(dataString, "TvWishList_ListViewMessages");

                                //File.Delete(TV_USER_FOLDER + @"\TvWishList\Messages.xml");
                                MessageBox.Show(lng.TranslateString("Messages deleted", 259), lng.TranslateString("Info", 4400));
                                checkBoxDeletemessageFile.Checked = false;
                                break;
                            }
                        case DialogResult.No:
                            {
                                // "No" processing
                                MessageBox.Show(lng.TranslateString("Operation canceled",260), lng.TranslateString("Info",4400));
                                break;

                            }
                    }

                }
                catch (Exception exc)
                {
                    LogDebug("Error in deleting message file\n" + TV_USER_FOLDER + @"\TvWishList\Messages.xml" + "\nException mesage was: " + exc.Message, (int)LogSetting.ERROR);
                }
            }
            
        }

        


        private void checkBoxResetEmailFormat_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxResetEmailFormat.Checked == false)
            {
                switch (MessageBox.Show(lng.TranslateString("Do you want to restore all formats to the default values? This will overwrite all settings and all formats",261), lng.TranslateString("Warning: Restoring Default Formats",4401), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
                {
                    case DialogResult.Yes:
                        {
                            // "Yes" processing
                            // restore all settings to the default values
                            checkBoxMon.Checked = false;
                            checkBoxTue.Checked = false;
                            checkBoxWed.Checked = false;
                            checkBoxThur.Checked = false;
                            checkBoxFri.Checked = false;
                            checkBoxSat.Checked = false;
                            checkBoxSun.Checked = false;
                            checkBoxEvery.Checked = true;
                            comboBoxdays.Text = "07";
                            comboBoxhours.Text = "06";
                            comboBoxminutes.Text = "00";

                            checkBoxschedule.Checked = true;
                            checkBoxscheduleconflicts.Checked = false;
                            checkBoxDeleteChangedEPG.Checked = true;
                            checkBoxemailreply.Checked = true;
                            checkBoxEmailOnlynew.Checked = true;
                            checkBoxSkipDeleted.Checked = true;
                            comboBoxDeleteExpiration.Text = "12";
                            comboBoxmaxfound.Text = "100";

                            checkBoxSlowCPU.Checked = true;
                            checkBoxScheduleMinutes.Checked = false;
                            comboBoxScheduleMinutes.Text = "00";

                           
                            textBoxEpgMark.Text = "";

                            textBoxDateTimeFormat.Text = String.Empty;
                            textBoxEmailFormat.Text = String.Empty;
                            checkBoxFilterEmail.Checked = true;       //only email as a default for emails
                            checkBoxFilterRecord.Checked = false;
                            checkBoxFilterConflicts.Checked = false;
                            comboBoxSortCriteria.Text = lng.TranslateString("Start",3202);
                            checkBoxdescendingSort.Checked = false;
                            array_Eventformats = new string[mymessage.MessageEventsNumber];
                            for (int i = 0; i < mymessage.MessageEventsNumber; i++)
                            {
                                array_Eventformats[i] = "";
                            }

                            break;
                        }
                    case DialogResult.No:
                        {
                            // "No" processing
                            MessageBox.Show(lng.TranslateString("Operation canceled",260), lng.TranslateString("Info",4400));
                            break;

                        }

                }
            }
            checkBoxResetEmailFormat.Checked = false;

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

            buttonallactive.Hide();
            buttonallinactive.Hide();
            buttonSkipAll.Hide();
            buttonSkipNone.Hide();
            label11.Hide();
            comboBoxGroups.Hide();

            label4.Hide();
            textBoxSmtpEmailAdress.Hide();
            label2.Hide();

            for (int i = 0; i < (int)TvWishEntries.end; i++)
            {
                dataGridView1.Columns[i].Visible = false;
            }
            dataGridView1.Columns[(int)TvWishEntries.searchfor].Visible = true;
            dataGridView1.Columns[(int)TvWishEntries.matchtype].Visible = true;
            dataGridView1.Columns[(int)TvWishEntries.recordtype].Visible = true;
            dataGridView1.Columns[(int)TvWishEntries.action].Visible = true;
            dataGridView1.Columns[(int)TvWishEntries.exclude].Visible = true;
            


        }


        private void AdvancedMode()
        {
            if (!tabControl1.TabPages.Contains(tabPage2))
                tabControl1.TabPages.Add(tabPage2);
            if (!tabControl1.TabPages.Contains(tabPage3))
                tabControl1.TabPages.Add(tabPage3);

            buttonallactive.Show();
            buttonallinactive.Show();
            buttonSkipAll.Show();
            buttonSkipNone.Show();
            label11.Show();
            comboBoxGroups.Show();

            label4.Show();
            textBoxSmtpEmailAdress.Show();
            label2.Show();

            for (int i = 0; i < (int)TvWishEntries.end; i++)
            {
                dataGridView1.Columns[i].Visible = true;
            }
            dataGridView1.Columns[(int)TvWishEntries.viewed].Visible = false;
            dataGridView1.Columns[(int)TvWishEntries.scheduled].Visible = false;
            dataGridView1.Columns[(int)TvWishEntries.tvwishid].Visible = false;
            dataGridView1.Columns[(int)TvWishEntries.recorded].Visible = false;
            dataGridView1.Columns[(int)TvWishEntries.deleted].Visible = false;
            dataGridView1.Columns[(int)TvWishEntries.emailed].Visible = false;
            dataGridView1.Columns[(int)TvWishEntries.conflicts].Visible = false;
        }

        private void checkBoxSlowCPU_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSlowCPU.Checked == true)
            {
                checkBoxScheduleMinutes.Checked = false;
            }
        }

        private void checkBoxScheduleMinutes_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxScheduleMinutes.Checked == true)
            {
                checkBoxSlowCPU.Checked = false;
            }
        }

        private void buttonSearchNow_Click(object sender, EventArgs e)
        {
            if (BUSY == true)
            {
                MessageBox.Show(lng.TranslateString("Processing ongoing - please wait for completion", 252), lng.TranslateString("Warning", 4401));
                return;
            }
            BUSY = true;
            System.Threading.Thread th = new System.Threading.Thread(TestTvWishList);
            th.Start();
        }

        
        

        
        

        

        

        
        

        

        
        

        

        
    }
}
