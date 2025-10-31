using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using RFID.Middleware.Shared;
using System.IO;

namespace RFID.Middleware.Server
{
    public class MiddlewareServer
    {
        private readonly int _port;
        private TcpListener _listener;
        private bool _running = false;

        public MiddlewareServer(int port) { _port = port; }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _running = true;
            Console.WriteLine($"[SERVER] Listening on port {_port}...");
            ThreadPool.QueueUserWorkItem(AcceptLoop);
        }

        private void AcceptLoop(object state)
        {
            while (_running)
            {
                try
                {
                    var client = _listener.AcceptTcpClient();
                    Console.WriteLine("[SERVER] Client connected: " + client.Client.RemoteEndPoint);
                    ThreadPool.QueueUserWorkItem(HandleClient, client);
                }
                catch (SocketException) { break; }
            }
        }

        private void HandleClient(object state)
        {
            var client = (TcpClient)state;
            using (client)
            using (var ns = client.GetStream())
            {
                try
                {
                    while (_running && client.Connected)
                    {
                        var headerBuf = new byte[12];
                        int read = 0;
                        while (read < 12)
                        {
                            int r = ns.Read(headerBuf, read, 12 - read);
                            if (r == 0) return;
                            read += r;
                        }
                        var header = MessageHeader.FromBytes(headerBuf);
                        Console.WriteLine($"[SERVER] Received Header: Cmd=0x{header.CommandType:X2}, BodyLen={header.BodyLength}");

                        string title = string.Empty;
                        if (header.TitleLength > 0)
                        {
                            var tbuf = new byte[header.TitleLength];
                            int tr = 0;
                            while (tr < tbuf.Length)
                            {
                                int r = ns.Read(tbuf, tr, tbuf.Length - tr);
                                if (r == 0) break;
                                tr += r;
                            }
                            title = Encoding.UTF8.GetString(tbuf);
                        }

                        var bodyBuf = new byte[header.BodyLength];
                        int br = 0;
                        while (br < bodyBuf.Length)
                        {
                            int r = ns.Read(bodyBuf, br, bodyBuf.Length - br);
                            if (r == 0) break;
                            br += r;
                        }
                        var xml = Encoding.UTF8.GetString(bodyBuf);
                        Console.WriteLine("[SERVER] Received XML:\n" + xml);

                        var responseXml = RequestHandler.ProcessRequest(xml);

                        var respHeader = new MessageHeader
                        {
                            CommandType = (byte)(header.CommandType | 0x80),
                            Status = 0,
                            Reserved1 = 0,
                            Reserved2 = 0,
                            TitleLength = 0,
                            BodyLength = Encoding.UTF8.GetByteCount(responseXml)
                        };
                        ns.Write(respHeader.ToBytes(), 0, 12);
                        var bodyBytes = Encoding.UTF8.GetBytes(responseXml);
                        ns.Write(bodyBytes, 0, bodyBytes.Length);
                        ns.Flush();
                        Console.WriteLine("[SERVER] Sent response for command 0x" + header.CommandType.ToString("X2"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[SERVER] Client handler error: " + ex.Message);
                }
            }
        }

        public void Stop()
        {
            _running = false;
            _listener?.Stop();
        }
    }
}
