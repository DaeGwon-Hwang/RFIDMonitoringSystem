using System;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using RFID.Middleware.Shared;

namespace RFID.Middleware.Server
{
    public static class RequestHandler
    {
        public static string ProcessRequest(string xml)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Models.WorkRoot));
                using (var sr = new StringReader(xml))
                {
                    var req = (Models.WorkRoot)serializer.Deserialize(sr);
                    Console.WriteLine("[HANDLER] Processing WorkCode=" + req.WorkInfo?.WorkCode);
                    var resp = MakeSampleResponse(req);
                    var xs = new XmlSerializer(typeof(Models.WorkRoot));
                    using (var sw = new StringWriter())
                    {
                        xs.Serialize(sw, resp);
                        return sw.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[HANDLER] Error: " + ex.Message);
                return MakeErrorResponse(ex.Message);
            }
        }

        static Models.WorkRoot MakeSampleResponse(Models.WorkRoot req)
        {
            var resp = new Models.WorkRoot
            {
                WorkInfo = req.WorkInfo,
                SendInfo = new Models.SendInfo { Ip = req.ReceiveInfo?.Ip ?? "127.0.0.1", Port = req.ReceiveInfo?.Port ?? 20014 },
                ReceiveInfo = new Models.ReceiveInfo { Ip = req.SendInfo?.Ip ?? "127.0.0.1", Port = req.SendInfo?.Port ?? 20013 },
                ParamList = req.ParamList,
                ReturnList = new Models.ReturnList { Id = req.ParamList?.Id ?? 1 },
            };

            var row = new Models.ReturnRow();
            string code = req.WorkInfo?.WorkCode ?? string.Empty;
            switch (code)
            {
                case "WC001":
                    row.Columns.Add(new Models.ReturnCol { Id = "SERVER_DATE_TIME", Value = DateTime.Now.ToString("yyyyMMddHHmmss") });
                    break;
                case "WC007":
                    row.Columns.Add(new Models.ReturnCol { Id = "CNTCHK_ID", Value = "2012000001" });
                    row.Columns.Add(new Models.ReturnCol { Id = "PLAN_YMD", Value = "20121017" });
                    row.Columns.Add(new Models.ReturnCol { Id = "USER_NM", Value = "김기록" });
                    row.Columns.Add(new Models.ReturnCol { Id = "CHECK_STATE_CD", Value = "0" });
                    break;
                default:
                    if (req.ParamList?.Params != null)
                    {
                        foreach (var p in req.ParamList.Params)
                        {
                            row.Columns.Add(new Models.ReturnCol { Id = p.Id, Value = p.Value });
                        }
                    }
                    else
                    {
                        row.Columns.Add(new Models.ReturnCol { Id = "SELECT_RESULT", Value = "No data" });
                    }
                    break;
            }
            resp.ReturnList.Rows.Add(row);
            return resp;
        }

        static string MakeErrorResponse(string message)
        {
            var template = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><work_root><work_info><work_key>ERROR</work_key><work_div>WD00</work_div><work_code>ERROR</work_code></work_info><send_info ip=\"127.0.0.1\" port=\"20013\"/><receive_info ip=\"127.0.0.1\" port=\"20014\"/><param_list id=\"1\"></param_list><return_list id=\"1\"><return_row><return_col id=\"SELECT_RESULT\">{System.Security.SecurityElement.Escape(message)}</return_col></return_row></return_list></work_root>";
            return template;
        }
    }
}
