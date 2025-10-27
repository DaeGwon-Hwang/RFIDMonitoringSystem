using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BPNS.TransitiveMiddleware
{
    public class CtrlClient
    {

        //public static Dictionary<string, object> connect(Dictionary<string, object> request)
        public static JObject connect(Dictionary<string, object> request)
        {
			Program.AppLog.Write("[ctrl - request : " + ConvertDictionaryToString<string, object>(request));

			string sResultValue = "0";
			string sResultMsg = "";

			DateTime dteStart = DateTime.Now;
			Program.AppLog.Write("start time : " + dteStart.ToString("yyyy-MM-dd HH:mm:ss.fff"));

			string sData = GetSendStr(request);
			string sReceiveVal = string.Empty;

			//JObject response = TCPClient.request(request, Properties.Settings.Default.TO_CTRL_HOST, Properties.Settings.Default.TO_CTRL_PORT, false);
			//JObject response = TCPClient.request(sData, Properties.Settings.Default.TO_CTRL_HOST, Properties.Settings.Default.TO_CTRL_PORT, false);

			

			string strServerIP = "192.168.200.231"; //Constants.TO_CTRL_HOST;
			int nServerPort = 20080;    //Constants.TO_CTRL_PORT;
			TcpClient client = new TcpClient(strServerIP, nServerPort);
			NetworkStream ns = client.GetStream();
			StreamWriter writer = new StreamWriter(ns);
			writer.WriteLine(sData);
			writer.Flush();

			StreamReader reader = new StreamReader(ns);
			sReceiveVal = reader.ReadLine();

			writer.Close();
			reader.Close();
			client.Close();

			Program.AppLog.Write("SendStr : " + sData);
			Program.AppLog.Write("receive : " + sReceiveVal.ToString());

			DateTime dteEnd = DateTime.Now;
			Program.AppLog.Write("end time : " + dteEnd.ToString("yyyy-MM-dd HH:mm:ss.fff"));
			

			TimeSpan TS = dteEnd - dteStart;
			double diffSec = TS.TotalSeconds;
			Program.AppLog.Write("execute time : " + diffSec + "s");

			Dictionary<string, object> LogData = new Dictionary<string, object>();
			LogData.Add("workdiv", request["WORKDIV"]);
			LogData.Add("starttime", dteStart.ToString("yyyyMMddHHmmssfff"));
			LogData.Add("endtime", dteEnd.ToString("yyyyMMddHHmmssfff"));
			LogData.Add("ResultValue", sResultValue);
			LogData.Add("ResultMsg", sResultMsg);
			//LogData.Add("result", );

			InsertLog(LogData);

			JObject response = new JObject();
			response.Add("RESULT", sReceiveVal);

			return response;


        }

        public static string GetSendStr(Dictionary<string, object> request)
        {
            string sWorkDiv = request["WORKDIV"] == null ? "" : request["WORKDIV"].ToString();
            StringBuilder sb = new StringBuilder();
		
			if (sWorkDiv == "GATE_STT")
			{
				sb.Append(request["WORKDIV"] == null ? "" : request["WORKDIV"].ToString());
				sb.Append("|");
				sb.Append(request["GATE_ID"] == null ? "" : request["GATE_ID"].ToString());
				sb.Append("|");
				sb.Append(request["READER_ID"] == null ? "" : request["READER_ID"].ToString());
				sb.Append("|");
				sb.Append(request["STATUS"] == null ? "" : request["STATUS"].ToString());
			}
			else if (sWorkDiv == "IL_DOC")
			{
				sb.Append(request["WORKDIV"] == null ? "" : request["WORKDIV"].ToString());
				sb.Append("|");
				sb.Append(request["GATE_ID"] == null ? "" : request["GATE_ID"].ToString());
				sb.Append("|");
				sb.Append(request["MGMT_ID"] == null ? "" : request["MGMT_ID"].ToString());
				sb.Append("|");
				sb.Append(request["BIND_NO"] == null ? "" : request["BIND_NO"].ToString());
				sb.Append("|");
				sb.Append(request["DOC_TTL"] == null ? "" : request["DOC_TTL"].ToString());
				sb.Append("|");
				sb.Append(request["MED_TYPE"] == null ? "" : request["MED_TYPE"].ToString());
				sb.Append("|");
				sb.Append(request["LOC"] == null ? "" : request["LOC"].ToString());
				sb.Append("|");
				sb.Append(request["IN_OUT_FL"] == null ? "" : request["IN_OUT_FL"].ToString());
			}
			else if (sWorkDiv == "RL_DOC")
			{
				sb.Append(request["WORKDIV"] == null ? "" : request["WORKDIV"].ToString());
				sb.Append("|");
				sb.Append(request["PAMS_MGMT_NO"] == null ? "" : request["PAMS_MGMT_NO"].ToString());
				sb.Append("|");
				sb.Append(request["BIND_NO"] == null ? "" : request["BIND_NO"].ToString());
			}
			else if (sWorkDiv == "CONNECTION_STT")
			{   
				sb.Append(request["WORKDIV"] == null ? "" : request["WORKDIV"].ToString());
				sb.Append("|");
				sb.Append("PAMS");
				sb.Append(":");
				sb.Append(request["PAMS_STT"] == null ? "" : request["PAMS_STT"].ToString());
				sb.Append("|");
				sb.Append("RFID_MW");
				sb.Append(":");
				sb.Append(request["RFIDMW_STT"] == null ? "" : request["RFIDMW_STT"].ToString());
			}

			Program.AppLog.Write("getSendStr : " + sb.ToString());

			return sb.ToString();
		}

		public static void InsertLog(Dictionary<string, object> data)
		{
			/*
				LogData.Add("workdiv", request["WORKDIV"]);
				LogData.Add("starttime", dteStart.ToString("yyyyMMddhhmmssfff"));
				LogData.Add("endtime", dteEnd.ToString("yyyyMMddhhmmssfff"));
				LogData.Add("sResultValue", sResultValue);
				LogData.Add("sResultMsg", sResultMsg);
			 */

			if (Properties.Settings.Default.IS_LOGGING_CONNT_HIS == false) return;

			Hashtable htParamsHashtable;

			string sWorkdiv = data["workdiv"] == null ? "" : data["workdiv"].ToString();
			string sStarttime = data["starttime"] == null ? "" : data["starttime"].ToString();
			string sEndtime = data["endtime"] == null ? "" : data["endtime"].ToString();
			string sResultValue = data["ResultValue"] == null ? "" : data["ResultValue"].ToString();
			string sResultMsg = data["ResultMsg"] == null ? "" : data["ResultMsg"].ToString();


			htParamsHashtable = new Hashtable();
			htParamsHashtable.Clear();
			htParamsHashtable.Add("IN_WORKDIV", sWorkdiv);
			htParamsHashtable.Add("IN_STARTTIME", sStarttime);
			htParamsHashtable.Add("IN_ENDTIME", sEndtime);
			htParamsHashtable.Add("IN_PROC_RST", sResultValue);
			htParamsHashtable.Add("IN_ERR_CD", sResultMsg);
			htParamsHashtable.Add("IN_CONN_TYPE", "2CTRL");

			string strAppCode = string.Empty;
			string strAppMsg = string.Empty;

			((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_CONNT_HIS", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
			strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
			strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

			if (strAppCode.Equals("0"))
			{

			}
			else
			{
				Program.AppLog.Write("PAMSDATA.TRFID_CONNT_HIS 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + sWorkdiv + sStarttime + sEndtime + sResultValue + sResultMsg);
				((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.TRFID_CONNT_HIS 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
			}
		}

		public static string ConvertDictionaryToString<DKey, DValue>(Dictionary<DKey, DValue> dict)
		{
			string format = "{0}='{1}',";

			StringBuilder itemString = new StringBuilder();
			foreach (KeyValuePair<DKey, DValue> kv in dict)
			{
				itemString.AppendFormat(format, kv.Key, kv.Value);
			}
			itemString.Remove(itemString.Length - 1, 1);

			return itemString.ToString();
		}

	}
}
