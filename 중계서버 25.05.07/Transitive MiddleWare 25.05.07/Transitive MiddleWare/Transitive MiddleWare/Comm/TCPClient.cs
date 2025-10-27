using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BPNS.TransitiveMiddleware
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

		public TCPClient(string sIP, int iPort)
		{
			host = sIP;
			port = iPort;
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

			connectEvent.Reset();
			sendEvent.Reset();
			receiveEvent.Reset();

			try
			{
				IPAddress serverIP = IPAddress.Parse(host);
				IPEndPoint serverEndPoint = new IPEndPoint(serverIP, port);

				Socket oSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				oSocket.BeginConnect(serverEndPoint, new AsyncCallback(cbConnect), oSocket);
				connectEvent.WaitOne();

				if (!string.IsNullOrEmpty(strErr))
				{
					throw new Exception(strErr);
				}

				oSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(cbSend), oSocket);
				sendEvent.WaitOne();

				if (!string.IsNullOrEmpty(strErr))
				{
					throw new Exception(strErr);
				}

				StateObject state = new StateObject();
				state.workSocket = oSocket;

				oSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(cbReceive), state);
				receiveEvent.WaitOne();

				if (!string.IsNullOrEmpty(strErr))
				{
					throw new Exception(strErr);
				}

				oSocket.Shutdown(SocketShutdown.Both);
				oSocket.Close();

				state.ms.Position = 0;

				var bytes = state.ms.ToArray();

				strResult = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
			}
			catch (SocketException se)
			{
				throw se;
			}
			catch (Exception e)
			{
				throw e;
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

					//receiveEvent.Set();
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

				result = JObject.Parse(client.request(request.getBytes()));
			}
			catch (Exception e)
			{
				//MessageBox.Show(e.Message);//jck
			}

			return result;
		}

		public static JObject request(Dictionary<string, object> aData, string sIP, int iPort, bool bAdd)
		{
			JObject result = new JObject();

			try
			{
				TCPClient client = new TCPClient(sIP, iPort);
				TCPRequest request = new TCPRequest(aData);

				result = JObject.Parse(client.request(request.getBytes()));
			}
			catch (Exception e)
			{
				//MessageBox.Show(e.Message);//jck
			}

			return result;
		}

		public static JObject request(string aData, string sIP, int iPort, bool bAdd)
		{
			JObject result = new JObject();

			try
			{
				TCPClient client = new TCPClient(sIP, iPort);
				TCPRequest request = new TCPRequest(aData);

				result = JObject.Parse(client.request(request.getBytes()));
			}
			catch (Exception e)
			{
				//MessageBox.Show(e.Message);//jck
			}

			return result;
		}

		
	}
}
