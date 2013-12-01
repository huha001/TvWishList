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

using MediaPortal.Common;
using MediaPortal.Common.Commands;
using MediaPortal.Common.General;
using MediaPortal.Common.Localization;
using MediaPortal.Common.Logging;


using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Screens;
using MediaPortal.UI.Presentation.Workflow;
using MediaPortal.UI.Presentation.Models;

namespace MediaPortal.Plugins.TvWishList
{
    // generate MP2 localized strings with MP1 code and a converted language file with MP2LanguageConverter
    public class PluginGuiLocalizeStrings
    {
        private static string _section;

        public static string MP2Section
        {
            get { return _section; }
            set { _section = value; }
        }

        public PluginGuiLocalizeStrings(string section)
        {
            _section = section;
        }

        

        public static string Get(int number)
        {
            string text = "[" + _section + "." + number.ToString() + "]";   //"[MyTestPlugin.FunctionButton]";
            string stringMP2Text = LocalizationHelper.Translate(text);
            return stringMP2Text;
        }
    }

    
}
