using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using RMSInterface.Structure.Payload.Request;

namespace RMSInterface.Structure.Payload
{
    public class RequestPayload
    {
        public RequestPayload() 
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
                    Port = 20014
                },
                ReceiveInfo = new ReceiveInfo
                {
                    Ip = "127.0.0.1",
                    Port = 20013
                },
                ParamList = new ParamList
                {
                    Id = "1",
                    Params = new List<Param>
                    {
                        new Param { Id = "USER_ID", Value = "0000000001" }
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
