using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using RMSInterface.Structure.Payload.Response;

namespace RMSInterface.Structure.Payload
{
    public class ResponsePayloadParse
    {
        public ResponsePayloadParse() 
        {
            string xml = @"<?xml version='1.0' encoding='UTF-8'?>
<work_root>
<work_info>
<work_key>2012071214550000001</work_key>
<work_div>WD00</work_div>
<work_code>WC007</work_code>
</work_info>
<send_info ip='127.0.0.1' port='20013'/>
<receive_info ip='127.0.0.1' port='20014'/>
<param_list id='1'>
<param id='USER_ID'>0000000001</param>
</param_list>
<return_list id='1'>
<return_row>
<return_col id='CNTCHK_ID'>2012000001</return_col>
<return_col id='PLAN_YMD'>20121017</return_col>
<return_col id='USER_NM'>김기록</return_col>
<return_col id='CHECK_STATE_CD'>0</return_col>
</return_row>
</return_list>
</work_root>";
            var serializer = new XmlSerializer(typeof(WorkRoot));

            // ✅ 역직렬화 (Deserialize)
            WorkRoot result;
            using (var reader = new StringReader(xml))
            {
                result = (WorkRoot)serializer.Deserialize(reader);
            }

            Console.WriteLine($"WorkKey: {result.WorkInfo.WorkKey}");
            Console.WriteLine($"User Name: {result.ReturnList.Rows[0].Columns.Find(c => c.Id == "USER_NM").Value}");

            // ✅ 직렬화 (Serialize, UTF-8)
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = new UTF8Encoding(false),
                OmitXmlDeclaration = false
            };

            using (var stream = new MemoryStream())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, result);
                writer.Flush();

                string xmlOut = Encoding.UTF8.GetString(stream.ToArray());
                Console.WriteLine("\nSerialized XML:");
                Console.WriteLine(xmlOut);
            }
        }
    }
}
