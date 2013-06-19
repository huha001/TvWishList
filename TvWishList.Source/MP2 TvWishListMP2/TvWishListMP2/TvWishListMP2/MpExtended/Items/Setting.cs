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


using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Plugins.TvWishListMP2.MPExtended;
using MediaPortal.Plugins.TvWishListMP2.Models;
using TvWishList;
using System.Threading;
using MediaPortal.Plugins.TvWishListMP2.Models;
using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishListMP2.MPExtended
{
  public class Setting : ISetting
  {
      //ILogger Log = ServiceRegistration.Get<ILogger>();
      #region Setting Member

      public string Tag { get; set; }

      public string Value { get; set; }

      #endregion

      public Setting()
      {

      }

      public void Persist()
      {
          Log.Debug("Setting:  Persist()");

          if (Main_GUI.MyTvProvider == null)
          {
              Main_GUI.MyTvProvider = ServiceRegistration.Get<ITvWishListTVProvider>();
          }

          if (Main_GUI.MyTvProvider.Initialized == false)
          {
              Main_GUI.MyTvProvider.Init();
          }

          Main_GUI.MyTvProvider.WriteSetting(Tag, Value);

      }

      
      public void Remove()
      {
          string response = string.Empty;
          string command = Main_GUI.PipeCommands.RemoveSetting.ToString()+this.Tag+":10";
          for (int i = 1; i < 120;i++ )
          {
              response = Main_GUI.Instance.RunSingleCommand(command);
              if (response == PluginGuiLocalizeStrings.Get(1200))  //Waiting for old process to finish
              {
                  Log.Debug("Waiting for old process to finish");
                  Thread.Sleep(1000);
              }
              else
              {
                  break;
              }

              
          }
          Log.Debug("Server response=" + response);
      }

  }
}