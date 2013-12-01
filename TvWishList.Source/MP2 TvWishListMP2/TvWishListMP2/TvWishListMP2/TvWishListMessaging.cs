using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MediaPortal.Common;
using MediaPortal.Common.Messaging;

namespace MediaPortal.Plugins.TvWishList
{
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
