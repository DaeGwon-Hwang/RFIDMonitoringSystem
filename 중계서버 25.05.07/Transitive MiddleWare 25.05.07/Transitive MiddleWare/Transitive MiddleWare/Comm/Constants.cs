using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.TransitiveMiddleware
{
	public class Constants
	{
		// KIC_RFID_MIDDLEWARE INIT File 정보
		public static string INIT_XML_FILE = "Connection.xml";

		public static string DAEMON_TITLE = "Connection Daemon. power the by KIC Systems.";
		public static string VERSION = "v.180821.1310";

		//	<!--  Console 정보 -->
		public static int CONSOLE_PORT = 11012;

		//	<!--  PAMS 서비스 정보 -->
		public static int FROM_PAMS_PORT = 8011;
		public static string TO_PAMS_HOST = "172.30.34.23";
		public static int TO_PAMS_PORT = 9011;

		//	<!-- 통합관제 정보 -->
		public static string TO_CTRL_HOST = "192.168.200.231";
		public static int TO_CTRL_PORT = 20080;
		public static int CTRL_TIMEOUT = 3000;

		//	<!-- 미들웨어 정보 -->
		public static int FROM_MW_PORT = 11812;

		public static string TO_MW_HOST = "192.168.200.22";
		public static int TO_MW_PORT = 11912;
		public static int MW_TIMEOUT = 3000;

		//	<!-- JSON 정보 -->
		public static int FROM_JSON_PORT = 12012;

		//	<!-- RFID 정보 -->
		public static int FROM_RFID_PORT = 12015;

		//	<!-- 발행기 정보 -->
		public static int PRT_PORT = 9100;
		public static int PRT_TIMEOUT = 9000;

		//	<!-- TRFID_CONNT_HIS 로그 남기기 선택 -->
		public static bool IS_LOGGING_CONNT_HIS = false;

		public static string sSUCCESS = "0";

		public static string sFAIL = "1";

		public static ServiceState SERVICE_STATE = ServiceState.STOPTED;
	}

	public enum ServiceState
	{
		RUNNING, STOPTED, PAUSE
	}



	public class ResponseResult
	{
		//SUCCESS("0"), FAIL("1");

		private string value;
		private string message;

        public string Value 
		{
			get => value; set => this.value = value; 
		}
        public string Message 
		{ 
			get => message; set => message = value; 
		}

		public ResponseResult()
		{
			value = "0";
			message = "";
		}
	}

}
