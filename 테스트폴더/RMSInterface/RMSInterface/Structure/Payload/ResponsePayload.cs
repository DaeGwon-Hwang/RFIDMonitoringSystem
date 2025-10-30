using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RMSInterface.Structure.Payload.Response;

namespace RMSInterface.Structure
{
    public class ResponsePayload
    {
        public ResponsePayload() 
        {
            var request = new WorkRoot
            {
                WorkInfo = new WorkInfo
                {
                    WorkKey = "2012071214550000001",
                    WorkDiv = "WD00",
                    WorkCode = "WC007"
                },
                SendInfo = new SendInfo
                {
                    Ip = "127.0.0.1",
                    Port = 20013
                },
                ReceiveInfo = new ReceiveInfo
                {
                    Ip = "127.0.0.1",
                    Port = 20014
                },
                ParamList = new ParamList
                {
                    Id = "1",
                    Params = new List<Param>
                    {
                        new Param { Id = "USER_ID", Value = "0000000001" }
                    }
                },
                ReturnList = new ReturnList
                { 
                    Id = "1",
                    Rows = new List<ReturnRow>
                    {
                        new ReturnRow 
                        {
                            Columns = new List<ReturnCol>
                            {
                                new ReturnCol { Id = "CNTCHK_ID", Value = "2012000001" },
                                new ReturnCol { Id = "PLAN_YMD", Value = "20121017" },
                                new ReturnCol { Id = "USER_NM", Value = "김기록" },
                                new ReturnCol { Id = "CHECK_STATE_CD", Value = "0" }
                            }
                        }
                    }
                }
            };

            var serializer = new XmlSerializer(typeof(WorkRoot));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, request);
                Console.WriteLine(writer.ToString());
            }
        }
    }
}
