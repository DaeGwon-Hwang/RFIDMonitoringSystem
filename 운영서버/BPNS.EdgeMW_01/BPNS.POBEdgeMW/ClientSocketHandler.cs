using System;
using System.Text;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Collections;
using System.Data;
using System.Xml;

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace BPNS.EdgeMW
{
    class ClientSocketHandler : IDisposable
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

        int iBufferSize = 4096;
        int iTimeOut = 2000;

        #endregion

        #region Constructor(생성자)
        public ClientSocketHandler()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ReceivceBuffer = new byte[iBufferSize];
            ReceiveBufferSize = 0;
            ReceiveBufferOffset = 0;
            parsingBuffer = new byte[iBufferSize];
            parsingBufferOffset = 0;
            parsingBufferSize = 0;

            dteCRCheckTime = DateTime.Now;
            dteLRCheckTime = DateTime.Now;
        }

        public ClientSocketHandler(Socket acceptedSocket)
        {
            // 소켓이 연결을 수락할때 처리
            // Accept된 Socket 저장
            clientSocket = acceptedSocket;
            // 소켓 옵션 설정(사용중인 주소 재사용 불가)
            //clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //clientSocket.LingerState = new LingerOption(true, 0);
            ReceivceBuffer = new byte[iBufferSize];
            ReceiveBufferSize = 0;
            ReceiveBufferOffset = 0;
            parsingBuffer = new byte[iBufferSize];
            parsingBufferOffset = 0;
            parsingBufferSize = 0;
            connectedDateTime = DateTime.Now;

            /////Program.AppLog.Write("소켓 연결[Handler:" + acceptedSocket.Handle.ToInt64().ToString() + "]");

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
                if ((DateTime.Now - lastCommunicationTime) < new TimeSpan(0, 0, 0, 0, iTimeOut))
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                /////Program.AppLog.Write(this, "Socket Timeout Check Exception!", e);
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
                clientSocket.BeginReceive(ReceivceBuffer, ReceiveBufferOffset, iBufferSize - ReceiveBufferSize - 1, 0, new AsyncCallback(AsyncReceiveCallback), this);
            }
            catch (ObjectDisposedException ode)
            {
                /////Program.AppLog.Write(this, "socket closed!");
                /////Program.SysLog.Write(this, "socket closed!(ObjectDisposedException!", ode);
            }
            catch (SocketException se)
            {
                if (!clientSocket.Connected)
                {
                    /////Program.AppLog.Write(this, "connection closed by peer![" + se.Message + "]");
                }
                else
                {
                    /////Program.SysLog.Write(this, "socket error!", se);
                }
            }
            catch (Exception e)
            {
                /////Program.SysLog.Write(this, "setAsyncReceive 오류", e);
            }
        }

        int _nPacketSize = 0;
        byte[] _packetBuffer;

        public void AsyncReceiveCallback(IAsyncResult ar)
        {
            ClientSocketHandler clientSocketHandler = (ClientSocketHandler)ar.AsyncState;
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
                        /////Program.AppLog.Write(this, "Connection Closed by peer!");
                        return;
                    }
                    else
                    {
                        SetLastCommTime();

                        if (_nPacketSize > 0)
                        {
                            byte[] _packetBuffer_temp = new byte[iBufferSize];
                            System.Buffer.BlockCopy(ReceivceBuffer, ReceiveBufferOffset, _packetBuffer_temp, 0, nRead);

                            _packetBuffer = Combine(_packetBuffer, ReceivceBuffer);
                        }
                        else
                        {
                            _packetBuffer = new byte[iBufferSize];

                            //System.Buffer.BlockCopy(ReceivceBuffer, ReceiveBufferOffset, parsingBuffer, parsingBufferOffset + parsingBufferSize, nRead);
                            System.Buffer.BlockCopy(ReceivceBuffer, ReceiveBufferOffset, _packetBuffer, _nPacketSize, nRead);
                        }

                        //parsingBufferSize += nRead;
                        /////Program.AppLog.Write("Recv :(" + sMWID + ")[" + EdASCII.GetString(clientSocketHandler.ReceivceBuffer, clientSocketHandler.ReceiveBufferOffset, nRead) + "]");
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
                    /////Program.AppLog.Write(this, "소켓 접속 끊김[" + se.Message + "]");
                    //                    Program.SysLog.ExceptionLogWrite(this, "Connection Closed by peer", se);
                }
                else
                {
                    /////Program.AppLog.Write(this, "소켓 오류![" + se.Message + "]");
                    /////Program.SysLog.Write(this, "소켓 오류!", se);
                }

                // return 하기 전에 연결 끊어짐에 대한 처리 루틴 추가

                return;
            }
            catch (Exception e)
            {
                /////Program.SysLog.Write(this, "AsyncReceiveCallback[BufferOffset:" + ReceiveBufferOffset.ToString() + ",parsingBufferOffset:" + parsingBufferOffset.ToString() + ", parsingBufferSize:" + parsingBufferSize.ToString() + ",nRead:" + nRead.ToString() + "]", e);
            }

            // Parse Receive Data
            try
            {
                clientSocketHandler.ClientSocket.BeginReceive(ReceivceBuffer, ReceiveBufferOffset, iBufferSize - ReceiveBufferSize - 1, 0, new AsyncCallback(AsyncReceiveCallback), clientSocketHandler);
            }
            catch (SocketException se)
            {
                /////Program.AppLog.Write(this, "소켓 오류![" + se.Message + "]");
                /////Program.SysLog.Write(this, "소켓 오류!", se);
            }
            catch (ObjectDisposedException ode)
            {
                /////Program.AppLog.Write(this, "소켓 접속 끊김![" + ode.Message + "]");
                /////Program.SysLog.Write(this, "소켓 접속 끊김!", ode);
            }
            catch (Exception e)
            {
                /////Program.AppLog.Write(this, "Unexpected exception![" + e.Message + "]");
                /////Program.SysLog.Write(this, "Unexpected exception!", e);
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
                    /////Program.SysLog.Write(this, "Unexpected Exception!", e);
                }
                return bPacketParsed;
            }
        }

        public void ParsePacketCheck()
        {
            string strRecvData = EdASCII.GetString(_packetBuffer, 0, _packetBuffer.Length);

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
            JObject result = new JObject();
            result = JObject.Parse(strRecvData);

            ((MainForm)(Application.OpenForms["MainForm"])).lstJOData.Add(result);

            Dictionary<string, string> aData = new Dictionary<string, string>();
            aData.Add("RESULT", "OK");
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

            SendResponse_New(sRtn, "004REQ");
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
                /////Program.AppLog.Write("Send : [" + EdASCII.GetString(bPacketBuffer) + "]");

            }
            catch (SocketException se)
            {
                /////Program.SysLog.Write(this, "SendResponse 함수 오류", se);
            }
            catch (ObjectDisposedException ode)
            {
                /////Program.SysLog.Write(this, "SendResponse 함수 오류", ode);
            }
            catch (Exception e)
            {
                /////Program.SysLog.Write(this, "SendResponse 함수 오류", e);
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
                /////Program.AppLog.Write("Send : [" + EdASCII.GetString(bPacketBuffer) + "(" + sLogAddMsg + ")" + "]");

            }
            catch (SocketException se)
            {
                /////Program.SysLog.Write(this, "SendResponse 함수 오류", se);
            }
            catch (ObjectDisposedException ode)
            {
                /////Program.SysLog.Write(this, "SendResponse 함수 오류", ode);
            }
            catch (Exception e)
            {
                /////Program.SysLog.Write(this, "SendResponse 함수 오류", e);
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
                /////Program.AppLog.Write("Send : [" + EdASCII.GetString(bPacketBuffer) + "(" + sLogAddMsg + ")" + "]");
                clientSocket.Close();

            }
            catch (SocketException se)
            {
                /////Program.SysLog.Write(this, "SendResponse 함수 오류", se);
            }
            catch (ObjectDisposedException ode)
            {
                /////Program.SysLog.Write(this, "SendResponse 함수 오류", ode);
            }
            catch (Exception e)
            {
                /////Program.SysLog.Write(this, "SendResponse 함수 오류", e);
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
                /////Program.AppLog.Write(this, "Unexpected exception!", e);
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
            //Hashtable htParamsHashtable;
            //int nProcessRowCount;
            //string strErrorDT;
            //string strErrorTM;
            //EQErrorState esErrorState;

            //if (string.IsNullOrEmpty(pstrMiddlewareID))
            //{
            //    return;
            //}
            //try
            //{
            //    lock (oClientSocketCO)
            //    {
            //        htParamsHashtable = new Hashtable();
            //        htParamsHashtable.Clear();
            //        strErrorDT = DateTime.Now.ToString("yyyyMMdd");
            //        strErrorTM = DateTime.Now.ToString("HHmmssfff");
            //        htParamsHashtable.Add("IN_CAR_NO", pstrMiddlewareID);
            //        htParamsHashtable.Add("IN_LAST_COMM_DTM", strErrorDT + strErrorTM);
            //        htParamsHashtable.Add("IN_LAST_COMM_STATE", (bConnect ? "Y" : "N"));

            //        nProcessRowCount = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PKG_HOCCOM_EXP_200717.COMM_STATE_SAVE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
            //        if (htParamsHashtable.ContainsKey("O_APP_CODE"))
            //        {
            //            if (!htParamsHashtable["O_APP_CODE"].ToString().Equals("0"))
            //            {
            //                ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("연결상태 DB 처리 오류[EQ ID:" + pstrMiddlewareID + ",NORMAL_YN:" + (bConnect ? "Y" : "N") + ",APP_CODE:" + htParamsHashtable["O_APP_CODE"].ToString() + ",APP_MSG:" + htParamsHashtable["O_APP_MSG"].ToString() + "]");
            //                Program.AppLog.Write("연결상태 DB 처리 오류[EQ ID:" + pstrMiddlewareID + ",NORMAL_YN:" + (bConnect ? "Y" : "N") + ",APP_CODE:" + htParamsHashtable["O_APP_CODE"].ToString() + ",APP_MSG:" + htParamsHashtable["O_APP_MSG"].ToString() + "]");
            //            }
            //        }
            //        else
            //        {
            //            ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("연결상태 DB 처리 오류[EQ ID:" + pstrMiddlewareID + ",NORMAL_YN:" + (bConnect ? "Y" : "N") + "]");
            //            Program.AppLog.Write("연결상태 DB 처리 오류[EQ ID:" + pstrMiddlewareID + ",NORMAL_YN:" + (bConnect ? "Y" : "N") + "]");
            //        }

            //        if (nProcessRowCount == -1000)
            //        {
            //            Program.AppLog.Write(this, "DB 연결 오류");
            //            //sendResponse(sSSID, sCommandCode, "0020", ErrorCode.DATABASE_CONNECT_ERROR, "");
            //            ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.Reconnect();
            //            return;
            //        }
            //        lock (((MainForm)(Application.OpenForms["MainForm"])).EQErrorStateCO)
            //        {
            //            esErrorState = ((MainForm)(Application.OpenForms["MainForm"])).GetEQErrorState(pstrMiddlewareID);
            //            if (esErrorState == null)
            //            {
            //                ((MainForm)(Application.OpenForms["MainForm"])).AddEQErrorState(new EQErrorState(pstrMiddlewareID, bConnect ? EnumEQState.Normal : EnumEQState.Disconnected, (bConnect ? "0000" : "0999"), (bConnect ? "연결됨" : "연결끊김"), strErrorDT + strErrorTM));
            //            }
            //            else
            //            {
            //                esErrorState.SetEQErrorState(pstrMiddlewareID, bConnect ? EnumEQState.Normal : EnumEQState.Disconnected, (bConnect ? "0000" : "0999"), (bConnect ? "연결됨" : "연결끊김"), strErrorDT + strErrorTM);
            //            }
            //        }
            //    }

            //}
            //catch (Exception e)
            //{
            //    Program.SysLog.Write(this, "연결 처리 오류", e);
            //}

        }

    }
}
