


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using System.Globalization;
//using System.Windows.Forms;
//using System.ComponentModel;
//using System.Diagnostics;
//using System.Data;
//using System.Drawing;

using System.IO;
//using System.Linq;
//using System.Text;

//using System.Xml;

//using System.IO.Pipes;
//using System.Security.Principal;
//using System.Threading;

using Log = TvLibrary.Log.huha.Log;

namespace MediaPortal.Plugins.TvWishList
{
    
        public enum PipeCommands
        {
            RequestTvVersion = 1,
            StartEpg,
            ImportTvWishes,
            ExportTvWishes,
            RemoveSetting,
            RemoveRecording,
            RemoveLongSetting,
            ReadAllCards,
            ReadAllChannels,
            ReadAllChannelsByGroup,
            ReadAllRadioChannels,
            ReadAllRadioChannelsByGroup,
            ReadAllChannelGroups,
            ReadAllRadioChannelGroups,
            ReadAllRecordings,
            ReadAllSchedules,
            ReadSetting,
            ScheduleDelete,
            ScheduleNew,
            WriteSetting,
            Ready,
            Error,
            Error_TimeOut,
            UnknownCommand,
        }
    

    //needed for pipes:
    // Defines the data protocol for reading and writing strings on our stream
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = 0;

            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                Log.Debug("***ERROR: Stream was too large and had to be truncated");
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }

    //needed for pipes:
    // Contains the method executed in the context of the impersonated user
    public class ReadFileToStream
    {
        private string fn;
        private StreamString ss;

        public ReadFileToStream(StreamString str, string filename)
        {
            fn = filename;
            ss = str;
        }

        public void Start()
        {
            string contents = File.ReadAllText(fn);
            ss.WriteString(contents);
        }
    }
}
