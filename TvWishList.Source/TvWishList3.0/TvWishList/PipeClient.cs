//******************************
// pipe client version 1.0.0.1
//******************************

using System;
using System.Collections.Generic;

using System.Globalization;
using System.Windows.Forms;
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


using Log = TvLibrary.Log.huha.Log;

#if (MP2)
using MediaPortal.Plugins.TvWishListMP2.Models;
using MediaPortal.UI.Presentation.Workflow;
using MediaPortal.Common;
using MediaPortal.Plugins.TvWishList.Items;
#elif (MPTV2)
using MediaPortal.Plugins.TvWishList.Items;
using MediaPortal.Plugins.TvWishList.Setup;
#endif

//using Mediaportal.TV.Server.MP1Translation;
//using TvEngine;
//using SetupTv.Sections;
//using MyTVMail;


namespace MediaPortal.Plugins.TvWishList
{
    public delegate void setupPipeLabelMessage(string text, PipeCommands type);

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
#if (MPTV2)
    [CLSCompliant(false)]
#endif
    public class PipeClient //This is a fake for the MPTV2 Pipe Client to reuse the code
    {
        //Instance
        public static PipeClient _instance = null;

        public static PipeClient Instance
        {
            get { return (PipeClient)_instance; }
        }

        //*************************************
        //pipe client 
        //*************************************

        NamedPipeClientStream pipeClient = null;
        string ClientMessage = string.Empty;
        string ReceivedMessage = "Error";
        //bool OldConnect = false;
        string TimeOutValueString = "1200";               //600s default, will be read from config settings?
        

        //Pipelocking mechanism
        System.Threading.Thread PipeRunThread;
        System.Threading.Thread PipeRunThreadTimeOutCounter = null;
        bool PipeRunThreadActive = false;
        bool _Debug = false;

        string pipeName = "**ERROR: NOTDEFINED";
        string hostName = "**ERROR: NOTDEFINED";

        public bool LOCKED = false;
        public bool TIMEOUT = false;
        
#if (MP11 || MP12 || MP16 || MPTV2 || TV30 || MP2)
        TvWishProcessing myTvWishes = null;
#endif
        LanguageTranslation lng = null;

        public event setupPipeLabelMessage newlabelmessage;

        public bool Debug
        {
            get { return _Debug; }
            set
            {
                _Debug = value;
//#if (!MP2)
                Log.DebugValue = _Debug;
//#endif
            }

        }

        public string HostName
        {
            get { return hostName; }
            set 
            { 
                hostName = value;
                if (hostName == "localhost")
                {
                    hostName = ".";
                }         
            }
        }

        # region constructor

#if (MPTV2)
        public PipeClient(TvWishProcessing myTvWishesTransfer, LanguageTranslation lngTransfer, string host, string pipe)
        {
            myTvWishes = myTvWishesTransfer;
            lng = lngTransfer;
            myTvWishes.ViewOnlyMode = false; //tvserver has no view only mode
        
#elif (MP2)  //needs update by on page load MainGui
        public PipeClient(TvWishProcessing myTvWishesTransfer, string host, string pipe)
        {
            myTvWishes = myTvWishesTransfer;

#elif (MP11 || MP12 || MP16) //needs update by on page load MainGui
public PipeClient(TvWishProcessing myTvWishesTransfer, string host, string pipe)
        {
            myTvWishes = myTvWishesTransfer;           
        
#else //native TV provider
        public PipeClient(string host, string pipe)
        {
            lng = new LanguageTranslation();
                    
#endif

            _instance = this;
            pipeName = pipe;
            hostName = host;
            if (hostName == "localhost")
            {
                hostName = ".";
            }
            else

            Log.Debug("Pipe client initiated");
        }
        #endregion constructor

        #region client pipe methods


#if (MPTV2 || TV30 || MP11 || MP12 || MP16 || MP2)
        public void OnButtonRun()
        {
            Log.Debug("EPG_RUN");


            //button lock
            if (myTvWishes.ButtonActive)
            {
#if (MPTV2 || TV30)
                string temp = lng.TranslateString("Waiting for old process to finish - Previous Operation Is Still Running", 1200);
#else
                myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Operation Is Still Running"
#endif               
                return;
            }
            myTvWishes.ButtonActive = true; //lock buttons

            try
            {

                if (PipeRunThread == null)
                {
                    Log.Debug("[TVWishListMP]:OnButtonRun Run Thread = null - starting new thread");
                    PipeRunThread = new System.Threading.Thread(RunThreadProcessing);
                    PipeRunThread.Start();
                    //StatusLabel( "New Thread Started - was null");
                }
                else if (PipeRunThread.IsAlive == false)
                {
                    Log.Debug("[TVWishListMP]:OnButtonRun Run Thread not alive - starting new thread");
                    PipeRunThread = new System.Threading.Thread(RunThreadProcessing);
                    PipeRunThread.Start();
                    //StatusLabel( "New Thread Started - was alive");
                }
                else
                {
                    Log.Debug("[TVWishListMP]:OnButtonRun: Old Thread must be running - do nothing");
#if (MPTV2 || TV30)
                    string temp = lng.TranslateString("Waiting for old process to finish - Previous Operation Is Still Running", 1200);
#else
                    myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Search Is Still Running"
#endif               
                }
            }
            catch (Exception ex)
            {
                Log.Error("Run Thread could not be started - exception message is\n" + ex.Message);
            }


        }



        public void RunThreadProcessing()
        {
            Log.Debug("[TVWishListMP]:OnButtonRun");
#if (TV30)
            // do nothing
#elif (MPTV2)
            TvWishListSetup.Instance.MySaveSettings(); //save data first before unlocking
#else
            Main_GUI.Instance.TvserverdatabaseSaveSettings();//save data first before unlocking
#endif

            //*****************************************************
            //unlock TvWishList
            
                bool ok = myTvWishes.UnLockTvWishList();
                if (!ok)
                {
                    Log.Error("Could not unlock before starting tvserver processing=");
                    myTvWishes.ButtonActive = false; //unlock buttons
                    return;
                }
                LOCKED = false;
#if (TV30)
            //do nothing
#elif (MPTV2 || TV30)
                TvWishListSetup.Instance.LoadSettingError = true; //do not save data anymore after unlocking
#else
                Main_GUI.Instance.TvServerLoadSettings_FAILED = true; //do not save data anymore after unlocking
#endif
                Log.Debug("Successfule unlock");
            

            //start versioncommand  - not needed
            //ClientMessage = PipeCommands.RequestTvVersion.ToString();
            //ClientThreadRoutineRunSingleCommand();
            //Log.Debug("TvServer Version = " + ReceivedMessage);

            //start runepgmode command

            //ClientMessage = PipeCommands.StartEpg.ToString() + "VIEWONLY=FALSE TIMEOUT:" + TimeOutValueString;


            if (myTvWishes.ViewOnlyMode)
            {
                ClientMessage = PipeCommands.StartEpg.ToString() + "VIEWONLY=TRUE TIMEOUT:" + TimeOutValueString;
            }
            else
            {
                ClientMessage = PipeCommands.StartEpg.ToString() + "VIEWONLY=FALSE TIMEOUT:" + TimeOutValueString;
            }


            if (PipeRunThreadActive)
            {
#if (MPTV2 || TV30)
                labelmessage(lng.TranslateString("Waiting for old process to finish - Previous Operation Is Still Running",1200), PipeCommands.StartEpg);
#else              
                myTvWishes.StatusLabel(PluginGuiLocalizeStrings.Get(1200));  //Waiting for old process to finish   "Previous Operation Is Still Running"
#endif
                myTvWishes.ButtonActive = false; //unlock buttons
                return;
            }

            PipeRunThreadActive = true; //lock pipe after saving settings
            Log.Debug("starting ClientRoutineRunEpg");
            ClientRoutineRunEpg();
            Log.Debug("completed ClientRoutineRunEpg");
            PipeRunThreadActive = false; //unlock pipe

            //*****************************************************
            //Lock Tvwishes
            bool success2 = myTvWishes.LockTvWishList("TvWishList:Setup");
            if (!success2)
            {
                MessageBox.Show(lng.TranslateString("Tv wish list is being processed by another process<br>Try again later<br>If the other process hangs reboot the system or stop the tv server manually",4311).Replace("<br>",Environment.NewLine));
                //myTvWishes.MyMessageBox(4305, 4311); //Tv wish list is being processed by another process<br>Try again later<br>If the other process hangs reboot the system or stop the tv server manually
                myTvWishes.ButtonActive = false; //unlock buttons
                LOCKED = false; //unlock tvwishlist
                
#if (TV30)
                // do nothing
#elif (MPTV2)
                TvWishListSetup.Instance.LoadSettingError = true; //do not save data anymore after unlocking
#else
                Main_GUI.Instance.TvServerLoadSettings_FAILED = true; //do not save data anymore after unlocking
                
#endif

#if (MP2)
                IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
                workflowManager.NavigatePop(1); //same as escape (takes one entry from the stack)
#elif (MP11 || MP12 || MP16)
                MediaPortal.GUI.Library.GUIWindowManager.ActivateWindow(1); //goto tv mainwindow to avoid a partial loop
#endif



            }
            else
            {
                LOCKED = true;
#if (TV30)
                //do nothing
#elif (MPTV2)
                TvWishListSetup.Instance.MyLoadSettings();
#else
                Main_GUI.Instance.TvserverdatabaseLoadSettings();
                Main_GUI.Instance.UpdateListItems();
#endif
            }



            //myTvWishes.MyMessageBox(4400, 1204);  //Epg Search Completed
            //myTvWishes.StatusLabel(statusmessage.Replace(@"\n", "")); //overwrite with last status message in case an error occured on the server
            Log.Debug("[TVWishListMP]:RunThreadProcessing Completed");

            myTvWishes.ButtonActive = false; //unlock buttons
#if (MPTV1)
            TvWishListSetup.Instance.BUSY = false;
#endif
        }
#endif

        public void StopClient()
        {
            try
            {
                if (pipeClient != null)
                {
                    pipeClient.Close();
                    if (pipeClient != null)
                        pipeClient = null;
                    Log.Debug("Pipeclient closed");
                    Thread.Sleep(1000);
                }

                if (PipeRunThread != null)
                {
                    PipeRunThread.Abort();
                    PipeRunThread = null;
                }
            }
            catch (Exception exc)
            {
                Log.Debug("StopClient() exception=" + exc.Message);
            }
        }

        


        private void ClientRoutineRunEpg()
        {
            bool success = false;
            for (int i = 1; i <= 3; i++) //three retries for sending command
            {
                TIMEOUT = false;
                try
                {
                    Log.Debug("ClientRoutineRunEpg: Connecting to server " + hostName);
                    Log.Debug("pipe=" + pipeName);
                    Log.Debug("hostname=" + hostName);

                    pipeClient = new NamedPipeClientStream(hostName, pipeName,
                            PipeDirection.InOut, PipeOptions.None,
                            TokenImpersonationLevel.Impersonation);

                    pipeClient.Connect();
                    StreamString ss = new StreamString(pipeClient);
                    //send clientmessage to server
                    Log.Debug("Writing command " + ClientMessage);
                    ss.WriteString(ClientMessage);
                    ReceivedMessage = "ClientThreadRoutineRunEpg Error";
                    while ((ReceivedMessage.StartsWith(PipeCommands.Ready.ToString()) == false) && (ReceivedMessage.StartsWith(PipeCommands.Error.ToString()) == false))
                    {
                        PipeRunThreadTimeOutCounter = new Thread(ClientTimeOutError);
                        PipeRunThreadTimeOutCounter.Start();
                        ReceivedMessage = ss.ReadString();
                        PipeRunThreadTimeOutCounter.Abort();
                        //evaluate response from server
                        string Processedmessage = string.Empty;
                        if (ReceivedMessage.StartsWith(PipeCommands.Ready.ToString()) == true)
                        {
                            Processedmessage = ReceivedMessage.Substring(PipeCommands.Ready.ToString().Length);
                        }                          
                        else if (ReceivedMessage.StartsWith(PipeCommands.Error.ToString()) == true)
                        {
                            Processedmessage = ReceivedMessage.Substring(PipeCommands.Error.ToString().Length);
                        }
                        else
                        {
                            Processedmessage = ReceivedMessage;
                        }
                        Log.Debug("***** SERVERMESSAGE=" + ReceivedMessage);



                        labelmessage(Processedmessage, PipeCommands.StartEpg); 
                        //myTvWishes.StatusLabel(Processedmessage);
                        Log.Debug("***** SERVERMESSAGE=" + ReceivedMessage);
                    }

                    Log.Debug("closing client pipe - command executed");
                    if (pipeClient != null)
                    {
                        pipeClient.Close();
                        if (pipeClient != null)
                            pipeClient = null;
                    }

                    

                    success = true;
                    break;
                }
                catch (Exception exc)
                {
                    Log.Debug("Sending tv server command failed in iteration i=" + i.ToString());



                    Log.Debug("ClientThread() exception=" + exc.Message);
                    Thread.Sleep(2000);
                    if (pipeClient != null)
                    {
                        pipeClient.Close();
                        if (pipeClient != null)
                            pipeClient = null;
                    }

                    if (TIMEOUT)
                    {
                        TIMEOUT = false;
                        break;
                    }
                }
            }

            if (success == false)
            {
                //MessageBox.Show(lng.TranslateString("TvWishList MediaPortal Plugin Does Not Match To TvWishList TV Server Plugin", 1206).Replace("<br>", "\n"));   
                MessageBox.Show(lng.TranslateString("TvWishList MediaPortal Plugin Does Not Match To TvWishList TV Server Plugin", 1206).Replace("<br>", Environment.NewLine),"Error"); 
               // myTvWishes.MyMessageBox(4305, 1206); //TvWishList MediaPortal Plugin Does Not Match To TvWishList TV Server Plugin
            }
            Log.Debug("ClientThreadRoutineRunEpg() Thread Completed");
            //OldConnect = false;
        }


        private void ClientTimeOutError()
        {
            TIMEOUT = false;
            int timeOutValue = 60;
            try
            {
                timeOutValue = Convert.ToInt32(TimeOutValueString);
            }
            catch { }

            for (int i = 0; i < timeOutValue; i++)
            {
                Thread.Sleep(1000);
            }
            TIMEOUT = true;
            Log.Debug("Timeout Error Client - stopping");
            StopClient();
            ReceivedMessage = PipeCommands.Error_TimeOut.ToString();
            //myTvWishes.StatusLabel(ReceivedMessage);
            labelmessage(ReceivedMessage, PipeCommands.Error_TimeOut);

            //myTvWishes.MyMessageBox(4305, 4312); //Timeout Error: Try to reboot your server.\nIf it reoccurs try to increase the timeout value in the expert settings
            labelmessage(lng.TranslateString("Timeout Error: Try to reboot your server.\nIf it reoccurs try to increase the timeout value in the expert settings", 4312), PipeCommands.Error_TimeOut);

            PipeRunThreadActive = false; //always needed because pipe is stopped

            
#if (MPTV2)
            if (ClientMessage.StartsWith(PipeCommands.StartEpg.ToString()))
                myTvWishes.ButtonActive = false; //unlock buttons only for StartEPG command
#endif
        }

        public string RunSingleCommand(string command)
        {
            Log.Debug("RunSingleCommand=" + command);
            if (PipeRunThreadActive)
            {
                string temp = lng.TranslateString("Waiting for old process to finish - Previous Operation Is Still Running", 1200);
                return(temp);
            }
            PipeRunThreadActive = true;
            ClientMessage = command;
            try
            {
                PipeRunThread = new Thread(ClientThreadRoutineRunSingleCommand);
                PipeRunThread.Start();
            }
            catch (Exception exc)
            {
                Log.Error("RunSingleCommand: SendClient() exception=" + exc.Message);
            }
            //wait for finish
            while (PipeRunThread.IsAlive)
            {
                Log.Debug("RunSingleCommand: Waiting for client thread to finish");
                Thread.Sleep(100);
            }
            PipeRunThreadActive = false;
            Log.Debug("RunSingleCommand: ReceivedMessage=" + ReceivedMessage);
            return ReceivedMessage;
        }

        private void ClientThreadRoutineRunSingleCommand()
        {
            ReceivedMessage = "Error in ClientThreadRoutineRunSingleCommand";
            try
            {
                Log.Debug("ClientThreadRoutineRunSingleCommand: Connecting to server " + hostName);
                Log.Debug("pipe=" + pipeName);
                Log.Debug("hostname=" + hostName);

                pipeClient = new NamedPipeClientStream(hostName, pipeName,
                        PipeDirection.InOut, PipeOptions.None,
                        TokenImpersonationLevel.Impersonation);
               
                pipeClient.Connect();
                
                StreamString ss = new StreamString(pipeClient);
                //Log.Debug("1 ClientMessage=" + ClientMessage);
                //send clientmessage to server
                ss.WriteString(ClientMessage);
                //pipeClient.ReadTimeout = 5000; //timeout not supported for async streams

                PipeRunThreadTimeOutCounter = new Thread(ClientTimeOutError);
                PipeRunThreadTimeOutCounter.Start();
                ReceivedMessage = ss.ReadString();
                PipeRunThreadTimeOutCounter.Abort();


                Log.Debug("ClientThreadRoutineRunSingleCommand: ***** SERVERMESSAGE=" + ReceivedMessage);


                Log.Debug("ClientThreadRoutineRunSingleCommand: closing client pipe - command executed");
            }
            catch (Exception exc)
            {
                Log.Debug("ClientThreadRoutineRunSingleCommand: ClientThread() exception=" + exc.Message);
            }

            if (pipeClient != null)
            {
                pipeClient.Close();
                //pipeClient = null;
            }

            Log.Debug("ClientThreadRoutineRunSingleCommand: Pipe Client Thread Completed");
            //OldConnect = false;
            return;
        }

        #endregion clientpipes


        public void labelmessage(string text, PipeCommands type)
        {
            if (newlabelmessage != null)
            {
                Log.Debug("Creating new event message: " + text + " for type=" + type.ToString());
                newlabelmessage(text, type);
            }
#if (MP11 || MP12 || MP16 || MP2)
            if (myTvWishes != null)
            {
                myTvWishes.StatusLabel(text);
            }
#endif

        }


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

#if (!MPTV2) && (!TV30)
    public class LanguageTranslation
    {
        public string TranslateString(string text, int number)
        {
            return text;
        }
    }
#endif
}
