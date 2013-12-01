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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//using TvWishList;
using Log = TvLibrary.Log.huha.Log;

#if (MP11 || MP12 || MP16)

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using Action = MediaPortal.GUI.Library.Action;
//using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;
using MediaPortal.Configuration;
using TvControl;
using TvDatabase;
using Gentle.Framework;
//using GUIKeyboard = MediaPortal.GUI.Library.huha.GUIKeyboard;

#elif (MP2)
using MediaPortal.Common;
using MediaPortal.Common.Commands;
using MediaPortal.Common.General;
using MediaPortal.Common.Logging;
using MediaPortal.Common.Messaging;
using MediaPortal.Common.Runtime;
using MediaPortal.Common.Settings;
using MediaPortal.Common.Localization;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Workflow;
using MediaPortal.UI.Presentation.Screens;
using MediaPortal.UI.Control.InputManager;
using MediaPortal.UI.Presentation.UiNotifications;

using MediaPortal.UI.SkinEngine.ScreenManagement;
using MediaPortal.UI.SkinEngine.Controls.Visuals;

using MediaPortal.Plugins.TvWishList;
using MediaPortal.Plugins.TvWishList.Items;
using MediaPortal.Plugins.TvWishListMP2.Settings; //needed for configuration setting loading
#endif

//Version 0.0.0.1

#if (MP11 || MP12 || MP16)
namespace MediaPortal.Plugins.TvWishList
#elif (MP2)
namespace MediaPortal.Plugins.TvWishListMP2.Models
#endif

{
    public partial class Result_GUI
    {
        #region globals
        //Instance
        public static Result_GUI _instance = null;

        public static Result_GUI Instance
        {
            get { return (Result_GUI)_instance; }
        }

        // default format strings for text boxes at init() initialized from language file and customized user formats
        public string _TextBoxFormat_4to3_EmailFormat = "";
        public string _TextBoxFormat_16to9_EmailFormat = "";
        public string _UserEmailFormat = "";
        public string _TextBoxFormat_4to3_ViewOnlyFormat = "";
        public string _TextBoxFormat_16to9_ViewOnlyFormat = "";
        public string _UserViewOnlyFormat = "";
        //public string _UserDateTimeFormat = "";
        public string _UserListItemFormat = "";
        //end formats



        public static string SectionName = "TvWishListMP";
        XmlMessages mymessages = null;
        TvWishProcessing myTvWishes = null;
        TvBusinessLayer layer = new TvBusinessLayer();

        private string _xmlfile = "";
        public string XMLfile
        {
            get { return _xmlfile; }
            set { _xmlfile = value; }
        }


        public TvWishProcessing TvWishes
        {
            get { return myTvWishes; }
            set { myTvWishes = value; }
        }

        public XmlMessages MyMessages
        {
            get { return mymessages; }
            set { mymessages = value; }
        }

        

        

        #endregion globals


        #region private methods


        

        #endregion private methods

    }
}