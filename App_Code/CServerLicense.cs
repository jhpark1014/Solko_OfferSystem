using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LicenseCheck
{
    public class CServerLicense
    {
        // 정  상 : https://support.solko.co.kr:2201
        // 디버그 : http://192.168.1.137:2201

        // operation address
        static string _http = "https";
        static string _addr = "support.solko.co.kr";
        static int _port = 2201;
        //static string _mail_senderID = "monitor@bitlogic.kr"; (2019.07.19) server change
        static string _mail_senderID = "tech@solidkorea.co.kr";
        static string _mail_receiveID = "ae@solidkorea.co.kr";

        //// dev address
        //static string DEBUG_HTTP = "http";
        //static string DEBUG_ADDR = "192.168.1.137";
        //static int DEBUG_PORT = 2201;
        //static string DEBUG_MAIL_SENDERID = "jshong@solidkorea.co.kr";
        //static string DEBUG_MAIL_RECEIVEID = "jshong@solidkorea.co.kr";

        // test address
        static string DEBUG_HTTP = "https";
        static string DEBUG_ADDR = "support.solko.co.kr";
        static int DEBUG_PORT = 2204;
        //static string DEBUG_MAIL_SENDERID = "monitor@bitlogic.kr"; (2019.07.19) server change
        static string DEBUG_MAIL_SENDERID = "tech@solidkorea.co.kr";
        static string DEBUG_MAIL_RECEIVEID = "ae@solidkorea.co.kr";

        static public void SetServerAddress(string newAddr)
        {
            _addr = newAddr;
        }
        static public string GetServerURL(bool debugMode = false)
        {
            if (debugMode)
                return string.Concat(DEBUG_HTTP, "://", DEBUG_ADDR, ":", DEBUG_PORT.ToString());
            else
                return string.Concat(_http, "://", _addr, ":", _port.ToString());
        }
        static public string GetServerMailSender(bool debugMode = false)
        {
            if (debugMode)
                return DEBUG_MAIL_SENDERID;
            else
                return _mail_senderID;
        }
        static public string GetServerMailReceiver(bool debugMode = false)
        {
            if (debugMode)
                return DEBUG_MAIL_RECEIVEID;
            else
                return _mail_receiveID;
        }
    }
}
