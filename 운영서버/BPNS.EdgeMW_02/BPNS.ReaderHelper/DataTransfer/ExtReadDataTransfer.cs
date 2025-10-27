
using BPNS.COM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BPNS.ReaderHelper.DataTransfer
{
    //class ExtReadDataTransfer
    //{
    //}

    internal static class NativeMethods
    {
        [DllImport("kernel32")]
        internal static extern bool SetSystemTime(ref SYSTEMTIME lpSystemTime);
        [DllImport("kernel32")]
        internal static extern void GetSystemTime(out SYSTEMTIME lpSystemTime);
        internal struct SYSTEMTIME
        {
            internal short Year;
            internal short Month;
            internal short DayOfWeek;
            internal short Day;
            internal short Hour;
            internal short Minute;
            internal short Second;
            internal short Milliseconds;
        }

    }

    public class ExtReadDataTransfer
    {
        #region 전역 변수
        private const byte StartOfText = 0x02;
        private const byte EndOfText = 0x03;

        //private static CRC16 oCrc16 = new CRC16();

        private Socket sckClient;
        //private IEnumerable<TransferDataContainer<TagData>> lTransferTagData;
        Encoding utf8;

        private int nTransferCount = 1;

        private DateTime dtLastTimeSyncDate;
        TimeSpan tsTime;

        private int iSendErr = 0;

        /// <summary>
        /// 서버와 연결 상태
        /// </summary>
        public static bool bConnection = false;
        private bool bFirstConnection = true;

        public bool bWork = false;

        public int iErrorCnt = 0;

        public string _sLastSendTag = string.Empty;
        public string _sLastSendTagDTM = string.Empty;
        public DateTime _dteLastDTM;


        #endregion

        public ExtReadDataTransfer()
        {
            try
            {
                dtLastTimeSyncDate = DateTime.MinValue;

            }
            catch (Exception ex)
            {
                //Program.SysLog.Write(this, "ReadDataTransfer 생성자 Exception", ex);
            }
        }


        #region 소켓 통신, 데이터 전송
        /// <summary>
        /// 서버로 데이터 전송
        /// </summary>
        public bool ReadDataSender(TagReport trReport)
        {
            bool _bSendData = true;

            utf8 = System.Text.Encoding.UTF8;

            bool bChk = false;

            bool rtn = false;

            bWork = true;

            //1. 서버 연결 상태 체크
            bChk = ConnectionCheck();


            if (!bChk || iSendErr >= 5)
            {//연결이 끊긴 경우 처리
                if (!ConnectServer(ProgramSettings.ServerIP, ProgramSettings.ServerPort))
                {//연결이 끊긴 경우 처리
                    if (bConnection == true || bFirstConnection == true)
                    {
                        /////Program.AppLog.Write("서버와 연결실패");
                        /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("서버와 연결실패");
                        //AddDeviceError(ProgramSettings.MiddlewareID, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmssfff"), "N", string.Format("{0:D4}", (int)DevErrorCode.Disconnected), "연결실패");
                        bConnection = false;
                    }

                    if (bFirstConnection == true) bFirstConnection = false;
                    //Program.AppLog.Write("ReadDataSender 연결실패");
                    bWork = false;
                    return rtn;
                }
            }

            //Program.AppLog.Write("ReadDataSender 연결설공");

            //서버연결 성공일 경우 로그 처리
            if (bConnection == false || bFirstConnection == true)
            {//이전 상태가 서버연결 실패 또는 최초 서버 연결일 경우 로그 처리
                /////Program.AppLog.Write("서버와 연결성공");
                /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("서버와 연결성공");
                //AddDeviceError(ProgramSettings.MiddlewareID, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmssfff"), "Y", string.Format("{0:D4}", (int)DevErrorCode.Connected), "연결성공");
                bConnection = true;
            }

            if (bFirstConnection == true) bFirstConnection = false;




            //string sTime = Convert.ToString(p_dt.Compute("max(time)", string.Empty));
            //DataRow[] dr = p_dt.Select("[Time] > '" + DateTime.Now.AddSeconds(-10).ToString("yyyy-MM-dd HH:mm:ss.fff") + "' and " + "time ='" + sTime + "' and UPD_YN <>'Y'");

            //DateTime dtm01;// = Convert.ToDateTime(_sLastSendTagDTM);
            //DateTime dtm02;// = Convert.ToDateTime(sTime);

            //DateTime.TryParse(_sLastSendTagDTM, out dtm01);
            //DateTime.TryParse(sTime, out dtm02);

            try
            {
                #region 23.12.03 주석 처리
                //if (trReport.TagNo.Length > 0 && trReport.BSendOk == false)
                //{
                //    _bSendData = true;
                //    DateTime t1 = trReport.DteTime;
                //    DateTime t2 = DateTime.Now;
                //    TimeSpan TS = t2 - t1;
                //    double diffDay = TS.TotalSeconds;
                //    if (diffDay > 10)
                //    {
                //        /////Program.AppLog.Write("Tag 인식 시간 초과(10초) : " + trReport.TagNo + " Time : " + trReport.STime);
                //        /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("Tag 인식 시간 초과(10초) : " + trReport.TagNo + " Time : " + trReport.STime);
                //        trReport.Clear();
                //    }
                //    else
                //    {
                //        //Program.AppLog.Write("보낼 데이터 있음");
                //        string sTagData = trReport.TagNo;

                //        //서버에 전송시간, 전송 여부 셋팅
                //        trReport.DteSendServer = DateTime.Now;
                //        trReport.BSendServer = true;

                //        //Program.AppLog.Write(sTagData);
                //        int iSendResult = SendTagData(sTagData);

                //        if (iSendResult > 0)
                //        {
                //            trReport.SetSend(sTagData);
                //            _sLastSendTag = sTagData;
                //            _sLastSendTagDTM = trReport.STime;
                //            _dteLastDTM = trReport.DteTime;

                //            /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("서버 Send Tag : " + _sLastSendTag + " Time : " + _sLastSendTagDTM);
                //            /////Program.AppLog.Write("서버 Send 성공 Tag : " + _sLastSendTag + " 인식 Time : " + _sLastSendTagDTM);


                //            //trReport.Clear();
                //            //p_dt.Rows.Clear();
                //            //p_dt.AcceptChanges();

                //        }
                //        else
                //        {
                //            /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("서버 Send 실패 Tag : " + sTagData + " 인식 Time : " + trReport.STime);
                //            /////Program.AppLog.Write("서버 Send 실패 Tag : " + sTagData + " 인식 Time : " + trReport.STime);
                //        }
                //    }
                //    //if (iSendResult > 0)
                //    //{
                //    //    p_dt.Rows.Clear();
                //    //    p_dt.AcceptChanges();
                //    //}
                //}
                //else
                //{
                //    /////Program.AppLog.Write("보낼 데이터 없음");
                //    _bSendData = false;
                //    trReport.Clear();
                //    //p_dt.Rows.Clear();
                //    //p_dt.AcceptChanges();
                //    //Program.AppLog.Write("보낼 데이터 없음");
                //} 
                #endregion
                rtn = true;
            }
            catch (Exception ex)
            {
                /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("Server Tag 전송 실패 Exception : " + ex.ToString());
                /////Program.SysLog.Write(this, "ReadDataSender Send 데이터 처리 에러", ex);
                rtn = false;
            }
            finally
            {
                Thread.Sleep(2000);
                bWork = false;

            }

            return rtn;
        }

        public bool ReadDataSenderDB(TagReport trReport, string sGateID)
        {
            bool _bSendData = true;

            utf8 = System.Text.Encoding.UTF8;

            bool bChk = false;

            bool rtn = false;

            bWork = true;

            ////1. DB연결 체크
            //bChk = DbConnectChk();

            //if (!bChk)
            //{
            //    if (bConnection)
            //    {
            //        bConnection = false;
            //        Program.AppLog.Write("서버(DB)와 연결실패");
            //        ((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("서버(DB)와 연결실패");
            //        iSendErr++;
            //    }
            //    else
            //    {
            //        iSendErr++;
            //    }
            //    return rtn;

            //}
            //else
            //{
            //    if (!bConnection)
            //    {
            //        Program.AppLog.Write("서버(DB)와 연결성공");
            //        ((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("서버(DB)와 연결성공");
            //    }
            //}


            //try
            //{
            //    if (trReport.TagNo.Length > 0 && trReport.BSendOk == false)
            //    {
            //        _bSendData = true;
            //        DateTime t1 = trReport.DteTime;
            //        DateTime t2 = DateTime.Now;
            //        TimeSpan TS = t2 - t1;
            //        double diffDay = TS.TotalSeconds;
            //        if (diffDay > 10)
            //        {
            //            Program.AppLog.Write("Tag 인식 시간 초과(10초) : " + trReport.TagNo + " Time : " + trReport.STime);
            //            ((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("Tag 인식 시간 초과(10초) : " + trReport.TagNo + " Time : " + trReport.STime);
            //            trReport.Clear();
            //        }
            //        else
            //        {



            //            //Program.AppLog.Write("보낼 데이터 있음");
            //            string sTagData = trReport.TagNo;

            //            //23.09.27 로직 추가(서영준차장 메일로 중복으로 데이터 들어오는것 제외 요청)
            //            //5초안에 보낸 데이터가 동일한 태그일 경우 제외
            //            if (sTagData == _sLastSendTag)
            //            {
            //                DateTime tLastDtm = _dteLastDTM;    //이전에 인식된 시각
            //                DateTime tNow = DateTime.Now;         //현재시작
            //                TimeSpan TS2 = tNow - tLastDtm;
            //                double diffSec2 = TS.TotalSeconds;
            //                if (diffDay <= 5)
            //                {
            //                    Program.AppLog.Write("이전 인식 태그와 동일 : " + diffSec2.ToString() + "초 " + sTagData + " Time : " + trReport.STime);
            //                    ((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("이전 인식 태그와 동일 : " + diffSec2.ToString() + "초 " + sTagData + " Time : " + trReport.STime);
            //                    trReport.Clear();
            //                    return true;
            //                }
            //            }



            //            //서버에 전송시간, 전송 여부 셋팅
            //            trReport.DteSendServer = DateTime.Now;
            //            trReport.BSendServer = true;

            //            //Program.AppLog.Write(sTagData);
            //            //int iSendResult = SendTagData(sTagData);
            //            int iSendResult = InsertData(sGateID, sTagData);

            //            if (iSendResult > 0)
            //            {
            //                trReport.SetSend(sTagData);
            //                _sLastSendTag = sTagData;
            //                _sLastSendTagDTM = trReport.STime;
            //                _dteLastDTM = trReport.DteTime;

            //                ((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("서버 Send Tag : " + _sLastSendTag + "(" + sGateID + ")" + " Time : " + _sLastSendTagDTM);
            //                Program.AppLog.Write("서버 Send 성공 Tag : " + _sLastSendTag + "(" + sGateID + ")" + " 인식 Time : " + _sLastSendTagDTM);


            //                //trReport.Clear();
            //                //p_dt.Rows.Clear();
            //                //p_dt.AcceptChanges();

            //            }
            //            else
            //            {
            //                ((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("서버 Send 실패 Tag : " + sTagData + "(" + sGateID + ")" + " 인식 Time : " + trReport.STime + " 코드 : " + iSendResult.ToString());
            //                Program.AppLog.Write("서버 Send 실패 Tag : " + sTagData + "(" + sGateID + ")" + " 인식 Time : " + trReport.STime + " 코드 : " + iSendResult.ToString());
            //            }
            //        }
            //        //if (iSendResult > 0)
            //        //{
            //        //    p_dt.Rows.Clear();
            //        //    p_dt.AcceptChanges();
            //        //}
            //    }
            //    else
            //    {
            //        Program.AppLog.Write("보낼 데이터 없음");
            //        _bSendData = false;
            //        trReport.Clear();
            //        //p_dt.Rows.Clear();
            //        //p_dt.AcceptChanges();
            //        //Program.AppLog.Write("보낼 데이터 없음");
            //    }
            //    rtn = true;
            //}
            //catch (Exception ex)
            //{
            //    ((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("Server Tag 전송 실패 Exception : " + ex.ToString());
            //    Program.SysLog.Write(this, "ReadDataSender Send 데이터 처리 에러", ex);
            //    rtn = false;
            //}
            //finally
            //{
            //    Thread.Sleep(2000);
            //    bWork = false;

            //}

            return rtn;
        }

        public int InsertData(string sLane, string sCarNo)
        {
            //string sQty = string.Empty;
            ////sLane = "01111";
            ////sCarNo = "38도1416";

            //sQty = "INSERT INTO RFID_RECV_LOG(GTR_RECV_DT, GTR_LANE, GTR_TRUCKNO) VALUES(SYSDATE, '" + sLane + "', '" + sCarNo + "') ";

            //if (DBManagement == null) DBManagement = new DatabaseManagementOracle();

            //int iChk = DBManagement.ExecuteNonQuery(sQty, 3);

            //return iChk;
            return 0;
        }

        public bool DbConnectChk()
        {
            bool bRtn = false;

            //if (DBManagement.bConnected == false)
            //{
            //    bRtn = DBManagement.Connect();
            //}
            //else
            //{
            //    bRtn = true;
            //}

            return bRtn;
        }

        public bool ServerCheck()
        {
            bool bChk = false;

            bool rtn = false;

            bWork = true;

            //1. 서버 연결 상태 체크
            bChk = ConnectionCheck();

            if (!bChk || iSendErr >= 5)
            {//연결이 끊긴 경우 처리
                if (!ConnectServer(ProgramSettings.ServerIP, ProgramSettings.ServerPort))
                {//연결이 끊긴 경우 처리
                    if (bConnection == true || bFirstConnection == true)
                    {
                        /////Program.AppLog.Write("서버와 연결실패");
                        /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("서버와 연결실패");
                        //AddDeviceError(ProgramSettings.MiddlewareID, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmssfff"), "N", string.Format("{0:D4}", (int)DevErrorCode.Disconnected), "연결실패");
                        bConnection = false;
                    }

                    if (bFirstConnection == true) bFirstConnection = false;
                    //Program.AppLog.Write("ReadDataSender 연결실패");
                    bWork = false;
                    return rtn;
                }
            }

            //Program.AppLog.Write("ReadDataSender 연결설공");

            //서버연결 성공일 경우 로그 처리
            if (bConnection == false || bFirstConnection == true)
            {//이전 상태가 서버연결 실패 또는 최초 서버 연결일 경우 로그 처리
                /////Program.AppLog.Write("서버와 연결성공");
                /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("서버와 연결성공");
                //AddDeviceError(ProgramSettings.MiddlewareID, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmssfff"), "Y", string.Format("{0:D4}", (int)DevErrorCode.Connected), "연결성공");
                bConnection = true;
            }

            if (bFirstConnection == true) bFirstConnection = false;

            return bConnection;
        }

        public int SendTagData(string sTagData)
        {
            string strSendBuffer;
            byte[] bPacketBuffer;
            int nSendByte;
            try
            {
                strSendBuffer = sTagData;
                bPacketBuffer = MakePacket(strSendBuffer);
                //Test용
                //bPacketBuffer = MakePacket_New(strSendBuffer);
                nSendByte = sckClient.Send(bPacketBuffer);

                /////Program.AppLog.Write("Send : [" + utf8.GetString(bPacketBuffer) + "]");

            }
            catch (TimeoutException te)
            {
                //SetLog(GetType().ToString(), "ReadDataSender", "SendHealthCheck-TimeoutException", te.ToString());
                iSendErr++;
                if (iSendErr >= 5)
                {
                    /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("서버 Send Err : " + iSendErr.ToString());
                }
                /////Program.SysLog.Write(this, "SendTagData-TimeoutException", te);
                nSendByte = -1;
            }
            catch (SocketException se)
            {
                //SetLog(GetType().ToString(), "ReadDataSender", "SendHealthCheck-SocketException", se.ToString());
                iSendErr++;
                if (iSendErr >= 5)
                {
                    /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("서버 Send Err : " + iSendErr.ToString());
                }
                /////Program.SysLog.Write(this, "SendTagData-SocketException", se);
                nSendByte = -1;
            }
            catch (Exception e)
            {
                //SetLog(GetType().ToString(), "ReadDataSender", "SendHealthCheck-Exception", e.ToString());
                iSendErr++;
                if (iSendErr >= 5)
                {
                    /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("서버 Send Err : " + iSendErr.ToString());
                }
                /////Program.SysLog.Write(this, "SendTagData-SocketException", e);
                nSendByte = -1;
            }
            return nSendByte;
        }



        /// <summary>
        /// 서버 소켓 통신 연결
        /// </summary>
        /// <param name="strServerIP"></param>
        /// <param name="nServerPort"></param>
        /// <returns></returns>
        public bool ConnectServer(string strServerIP, int nServerPort)
        {
            bool isConnected = false;

            IPAddress serverAddress;

            try
            {
                serverAddress = IPAddress.Parse(strServerIP);

                sckClient = null;
                sckClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IAsyncResult result = sckClient.BeginConnect(serverAddress, nServerPort, null, null);
                isConnected = result.AsyncWaitHandle.WaitOne(500, true);

                if (!isConnected)
                {
                    sckClient.Close();
                    sckClient.Dispose();
                    //SetLog(GetType().ToString(), "ConnectServer", "연결실패", "서버와 통신연결 실패");
                    /////Program.AppLog.Write("서버와 통신연결 실패");

                    return isConnected;
                }

                int size = sizeof(UInt32);
                UInt32 on = 1;
                UInt32 keepAliveInterval = 5000; //Send a packet once every 5 seconds.
                UInt32 retryInterval = 1000;     //If no response, resend every second.
                byte[] inArray = new byte[size * 3];
                Array.Copy(BitConverter.GetBytes(on), 0, inArray, 0, size);
                Array.Copy(BitConverter.GetBytes(keepAliveInterval), 0, inArray, size, size);
                Array.Copy(BitConverter.GetBytes(retryInterval), 0, inArray, size * 2, size);

                sckClient.IOControl(IOControlCode.KeepAliveValues, inArray, null);

                sckClient.SendTimeout = 2000;
                sckClient.ReceiveTimeout = 2000;

                //연결성공 인단 제외 2019.02.15
                //SetLog(GetType().ToString(), "ConnectServer", "연결성공", string.Empty);
                iSendErr = 0;
                return isConnected = true;
            }
            catch (SocketException se)
            {
                /////Program.SysLog.Write(this, "ConnectServer 소켓 연결 오류", se);
                //SetLog(GetType().ToString(), "ConnectServer", "소켓 연결 오류", se.ToString());
            }
            catch (Exception e)
            {
                /////Program.SysLog.Write(this, "ConnectServer 예상 되지 않은 오류", e);
                //SetLog(GetType().ToString(), "ConnectServer", "예상 되지 않은 오류", e.ToString());
            }

            return isConnected;
        }

        public void CloseServer(string sSate = "1", string sStateTitle = "서버 통신 연결 종료", string sStateMsg = "소켓 연결 종료")
        {
            try
            {
                /////if (sSate == "9") Program.AppLog.Write("서버 통신 연결 종료");//SetLog(GetType().ToString(), "Server", "서버 통신 연결 종료", "프로그램 종료");
                /////else if (sSate == "1") Program.AppLog.Write("ConnectServer : " + sStateTitle); //SetLog(GetType().ToString(), "ConnectServer", sStateTitle, sStateMsg);
                /////else Program.AppLog.Write("ConnectServer : " + sStateTitle);//SetLog(GetType().ToString(), "ConnectServer", sStateTitle, sStateMsg);
                if (sckClient != null)
                {
                    sckClient.Close();
                    sckClient.Dispose();
                }
            }
            catch (Exception ex)
            {
                /////Program.SysLog.Write(this, "CloseServer 에러", ex);
                //SetLog(GetType().ToString(), "ConnectServer", "CloseServer 에러", ex.ToString());

            }
        }

        public bool ConnectionCheck()
        {
            bool bConnected = true;
            //if (sckClient == null || sckClient.Connected == false)
            if (sckClient == null)
                return false;

            if (iSendErr >= 5) return false;

            // 읽기 데이터 존재여부 확인(즉시 리턴)
            try
            {
                if (sckClient.Poll(0, SelectMode.SelectRead))
                {
                    byte[] Buff = new byte[1];
                    // 읽을 데이터가 있는 경우에 Peek 형태로 데이터를 읽어 왔는데 0바이트인 경우 연결 끊어짐 오류 발생
                    if (sckClient.Receive(Buff, SocketFlags.Peek) == 0)
                    {
                        SocketDisconnect();
                        bConnected = false;
                    }
                }
            }
            catch (Exception ex)
            {
                /////Program.SysLog.Write(this, "ConnectionCheck 에러", ex);
                SocketDisconnect();
                bConnected = false;
            }

            return bConnected;
        }

        public void SocketDisconnect()
        {
            if (sckClient != null)
            {
                sckClient.Close();
                sckClient.Dispose();
            }
        }

        /// <summary>
        /// 전송 데이터에 STX, ETX 및 CRC를 추가하여 실제 전송 Packet 생성
        /// </summary>
        /// <param name="strSendBuffer"></param>
        /// <returns></returns>
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
                sendBuffer = new byte[utf8.GetBytes(strSendBuffer).Length];
                //sendBuffer[0] = StartOfText;

                sendBuffer = utf8.GetBytes(strSendBuffer);
                //System.Buffer.BlockCopy(tempBuffer, 0, sendBuffer, 1, tempBuffer.Length);
                //sendBuffer[tempBuffer.Length + 1] = EndOfText;
                //nCRCValue = Program.crc16.ComputeChecksum(tempBuffer);
                //strCRC = string.Format("{0:X4}", nCRCValue);
                //System.Buffer.BlockCopy(Encoding.ASCII.GetBytes(strCRC), 0, sendBuffer, tempBuffer.Length + 2, 4);
                return sendBuffer;
                //            reqCRC16TextBox.Text = strCRC;
            }
            catch (Exception e)
            {
                /////Program.AppLog.Write(this, "Unexpected exception!(MakePacket)", e);
            }

            return null;
        }

        /// <summary>
        /// 전송 데이터에 STX, ETX 및 CRC를 추가하여 실제 전송 Packet 생성
        /// </summary>
        /// <param name="strSendBuffer"></param>
        /// <returns></returns>
        public byte[] MakePacket_New(string strSendBuffer)
        {
            byte[] sendBuffer;
            byte[] tempBuffer;
            ushort nCRCValue;
            //string strBuffer;
            string strCRC;

            try
            {

                //sendBuffer = new byte[EUC_KR.GetBytes(strSendBuffer).Length + 2 + 4];
                sendBuffer = new byte[utf8.GetBytes(strSendBuffer).Length + 2];
                sendBuffer[0] = StartOfText;

                tempBuffer = utf8.GetBytes(strSendBuffer);
                System.Buffer.BlockCopy(tempBuffer, 0, sendBuffer, 1, tempBuffer.Length);
                sendBuffer[tempBuffer.Length + 1] = EndOfText;
                //nCRCValue = Program.crc16.ComputeChecksum(tempBuffer);
                //strCRC = string.Format("{0:X4}", nCRCValue);
                //System.Buffer.BlockCopy(Encoding.ASCII.GetBytes(strCRC), 0, sendBuffer, tempBuffer.Length + 2, 4);
                return sendBuffer;
                //            reqCRC16TextBox.Text = strCRC;
            }
            catch (Exception e)
            {
                /////Program.AppLog.Write(this, "Unexpected exception!(MakePacket)", e);
            }

            return null;
        }

        public void ResetTransferCount()
        {
            nTransferCount = 1;
        }

        public void ResetError()
        {
            try
            {
                if (ConnectionCheck())
                {
                    CloseServer("2", "서버 통신 연결 종료", "소켓 연결 종료(초기화)");
                }

                nTransferCount = 1;
                iErrorCnt = 0;
            }
            catch (Exception ex)
            {
                /////Program.AppLog.Write(this, "ResetError() exception", ex);
            }
        }
        #endregion
    }
}
