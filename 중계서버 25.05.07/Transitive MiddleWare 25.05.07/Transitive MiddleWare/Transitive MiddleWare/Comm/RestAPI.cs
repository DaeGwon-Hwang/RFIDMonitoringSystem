using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


using Newtonsoft.Json.Linq;

namespace BPNS.TransitiveMiddleware
{
    public class RestAPI
    {
        private TimeSpan defaultTimeSpan = TimeSpan.FromSeconds(600);

        public JObject CallAPI(string sMsg, string sWORKDIV, TimeSpan? timeout = null)
        {
            string sResponseCode = string.Empty;
            string sResponseData = string.Empty;
            JObject JoRtn = null;
            try
            {
                Program.AppLog.Write("[pams - request : " + sWORKDIV + ":" + sMsg);

                string sResultValue = "0";
                string sResultMsg = "";

                DateTime dteStart = DateTime.Now;
                Program.AppLog.Write("start time : " + sWORKDIV + ":" + dteStart.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                timeout = timeout ?? this.defaultTimeSpan;
                
                string sSUCCESS = "SUCCESS";

                WebRequest request = WebRequest.Create(sMsg);
                request.ContentType = "application/json;charset=utf-8";
                request.Timeout = (int)(timeout.Value.TotalMilliseconds);

                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();

                StreamReader reader = new StreamReader(dataStream, Encoding.UTF8);

                string sResponseFromServer = reader.ReadToEnd();

                reader.Close();
                dataStream.Close();
                response.Close();

                JObject JResData = JObject.Parse(sResponseFromServer);
                sResponseCode = JResData["responseCode"].ToString();
                sResponseData = JResData["responseData"].ToString();

                if (sResponseCode == sSUCCESS)
                {
                    JoRtn = DataConvert(JObject.Parse(sResponseData));
                }
                else
                {
                    JoRtn = JObject.Parse(sResponseData);
                }

                Program.AppLog.Write("send : " + sMsg);
                Program.AppLog.Write("receive : " + JoRtn.ToString());

                DateTime dteEnd = DateTime.Now;
                Program.AppLog.Write("end time : " + sWORKDIV + ":" + dteEnd.ToString("yyyy-MM-dd HH:mm:ss.fff"));


                TimeSpan TS = dteEnd - dteStart;
                double diffSec = TS.TotalSeconds;
                Program.AppLog.Write("execute time : " + sWORKDIV + ":" + diffSec + "s");

                Dictionary<string, object> LogData = new Dictionary<string, object>();
                LogData.Add("workdiv", sWORKDIV);
                LogData.Add("starttime", dteStart.ToString("yyyyMMddHHmmssfff"));
                LogData.Add("endtime", dteEnd.ToString("yyyyMMddHHmmssfff"));
                LogData.Add("ResultValue", sResultValue);
                LogData.Add("ResultMsg", sResultMsg);
                //LogData.Add("result", );

                //PamsClient.InsertLog(LogData);
            }
            catch (Exception ex)
            {
                Program.AppLog.Write("Exception send : " + sMsg);

                if (JoRtn == null)
                {
                    Program.AppLog.Write("Exception receive : " + "null"+ "sResponseData :" + sResponseData);
                }
                else
                {
                    Program.AppLog.Write("Exception receive : " + JoRtn.ToString());
                }
                

                DateTime dteEnd = DateTime.Now;
                Program.AppLog.Write("Exception end time : " + sWORKDIV + ":" + dteEnd.ToString("yyyy-MM-dd HH:mm:ss.fff"));


                throw ex;
            }
            finally
            { 
            
            }

            return JoRtn;
        }

        public JObject DataConvert(JObject old_request)
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

            if (old_request.ContainsKey("rows"))
            {
                old_request.Remove("rows");
            }

            //if (old_request["LIST"] != null)
            //{
            //    rows = (JArray)old_request["LIST"];

            //    for (int i = 0; i < rows.Count; i++)
            //    {
            //        JObject row = (JObject)rows[i];

            //        list.Add(row);
            //    }
            //    //request.Add("LIST", list);
            //}

            //if (old_request.ContainsKey("LIST"))
            //{
            //    old_request.Remove("LIST");
            //}


            request = JObject.FromObject(old_request).ToObject<Dictionary<string, object>>();

            if (list.Count > 0)
            {
                request.Add("rows", list);
            }

            if (request.ContainsKey("RESULT")) request["RESULT"] = "0";
            else request.Add("RESULT", "0");


            JObject json = new JObject();

            if (request != null)
            {
                foreach (string strKey in request.Keys)
                {
                    JProperty property = new JProperty(strKey, request[strKey]);

                    json.Add(property);
                }
            }

            return json;
        }
    }
}
