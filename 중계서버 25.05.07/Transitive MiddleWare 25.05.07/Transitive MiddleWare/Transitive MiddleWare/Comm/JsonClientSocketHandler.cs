
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

    class JsonClientSocketHandler : IDisposable
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



        #endregion

        #region Constructor(생성자)
        public JsonClientSocketHandler()
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

        public JsonClientSocketHandler(Socket acceptedSocket)
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

        #region State Check Function(상태 체크 함수)
        //public bool bCheckConnection()
        //{
        //    try
        //    {
        //        if (!clientSocket.Connected && !bDisconnectedDateTimeUpdated)
        //        {
        //            bDisconnectedDateTimeUpdated = true;
        //            disconnectedDateTime = DateTime.Now;
        //            AddMiddlewareConnectHistory(sMWID, false);
        //        }
        //        return clientSocket.Connected;
        //    }
        //    catch (Exception e)
        //    {
        //        Program.SysLog.Write(this, "Socket Connection Check Exception!", e);
        //    }
        //    return false;
        //}
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
        #endregion

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
            JsonClientSocketHandler clientSocketHandler = (JsonClientSocketHandler)ar.AsyncState;
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

        // STX, ETX를 이용한 한 패킷 분리
        public bool ParseBuffer()
        {
            int nSearchOffset;
            int nSearchSize;
            int nSTXPosition = -1;
            int nETXPosition = -1;
            byte[] packetBuffer;
            int nPacketSize;
            bool bPacketParsed = false;

            lock (parsingBufferCO)
            {
                try
                {
                    nSearchOffset = parsingBufferOffset;
                    nSearchSize = parsingBufferSize;
                    while (nSearchSize > 0)
                    {
                        if (parsingBuffer[nSearchOffset] == STX) // STX 이전 데이터는 ETX가 없는 데이터 이므로 비정상 프로토콜로 보고 제거한다.
                        {
                            nSTXPosition = nSearchOffset;
                            parsingBufferOffset = nSearchOffset;
                            parsingBufferSize = nSearchSize;
                        }
                        if (parsingBuffer[nSearchOffset] == ETX)
                        {
                            nETXPosition = nSearchOffset;

                            nPacketSize = nETXPosition - nSTXPosition + 1;
                            nSearchOffset += 1;
                            nSearchSize -= 1;
                            packetBuffer = new byte[nPacketSize];
                            Buffer.BlockCopy(parsingBuffer, nSTXPosition, packetBuffer, 0, nPacketSize);
                            Buffer.BlockCopy(parsingBuffer, nSearchOffset, parsingBuffer, 0, nSearchSize);
                            parsingBufferOffset = 0;
                            parsingBufferSize = nSearchSize;

                            // 하나의 패킷을 다시 파싱
                            ParsePacket(packetBuffer);
                            nSearchOffset = 0;
                            continue;
                            //}
                            //else // 남은 자리수가 4보다 작을 경우 데이터를 한번 더 수신하기 위해 파싱하지 않고 대기
                            //{
                            //    return false;
                            //}
                        }
                        nSearchOffset++;
                        nSearchSize--;
                    }
                }
                catch (Exception e)
                {
                    Program.SysLog.Write(this, "Unexpected Exception!", e);
                }
                return bPacketParsed;
            }
        }

        public void ParsePacketCheck()
        {
            //string strRecvData = EdASCII.GetString(_packetBuffer, 0, _packetBuffer.Length);
            string strRecvData = EdASCII.GetString(_packetBuffer, 0, _packetBuffer.Length);

            //string strRecvData = Convert.ToBase64String(_packetBuffer, 0, _packetBuffer.Length);

            int iEOF = strRecvData.IndexOf("<CMD:EOF>");
            string sTrRecvCmd;

            JObject result = new JObject();
            if (iEOF > 0)
            {
                sTrRecvCmd = strRecvData.Substring(0, iEOF);

                //result = JObject.Parse(sTrRecvCmd);

                Program.AppLog.Write("<CMD:EOF>있음" + strRecvData);
                _nPacketSize = 0;

                ParsePacket_New(sTrRecvCmd);
                //ParsePacket_New(parsingBuffer);
            }
            else
            {
                Program.AppLog.Write("<CMD:EOF>없음" + strRecvData);
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

                strCommandCode = result["WORKDIV"].ToString();

                switch (strCommandCode)
                {
                    //발행 TAG_ID 요청
                    case "000REQ":

                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 발행 TAG_ID 요청");
                        C000REQ(result);

                        break;
                    //C001REQ
                    case "001REQ":

                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 태그발급 대상리스트 요청");
                        C001REQ(result);

                        break;

                    //C002REQ(태그재발행 대상 등록)
                    case "002REQ":

                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 태그재발행 대상 등록");
                        C002REQ(result);

                        break;

                    //C003REQ(PDA 태그 재발행 처리)
                    case "003REQ":

                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : PDA 태그 재발행 처리");
                        C003REQ(result);

                        break;

                    //C004REQ(MW : 시작/중지/재시작)
                    //TYPE : NS:시작, NP:중지, RB: 재시작, beepoff:경보해제
                    //GATE_ID: 게이트 ID 
                    case "004REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : MW(시작/중지/재시작)");
                        C004REQ(result);
                        break;

                    //C005REQ(PDA SW 요청)
                    case "005REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : PDA SW 요청");
                        C005REQ(result);
                        break;

                    //C010REQ(PAMS 상태 체크)
                    case "010REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : PAMS 상태 체크");
                        C010REQ(result);
                        break;

                        

                    case "020REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 연계상태 요청");
                        C020REQ(result);

                        break;

                    //로그인 처리
                    case "100REQ":

                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 로그인");
                        C100REQ(result);

                        break;

                    //서고 목록 요청
                    case "110REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 서고 목록 요청");
                        C110REQ(result);

                        break;

                    //서가 목록 요청
                    case "111REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 서가 목록 요청");
                        C111REQ(result);

                        break;

                    //연 목록요청
                    case "112REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 연 목록요청");
                        C112REQ(result);
                        break;

                    //단 목록요청
                    case "113REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 연단 목록요청");
                        C113REQ(result);
                        break;

                    //박스 목록요청
                    case "114REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 보존박스 목록 요청");
                        C114REQ(result);
                        break;

                    //기록물 상세정보 요청 데이터 조회
                    case "115REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 기록물 상세 요청");
                        C115REQ(result);
                        break;

                    //관리번호 또는 제목으로 기록물 목록 검색
                    case "116REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 관리번호 또는 제목으로 기록물 목록 검색");
                        C116REQ(result);
                        break;

                    //C117REQ
                    case "117REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 존 관리");
                        C117REQ(result);
                        break;

                    //C118REQ
                    case "118REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : GATE정보 관리");
                        C118REQ(result);
                        break;

                    //C119REQ
                    case "119REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 장비정보 관리");
                        C119REQ(result);
                        break;

                    //C120REQ
                    case "120REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 프로그램 관리");
                        C120REQ(result);
                        break;

                    //(사용하는 곳 있는 지 확인 필요 : 현재는 없음)
                    //장비별프로그램관리
                    case "121REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 장비별프로그램 관리");
                        C121REQ(result);
                        break;

                    //서가별 서고 현황 요청(서고현황)
                    case "200REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 서고현황 요청");
                        C200REQ(result);
                        break;

                    //C201REQ
                    case "201REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 서고별현황 요청");
                        C201REQ(result);
                        break;

                    //C202REQ
                    case "202REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 연별현황 요청");
                        C202REQ(result);
                        break;

                    //C203REQ
                    case "203REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 단별 기록물 목록 요청");
                        C203REQ(result);
                        break;

                    //300 서가 기록물 목록
                    //테스트 코드 하드 코딩으로 제외

                    //C400REQ(배치 : 배치의뢰서 요청
                    case "400REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 배치의뢰서 요청");
                        C400REQ(result);
                        break;

                    //C401REQ(배치 : 배치대상물 목록)
                    case "401REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 배치대상물 목록 요청");
                        C401REQ(result);
                        break;

                    //402
                    case "402REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 기록물 배치 요청");
                        C402REQ(result);
                        break;

                    //403
                    case "403REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 기록물 배치");
                        C403REQ(result);
                        break;

                    //500
                    //기록물 재배치 요청(하드코딩으로 제외)

                    //502
                    case "502REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 기록물 배치(재배치요청)");
                        C502REQ(result);
                        break;

                    //C600REQ
                    case "600REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 반출서 목록 요청");
                        C600REQ(result);
                        break;

                    //C601REQ
                    case "601REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 반출 기록물 목록 요청");
                        C601REQ(result);
                        break;

                    //C602REQ
                    case "602REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 반출 처리 요청");
                        C602REQ(result);
                        break;

                    case "650REQ":
                        //C650REQ(무단 반출입 제외대상 등록 요청)
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 무단 반출/입 제외 대상 등록 요청");
                        C650REQ(result);

                        break;

                    //C700REQ
                    case "700REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 반입서 목록 요청 요청");
                        C700REQ(result);
                        break;

                    //C701REQ
                    //반입기록물 목록 요청
                    case "701REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 반입기록물 목록 요청");
                        C701REQ(result);
                        break;

                    //C702REQ
                    //반입 처리 요청
                    case "702REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 반입 처리 요청");
                        C702REQ(result);
                        break;

                    //C800REQ
                    case "800REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 미반입 목록 요청");
                        C800REQ(result);
                        break;

                    //C801REQ
                    //미승인 무단 반출/입 목록 조회
                    case "801REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 미승인 무단 반출/입 목록 조회 요청");
                        C801REQ(result);
                        break;

                    //C802REQ
                    //미승인 무단 반출/입 목록 결과 전송
                    case "802REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 미승인 무단 반출/입 목록 결과 전송 요청");
                        C802REQ(result);
                        break;


                    //C900
                    //정수점검 계획서 목록 조회
                    case "900":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 정수점검 계획서 목록 조회 요청");
                        C900(result);
                        break;

                    //C901REQ
                    //정수점검 계획서 기록물 목록 조회
                    case "901REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 정수점검 계획서 기록물 목록 조회 요청");
                        C901REQ(result);
                        break;

                    //C902REQ
                    //정수점검 결과 처리
                    case "902REQ":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : 정수점검 결과 처리 요청");
                        C902REQ(result);
                        break;

                    //C999REQ
                    //공통 : PING
                    case "999":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : PING 처리 요청" + " : " + sIP);
                        C999REQ(result);
                        break;



                    default:
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList( "IP : " + sIP + " RCV DATA :" + strRecvData);
                        Program.AppLog.Write("Case default : " + strRecvData);
                        break;
                }

            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "프로토콜 파싱 오류", e);
            }
        }

        public string DataTableToJsonWithJsonNet(DataTable dt)
        {
            string sJson = string.Empty;
            sJson = JsonConvert.SerializeObject(dt);
            return sJson;
        }

        /// <summary>
        /// 000REQ(발행 TAG_ID 요청)
        /// </summary>
        /// <param name="joCmd"></param>
        public void C000REQ(JObject request)
        {
            string sWORKDIV = "000";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "RfidTagReExecImp";
                    }
                    else
                    {
                        request.Add("WORKDIV", "RfidTagReExecImp");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.RfidTagReExecImp", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "발행 TAG_ID 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// 001REQ(태그 재발행 목록 요청 : 태그발급 대상리스트 요청)
        /// </summary>
        /// <param name="joCmd"></param>
        public void C001REQ(JObject request)
        {
            string sWORKDIV = "001";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "RfidReTagBindListMsg";
                    }
                    else
                    {
                        request.Add("WORKDIV", "RfidReTagBindListMsg");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.RfidReTagBindListMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "태그발급 대상리스트 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //C002REQ(태그재발행 대상 등록)
        /// </summary>
        /// <param name="joCmd"></param>
        public void C002REQ(JObject request)
        {
            string sWORKDIV = "002";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "RfidAddReTagMsg";
                    }
                    else
                    {
                        request.Add("WORKDIV", "RfidAddReTagMsg");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.RfidAddReTagMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "태그재발행 대상 등록 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //C003REQ(PDA 태그 재발행 처리)
        /// </summary>
        /// <param name="joCmd"></param>
        public void C003REQ(JObject request)
        {
            string sWORKDIV = "003";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {

                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.RfidTagReExecImp", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "PDA 태그 재발행 처리 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        //004REQ(MW : 시작/중지/재시작)
        public void C004REQ(JObject request)
        {
            string sWORKDIV = "004";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "1110REQ";
                    }
                    else
                    {
                        request.Add("WORKDIV", "1110REQ");
                    }

                    response = MwClient.connect(request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "MW : 시작/중지/재시작 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }
        /// <summary>
        /// PDA : SW 요청
        /// (쿼리 확인 필요)
        /// </summary>
        /// <param name="request"></param>
        public void C005REQ(JObject request)
        {
            /*
             <resultMap id="file_result" type="java.util.HashMap">
	         <result property="SEQ" column="SEQ" />   
	         <result property="SW_NM" column="SW_NM" />   
	         <result property="SW_FILENAME" column="SW_FILENAME" />   
	         <result property="SW_FILE" column="SW_FILE" jdbcType="CLOB" javaType="java.lang.String" />
	        </resultMap>
	
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
            JObject test = new JObject();
            

            string sWORKDIV = "005";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sEQ_TYPE = string.Empty;
            string sSEQ = string.Empty;

            sEQ_TYPE = request["EQ_TYPE"] == null ? "" : request["EQ_TYPE"].ToString();
            sSEQ = request["SEQ"] == null ? "" : request["SEQ"].ToString();

            try
            {
                lock (oClientSocketCO)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("SEQ", typeof(string));
                    dt.Columns.Add("SW_NM", typeof(string));
                    dt.Columns.Add("SW_FILENAME", typeof(string));
                    dt.Columns.Add("SW_FILE", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["SEQ"] = "1";
                    dr["SW_NM"] = "1";
                    dr["SW_FILENAME"] = "1";
                    dr["SW_FILE"] = "1";

                    dt.Rows.Add(dr);

                    var list = new List<JObject>();

                    foreach (DataRow row in dt.Rows)
                    {
                        var item = new JObject();

                        foreach (DataColumn column in dt.Columns)
                        {
                            item.Add(column.ColumnName, JToken.FromObject(row[column.ColumnName]));
                        }

                        list.Add(item);
                    }

                    Dictionary<string, object> aData = new Dictionary<string, object>();
                    aData.Add("WORKDIV", sWORKDIV + "REP");
                    aData.Add("RESULT", "0");
                    aData.Add("ERROR", "");
                    aData.Add("rows", list);

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
                Program.SysLog.Write(this, "PDA : SW 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// C010REQ(PAMS 상태 체크)
        /// </summary>
        /// <param name="request"></param>
        public void C010REQ(JObject request)
        {
            string sWORKDIV = "010";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    //if (request.ContainsKey("WORKDIV"))
                    //{
                    //    request["WORKDIV"] = "ALIVECHECK";
                    //}
                    //else
                    //{
                    //    request.Add("WORKDIV", "ALIVECHECK");
                    //}
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.RfidPamsAlivecheckMsg", request);
                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "PAMS 상태 체크 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// 020REQ(연계상태 요청)
        /// </summary>
        /// <param name="request"></param>
        public void C020REQ(JObject request)
        {
            string sWORKDIV = "020";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            try
            {
                lock (oClientSocketCO)
                {
                    //((ConnectionStatus)((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus[((MainForm)(Application.OpenForms["MainForm"])).PAMS]).toString();

                    DataTable dt = new DataTable();
                    dt.Columns.Add("connectionKind", typeof(string));
                    dt.Columns.Add("connectionStatus", typeof(string));
                    dt.Columns.Add("lastConnectionTime", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["connectionKind"] = ((ConnectionStatus)((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus[((MainForm)(Application.OpenForms["MainForm"])).PAMS]).getConnectionKind();
                    dr["connectionStatus"] = ((ConnectionStatus)((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus[((MainForm)(Application.OpenForms["MainForm"])).PAMS]).getConnectionStatus();
                    dr["lastConnectionTime"] = ((ConnectionStatus)((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus[((MainForm)(Application.OpenForms["MainForm"])).PAMS]).getLastConnectionTime();

                    DataRow dr2 = dt.NewRow();
                    dr2["connectionKind"] = ((ConnectionStatus)((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus[((MainForm)(Application.OpenForms["MainForm"])).RFID_MW]).getConnectionKind();
                    dr2["connectionStatus"] = ((ConnectionStatus)((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus[((MainForm)(Application.OpenForms["MainForm"])).RFID_MW]).getConnectionStatus();
                    dr2["lastConnectionTime"] = ((ConnectionStatus)((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus[((MainForm)(Application.OpenForms["MainForm"])).RFID_MW]).getLastConnectionTime();

                    DataRow dr3 = dt.NewRow();
                    dr3["connectionKind"] = ((ConnectionStatus)((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus[((MainForm)(Application.OpenForms["MainForm"])).EMS]).getConnectionKind();
                    dr3["connectionStatus"] = ((ConnectionStatus)((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus[((MainForm)(Application.OpenForms["MainForm"])).EMS]).getConnectionStatus();
                    dr3["lastConnectionTime"] = ((ConnectionStatus)((MainForm)(Application.OpenForms["MainForm"])).htConnectionStatus[((MainForm)(Application.OpenForms["MainForm"])).EMS]).getLastConnectionTime();

                    dt.Rows.Add(dr);
                    dt.Rows.Add(dr2);
                    dt.Rows.Add(dr3);

                    var list = new List<JObject>();

                    foreach (DataRow row in dt.Rows)
                    {
                        var item = new JObject();

                        foreach (DataColumn column in dt.Columns)
                        {
                            item.Add(column.ColumnName, JToken.FromObject(row[column.ColumnName]));
                        }

                        list.Add(item);
                    }

                    Dictionary<string, object> aData = new Dictionary<string, object>();
                    aData.Add("WORKDIV", sWORKDIV + "REP");
                    aData.Add("count", list.Count);
                    aData.Add("CurrentTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    aData.Add("rows", list);

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
                Program.SysLog.Write(this, "연계상태 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// C100REQ(로그인)
        /// </summary>
        /// <param name="request"></param>
        public void C100REQ(JObject request)
        {
            string sWORKDIV = "100";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "LOGIN";
                    }
                    else
                    {
                        request.Add("WORKDIV", "LOGIN");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.LoginCheckMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "로그인 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// 서고 목록 요청
        /// </summary>
        /// <param name="request"></param>
        public void C110REQ(JObject request)
        {
            /*
                HashMap<String, Object> param = new HashMap<String, Object>();
		        param.put("queryid", "TRFID_BK_INFO.bkroomList");
		        List<HashMap<String, Object>> result = SqlMapClient.selectList(param); 
            */

            string sWORKDIV = "110";

            Hashtable htParamsHashtable;
            DataSet dsResult;

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            try
            {
                lock (oClientSocketCO)
                {

                    htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();

                    dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C110REQ", htParamsHashtable, Properties.Settings.Default.DB_Timeout);



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
                        aData.Add("rows", list);
                        aData.Add("RESULT", "0");
                        aData.Add("ERROR", "");

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
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("서고 목록 요청 조회 안됨.(" + request.ToString() + ")");
                        Program.AppLog.Write("서고 목록 요청 조회 안됨.(" + request.ToString() + ")");

                        var list = new List<JObject>();
                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", sWORKDIV + "REP");
                        aData.Add("count", list.Count);
                        aData.Add("rows", list);
                        aData.Add("RESULT", "0");
                        aData.Add("ERROR", "");

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
                Program.SysLog.Write(this, "서고 목록 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// 서가 데이터 조회
        /// </summary>
        /// <param name="request"></param>
        public void C111REQ(JObject request)
        {
            /*
                HashMap<String, Object> param = new HashMap<String, Object>();
		        param.put("queryid", "TRFID_BK_INFO.bkstndList");
		        param.put("bkroom_id", request.get("BKROOM_ID"));
		
		        List<HashMap<String, Object>> result = SqlMapClient.selectList(param);
            */

            string sWORKDIV = "111";

            Hashtable htParamsHashtable;
            DataSet dsResult;

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sBKROOM_ID = request["BKROOM_ID"] == null ? "" : request["BKROOM_ID"].ToString();

            try
            {
                lock (oClientSocketCO)
                {

                    htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();
                    htParamsHashtable.Add("IN_BKROOM_ID", sBKROOM_ID);

                    dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C111REQ", htParamsHashtable, Properties.Settings.Default.DB_Timeout);



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
                        aData.Add("rows", list);
                        aData.Add("RESULT", "0");
                        aData.Add("ERROR", "");

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
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("서가 데이터 조회 안됨.(" + request.ToString() + ")");
                        Program.AppLog.Write("서가 데이터 조회 안됨.(" + request.ToString() + ")");

                        var list = new List<JObject>();
                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", sWORKDIV + "REP");
                        aData.Add("count", list.Count);
                        aData.Add("rows", list);
                        aData.Add("RESULT", "0");
                        aData.Add("ERROR", "");

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
                Program.SysLog.Write(this, "서가 목록 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// 연 목록요청
        /// </summary>
        /// <param name="request"></param>
        public void C112REQ(JObject request)
        {
            /*
                HashMap<String, Object> param = new HashMap<String, Object>();
		        param.put("queryid", "TRFID_BK_INFO.bkshlfColList");
		        param.put("bkroom_id", request.get("BKROOM_ID"));
		        param.put("bkstnd_id", request.get("BKSTND_ID"));
		
		        List<HashMap<String, Object>> result = SqlMapClient.selectList(param);
            */

            string sWORKDIV = "112";

            Hashtable htParamsHashtable;
            DataSet dsResult;

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sBKROOM_ID = request["BKROOM_ID"] == null ? "" : request["BKROOM_ID"].ToString();
            string sBKSTND_ID = request["BKSTND_ID"] == null ? "" : request["BKSTND_ID"].ToString();

            try
            {
                lock (oClientSocketCO)
                {

                    htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();
                    htParamsHashtable.Add("IN_BKROOM_ID", sBKROOM_ID);
                    htParamsHashtable.Add("IN_BKSTND_ID", sBKSTND_ID);

                    dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C112REQ", htParamsHashtable, Properties.Settings.Default.DB_Timeout);



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

                        //var list = new List<JObject>();
                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", sWORKDIV + "REP");
                        aData.Add("count", list.Count);
                        aData.Add("rows", list);
                        aData.Add("RESULT", "0");
                        aData.Add("ERROR", "");
                        

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
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("연 목록 데이터 조회 안됨.(" + request.ToString() + ")");
                        Program.AppLog.Write("연 목록 데이터 조회 안됨.(" + request.ToString() + ")");

                        var list = new List<JObject>();
                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", sWORKDIV + "REP");
                        aData.Add("count", list.Count);
                        aData.Add("rows", list);
                        aData.Add("RESULT", "0");
                        aData.Add("ERROR", "");

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
                Program.SysLog.Write(this, "연 목록 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// 단 목록요청
        /// </summary>
        /// <param name="request"></param>
        public void C113REQ(JObject request)
        {
            /*
                HashMap<String, Object> param = new HashMap<String, Object>();
		        param.put("queryid", "TRFID_BK_INFO.bkshlfColList");
		        param.put("bkroom_id", request.get("BKROOM_ID"));
		        param.put("bkstnd_id", request.get("BKSTND_ID"));
		
		        List<HashMap<String, Object>> result = SqlMapClient.selectList(param);
            */

            string sWORKDIV = "113";

            Hashtable htParamsHashtable;
            DataSet dsResult;

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sBKROOM_ID = request["BKROOM_ID"] == null ? "" : request["BKROOM_ID"].ToString();
            string sBKSTND_ID = request["BKSTND_ID"] == null ? "" : request["BKSTND_ID"].ToString();
            string sCOL_NO = request["COL_NO"] == null ? "" : request["COL_NO"].ToString();

            try
            {
                lock (oClientSocketCO)
                {

                    htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();
                    htParamsHashtable.Add("IN_BKROOM_ID", sBKROOM_ID);
                    htParamsHashtable.Add("IN_BKSTND_ID", sBKSTND_ID);
                    htParamsHashtable.Add("IN_COL_NO", sCOL_NO);

                    dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C113REQ", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

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
                        aData.Add("rows", list);
                        aData.Add("RESULT", "0");
                        aData.Add("ERROR", "");
                        

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
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("단 목록 요청 데이터 조회 안됨.(" + request.ToString() + ")");
                        Program.AppLog.Write("단 목록 요청 데이터 조회 안됨.(" + request.ToString() + ")");

                        var list = new List<JObject>();
                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", sWORKDIV + "REP");
                        aData.Add("count", list.Count);
                        aData.Add("rows", list);
                        aData.Add("RESULT", "0");
                        aData.Add("ERROR", "");

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
                Program.SysLog.Write(this, "단 목록 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// 보존박스 목록 요청
        /// </summary>
        /// <param name="request"></param>
        public void C114REQ(JObject request)
        {
            string sWORKDIV = "114";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.BoxListMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "보존박스 목록 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        //
        /// <summary>
        /// 기록물 상세 요청
        /// </summary>
        /// <param name="request"></param>
        public void C115REQ(JObject request)
        {
            string sWORKDIV = "115";
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.BindViewInfoMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    //JObject.FromObject(dict);
                    if (response["BINDINFO"] != null)
                    {
                        foreach (JProperty x in response["BINDINFO"])
                        { // Where 'obj' and 'obj["otherObject"]' are both JObjects
                            string name = x.Name;
                            JToken value = x.Value;
                            response.Add(name, value);
                        }
                        response.Remove("BINDINFO");
                    }

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "기록물 상세 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// 기록물 상세 요청(관리번호 또는 제목으로 기록물 목록 검색)
        /// </summary>
        /// <param name="request"></param>
        public void C116REQ(JObject request)
        {
            string sWORKDIV = "116";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.BindSrchMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "기록물 상세 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //117REQ(존관리)
        /// </summary>
        /// <param name="request"></param>
        public void C117REQ(JObject request)
        {
            string sWORKDIV = "117";

            Hashtable htParamsHashtable;
            DataSet dsResult;

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sCRUD_TP = request["CRUD_TP"] == null ? "" : request["CRUD_TP"].ToString();
            string sZONE_ID = string.Empty;
            string sZONE_NM = string.Empty;

            if (sCRUD_TP == "R")
            {
                try
                {
                    lock (oClientSocketCO)
                    {

                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();

                        dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C117REQ_SELECT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

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
                            aData.Add("rows", list);
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");
                            

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

                            SendResponse_New(sRtn, "117REQ");
                        }
                        else
                        {
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("존관리 조회 안됨.(" + request.ToString() + ")");
                            Program.AppLog.Write("존관리 조회 안됨.(" + request.ToString() + ")");

                            var list = new List<JObject>();
                            Dictionary<string, object> aData = new Dictionary<string, object>();
                            aData.Add("WORKDIV", sWORKDIV + "REP");
                            aData.Add("count", list.Count);
                            aData.Add("rows", list);
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");

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
                    Program.SysLog.Write(this, "존관리 조회 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
            else if (sCRUD_TP == "C")
            {
                sZONE_ID = request["ZONE_ID"] == null ? "" : request["ZONE_ID"].ToString();
                sZONE_NM = request["ZONE_NM"] == null ? "" : request["ZONE_NM"].ToString();

                try
                {
                    lock (oClientSocketCO)
                    {
                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_ZONE_ID", sZONE_ID);
                        htParamsHashtable.Add("IN_ZONE_NM", sZONE_NM);


                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.C117REQ_INSERT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", sWORKDIV + "REP");

                        if (strAppCode.Equals("0"))
                        {
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");

                            htParamsHashtable = new Hashtable();
                            htParamsHashtable.Clear();

                            dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C117REQ_SELECT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

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

                                Dictionary<string, object> param = new Dictionary<string, object>();
                                param.Add("WORKDIV", "RFIDZONE");
                                param.Add("CRUD_TP", sCRUD_TP);
                                param.Add("ZONE_ID", sZONE_ID);
                                param.Add("ZONE_NM", sZONE_NM);
                                param.Add("LIST", list);

                                JObject json2 = JObject.FromObject(PamsClient.connect("com.hs.pams.common.server.bkroom.RfidReTagBindListMsg", param));

                                string sRtn2 = json2.ToString();

                                SendResponse_New(sRtn2, "117REQ");
                            }
                        }
                        else
                        {
                            aData.Add("RESULT", "-1");
                            aData.Add("ERROR", strAppMsg);

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

                            SendResponse_New(sRtn, "117REQ");

                            Program.AppLog.Write("PAMSDATA.C117REQ_INSERT 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + sZONE_ID + "," +  sZONE_NM);
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.C117REQ_INSERT 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
                        }

                        

                    }
                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "C117REQ_INSERT 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "존관리 INSERT 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
            else if (sCRUD_TP == "U")
            {
                sZONE_ID = request["ZONE_ID"] == null ? "" : request["ZONE_ID"].ToString();
                sZONE_NM = request["ZONE_NM"] == null ? "" : request["ZONE_NM"].ToString();

                try
                {
                    lock (oClientSocketCO)
                    {
                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_ZONE_ID", sZONE_ID);
                        htParamsHashtable.Add("IN_ZONE_NM", sZONE_NM);


                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.C117REQ_UPDATE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", "117REP");

                        if (strAppCode.Equals("0"))
                        {
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");

                            htParamsHashtable = new Hashtable();
                            htParamsHashtable.Clear();

                            dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C117REQ_SELECT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

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

                                Dictionary<string, object> param = new Dictionary<string, object>();
                                param.Add("WORKDIV", "RFIDZONE");
                                param.Add("CRUD_TP", sCRUD_TP);
                                param.Add("ZONE_ID", sZONE_ID);
                                param.Add("ZONE_NM", sZONE_NM);
                                param.Add("LIST", list);

                                JObject json2 = JObject.FromObject(PamsClient.connect("com.hs.pams.common.server.bkroom.RfidReTagBindListMsg", param));

                                string sRtn2 = json2.ToString();

                                SendResponse_New(sRtn2, "117REQ");
                            }
                        }
                        else
                        {
                            aData.Add("RESULT", "-1");
                            aData.Add("ERROR", strAppMsg);

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

                            SendResponse_New(sRtn, "117REQ");

                            Program.AppLog.Write("PAMSDATA.C117REQ_UPDATE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + sZONE_ID + "," + sZONE_NM);
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.C117REQ_UPDATE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
                        }

                        

                    }
                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "C117REQ_UPDATE 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "존관리 UPDATE 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
            else if (sCRUD_TP == "D")
            {
                sZONE_ID = request["ZONE_ID"] == null ? "" : request["ZONE_ID"].ToString();

                try
                {
                    lock (oClientSocketCO)
                    {
                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_ZONE_ID", sZONE_ID);


                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.C117REQ_DELETE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", "117REP");

                        if (strAppCode.Equals("0"))
                        {
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");

                            htParamsHashtable = new Hashtable();
                            htParamsHashtable.Clear();

                            dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C117REQ_SELECT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

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

                                Dictionary<string, object> param = new Dictionary<string, object>();
                                param.Add("WORKDIV", "RFIDZONE");
                                param.Add("CRUD_TP", sCRUD_TP);
                                param.Add("ZONE_ID", sZONE_ID);
                                param.Add("ZONE_NM", sZONE_NM);
                                param.Add("LIST", list);

                                JObject json2 = JObject.FromObject(PamsClient.connect("com.hs.pams.common.server.bkroom.RfidReTagBindListMsg", param));

                                string sRtn2 = json2.ToString();

                                SendResponse_New(sRtn2, "117REQ");
                            }
                        }
                        else
                        {
                            aData.Add("RESULT", "-1");
                            aData.Add("ERROR", strAppMsg);

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

                            SendResponse_New(sRtn, "117REQ");

                            Program.AppLog.Write("PAMSDATA.C117REQ_DELETE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + sZONE_ID + "," + sZONE_NM);
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.C117REQ_DELETE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
                        }

                        

                    }
                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "C117REQ_DELETE 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "존관리 C117REQ_DELETE 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
        }

        /// <summary>
        /// //118REQ(Gate 관리)
        /// </summary>
        /// <param name="request"></param>
        public void C118REQ(JObject request)
        {
            string sWORKDIV = "118";

            Hashtable htParamsHashtable;
            DataSet dsResult;

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sCRUD_TP = request["CRUD_TP"] == null ? "" : request["CRUD_TP"].ToString();
            string sGATE_ID = string.Empty;
            string sGATE_TYPE = string.Empty;
            string sGATE_NM = string.Empty;
            string sUSE_SENSOR = string.Empty;
            string sIN_ZN_ID = string.Empty;
            string sOUT_ZN_ID = string.Empty;

            if (sCRUD_TP == "R")
            {
                try
                {
                    lock (oClientSocketCO)
                    {

                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();

                        dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C118REQ_SELECT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

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
                            aData.Add("WORKDIV", "118REP");
                            aData.Add("count", list.Count);
                            aData.Add("rows", list);
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");


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

                            SendResponse_New(sRtn, "118REQ");
                        }
                        else
                        {
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("GATE 관리 조회 안됨.(" + request.ToString() + ")");
                            Program.AppLog.Write("GATE 관리 조회 안됨.(" + request.ToString() + ")");

                            var list = new List<JObject>();
                            Dictionary<string, object> aData = new Dictionary<string, object>();
                            aData.Add("WORKDIV", "118REP");
                            aData.Add("count", list.Count);
                            aData.Add("rows", list);
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");

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

                            SendResponse_New(sRtn, "118REQ");
                        }

                    }
                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "GATE관리 조회 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "GATE관리 조회 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
            else if (sCRUD_TP == "C")
            {
                sGATE_ID = request["GATE_ID"] == null ? "" : request["GATE_ID"].ToString();
                sGATE_TYPE = request["GATE_TYPE"] == null ? "" : request["GATE_TYPE"].ToString();
                sGATE_NM = request["GATE_NM"] == null ? "" : request["GATE_NM"].ToString();
                sUSE_SENSOR = request["USE_SENSOR"] == null ? "" : request["USE_SENSOR"].ToString();
                sIN_ZN_ID = request["IN_ZN_ID"] == null ? "" : request["IN_ZN_ID"].ToString();
                sOUT_ZN_ID = request["OUT_ZN_ID"] == null ? "" : request["OUT_ZN_ID"].ToString();


                try
                {
                    lock (oClientSocketCO)
                    {
                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_GATE_ID", sGATE_ID);
                        htParamsHashtable.Add("IN_GATE_TYPE", sGATE_TYPE);
                        htParamsHashtable.Add("IN_GATE_NM", sGATE_NM);
                        htParamsHashtable.Add("IN_USE_SENSOR", sUSE_SENSOR);
                        htParamsHashtable.Add("IN_IN_ZN_ID", sIN_ZN_ID);
                        htParamsHashtable.Add("IN_OUT_ZN_ID", sOUT_ZN_ID);


                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.C118REQ_INSERT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", "118REP");

                        if (strAppCode.Equals("0"))
                        {
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");

                            htParamsHashtable = new Hashtable();
                            htParamsHashtable.Clear();

                            dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C118REQ_SELECT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

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
                                /*
                                    htParamsHashtable.Add("IN_GATE_ID", sGATE_ID);
                                    htParamsHashtable.Add("IN_GATE_TYPE", sGATE_TYPE);
                                    htParamsHashtable.Add("IN_GATE_NM", sGATE_NM);
                                    htParamsHashtable.Add("IN_USE_SENSOR", sUSE_SENSOR);
                                    htParamsHashtable.Add("IN_IN_ZN_ID", sIN_ZN_ID);
                                    htParamsHashtable.Add("IN_OUT_ZN_ID", sOUT_ZN_ID); 
                                */
                                Dictionary<string, object> param = new Dictionary<string, object>();
                                param.Add("WORKDIV", "RFIDGATE");
                                param.Add("CRUD_TP", sCRUD_TP);
                                param.Add("GATE_ID", sGATE_ID);
                                param.Add("GATE_TYPE", sGATE_TYPE);
                                param.Add("GATE_NM", sGATE_NM);
                                param.Add("USE_SENSOR", sUSE_SENSOR);
                                param.Add("IN_ZN_ID", sIN_ZN_ID);
                                param.Add("OUT_ZN_ID", sOUT_ZN_ID);
                                param.Add("LIST", list);

                                JObject json2 = JObject.FromObject(PamsClient.connect("com.hs.pams.common.server.bkroom.RfidGearReceiveMsg", param));
                                string sRtn2 = json2.ToString();

                                SendResponse_New(sRtn2, sWORKDIV + "REQ");
                            }
                        }
                        else
                        {
                            aData.Add("RESULT", "-1");
                            aData.Add("ERROR", strAppMsg);

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

                            SendResponse_New(sRtn, "118REQ");

                            Program.AppLog.Write("PAMSDATA.C118REQ_INSERT 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + request.ToString());
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.C118REQ_INSERT 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
                        }

                        

                    }
                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "C118REQ_INSERT 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "GATE관리 INSERT  오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
            else if (sCRUD_TP == "U")
            {
                sGATE_ID = request["GATE_ID"] == null ? "" : request["GATE_ID"].ToString();
                sGATE_TYPE = request["GATE_TYPE"] == null ? "" : request["GATE_TYPE"].ToString();
                sGATE_NM = request["GATE_NM"] == null ? "" : request["GATE_NM"].ToString();
                sUSE_SENSOR = request["USE_SENSOR"] == null ? "" : request["USE_SENSOR"].ToString();
                sIN_ZN_ID = request["IN_ZN_ID"] == null ? "" : request["IN_ZN_ID"].ToString();
                sOUT_ZN_ID = request["OUT_ZN_ID"] == null ? "" : request["OUT_ZN_ID"].ToString();

                try
                {
                    lock (oClientSocketCO)
                    {
                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_GATE_ID", sGATE_ID);
                        htParamsHashtable.Add("IN_GATE_TYPE", sGATE_TYPE);
                        htParamsHashtable.Add("IN_GATE_NM", sGATE_NM);
                        htParamsHashtable.Add("IN_USE_SENSOR", sUSE_SENSOR);
                        htParamsHashtable.Add("IN_IN_ZN_ID", sIN_ZN_ID);
                        htParamsHashtable.Add("IN_OUT_ZN_ID", sOUT_ZN_ID);


                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.C118REQ_UPDATE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", "118REP");

                        if (strAppCode.Equals("0"))
                        {
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");

                            htParamsHashtable = new Hashtable();
                            htParamsHashtable.Clear();

                            dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C118REQ_SELECT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

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
                                /*
                                    htParamsHashtable.Add("IN_GATE_ID", sGATE_ID);
                                    htParamsHashtable.Add("IN_GATE_TYPE", sGATE_TYPE);
                                    htParamsHashtable.Add("IN_GATE_NM", sGATE_NM);
                                    htParamsHashtable.Add("IN_USE_SENSOR", sUSE_SENSOR);
                                    htParamsHashtable.Add("IN_IN_ZN_ID", sIN_ZN_ID);
                                    htParamsHashtable.Add("IN_OUT_ZN_ID", sOUT_ZN_ID); 
                                */
                                Dictionary<string, object> param = new Dictionary<string, object>();
                                param.Add("WORKDIV", "RFIDGATE");
                                param.Add("CRUD_TP", sCRUD_TP);
                                param.Add("GATE_ID", sGATE_ID);
                                param.Add("GATE_TYPE", sGATE_TYPE);
                                param.Add("GATE_NM", sGATE_NM);
                                param.Add("USE_SENSOR", sUSE_SENSOR);
                                param.Add("IN_ZN_ID", sIN_ZN_ID);
                                param.Add("OUT_ZN_ID", sOUT_ZN_ID);
                                param.Add("LIST", list);

                                JObject json2 = JObject.FromObject(PamsClient.connect("com.hs.pams.common.server.bkroom.RfidGearReceiveMsg", param));
                                string sRtn2 = json2.ToString();

                                SendResponse_New(sRtn2, sWORKDIV + "REQ");
                            }
                        }
                        else
                        {
                            aData.Add("RESULT", "-1");
                            aData.Add("ERROR", strAppMsg);

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

                            SendResponse_New(sRtn, "118REQ");

                            Program.AppLog.Write("PAMSDATA.C118REQ_UPDATE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + request.ToString());
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.C118REQ_UPDATE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
                        }

                        

                    }
                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "C118REQ_UPDATE 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "GATE관리 UPDATE  오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
            else if (sCRUD_TP == "D")
            {
                sGATE_ID = request["GATE_ID"] == null ? "" : request["GATE_ID"].ToString();

                try
                {
                    lock (oClientSocketCO)
                    {
                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_GATE_ID", sGATE_ID);


                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.C118REQ_DELETE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", "118REP");

                        if (strAppCode.Equals("0"))
                        {
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");

                            htParamsHashtable = new Hashtable();
                            htParamsHashtable.Clear();

                            dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C118REQ_SELECT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

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
                                /*
                                    htParamsHashtable.Add("IN_GATE_ID", sGATE_ID);
                                    htParamsHashtable.Add("IN_GATE_TYPE", sGATE_TYPE);
                                    htParamsHashtable.Add("IN_GATE_NM", sGATE_NM);
                                    htParamsHashtable.Add("IN_USE_SENSOR", sUSE_SENSOR);
                                    htParamsHashtable.Add("IN_IN_ZN_ID", sIN_ZN_ID);
                                    htParamsHashtable.Add("IN_OUT_ZN_ID", sOUT_ZN_ID); 
                                */
                                Dictionary<string, object> param = new Dictionary<string, object>();
                                param.Add("WORKDIV", "RFIDGATE");
                                param.Add("CRUD_TP", sCRUD_TP);
                                param.Add("GATE_ID", sGATE_ID);
                                param.Add("GATE_TYPE", sGATE_TYPE);
                                param.Add("GATE_NM", sGATE_NM);
                                param.Add("USE_SENSOR", sUSE_SENSOR);
                                param.Add("IN_ZN_ID", sIN_ZN_ID);
                                param.Add("OUT_ZN_ID", sOUT_ZN_ID);
                                param.Add("LIST", list);

                                JObject json2 = JObject.FromObject(PamsClient.connect("com.hs.pams.common.server.bkroom.RfidGearReceiveMsg", param));
                                string sRtn2 = json2.ToString();

                                SendResponse_New(sRtn2, sWORKDIV + "REQ");
                            }
                        }
                        else
                        {
                            aData.Add("RESULT", "-1");
                            aData.Add("ERROR", strAppMsg);

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

                            SendResponse_New(sRtn, "118REQ");

                            Program.AppLog.Write("PAMSDATA.C118REQ_DELETE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + request.ToString());
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.C118REQ_DELETE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
                        }

                        

                    }
                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "C118REQ_DELETE 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "GATE관리 DELETE  오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
        }


        /// <summary>
        /// //119REQ(장비정보)
        /// </summary>
        /// <param name="request"></param>
        public void C119REQ(JObject request)
        {
            string sWORKDIV = "119";

            Hashtable htParamsHashtable;
            DataSet dsResult;

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sCRUD_TP = request["CRUD_TP"] == null ? "" : request["CRUD_TP"].ToString();
            string sEQ_ID = string.Empty;
            string sEQ_NM = string.Empty;
            string sEQ_TYPE = string.Empty; //고정형 : "GAT", 휴대형 리더기 : "PDA", 정수점검기 : "CNT", 태그발행기 : "PRT"
            string sGATE_ID = string.Empty;
            string sIP_ADDR = string.Empty;
            string sNETMASK = string.Empty;
            string sGATEWAY = string.Empty;
            string sMAC_ADDR = string.Empty;
            string sATTENU = string.Empty;
            string sANT_CNT = string.Empty;
            string sANT_SEQ = string.Empty;
            string sNOTFY_TYPE = string.Empty;
            string sNOTFY_STT = string.Empty;

            if (sCRUD_TP == "R")
            {
                try
                {
                    lock (oClientSocketCO)
                    {

                        sEQ_TYPE = request["EQ_TYPE"] == null ? "" : request["EQ_TYPE"].ToString();

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
                            aData.Add("rows", list);
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");
                            

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
                            aData.Add("rows", list);
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");

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

                            SendResponse_New(sRtn, "119REQ");
                        }

                    }
                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "장비 관리 조회 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "장비 관리 조회  오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
            else if (sCRUD_TP == "C")
            {
                sEQ_ID = request["EQ_ID"] == null ? "" : request["EQ_ID"].ToString();
                sEQ_NM = request["EQ_NM"] == null ? "" : request["EQ_NM"].ToString();
                sEQ_TYPE = request["EQ_TYPE"] == null ? "" : request["EQ_TYPE"].ToString();
                sGATE_ID = request["GATE_ID"] == null ? "" : request["GATE_ID"].ToString();
                sIP_ADDR = request["IP_ADDR"] == null ? "" : request["IP_ADDR"].ToString();
                sNETMASK = request["NETMASK"] == null ? "" : request["NETMASK"].ToString();
                sGATEWAY = request["GATEWAY"] == null ? "" : request["GATEWAY"].ToString();
                sMAC_ADDR = request["MAC_ADDR"] == null ? "" : request["MAC_ADDR"].ToString();
                sATTENU = request["ATTENU"] == null ? "" : request["ATTENU"].ToString();
                sANT_CNT = request["ANT_CNT"] == null ? "" : request["ANT_CNT"].ToString();
                sANT_SEQ = request["ANT_SEQ"] == null ? "" : request["ANT_SEQ"].ToString();
                sNOTFY_TYPE = request["NOTFY_TYPE"] == null ? "" : request["NOTFY_TYPE"].ToString();
                sNOTFY_STT = request["NOTFY_STT"] == null ? "" : request["NOTFY_STT"].ToString();

                try
                {
                    lock (oClientSocketCO)
                    {
                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_EQ_ID", sEQ_ID);
                        htParamsHashtable.Add("IN_EQ_NM", sEQ_NM);
                        htParamsHashtable.Add("IN_EQ_TYPE", sEQ_TYPE);
                        htParamsHashtable.Add("IN_GATE_ID", sGATE_ID);
                        htParamsHashtable.Add("IN_IP_ADDR", sIP_ADDR);
                        htParamsHashtable.Add("IN_NETMASK", sNETMASK);
                        htParamsHashtable.Add("IN_GATEWAY", sGATEWAY);
                        htParamsHashtable.Add("IN_MAC_ADDR", sMAC_ADDR);
                        htParamsHashtable.Add("IN_ATTENU", sATTENU);
                        htParamsHashtable.Add("IN_ANT_CNT", sANT_CNT);
                        htParamsHashtable.Add("IN_ANT_SEQ", sANT_SEQ);
                        htParamsHashtable.Add("IN_NOTFY_TYPE", sNOTFY_TYPE);
                        htParamsHashtable.Add("IN_NOTFY_STT", sNOTFY_STT);


                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.C119REQ_INSERT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", "119REP");

                        if (strAppCode.Equals("0"))
                        {
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");

                            htParamsHashtable = new Hashtable();
                            htParamsHashtable.Clear();
                            htParamsHashtable.Add("IN_EQ_TYPE", "");

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

                                /*
                                     sEQ_ID = request["EQ_ID"] == null ? "" : request["EQ_ID"].ToString();
                                    sEQ_NM = request["EQ_NM"] == null ? "" : request["EQ_NM"].ToString();
                                    sEQ_TYPE = request["EQ_TYPE"] == null ? "" : request["EQ_TYPE"].ToString();
                                    sGATE_ID = request["GATE_ID"] == null ? "" : request["GATE_ID"].ToString();
                                    sIP_ADDR = request["IP_ADDR"] == null ? "" : request["IP_ADDR"].ToString();
                                    sNETMASK = request["NETMASK"] == null ? "" : request["NETMASK"].ToString();
                                    sGATEWAY = request["GATEWAY"] == null ? "" : request["GATEWAY"].ToString();
                                    sMAC_ADDR = request["MAC_ADDR"] == null ? "" : request["MAC_ADDR"].ToString();
                                    sATTENU = request["ATTENU"] == null ? "" : request["ATTENU"].ToString();
                                    sANT_CNT = request["ANT_CNT"] == null ? "" : request["ANT_CNT"].ToString();
                                    sANT_SEQ = request["ANT_SEQ"] == null ? "" : request["ANT_SEQ"].ToString();
                                    sNOTFY_TYPE = request["NOTFY_TYPE"] == null ? "" : request["NOTFY_TYPE"].ToString();
                                    sNOTFY_STT = request["NOTFY_STT"] == null ? "" : request["NOTFY_STT"].ToString();
                                */

                                Dictionary<string, object> param = new Dictionary<string, object>();
                                param.Add("WORKDIV", "RFIDEQUIP");
                                param.Add("CRUD_TP", sCRUD_TP);
                                param.Add("EQ_ID", sEQ_ID);
                                param.Add("EQ_NM", sEQ_NM);
                                //param.Add("EQ_TYPE", sEQ_TYPE);
                                param.Add("GATE_ID", sGATE_ID);
                                param.Add("IP_ADDR", sIP_ADDR);
                                param.Add("NETMASK", sNETMASK);
                                param.Add("GATEWAY", sGATEWAY);
                                param.Add("MAC_ADDR", sMAC_ADDR);
                                param.Add("ATTENU", sATTENU);
                                param.Add("ANT_CNT", sANT_CNT);
                                param.Add("ANT_SEQ", sANT_SEQ);
                                param.Add("NOTFY_TYPE", sNOTFY_TYPE);
                                param.Add("NOTFY_STT", sNOTFY_STT);
                                param.Add("LIST", list);

                                JObject json2 = JObject.FromObject(PamsClient.connect("com.hs.pams.common.server.bkroom.RfidGearReceiveMsg", param));
                                string sRtn2 = json2.ToString();

                                SendResponse_New(sRtn2, sWORKDIV + "REQ");
                            }

                        }
                        else
                        {
                            aData.Add("RESULT", "-1");
                            aData.Add("ERROR", strAppMsg);

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

                            SendResponse_New(sRtn, "119REQ");

                            Program.AppLog.Write("PAMSDATA.C119REQ_INSERT 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + request.ToString());
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.C119REQ_INSERT 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
                        }

                        

                    }
                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "C119REQ_INSERT 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "장비 관리 INSERT  오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
            else if (sCRUD_TP == "U")
            {
                sEQ_ID = request["EQ_ID"] == null ? "" : request["EQ_ID"].ToString();
                sEQ_NM = request["EQ_NM"] == null ? "" : request["EQ_NM"].ToString();
                sEQ_TYPE = request["EQ_TYPE"] == null ? "" : request["EQ_TYPE"].ToString();
                sGATE_ID = request["GATE_ID"] == null ? "" : request["GATE_ID"].ToString();
                sIP_ADDR = request["IP_ADDR"] == null ? "" : request["IP_ADDR"].ToString();
                sNETMASK = request["NETMASK"] == null ? "" : request["NETMASK"].ToString();
                sGATEWAY = request["GATEWAY"] == null ? "" : request["GATEWAY"].ToString();
                sMAC_ADDR = request["MAC_ADDR"] == null ? "" : request["MAC_ADDR"].ToString();
                sATTENU = request["ATTENU"] == null ? "" : request["ATTENU"].ToString();
                sANT_CNT = request["ANT_CNT"] == null ? "" : request["ANT_CNT"].ToString();
                sANT_SEQ = request["ANT_SEQ"] == null ? "" : request["ANT_SEQ"].ToString();
                sNOTFY_TYPE = request["NOTFY_TYPE"] == null ? "" : request["NOTFY_TYPE"].ToString();
                sNOTFY_STT = request["NOTFY_STT"] == null ? "" : request["NOTFY_STT"].ToString();

                try
                {
                    lock (oClientSocketCO)
                    {
                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_EQ_ID", sEQ_ID);
                        htParamsHashtable.Add("IN_EQ_NM", sEQ_NM);
                        htParamsHashtable.Add("IN_EQ_TYPE", sEQ_TYPE);
                        htParamsHashtable.Add("IN_GATE_ID", sGATE_ID);
                        htParamsHashtable.Add("IN_IP_ADDR", sIP_ADDR);
                        htParamsHashtable.Add("IN_NETMASK", sNETMASK);
                        htParamsHashtable.Add("IN_GATEWAY", sGATEWAY);
                        htParamsHashtable.Add("IN_MAC_ADDR", sMAC_ADDR);
                        htParamsHashtable.Add("IN_ATTENU", sATTENU);
                        htParamsHashtable.Add("IN_ANT_CNT", sANT_CNT);
                        htParamsHashtable.Add("IN_ANT_SEQ", sANT_SEQ);
                        htParamsHashtable.Add("IN_NOTFY_TYPE", sNOTFY_TYPE);
                        htParamsHashtable.Add("IN_NOTFY_STT", sNOTFY_STT);


                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.C119REQ_UPDATE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", "119REP");

                        if (strAppCode.Equals("0"))
                        {
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");

                            htParamsHashtable = new Hashtable();
                            htParamsHashtable.Clear();
                            htParamsHashtable.Add("IN_EQ_TYPE", "");

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

                                /*
                                     sEQ_ID = request["EQ_ID"] == null ? "" : request["EQ_ID"].ToString();
                                    sEQ_NM = request["EQ_NM"] == null ? "" : request["EQ_NM"].ToString();
                                    sEQ_TYPE = request["EQ_TYPE"] == null ? "" : request["EQ_TYPE"].ToString();
                                    sGATE_ID = request["GATE_ID"] == null ? "" : request["GATE_ID"].ToString();
                                    sIP_ADDR = request["IP_ADDR"] == null ? "" : request["IP_ADDR"].ToString();
                                    sNETMASK = request["NETMASK"] == null ? "" : request["NETMASK"].ToString();
                                    sGATEWAY = request["GATEWAY"] == null ? "" : request["GATEWAY"].ToString();
                                    sMAC_ADDR = request["MAC_ADDR"] == null ? "" : request["MAC_ADDR"].ToString();
                                    sATTENU = request["ATTENU"] == null ? "" : request["ATTENU"].ToString();
                                    sANT_CNT = request["ANT_CNT"] == null ? "" : request["ANT_CNT"].ToString();
                                    sANT_SEQ = request["ANT_SEQ"] == null ? "" : request["ANT_SEQ"].ToString();
                                    sNOTFY_TYPE = request["NOTFY_TYPE"] == null ? "" : request["NOTFY_TYPE"].ToString();
                                    sNOTFY_STT = request["NOTFY_STT"] == null ? "" : request["NOTFY_STT"].ToString();
                                */

                                Dictionary<string, object> param = new Dictionary<string, object>();
                                param.Add("WORKDIV", "RFIDEQUIP");
                                param.Add("CRUD_TP", sCRUD_TP);
                                param.Add("EQ_ID", sEQ_ID);
                                param.Add("EQ_NM", sEQ_NM);
                                //param.Add("EQ_TYPE", sEQ_TYPE);
                                param.Add("GATE_ID", sGATE_ID);
                                param.Add("IP_ADDR", sIP_ADDR);
                                param.Add("NETMASK", sNETMASK);
                                param.Add("GATEWAY", sGATEWAY);
                                param.Add("MAC_ADDR", sMAC_ADDR);
                                param.Add("ATTENU", sATTENU);
                                param.Add("ANT_CNT", sANT_CNT);
                                param.Add("ANT_SEQ", sANT_SEQ);
                                param.Add("NOTFY_TYPE", sNOTFY_TYPE);
                                param.Add("NOTFY_STT", sNOTFY_STT);
                                param.Add("LIST", list);

                                JObject json2 = JObject.FromObject(PamsClient.connect("com.hs.pams.common.server.bkroom.RfidGearReceiveMsg", param));
                                string sRtn2 = json2.ToString();

                                SendResponse_New(sRtn2, sWORKDIV + "REQ");
                            }
                        }
                        else
                        {
                            aData.Add("RESULT", "-1");
                            aData.Add("ERROR", strAppMsg);

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

                            SendResponse_New(sRtn, "119REQ");

                            Program.AppLog.Write("PAMSDATA.C119REQ_UPDATE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + request.ToString());
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.C119REQ_UPDATE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
                        }

                        

                    }
                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "C119REQ_UPDATE 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "장비 관리 UPDATE  오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
            else if (sCRUD_TP == "D")
            {
                sEQ_ID = request["EQ_ID"] == null ? "" : request["EQ_ID"].ToString();

                try
                {
                    lock (oClientSocketCO)
                    {
                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_EQ_ID", sEQ_ID);


                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.C119REQ_DELETE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", "119REP");

                        if (strAppCode.Equals("0"))
                        {
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");

                            htParamsHashtable = new Hashtable();
                            htParamsHashtable.Clear();
                            htParamsHashtable.Add("IN_EQ_TYPE", "");

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

                                /*
                                     sEQ_ID = request["EQ_ID"] == null ? "" : request["EQ_ID"].ToString();
                                    sEQ_NM = request["EQ_NM"] == null ? "" : request["EQ_NM"].ToString();
                                    sEQ_TYPE = request["EQ_TYPE"] == null ? "" : request["EQ_TYPE"].ToString();
                                    sGATE_ID = request["GATE_ID"] == null ? "" : request["GATE_ID"].ToString();
                                    sIP_ADDR = request["IP_ADDR"] == null ? "" : request["IP_ADDR"].ToString();
                                    sNETMASK = request["NETMASK"] == null ? "" : request["NETMASK"].ToString();
                                    sGATEWAY = request["GATEWAY"] == null ? "" : request["GATEWAY"].ToString();
                                    sMAC_ADDR = request["MAC_ADDR"] == null ? "" : request["MAC_ADDR"].ToString();
                                    sATTENU = request["ATTENU"] == null ? "" : request["ATTENU"].ToString();
                                    sANT_CNT = request["ANT_CNT"] == null ? "" : request["ANT_CNT"].ToString();
                                    sANT_SEQ = request["ANT_SEQ"] == null ? "" : request["ANT_SEQ"].ToString();
                                    sNOTFY_TYPE = request["NOTFY_TYPE"] == null ? "" : request["NOTFY_TYPE"].ToString();
                                    sNOTFY_STT = request["NOTFY_STT"] == null ? "" : request["NOTFY_STT"].ToString();
                                */

                                Dictionary<string, object> param = new Dictionary<string, object>();
                                param.Add("WORKDIV", "RFIDEQUIP");
                                param.Add("CRUD_TP", sCRUD_TP);
                                param.Add("EQ_ID", sEQ_ID);
                                param.Add("EQ_NM", sEQ_NM);
                                //param.Add("EQ_TYPE", sEQ_TYPE);
                                param.Add("GATE_ID", sGATE_ID);
                                param.Add("IP_ADDR", sIP_ADDR);
                                param.Add("NETMASK", sNETMASK);
                                param.Add("GATEWAY", sGATEWAY);
                                param.Add("MAC_ADDR", sMAC_ADDR);
                                param.Add("ATTENU", sATTENU);
                                param.Add("ANT_CNT", sANT_CNT);
                                param.Add("ANT_SEQ", sANT_SEQ);
                                param.Add("NOTFY_TYPE", sNOTFY_TYPE);
                                param.Add("NOTFY_STT", sNOTFY_STT);
                                param.Add("LIST", list);

                                JObject json2 = JObject.FromObject(PamsClient.connect("com.hs.pams.common.server.bkroom.RfidGearReceiveMsg", param));
                                string sRtn2 = json2.ToString();

                                SendResponse_New(sRtn2, sWORKDIV + "REQ");
                            }
                        }
                        else
                        {
                            aData.Add("RESULT", "-1");
                            aData.Add("ERROR", strAppMsg);

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

                            SendResponse_New(sRtn, "119REQ");

                            Program.AppLog.Write("PAMSDATA.C119REQ_DELETE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + sGATE_ID);
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.C119REQ_DELETE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
                        }

                        

                    }
                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "C119REQ_DELETE 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "장비 관리 DELETE  오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
        }

        /// <summary>
        /// //120REQ(프로그램관리)
        /// </summary>
        /// <param name="request"></param>
        public void C120REQ(JObject request)
        {
            string sWORKDIV = "120";

            Hashtable htParamsHashtable;
            DataSet dsResult;

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            string sCRUD_TP = request["CRUD_TP"] == null ? "" : request["CRUD_TP"].ToString();
            string sSEQ = string.Empty;
            string sEQ_TYPE = string.Empty;     //고정형 : "GAT", 휴대형 리더기 : "PDA", 정수점검기 : "CNT", 태그발행기 : "PRT"
            string sSW_TYPE = string.Empty;
            string sSW_NM = string.Empty;
            string sSW_FILENAME = string.Empty;
            string sSW_FILE = string.Empty;
            string sDESCP = string.Empty;

            if (sCRUD_TP == "R")
            {
                try
                {
                    lock (oClientSocketCO)
                    {

                        sEQ_TYPE = request["EQ_TYPE"] == null ? "" : request["EQ_TYPE"].ToString();

                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_EQ_TYPE", sEQ_TYPE);

                        dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C120REQ_SELECT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

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
                            aData.Add("rows", list);
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");

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
                            aData.Add("count", list.Count);
                            aData.Add("rows", list);
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");

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
                    Program.SysLog.Write(this, "장비 관리 조회  오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
            else if (sCRUD_TP == "C")
            {
                sEQ_TYPE = request["EQ_TYPE"] == null ? "" : request["EQ_TYPE"].ToString();
                sSW_TYPE = request["SW_TYPE"] == null ? "" : request["SW_TYPE"].ToString();
                sSW_NM = request["SW_NM"] == null ? "" : request["SW_NM"].ToString();
                sSW_FILENAME = request["SW_FILENAME"] == null ? "" : request["SW_FILENAME"].ToString();
                sSW_FILE = request["SW_FILE"] == null ? "" : request["SW_FILE"].ToString();
                sDESCP = request["DESCP"] == null ? "" : request["DESCP"].ToString();

                try
                {
                    lock (oClientSocketCO)
                    {
                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_EQ_TYPE", sEQ_TYPE);
                        htParamsHashtable.Add("IN_SW_TYPE", sSW_TYPE);
                        htParamsHashtable.Add("IN_SW_NM", sSW_NM);
                        htParamsHashtable.Add("IN_SW_FILENAME", sSW_FILENAME);
                        //htParamsHashtable.Add("IN_SW_FILE", sSW_FILE);

                        if (sSW_FILE.Trim().Length > 0)
                        {
                            byte[] buff;
                            buff = Convert.FromBase64String(sSW_FILE);

                            htParamsHashtable.Add("IN_SW_FILE", buff);
                        }
                        //else
                        //{
                        //    htParamsHashtable.Add("IN_SW_FILE", sSW_FILE);
                        //}


                        htParamsHashtable.Add("IN_DESCP", sDESCP);


                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.C120REQ_INSERT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", "120REP");

                        if (strAppCode.Equals("0"))
                        {
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");
                        }
                        else
                        {
                            aData.Add("RESULT", "-1");
                            aData.Add("ERROR", strAppMsg);

                            Program.AppLog.Write("PAMSDATA.C120REQ_INSERT 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + request.ToString());
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.C120REQ_INSERT 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
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
                    //Program.SysLog.Write(this, "프로그램 관리 INSERT 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "장비 관리 INSERT  오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
            else if (sCRUD_TP == "U")
            {
                sSEQ = request["SEQ"] == null ? "" : request["SEQ"].ToString();
                sEQ_TYPE = request["EQ_TYPE"] == null ? "" : request["EQ_TYPE"].ToString();
                sSW_TYPE = request["SW_TYPE"] == null ? "" : request["SW_TYPE"].ToString();
                sSW_NM = request["SW_NM"] == null ? "" : request["SW_NM"].ToString();
                sSW_FILENAME = request["SW_FILENAME"] == null ? "" : request["SW_FILENAME"].ToString();
                sSW_FILE = request["SW_FILE"] == null ? "" : request["SW_FILE"].ToString();
                sDESCP = request["DESCP"] == null ? "" : request["DESCP"].ToString();

                try
                {
                    lock (oClientSocketCO)
                    {
                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_SEQ", sSEQ);
                        htParamsHashtable.Add("IN_EQ_TYPE", sEQ_TYPE);
                        htParamsHashtable.Add("IN_SW_TYPE", sSW_TYPE);
                        htParamsHashtable.Add("IN_SW_NM", sSW_NM);
                        htParamsHashtable.Add("IN_SW_FILENAME", sSW_FILENAME);
                        //htParamsHashtable.Add("IN_SW_FILE", sSW_FILE);

                        if (sSW_FILE.Trim().Length > 0)
                        {
                            byte[] buff;
                            buff = Convert.FromBase64String(sSW_FILE);
                            htParamsHashtable.Add("IN_SW_FILE", buff);
                        }
                        //else
                        //{
                        //    htParamsHashtable.Add("IN_SW_FILE", sSW_FILE);
                        //}

                        htParamsHashtable.Add("IN_DESCP", sDESCP);


                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.C120REQ_UPDATE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", sWORKDIV + "REP");

                        if (strAppCode.Equals("0"))
                        {
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");
                        }
                        else
                        {
                            aData.Add("RESULT", "-1");
                            aData.Add("ERROR", strAppMsg);

                            Program.AppLog.Write("PAMSDATA.C120REQ_UPDATE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + request.ToString());
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.C120REQ_UPDATE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]");
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
                    //Program.SysLog.Write(this, "프로그램관리 UPDATE 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "장비 관리 UPDATE  오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
            else if (sCRUD_TP == "D")
            {
                sSEQ = request["SEQ"] == null ? "" : request["SEQ"].ToString();

                try
                {
                    lock (oClientSocketCO)
                    {
                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();
                        htParamsHashtable.Add("IN_SEQ", sSEQ);

                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.C120REQ_DELETE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", sWORKDIV + "REP");

                        if (strAppCode.Equals("0"))
                        {
                            aData.Add("RESULT", "0");
                            aData.Add("ERROR", "");
                        }
                        else
                        {
                            aData.Add("RESULT", "-1");
                            aData.Add("ERROR", strAppMsg);

                            Program.AppLog.Write("PAMSDATA.C120REQ_DELETE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + "SEQ : "+ sSEQ);
                            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("PAMSDATA.C120REQ_DELETE 오류 : [APP_CODE:" + strAppCode + ", APP_MSG:" + strAppMsg + "]" + "SEQ : " + sSEQ);
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
                    //Program.SysLog.Write(this, "프로그램관리 DELETE 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "장비 관리 DELETE  오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                }
            }
        }

        /// <summary>
        /// 장비별프로그램관리
        /// (사용하는 곳 있는 지 확인 필요 : 현재는 없음)
        /// </summary>
        /// <param name="request"></param>
        public void C121REQ(JObject request)
        { 
        
        }

        /// <summary>
        /// 200REQ(서가별 서고 현황 요청) : 서고현황
        /// </summary>
        /// <param name="request"></param>
        public void C200REQ(JObject request)
        {
            /*
             * 	=================== summary ===================
             * 	 	업무명     : 서고현황
             * 		서브업무명 : 서고현황
             *  ===============================================
             *  
             * 	============= request parameters ==============
             * 		BKROOM_ID : 서고ID
             *  ===============================================
             *  
             * 	============= response parameters =============
             * 		LIST
             * 		  - BKROOM_ID 			: 서고ID
             * 		  - BKROOM_NM  			: 서고명
             * 		  - BKSTND_ID   		: 서가ID
             * 		  - BKSTND_NM  			: 서가명
             * 		  - BIND_CNT  			: 관리수
             * 		  - BIND_NOT_IN_CNT  	: 보류수
             * 		  - BIND_OUT_CNT  		: 반출수
             *  ===============================================
            */

            string sWORKDIV = "200";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "BKSTNDLIST";
                    }
                    else
                    {
                        request.Add("WORKDIV", "BKSTNDLIST");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.BkroomViewSttsMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "서고현황 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// 201REQ(연별 서가 현황 요청) :서가별현황
        /// </summary>
        /// <param name="request"></param>
        public void C201REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 서고현황
	         * 		서브업무명 : 서가별현황
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		BKROOM_ID : 서고ID
	         * 		BKSTND_ID : 서가ID
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		LIST
	         * 		  - BKROOM_ID 			: 서고ID
	         * 		  - BKROOM_NM  			: 서고명
	         * 		  - BKSTND_ID   		: 서가ID
	         * 		  - BKSTND_NM  			: 서가명
	         * 		  - COL_NO				: 연번호
	         * 		  - BIND_CNT  			: 관리수
	         * 		  - BIND_NOT_IN_CNT  	: 보류수
	         * 		  - BIND_OUT_CNT  		: 반출수
	         *  ===============================================
	         *  
	        */

            string sWORKDIV = "201";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "BKCOLLIST";
                    }
                    else
                    {
                        request.Add("WORKDIV", "BKCOLLIST");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.BkroomViewSttsMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "서가별현황 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// 202REQ(단별 연 현황 요청) :연별현황
        /// </summary>
        /// <param name="request"></param>
        public void C202REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	 * 	 	업무명     : 서고현황
	 * 		서브업무명 : 연별현황
	 *  ===============================================
	 *  
	 * 	============= request parameters ==============
	 * 		BKROOM_ID : 서고ID
	 * 		BKSTND_ID : 서가ID
	 * 		COL_NO    : 연번호
	 *  ===============================================
	 *  
	 * 	============= response parameters =============
	 * 		LIST
	 * 		  - BKROOM_ID 			: 서고ID
	 * 		  - BKROOM_NM  			: 서고명
	 * 		  - BKSTND_ID   		: 서가ID
	 * 		  - BKSTND_NM  			: 서가명
	 * 		  - COL_NO				: 연번호
	 * 		  - ROW_NO				: 단번호
	 * 		  - BIND_CNT  			: 관리수
	 * 		  - BIND_NOT_IN_CNT  	: 보류수
	 * 		  - BIND_OUT_CNT  		: 반출수
	 *  ===============================================
	        */

            string sWORKDIV = "202";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "BKROWNOLIST";
                    }
                    else
                    {
                        request.Add("WORKDIV", "BKROWNOLIST");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.BkroomViewSttsMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "연별현황 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //203REQ(단별 기록물 목록 요청)
        /// </summary>
        /// <param name="request"></param>
        public void C203REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 서고현황
	         * 		서브업무명 : 단별 기록물 목록
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		BKROOM_ID : 서고ID
	         * 		BKSTND_ID : 서가ID
	         * 		COL_NO    : 연번호
	         * 		ROW_NO	  : 단번호
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		LIST
	         * 		  - BKROOM_ID 			: 서고ID
	         * 		  - BKROOM_NM  			: 서고명
	         * 		  - BKSTND_ID   		: 서가ID
	         * 		  - BKSTND_NM  			: 서가명
	         * 		  - COL_NO				: 연번호
	         * 		  - ROW_NO				: 단번호
	         * 		  - BIND_CNT  			: 관리수
	         * 		  - BIND_NOT_IN_CNT  	: 보류수
	         * 		  - BIND_OUT_CNT  		: 반출수
	         *  ===============================================
	        */

            string sWORKDIV = "203";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "BKBINDLIST";
                    }
                    else
                    {
                        request.Add("WORKDIV", "BKBINDLIST");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.BkroomViewSttsMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "단별 기록물 목록 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //400REQ(배치/재배치 의뢰서 목록 요청)
        /// </summary>
        /// <param name="request"></param>
        public void C400REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 배치
	         * 		서브업무명 : 배치의뢰서 요청
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		USER_ID   : 의뢰자ID(?)
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		ARRN_APPL_ID  : 배치의뢰서ID
	         * 		APPL_ORG_NM   : 의뢰부서명
	         * 		APPL_DT  	  : 의뢰일
	         * 		ARRN_RSN  	  : 배치사유 
	         *  ===============================================
	        */

            string sWORKDIV = "400";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "ARRNAPPLLIST";
                    }
                    else
                    {
                        request.Add("WORKDIV", "ARRNAPPLLIST");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.RfidBkroomArrnMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "배치의뢰서 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //C401REQ(배치 : 배치대상물 목록)
        /// </summary>
        /// <param name="request"></param>
        public void C401REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 배치
	         * 		서브업무명 : 배치대상물 목록
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		ARRN_APPL_ID : 배치의뢰서ID
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		ARRN_TGT_ID  : 배치대상ID
	         * 		ARRN_TGT     : 배치대상
	         * 		BOX_NM  	 : 상자명
	         * 		DOC_TYPE  	 : 기록물형태 
	         * 		CNT  	     : 기록철수 
	         *  ===============================================
	        */

            string sWORKDIV = "401";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "ARRNINFOLIST";
                    }
                    else
                    {
                        request.Add("WORKDIV", "ARRNINFOLIST");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.RfidBkroomArrnMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "배치대상물 목록 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }


        /// <summary>
        /// //C402REQ(배치 : 기록물 배치)
        /// </summary>
        /// <param name="request"></param>
        public void C402REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 배치
	         * 		서브업무명 : 기록물 배치
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		ARRN_APPL_ID : 배치의뢰서ID
	         * 		ARRN_TGT_ID  : 배치대상ID
	         * 		BKROOM_ID    : 서고ID
	         *  	BKSTND_ID    : 서가ID
	         *  	COL_NO       : 연번호
	         *  	ROW_NO       : 단번호
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		ARRN_APPL_ID  : 배치의뢰서ID
	         * 		ARRN_TGT_ID   : 배치대상ID
	         * 		ARRN_STTS  	  : 배치상태
	         *  ===============================================
	        */

            string sWORKDIV = "402";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "ARRNFIX";
                    }
                    else
                    {
                        request.Add("WORKDIV", "ARRNFIX");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.RfidBkroomArrnMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    //JArray rows = null;
                    //if (response["LIST"] != null)
                    //{
                    //    rows = (JArray)response["LIST"];
                    //}

                    //response.Add("count", JToken.FromObject(rows.Count));
                    //response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "배치대상물 목록 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //C403REQ(의뢰서 없이 기록물 배치)
        /// </summary>
        /// <param name="request"></param>
        public void C403REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 배치
	         * 		서브업무명 : 기록물 배치
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		ARRN_APPL_ID : 배치의뢰서ID
	         * 		ARRN_TGT_ID  : 배치대상ID
	         * 		BKROOM_ID    : 서고ID
	         *  	BKSTND_ID    : 서가ID
	         *  	COL_NO       : 연번호
	         *  	ROW_NO       : 단번호
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		ARRN_APPL_ID  : 배치의뢰서ID
	         * 		ARRN_TGT_ID   : 배치대상ID
	         * 		ARRN_STTS  	  : 배치상태
	         *  ===============================================
	        */

            string sWORKDIV = "403";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "BINDREARRN";
                    }
                    else
                    {
                        request.Add("WORKDIV", "BINDREARRN");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.RfidBkroomArrnMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    //JArray rows = null;
                    //if (response["LIST"] != null)
                    //{
                    //    rows = (JArray)response["LIST"];
                    //}

                    //response.Add("count", JToken.FromObject(rows.Count));
                    //response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "(의뢰서 없이)기록물 배치 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //C502REQ(기록물 배치 : 재배치 요청)
        /// </summary>
        /// <param name="request"></param>
        public void C502REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 배치
	         * 		서브업무명 : 기록물 배치
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		ARRN_APPL_ID : 배치의뢰서ID
	         * 		ARRN_TGT_ID  : 배치대상ID
	         * 		BKROOM_ID    : 서고ID
	         *  	BKSTND_ID    : 서가ID
	         *  	COL_NO       : 연번호
	         *  	ROW_NO       : 단번호
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		ARRN_APPL_ID  : 배치의뢰서ID
	         * 		ARRN_TGT_ID   : 배치대상ID
	         * 		ARRN_STTS  	  : 배치상태
	         *  ===============================================
	        */

            string sWORKDIV = "502";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "REARRNFIX";
                    }
                    else
                    {
                        request.Add("WORKDIV", "REARRNFIX");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.RfidBkroomArrnMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    //JArray rows = null;
                    //if (response["LIST"] != null)
                    //{
                    //    rows = (JArray)response["LIST"];
                    //}

                    //response.Add("count", JToken.FromObject(rows.Count));
                    //response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "(재배치)기록물 배치 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //600REQ(반출 : 반출의뢰서 목록 요청)
        /// </summary>
        /// <param name="request"></param>
        public void C600REQ(JObject request)
        {
            /*
             * 	=================== summary ===================
             * 	 	업무명     : 반출
             * 		서브업무명 : 반출서 목록 요청
             *  ===============================================
             *  
             * 	============= request parameters ==============
             * 		USER_ID : 신청자ID
             *  ===============================================
             *  
             * 	============= response parameters =============
             * 		OUT_APPL_ID    : 반출서ID
             * 		APPL_ORG_NM    : 신청자부서명
             * 		APPL_USER_ID   : 신청자ID
             * 		APPL_USER_NM   : 신청자명 
             * 		OUT_APPL_DT    : 반출의뢰일
             * 		OUT_DT 		   : 반출일
             *  ===============================================
             *  
            */

            string sWORKDIV = "600";
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "INOUTLISTOUT";
                    }
                    else
                    {
                        request.Add("WORKDIV", "INOUTLISTOUT");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.InoutViewSttsMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "반출서 목록 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //601REQ(반출기록물 목록 요청)
        /// </summary>
        /// <param name="request"></param>
        public void C601REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 반출
	         * 		서브업무명 : 반출기록물 목록 요청
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		OUT_APPL_ID : 반출서ID
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		MGMT_ID    		: 관리번호
	         * 		DOC_TTL    		: 제목
	         * 		DOC_TYPE   		: 기록물형태
	         * 		PROD_ORG_CD   	: 생산기관 
	         * 		PROD_YY    		: 생산년도
	         * 		BKROOM_ID 		: 서고ID
	         * 		BKSTND_ID 		: 서가ID
	         * 		COL_NO 		    : 연번호
	         * 		ROW_NO 		    : 단번호
	         * 		XXXXX 		    : 철상태
	         * 		PAMS_CLS_CD 	: 분류체계
	         * 		DOC_CNT 		: 건수
	         * 		PAGE_CNT 		: 쪽수
	         * 		XXXXX1 		    : 반출내역
	         *  ===============================================
             *  
            */

            string sWORKDIV = "601";
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "INOUTBINDOUT";
                    }
                    else
                    {
                        request.Add("WORKDIV", "INOUTBINDOUT");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.InoutViewSttsMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "반출기록물 목록 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }


        /// <summary>
        /// //C602REQ(반출 : 반출 처리 요청)
        /// </summary>
        /// <param name="request"></param>
        public void C602REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 반출
	         * 		서브업무명 : 반출 처리 요청
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		OUT_APPL_ID  : 반출서ID
	         * 		MGMT_ID  	 : 관리번호
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		OUT_APPL_ID  : 반출서ID
	         * 		MGMT_ID   	 : 관리번호
	         * 		RESULT_CD  	 : 결과코드
	         *  ===============================================
             *  
            */

            string sWORKDIV = "602";
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "INOUTPROCOUT";
                    }
                    else
                    {
                        request.Add("WORKDIV", "INOUTPROCOUT");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.InoutProcSttsMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    //JArray rows = null;
                    //if (response["LIST"] != null)
                    //{
                    //    rows = (JArray)response["LIST"];
                    //}

                    //response.Add("count", JToken.FromObject(rows.Count));
                    //response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "반출 처리 요청 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //C650REQ(무단 반출/입 제외 대상 등록 요청)
        /// </summary>
        /// <param name="request"></param>
        public void C650REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 사전제외 
	         * 		서브업무명 : 무단 반출/입 제외 대상 등록 요청
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		USERID : 작성자ID
	         * 		EXTSTRTDT : 제외시작일시
	         * 		EXTENDDT : 제외종료일시
 	         *     TAGID : 제외대상목록, LIST
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		RESULT_CD  	 : 결과코드
	         *  ===============================================
             *  
            */

            string sWORKDIV = "650";
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    //if (request.ContainsKey("WORKDIV"))
                    //{
                    //    request["WORKDIV"] = "INOUTPROCOUT";
                    //}
                    //else
                    //{
                    //    request.Add("WORKDIV", "INOUTPROCOUT");
                    //}
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.AddNpermnOutMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    //JArray rows = null;
                    //if (response["LIST"] != null)
                    //{
                    //    rows = (JArray)response["LIST"];
                    //}

                    //response.Add("count", JToken.FromObject(rows.Count));
                    //response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "무단 반출/입 제외 대상 등록 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //700REQ(반입 : 반입서 목록 요청)
        /// </summary>
        /// <param name="request"></param>
        public void C700REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 반입
	         * 		서브업무명 : 반입서 목록 요청
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		USER_ID : 신청자ID
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		OUT_APPL_ID  	: 반출서ID
	         * 		IN_NO    		: 반입서번호
	         * 		APPL_ORG_NM  	: 신청자부서명
	         * 		APPL_USER_ID   	: 신청자ID 
	         * 		APPL_USER_NM 	: 신청자명
	         * 		OUT_APPL_DT     : 반출의뢰일
	         * 		OUT_DT 		    : 반출일
	         *  ===============================================
             *  
            */

            string sWORKDIV = "700";
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "INOUTLISTIN";
                    }
                    else
                    {
                        request.Add("WORKDIV", "INOUTLISTIN");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.InoutViewSttsMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "반입서 목록 요청 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //701REQ(반입기록물 목록 요청)
        /// </summary>
        /// <param name="request"></param>
        public void C701REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 반입
	         * 		서브업무명 : 반입기록물 목록 요청
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		OUT_APPL_ID : 반출서ID
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		MGMT_ID    		: 관리번호
	         * 		DOC_TTL    		: 제목
	         * 		DOC_TYPE   		: 기록물형태
	         * 		PROD_ORG_CD   	: 생산기관 
	         * 		PROD_YY    		: 생산년도
	         * 		BKROOM_ID 		: 서고ID
	         * 		BKSTND_ID 		: 서가ID
	         * 		COL_NO 		    : 연번호
	         * 		ROW_NO 		    : 단번호
	         * 		XXXXX 		    : 철상태
	         * 		PAMS_CLS_CD 	: 분류체계
	         * 		DOC_CNT 		: 건수
	         * 		PAGE_CNT 		: 쪽수
	         * 		XXXXX1 		    : 반출내역
	         *  ===============================================
             *  
            */

            string sWORKDIV = "701";
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "INOUTBINDIN";
                    }
                    else
                    {
                        request.Add("WORKDIV", "INOUTBINDIN");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.InoutViewSttsMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "반입기록물 목록 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //C702REQ반입 처리 요청
        /// </summary>
        /// <param name="request"></param>
        public void C702REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 반입
	         * 		서브업무명 : 반입 처리 요청
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		OUT_APPL_ID  : 반출서ID
	         * 		IN_NO		 : 반입서번호
	         * 		MGMT_ID  	 : 관리번호
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		OUT_APPL_ID  : 반출서ID
	         * 		IN_NO		 : 반입서번호
	         * 		MGMT_ID   	 : 관리번호
	         * 		RESULT_CD  	 : 결과코드
	         *  ===============================================
             *  
            */

            string sWORKDIV = "702";
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "INOUTPROCIN";
                    }
                    else
                    {
                        request.Add("WORKDIV", "INOUTPROCIN");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.InoutProcSttsMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    //JArray rows = null;
                    //if (response["LIST"] != null)
                    //{
                    //    rows = (JArray)response["LIST"];
                    //}

                    //response.Add("count", JToken.FromObject(rows.Count));
                    //response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "반입 처리 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //800REQ(미반입 목록 요청)
        /// </summary>
        /// <param name="request"></param>
        public void C800REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 미반입 현황
	         * 		서브업무명 : 미반입 목록 요청
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		USER_ID    : 검색구분
	         * 		SEARCH_TXT : 키워드
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		OUT_APPL_ID  	: 반출서ID
	         * 		APPL_ORG_NM    	: 신청자부서명
	         * 		APPL_USER_ID  	: 신청자ID
	         * 		APPL_USER_NM   	: 신청자명 
	         * 		OUT_TYPE 		: 반출구분
	         * 		OUT_RSN     	: 반출사유
	         * 		OUT_DT 		    : 반출일
	         * 		IN_SCHDL_DT		: 반입예정일
	         *  ===============================================
             *  
            */

            string sWORKDIV = "800";
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "INOUTNOTIN";
                    }
                    else
                    {
                        request.Add("WORKDIV", "INOUTNOTIN");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.InoutViewSttsMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "미반입 목록 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //801REQ(무단 반출입 기록물 목록 요청)
        /// </summary>
        /// <param name="request"></param>
        public void C801REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 미반입 현황
	         * 		서브업무명 : 미승인 무단 반출/입 목록 조회
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         *  ===============================================
             *  
            */

            string sWORKDIV = "801";
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    //if (request.ContainsKey("WORKDIV"))
                    //{
                    //    request["WORKDIV"] = "INOUTNOTIN";
                    //}
                    //else
                    //{
                    //    request.Add("WORKDIV", "INOUTNOTIN");
                    //}
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.NermnBindListMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "미승인 무단 반출/입 목록 조회 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //C802REQ(무단반출입처리요청 : 미승인 무단 반출/입 목록 결과 전송)
        /// </summary>
        /// <param name="request"></param>
        public void C802REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 미반입 현황
	         * 		서브업무명 : 미승인 무단 반출/입 목록 결과 전송
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         *  ===============================================
             *  
            */

            string sWORKDIV = "802";
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    //if (request.ContainsKey("WORKDIV"))
                    //{
                    //    request["WORKDIV"] = "INOUTNOTIN";
                    //}
                    //else
                    //{
                    //    request.Add("WORKDIV", "INOUTNOTIN");
                    //}
                    response = PamsClient.connect("com.hs.pams.common.server.bkroom.NpermnTrtmReceiveMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    //JArray rows = null;
                    //if (response["LIST"] != null)
                    //{
                    //    rows = (JArray)response["LIST"];
                    //}

                    //response.Add("count", JToken.FromObject(rows.Count));
                    //response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "미승인 무단 반출/입 목록 결과 전송 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //900(정수점검 계획서 목록 조회)
        /// </summary>
        /// <param name="request"></param>
        public void C900(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 정수 점검기
	         * 		서브업무명 : 정수점검 계획서 목록 조회
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		LIST
	         * 		  - PLAN_ID   : 계획서ID
	         * 		  - PCHRG_NM  : 담당자명
	         * 		  - PCHRG_ID  : 담당자ID
	         * 		  - TTL   	  : 계획서명 
	         *  ===============================================
             *  
            */

            string sWORKDIV = "900";
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "PRSVPLAN";
                    }
                    else
                    {
                        request.Add("WORKDIV", "PRSVPLAN");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.prsv.sta.RfidPrsvPlanMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "정수점검 계획서 목록 조회 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        /// <summary>
        /// //901REQ(정수점검 계획서 기록물 목록 조회)
        /// </summary>
        /// <param name="request"></param>
        public void C901REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 정수 점검기
	         * 		서브업무명 : 정수점검 계획서 목록 조회
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		LIST
	         * 		  - PLAN_ID   : 계획서ID
	         * 		  - PCHRG_NM  : 담당자명
	         * 		  - PCHRG_ID  : 담당자ID
	         * 		  - TTL   	  : 계획서명 
	         *  ===============================================
             *  
            */

            string sWORKDIV = "901";
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "PRSVPLANBIND";
                    }
                    else
                    {
                        request.Add("WORKDIV", "PRSVPLANBIND");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.prsv.sta.RfidPrsvPlanMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    JArray rows = null;
                    if (response["LIST"] != null)
                    {
                        rows = (JArray)response["LIST"];
                    }

                    response.Add("count", JToken.FromObject(rows.Count));
                    response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "정수점검 계획서 기록물 목록 조회 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }


        /// <summary>
        /// //902REQ(정수점검 완료처리)
        /// </summary>
        /// <param name="request"></param>
        public void C902REQ(JObject request)
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 정수 점검기
	         * 		서브업무명 : 정수점검 결과 처리
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		PLAN_ID : 계획서ID
	         * 		TASK_NM : 작업자
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		LIST
	         * 		  - PAMS_MGMT_NO   	: 관리번호
	         * 		  - TASK_DT  		: 검검일시
	         * 		  - INSP_RSLT  		: 점검상태		0:정상, 1:이상
	         * 		  - INSP_TAG   	  	: 태그상태
	         * 		  - BIND_STTS		: 기록물상태
	         *  ===============================================
             *  
            */

            string sWORKDIV = "902";
            JObject response = null;

            try
            {
                lock (oClientSocketCO)
                {
                    if (request.ContainsKey("WORKDIV"))
                    {
                        request["WORKDIV"] = "PRSVPLANRST";
                    }
                    else
                    {
                        request.Add("WORKDIV", "PRSVPLANRST");
                    }
                    response = PamsClient.connect("com.hs.pams.common.server.prsv.sta.RfidPrsvPlanMsg", request);

                    if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                    else response.Add("WORKDIV", sWORKDIV + "REP");

                    //JArray rows = null;
                    //if (response["LIST"] != null)
                    //{
                    //    rows = (JArray)response["LIST"];
                    //}

                    //response.Add("count", JToken.FromObject(rows.Count));
                    //response.Add("rows", JToken.FromObject(rows));

                    string sRtn = response.ToString();

                    SendResponse_New(sRtn, sWORKDIV + "REQ");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "정수점검 결과 처리 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }
        }

        //RFIDController


        

        
        /// <summary>
        /// 공통 : PING
        /// </summary>
        /// <param name="request"></param>
        public void C999REQ(JObject request)
        {
            string sWORKDIV = "999";

            Hashtable htParamsHashtable;

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            try
            {
                lock (oClientSocketCO)
                {
                    htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();
                    htParamsHashtable.Add("IN_IP", sIP);

                    ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.PING_CHECK", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                    strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                    strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                    Dictionary<string, object> aData = new Dictionary<string, object>();
                    aData.Add("WORKDIV", sWORKDIV + "REP");

                    if (strAppCode.Equals("0"))
                    {
                        aData.Add("RESULT", "0");
                        aData.Add("ERROR", "");
                    }
                    else
                    {
                        aData.Add("RESULT", "-1");
                        aData.Add("ERROR", strAppMsg);

                        

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
                Program.SysLog.Write(this, "공통 PING 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
            }

        }



        // 하나의 패킷을 파싱
        public void ParsePacket(byte[] packetBuffer)
        {
            //string strRecvData;
            //string strCommandCode;
            //DateTime dteParsePacket;
            //TimeSpan gap;

            //try
            //{
            //    //if (sIP == "10.49.0.207")
            //    //{
            //    //    return;
            //    //}

            //    //if (MiddlewareID == "HCW00002")
            //    //{
            //    //    return;
            //    //}



            //    strRecvData = EdASCII.GetString(packetBuffer, 1, packetBuffer.Length - 2);

            //    strCommandCode = strRecvData.Substring(0, 2);
            //    switch (strCommandCode)
            //    {
            //        //통신 상태 체크
            //        case "AA":
            //            //((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strRecvData + ", 통신 상태 체크");
            //            //((MainForm)(Application.OpenForms["MainForm"])).AddLogList("통신 상태 체크");
            //            SendResponse("BB", "0", "SUCCESS");

            //            break;

            //        //자격 List 주기적 Update
            //        case "LU":
            //            //((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strRecvData + ", 자격 List(LU) 요청");

            //            //LU 주기적 업데이트를 시작하면 마지막 LR 업데이트는 중지 한다.
            //            if (_bProcLR)
            //            {
            //                _bProcLR = false;
            //                _lstCardInfo.Clear();
            //            }

            //            CardListLU(strRecvData);
            //            //((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strRecvData + ", 자격 List(LU) 요청");

            //            break;
            //        // 인증
            //        case "CR":
            //            dteParsePacket = DateTime.Now;
            //            gap = dteParsePacket - dteCRCheckTime;
            //            if (gap.TotalSeconds > 1)
            //            {
            //                ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strRecvData + ", 인증");
            //                TagCertification(strRecvData);
            //                dteCRCheckTime = dteParsePacket;
            //            }
            //            else
            //            {
            //                //((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strRecvData + ", 인증(처리 제외 :" + gap.Seconds.ToString() + ")");
            //                //((MainForm)(Application.OpenForms["MainForm"])).AddLogList("인증(처리 제외 :" + gap.Seconds.ToString() + ")" + dteParsePacket.ToString("yyyy-MM-dd HH:mm:ss") + "," + dteCRCheckTime.ToString("yyyy-MM-dd HH:mm:ss"));
            //                Program.AppLog.Write("인증(처리 제외: " + gap.Seconds.ToString() + ")" + dteParsePacket.ToString("yyyy - MM - dd HH: mm:ss") + ", " + dteCRCheckTime.ToString("yyyy - MM - dd HH: mm:ss"));
            //            }
            //            break;
            //        //// 운행/조작
            //        case "OP":
            //            //((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strRecvData + ", 운행/조작");
            //            ControlState(strRecvData);
            //        //    ProcessElectronicDisplayInfo(strRecvData);
            //            break;
            //        //// 감지
            //        case "US":
            //            //((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strRecvData + ", 감지");
            //            SensingState(strRecvData);
            //            //    ProcessPOBInfo(strRecvData);
            //            break;
            //        //// 자격 List
            //        case "LR":

            //            if (!_bProcLR)
            //            {
            //                    ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strRecvData + ", 자격 List 요청");
            //                    CardListLR(strRecvData);
            //            }
            //            //gap = dteParsePacket - dteLRCheckTime;
            //            //if (gap.TotalSeconds > 1)
            //            //{
            //            //    ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strRecvData + ", 자격 List 요청");
            //            //    CardListLR(strRecvData);
            //            //    dteLRCheckTime = dteParsePacket;
            //            //}
            //            //else
            //            //{
            //            //    //((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA 자격 List 요청(처리 제외 :" + gap.Seconds.ToString() + "):" + strRecvData + ", 자격 List 요청(처리 제외 :" + gap.Seconds.ToString() + ")");
            //            //}
            //            break;
            //        //환경 정보
            //        case "ER":
            //            //((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strRecvData + ", 환경 정보 요청");
            //            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("(" + sMWID + ")" +  "환경 정보 요청");
            //            BaseInfo(strRecvData);
            //            break;
            //        //주기적 업데이트 완료 구문
            //        case "ZZ":

            //            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("LU 업데이트 완료");
            //            if (_bProcLR)
            //            {
            //                if (_lstCardInfo.Count > 0)
            //                {
            //                    SendResponse(_lstCardInfo[0], "0", "SUCCESS");
            //                    RecieveDataSave(DateTime.Now.ToString("yyyyMMddHHmmss"), "SND", sMWIDLR, "LU", _lstCardInfo[0]);
            //                    ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("SND DATA :" + _lstCardInfo[0] + ", 자격 List UL SEND");
            //                    _lstCardInfo.RemoveAt(0);
            //                }

            //                if (_lstCardInfo.Count == 0)
            //                {
            //                    //int iDeleyCnt = 0;
            //                    //while (_bMF == false && iDeleyCnt < 5)
            //                    //{
            //                        //처리 완료 Send
            //                        int iListMax = 0;
            //                        int iListNo = 0;
            //                        string sCardListLR = "";
            //                        sCardListLR = sCardListLR.PadRight(240, '0');
            //                        SendResponse("LR" + iListMax.ToString().PadLeft(3, '0') + iListNo.ToString().PadLeft(3, '0') + sCardListLR, "0", "SUCCESS");
            //                        RecieveDataSave(DateTime.Now.ToString("yyyyMMddHHmmss"), "SND", sMWIDLR, "LR", "LR" + iListMax.ToString().PadLeft(3, '0') + iListNo.ToString().PadLeft(3, '0') + sCardListLR);
            //                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("SND DATA :" + "LR" + iListMax.ToString().PadLeft(3, '0') + iListNo.ToString().PadLeft(3, '0') + sCardListLR + ", 자격 List SEND");

            //                    //Delay(3000);
            //                    //iDeleyCnt++;
            //                    //}
            //                }
            //            }
            //            break;


            //        // 자격 리스트 FTP 전체 자격 리스트 파일 업데이트 유무 체크
            //        case "LS":

            //            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("자격 List 다량 업데이트 유무 체크 요청");
            //            LSInfo(strRecvData);
            //            break;


            //        // 펌웨어 SW Version 체크
            //        case "SC":

            //            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("펌웨어 SW Version 체크 요청");
            //            SCInfo(strRecvData);
            //            break;

            //        // 펌웨어 SW Version 체크
            //        case "ST":

            //            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("Status Report 전송");
            //            StatusReport(strRecvData);
            //            break;

            //        //// 장비 오류 정보 전송
            //        //case "DB-5":
            //        //    ProcessDevError(strRecvData);
            //        //    break;
            //        // 환경설정 파일 요청

            //        //// 헬스 체크 프로토콜 처리(통신상태 확인)
            //        //case "DB-F":
            //        //    ProcessHealthCheck(strRecvData);
            //        //    break;

            //        default:
            //            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList(strRecvData);
            //            break;
            //    }

            //}
            //catch (Exception e)
            //{
            //    Program.SysLog.Write(this, "프로토콜 파싱 오류", e);

            //}
        }






        private DateTime Delay(int MS)

        {

            DateTime ThisMoment = DateTime.Now;

            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);

            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)

            {

                System.Windows.Forms.Application.DoEvents();

                ThisMoment = DateTime.Now;

            }

            return DateTime.Now;

        }



        public void ProcessServerTime(string strRecvData)
        {
            string[] strSplit;
            string strHeader;
            string strHeaderFront;
            string strHeaderRear;
            string strData;
            string strTail;
            Hashtable htParamsHashtable;
            string strFunctionResult = "";
            DataSet dtResultDataSet;
            string strMiddlewareID;
            string strAppCode;
            string strAppMsg;

            try
            {
                lock (oClientSocketCO)
                {

                    strMiddlewareID = strRecvData.Split(new char[] { '\\' })[0].Split(new char[] { ',' })[4];
                    if (string.IsNullOrEmpty(sMWID))
                    {
                        AddMiddlewareConnectHistory(strMiddlewareID, true);
                        sMWID = strMiddlewareID;
                    }
                    strSplit = strRecvData.Split(new char[] { '\\' });
                    htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();
                    //htParamsHashtable.Add("I_DATA", strRecvData);
                    //                strFunctionResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteFunctionString("POBP_COMM.FC_GET_DATE", htParamsHashtable);
                    dtResultDataSet = null;
                    switch (Properties.Settings.Default.DB_Type)
                    {
                        case "ORACLE":
                            dtResultDataSet = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectQuery("SELECT TO_CHAR(SYSTIMESTAMP, 'YYYYMMDDHH24MISSFF3') FROM DUAL", 1);
                            strFunctionResult = dtResultDataSet?.Tables[0].Rows[0][0].ToString();
                            break;
                        case "SQLSERVER":
                            dtResultDataSet = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectQuery("SELECT dbo.FC_STR_GETDATE('MS')", 1);
                            strFunctionResult = dtResultDataSet?.Tables[0].Rows[0][0].ToString();
                            break;
                        default:
                            strFunctionResult = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            break;
                    }
                    strHeader = strSplit[0];
                    strData = strFunctionResult + ",\\";
                    strTail = strSplit[1] + "\\";
                    strSplit = strHeader.Split(new char[] { ',' });
                    strHeaderFront = strSplit[0] + "," + strSplit[1] + "," + strSplit[2] + ",";
                    strHeaderRear = "," + strSplit[4] + "," + strSplit[5] + ",\\";
                    strHeaderRear = strHeaderRear + strData + strTail;
                    strData = strHeaderFront + (strHeaderFront.Length + strHeaderRear.Length + 4).ToString("D4") + strHeaderRear;
                    SendResponse(strData, "0", "SUCCESS");
                    ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("MW : " + strMiddlewareID + ", 시간요청");
                    strAppCode = "0";
                    strAppMsg = "SUCCESS";
                    htParamsHashtable.Clear();
                    htParamsHashtable.Add("I_APP_CODE", strAppCode);
                    htParamsHashtable.Add("I_APP_MSG", strAppMsg);
                    htParamsHashtable.Add("I_DATA", strRecvData);
                    ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("POBP_EQ_ERROR_HIST.PD_EQ_ERROR_HIST_DATA_RCV", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "시간동기화 프로토콜 처리 오류", e);
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

        public void SendResponse(string strRecvData, string strErrorCode, string strErrorMessage, string sLogAddMsg)
        {
            string strSendBuffer;
            byte[] bPacketBuffer;
            int nSendByte;
            try
            {
                strSendBuffer = strRecvData;
                bPacketBuffer = MakePacket(strSendBuffer);
                nSendByte = clientSocket.Send(bPacketBuffer);
                Program.AppLog.Write("Send : [" + EdASCII.GetString(bPacketBuffer) + "(" + sLogAddMsg + ")" + "]");

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

        private void TMCalc(double dlati, double longi, out double dTmX, out double dTmY)
        {
            double dValX = 0, dValY = 0;

            ConvertCoordinates.LL2Six(dlati, longi, out dValX, out dValY);
            ConvertCoordinates.WGS_TM(ID.TM_EAST, dValY, dValX, out dTmX, out dTmY);

        }

        public void Test()
        {
            string strJson = "";
            DataTable dtTest = JsonConvert.DeserializeObject<DataTable>(strJson);
        }
    }
}
