#region Copyright (C) 2007-2012 Team MediaPortal

/*
    Copyright (C) 2007-2011 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion


using System;
using System.Collections.Generic;
using System.Threading;

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
using MediaPortal.UI.Presentation.UiNotifications;

using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishListMP2NativeTvProvider
{
    /// <summary>
    /// Base class for models which are registered to messages from the system and which are listening for
    /// messages all over their lifetime.
    /// </summary>
    /// <remarks>
    /// In general, workflow models should not be derived from this class as workflow models are normally receiving
    /// messages only during the time when they are active. The other time they normally will temporary shut down their
    /// message queue.
    /// </remarks>
    public class TvWishListNativeTvMessageModel : BaseMessageControlledModel  //this can pe used for a process plugin and will be active all the time listening for messages
    {
        protected object _syncObj = new object();

        /// <summary>
        /// Creates a new <see cref="BaseMessageControlledModel"/> instance and starts the message queue.
        /// </summary>
        /// 
        protected TvWishListNativeTvMessageModel()
        {
            Log.Debug("TvWishListNativeTvMessageModel constructed");
            SubscribeToMessages();
        }


        void SubscribeToMessages()
        {
         
            _messageQueue.SubscribeToMessageChannel(SystemMessaging.CHANNEL);
            _messageQueue.SubscribeToMessageChannel(TvWishListMessaging.CHANNEL);
            _messageQueue.SubscribeToMessageChannel(NotificationServiceMessaging.CHANNEL);
            
            _messageQueue.MessageReceived += OnMessageReceived;
            _messageQueue.ThreadPriority = ThreadPriority.BelowNormal;
        }

        void OnMessageReceived(AsynchronousMessageQueue queue, SystemMessage message)
        {
            Log.Debug("OnMessageReceived Channel="+message.ChannelName);
            if (message.ChannelName == SystemMessaging.CHANNEL)
            {
                //do something
                Log.Debug("SystemServiceMessaging received = "+message.MessageData[SystemMessaging.NEW_STATE].ToString());
                
            }
            else if (message.ChannelName == TvWishListMessaging.CHANNEL)
            {
                //do something
                Log.Debug("MyTestPluginMessaging received message=" + message.MessageData[TvWishListMessaging.MESSAGE].ToString());
                //update pipename
            }
            else if (message.ChannelName == NotificationServiceMessaging.CHANNEL)
            {
                //do something
                Log.Debug("NotificationServiceMessaging received = " + message.MessageData[NotificationServiceMessaging.NOTIFICATION].ToString());               
            }
        } 

        
    }


    public static class TvWishListMessaging
    {
        // Message channel name
        public const string CHANNEL = "TvWishList";
        public const string MESSAGE = "Message";

        public static void SendTvWishListMessage(TvWishListMessaging.MessageType type, string mymessage)
        {
            // Send Startup Finished Message.
            SystemMessage msg = new SystemMessage(type);
            msg.MessageData[MESSAGE] = mymessage;
            ServiceRegistration.Get<IMessageBroker>().Send(CHANNEL, msg);
        }

        public enum MessageType
        {
            Initialization = 0,
            OnPageLoad = 1,
        }
    }
    
}




