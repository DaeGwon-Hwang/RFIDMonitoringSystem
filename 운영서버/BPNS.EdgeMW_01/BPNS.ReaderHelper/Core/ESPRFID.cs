using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace BPNS.ReaderHelper
{
    public enum StringProtocolState
    {
        Cursor = 0,
        Error = 1,
        Fail = 2,
        Login = 3,
        Timeout = 4,
    }

    public enum RFIDReaderSocketState
    {
        Closed = 0,
        Connected = 1,
    }

    public class ESPRFID
    {
        Socket sckSocket;

        string ID = "ubists";
        string PW = "0000";

        //string ID = "cuint";
        //string PW = "0000";

        #region -- 명령 관련

        string ReqPW = "Password : ";
        //string Cursor = "URSP>";
        string Cursor = "UBISTS>";

        public bool bSensor = false;

        public DateTime _dteSensor;

        public DateTime DteSensor { get => _dteSensor; set => _dteSensor = value; }

        #endregion

        public ESPRFID()
        {
            _dteSensor = DateTime.Now.AddDays(-1);
        }

        public bool Connect(string sAddress, int nPort)
        {
            bool bResult = false;
            //lock (oLibraryCO)
            {

                // 소켓 초기화
                if (sckSocket == null)
                {
                    try
                    {
                        sckSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    }
                    catch (SocketException se)
                    {

                        throw se;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                else
                {
                    sckSocket.Close();
                    sckSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }

                try
                {
                    // 소켓 연결
                    sckSocket.Connect(sAddress, nPort);
                    bool bConnected = sckSocket.Connected;

                    // 연결 실패할 경우 소켓 초기화 하고종료
                    if (!bConnected)
                    {
                        sckSocket.Close();
                        sckSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        return false;
                    }

                    int size = sizeof(UInt32);
                    UInt32 on = 1;
                    UInt32 keepAliveInterval = 2000; //Send a packet once every 10 seconds.
                    UInt32 retryInterval = 1000; //If no response, resend every second.
                    byte[] inArray = new byte[size * 3];
                    Array.Copy(BitConverter.GetBytes(on), 0, inArray, 0, size);
                    Array.Copy(BitConverter.GetBytes(keepAliveInterval), 0, inArray, size, size);
                    Array.Copy(BitConverter.GetBytes(retryInterval), 0, inArray, size * 2, size);

                    sckSocket.IOControl(IOControlCode.KeepAliveValues, inArray, null);

                    // 소켓 타임아웃 설정
                    sckSocket.ReceiveTimeout = 1000;
                    sckSocket.SendTimeout = 1000;


                }
                catch (SocketException se)
                {

                    return false;
                }
                catch (Exception e)
                {
                    throw e;
                }

                try
                {
                    // 연결 완료 후 RFID 리더 로그인(모델마다 ID, 비번 다름 - 모델별 통신 명령어 설정파일에 저장(XML 파일))
                    if (!LoginProcess())
                        return false;
                }
                catch (SocketException se)
                {
                    Disconnect();
                }
                catch (Exception e)
                {
                    throw e;
                }

                try
                {
                    // 리더기 환경 설정(시각, 안테나 수, 안테나 출력, 태그 데이터 포맷)
                    if (!InitializeProcess())
                      return false;

                    // 시간 세팅과 리딩시작 설정
                    if (!BeginResult())
                        return false;

                    bResult = true;

                }
                catch (SocketException se)
                {
                }
                catch (Exception e)
                {
                }
            }
            return bResult;
        }

        public void Disconnect()
        {
            if (sckSocket != null)
                sckSocket.Close();

            sckSocket = null;
        }

        // 소켓 연결 확인
        public bool CheckConnection()
        {
            bool bResult = false;
            if (sckSocket?.Connected == false)
                return false;

            bResult = true;
            byte[] readBuffer;
            readBuffer = new byte[1];
            if (sckSocket == null)
            {

                bResult = false;

            }
            // 일반적으로 소켓의 Connected 변수는 연결 상태를 정확히 체크하지 못하므로 소켓 Poll 함수를 이용하여 Read 상태 체크를 이용해야 한다.

            try
            {
                //연결 종료 확인
                if (sckSocket.Poll(0, SelectMode.SelectRead) && sckSocket.Available == 0)
                {
                    int nReadByte = sckSocket.Available;
                    if (nReadByte == 0)
                    {
                        bResult = false;
                    }
                }


                /*
                //로컬 PC의 ip정보관련한 객체를 가져온다
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

                //tcp상태 정보를 배열로 저장하기 위해 정의한다.
                TcpConnectionInformation[] tcpConnection;
                try
                {
                    //소켓정보에 부함되는 연결을 필터링 해서 가져온다. 
                    tcpConnection = ipProperties.GetActiveTcpConnections().Where(x => x.LocalEndPoint.Equals(sckSocket.LocalEndPoint) && x.RemoteEndPoint.Equals(sckSocket.RemoteEndPoint)).ToArray();
                }
                catch (Exception)
                {
                    //오류 발생시 disconnect()가 호출되면 해당 속성은 nullexception 이 발생된다. 
                    return false;
                }

                if (tcpConnection != null && tcpConnection.Length > 0)
                {
                    TcpState stateofConnection = tcpConnection.First().State;

                    //연결되어서 데이터를 주고 받을 수 있는 상태를 가르키는 값 확인
                    if (stateofConnection == TcpState.Established)
                        return true;
                    else
                        return false;
                }
                */

            }
            catch (Exception)
            {

                return false;
            }



            return bResult;
        }

        public bool CheckPing(string sAddress)
        {
            bool bRtn = false;

            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send(sAddress);

            if (reply.Status == IPStatus.Success) bRtn = true;

            return bRtn;
        }


        private bool LoginProcess()
        {
            bool bResult = false; ;
            StringProtocolState Result;


            SendString(ID);

            WaitString(ReqPW);

            SendString(PW);

            Result = WaitResult();

            if (Result == StringProtocolState.Cursor)
                bResult = true;

            //최초 주석
            //SetDefaultSignal();
            //GetReadExtOutputValue();//데이터 확인용




            return bResult;
        }

        // 지정된 문자열을 RFID 리더에 소켓통신으로 전송
        private int SendString(string strSendString)
        {
            int nResult = sckSocket.Send(Encoding.ASCII.GetBytes(strSendString + "\r\n"));

            return nResult;
        }

        // 특정 문자열이 포함된 데이터 대기(설정 명령어 전송 후 결과 대기용)
        public void WaitString(string sWaitString)
        {
            byte[] receiveBuffer = new byte[4096];
            int nOffset = 0;
            int nReadByteCount;
            string sReceviceString;

            do
            {
                nReadByteCount = sckSocket.Receive(receiveBuffer, nOffset, 4096 - nOffset, SocketFlags.None);
                nOffset += nReadByteCount;
                sReceviceString = Encoding.ASCII.GetString(receiveBuffer, 0, nOffset);
            } while (sReceviceString.IndexOf(sWaitString) == -1);
        }

        // 설정 명령어 전송 후 결과 문자열을 수신하여 처리 결과 상태 변수 리턴(커서, 에러 , 실패, 사용자ID 요청)
        private StringProtocolState WaitResult()
        {
            StringProtocolState Result;
            byte[] receiveBuffer = new byte[4096];
            int nOffset = 0;
            int nReadByteCount;
            string sReceviceString;

            do
            {
                nReadByteCount = sckSocket.Receive(receiveBuffer, nOffset, 4096 - nOffset, SocketFlags.None);
                nOffset += nReadByteCount;
                sReceviceString = Encoding.ASCII.GetString(receiveBuffer, 0, nOffset);

                
                if (sReceviceString.Equals("\r\n"))
                {
                    Result = StringProtocolState.Cursor;
                    //break;
                }
                else if (sReceviceString.Equals("Error!\r\n"))
                {
                    Result = StringProtocolState.Error;
                    break;
                }
                else if (sReceviceString.Equals("Fail!\r\n"))
                {
                    Result = StringProtocolState.Fail;
                    break;
                }
                else if (sReceviceString.IndexOf(Cursor) >= 0)
                {
                    Result = StringProtocolState.Cursor;
                    return Result;
                }
                else if (sReceviceString.IndexOf("UserName : ") >= 0)
                {
                    Result = StringProtocolState.Login;
                    return Result;
                }
            } while (true);

            WaitString(Cursor);
            return Result;
        }

        private string WaitResultExt()
        {
            StringProtocolState Result;
            byte[] receiveBuffer = new byte[4096];
            int nOffset = 0;
            int nReadByteCount;
            string sReceviceString = string.Empty;
            string sBReceviceString = string.Empty;

            do
            {
                nReadByteCount = sckSocket.Receive(receiveBuffer, nOffset, 4096 - nOffset, SocketFlags.None);
                nOffset += nReadByteCount;
                sReceviceString = Encoding.ASCII.GetString(receiveBuffer, 0, nOffset);


                if (sReceviceString.Equals("\r\n"))
                {
                    Result = StringProtocolState.Cursor;
                    sBReceviceString = "\r\n";
                    //break;
                }
                else if (sReceviceString.Equals("Error!\r\n"))
                {
                    Result = StringProtocolState.Error;
                    sBReceviceString = "Error";
                    break;
                }
                else if (sReceviceString.Equals("Fail!\r\n"))
                {
                    Result = StringProtocolState.Fail;
                    sBReceviceString = "Fail";
                    break;
                }
                else if (sReceviceString.IndexOf(Cursor) >= 0)
                {
                    Result = StringProtocolState.Cursor;
                    return sBReceviceString;
                }
                else if (sReceviceString.IndexOf("UserName : ") >= 0)
                {
                    Result = StringProtocolState.Login;
                    return sBReceviceString;
                }
                else
                {
                    sBReceviceString = sReceviceString;
                }
            } while (true);

            WaitString(Cursor);
            return sBReceviceString;
        }

        /// <summary>
        /// 테스트 장비에서는 녹색 붉은 색
        /// </summary>
        /// <returns></returns>
        public StringProtocolState SetNormalSignal()
        {
            StringProtocolState Result;
            SendString(" InitExtOut 253");
            Result = WaitResult();

            return Result;
        }

        /// <summary>
        /// 테스트 장비에서는 녹색 
        /// </summary>
        /// <returns></returns>
        public StringProtocolState SetExceptionSignal()
        {
            StringProtocolState Result;

            SendString(" InitExtOut 251");
            Result = WaitResult();
            
            return Result;
        }

        /// <summary>
        /// 테스트 장비에서는 녹색 
        /// </summary>
        /// <returns></returns>
        public StringProtocolState SetExceptionSignal2()
        {
            StringProtocolState Result;

            SendString(" InitExtOut 243");
            Result = WaitResult();

            return Result;
        }

        /// <summary>
        /// 경광등 꺼져있는 상태
        /// </summary>
        /// <returns></returns>
        public StringProtocolState SetDefaultSignal()
        {
            StringProtocolState Result;

            SendString(" InitExtOut 255");
            Result = WaitResult();

            return Result;
        }

        public StringProtocolState GetReadExtOutputValue()
        {
            StringProtocolState Result;

            SendString("ReadExtOutputValue");
            Result = WaitResult();

            return Result;
        }

        string[] _sInValue = new string[] { "OFF", "OFF", "OFF", "OFF" };


        string _sReadExtInputValue = "ON ON ON ON ";
        /// <summary>
        /// 센서 체크 후 센서 감지 시간 설정
        /// </summary>
        /// <returns></returns>
        public string GetReadExtInputValue()
        {
            //StringProtocolState Result;
            string sRtn = string.Empty; 

            SendString("ReadExtInputValue");
            sRtn = WaitResultExt();

            //string[] sArrRtn = sRtn.Split(' ');

            //if (sArrRtn.Length >= 4)
            //{
            //    for (int i = 3; i >= 0; i--)
            //    {
            //        if (sArrRtn[i] == "ON")
            //        {
            //            sRtn += "1";
            //        }
            //        else
            //        {
            //            sRtn += "0";
            //        }

            //    }

            //    if (sRtn == "1100") //센서 On
            //    {
            //        bSensor = true;
            //        _dteSensor = DateTime.Now;
            //    }
            //    else
            //    {
            //        bSensor = false;
            //    }
            //}

            if (sRtn == "ON ON ON OFF " || sRtn == "ON ON OFF ON " || sRtn == "ON ON OFF OFF ") //센서 On
            {
                //SendString("ReadExtOutputValue");
                //string sGetRtn = WaitResultExt();
                
                bSensor = true;
                _dteSensor = DateTime.Now;

                if (sRtn == "ON ON ON OFF ")
                {
                    //if (_sReadExtInputValue != sRtn)
                    //{
                        SetNormalSignal();
                        _sReadExtInputValue = sRtn;
                    //}
                }

                if (sRtn == "ON ON OFF ON ")
                {
                    //if (_sReadExtInputValue != sRtn)
                    //{
                        SetNormalSignal();
                        _sReadExtInputValue = sRtn;
                    //}
                }

                if (sRtn == "ON ON OFF OFF ")
                {
                    //if (_sReadExtInputValue != sRtn)
                    //{
                        SetExceptionSignal2();
                        _sReadExtInputValue = sRtn;
                    //}
                }
            }
            else
            {
                //if (_sReadExtInputValue != sRtn)
                //{
                    SetDefaultSignal();
                    _sReadExtInputValue = sRtn;
                //}

                bSensor = false;
            }

            return sRtn;
        }

        // 시간 설정 명령어 전송
        public StringProtocolState SetCurrentTime()
        {
            StringProtocolState Result;

            SendString("WriteSysTime " + DateTime.Now.ToString("yyyy'/'MM'/'dd HH':'mm':'ss"));
            Result = WaitResult();

            return Result;
        }

        // 안테나 설정 명령어 전송
        public StringProtocolState SetAntenna(string sAntennaSequence)
        {
            StringProtocolState Result;
            //lock (oLibraryCO)
            {
                //Program.AppLog.Write(this, "lock oLibraryCO Enter");
                SendString("setAntSequence " + sAntennaSequence);
                Result = WaitResult();
                //Program.AppLog.Write(this, "lock oLibraryCO Exit");
            }
            return Result;
        }

        // 안테나 출력 설정 명령어 전송
        public StringProtocolState SetAntennaPower(int nPower)
        {
            StringProtocolState Result;
            //lock (oLibraryCO)
            {
                //Program.AppLog.Write(this, "lock oLibraryCO Enter");
                SendString("setRFAttenuation " + nPower.ToString());
                Result = WaitResult();
                //Program.AppLog.Write(this, "lock oLibraryCO Exit");
            }
            return Result;
        }

        public StringProtocolState SetMaxAntPort(int nMaxPort)
        {
            StringProtocolState Result;
            //lock (oLibraryCO)
            {
                //Program.AppLog.Write(this, "lock oLibraryCO Enter");
                SendString("setMaxAntPort " + nMaxPort.ToString());
                Result = WaitResult();
                //Program.AppLog.Write(this, "lock oLibraryCO Exit");
            }
            return Result;
        }


        public StringProtocolState Save()
        {
            StringProtocolState Result;
            //lock (oLibraryCO)
            {
                //Program.AppLog.Write(this, "lock oLibraryCO Enter");
                sckSocket.ReceiveTimeout = 10000;
                sckSocket.SendTimeout = 10000;
                SendString("save");
                Result = WaitResult();
                sckSocket.ReceiveTimeout = 1000;
                sckSocket.SendTimeout = 1000;
                //Program.AppLog.Write(this, "lock oLibraryCO Exit");
            }
            return Result;
        }


        // 태그 데이터 포맷 설정 명령어 전송(태그 데이터 포맷은 모델별 명령어 문자열 저장 파일에 저장되어 있음)
        public StringProtocolState SetTagDataFormat(string sFormat)
        {
            StringProtocolState Result;
            //lock (oLibraryCO)
            {
                //Program.AppLog.Write(this, "lock oLibraryCO Enter");
                SendString("setTagListFormat " + sFormat);
                Result = WaitResult();
                //Program.AppLog.Write(this, "lock oLibraryCO Exit");
            }
            return Result;
        }

        // NoTagSend 모드 설정(리더기 프로토콜 파일 참조)
        public StringProtocolState SetNoTagSend(string sOption)
        {
            StringProtocolState Result;
            //lock (oLibraryCO)
            {
                //Program.AppLog.Write(this, "lock oLibraryCO Enter");
                SendString("setNoTagSend " + sOption);
                Result = WaitResult();
                //Program.AppLog.Write(this, "lock oLibraryCO Exit");
            }
            return Result;
        }

        // BeginRead 명령어 전송(RFID 리더기가 BeginRead명령을 수신하면 RFID 태그를 읽기 시작한다)
        public StringProtocolState BeginRead()
        {
            StringProtocolState Result;
            //lock (oLibraryCO)
            {
                //Program.AppLog.Write(this, "lock oLibraryCO Enter");
                SendString("BeginRead");
                Result = WaitResult();
                //Program.AppLog.Write(this, "lock oLibraryCO Exit");
            }
            return Result;
        }

        // FinishRead 명령어를 전송하여 RFID리더기가 태그 읽기 동작을 중지하게 한다)
        public StringProtocolState FinishRead()
        {
            StringProtocolState Result;
            //lock (oLibraryCO)
            {
                //Program.AppLog.Write(this, "lock oLibraryCO Enter");
                SendString("FinishRead");
                Result = WaitResult();
                //Program.AppLog.Write(this, "lock oLibraryCO Exit");
            }
            return Result;
        }


        private bool InitializeProcess()
        {
            bool bResult = false;
            StringProtocolState Result;

            // 최초 실행시 리더기 시간이 맞지 않을 수 있으므로 해당 시간 설정기능은 제거하지 않는다.
            // 물론 서버에서 시간을 받아와서 PC 시간 설정이 완료되면 다시 시간설정이 진행되지만 차이가 있으므로 별도로 두번 다 진행한다.
            Result = SetCurrentTime();
            if (Result == StringProtocolState.Error || Result == StringProtocolState.Fail)
            {
                return bResult;
            }

            string Antenna = "1,2,3,4";//"1, 2";            // 안테나 설정 "1,2,3,4"

            Result = SetAntenna(Antenna);
            if (Result == StringProtocolState.Error || Result == StringProtocolState.Fail)
            {
                return bResult;
            }
            int AntennaPower = 20;

            // 안테나 출력 설정 (안테나 출력이 0이면 리더기 설정값을 이용한다)
            if (AntennaPower != 0)
            {
                Result = SetAntennaPower(AntennaPower);
                if (Result == StringProtocolState.Error || Result == StringProtocolState.Fail)
                {
                    return bResult;
                }
            }

            // 안테나 최대 개수 설정
            int AttenaCnt = 4;
            Result = SetMaxAntPort(AttenaCnt);
            if (Result == StringProtocolState.Error || Result == StringProtocolState.Fail)
            {
                return bResult;
            }

            // Tag Data 출력 양식 설정
            string TagDataFormatString = "NO:%i,ID:%w,TIME:%d %t %e,tcnt:%r,cnt1:%1,cnt2:%2,cnt3:%3,cnt4:%4,ant:%a";
            Result = SetTagDataFormat(TagDataFormatString);
            if (Result == StringProtocolState.Error || Result == StringProtocolState.Fail)
            {
                return bResult;
            }

            string NoTagSendValue = "0";
            Result = SetNoTagSend(NoTagSendValue);
            if (Result == StringProtocolState.Error || Result == StringProtocolState.Fail)
            {
                return bResult;
            }
            // 설정정보 저장
            Result = Save();
            if (Result == StringProtocolState.Error || Result == StringProtocolState.Fail)
            {
                return bResult;
            }
            //Result = FinishRead();
            //if (Result == StringProtocolState.Error || Result == StringProtocolState.Fail)
            //{
            //    return bResult;
            //}




            bResult = BeginResult();



            return bResult;
        }



        public bool BeginResult()
        {
            StringProtocolState Result;
            bool bResult = false;


            // 최초 실행시 리더기 시간 맞지 않음 재부팅시 시간 초기화 됨 
            // 이를 위해 최초 연결 시 시간을 세팅하는 방식 고려           
            Result = SetCurrentTime();
            if (Result == StringProtocolState.Error || Result == StringProtocolState.Fail)
            {
                return bResult;
            }


            Result = BeginRead();

            // FinishRead를 수행하지 않으면 오류가 나지만 FinishRead를 할 경우 이전 태그 정보가 지워지므로 Error를 무시한다.
            // 이미 BeginRead가 수행된 리더기의 세션에 연결하여 BeginRead를 하면 오류 발생한다.
            if (Result == StringProtocolState.Fail)
            {
                return bResult;
            }
            bResult = true;



            return bResult;
        }

        // RFID 리더기에 태그 리스트를 요청하고 결과 문자열을 처리하여 구조체에 저장한다.
        public List<TagDataRaw> GetTagList()
        {
            List<TagDataRaw> readTagString = new List<TagDataRaw>();
            string sReceiveString;
            string strTemp;
            string[] sTagDataList;
            bool bReceiveCursor = false;
            int i;

            // 통신 연결이 되어 있지 않은 경우 데이터 없는 상태로 리턴
            if (!CheckConnection())
            {
                return readTagString;
            }

            // 태그 리스트 요청 명령어 전송
            SendString("L");

            try
            {
                sReceiveString = string.Empty;
                // 커서가 수신될 때 까지 데이터 수신
                do
                {
                    sReceiveString += ReceiveString();
                    // 수신 버퍼내에 커서 문자열 검색 후 있으면 종료
                    if (sReceiveString.IndexOf(Cursor) != -1)
                    {
                        bReceiveCursor = true;
                    }
                } while (!bReceiveCursor);
                // 텍스트 Line 별 문자열 분리
                sTagDataList = sReceiveString.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                // 텍스트 데이터 파싱
                for (i = 0; i < sTagDataList.Length; i++)
                {
                    // 커서인 경우 데이터 처리가 완료 되었으므로 루프 종료
                    if (sTagDataList[i].IndexOf(Cursor) != -1)
                    {
                        bReceiveCursor = true;
                        break;
                    }
                    // 문자열 파싱
                    TagDataRaw newTagData = new TagDataRaw();
                    newTagData.parseString(sTagDataList[i]);


                    readTagString.Add(newTagData);

                }
            }
            catch (SocketException se)
            {
                // Timeout 오류일 경우 연결 종료
                if (se.ErrorCode == (int)SocketError.TimedOut)
                {

                    sckSocket.Close();
                    sckSocket = null;

                }
            }


            return readTagString;
        }

        // RFID 리더기에 태그 리스트를 요청하고 결과 문자열을 처리하여 구조체에 저장한다.
        public int GetTagList(out List<TagDataRaw> outTagLIst)
        {
            List<TagDataRaw> readTagString = new List<TagDataRaw>();
            string sReceiveString;
            string strTemp;
            string[] sTagDataList;
            bool bReceiveCursor = false;
            int i;

            // 통신 연결이 되어 있지 않은 경우 데이터 없는 상태로 리턴
            if (!CheckConnection())
            {
                outTagLIst = null;
                return 0;
            }

            // 태그 리스트 요청 명령어 전송
            SendString("L");

            try
            {
                sReceiveString = string.Empty;
                // 커서가 수신될 때 까지 데이터 수신
                do
                {
                    sReceiveString += ReceiveString();
                    // 수신 버퍼내에 커서 문자열 검색 후 있으면 종료
                    if (sReceiveString.IndexOf(Cursor) != -1)
                    {
                        bReceiveCursor = true;
                    }
                } while (!bReceiveCursor);
                // 텍스트 Line 별 문자열 분리
                sTagDataList = sReceiveString.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                // 텍스트 데이터 파싱
                for (i = 0; i < sTagDataList.Length; i++)
                {
                    // 커서인 경우 데이터 처리가 완료 되었으므로 루프 종료
                    if (sTagDataList[i].IndexOf(Cursor) != -1)
                    {
                        bReceiveCursor = true;
                        break;
                    }
                    // 문자열 파싱
                    TagDataRaw newTagData = new TagDataRaw();
                    newTagData.parseString(sTagDataList[i]);


                    readTagString.Add(newTagData);

                }
            }
            catch (SocketException se)
            {
                // Timeout 오류일 경우 연결 종료
                if (se.ErrorCode == (int)SocketError.TimedOut)
                {

                    sckSocket.Close();
                    sckSocket = null;

                }
            }

            outTagLIst = readTagString;

            return outTagLIst.Count;
        }

        // 수신 문자열 처리
        public string ReceiveString()
        {
            byte[] receiveBuffer = new byte[4096];
            int nReadByteCount;
            string sReceviceString = string.Empty;

            // 50밀리세컨드 이내에 데이터가 있을 경우 데이터 읽기 진행
            if (sckSocket.Poll(50000, SelectMode.SelectRead))
            {
                nReadByteCount = sckSocket.Receive(receiveBuffer);
                if (nReadByteCount <= 0)
                    return null;
                sReceviceString = Encoding.ASCII.GetString(receiveBuffer, 0, nReadByteCount).Trim(new Char[] { '\0' });

            }

            return sReceviceString;
        }
    }

    /// <summary>
    /// Tag 정보..
    /// </summary>
    public class TagDataRaw
    {
        private string strTagID;
        public string TagID
        {
            get { return strTagID; }
            set { strTagID = value; }
        }

        private string strDate;
        public string Date
        {
            get { return strDate; }
            set { strDate = value; }
        }

        private string strTime;
        public string Time
        {
            get { return strTime; }
            set { strTime = value; }
        }

        private int nMilliSecond;
        public int MilliSecond
        {
            get { return nMilliSecond; }
            set { nMilliSecond = value; }
        }

        private int nTotalCount;
        public int TotalCount
        {
            get { return nTotalCount; }
            set { nTotalCount = value; }
        }

        private int[] nAntCnt = new int[4];
        public int[] AntCnt
        {
            get { return nAntCnt; }
            set { nAntCnt = value; }
        }

        private int nLastReadAnt;
        public int LastReadAnt
        {
            get { return nLastReadAnt; }
            set { nLastReadAnt = value; }
        }

        public bool parseString(string TagDataString)
        {
            string[] sSplitStrings;
            int nIndex = 0;
            try
            {
                //    <TagDataFormatString>NO:%i,ID:%w,TIME:%d %t %e,tcnt:%r,cnt1:%1,cnt2:%2,cnt3:%3,cnt4:%4,ant%a</TagDataFormatString>
                sSplitStrings = TagDataString.Split(new char[] { ',' });
                nIndex = 1;
                strTagID = sSplitStrings[nIndex].Substring(sSplitStrings[nIndex].IndexOf(':') + 1);
                nIndex++;
                strDate = sSplitStrings[nIndex].Substring(sSplitStrings[nIndex].IndexOf(':') + 1).Substring(0, 8);
                strDate = DateTime.Now.Year.ToString().Substring(0, 2) + strDate.Substring(0, 2) + strDate.Substring(3, 2) + strDate.Substring(6, 2);
                strTime = sSplitStrings[nIndex].Substring(sSplitStrings[nIndex].IndexOf(':') + 1).Substring(9, 8);
                strTime = strTime.Substring(0, 2) + strTime.Substring(3, 2) + strTime.Substring(6, 2);
                nMilliSecond = int.Parse(sSplitStrings[nIndex].Substring(sSplitStrings[nIndex].IndexOf(':') + 1).Substring(18));
                nIndex++;
                nTotalCount = int.Parse(sSplitStrings[nIndex].Substring(sSplitStrings[nIndex].IndexOf(':') + 1));
                nIndex++;
                nAntCnt[0] = int.Parse(sSplitStrings[nIndex].Substring(sSplitStrings[nIndex].IndexOf(':') + 1));
                nIndex++;
                nAntCnt[1] = int.Parse(sSplitStrings[nIndex].Substring(sSplitStrings[nIndex].IndexOf(':') + 1));
                nIndex++;
                nAntCnt[2] = int.Parse(sSplitStrings[nIndex].Substring(sSplitStrings[nIndex].IndexOf(':') + 1));
                nIndex++;
                nAntCnt[3] = int.Parse(sSplitStrings[nIndex].Substring(sSplitStrings[nIndex].IndexOf(':') + 1));
                nIndex++;
                nLastReadAnt = int.Parse(sSplitStrings[nIndex].Substring(sSplitStrings[nIndex].IndexOf(':') + 1));
            }
            catch (Exception e)
            {

                return false;
            }
            return true;
        }

        public int GetMaxAnt()
        {
            int nMaxIndex = -1;
            int nMaxCount = 0;

            for (int i = 0; i < 4; i++)
            {
                if (nAntCnt[i] > nMaxCount)
                {
                    nMaxCount = nAntCnt[i];
                    nMaxIndex = i;
                }
            }
            if (nMaxIndex == -1)
                return nMaxIndex;
            if (nMaxIndex < 4)
                return nMaxIndex + 1;
            return -1;
        }
    }
}
