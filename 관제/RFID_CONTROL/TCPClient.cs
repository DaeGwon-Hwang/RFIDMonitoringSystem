using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace RFID_CONTROLLER
{
    class TCPClient
    {
        public TCPClient()
        {

            //host = "192.168.200.21";
            //port = 12012;
            host = Utils.HOST_IP;
            port = Utils.HOST_PORT;
        }

        private ManualResetEvent connectEvent = new ManualResetEvent(false);
        private ManualResetEvent sendEvent = new ManualResetEvent(false);
        private ManualResetEvent receiveEvent = new ManualResetEvent(false);

        private string strErr = "";

        public string host
        {
            get; set;
        }

        public int port
        {
            get; set;
        }

        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 256;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];

            public MemoryStream ms = new MemoryStream();
        }

        public string getErrorMessage()
        {
            return strErr;
        }

        public string request(string strData)
        {
            //return request(Encoding.UTF8.GetBytes(strData));
            return request(Encoding.Default.GetBytes(strData));
        }

        public string request(byte[] byteData)
        {
            strErr = "";
            string strResult = "";
            bool bFalse = true; 

            connectEvent.Reset();
            sendEvent.Reset();
            receiveEvent.Reset();

            Socket oSocket = null;
            try
            {
                MainController.isSocketRun = true;
                IPAddress serverIP = IPAddress.Parse(host);
                IPEndPoint serverEndPoint = new IPEndPoint(serverIP, port);

                Console.WriteLine("host > " + serverIP);
                Console.WriteLine("port > " + port);

                oSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                
                oSocket.BeginConnect(serverEndPoint, new AsyncCallback(cbConnect), oSocket);
                Console.WriteLine("connectEvent.WaitOne > " + connectEvent.WaitOne());
                connectEvent.WaitOne();

                if (!string.IsNullOrEmpty(strErr))
                {
                    bFalse = false;
                    throw new Exception(strErr);
                }

                oSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(cbSend), oSocket);
                Console.WriteLine("sendEvent.WaitOne > " + sendEvent.WaitOne());
                sendEvent.WaitOne();

                if (!string.IsNullOrEmpty(strErr))
                {
                    bFalse = false;
                    throw new Exception(strErr);
                }

                StateObject state = new StateObject();
                state.workSocket = oSocket;

                oSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(cbReceive), state);

                int timeoutMilliseconds = 5000;
                if (receiveEvent.WaitOne(timeoutMilliseconds))
                {
                    Console.WriteLine("Send operation completed successfully.");
                }
                else
                {
                    bFalse = false;
                    Console.WriteLine("Timeout: No response from server. Closing socket.");
                    throw new Exception(strErr);
                }
                /*
                if (!receiveEvent.WaitOne(20000))
                {
                    throw new Exception("서버로부터 응답을 받을 수 없습니다.");
                }
                
                if (!string.IsNullOrEmpty(strErr))
                {
                    throw new Exception(strErr);
                }
                */

                oSocket.Shutdown(SocketShutdown.Both);

                state.ms.Position = 0;

                var bytes = state.ms.ToArray();

                strResult = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            }
            catch (SocketException se)
            {
                bFalse = false;
                throw se;
            }
            catch (Exception e)
            {
                bFalse = false;
                throw e;
            }
            finally
            {
                MainController.isSocketRun = false;
                if(bFalse) oSocket.Close();

            }

            return strResult;
        }

        private void cbConnect(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                client.EndConnect(ar);
            }
            catch (SocketException e)
            {
                strErr = e.Message;
            }
            finally
            {
                connectEvent.Set();
            }
        }

        private void cbSend(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                client.EndSend(ar);
            }
            catch (SocketException e)
            {
                strErr = e.Message;
            }
            finally
            {
                sendEvent.Set();
            }
        }

        private void cbReceive(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.ms.Write(state.buffer, 0, bytesRead);

                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(cbReceive), state);
                }
                else
                {
                    receiveEvent.Set();
                }
            }
            catch (SocketException e)
            {
                strErr = e.Message;

                receiveEvent.Set();
            }
        }

        public static JObject request(Dictionary<string, object> aData)
        {
            
            JObject result = new JObject();

            try
            {
                TCPClient client = new TCPClient();
                TCPRequest request = new TCPRequest(aData);


                string reqResult = client.request(request.getBytes());
                if (!string.IsNullOrWhiteSpace(reqResult))
                {
                    result = JObject.Parse(reqResult);
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);//jck
            }

            //string jsonString = result.ToString();
            //Console.WriteLine("response > " + jsonString);

            return result;
        }
    }
}
