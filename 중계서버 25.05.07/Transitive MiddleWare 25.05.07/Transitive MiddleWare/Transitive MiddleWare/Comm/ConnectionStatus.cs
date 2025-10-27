using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.TransitiveMiddleware
{
    public class ConnectionStatus
    {
		// 연결 종류
		private String connectionKind;
		// 연결 상태
		private String connectionStatus;
		// 마지막 연결 시각
		private String lastConnectionTime;

		// 생성자
		public ConnectionStatus(String ConnectionKind, String ConnectionStatus, String LastConnectionTime)
		{
			this.connectionKind = ConnectionKind;
			this.connectionStatus = ConnectionStatus;
			this.lastConnectionTime = LastConnectionTime;
		}

		public string getConnectionKind()
		{
			return connectionKind;
		}
		public void setConnectionKind(string connectionKind)
		{
			this.connectionKind = connectionKind;
		}

		public string getConnectionStatus()
		{
			return connectionStatus;
		}
		public void setConnectionStatus(string connectionStatus)
		{
			this.connectionStatus = connectionStatus;
		}

		public string getLastConnectionTime()
		{
			return lastConnectionTime;
		}
		public void setLastConnectionTime(string lastConnectionTime)
		{
			this.lastConnectionTime = lastConnectionTime;
		}

		// toString() 시 JSON 반환
		public string toString()
		{
			JObject jsarr = new JObject();
			JProperty property = new JProperty("connectionKind", this.connectionKind);
			JProperty property2 = new JProperty("connectionStatus", this.connectionStatus);
			JProperty property3 = new JProperty("lastConnectionTime", this.lastConnectionTime);

			jsarr.Add(property);
			jsarr.Add(property2);
			jsarr.Add(property3);

			return jsarr.ToString();
		}
	}
}
