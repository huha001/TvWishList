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

using MediaPortal.Plugins.SlimTv.Interfaces.Extensions;
using MediaPortal.Plugins.SlimTv.Interfaces.Items;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Plugins.TvWishListMP2.Models;
using Log = TvLibrary.Log.huha.Log;
namespace MediaPortal.Plugins.TvWishListMP2.ExtensionButtons
{
  /// <summary>
  /// Example class for definition of <see cref="IProgramAction"/>s.
  /// </summary>
  class SlimTvCreateTvWish: IProgramAction
  {
    //ILogger Log = ServiceRegistration.Get<ILogger>();
    public bool DoSomethingWithProgram(IProgram program)
    {
        Log.Debug("SlimTv Button Create TvWish was pressed");
        if (Main_GUI.Instance != null)
        {
            Main_GUI.Instance.Title(program.Title);
            Main_GUI.Instance.Name(program.Title);
            string mycommand = "NEWTVWISH//TITLE//NAME";
            Log.Debug("Executing command " + mycommand);
            Main_GUI.Instance.Command(mycommand);
            

        }
        return true;
    }

    public bool IsAvailable (IProgram program)
    {
      return true;
    }

    public ProgramActionDelegate ProgramAction
    {
      get { return DoSomethingWithProgram; }
    }
  }
}
