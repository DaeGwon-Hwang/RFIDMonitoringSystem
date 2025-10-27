
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

    class MwClientSocketHandler : IDisposable
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
        public MwClientSocketHandler()
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

        public MwClientSocketHandler(Socket acceptedSocket)
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
            MwClientSocketHandler clientSocketHandler = (MwClientSocketHandler)ar.AsyncState;
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

                            _packetBuffer = Combine(_packetBuffer, ReceivceBuffer);
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
                //JObject result = new JObject();
                //result = JObject.Parse(strRecvData);

                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(strRecvData);

                strCommandCode = result["WORKDIV"].ToString();

                switch (strCommandCode)
                {
                    //1130
                    case "1130":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : MW : 파일 전송");
                        C1130(result);
                        break;

                    //1210
                    case "1210":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : MW : 고정형 리더기 상태");
                        C1210(result);
                        break;

                    //1400
                    case "1400":
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList("RCV DATA :" + strCommandCode + " : MW : 태그정보");
                        C1130(result);
                        break;

                    default:
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList(strRecvData);
                        Program.AppLog.Write("Case default : " + strRecvData);
                        break;
                }

            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "프로토콜 파싱 오류", e);
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

    }
}
