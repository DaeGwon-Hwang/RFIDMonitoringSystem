using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using RFID.Middleware.Shared;
using System.IO;
using System.Xml.Serialization;
using RFID.Middleware.Server.Models;
using System.Collections.Generic;

namespace RFID.Middleware.Client
{
    public class MiddlewareClient
    {
        private readonly string _host;
        private readonly int _port;

        public MiddlewareClient(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public void RunTests()
        {
            var codes = new List<string> { "WC001","WC002","WC003","WC004","WC005","WC006","WC007","WC008","WC009","WC010","WC011","WC012","WC013","WC014","WC015" };
            foreach (var code in codes)
            {
                Console.WriteLine($"[CLIENT] Test {code} -> connecting to {_host}:{_port}");
                try
                {
                    using (var client = new TcpClient())
                    {
                        client.Connect(_host, _port);
                        using (var ns = client.GetStream())
                        {
                            var reqXml = TestScenarios.BuildRequestXml(code);
                            var bodyBytes = Encoding.UTF8.GetBytes(reqXml);
                            var header = new MessageHeader
                            {
                                CommandType = 0x02,
                                Status = 0,
                                Reserved1 = 0,
                                Reserved2 = 0,
                                TitleLength = 0,
                                BodyLength = bodyBytes.Length
                            };
                            ns.Write(header.ToBytes(), 0, 12);
                            ns.Write(bodyBytes, 0, bodyBytes.Length);
                            ns.Flush();

                            var headerBuf = new byte[12];
                            int read = 0;
                            while (read < 12)
                            {
                                int r = ns.Read(headerBuf, read, 12 - read);
                                if (r == 0) break;
                                read += r;
                            }
                            var respHeader = MessageHeader.FromBytes(headerBuf);
                            Console.WriteLine($"[CLIENT] Received header: Cmd=0x{respHeader.CommandType:X2}, BodyLen={respHeader.BodyLength}");
                            var bodyBuf = new byte[respHeader.BodyLength];
                            int br = 0;
                            while (br < bodyBuf.Length)
                            {
                                int r = ns.Read(bodyBuf, br, bodyBuf.Length - br);
                                if (r == 0) break;
                                br += r;
                            }
                            var respXml = Encoding.UTF8.GetString(bodyBuf);
                            Console.WriteLine("[CLIENT] Response XML:\n" + respXml);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[CLIENT] Error: " + ex.Message);
                }
                Thread.Sleep(200);
            }
        }
    }
}
