using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RFID_CONTROLLER
{
    class TCPRequest
    {
        public string data;
        public byte[] file = null;

        public TCPRequest(Dictionary<string, object> aData)
        {
            JObject json = new JObject();

            if (aData != null)
            {
                foreach (string strKey in aData.Keys)
                {
                    JProperty property = new JProperty(strKey, aData[strKey]);

                    json.Add(property);
                }
            }

            data = json.ToString();
            data += "<CMD:EOF>";

            Console.WriteLine("request > " + data);
        }

        public byte[] getBytes()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(data);

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
