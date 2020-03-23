using System;
using System.IO;
using System.Net;
using System.Text;
using M800SMS.Configurations;

namespace M800SMS
{
  public class Sms
    {
        private readonly string _domain;
        private readonly string _username;
        private readonly string _password;


        public Sms(M800 setting)
        {
            _domain = setting.Domain;
            _username = setting.Username;
            _password = setting.Password;
        }
        
        public string SendSms(string countryCode, string mobileNo, string rawMsg)
        {


            bool rawMsgTF = IsUnicode(rawMsg);
            int rawMsgInt = rawMsg.Length;

            //0 = url excode, 8 = unicode
            string dcs = "0";
            //英文 160字以上要設為1, 中文70字以上設為1
            string LongMsg = "0";

            string SMSMsg = rawMsg;

            if (rawMsgTF == true)
            {
                SMSMsg = ChangeToUnicode(rawMsg);
                SMSMsg = SMSMsg.Replace("\\u", "");
                dcs = "8";
                if (rawMsgInt > 70)
                {
                    LongMsg = "1";
                }
            }
            else 
            {
                SMSMsg = SMSMsg.Replace("&", "%26");
                SMSMsg = SMSMsg.Replace(" ", "%20");
                dcs = "0";
                if (rawMsgInt > 160)
                {
                    LongMsg = "1";
                }
            }

            string Final ="httpRestStub/SendMsg?Username=" + _username + "&Password=" + _password + "&SrcTON=1&SrcNPI=1&SrcID=5001000346&DestTON=1&DestNPI=1&Mobile=" + countryCode + mobileNo + "&DCS=" + dcs + "&LongMsg=" + LongMsg + "&MsgText=" + SMSMsg;
            string Result = GetMethodRequest(_domain, Final.ToString());
            return Result;
        }

        public static bool IsUnicode(string input)
        {
            var asciiBytesCount = Encoding.ASCII.GetByteCount(input);
            var unicodBytesCount = Encoding.UTF8.GetByteCount(input);
            return asciiBytesCount != unicodBytesCount;
        }

        public static string ChangeToUnicode(string srcText)
        {
            string dst = "";
            char[] src = srcText.ToCharArray();
            for (int i = 0; i < src.Length; i++)
            {
                byte[] bytes = Encoding.Unicode.GetBytes(src[i].ToString());
                string str = @"\u" + bytes[1].ToString("X2") + bytes[0].ToString("X2");
                dst += str;
            }
            return dst;
        }

        private static dynamic GetMethodRequest(string domain, string postData)
        {
            HttpWebRequest request = null;
            Uri uri = new Uri(domain);
            byte[] bytes = Encoding.UTF8.GetBytes(postData);
            string result = string.Empty;

            request = (HttpWebRequest)WebRequest.Create(uri + postData);
            request.Method = "GET";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            result = readStream.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException we)
            {
                using (Stream responseStream = we.Response.GetResponseStream())
                {
                    using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        try
                        {
                            result = readStream.ReadToEnd();
                        }
                        catch (ArgumentException ae)
                        {
                            
                        }
                    }
                }
            }

            return result;
        }

    }
}
