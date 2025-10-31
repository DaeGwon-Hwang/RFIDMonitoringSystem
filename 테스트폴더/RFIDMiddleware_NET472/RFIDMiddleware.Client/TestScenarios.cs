using System;
using System.IO;
using System.Xml.Serialization;
using RFID.Middleware.Server.Models;

namespace RFID.Middleware.Client
{
    public static class TestScenarios
    {
        public static string BuildRequestXml(string workCode)
        {
            var root = new WorkRoot
            {
                WorkInfo = new WorkInfo
                {
                    WorkKey = DateTime.Now.ToString("yyyyMMddHHmmss") + "_1",
                    WorkDiv = "WD00",
                    WorkCode = workCode
                },
                SendInfo = new SendInfo { Ip = "127.0.0.1", Port = 20014 },
                ReceiveInfo = new ReceiveInfo { Ip = "127.0.0.1", Port = 20013 },
                ParamList = new ParamList { Id = 1 }
            };

            switch (workCode)
            {
                case "WC002":
                    root.ParamList.Params.Add(new Param { Id = "DEPT_CD", Value = "1311000" });
                    root.ParamList.Params.Add(new Param { Id = "LOGIN_USER_ID", Value = "user01" });
                    break;
                case "WC007":
                    root.ParamList.Params.Add(new Param { Id = "USER_ID", Value = "0000000001" });
                    break;
                default:
                    root.ParamList.Params.Add(new Param { Id = "USER_ID", Value = "0000000001" });
                    break;
            }

            var xs = new XmlSerializer(typeof(WorkRoot));
            using (var sw = new StringWriter())
            {
                xs.Serialize(sw, root);
                return sw.ToString();
            }
        }
    }
}
