using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Aquila_Software
{
    internal class SMSHelper
    {
        static string server = "192.168.16.135";
        static string port = "9090";

        /// <summary>
        /// Sends the SMS, with the default text.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <param name="equity">The equity.</param>
        /// <returns></returns>
        public static bool sendDefaultSMS(string recipient, string equity)
        {
            string url = "http://" + server + ":" + port + "/sendsms?phone=" + recipient + "&text="+ equity+ " is waiting for an decision&password=";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();

            return true;
        }

        /// <summary>
        /// Sends the SMS.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static bool sendSMS(string recipient, string message)
        {
            string url = "http://" + server + ":" + port + "/sendsms?phone=" + recipient + "&text=" + message + "&password=";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            return true;
        }
    }
}
