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
using MediaPortal.Common.Logging;
//using MPExtended.Services.TVAccessService.Interfaces;
using MediaPortal.Plugins.TvWishListMP2.MPExtended;
using MediaPortal.Plugins.TvWishListMP2.Models;
using TvWishList;
using System.Threading;
using Log = TvLibrary.Log.huha.Log;


namespace MediaPortal.Plugins.TvWishListMP2.MPExtended
{
    public class Recording : IRecording
    {
        
        #region Recording Member

        public int IdRecording { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public int IdChannel { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        #endregion Recording Member

        //ILogger Log = ServiceRegistration.Get<ILogger>();

        #region public methods
        public static IList<Recording> ListAll()
        {
            IList<IRecording> allIRecordings = null;

            if (Main_GUI.MyTvProvider == null)
            {
                Main_GUI.MyTvProvider = ServiceRegistration.Get<ITvWishListTVProvider>();
            }

            if (Main_GUI.MyTvProvider.Initialized == false)
            {
                Main_GUI.MyTvProvider.Init();
            }

            Main_GUI.MyTvProvider.ReadAllRecordings(out allIRecordings);

            IList<Recording> allrecordings = new List<Recording>();
            foreach (IRecording myrecording in allIRecordings)
            {
                allrecordings.Add(new Recording
                {
                    IdRecording = myrecording.IdRecording,
                    Title = myrecording.Title,
                    IdChannel = myrecording.IdChannel,
                    StartTime = myrecording.StartTime,
                    EndTime = myrecording.EndTime,
                    FileName = myrecording.FileName });
            }

            return allrecordings;
        }

        public void Delete()
        {
            Log.Debug("Delete Recording");

            string response = string.Empty;
            string command = Main_GUI.PipeCommands.RemoveRecording.ToString() + this.IdRecording.ToString() + ":10";
            Log.Debug("command=" + command);
            for (int i = 1; i < 120; i++)
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

        #endregion public methods
    }
}
