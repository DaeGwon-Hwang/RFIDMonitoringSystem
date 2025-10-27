using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BPNS.TransitiveMiddleware
{
    class TCPRequest
    {
		public string data;
		public byte[] file = null;

		public TCPRequest(Dictionary<string, object> aData, bool bAdd = false)
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
			if(bAdd) data += "<CMD:EOF>";
			//data += "<CMD:EOF>";
		}

		public TCPRequest(string aData, bool bAdd = false)
		{
			data = aData;

			if (bAdd) data += "<CMD:EOF>";
		}

		public byte[] getBytes()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(data);

			return Encoding.UTF8.GetBytes(sb.ToString());
		}

	}
}
