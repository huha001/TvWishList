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
using System.Globalization;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
//using System.Data;
//using System.Drawing;
 
using System.IO;
//using System.Linq;
//using System.Text;

using System.Xml;

using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Threading;


using MediaPortal.Configuration;
using MediaPortal.Localisation;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using Action = MediaPortal.GUI.Library.Action;
//using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;
using TvControl;
using TvDatabase;
using Gentle.Framework;
using TvWishList;
using Log = TvLibrary.Log.huha.Log;
//using GUIKeyboard = MediaPortal.GUI.Library.huha.GUIKeyboard;


namespace MediaPortal.Plugins.TvWishList
{
    
  public partial class Main_GUI : GUIWindow, ISetupForm
  {
    

    [SkinControlAttribute(3)] protected GUIButtonControl buttonRun=null;
    [SkinControlAttribute(5)] protected GUIButtonControl buttonNew = null;
    [SkinControlAttribute(6)] protected GUIButtonControl buttonSave=null;
    [SkinControlAttribute(7)] protected GUIButtonControl buttonCancel = null;
    [SkinControlAttribute(2)] protected GUIButtonControl buttonViwOnlyMode = null;
    [SkinControlAttribute(8)] protected GUIButtonControl buttonMore = null;

    [SkinControlAttribute(51)] protected GUIPlayListItemListControl myListView = null;
    [SkinControlAttribute(70)] protected GUITextScrollUpControl mytextbox = null;

    Result_GUI GUI_List_window=null;
    Edit_GUI GUI_Edit_window = null;
    //ServiceInterface myinterface;
    int _mainwindowid = 70943675;
    int _guilistwindowid = 70943676;
    int _guieditwindowid = 70943677;

    int PREVIOUSWINDOW = -1;

    bool FIRST_INITIALIZE = true;

    //public List<TvWish> TvWishes = new List<TvWish>();
    
    #region ISetupForm Members

    // Returns the name of the plugin which is shown in the plugin menu
    public string PluginName()
    {
        return "TvWishListMP";
    }

    // Returns the description of the plugin is shown in the plugin menu
    public string Description()
    {
        return "MediaPortal plugin to input data for TvWishList";
    }

    // Returns the author of the plugin which is shown in the plugin menu
    public string Author()
    {
      return "huha";
    }

    // show the setup dialog
    public void ShowPlugin()
    {
        var form = new TvWishListSetup();
        form.ShowDialog();
    }

    // Indicates whether plugin can be enabled/disabled
    public bool CanEnable()
    {
      return true;
    }

    // Get Windows-ID
    public int GetWindowId()
    {
      // WindowID of windowplugin belonging to this setup
      // enter your own unique code
      return _mainwindowid;
    }

    // Indicates if plugin is enabled by default;
    public bool DefaultEnabled()
    {
      return true;
    }

    // indicates if a plugin has it's own setup screen
    public bool HasSetup()
    {
      return true;
    }

    /// <summary>
    /// If the plugin should have it's own button on the main menu of Mediaportal then it
    /// should return true to this method, otherwise if it should not be on home
    /// it should return false
    /// </summary>
    /// <param name="strButtonText">text the button should have</param>
    /// <param name="strButtonImage">image for the button, or empty for default</param>
    /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
    /// <param name="strPictureImage">subpicture for the button or empty for none</param>
    /// <returns>true : plugin needs it's own button on home
    /// false : plugin does not need it's own button on home</returns>

    public bool GetHome(out string strButtonText, out string strButtonImage,
      out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText=PluginName();
      strButtonImage = String.Empty;
      strButtonImageFocus=String.Empty;
      strPictureImage = @"hover_TvWishList.png";
      return true;
    }

    // With GetID it will be an window-plugin / otherwise a process-plugin
    // Enter the id number here again
     public override int GetID
    {
      get
      {
        return _mainwindowid;
      }

      set
      {
      }
    }

    #endregion

    //when MP is started
     
    public override bool Init()
    {
        Log.Info("[TVWishListMP]:Init");

        _instance = this;

        //load MP language
        PluginGuiLocalizeStrings.LoadMPlanguage();

        Log.Info("Current language is " + PluginGuiLocalizeStrings.CurrentLanguage());
        
        //Message Initialization after language
        Log.Debug("Initializing messages");
        mymessages = new XmlMessages(PluginGuiLocalizeStrings.Get(4900), "", false);  //user defined custom DateTimeFormat will overwrite default value later
        Log.Debug("[TVWishListMP GUI_List]:Init  mymessages.filename=" + mymessages.filename);
        
        //TvWishes Initialization
        myTvWishes = new TvWishProcessing();

        //activating GUI List Window
        if (GUIWindowManager.GetWindow(_guilistwindowid) == null)
        {
            Log.Info("initializing _guilistwindow");
            GUI_List_window = new Result_GUI();
            GUI_List_window.Init();
            var win = (GUIWindow)GUI_List_window;
            GUIWindowManager.Add(ref win);
            Log.Info("end initializing _guilistwindow");
        }

        try
        {
            GUI_List_window = (Result_GUI)GUIWindowManager.GetWindow(_guilistwindowid);
            GUI_List_window.TvWishes = myTvWishes;
            GUI_List_window.MyMessages = mymessages;
        }
        catch (Exception exc)
        {
            Log.Error("[TVWishListMP]:Init: Gui List window could not be activated. Exception error " + exc.Message);
        }
        //end activating GUI List Window


        //activating GUI Edit Window
        if (GUIWindowManager.GetWindow(_guieditwindowid) == null)
        {
            Log.Info("initializing _guieditwindow");
            GUI_Edit_window = new Edit_GUI();
            GUI_Edit_window.Init();
            var win = (GUIWindow)GUI_Edit_window;
            GUIWindowManager.Add(ref win);
            Log.Info("end initializing _guieditwindow");
        }

        try
        {
            GUI_Edit_window = (Edit_GUI)GUIWindowManager.GetWindow(_guieditwindowid);
            GUI_Edit_window.TvWishes = myTvWishes;
            GUI_Edit_window.MyMessages = mymessages;
        }
        catch (Exception exc)
        {
            Log.Debug("[TVWishListMP]:Init: Gui Edit window could not be activated. Exception error " + exc.Message);
        }
        //end activating GUI List Window

        GUIPropertyManager.SetProperty("#tvwishlist.label", String.Format(PluginGuiLocalizeStrings.Get(100)));
        GUIPropertyManager.SetProperty("#tvwish.label", String.Format(PluginGuiLocalizeStrings.Get(101)));

        //initialize TV database
        //myinterface = new ServiceInterface();
        
        _TextBoxFormat_16to9_EmailFormat_Org = PluginGuiLocalizeStrings.Get(1900);
        _TextBoxFormat_16to9_EmailFormat_Org = _TextBoxFormat_16to9_EmailFormat_Org.Replace(@"\n", "\n");
        _TextBoxFormat_16to9_EmailFormat_Org = _TextBoxFormat_16to9_EmailFormat_Org.Replace("<br>", "\n");
        _TextBoxFormat_16to9_EmailFormat_Org = _TextBoxFormat_16to9_EmailFormat_Org.Replace("<BR>", "\n");

        _TextBoxFormat_4to3_EmailFormat_Org = PluginGuiLocalizeStrings.Get(1902);
        _TextBoxFormat_4to3_EmailFormat_Org = _TextBoxFormat_4to3_EmailFormat_Org.Replace(@"\n", "\n");
        _TextBoxFormat_4to3_EmailFormat_Org = _TextBoxFormat_4to3_EmailFormat_Org.Replace("<br>", "\n");
        _TextBoxFormat_4to3_EmailFormat_Org = _TextBoxFormat_4to3_EmailFormat_Org.Replace("<BR>", "\n");

        _TextBoxFormat_16to9_ViewOnlyFormat_Org = PluginGuiLocalizeStrings.Get(1901);
        _TextBoxFormat_16to9_ViewOnlyFormat_Org = _TextBoxFormat_16to9_ViewOnlyFormat_Org.Replace(@"\n", "\n");
        _TextBoxFormat_16to9_ViewOnlyFormat_Org = _TextBoxFormat_16to9_ViewOnlyFormat_Org.Replace("<br>", "\n");
        _TextBoxFormat_16to9_ViewOnlyFormat_Org = _TextBoxFormat_16to9_ViewOnlyFormat_Org.Replace("<BR>", "\n");

        _TextBoxFormat_4to3_ViewOnlyFormat_Org = PluginGuiLocalizeStrings.Get(1903);
        _TextBoxFormat_4to3_ViewOnlyFormat_Org = _TextBoxFormat_4to3_ViewOnlyFormat_Org.Replace(@"\n", "\n");
        _TextBoxFormat_4to3_ViewOnlyFormat_Org = _TextBoxFormat_4to3_ViewOnlyFormat_Org.Replace("<br>", "\n");
        _TextBoxFormat_4to3_ViewOnlyFormat_Org = _TextBoxFormat_4to3_ViewOnlyFormat_Org.Replace("<BR>", "\n");

        return Load(GUIGraphicsContext.Skin + @"\TvWishListMP.xml");        
    }
   
    //when MP is started after Init()
    public override void OnAdded()
    { //after loading plugin
        Log.Debug("[TVWishListMP]:OnAdded");
       
    }

    public void InitializeTvWishList() //initialization must be done outside Init() and outside OnAdded()
    {
        if (FIRST_INITIALIZE)
        {
            // get pipename from Tvserver as MPExtended uses TV1 and NativeTvserver TV2
            // get hostname from tvserver for multiseat installation
            TvBusinessLayer layer = new TvBusinessLayer();
            Setting setting;
#if (MP2)
            setting = layer.GetSetting("TvWishList_PipeName", "MP2TvWishListPipe");
#else
            setting = layer.GetSetting("TvWishList_PipeName", "TvWishListPipe");
#endif

            string pipename = setting.Value;
            setting = layer.GetSetting("TvWishList_MachineName", "localhost");
            string hostname = setting.Value;
            myPipeClient = new PipeClient(myTvWishes, hostname, pipename);
            //load MP data
            LoadSettings(); //must be done after initialization of windows

            //load TvwishlistFolder and filenames from TvServer
            LoadFromTvServer();
            MpVersion = this.Version();
            if (TvVersion != MpVersion) //version does not match
            {
                Log.Debug("TvVersion " + TvVersion + " does not match MpVersion " + MpVersion + " -Aborting plugin");
                VersionMismatch = true;
            }
            Log.Debug("MpVersion =" + MpVersion);
            Log.Debug("TvVersion =" + TvVersion);

            FIRST_INITIALIZE = false;
            //check command from tvserver needed?
        }
    }

    //plugin window activated
    protected override void OnPageLoad()
    {
        InitializeTvWishList();

        Log.Debug("[TVWishListMP]:OnPageLoad  VIEWONLY=" + myTvWishes.ViewOnlyMode.ToString());
        PREVIOUSWINDOW = this.PreviousWindowId;        
        Log.Debug("Previous WidowID=" + PREVIOUSWINDOW);

        //update hostname for pipes if different server was chose
        TvBusinessLayer layer = new TvBusinessLayer();
        Setting setting;
        setting = layer.GetSetting("TvWishList_MachineName", "localhost");
        myPipeClient.HostName = setting.Value;

        if (VersionMismatch) //bye
        {
            myTvWishes.MyMessageBox(4305, 4306); //TvWishList MediaPortal Plugin Does Not Match To TvWishList TV Server Plugin
            //GUIWindowManager.ActivateWindow(PREVIOUSWINDOW);
            TvServerLoadSettings_FAILED = true; // do not save data after version mismatch
            GUIWindowManager.ActivateWindow(1); //goto tv mainwindow to avoid a partial loop
            return;
        }

        
        if ((PREVIOUSWINDOW == _guilistwindowid)||(PREVIOUSWINDOW == _guieditwindowid))//TVWishList  VIEW or EDIT
        {
            if ((LOCKED == false) && (TvServerLoadSettings_FAILED == false)) 
            {
                //*****************************************************
                //Lock Tvwishes
                bool success = myTvWishes.LockTvWishList("TvWishListMP:Main");
                if (!success)
                {
                    myTvWishes.MyMessageBox(4305, 4311); //Tv wish list is being processed by another process<br>Try again later<br>If the other process hangs reboot the system or stop the tv server manually
                    LOCKED = false;
                    Log.Debug("1 LOCKED=" + LOCKED.ToString());
                    TvServerLoadSettings_FAILED = true;
                    GUIWindowManager.ActivateWindow(1); //goto tv mainwindow to avoid a partial loop                    
                }
                else
                {
                    LOCKED = true;
                    Log.Debug("2 LOCKED=" + LOCKED.ToString());
                }
            }
            //do not load settings            
        }
        else
        {
            //*****************************************************
            //Lock and load Tvwishes
            bool success = myTvWishes.LockTvWishList("TvWishListMP:Main");
            if (!success)
            {
                myTvWishes.MyMessageBox(4305, 4311); //Tv wish list is being processed by another process<br>Try again later<br>If the other process hangs reboot the system or stop the tv server manually
                LOCKED = false;
                Log.Debug("3 LOCKED=" + LOCKED.ToString());
                TvServerLoadSettings_FAILED = true;
                GUIWindowManager.ActivateWindow(1); //goto tv mainwindow to avoid a partial loop                
            }
            else
            {
                LOCKED = true;
                Log.Debug("4 LOCKED=" + LOCKED.ToString());
            }
            

            TvserverdatabaseLoadSettings();
            //MP settings at mediaportal exit
        }
        Log.Debug("_Tvwishes.Count=" + myTvWishes.ListAll().Count.ToString());

        //trying new position for formatprocessing every time a page is loaded so that edit items are being updated when allitems are selected in edit window
        // Postprocess formats and keep only selected edit items in textboxformats (must be done after MP LoadSettings!)
        _TextBoxFormat_16to9_EmailFormat = TextBoxFormatConversion(_TextBoxFormat_16to9_EmailFormat_Org);
        _TextBoxFormat_4to3_EmailFormat = TextBoxFormatConversion(_TextBoxFormat_4to3_EmailFormat_Org);
        _UserEmailFormat = TextBoxFormatConversion(_UserEmailFormat_Org);

        _TextBoxFormat_16to9_ViewOnlyFormat = TextBoxFormatConversion(_TextBoxFormat_16to9_ViewOnlyFormat_Org);
        _TextBoxFormat_4to3_ViewOnlyFormat = TextBoxFormatConversion(_TextBoxFormat_4to3_ViewOnlyFormat_Org);
        _UserViewOnlyFormat = TextBoxFormatConversion(_UserViewOnlyFormat_Org);
        // end postprocessing formats
        

        UpdateControls();
        UpdateListItems();


#if (MP12 || MP16)
        //check for parameters
        if (_loadParameter != null)
        {
            Log.Debug("new loadparameter="+_loadParameter);
            ParameterEvaluation(_loadParameter, PREVIOUSWINDOW, TvWishListQuickMenu);
        }
#endif
        
        //GUIControl.FocusControl(_mainwindowid, 51);
        Action myaction = new Action(Action.ActionType.ACTION_MOUSE_MOVE,0,0);
        GUIWindowManager.OnAction(myaction);

        base.OnPageLoad();
    }

    

   
    
    protected override void OnPageDestroy(int newWindowId)
    {
        Log.Debug("[TVWishListMP]:OnPageDestroy  new windowID=" + newWindowId.ToString());
        Log.Debug("[TVWishListMP]:VIEWONLY=" + myTvWishes.ViewOnlyMode.ToString());
        Log.Debug("newWindowId=" + newWindowId.ToString());

        //save data if going to a new page
        if ((newWindowId != _guilistwindowid)&&(newWindowId != _guieditwindowid))
        {
            if (TvserverdatabaseSaveSettings() == true)
            {
                myTvWishes.MyMessageBox(4400, 1300);   //Info, TvWish data have been saved
            }
            else
            {
                myTvWishes.MyMessageBox(4305, 1301);   //Error, TvWish data could not be saved
            }

            //*****************************************************
            //unlock TvWishList
            myTvWishes.UnLockTvWishList();
            LOCKED = false;

        }

        PREVIOUSWINDOW = -1;
        if (myListView !=  null)
        myListView.Clear();

        base.OnPageDestroy(newWindowId);
    }


    //GUI clicked
    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
          Log.Debug("[TVWishListMP]:OnClicked");

          //myTvWishes.StatusLabel(actionType.ToString());
          if (PipeRunThreadActive == false)
          {
              if (control == myListView) //playlist
              {
                  switch (actionType)
                  {
                      case MediaPortal.GUI.Library.Action.ActionType.ACTION_SHOW_PLAYLIST:
                          // GUIWindowManager.ShowPreviousWindow();
                          break;
                      case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_SELECTED_ITEM_UP:
                          MovePlayListItemUp();
                          break;
                      case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_SELECTED_ITEM_DOWN:
                          MovePlayListItemDown();
                          break;
                      case MediaPortal.GUI.Library.Action.ActionType.ACTION_DELETE_SELECTED_ITEM:
                          DeletePlayListItem();
                          break;
                      case MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM:
                      case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOUSE_CLICK:
                          ItemSelectionChanged();
                          break;

                      /*case MediaPortal.GUI.Library.Action.ActionType.ACTION_CONTEXT_MENU:
                      case MediaPortal.GUI.Library.Action.ActionType.ACTION_SHOW_INFO:
                      case MediaPortal.GUI.Library.Action.ActionType.REMOTE_0:
                      case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOUSE_DOUBLECLICK:
                          //View results of a single TvWish
                          myTvWishes.FocusedWishIndex = myListView.SelectedListItemIndex;
                          mymessages.FilterName = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).tvwishid;
                          GUIWindowManager.ActivateWindow(_guilistwindowid);
                          break;*/

                  }
              }
              else if (control == buttonRun)
              {
                  OnButtonRun();
              }
              else if (control == buttonSave)
              {
                  OnButtonSave();
              }
              else if (control == buttonCancel)
              {
                  OnButtonCancel();
              }
              else if (control == buttonNew)
              {
                  OnButtonNew();
              }
              else if (control == buttonViwOnlyMode)
              {
                  OnButtonViewOnlyMode();
              }
              else if (control == buttonMore)
              {
                  OnButtonMore();
              }
          }
          else
          {
              myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Search Is Still Running"
          }
          base.OnClicked (controlId, control, actionType);
    }
    
    //called for mouse move, mouse click, key stroke, actioncode after plugin is selected   Action sequence is mousemove mouseclicked
    public override void OnAction(Action action)
    {
        //Log.Debug("[TVWishListMP]:OnAction");
        
        switch (action.wID) //general actions
        {
            case MediaPortal.GUI.Library.Action.ActionType.ACTION_CONTEXT_MENU:
            case MediaPortal.GUI.Library.Action.ActionType.ACTION_SHOW_INFO:
                //View results of a single TvWish
                myTvWishes.FocusedWishIndex = myListView.SelectedListItemIndex;
                mymessages.FilterName = myTvWishes.GetAtIndex(myTvWishes.FocusedWishIndex).tvwishid;
                GUIWindowManager.ActivateWindow(_guilistwindowid);
                break;

            

            case MediaPortal.GUI.Library.Action.ActionType.ACTION_PREVIOUS_MENU:
                GUIWindowManager.ShowPreviousWindow();
                return;
                
            case MediaPortal.GUI.Library.Action.ActionType.ACTION_EXIT:
            case MediaPortal.GUI.Library.Action.ActionType.ACTION_HIBERNATE:
            case MediaPortal.GUI.Library.Action.ActionType.ACTION_POWER_OFF:
            case MediaPortal.GUI.Library.Action.ActionType.ACTION_REBOOT:
                if (TvserverdatabaseSaveSettings() == true)
                {
                    myTvWishes.MyMessageBox(4400, 1300);   //Info, TvWish data have been saved
                }
                else
                {
                    myTvWishes.MyMessageBox(4305, 1301);   //Error, TvWish data could not be saved
                }

                break;

        }
        
        base.OnAction(action);
        
    }

    public override bool OnMessage(GUIMessage message)
    {
        //Log.Info("[TVWishListMP]:OnMessage" + message.ToString());

        switch (message.Message)
        {
            

            case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
                

                if (myListView.SelectedListItemIndex >= 0)
                {
                    try
                    {
                        
                        string _TextBoxFormat = "";
                        //Log.Debug("mytextbox.Height=" + mytextbox.Height.ToString());
                        //Log.Debug("mytextbox.Width=" + mytextbox.Width.ToString());
                        if (myTvWishes.ViewOnlyMode == false)
                        {
                            if (_UserEmailFormat != "")
                            {
                                _TextBoxFormat = _UserEmailFormat;
                            }
                            else if ((2 * mytextbox.Height) > mytextbox.Width)
                            {
                                _TextBoxFormat = _TextBoxFormat_16to9_EmailFormat;
                            }
                            else
                            {
                                _TextBoxFormat = _TextBoxFormat_4to3_EmailFormat;
                            }
                        }
                        else
                        {
                            if (_UserViewOnlyFormat != "")
                            {
                                _TextBoxFormat = _UserViewOnlyFormat;
                            }
                            else if ((2 * mytextbox.Height) > mytextbox.Width)
                            {
                                _TextBoxFormat = _TextBoxFormat_16to9_ViewOnlyFormat;
                            }
                            else
                            {
                                _TextBoxFormat = _TextBoxFormat_4to3_ViewOnlyFormat;
                            }
                        }
                        string messagetext = "";

                        if ((myListView.SelectedListItemIndex >= 0) && (myTvWishes.ListAll().Count > 0))
                            messagetext = FormatTvWish(myTvWishes.GetAtIndex(myListView.SelectedListItemIndex), _TextBoxFormat);

                        
                        GUIPropertyManager.SetProperty("#textbox.label", messagetext);
                        
                    }
                    catch (Exception exc)
                    {
                        Log.Error("[TVWishListM]:OnMessage Error in format string: Exception=" + exc.Message);
                    }

                    
                }
                break;


            case GUIMessage.MessageType.GUI_MSG_CLICKED:
                
                Log.Debug("message.Label=" + message.Label);
                break;
        }


        return base.OnMessage(message);
    }

    // at mediaportal exit
    public override void DeInit()
    {
        Log.Debug("[TVWishListMP]:DeInit");
        SaveSettings();
        
        //*****************************************************
        //save data and unlock TvWishList and 
        if (LOCKED == true)
        {
            TvserverdatabaseSaveSettings();
            myTvWishes.UnLockTvWishList();
            LOCKED = false;
            Log.Debug("5 LOCKED=" + LOCKED.ToString());
        }
    }


    private void OnButtonNew()
    {
        Log.Debug("[TVWishListMP]:OnButtonNew");

        string keyboardstring = "";
        if (GUI_Edit_window.GetUserInputString(ref keyboardstring, false) == true)
        {
            TvWish newwish = myTvWishes.DefaultData();
            newwish.searchfor = keyboardstring;
            newwish.name = keyboardstring;
            myTvWishes.Add(newwish);
            myTvWishes.FocusedWishIndex = myTvWishes.ListAll().Count - 1;
            UpdateListItems();
        }
    }

    private void OnButtonMore()
    {
        Log.Debug("[TVWishListMP GUI_List]:OnButtonMore");
        
        
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        dlg.ShowQuickNumbers = false;
        dlg.SetHeading(PluginGuiLocalizeStrings.Get(1107));
        for (int i = 1500; i <= 1508; i++)
        {
            dlg.Add(PluginGuiLocalizeStrings.Get(i));
        }       
        dlg.DoModal(GUIWindowManager.ActiveWindow);
        MoreEvaluation(dlg.SelectedId);
    }

    protected void UpdateControls()
    {
        Log.Debug("[TVWishListMP]:UpdateControls");
       
        GUIPropertyManager.SetProperty("#textbox.label", "");
        GUIPropertyManager.SetProperty("#headershort.label", String.Format(PluginGuiLocalizeStrings.Get(1000)));

        myTvWishes.StatusLabel("");
        if (myTvWishes.ViewOnlyMode == false)
        {
            GUIControl.SetControlLabel(_mainwindowid, 2, String.Format(PluginGuiLocalizeStrings.Get(1100)));//Modus Viewonly
            GUIPropertyManager.SetProperty("#header.label", String.Format(PluginGuiLocalizeStrings.Get(1000)) + "  " + String.Format(PluginGuiLocalizeStrings.Get(4200))); //"TVWishList - Main-Email&Record");                        
            GUIPropertyManager.SetProperty("#modus.label", String.Format(PluginGuiLocalizeStrings.Get(4200))); //"Email&Record");
        }
        else
        {
            GUIControl.SetControlLabel(_mainwindowid, 2, String.Format(PluginGuiLocalizeStrings.Get(1101)));//Modus Email&Record
            GUIPropertyManager.SetProperty("#header.label", String.Format(PluginGuiLocalizeStrings.Get(3000)) + "  " + String.Format(PluginGuiLocalizeStrings.Get(4201))); //"TVWishList - Main-Viewonly");                            
            GUIPropertyManager.SetProperty("#modus.label", String.Format(PluginGuiLocalizeStrings.Get(4201))); //"Viewonly");            
        }



        GUIControl.SetControlLabel(_mainwindowid, 3, String.Format(PluginGuiLocalizeStrings.Get(1102)));//Run
        GUIControl.SetControlLabel(_mainwindowid, 4, String.Format(PluginGuiLocalizeStrings.Get(1103)));//View
        GUIControl.SetControlLabel(_mainwindowid, 5, String.Format(PluginGuiLocalizeStrings.Get(1104)));//New
        GUIControl.SetControlLabel(_mainwindowid, 6, String.Format(PluginGuiLocalizeStrings.Get(1105)));//Save
        GUIControl.SetControlLabel(_mainwindowid, 7, String.Format(PluginGuiLocalizeStrings.Get(1106)));//Cancel        
        GUIControl.SetControlLabel(_mainwindowid, 8, String.Format(PluginGuiLocalizeStrings.Get(1107)));//More       
    }

    public void UpdateListItems()
    {
        Log.Debug("[TVWishListMP]:UpdateListItems");

        //display messages
        //GUIControl.ClearControl(GetID, myListView.GetID);
        GUIListItem myGuiListItem;
        
        try
        {
            myListView.Clear();
            foreach (TvWish mywish in myTvWishes.ListAll())
            {
                string listitem="";
                Log.Debug("UpdateListItem name=" + mywish.name);
                if (_UserListItemFormat != "")
                    listitem = FormatTvWish(mywish, _UserListItemFormat); //user defined listitem format
                else if (myTvWishes.ViewOnlyMode == false)
                    listitem = FormatTvWish(mywish, PluginGuiLocalizeStrings.Get(1904));  //Email listitemformat
                else
                    listitem = FormatTvWish(mywish, PluginGuiLocalizeStrings.Get(1905));  //View Only listitemformat

                myGuiListItem = new GUIListItem();
                listitem=listitem.Replace("||", "\n");               
                string[] labels = listitem.Split('\n');
                Log.Debug("listitem=" + listitem);
                Log.Debug("labels.Length=" + labels.Length.ToString());
                if (labels.Length == 2)
                {
                    myGuiListItem.Label = labels[0];
                    myGuiListItem.Label2 = labels[1];
                }
                else
                {
                    myGuiListItem.Label = listitem;
                }
                
                myListView.Add(myGuiListItem);
               
            }
            if (myTvWishes.ListAll().Count == 0)
            {
                myGuiListItem = new GUIListItem(PluginGuiLocalizeStrings.Get(4301));  //"No items found"
                myListView.Add(myGuiListItem);
            }

            Log.Debug("[TVWishListMP]:UpdateListItems message number found:myListView.Count=" + myListView.Count.ToString());
        }
        catch (Exception exc)
        {
            myGuiListItem = new GUIListItem(PluginGuiLocalizeStrings.Get(4302));   //Error in creating item list
            myListView.Add(myGuiListItem);
            Log.Error("Error in creating item list - exception was:" + exc.Message);
        }
        
       
    }

    private void ItemSelectionChanged()
    {
        Log.Debug("[TVWishListMP]:ItemSelectionChanged");
        Log.Debug("Selection changed to item: "+myListView.SelectedListItemIndex.ToString());

        
        if (myTvWishes.ListAll().Count == 0)
            return;

        myTvWishes.FocusedWishIndex = myListView.SelectedListItemIndex;

        GUIWindowManager.ActivateWindow(_guieditwindowid); //redirect


        /*
        System.Threading.Thread.Sleep(200); //Wait for UP DOWN or DELETE Action
        if (ListItemActionClick == true) //no further action found
        {
            GUIWindowManager.ActivateWindow(_guieditwindowid); //redirect
        }*/
        //StatusLabel( "Selection changed to item: " + myListView.SelectedListItemIndex.ToString());
    }


    public void DeletePlayListItem()
    {
        try
        {
            if (myTvWishes.ListAll().Count == 0)
                return;

            int index = myListView.SelectedListItemIndex;
            myTvWishes.RemoveAtIndex(index);
            index = myListView.RemoveItem(index);
            myListView.SelectedListItemIndex = index;

            //update message.tvwishid if tvwish has been deleted or is unknown
            Log.Debug("mp list window before updating messages: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
            for (int i = mymessages.ListAllTvMessages().Count - 1; i >= 0; i--)
            {
                xmlmessage mymessage = mymessages.GetTvMessageAtIndex(i);
                TvWish mywish = myTvWishes.RetrieveById(mymessage.tvwishid);
                if (mywish == null)
                {
                    Log.Debug("deleting " + mymessage.title + " at " + mymessage.start.ToString() + " ID: " + mymessage.tvwishid);
                    mymessages.DeleteTvMessageAt(i);
                }
                
            }
            Log.Debug("mp list window after updating messages: TvMessages.Count=" + mymessages.ListAllTvMessages().Count.ToString());
            //StatusLabel( "Deleted item: " + index.ToString());
        }
        catch (Exception exc)
        {
            Log.Error("Error DeletePlayListItem: Exception " + exc.Message);
        }
        UpdateListItems();

    }

    private void MovePlayListItemDown()
    {
        try
        {
            if (myTvWishes.ListAll().Count == 0)
                return;

            int index = myListView.SelectedListItemIndex;
            if (myTvWishes.ListAll().Count > index + 1)
            {
                TvWish tempwish = myTvWishes.GetAtIndex(index);
                myTvWishes.ReplaceAtIndex(index, myTvWishes.GetAtIndex(index + 1));
                myTvWishes.ReplaceAtIndex(index + 1,tempwish);
            }
            else
            {
                TvWish tempwish = myTvWishes.GetAtIndex(index);
                myTvWishes.ReplaceAtIndex(index, myTvWishes.GetAtIndex(0));
                myTvWishes.ReplaceAtIndex(0,tempwish);
            }
            index = myListView.MoveItemDown(index);
            myListView.SelectedListItemIndex = index;
            //StatusLabel( "Move down item: " + index.ToString());
        }
        catch (Exception exc)
        {
            Log.Error("Error MovePlayListItemDown: Exception " + exc.Message);
        }
    }

    private void MovePlayListItemUp()
    {
        try
        {
            if (myTvWishes.ListAll().Count == 0)
                return;

            int index = myListView.SelectedListItemIndex;
            if (index > 0)
            {
                TvWish tempwish = myTvWishes.GetAtIndex(index);
                myTvWishes.ReplaceAtIndex(index, myTvWishes.GetAtIndex(index - 1));
                myTvWishes.ReplaceAtIndex(index - 1, tempwish);
            }
            else
            {
                TvWish tempwish = myTvWishes.GetAtIndex(index);
                myTvWishes.ReplaceAtIndex(index, myTvWishes.GetAtIndex(myTvWishes.ListAll().Count-1));
                myTvWishes.ReplaceAtIndex(myTvWishes.ListAll().Count - 1, tempwish);
            }
            index = myListView.MoveItemUp(index);
            myListView.SelectedListItemIndex = index;
            //StatusLabel( "Move up item: " + index.ToString());
        }
        catch (Exception exc)
        {
            Log.Error("Error MovePlayListItemUp: Exception " + exc.Message);
        }
    }


    public void SaveSettings()
    {
        Log.Debug("[TVWishListMP]:Savesettings");
        try
        {
            using (var reader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "TvWishListMP.xml")))
            {
                //reader.SetValue(SectionName, "TvwishlistFolder", TvwishlistFolder);
                reader.SetValue(SectionName, "TvWishItemSeparator", TvWishItemSeparator.ToString());
                reader.SetValue(SectionName, "Sort", GUI_List_window._sort.ToString());
                reader.SetValue(SectionName, "SortReverse", GUI_List_window._sortreverse.ToString());
                
                reader.SetValue(SectionName, "FilterEmailed", GUI_List_window._Email.ToString());
                reader.SetValue(SectionName, "FilterDeleted", GUI_List_window._Deleted.ToString());
                reader.SetValue(SectionName, "FilterConflicts", GUI_List_window._Conflicts.ToString());
                reader.SetValue(SectionName, "FilterScheduled", GUI_List_window._Scheduled.ToString());
                reader.SetValue(SectionName, "FilterRecorded", GUI_List_window._Recorded.ToString());
                reader.SetValue(SectionName, "FilterViewed", GUI_List_window._View.ToString());

                string defaultformats = "";
                for (int i = 0; i < (int)TvWishEntries.end; i++)
                {
                    defaultformats += myTvWishes.DefaultValues[i] + TvWishItemSeparator.ToString();
                    //Log.Debug("i=" + i.ToString() + "array_Eventformats[i]=" + array_Eventformats[i], (int)LogSetting.DEBUG);
                }
                defaultformats = defaultformats.Substring(0, defaultformats.Length - 1);
                Log.Debug("Save: defaultformats=" + defaultformats);
                reader.SetValue(SectionName, "DefaultFormats", defaultformats);     
            }
        }
        catch (Exception exc)
        {
            Log.Debug("Error SaveSettings: Exception " + exc.Message);
        }
    }
    
    public void LoadSettings()
    {
        Log.Debug("[TVWishListMP]:Loadsettings");
        try
        {
            using (var reader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "TvWishListMP.xml")))
            {
                
                //set log verbosity
                Log.DebugValue = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxDebug", "false"));
                
                _UserListItemFormat = reader.GetValueAsString(SectionName, "MainItemFormat", "");
                _UserListItemFormat = _UserListItemFormat.Replace("<br>", "\n");
                _UserListItemFormat = _UserListItemFormat.Replace("<BR>", "\n");

                _UserEmailFormat_Org = reader.GetValueAsString(SectionName, "EmailMainFormat", "");
                _UserEmailFormat_Org = _UserEmailFormat_Org.Replace("<br>", "\n");
                _UserEmailFormat_Org = _UserEmailFormat_Org.Replace("<BR>", "\n");

                _UserViewOnlyFormat_Org = reader.GetValueAsString(SectionName, "ViewMainFormat", "");
                _UserViewOnlyFormat_Org = _UserViewOnlyFormat_Org.Replace("<br>", "\n");
                _UserViewOnlyFormat_Org = _UserViewOnlyFormat_Org.Replace("<BR>", "\n");

                TvWishItemSeparator = Convert.ToChar(reader.GetValueAsString(SectionName, "TvWishItemSeparator", ";"));
                //TIMEOUT = Convert.ToInt32(reader.GetValueAsString(SectionName, "TimeOut", "60"));
                TimeOutValueString = reader.GetValueAsString(SectionName, "TimeOut", "60");
                
                //data for GUI_Edit_window
                myTvWishes._boolTranslator[(int)TvWishEntries.action] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxAction", "true"));
                myTvWishes._boolTranslator[(int)TvWishEntries.active] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxActive", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.afterdays] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxAfterDays", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.aftertime] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxAfterTime", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.beforedays] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxBeforeDay", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.beforetime] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxBeforeTime", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.channel] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxChannel", "true"));
                myTvWishes._boolTranslator[(int)TvWishEntries.episodename] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxEpisodeName", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.episodenumber] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxEpisodeNumber", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.episodepart] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxEpisodePart", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.exclude] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxExclude", "true"));
                myTvWishes._boolTranslator[(int)TvWishEntries.group] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxGroup", "true"));
                //myTvWishes._boolTranslator[(int)TvWishEntries.hits] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxHits", "true"));
                myTvWishes._boolTranslator[(int)TvWishEntries.keepepisodes] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxKeepEpisodes", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.keepuntil] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxKeepUntil", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.matchtype] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxMatchType", "true"));
                myTvWishes._boolTranslator[(int)TvWishEntries.name] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxEpisodeName", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.postrecord] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxPostrecord", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.prerecord] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxPrerecord", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.priority] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxPriority", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.recommendedcard] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxRecommendedCard", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.recordtype] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxRecordtype", "true"));
                myTvWishes._boolTranslator[(int)TvWishEntries.searchfor] = true;
                myTvWishes._boolTranslator[(int)TvWishEntries.seriesnumber] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxSeriesNumber", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.skip] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxSkip", "true"));
                myTvWishes._boolTranslator[(int)TvWishEntries.useFolderName] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxUseFolderNames", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.withinNextHours] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxRecordtype", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.episodecriteria] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxEpisodeCriteria", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.preferredgroup] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxPreferredGroup", "false"));
                myTvWishes._boolTranslator[(int)TvWishEntries.includerecordings] = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxIncludeRecordings", "false"));
                
                //modify for listview table changes
                myTvWishes.DisableInfoWindow = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxDisableInfoWindow", "false"));
                TvWishListQuickMenu = Convert.ToBoolean(reader.GetValueAsString(SectionName, "checkBoxDisableQuickMenu", "false"));

                //create backup vector for usage after view only mode
                for (int i = 0; i < myTvWishes._boolTranslator.Length; i++)
                {
                    myTvWishes._boolTranslatorbackup[i] = myTvWishes._boolTranslator[i];
                }

                //load user defined default formats
                string userDefaultFormatsString = reader.GetValueAsString(SectionName, "DefaultFormats", myTvWishes.DefaultFormatString);

                if (userDefaultFormatsString == string.Empty)
                    userDefaultFormatsString = "True";//all other defaults will be set from checking below

                Log.Debug("DefaultFormatsString=" + userDefaultFormatsString);
                string[] userDefaultFormats = myTvWishes.CheckColumnsEntries(userDefaultFormatsString.Split(TvWishItemSeparator), TvWishItemSeparator,true);
                
                for (int i = 0; i < userDefaultFormats.Length; i++)
                { //overwrite myTvWishes.DefaultValues with user defined default values
                    myTvWishes.DefaultValues[i] = userDefaultFormats[i];
                }
                //id default must be -1 
                myTvWishes.DefaultValues[(int)TvWishEntries.tvwishid] = "-1";
                //counter defaults must be 0
                myTvWishes.DefaultValues[(int)TvWishEntries.viewed] = "0";
                myTvWishes.DefaultValues[(int)TvWishEntries.emailed] = "0";
                myTvWishes.DefaultValues[(int)TvWishEntries.conflicts] = "0";
                myTvWishes.DefaultValues[(int)TvWishEntries.deleted] = "0";
                myTvWishes.DefaultValues[(int)TvWishEntries.recorded] = "0";
                myTvWishes.DefaultValues[(int)TvWishEntries.scheduled] = "0";

                //data for GUI_List_window         
                GUI_List_window._sort = Convert.ToInt32(reader.GetValueAsString(SectionName, "Sort", "1"));
                GUI_List_window._sortreverse = Convert.ToBoolean(reader.GetValueAsString(SectionName, "SortReverse", "false"));
                GUI_List_window._Email = Convert.ToBoolean(reader.GetValueAsString(SectionName, "FilterEmailed", "true"));
                GUI_List_window._Deleted = Convert.ToBoolean(reader.GetValueAsString(SectionName, "FilterDeleted", "false"));
                GUI_List_window._Conflicts = Convert.ToBoolean(reader.GetValueAsString(SectionName, "FilterConflicts", "false"));
                GUI_List_window._Scheduled = Convert.ToBoolean(reader.GetValueAsString(SectionName, "FilterScheduled", "true"));
                GUI_List_window._Recorded = Convert.ToBoolean(reader.GetValueAsString(SectionName, "FilterRecorded", "false"));
                GUI_List_window._View = Convert.ToBoolean(reader.GetValueAsString(SectionName, "FilterViewed", "true"));

                GUI_List_window._UserListItemFormat = reader.GetValueAsString(SectionName, "ResultsItemFormat", "");
                GUI_List_window._UserListItemFormat = GUI_List_window._UserListItemFormat.Replace("<br>", "\n");
                GUI_List_window._UserListItemFormat = GUI_List_window._UserListItemFormat.Replace("<BR>", "\n");

                GUI_List_window._UserEmailFormat = reader.GetValueAsString(SectionName, "EmailResultsFormat", "");
                GUI_List_window._UserEmailFormat = GUI_List_window._UserEmailFormat.Replace("<br>", "\n");
                GUI_List_window._UserEmailFormat = GUI_List_window._UserEmailFormat.Replace("<BR>", "\n");

                GUI_List_window._UserViewOnlyFormat = reader.GetValueAsString(SectionName, "ViewResultsFormat", "");
                GUI_List_window._UserViewOnlyFormat = GUI_List_window._UserViewOnlyFormat.Replace("<br>", "\n");
                GUI_List_window._UserViewOnlyFormat = GUI_List_window._UserViewOnlyFormat.Replace("<BR>", "\n");

                string _userDateTimeFormat = reader.GetValueAsString(SectionName, "DateTimeFormat", "");
                if (_userDateTimeFormat != "")
                {
                    mymessages.date_time_format = _userDateTimeFormat;
                    Log.Debug("GUI_List_window.mymessages.date_time_format changed to "+mymessages.date_time_format);
                }            
            }
        }
        catch (Exception exc)
        {
            Log.Error("Error LoadSettings: Exception " + exc.Message);
        }
    }


/*
    public void DebugWindow(string line1)
    {
        DebugWindow(line1, "", "");
    }
    public void DebugWindow(string line1, string line2)
    {
        DebugWindow(line1, line2, "");
    }
    public void DebugWindow(string line1,string line2, string line3)
    {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        dlg.SetHeading("Debug");
        dlg.Add(line1);
        dlg.Add(line2);
        dlg.Add(line3);
        
        dlg.DoModal(GUIWindowManager.ActiveWindow);
    }*/

  }


}
   
