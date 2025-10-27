using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BPNS.TransitiveMiddleware
{
    public class PamsClient
    {
		public static JObject connect(string className, JObject old_request)
		{
			Dictionary<string, object> request = new Dictionary<string, object>();

			JArray rows = null;
			var list = new List<JObject>();

			if (old_request["rows"] != null)
			{
				rows = (JArray)old_request["rows"];

				for (int i = 0; i < rows.Count; i++)
				{
					JObject row = (JObject)rows[i];

					list.Add(row);
				}
				//request.Add("LIST", list);
			}

			old_request.Remove("rows");

			request = JObject.FromObject(old_request).ToObject<Dictionary<string, object>>();

			if (list.Count > 0)
			{
				request.Add("LIST", list);
			}

			return JObject.FromObject(connect(className, request));
		}

		public static Dictionary<string, object> connect(string className, Dictionary<string, object> request)
		{
			Program.AppLog.Write("[pams - request : " + ConvertDictionaryToString<string, object>(request));

			Dictionary<string, object> response = null;

			string sResultValue = "0";
			string sResultMsg = "";

			DateTime dteStart = DateTime.Now;
			Program.AppLog.Write("start time : " + dteStart.ToString("yyyy-MM-dd hh:mm:ss.fff"));

			JObject Jresponse = TCPClient.request(request, Properties.Settings.Default.TO_PAMS_HOST, Properties.Settings.Default.TO_PAMS_PORT, false);
			response = JObjectToDic(Jresponse);

			Program.AppLog.Write("send : " + ConvertDictionaryToString<string, object>(request));
			Program.AppLog.Write("receive : " + Jresponse.ToString());

			//확인 필요 소스상 필요없을 것 같음
			//if (((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus != null && ((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus.ContainsKey(((MainForm)(Application.OpenForms["MainForm"])).PAMS))
			//{
			//	((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus.Remove(((MainForm)(Application.OpenForms["MainForm"])).PAMS);
			//}

			DateTime dteEnd = DateTime.Now;
			Program.AppLog.Write("end time : " + dteEnd.ToString("yyyy-MM-dd hh:mm:ss.fff"));


			TimeSpan TS = dteEnd - dteStart;
			double diffSec = TS.TotalSeconds;
			Program.AppLog.Write("execute time : " + diffSec + "s");

			Dictionary<string, object> LogData = new Dictionary<string, object>();
			LogData.Add("workdiv", request["WORKDIV"]);
			LogData.Add("starttime", dteStart.ToString("yyyyMMddhhmmssfff"));
			LogData.Add("endtime", dteEnd.ToString("yyyyMMddhhmmssfff"));
			LogData.Add("ResultValue", sResultValue);
			LogData.Add("ResultMsg", sResultMsg);
			//LogData.Add("result", );

			//InsertLog(LogData);

			return response;


		}

		public static Dictionary<string, object> JObjectToDic(JObject old_request)
		{
			Dictionary<string, object> request = new Dictionary<string, object>();

			JArray rows = null;
			var list = new List<JObject>();

			if (old_request["rows"] != null)
			{
				rows = (JArray)old_request["rows"];

				for (int i = 0; i < rows.Count; i++)
				{
					JObject row = (JObject)rows[i];

					list.Add(row);
				}
				//request.Add("LIST", list);
			}

			old_request.Remove("rows");

			request = JObject.FromObject(old_request).ToObject<Dictionary<string, object>>();

			if (list.Count > 0)
			{
				request.Add("rows", list);
			}

			return request;
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
			htParamsHashtable.Add("IN_CONN_TYPE", "2PAMS");

			string strAppCode = string.Empty;
			string strAppMsg = string.Empty;

            try
            {
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
            catch (NullReferenceException ex)
            {
				Program.AppLog.Write("PAMSDATA.TRFID_CONNT_HIS 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + sWorkdiv + sStarttime + sEndtime + sResultValue + sResultMsg);
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
