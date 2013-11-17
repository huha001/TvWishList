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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections.Generic;

using TvDatabase;
using Gentle.Framework;
using TvControl;
using MediaPortal.GUI.Library;
using Log = TvLibrary.Log.huha.Log;

//using System.Data.SQLite;
//using MySql.Data.MySqlClient;

namespace TvWishList
{
    
    public class ServiceInterface : System.Web.Services.WebService
    {
       public string gentleConfig;
        public string connStr;
       
        public bool ConnectToDatabase()
        {
            Log.Debug("");
            Log.Debug("RemoteControl.IsConnected=" + RemoteControl.IsConnected.ToString());
                        
            try
            {
                if (RemoteControl.IsConnected)
                    return true;

                string provider = "";
                RemoteControl.HostName = Environment.MachineName;
                RemoteControl.Instance.GetDatabaseConnectionString(out connStr, out provider);
                Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connStr);               
            }
            catch (Exception exc)
            {
                Log.Error("TvWishListMP: Connecting to Tvserver database failed");
                Log.Error("Exception is:" + exc.Message);
                return false;
            }
            Log.Debug("Connected to Tv database");
            return true;
        }

        



    }
}