
namespace BPNS.TransitiveMiddleware
{
    using System;
    using System.Text;
    using System.Net.Sockets;
    using System.Windows.Forms;
    using System.Collections;
    using System.Data;
    using Comm;             //enum CommandCode
    using System.Xml;

    using System.Collections.Generic;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    //[Serializable]
    class PamsClientSocketHandler : IDisposable
    {
        #region Attributes(클래스 속성)
        private Socket clientSocket;

        public Socket ClientSocket
        {
            get { return clientSocket; }
            set { clientSocket = value; }
        }

        private volatile object oClientSocketCO = new object();

        /// <summary>
        /// 마지막 통신 시간 저장(Timeout 체크용)
        /// </summary>
        private DateTime lastCommunicationTime;

        public DateTime LastCommunicationTime
        {
            get { return lastCommunicationTime; }
            set { lastCommunicationTime = value; }
        }

        /// <summary>
        /// 마지막 Health Check 전송시간(Agent 정상 동작 확인용)
        /// </summary>
        private DateTime lastHealthCheckTime;

        private DateTime dteCRCheckTime;

        private DateTime dteLRCheckTime;

        /// <summary>
        /// 마지막 Health Check 통신시 수신한 Count
        /// </summary>
        private int nLastHealthCheckCount;

        private string sMWID;
        public string MiddlewareID
        {
            get { return sMWID; }
            set { sMWID = value; }
        }

        private string sIP;

        private DateTime connectedDateTime;
        public DateTime ConnectedDateTime
        {
            get { return connectedDateTime; }
            set { connectedDateTime = value; }
        }
        private bool bDisconnectedDateTimeUpdated = false;
        private DateTime disconnectedDateTime;

        public DateTime DisconnectedDateTime
        {
            get { return disconnectedDateTime; }
            set { disconnectedDateTime = value; }
        }

        // 수신 버퍼, 버퍼 사이즈, 버퍼 시작점
        //      private Socket socket;
        private byte[] ReceivceBuffer;
        private int ReceiveBufferSize;
        private int ReceiveBufferOffset;

        // 파싱처리한 데이터 버퍼 데이터 수신 -> 수신버퍼 -> 패킷분할 -> 파싱버퍼 -> 프로토콜 처리
        private byte[] parsingBuffer;
        private int parsingBufferSize;
        private int parsingBufferOffset;

        private object parsingBufferCO = new object();
        private const byte STX = 0x02;
        private const byte ETX = 0x03;

        private bool bDisposed = false;
        //private Encoding EdASCII = Encoding.ASCII; //private Encoding EUC_KR = Encoding.GetEncoding("euc-kr");
        private Encoding EdASCII = Encoding.UTF8; //private Encoding EUC_KR = Encoding.GetEncoding("euc-kr");

        public bool bComplete = false;

        #endregion

        #region Constructor(생성자)
        public PamsClientSocketHandler()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ReceivceBuffer = new byte[Properties.Settings.Default.NET_BufferSize];
            ReceiveBufferSize = 0;
            ReceiveBufferOffset = 0;
            parsingBuffer = new byte[Properties.Settings.Default.NET_BufferSize];
            parsingBufferOffset = 0;
            parsingBufferSize = 0;

            dteCRCheckTime = DateTime.Now;
            dteLRCheckTime = DateTime.Now;
        }

        public PamsClientSocketHandler(Socket acceptedSocket)
        {
            // 소켓이 연결을 수락할때 처리
            // Accept된 Socket 저장
            clientSocket = acceptedSocket;
            // 소켓 옵션 설정(사용중인 주소 재사용 불가)
            //clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //clientSocket.LingerState = new LingerOption(true, 0);
            ReceivceBuffer = new byte[Properties.Settings.Default.NET_BufferSize];
            ReceiveBufferSize = 0;
            ReceiveBufferOffset = 0;
            parsingBuffer = new byte[Properties.Settings.Default.NET_BufferSize];
            parsingBufferOffset = 0;
            parsingBufferSize = 0;
            connectedDateTime = DateTime.Now;
            Program.AppLog.Write("소켓 연결[Handler:" + acceptedSocket.Handle.ToInt64().ToString() + "]");

            dteCRCheckTime = DateTime.Now;
            dteLRCheckTime = DateTime.Now;

            //최초 장비 번호 없을 경우 ip로 셋팅
            sMWID = clientSocket.RemoteEndPoint.ToString().Substring(0, clientSocket.RemoteEndPoint.ToString().IndexOf(':'));
            sIP = clientSocket.RemoteEndPoint.ToString().Substring(0, clientSocket.RemoteEndPoint.ToString().IndexOf(':'));
        }
        #endregion

        public bool bCheckConnection()
        {
            bool bResult = false;

            //lock (oLibraryCO)
            {
                //Program.AppLog.Write(this, "lock oLibraryCO Enter");
                if (clientSocket?.Connected == false)
                {
                    if (!bDisconnectedDateTimeUpdated)
                    {
                        bDisconnectedDateTimeUpdated = true;
                        disconnectedDateTime = DateTime.Now;
                        AddMiddlewareConnectHistory(sMWID, false);
                    }
                    return false;
                }
                bResult = true;
                byte[] readBuffer;
                readBuffer = new byte[1];
                // 일반적으로 소켓의 Connected 변수는 연결 상태를 정확히 체크하지 못하므로 소켓 Poll 함수를 이용하여 Read 상태 체크를 이용해야 한다.
                if (clientSocket.Poll(0, SelectMode.SelectRead))
                {
                    //int nReadByte = m_Socket.Receive(readBuffer, SocketFlags.Peek);
                    int nReadByte = clientSocket.Available;
                    if (nReadByte == 0)
                    {
                        if (!bDisconnectedDateTimeUpdated)
                        {
                            bDisconnectedDateTimeUpdated = true;
                            disconnectedDateTime = DateTime.Now;
                            AddMiddlewareConnectHistory(sMWID, false);
                        }
                        bResult = false;
                    }
                }
                //Program.AppLog.Write(this, "lock oLibraryCO Exit");
            }
            return bResult;
        }

        public bool bCheckTimeout()
        {
            try
            {
                if ((DateTime.Now - lastCommunicationTime) < new TimeSpan(0, 0, 0, 0, Properties.Settings.Default.NET_MWTimeout))
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "Socket Timeout Check Exception!", e);
            }
            return true;
        }

        public void SetLastCommTime()
        {
            lastCommunicationTime = DateTime.Now;
        }

        public void setAsyncReceive()
        {
            try
            {
                clientSocket.BeginReceive(ReceivceBuffer, ReceiveBufferOffset, Properties.Settings.Default.NET_BufferSize - ReceiveBufferSize - 1, 0, new AsyncCallback(AsyncReceiveCallback), this);
            }
            catch (ObjectDisposedException ode)
            {
                Program.AppLog.Write(this, "socket closed!");
                Program.SysLog.Write(this, "socket closed!(ObjectDisposedException!", ode);
            }
            catch (SocketException se)
            {
                if (!clientSocket.Connected)
                {
                    Program.AppLog.Write(this, "connection closed by peer![" + se.Message + "]");
                }
                else
                {
                    Program.SysLog.Write(this, "socket error!", se);
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "setAsyncReceive 오류", e);
            }
        }

        int _nPacketSize = 0;
        byte[] _packetBuffer;

        public void AsyncReceiveCallback(IAsyncResult ar)
        {
            PamsClientSocketHandler clientSocketHandler = (PamsClientSocketHandler)ar.AsyncState;
            int nRead;

            nRead = 0;
            try
            {
                nRead = clientSocketHandler.clientSocket.EndReceive(ar);
                // buffer overflow 방지를 위한 순환 버퍼 처리 추가
                lock (parsingBufferCO)
                {
                    if (nRead == 0)
                    {
                        clientSocketHandler.ClientSocket.Close();
                        Program.AppLog.Write(this, "Connection Closed by peer!");
                        return;
                    }
                    else
                    {
                        SetLastCommTime();

                        if (_nPacketSize > 0)
                        {
                            byte[] _packetBuffer_temp = new byte[Properties.Settings.Default.NET_BufferSize];
                            System.Buffer.BlockCopy(ReceivceBuffer, ReceiveBufferOffset, _packetBuffer_temp, 0, nRead);

                            _packetBuffer = Combine(_packetBuffer, _packetBuffer_temp);
                        }
                        else
                        {
                            _packetBuffer = new byte[Properties.Settings.Default.NET_BufferSize];

                            //System.Buffer.BlockCopy(ReceivceBuffer, ReceiveBufferOffset, parsingBuffer, parsingBufferOffset + parsingBufferSize, nRead);
                            System.Buffer.BlockCopy(ReceivceBuffer, ReceiveBufferOffset, _packetBuffer, _nPacketSize, nRead);
                        }

                        //parsingBufferSize += nRead;
                        Program.AppLog.Write("Recv :(" + sMWID + ")[" + EdASCII.GetString(clientSocketHandler.ReceivceBuffer, clientSocketHandler.ReceiveBufferOffset, nRead) + "]");
                    }
                }

                ParsePacketCheck();


                // 수신 버퍼 파싱 (STX, ETX를 이용한 한 패킷 분리)
                //clientSocketHandler.ParseBuffer();

            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    Program.AppLog.Write(this, "소켓 접속 끊김[" + se.Message + "]");
                    //                    Program.SysLog.ExceptionLogWrite(this, "Connection Closed by peer", se);
                }
                else
                {
                    Program.AppLog.Write(this, "소켓 오류![" + se.Message + "]");
                    Program.SysLog.Write(this, "소켓 오류!", se);
                }

                // return 하기 전에 연결 끊어짐에 대한 처리 루틴 추가

                return;
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "AsyncReceiveCallback[BufferOffset:" + ReceiveBufferOffset.ToString() + ",parsingBufferOffset:" + parsingBufferOffset.ToString() + ", parsingBufferSize:" + parsingBufferSize.ToString() + ",nRead:" + nRead.ToString() + "]", e);
            }

            // Parse Receive Data
            try
            {
                clientSocketHandler.ClientSocket.BeginReceive(ReceivceBuffer, ReceiveBufferOffset, Properties.Settings.Default.NET_BufferSize - ReceiveBufferSize - 1, 0, new AsyncCallback(AsyncReceiveCallback), clientSocketHandler);
            }
            catch (SocketException se)
            {
                Program.AppLog.Write(this, "소켓 오류![" + se.Message + "]");
                Program.SysLog.Write(this, "소켓 오류!", se);
            }
            catch (ObjectDisposedException ode)
            {
                Program.AppLog.Write(this, "소켓 접속 끊김![" + ode.Message + "]");
                Program.SysLog.Write(this, "소켓 접속 끊김!", ode);
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "Unexpected exception![" + e.Message + "]");
                Program.SysLog.Write(this, "Unexpected exception!", e);
            }
        }

        public byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        public static object ByteToObject(byte[] buffer)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    stream.Position = 0;
                    return binaryFormatter.Deserialize(stream);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return null;
        }

        public static object ByteToObject2(byte[] buffer)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    stream.Position = 0;
                    return binaryFormatter.Deserialize(stream);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return null;
        }

        public void ParsePacketCheck()
        {
            //string strRecvData = EdASCII.GetString(_packetBuffer, 0, _packetBuffer.Length);
            string strRecvData = EdASCII.GetString(_packetBuffer, 0, _packetBuffer.Length);

            //object objTest = ByteToObject(_packetBuffer);

            //string strRecvData = Convert.ToBase64String(_packetBuffer, 0, _packetBuffer.Length);

            //int iSOF = strRecvData.IndexOf("t\u0001");
            //int iEOF = strRecvData.Substring(iSOF + 3).LastIndexOf("y");

            int iSOF = strRecvData.IndexOf("{\"");
            int iEOF = strRecvData.Substring(iSOF).LastIndexOf("y");



            string sTrRecvCmd;

            JObject result = new JObject();
            if (iEOF > 0 && iSOF > 0)
            {
                //string sSubData = strRecvData.Substring(iSOF + 3, iEOF);
                //sTrRecvCmd = strRecvData.Substring(0, iEOF);
                //strRecvData.Substring(iSOF + 3, iEOF);
                sTrRecvCmd = strRecvData.Substring(iSOF, iEOF);
                sTrRecvCmd = sTrRecvCmd.Replace("\0", "");

                //result = JObject.Parse(sTrRecvCmd);

                //24.02.15 로그제외
                //Program.AppLog.Write("변환문자열 :" + strRecvData);

                Program.AppLog.Write("변환문자열(자른데이터) :" + sTrRecvCmd);
                _nPacketSize = 0;

                ParsePacket_New(sTrRecvCmd);
                //ParsePacket_New(parsingBuffer);
            }
            else
            {
                //24.02.15 로그제외
                //Program.AppLog.Write("시작완료문자열 없음" + strRecvData);
                _nPacketSize = _packetBuffer.Length;
                //SendResponse_New("<CMD:EOF> 구문 없음", "<CMD:EOF> 구문 없음" + strRecvData);
            }

        }

        public void ParsePacket_New(string strRecvData)
        {
            string strCommandCode;


            try
            {
                JObject result = new JObject();
                result = JObject.Parse(strRecvData);

                //Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(strRecvData);

                strCommandCode = result["WORKDIV"].ToString();

                switch (strCommandCode)
                {
                    //ALERTRELEASE
                    case "ALERTRELEASE":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 무단반출입 : 경보해지");
                        ALERTRELEASE(result);

                        break;

                    //ALIVECHECK
                    case "ALIVECHECK":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : PAMS로 부터 수신된 ALIVE CHECK");
                        ALIVECHECK(result);

                        break;

                    //BKROOMALLLIST
                    case "BKROOMALLLIST":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 서고서가연단 목록");
                        BKROOMALLLIST(result);

                        break;

                    //EXCTNPERMN
                    case "EXCTNPERMN":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 미승인 무단 반출/입 제외 기록물 전송");
                        EXCTNPERMN(result);

                        break;

                    //INOUTRST
                    case "INOUTRST":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 반입/반출 처리 후 RFID에 결과 전송");
                        INOUTRST(result);

                        break;

                    //NPMNTRTMRCV
                    case "NPMNTRTMRCV":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 미승인 무단 반출/입 처리 결과 전송");
                        NPMNTRTMRCV(result);
                        break;

                    //READERSTATE
                    case "READERSTATE":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 고정형리더기 상태 조회 후 PAMS전송");
                        READERSTATE(result);

                        break;

                    //RETAGID
                    case "RETAGID":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 태그재발행");
                        RETAGID(result);
                        break;

                    //TAGID
                    case "TAGID":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 태그발행");
                        TAGID(result);
                        break;

                    case "INREGDT":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 반입예정일 변경");
                        INREGDT(result);
                        break;

                        //default:
                        //    ((MainForm)(Application.OpenForms["MainForm"])).AddLogList(strRecvData);
                        //    Program.AppLog.Write("Case default : " + strRecvData);
                        //    break;
                }

            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "프로토콜 파싱 오류", e);
            }
        }

        /// <summary>
        /// 무단반출입 : 경보해지
        /// </summary>
        /// <param name="request"></param>
        public void ALERTRELEASE(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 무단반출입
	         * 		서브업무명 : 경보해지
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		GATE_ID		: GATE ID
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         *  ===============================================
             */

            string sWORKDIV = "ALERTRELEASE";

            if (request.ContainsKey("WORKDIV")) request["WORKDIV"] = "1110REQ";
            else request.Add("WORKDIV", "1110REQ");

            if (request.ContainsKey("WK_TYPE")) request["WK_TYPE"] = "OBZ";
            else request.Add("WK_TYPE", "OBZ");

            //확인 필요
            MwClient.connect(request);

            Dictionary<string, object> aData = new Dictionary<string, object>();
            aData.Add("WORKDIV", sWORKDIV + "REP");
            aData.Add("RESULT", "0");

            JObject json = new JObject();

            if (aData != null)
            {
                foreach (string strKey in aData.Keys)
                {
                    JProperty property = new JProperty(strKey, aData[strKey]);

                    json.Add(property);
                }
            }

            string sRtn = json.ToString();

            SendResponse_New(sRtn, sWORKDIV + "REQ");

        }

        /// <summary>
        /// PAMS로 부터 수신된 ALIVE CHECK
        /// </summary>
        /// <param name="request"></param>
        public void ALIVECHECK(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : PAMS로 부터 수신된 ALIVE CHECK
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         *  ===============================================
             */

            string sWORKDIV = "ALIVECHECK";

            Dictionary<string, object> aData = new Dictionary<string, object>();
            aData.Add("WORKDIV", sWORKDIV + "REP");
            aData.Add("RESULT", "0");

            JObject json = new JObject();

            if (aData != null)
            {
                foreach (string strKey in aData.Keys)
                {
                    JProperty property = new JProperty(strKey, aData[strKey]);

                    json.Add(property);
                }
            }

            string sRtn = json.ToString();

            SendResponse_New(sRtn, sWORKDIV + "REQ");

        }


        /// <summary>
        /// 서고서가연단 목록 연동
        /// </summary>
        /// <param name="request"></param>
        public void BKROOMALLLIST(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 공통
	         * 		서브업무명 : 서고서가연단 목록
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         *  	BKROOM_LIST		: 서고서가연단 목록
	         *  	  - BKROOM_ID	: 서고 ID
	         *  	  - BKROOM_NM	: 서고명
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		RESULT_CD : 결과코드
	         *  ===============================================
             */

            string sWORKDIV = "BKROOMALLLIST";



            Hashtable htParamsHashtable;

            htParamsHashtable = new Hashtable();
            htParamsHashtable.Clear();

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.BKROOMALLLIST_DELETE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
            strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
            strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();


            bool bRtn = true;
            if (strAppCode.Equals("0"))
            {
                //List<Dictionary<string, object>> list = ((List<Dictionary<string, object>>)(request["BKROOM_LIST"]));
                JArray list = (JArray)(request["BKROOM_LIST"]);

                string sBKROOM_ID = string.Empty;
                string sBKSTND_ID = string.Empty;
                string sBKSHLF_ID = string.Empty;
                for (int i = 0; i < list.Count; i++)
                {

                    sBKROOM_ID = list[i]["BKROOM_ID"] == null ? "" : list[i]["BKROOM_ID"].ToString();
                    sBKSTND_ID = list[i]["BKSTND_ID"] == null ? "" : list[i]["BKSTND_ID"].ToString();
                    sBKSHLF_ID = list[i]["BKSHLF_ID"] == null ? "" : list[i]["BKSHLF_ID"].ToString();
                    if (sBKROOM_ID.Trim().Length > 0)
                    {
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_BKROOM_ID", sBKROOM_ID);
                        htParamsHashtable.Add("IN_BKROOM_NM", list[i]["BKROOM_NM"] == null ? "" : list[i]["BKROOM_NM"].ToString());

                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.BKROOM_MST_SAVE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        if (!strAppCode.Equals("0"))
                        {
                            bRtn = false;
                            break;
                        }
                    }

                    if (sBKSTND_ID.Trim().Length > 0)
                    {
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_BKROOM_ID", sBKROOM_ID);
                        htParamsHashtable.Add("IN_BKSTND_ID", sBKSTND_ID);
                        htParamsHashtable.Add("IN_BKSTND_NM", list[i]["BKSTND_NM"] == null ? "" : list[i]["BKSTND_NM"].ToString());

                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.BKSTND_MST_SAVE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        if (!strAppCode.Equals("0"))
                        {
                            bRtn = false;
                            break;
                        }
                    }

                    if (sBKSHLF_ID.Trim().Length > 0)
                    {
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_BKROOM_ID", sBKROOM_ID);
                        htParamsHashtable.Add("IN_BKSTND_ID", sBKSTND_ID);
                        htParamsHashtable.Add("IN_BKSHLF_ID", sBKSHLF_ID);
                        htParamsHashtable.Add("IN_COL_NO", list[i]["COL_NO"] == null ? "" : list[i]["COL_NO"].ToString());
                        htParamsHashtable.Add("IN_ROW_NO", list[i]["ROW_NO"] == null ? "" : list[i]["ROW_NO"].ToString());

                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.BKSHLF_MST_SAVE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        if (!strAppCode.Equals("0"))
                        {
                            bRtn = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                bRtn = false;

                Program.AppLog.Write("PAMSDATA.BKROOMALLLIST_DELETE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
                ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.BKROOMALLLIST_DELETE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");


            }

            //if (bRtn)
            //{
            //    htParamsHashtable.Clear();

            //    ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.BKROOMALLLIST_DELETE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
            //    strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
            //    strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();
            //}

            Dictionary<string, object> aData = new Dictionary<string, object>();
            aData.Add("WORKDIV", sWORKDIV + "REP");
            aData.Add("RESULT", "0");

            JObject json = new JObject();

            if (aData != null)
            {
                foreach (string strKey in aData.Keys)
                {
                    JProperty property = new JProperty(strKey, aData[strKey]);

                    json.Add(property);
                }
            }

            string sRtn = json.ToString();

            SendResponse_New(sRtn, sWORKDIV + "REQ");

        }

        /// <summary>
        /// 미승인 무단 반출/입 제외 기록물 전송
        /// </summary>
        /// <param name="request"></param>
        public void EXCTNPERMN(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 공통
	         * 		서브업무명 : 미승인 무단 반출/입 제외 기록물 전송
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		NPERMN_OUT_APPL_ID 	: 제외의뢰서ID
	         *  	LIST				: 목록
	         *  	  - PAMS_MGMT_NO	: 관리번호
	         *  	  - BIND_NO			: 기록물철 ID
	         *  	  - TAG_ID			: 태그 ID
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		RESULT_CD : 결과코드
	         *  ===============================================
             */

            string sWORKDIV = "EXCTNPERMN";

            Hashtable htParamsHashtable;

            htParamsHashtable = new Hashtable();
            htParamsHashtable.Clear();

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sNPERMN_OUT_APPL_ID = string.Empty;
            string sPAMS_MGMT_NO = string.Empty;
            string sTAG_ID = string.Empty;
            string sEXTDT = string.Empty;
            string sEXTENDDT = string.Empty;
            string sBIND_NO = string.Empty;
            string sINOUT_FLAG = "EXP";

            string sFILE_BREAK_ID = string.Empty;

            bool bRtn = true;

            if (request.ContainsKey("NPERMN_OUT_APPL_ID"))
            {
                sNPERMN_OUT_APPL_ID = request["NPERMN_OUT_APPL_ID"] == null ? "" : request["NPERMN_OUT_APPL_ID"].ToString();
                sEXTDT = request["EXTDT"] == null ? "" : request["EXTDT"].ToString();
                sEXTENDDT = request["EXTENDDT"] == null ? "" : request["EXTENDDT"].ToString();
            }

            if (sNPERMN_OUT_APPL_ID.Trim().Length > 0)
            {
                htParamsHashtable.Add("IN_NPERMN_OUT_APPL_ID", sNPERMN_OUT_APPL_ID);
                ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_NPERMN_TAG_DELETE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                //List<Dictionary<string, object>> list = ((List<Dictionary<string, object>>)(request["LIST"]));
                JArray list = ((JArray)(request["LIST"]));

                for (int i = 0; i < list.Count; i++)
                {
                    sPAMS_MGMT_NO = list[i]["noelecDcrymanageNo"] == null ? "" : list[i]["noelecDcrymanageNo"].ToString(); //list[i]["PAMS_MGMT_NO"] == null ? "" : list[i]["PAMS_MGMT_NO"].ToString();
                    sTAG_ID = list[i]["tagid"] == null ? "" : list[i]["tagid"].ToString(); //list[i]["TAG_ID"] == null ? "" : list[i]["TAG_ID"].ToString();

                    sBIND_NO = list[i]["rcBindNo"] == null ? "" : list[i]["rcBindNo"].ToString(); //list[i]["BIND_NO"] == null ? "" : list[i]["BIND_NO"].ToString();

                    try
                    {
                        sFILE_BREAK_ID = list[i]["FILE_BREAK_ID"] == null ? "" : list[i]["FILE_BREAK_ID"].ToString().Trim().Replace("\0", "").Replace("00", "");
                    }
                    catch (Exception e)
                    {
                        Program.SysLog.Write(this, "EXCTNPERMN FILE_BREAK_ID 파싱 Exception", e);
                        sFILE_BREAK_ID = "";
                    }

                    if (sPAMS_MGMT_NO.Trim().Length > 0 && sTAG_ID.Trim().Length > 0)
                    {
                        //querylist.add(new SqlMapVO("TRFID_NPERMN_TAG.insert", row));
                        //querylist.add(new SqlMapVO("TRFID_OUTIN_STTS.delete", row));
                        //querylist.add(new SqlMapVO("TRFID_OUTIN_STTS.insert", row));

                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_NPERMN_OUT_APPL_ID", sNPERMN_OUT_APPL_ID);
                        htParamsHashtable.Add("IN_PAMS_MGMT_NO", sPAMS_MGMT_NO);
                        htParamsHashtable.Add("IN_BIND_NO", sBIND_NO);
                        htParamsHashtable.Add("IN_TAG_ID", sTAG_ID);
                        htParamsHashtable.Add("IN_EXTDT", sEXTDT);
                        htParamsHashtable.Add("IN_EXTENDDT", sEXTENDDT);

                        htParamsHashtable.Add("IN_FILE_BREAK_ID", sFILE_BREAK_ID);

                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_NPERMN_TAG_INSERT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_PAMS_MGMT_NO", sPAMS_MGMT_NO);
                        htParamsHashtable.Add("IN_FILE_BREAK_ID", sFILE_BREAK_ID);

                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_OUTIN_STTS_DELETE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_PAMS_MGMT_NO", sPAMS_MGMT_NO);
                        htParamsHashtable.Add("IN_TAG_ID", sTAG_ID);
                        htParamsHashtable.Add("IN_INOUT_FLAG", sINOUT_FLAG);
                        htParamsHashtable.Add("IN_EXTDT", sEXTDT);
                        htParamsHashtable.Add("IN_EXTENDDT", sEXTENDDT);

                        htParamsHashtable.Add("IN_FILE_BREAK_ID", sFILE_BREAK_ID);

                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_OUTIN_STTS_INSERT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();
                    }
                }
            }
            else
            {
                //List<Dictionary<string, object>> list = ((List<Dictionary<string, object>>)(request["LIST"]));
                JArray list = ((JArray)(request["LIST"]));

                for (int i = 0; i < list.Count; i++)
                {
                    //querylist.add(new SqlMapVO("TRFID_NPERMN_TAG.delete", row));
                    sPAMS_MGMT_NO = list[i]["noelecDcrymanageNo"] == null ? "" : list[i]["noelecDcrymanageNo"].ToString(); //list[i]["PAMS_MGMT_NO"] == null ? "" : list[i]["PAMS_MGMT_NO"].ToString();
                    sTAG_ID = list[i]["tagid"] == null ? "" : list[i]["tagid"].ToString(); //list[i]["TAG_ID"] == null ? "" : list[i]["TAG_ID"].ToString();

                    sBIND_NO = list[i]["rcBindNo"] == null ? "" : list[i]["rcBindNo"].ToString(); //list[i]["BIND_NO"] == null ? "" : list[i]["BIND_NO"].ToString();

                    try
                    {
                        sFILE_BREAK_ID = list[i]["FILE_BREAK_ID"] == null ? "" : list[i]["FILE_BREAK_ID"].ToString().Trim().Replace("\0", "").Replace("00", "");
                    }
                    catch (Exception e)
                    {
                        Program.SysLog.Write(this, "EXCTNPERMN FILE_BREAK_ID 파싱 Exception", e);
                        sFILE_BREAK_ID = "";
                    }

                    if (((JObject)(list[i])).ContainsKey("NPERMN_OUT_APPL_ID"))
                    {
                        sNPERMN_OUT_APPL_ID = list[i]["NPERMN_OUT_APPL_ID"] == null ? "" : list[i]["NPERMN_OUT_APPL_ID"].ToString();

                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_NPERMN_OUT_APPL_ID", sNPERMN_OUT_APPL_ID);
                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_NPERMN_TAG_DELETE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();
                    }
                }
            }


            Dictionary<string, object> aData = new Dictionary<string, object>();
            aData.Add("WORKDIV", sWORKDIV + "REP");
            aData.Add("RESULT", "0");

            JObject json = new JObject();

            if (aData != null)
            {
                foreach (string strKey in aData.Keys)
                {
                    JProperty property = new JProperty(strKey, aData[strKey]);

                    json.Add(property);
                }
            }

            string sRtn = json.ToString();

            SendResponse_New(sRtn, sWORKDIV + "REQ");

        }


        /// <summary>
        /// 반입/반출 처리 후 RFID에 결과 전송
        /// </summary>
        /// <param name="request"></param>
        public void INOUTRST(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 반출/반입
	         * 		서브업무명 : 반입/반출 처리 후 RFID에 결과 전송
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		INOUT_FLAG = IN, OUT
	         * 		LIST
	         * 		  - TAG_ID : 태그
	         * 		  - PAMS_MGMT_NO : 관리번호
	         *  ===============================================
             */

            string sWORKDIV = "INOUTRST";

            Hashtable htParamsHashtable;

            htParamsHashtable = new Hashtable();
            htParamsHashtable.Clear();

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sNPERMN_OUT_APPL_ID = string.Empty;
            string sPAMS_MGMT_NO = string.Empty;
            string sTAG_ID = string.Empty;
            string sEXTDT = string.Empty;
            string sEXTENDDT = string.Empty;
            string sBIND_NO = string.Empty;
            string sINOUT_FLAG = string.Empty;

            string sFILE_BREAK_ID = string.Empty;

            bool bRtn = true;

            //List<Dictionary<string, object>> list = ((List<Dictionary<string, object>>)(request["LIST"]));
            JArray list = ((JArray)(request["LIST"]));

            for (int i = 0; i < list.Count; i++)
            {
                sPAMS_MGMT_NO = list[i]["PAMS_MGMT_NO"] == null ? "" : list[i]["PAMS_MGMT_NO"].ToString().Trim().Replace("\0", "");
                try
                {
                    sFILE_BREAK_ID = list[i]["FILE_BREAK_ID"] == null ? "" : list[i]["FILE_BREAK_ID"].ToString().Trim().Replace("\0", "").Replace("00", "");
                }
                catch (Exception e)
                {
                    Program.SysLog.Write(this, "INOUTRST FILE_BREAK_ID 파싱 Exception", e);
                    sFILE_BREAK_ID = "";
                }

                htParamsHashtable.Clear();
                htParamsHashtable.Add("IN_PAMS_MGMT_NO", sPAMS_MGMT_NO);
                htParamsHashtable.Add("IN_FILE_BREAK_ID", sFILE_BREAK_ID);

                ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_OUTIN_STTS_DELETE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                sINOUT_FLAG = request["INOUT_FLAG"] == null ? "" : request["INOUT_FLAG"].ToString().Trim().Replace("\0", "");
                sTAG_ID = list[i]["TAG_ID"] == null ? "" : list[i]["TAG_ID"].ToString().Trim().Replace("\0", "");

                if (sINOUT_FLAG == "OUT" || sINOUT_FLAG == "RIN" || sINOUT_FLAG == "ARR")
                {
                    sEXTENDDT = list[i]["IN_REG_DT"] == null ? "" : list[i]["IN_REG_DT"].ToString().Trim().Replace("\0", "");


                    if (sINOUT_FLAG == "OUT")
                    {
                        sEXTDT = DateTime.Now.ToString("yyyyMMddHHmmss");
                    }
                    else
                    {
                        try
                        {
                            sEXTDT = list[i]["EXTDT"] == null ? "" : list[i]["EXTDT"].ToString().Trim().Replace("\0", "");
                        }
                        catch (Exception ex)
                        {
                            sEXTDT = "";
                        }
                    }

                    htParamsHashtable.Clear();
                    htParamsHashtable.Add("IN_PAMS_MGMT_NO", sPAMS_MGMT_NO);
                    htParamsHashtable.Add("IN_TAG_ID", sTAG_ID);
                    htParamsHashtable.Add("IN_INOUT_FLAG", sINOUT_FLAG);
                    htParamsHashtable.Add("IN_EXTDT", sEXTDT);

                    if (sINOUT_FLAG == "OUT")
                    {
                        if (sEXTENDDT.Trim().Length == 0)
                        {
                            sEXTENDDT = DateTime.Now.ToString("yyyyMMdd");
                        }
                    }

                    htParamsHashtable.Add("IN_EXTENDDT", sEXTENDDT);

                    htParamsHashtable.Add("IN_FILE_BREAK_ID", sFILE_BREAK_ID);

                    ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_OUTIN_STTS_INSERT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                    strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                    strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                    htParamsHashtable.Clear();
                    htParamsHashtable.Add("IN_LOG_DTTM", DateTime.Now.ToString("yyyyMMdd HHmmss.fff"));
                    htParamsHashtable.Add("IN_MGMT_NO", sPAMS_MGMT_NO);
                    htParamsHashtable.Add("IN_TAG_ID", sTAG_ID);
                    htParamsHashtable.Add("IN_GATE_ID", "");
                    htParamsHashtable.Add("IN_PROC_CODE", "IO_STT");
                    htParamsHashtable.Add("IN_PROC_VAL", sINOUT_FLAG + "/" + DateTime.Now.ToString("yyyyMMdd") + "/" + sEXTENDDT);
                    htParamsHashtable.Add("IN_INOUT_FLAG", " ");
                    htParamsHashtable.Add("IN_SDATE", " ");
                    htParamsHashtable.Add("IN_EDATE", " ");

                    htParamsHashtable.Add("IN_FILE_BREAK_ID", sFILE_BREAK_ID);

                    ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_EVENT_PROC_HIS_INSERT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                    strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                    strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();
                }
                else
                {
                    htParamsHashtable.Clear();
                    htParamsHashtable.Add("IN_LOG_DTTM", DateTime.Now.ToString("yyyyMMdd HHmmss.fff"));
                    htParamsHashtable.Add("IN_MGMT_NO", sPAMS_MGMT_NO);
                    htParamsHashtable.Add("IN_TAG_ID", sTAG_ID);
                    htParamsHashtable.Add("IN_GATE_ID", "");
                    htParamsHashtable.Add("IN_PROC_CODE", "IO_STT");
                    htParamsHashtable.Add("IN_PROC_VAL", sINOUT_FLAG);
                    htParamsHashtable.Add("IN_INOUT_FLAG", " ");
                    htParamsHashtable.Add("IN_SDATE", " ");
                    htParamsHashtable.Add("IN_EDATE", " ");

                    htParamsHashtable.Add("IN_FILE_BREAK_ID", sFILE_BREAK_ID);

                    ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_EVENT_PROC_HIS_INSERT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                    strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                    strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();
                }
            }



            Dictionary<string, object> aData = new Dictionary<string, object>();
            aData.Add("WORKDIV", sWORKDIV + "REP");
            aData.Add("RESULT", "0");

            JObject json = new JObject();

            if (aData != null)
            {
                foreach (string strKey in aData.Keys)
                {
                    JProperty property = new JProperty(strKey, aData[strKey]);

                    json.Add(property);
                }
            }

            string sRtn = json.ToString();

            SendResponse_New(sRtn, sWORKDIV + "REQ");

        }

        /// <summary>
        /// 반입/반출 처리 후 RFID에 결과 전송
        /// </summary>
        /// <param name="request"></param>
        public void INREGDT(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 반입예정일 변경
	         * 		서브업무명 : 반입예정일 변경
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		  - TAG_ID : 태그
	         * 		  - PAMS_MGMT_NO : 관리번호
	         * 		  - IN_REG_DT :반입예정일
	         *  ===============================================
             */

            string sWORKDIV = "INREGDT";

            Hashtable htParamsHashtable;

            htParamsHashtable = new Hashtable();
            htParamsHashtable.Clear();

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sPAMS_MGMT_NO = string.Empty;
            string sTAG_ID = string.Empty;
            string sIN_REG_DT = string.Empty;
            string sFILE_BREAK_ID = string.Empty;

            JArray list = ((JArray)(request["LIST"]));

            for (int i = 0; i < list.Count; i++)
            {
                sPAMS_MGMT_NO = list[i]["PAMS_MGMT_NO"] == null ? "" : list[i]["PAMS_MGMT_NO"].ToString().Trim().Replace("\0", "");
                sTAG_ID = list[i]["TAG_ID"] == null ? "" : list[i]["TAG_ID"].ToString().Trim().Replace("\0", "");
                sIN_REG_DT = list[i]["IN_REG_DT"] == null ? "" : list[i]["IN_REG_DT"].ToString().Trim().Replace("\0", "");

                try
                {
                    sFILE_BREAK_ID = list[i]["FILE_BREAK_ID"] == null ? "" : list[i]["FILE_BREAK_ID"].ToString().Trim().Replace("\0", "").Replace("00", "");
                }
                catch (Exception e)
                {
                    Program.SysLog.Write(this, "INREGDT FILE_BREAK_ID 파싱 Exception", e);
                    sFILE_BREAK_ID = "";
                }

                htParamsHashtable.Clear();
                htParamsHashtable.Add("IN_PAMS_MGMT_NO", sPAMS_MGMT_NO);
                htParamsHashtable.Add("IN_TAG_ID", sTAG_ID);
                htParamsHashtable.Add("IN_REG_DT", sIN_REG_DT);
                htParamsHashtable.Add("IN_FILE_BREAK_ID", sFILE_BREAK_ID);

                ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_OUTIN_STTS_UPDATE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();
            }


            Dictionary<string, object> aData = new Dictionary<string, object>();
            aData.Add("WORKDIV", sWORKDIV + "REP");
            aData.Add("RESULT", "0");

            JObject json = new JObject();

            if (aData != null)
            {
                foreach (string strKey in aData.Keys)
                {
                    JProperty property = new JProperty(strKey, aData[strKey]);

                    json.Add(property);
                }
            }

            string sRtn = json.ToString();

            SendResponse_New(sRtn, sWORKDIV + "REQ");

        }

        public void NPMNTRTMRCV(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 무단반출입
	         * 		서브업무명 : 미승인 무단 반출/입 처리 결과 전송
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         *  ===============================================
             */

            string sWORKDIV = "NPMNTRTMRCV";

            Hashtable htParamsHashtable;

            htParamsHashtable = new Hashtable();
            htParamsHashtable.Clear();

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sNPERMN_OUT_APPL_ID = string.Empty;
            string sPAMS_MGMT_NO = string.Empty;
            string sTAG_ID = string.Empty;
            string sEXTDT = string.Empty;
            string sEXTENDDT = string.Empty;
            string sBIND_NO = string.Empty;
            string sINOUT_FLAG = string.Empty;

            string sFILE_BREAK_ID = string.Empty;

            bool bRtn = true;

            //List<Dictionary<string, object>> list = ((List<Dictionary<string, object>>)(request["LIST"]));
            JArray list = ((JArray)(request["LIST"]));

            if (list != null)
            {
                //List<Dictionary<string, object>> tmplist = new List<Dictionary<string, object>>();



                for (int iRow = 0; iRow < 5; iRow++)
                {
                    JArray tmplist = new JArray(list);

                    for (int i = 0; i < tmplist.Count; i++)
                    {
                        Dictionary<string, object> NewParam = tmplist[i].ToObject<Dictionary<string, object>>();


                        sPAMS_MGMT_NO = NewParam["PAMS_MGMT_NO"] == null ? "" : NewParam["PAMS_MGMT_NO"].ToString().Trim().Replace("\0", "");

                        if (NewParam.ContainsKey("TAG_ID"))
                        {
                            sTAG_ID = NewParam["TAG_ID"] == null ? "" : NewParam["TAG_ID"].ToString().Trim().Replace("\0", "");
                        }
                        else
                        {
                            sTAG_ID = " ";
                        }

                        if (NewParam.ContainsKey("FILE_BREAK_ID"))
                        {
                            sFILE_BREAK_ID = NewParam["FILE_BREAK_ID"] == null ? "" : NewParam["FILE_BREAK_ID"].ToString().Trim().Replace("\0", "").Replace("00", "");
                        }
                        else
                        {
                            sFILE_BREAK_ID = "";
                        }

                        if (NewParam.ContainsKey("MGMT_NO")) NewParam["MGMT_NO"] = NewParam["PAMS_MGMT_NO"];
                        else NewParam.Add("MGMT_NO", NewParam["PAMS_MGMT_NO"]);

                        if (NewParam.ContainsKey("WORKDIV")) NewParam["WORKDIV"] = "RL_DOC";
                        else NewParam.Add("WORKDIV", "RL_DOC");

                        if (!NewParam.ContainsKey("BIND_NO"))
                        {
                            NewParam.Add("BIND_NO", "");
                        }

                        if (!NewParam.ContainsKey("FILE_BREAK_ID"))
                        {
                            NewParam.Add("FILE_BREAK_ID", "");
                        }

                        JObject ctrlResult = CtrlClient.connect(NewParam);
                        Dictionary<string, object> ctrlDic = JObject.FromObject(ctrlResult).ToObject<Dictionary<string, object>>();

                        string sCtrlResult = ctrlDic["RESULT"] == null ? "" : ctrlDic["RESULT"].ToString().Trim().Replace("\0", "");

                        //if (NewParam.ContainsKey("WORKDIV")) NewParam["WORKDIV"] = "1500REQ";
                        //else NewParam.Add("WORKDIV", "1500REQ");

                        //Dictionary<string, object> MwDic = MwClient.connect(NewParam);

                        //string sMwResult = MwDic["RESULT"] == null ? "" : MwDic["RESULT"].ToString().Trim().Replace("\0", "");

                        //if (sCtrlResult == "OK")//if (sCtrlResult == "OK" && sMwResult == "OK")
                        //{
                        //    //성공하면 리스트 제거
                        //    list.RemoveAt(i);
                        //}

                        if (sCtrlResult == "OK")//if (sCtrlResult == "OK" && sMwResult == "OK")
                        {
                            for (int ii = 0; ii < list.Count; ii++)
                            {
                                string sPAMS_MGMT_NO_CHK = list[ii]["PAMS_MGMT_NO"] == null ? "" : list[ii]["PAMS_MGMT_NO"].ToString();
                                string sFILE_BREAK_ID_CHK = "";
                                try
                                {
                                    sFILE_BREAK_ID_CHK = list[ii]["FILE_BREAK_ID"] == null ? "" : list[ii]["FILE_BREAK_ID"].ToString().Trim().Replace("\0", "").Replace("00", "");
                                }
                                catch (Exception e)
                                {
                                    Program.SysLog.Write(this, "NPMNTRTMRCV FILE_BREAK_ID 파싱 Exception", e);
                                    sFILE_BREAK_ID_CHK = "";
                                }

                                if (sPAMS_MGMT_NO == sPAMS_MGMT_NO_CHK && sFILE_BREAK_ID == sFILE_BREAK_ID_CHK)
                                {
                                    //성공하면 리스트 제거
                                    list.RemoveAt(ii);
                                    break;
                                }

                            }

                        }



                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_LOG_DTTM", DateTime.Now.ToString("yyyyMMdd HHmmss.fff"));
                        htParamsHashtable.Add("IN_MGMT_NO", sPAMS_MGMT_NO);
                        htParamsHashtable.Add("IN_TAG_ID", sTAG_ID);
                        htParamsHashtable.Add("IN_GATE_ID", " ");
                        htParamsHashtable.Add("IN_PROC_CODE", "W_CLR");
                        htParamsHashtable.Add("IN_PROC_VAL", "경보해제");
                        htParamsHashtable.Add("IN_INOUT_FLAG", " ");
                        htParamsHashtable.Add("IN_SDATE", " ");
                        htParamsHashtable.Add("IN_EDATE", " ");
                        htParamsHashtable.Add("IN_FILE_BREAK_ID", sFILE_BREAK_ID);

                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_EVENT_PROC_HIS_INSERT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();


                    }
                }


            }



            Dictionary<string, object> aData = new Dictionary<string, object>();
            aData.Add("WORKDIV", sWORKDIV + "REP");
            if (list.Count == 0) aData.Add("RESULT", "0");
            else aData.Add("RESULT", "0");

            JObject json = new JObject();

            if (aData != null)
            {
                foreach (string strKey in aData.Keys)
                {
                    JProperty property = new JProperty(strKey, aData[strKey]);

                    json.Add(property);
                }
            }

            string sRtn = json.ToString();

            SendResponse_New(sRtn, sWORKDIV + "REQ");

        }


        public void READERSTATE(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 고정형리더기 상태
	         * 		서브업무명 : 고정형리더기 상태 조회 후 PAMS전송
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         *  	EQ_TYPE		: GAT
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		RESULT_CD : 결과코드
	         * 		LIST
	         *   	- EQ_NM : 장비 명칭
	         *   	- NETMASK
	         *   	- GATEWAY
	         *   	- NOTFY_TYPE_NM : '"AC" : AC COMMNAD, "SC" : NET NOTFY';
	         *   	- NOTFY_TYPE : '"AC" : AC COMMNAD, "SC" : NET NOTFY';
	         *   	- EQ_TYPE : "GAT" : 고정형, "PDA" : PDA, "CNT" : 정수점검기, "PRT" : 태그발행기';
	         *   	- UPD_DTTM": "수정일시",
	         *   	- GATE_ID": "GATE ID ",
	         *   	- IP_ADDR": "192.168.200.138",
	         *   	- ANT_CNT": "4",
	         *   	- FW_NM": "펌웨어 v0.2",
	         *   	- STATUS": "NOR",
	         *   	- ATTENU": "3",
	         *   	- EQ_ID": "GTB1R010",
	         *   	- MAC_ADDR": "00:14:00:02:05:E4",
	         *   	- ANT_SEQ": "1234",
	         *   	- NOTFY_STT": "ATV",
	         *   	- GATE_ID_NM": "지하1층G01",
	         *   	- STATUS_NM": "정상",
	         *   	- UPD_USER_ID": "DAEMON"
	         *  ===============================================
             */

            string sWORKDIV = "READERSTATE";

            Hashtable htParamsHashtable;
            DataSet dsResult;

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sEQ_TYPE = string.Empty; //고정형 : "GAT", 휴대형 리더기 : "PDA", 정수점검기 : "CNT", 태그발행기 : "PRT"

            try
            {
                lock (oClientSocketCO)
                {

                    sEQ_TYPE = request["EQ_TYPE"] == null ? "" : request["EQ_TYPE"].ToString().Trim().Replace("\0", "");

                    htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();
                    htParamsHashtable.Add("IN_EQ_TYPE", sEQ_TYPE);

                    dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C119REQ_SELECT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

                    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                    {
                        var list = new List<JObject>();

                        foreach (DataRow row in dsResult.Tables[0].Rows)
                        {
                            var item = new JObject();

                            foreach (DataColumn column in dsResult.Tables[0].Columns)
                            {
                                item.Add(column.ColumnName, JToken.FromObject(row[column.ColumnName]));
                            }

                            list.Add(item);
                        }

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", sWORKDIV + "REP");
                        aData.Add("count", list.Count);
                        aData.Add("LIST", list);
                        aData.Add("RESULT", "0");


                        JObject json = new JObject();

                        if (aData != null)
                        {
                            foreach (string strKey in aData.Keys)
                            {
                                JProperty property = new JProperty(strKey, aData[strKey]);

                                json.Add(property);
                            }
                        }

                        string sRtn = json.ToString();

                        SendResponse_New(sRtn, sWORKDIV + "REQ");
                    }
                    else
                    {
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("장비 관리 조회 안됨.(" + request.ToString() + ")");
                        Program.AppLog.Write("장비 관리 조회 안됨.(" + request.ToString() + ")");

                        var list = new List<JObject>();
                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", sWORKDIV + "REP");
                        aData.Add("count", list.Count);
                        aData.Add("LIST", list);
                        aData.Add("RESULT", "0");

                        JObject json = new JObject();

                        if (aData != null)
                        {
                            foreach (string strKey in aData.Keys)
                            {
                                JProperty property = new JProperty(strKey, aData[strKey]);

                                json.Add(property);
                            }
                        }

                        string sRtn = json.ToString();

                        SendResponse_New(sRtn, sWORKDIV + "REQ");
                    }

                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "(READERSTATE)고정형리더기 상태 조회 후 PAMS전송 오류" + ", RcvData:" + request.ToString(), e);
            }

        }


        public void RETAGID(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 태그발행
	         * 		서브업무명 : 태그재발행
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		LIST
	         * 		  - EQ_ID : 장비 ID
	         * 		  - PRT_EQ_ID : 장비(발행기) ID
	         * 		  - TAG_ID : 태그ID
	         * 		  - PAMS_MGMT_NO : 관리번호
	         * 		  - PRSDT_NM : 대통령
	         * 		  - PROD_ORG_NM : 생산기관
	         * 		  - PROD_STRT_YY : 생산년도
	         * 		  - PRSRV_PRD : 보존기간
	         *  ===============================================
             */

            string sWORKDIV = "RETAGID";

            request.Add("ISSUE_TYPE", "REI");
            request.Add("PRT_EQ_ID", request["EQ_ID"]);

            if (request.ContainsKey("EQ_ID"))
            {
                request["EQ_ID"] = "PAMS0000";
            }
            else
            {
                request.Add("EQ_ID", "PAMS0000");
            }

            //request.Add("ISSUE_RESULT", "RDY");
            request.Add("ISSUE_RESULT", "SAV");

            insertRequest(request);

            Dictionary<string, object> aData = new Dictionary<string, object>();
            aData.Add("WORKDIV", sWORKDIV + "REP");
            aData.Add("RESULT", "0");

            JObject json = new JObject();

            if (aData != null)
            {
                foreach (string strKey in aData.Keys)
                {
                    JProperty property = new JProperty(strKey, aData[strKey]);

                    json.Add(property);
                }
            }

            string sRtn = json.ToString();

            SendResponse_New(sRtn, sWORKDIV + "REQ");
        }

        public void TAGID(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 태그발행
	         * 		서브업무명 : 태그발행
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		LIST
	         * 		  - EQ_ID : 장비 ID
	         * 		  - PRT_EQ_ID : 장비(발행기) ID
	         * 		  - TAG_ID : 태그ID
	         * 		  - PAMS_MGMT_NO : 관리번호
	         * 		  - PRSDT_NM : 대통령
	         * 		  - PROD_ORG_NM : 생산기관
	         * 		  - PROD_STRT_YY : 생산년도
	         * 		  - PRSRV_PRD : 보존기간
	         *  ===============================================
             */

            string sWORKDIV = "TAGID";

            request.Add("ISSUE_TYPE", "ISS");
            request.Add("PRT_EQ_ID", request["EQ_ID"]);

            if (request.ContainsKey("EQ_ID"))
            {
                request["EQ_ID"] = "PAMS0000";
            }
            else
            {
                request.Add("EQ_ID", "PAMS0000");
            }

            //request.Add("ISSUE_RESULT", "RDY");
            request.Add("ISSUE_RESULT", "SAV");


            insertRequest(request);

            Dictionary<string, object> aData = new Dictionary<string, object>();
            aData.Add("WORKDIV", sWORKDIV + "REP");
            aData.Add("RESULT", "0");

            JObject json = new JObject();

            if (aData != null)
            {
                foreach (string strKey in aData.Keys)
                {
                    JProperty property = new JProperty(strKey, aData[strKey]);

                    json.Add(property);
                }
            }

            string sRtn = json.ToString();

            SendResponse_New(sRtn, sWORKDIV + "REQ");
        }

        public void insertRequest(JObject request)
        {

            if (!request.ContainsKey("LIST") || request["LIST"] == null)
            {
                JArray nolist = new JArray();
                nolist.Add(request);

                if (request.ContainsKey("LIST")) request["LIST"] = nolist;
                else request.Add("LIST", nolist);
            }

            JArray list = (JArray)(request["LIST"]);



            for (int i = 0; i < list.Count; i++)
            {
                string sTAG_ID = list[i]["TAG_ID"] == null ? "" : list[i]["TAG_ID"].ToString().Trim().Replace("\0", "");
                string sEQ_ID = request["EQ_ID"] == null ? "" : request["EQ_ID"].ToString().Trim().Replace("\0", "");
                string sPRT_EQ_ID = request["PRT_EQ_ID"] == null ? "" : request["PRT_EQ_ID"].ToString().Trim().Replace("\0", "");
                string sISSUE_RESULT = request["ISSUE_RESULT"] == null ? "" : request["ISSUE_RESULT"].ToString().Trim().Replace("\0", "");
                string sISSUE_TYPE = request["ISSUE_TYPE"] == null ? "" : request["ISSUE_TYPE"].ToString().Trim().Replace("\0", "");

                Hashtable htParamsHashtable = new Hashtable();
                htParamsHashtable.Clear();
                htParamsHashtable.Add("IN_PRT_EQ_ID", sPRT_EQ_ID);
                htParamsHashtable.Add("IN_TAG_ID", sTAG_ID);

                DataSet dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.TRFID_ISSUE_REQ_CHECK", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                {

                }
                else
                {


                    string sPAMS_MGMT_NO = list[i]["PAMS_MGMT_NO"] == null ? "" : list[i]["PAMS_MGMT_NO"].ToString().Trim().Replace("\0", "");
                    string sPRSDT_NM = list[i]["PRSDT_NM"] == null ? "" : list[i]["PRSDT_NM"].ToString().Trim().Replace("\0", "");
                    string sPROD_ORG_NM = list[i]["PROD_ORG_NM"] == null ? "" : list[i]["PROD_ORG_NM"].ToString().Trim().Replace("\0", "");
                    string sPROD_STRT_YY = list[i]["PROD_STRT_YY"] == null ? "" : list[i]["PROD_STRT_YY"].ToString().Trim().Replace("\0", "");
                    string sPRSRV_PRD = list[i]["PRSRV_PRD"] == null ? "" : list[i]["PRSRV_PRD"].ToString().Trim().Replace("\0", "");
                    string sDOC_TTL = list[i]["DOC_TTL"] == null ? "" : list[i]["DOC_TTL"].ToString().Trim().Replace("\0", "");
                    string sMED_TYPE = list[i]["MED_TYPE"] == null ? "" : list[i]["MED_TYPE"].ToString().Trim().Replace("\0", "");
                    string sLOC = list[i]["LOC"] == null ? "" : list[i]["LOC"].ToString().Trim().Replace("\0", "");
                    string sFILE_BREAK_ID = "";

                    try
                    {
                        sFILE_BREAK_ID = list[i]["FILE_BREAK_ID"] == null ? "" : list[i]["FILE_BREAK_ID"].ToString().Trim().Replace("\0", "").Replace("00", "");
                    }
                    catch (Exception e)
                    {
                        Program.SysLog.Write(this, "insertRequest FILE_BREAK_ID 파싱 Exception", e);
                        sFILE_BREAK_ID = "";
                    }

                    //FILEBREAK_ID

                    if (sFILE_BREAK_ID.Trim().Length == 0)
                    {

                        try
                        {
                            sFILE_BREAK_ID = list[i]["FILE_BREAK_ID"] == null ? "" : list[i]["FILE_BREAK_ID"].ToString().Trim().Replace("\0", "").Replace("00", "");
                        }
                        catch (Exception e)
                        {
                            Program.SysLog.Write(this, "insertRequest FILE_BREAK_ID 파싱 Exception", e);
                            sFILE_BREAK_ID = "";
                        }
                    }


                    htParamsHashtable.Clear();

                    htParamsHashtable.Add("IN_EQ_ID", sEQ_ID);
                    htParamsHashtable.Add("IN_PRT_EQ_ID", sPRT_EQ_ID);
                    htParamsHashtable.Add("IN_TAG_ID", sTAG_ID);
                    htParamsHashtable.Add("IN_PAMS_MGMT_NO", sPAMS_MGMT_NO);
                    htParamsHashtable.Add("IN_PRSDT_NM", sPRSDT_NM);
                    htParamsHashtable.Add("IN_PROD_ORG_NM", sPROD_ORG_NM);
                    htParamsHashtable.Add("IN_PROD_STRT_YY", sPROD_STRT_YY);
                    htParamsHashtable.Add("IN_PRSRV_PRD", sPRSRV_PRD);
                    htParamsHashtable.Add("IN_ISSUE_TYPE", sISSUE_TYPE);
                    htParamsHashtable.Add("IN_ISSUE_RESULT", sISSUE_RESULT);
                    htParamsHashtable.Add("IN_DOC_TTL", sDOC_TTL);
                    htParamsHashtable.Add("IN_MED_TYPE", sMED_TYPE);
                    htParamsHashtable.Add("IN_LOC", sLOC);
                    htParamsHashtable.Add("IN_FILE_BREAK_ID", sFILE_BREAK_ID);

                    ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_ISSUE_REQ_INSERT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                    string strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                    string strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();
                }
            }

            if (list.Count > 0)
            {
                string sIN_PRT_EQ_ID = request["PRT_EQ_ID"] == null ? "" : request["PRT_EQ_ID"].ToString().Trim().Replace("\0", "");

                Hashtable htParamsHashtable2 = new Hashtable();
                htParamsHashtable2.Add("IN_PRT_EQ_ID", sIN_PRT_EQ_ID);

                ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_ISSUE_REQ_RESULT_UPDATE", htParamsHashtable2, Properties.Settings.Default.DB_Timeout);
                string strAppCode = htParamsHashtable2["O_APP_CODE"].ToString();
                string strAppMsg = htParamsHashtable2["O_APP_MSG"].ToString();
            }
        }

        /// <summary>
        /// 파일 전송
        /// </summary>
        /// <param name="request"></param>
        public void C1130(Dictionary<string, object> request)
        {
            /*
             	 * 	=================== summary ===================
	             * 	 	업무명     : MW
	             * 		서브업무명 : 파일 전송
	             *  ===============================================
	             *  
	             * 	============= request parameters ==============
	             * 		SEQ			: 장치 버젼
	             * 		EQ_TYPE		: 소프트웨어 종료
	             *  ===============================================
	             *  
	             * 	============= response parameters =============
	             *  ===============================================
	 *  
                <select id="file" parameterType="java.util.Map" resultMap="file_result">
		            SELECT X.* FROM (
			            SELECT
				             TO_CHAR(A.SEQ) AS SEQ
				            ,A.SW_NM
				            ,A.SW_FILENAME
				            ,BASE64ENCODE(A.SW_FILE) AS SW_FILE
			            FROM
				            TRFID_SW_INFO A
			            WHERE 1 = 1
			              AND A.DEL_USER_ID IS NULL
			              AND A.DEL_DTTM IS NULL
		                  AND A.EQ_TYPE = #{EQ_TYPE}
			              AND A.SEQ <![CDATA[ > ]]> #{SEQ}
			            ORDER BY A.SEQ DESC
		            ) X
		            WHERE ROWNUM = 1
	            </select>
             */

            string sWORKDIV = "1130";

            Hashtable htParamsHashtable;
            DataSet dsResult;

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sSEQ = string.Empty;
            string sEQ_TYPE = string.Empty;     //고정형 : "GAT", 휴대형 리더기 : "PDA", 정수점검기 : "CNT", 태그발행기 : "PRT"

            try
            {
                lock (oClientSocketCO)
                {

                    sEQ_TYPE = request["EQ_TYPE"] == null ? "" : request["EQ_TYPE"].ToString();
                    sSEQ = request["SEQ"] == null ? "" : request["SEQ"].ToString();

                    htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();
                    htParamsHashtable.Add("IN_EQ_TYPE", sEQ_TYPE);
                    htParamsHashtable.Add("IN_SEQ", sEQ_TYPE);

                    dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.FILE_SELECT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

                    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                    {

                        var list = new List<JObject>();

                        foreach (DataRow row in dsResult.Tables[0].Rows)
                        {
                            var item = new JObject();

                            foreach (DataColumn column in dsResult.Tables[0].Columns)
                            {
                                item.Add(column.ColumnName, JToken.FromObject(row[column.ColumnName]));
                            }

                            list.Add(item);
                        }

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", sWORKDIV + "REP");
                        aData.Add("rows", list);
                        aData.Add("RESULT_CD", "OK");


                        JObject json = new JObject();

                        if (aData != null)
                        {
                            foreach (string strKey in aData.Keys)
                            {
                                JProperty property = new JProperty(strKey, aData[strKey]);

                                json.Add(property);
                            }
                        }

                        string sRtn = json.ToString();

                        SendResponse_New(sRtn, "120REQ");
                    }
                    else
                    {
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("프로그램 관리 조회 안됨.(" + request.ToString() + ")");
                        Program.AppLog.Write("프로그램 관리 조회 안됨.(" + request.ToString() + ")");

                        var list = new List<JObject>();

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", sWORKDIV + "REP");
                        aData.Add("rows", list);
                        aData.Add("RESULT_CD", "OK");

                        JObject json = new JObject();

                        if (aData != null)
                        {
                            foreach (string strKey in aData.Keys)
                            {
                                JProperty property = new JProperty(strKey, aData[strKey]);

                                json.Add(property);
                            }
                        }

                        string sRtn = json.ToString();

                        SendResponse_New(sRtn, sWORKDIV + "REQ");
                    }

                }
            }
            catch (Exception e)
            {
                //Program.SysLog.Write(this, "프로그램 관리 조회 오류" + ", RcvData:" + request.ToString(), e);
                Program.SysLog.Write(this, "파일 전송 조회  오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }

        }

        /// <summary>
        /// 고정형 리더기 상태
        /// </summary>
        /// <param name="request"></param>
        public void C1210(Dictionary<string, object> request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : MW
	         * 		서브업무명 : 고정형 리더기 상태
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		WORKDIV 	: 1210REQ
	         * 		GATE_ID 	: Gate ID
	         * 		READER_ID	: Reader ID
	         * 		STATUS		: 상태
	         * 		 -NOR : 정상상태
	         * 		 -ERR : 연결끊어짐
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		WORKDIV 	: 1210REP
	         * 		RESULT : 결과코드
	         * 		 -OK : 완료
	         * 		 -ER : 오류
	         *  ===============================================
             */

            string sWORKDIV = "1210";

            Hashtable htParamsHashtable;

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sCurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus != null && ((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus.ContainsKey(((MainForm)(Application.OpenForms["MainForm"])).RFID_MW))
            {
                ((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus.Remove(((MainForm)(Application.OpenForms["MainForm"])).RFID_MW);
            }

            ((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus.Add(((MainForm)(Application.OpenForms["MainForm"])).RFID_MW, new ConnectionStatus(((MainForm)(Application.OpenForms["MainForm"])).RFID_MW, "T", sCurrentTime));

            if (request.ContainsKey("WORKDIV")) request["WORKDIV"] = "GATE_STT";
            else request.Add("WORKDIV", "GATE_STT");

            CtrlClient.connect(request);
            ((MainForm)(Application.OpenForms["MainForm"])).SetGateEquipStatus(request);


            string sREADER_ID = request["READER_ID"] == null ? "" : request["READER_ID"].ToString();
            string sSTATUS = request["STATUS"] == null ? "" : request["STATUS"].ToString();
            try
            {
                lock (oClientSocketCO)
                {
                    htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();
                    htParamsHashtable.Add("IN_EQ_ID", sREADER_ID);
                    htParamsHashtable.Add("IN_STATUS", sSTATUS);

                    ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_EQUIP_INFO_UPDATE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                    strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                    strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                    Dictionary<string, object> aData = new Dictionary<string, object>();
                    aData.Add("WORKDIV", sWORKDIV + "REP");
                    aData.Add("RESULT", "OK");
                    aData.Add("command", "no-log");

                    if (strAppCode.Equals("0"))
                    {
                        //aData.Add("RESULT", "0");
                        //aData.Add("ERROR", "");
                    }
                    else
                    {
                        //aData.Add("RESULT", "-1");
                        //aData.Add("ERROR", strAppMsg);

                        Program.AppLog.Write("PAMSDATA.C117REQ_INSERT 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.C117REQ_INSERT 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
                    }

                    JObject json = new JObject();

                    if (aData != null)
                    {
                        foreach (string strKey in aData.Keys)
                        {
                            JProperty property = new JProperty(strKey, aData[strKey]);

                            json.Add(property);
                        }
                    }

                    string sRtn = json.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");

                }
            }
            catch (Exception e)
            {
                //Program.SysLog.Write(this, "C117REQ_INSERT 오류" + ", RcvData:" + request.ToString(), e);
                Program.SysLog.Write(this, "고정형 리더기 상태 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// MW : 태그정보
        /// </summary>
        /// <param name="request"></param>
        public void C1400(Dictionary<string, object> request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : MW
	         * 		서브업무명 : 태그정보
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		IN_OUT_FL 	: OUT......
	         * 		ZONE_ID		: ZONEID
	         * 		MGMT_ID		: 관리번호
	         * 		GATE_ID		: GATEID
	         * 		NOR_ILG_FL	: NOR_ILG_FL
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         *  ===============================================
             */

            string sWORKDIV = "1400";

            if (request.ContainsKey("PAMS_MGMT_NO")) request["PAMS_MGMT_NO"] = request["MGMT_ID"];
            else request.Add("PAMS_MGMT_NO", request["MGMT_ID"]);

            string sNOR_ILG_FL = request["NOR_ILG_FL"] == null ? "" : request["NOR_ILG_FL"].ToString();
            if (sNOR_ILG_FL != "NOR")
            {
                try
                {
                    CtrlClient.connect(request);
                }
                catch (Exception ex)
                {
                    Program.SysLog.Write(this, "MW:태그정보 CtrlClient.connect 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), ex);
                }
            }

            try
            {
                //JObject json2 = JObject.FromObject(PamsClient.connect("com.hs.pams.common.server.bkroom.RfidBindArryInfoMsg", request));
                Dictionary<string, object> PamsRespons = PamsClient.connect("com.hs.pams.common.server.bkroom.RfidBindArryInfoMsg", request);
            }
            catch (Exception ex)
            {
                Program.SysLog.Write(this, "MW:태그정보 PamsClient.connect 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), ex);
            }

            Dictionary<string, object> aData = new Dictionary<string, object>();
            aData.Add("WORKDIV", sWORKDIV + "REP");
            aData.Add("RESULT_CD", "OK");

            JObject json = new JObject();

            if (aData != null)
            {
                foreach (string strKey in aData.Keys)
                {
                    JProperty property = new JProperty(strKey, aData[strKey]);

                    json.Add(property);
                }
            }

            string sRtn = json.ToString();

            SendResponse_New(sRtn, sWORKDIV + "REQ");

        }

        public static void InsertLog(Dictionary<string, object> data)
        {
            /*
				LogData.Add("workdiv", request["WORKDIV"]);
				LogData.Add("starttime", dteStart.ToString("yyyyMMddhhmmssfff"));
				LogData.Add("endtime", dteEnd.ToString("yyyyMMddhhmmssfff"));
				LogData.Add("sResultValue", sResultValue);
				LogData.Add("sResultMsg", sResultMsg);
			 */

            if (Properties.Settings.Default.IS_LOGGING_CONNT_HIS == false) return;

            Hashtable htParamsHashtable;

            string sWorkdiv = data["workdiv"] == null ? "" : data["workdiv"].ToString();
            string sStarttime = data["starttime"] == null ? "" : data["starttime"].ToString();
            string sEndtime = data["endtime"] == null ? "" : data["endtime"].ToString();
            string sResultValue = data["ResultValue"] == null ? "" : data["ResultValue"].ToString();
            string sResultMsg = data["ResultMsg"] == null ? "" : data["ResultMsg"].ToString();


            htParamsHashtable = new Hashtable();
            htParamsHashtable.Clear();
            htParamsHashtable.Add("IN_WORKDIV", sWorkdiv);
            htParamsHashtable.Add("IN_STARTTIME", sStarttime);
            htParamsHashtable.Add("IN_ENDTIME", sEndtime);
            htParamsHashtable.Add("IN_PROC_RST", sResultValue);
            htParamsHashtable.Add("IN_ERR_CD", sResultMsg);
            htParamsHashtable.Add("IN_CONN_TYPE", "FMW");

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_CONNT_HIS", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
            strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
            strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

            if (strAppCode.Equals("0"))
            {

            }
            else
            {
                Program.AppLog.Write("PAMSDATA.TRFID_CONNT_HIS 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + sWorkdiv + sStarttime + sEndtime + sResultValue + sResultMsg);
                ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.TRFID_CONNT_HIS 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
            }
        }

        public void SendResponse(string strRecvData, string strErrorCode, string strErrorMessage)
        {
            string strSendBuffer;
            byte[] bPacketBuffer;
            int nSendByte;
            try
            {
                strSendBuffer = strRecvData;
                bPacketBuffer = MakePacket(strSendBuffer);
                nSendByte = clientSocket.Send(bPacketBuffer);
                Program.AppLog.Write("Send : [" + EdASCII.GetString(bPacketBuffer) + "]");

            }
            catch (SocketException se)
            {
                Program.SysLog.Write(this, "SendResponse 함수 오류", se);
            }
            catch (ObjectDisposedException ode)
            {
                Program.SysLog.Write(this, "SendResponse 함수 오류", ode);
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "SendResponse 함수 오류", e);
            }
        }

        public void SendResponse_New(string strRecvData, string sLogAddMsg)
        {
            string strSendBuffer;
            byte[] bPacketBuffer;
            int nSendByte;
            try
            {
                strSendBuffer = strRecvData;
                bPacketBuffer = getBytes(strSendBuffer);
                nSendByte = clientSocket.Send(bPacketBuffer);
                Program.AppLog.Write("Send : [" + EdASCII.GetString(bPacketBuffer) + "(" + sLogAddMsg + ")" + "]");
                clientSocket.Close();
                bComplete = true;
            }
            catch (SocketException se)
            {
                Program.SysLog.Write(this, "SendResponse 함수 오류", se);
                bComplete = true;
            }
            catch (ObjectDisposedException ode)
            {
                Program.SysLog.Write(this, "SendResponse 함수 오류", ode);
                bComplete = true;
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "SendResponse 함수 오류", e);
                bComplete = true;
            }
        }

        public byte[] getBytes(string sData)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(sData);

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public byte[] MakePacket(string strSendBuffer)
        {
            byte[] sendBuffer;
            byte[] tempBuffer;
            ushort nCRCValue;
            //string strBuffer;
            string strCRC;

            try
            {
                //sendBuffer = new byte[EUC_KR.GetBytes(strSendBuffer).Length + 2 + 4];
                sendBuffer = new byte[EdASCII.GetBytes(strSendBuffer).Length + 2];
                sendBuffer[0] = STX;

                tempBuffer = EdASCII.GetBytes(strSendBuffer);
                System.Buffer.BlockCopy(tempBuffer, 0, sendBuffer, 1, tempBuffer.Length);
                sendBuffer[tempBuffer.Length + 1] = ETX;
                //nCRCValue = Program.crc16.ComputeChecksum(tempBuffer);
                //strCRC = string.Format("{0:X4}", nCRCValue);
                //System.Buffer.BlockCopy(Encoding.ASCII.GetBytes(strCRC), 0, sendBuffer, tempBuffer.Length + 2, 4);
                return sendBuffer;
                //            reqCRC16TextBox.Text = strCRC;
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "Unexpected exception!", e);
            }

            return null;
        }

        public void Dispose()
        {
            if (!bDisposed)
            {
                //                AddMiddlewareConnectHistory(sMWID, false);
                clientSocket.Close();
                bDisposed = true;
            }
        }

        public void AddMiddlewareConnectHistory(string pstrMiddlewareID, bool bConnect)
        {
            Hashtable htParamsHashtable;
            int nProcessRowCount;
            string strErrorDT;
            string strErrorTM;
            EQErrorState esErrorState;

            if (string.IsNullOrEmpty(pstrMiddlewareID))
            {
                return;
            }
            try
            {
                lock (oClientSocketCO)
                {
                    htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();
                    strErrorDT = DateTime.Now.ToString("yyyyMMdd");
                    strErrorTM = DateTime.Now.ToString("HHmmssfff");
                    htParamsHashtable.Add("IN_CAR_NO", pstrMiddlewareID);
                    htParamsHashtable.Add("IN_LAST_COMM_DTM", strErrorDT + strErrorTM);
                    htParamsHashtable.Add("IN_LAST_COMM_STATE", (bConnect ? "Y" : "N"));

                    nProcessRowCount = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PKG_HOCCOM_EXP_200717.COMM_STATE_SAVE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                    if (htParamsHashtable.ContainsKey("O_APP_CODE"))
                    {
                        if (!htParamsHashtable["O_APP_CODE"].ToString().Equals("0"))
                        {
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("연결상태 DB 처리 오류[EQ ID:" + pstrMiddlewareID + ",NORMAL_YN:" + (bConnect ? "Y" : "N") + ",APP_CODE:" + htParamsHashtable["O_APP_CODE"].ToString() + ",APP_MSG:" + htParamsHashtable["O_APP_MSG"].ToString() + "]");
                            Program.AppLog.Write("연결상태 DB 처리 오류[EQ ID:" + pstrMiddlewareID + ",NORMAL_YN:" + (bConnect ? "Y" : "N") + ",APP_CODE:" + htParamsHashtable["O_APP_CODE"].ToString() + ",APP_MSG:" + htParamsHashtable["O_APP_MSG"].ToString() + "]");
                        }
                    }
                    else
                    {
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("연결상태 DB 처리 오류[EQ ID:" + pstrMiddlewareID + ",NORMAL_YN:" + (bConnect ? "Y" : "N") + "]");
                        Program.AppLog.Write("연결상태 DB 처리 오류[EQ ID:" + pstrMiddlewareID + ",NORMAL_YN:" + (bConnect ? "Y" : "N") + "]");
                    }

                    if (nProcessRowCount == -1000)
                    {
                        Program.AppLog.Write(this, "DB 연결 오류");
                        //sendResponse(sSSID, sCommandCode, "0020", ErrorCode.DATABASE_CONNECT_ERROR, "");
                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.Reconnect();
                        return;
                    }
                    lock (((MainForm)(Application.OpenForms["MainForm"])).EQErrorStateCO)
                    {
                        esErrorState = ((MainForm)(Application.OpenForms["MainForm"])).GetEQErrorState(pstrMiddlewareID);
                        if (esErrorState == null)
                        {
                            ((MainForm)(Application.OpenForms["MainForm"])).AddEQErrorState(new EQErrorState(pstrMiddlewareID, bConnect ? EnumEQState.Normal : EnumEQState.Disconnected, (bConnect ? "0000" : "0999"), (bConnect ? "연결됨" : "연결끊김"), strErrorDT + strErrorTM));
                        }
                        else
                        {
                            esErrorState.SetEQErrorState(pstrMiddlewareID, bConnect ? EnumEQState.Normal : EnumEQState.Disconnected, (bConnect ? "0000" : "0999"), (bConnect ? "연결됨" : "연결끊김"), strErrorDT + strErrorTM);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "연결 처리 오류", e);
            }

        }

    }
}
