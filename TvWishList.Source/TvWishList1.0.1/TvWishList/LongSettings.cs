using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TvControl;
using TvDatabase;
using Gentle.Framework;

#if (MP12 || MP11)
using Log = TvLibrary.Log.huha.Log;
#else
using TvLibrary.Log.huha;
#endif

namespace TvWishList
{
    class LongSettings
    {
        TvBusinessLayer layer = new TvBusinessLayer();        
        int STRINGLIMIT = 4000; //is 4096  string limit for settings - split if larger
        bool _debug = false;

        public LongSettings(bool Debug)
        {
            _debug = Debug;
        }

        public string loadlongsettings(string name)
        {

            //splits long setting strings in multiple parts

            //LogDebug("Longsetting TvWishList_ListView", (int)LogSetting.DEBUG);
            string stringdata = layer.GetSetting(name, "").Value;
            //LogDebug("listviewdata TvWishList_ListView=" + listviewdata, (int)LogSetting.DEBUG);
            int count = 1;
            string partial = layer.GetSetting(name + "_" + count.ToString("D3"), "").Value;
            //LogDebug("partial " + "TvWishList_ListView_" + count.ToString("D3") + "=" + partial, (int)LogSetting.DEBUG);
            while (partial != "")
            {
                stringdata += partial;
                count++;
                partial = layer.GetSetting(name + "_" + count.ToString("D3"), "").Value;
                //LogDebug("partial " + name + "_" + count.ToString("D3") + "=" + partial, (int)LogSetting.DEBUG);
            }


            //LogDebug("Merged" + "TvWishList_ListView Length =" + listviewdata.Length.ToString(), (int)LogSetting.DEBUG);
            //LogDebug("Merged" + "TvWishList_ListView =" + listviewdata, (int)LogSetting.DEBUG);

            return stringdata;
        }


        public void save_longsetting(string mystring, string mysetting)
        {
            Setting setting;

            // split string if too large  !!! Limit of 4096 characters in tv server

            try
            {
                if (mystring.Length > STRINGLIMIT)
                {

                    string partial_string = mystring.Substring(0, STRINGLIMIT);
                    setting = layer.GetSetting(mysetting, "");
                    setting.Value = partial_string;
                    //LogDebug("partial string  " + mysetting + "  =" + partial_string, (int)LogSetting.DEBUG);
                    setting.Persist();
                    int ctr = 1;
                    while (ctr * STRINGLIMIT <= mystring.Length)
                    {
                        if ((ctr + 1) * STRINGLIMIT < mystring.Length)
                        {
                            partial_string = mystring.Substring(ctr * STRINGLIMIT, STRINGLIMIT);
                            setting = layer.GetSetting(mysetting + "_" + ctr.ToString("D3"), "");
                            setting.Value = partial_string;
                            //LogDebug("partial string  " + mysetting + "_" + ctr.ToString("D3") + "  =" + partial_string, (int)LogSetting.DEBUG);
                            setting.Persist();

                        }
                        else
                        {
                            partial_string = mystring.Substring(ctr * STRINGLIMIT, mystring.Length - ctr * STRINGLIMIT);
                            setting = layer.GetSetting(mysetting + "_" + ctr.ToString("D3"), "");
                            setting.Value = partial_string;
                            //LogDebug("partial listviewstring  " + mysetting + "_" + ctr.ToString("D3") + "  =" + partial_string, (int)LogSetting.DEBUG);
                            setting.Persist();
                            ctr++;
                            setting = layer.GetSetting(mysetting + "_" + ctr.ToString("D3"), "");
                            setting.Value = "";
                            setting.Persist();

                        }
                        ctr++;

                        if (ctr > 999)
                        {
                            LogDebug("!!!!!!!!!!!!!!!!!!!! Fatal Error: Too many data entries - skipping data", (int)LogSetting.ERROR);
                            break;
                        }
                    }

                }
                else //do not split string - small enough
                {
                    setting = layer.GetSetting(mysetting, "");
                    setting.Value = mystring;
                    setting.Persist();
                    int ctr = 1;
                    //LogDebug("string  " + mysetting + "=" + mystring, (int)LogSetting.DEBUG);
                    setting = layer.GetSetting(mysetting + "_" + ctr.ToString("D3"), "");
                    setting.Value = "";
                    setting.Persist();
                }
            }
            catch (Exception exc)
            {
                LogDebug("Adding long setting failed with message \n" + exc.Message, (int)LogSetting.ERROR);
            }

        }


        //-------------------------------------------------------------------------------------------------------------        
        //log handling for debug, error and addmessage (return mals)
        //-------------------------------------------------------------------------------------------------------------                
        public void LogDebug(string text, int field)
        {
            //trigger message event

            if (field == (int)LogSetting.INFO)
            {
                Log.Debug("[TvWishList MessageClass]: " + text);
                //if (newmessage != null)
                //    newmessage(text, field);

            }
            else if ((field == (int)LogSetting.DEBUG) && (_debug == true))
            {
                if (_debug == true)
                {
                    Log.Debug("[TvWishList MessageClass]: " + text);
                    //if (newmessage != null)
                    //    newmessage(text, field);
                }

            }
            else if (field == (int)LogSetting.ERROR)
            {
                Log.Error("[TvWishList MessageClass]: " + text);
                Log.Debug("[TvWishList MessageClass]: " + text);
            }
            else if (field == (int)LogSetting.ERRORONLY)
            {
                Log.Error("[TvWishList MessageClass]: " + text);
                //if (newmessage != null)
                //    newmessage(text, field);

            }
            else
            {
                //Log.Error("TvWishList Error MailClass: Unknown message Code " + field.ToString(), (int)LogSetting.ERROR);
            }
        }

    }
}
