using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using RMSInterface.Structure.Payload.Request;

namespace RMSInterface.Structure.Payload
{
    public class RequestPayloadParse
    {
        public RequestPayloadParse() 
        {
            string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<work_root>
<work_info>
<work_key>2012071214550000001</work_key>
<work_div>WD00</work_div>
<work_code>WC007</work_code>
</work_info>
<send_infoip=""127.0.0.1"" port=""20014""/>
<receive_infoip=""127.0.0.1"" port=""20013""/>
<param_list id=""1"">
<param id=""USER_ID"">0000000001</param>
</param_list>
</work_root>
";
            // XML -> Object 변환
            XmlSerializer serializer = new XmlSerializer(typeof(WorkRoot));
            using (StringReader reader = new StringReader(xml))
            {
                WorkRoot workRoot = (WorkRoot)serializer.Deserialize(reader);

                Console.WriteLine("✅ XML 파싱 결과:");
                Console.WriteLine($"Work Key: {workRoot.WorkInfo.WorkKey}");
                Console.WriteLine($"Work Div: {workRoot.WorkInfo.WorkDiv}");
                Console.WriteLine($"Work Code: {workRoot.WorkInfo.WorkCode}");
                Console.WriteLine($"Send Info: {workRoot.SendInfo.Ip}:{workRoot.SendInfo.Port}");
                Console.WriteLine($"Receive Info: {workRoot.ReceiveInfo.Ip}:{workRoot.ReceiveInfo.Port}");
                Console.WriteLine($"ParamList Id: {workRoot.ParamList.Id}");
                foreach (var param in workRoot.ParamList.Params)
                {
                    Console.WriteLine($"  Param {param.Id} = {param.Value}");
                }
            }
        }
    }
}
