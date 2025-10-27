using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BPNS.TransitiveMiddleware
{
    public class PrinterAgent
    {
        public PrinterAgent()
        {

        }

        public static bool alive(string sIP, string sPrtGbn)
        {
            bool bRtn = false;

            bRtn = PrintAliveCheck(sIP, 9100);

            return bRtn;
        }

        public static bool PrintAliveCheck(string strServerIP, int nServerPort)
        {
            bool bRtn = false;
            IPAddress serverAddress;
            IPHostEntry hostInfo;
            Socket sckClient;
            try
            {
                //lock (oSocketCO)
                {
                    //Program.AppLog.Write(this, "lock oSocketCO Enter");
                    try
                    {
                        serverAddress = System.Net.IPAddress.Parse(strServerIP);
                    }
                    catch (FormatException fe)
                    {
                        //Program.AppLog.Write("IP 주소 파싱 오류[도메인 이름 검색]");
                        hostInfo = Dns.GetHostByName(strServerIP);
                        serverAddress = hostInfo.AddressList[0];
                    }
                    //if (client == null)
                    //    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sckClient = null;
                    sckClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IAsyncResult result = sckClient.BeginConnect(serverAddress, nServerPort, null, null);
                    bool bConnected = result.AsyncWaitHandle.WaitOne(200, true);
                    if (!bConnected)
                    {
                        sckClient.Close();
                        sckClient.Dispose();
                    }
                    else
                    {
                        // 데이터 전송
                        sckClient.Send(HexToByte("05"));
                        byte[] bReceiveBuffer = new byte[4096];
                        int nReadByte = sckClient.Receive(bReceiveBuffer, 0, 4096, SocketFlags.None);
                        string sReceiveVal = ByteHexToHexString(bReceiveBuffer);

                        if (sReceiveVal == "22") bRtn = true;
                    }

                    sckClient.Close();
                    sckClient.Dispose();

                }
            }
            catch (SocketException se)
            {
                //Program.AppLog.Write(this, "소켓 연결 오류[" + se.Message + "]");
                //Program.AppLog.Write(this, "lock oSocketCO Exit");
            }
            catch (Exception e)
            {
                // Program.SysLog.Write(this, "예상 되지 않은 오류!", e);
                //Program.AppLog.Write(this, "lock oSocketCO Exit");
            }

            return bRtn;
        }

        public static bool print(Dictionary<string, object> request)
        {
            string sRtn = "0";
            string sMessage = "ISS";

            bool bRtn = false;
            IPAddress serverAddress;
            IPHostEntry hostInfo;
            Socket sckClient = null;
            string strServerIP = request["IP_ADDR"].ToString();
            string strPrtEqId = request["PRT_EQ_ID"].ToString();
            int nServerPort = Constants.PRT_PORT;
            try
            {
                //lock (oSocketCO)
                {
                    //Program.AppLog.Write(this, "lock oSocketCO Enter");
                    try
                    {
                        serverAddress = System.Net.IPAddress.Parse(strServerIP);
                    }
                    catch (FormatException fe)
                    {
                        //Program.AppLog.Write("IP 주소 파싱 오류[도메인 이름 검색]");
                        hostInfo = Dns.GetHostByName(strServerIP);
                        serverAddress = hostInfo.AddressList[0];
                    }
                    //if (client == null)
                    //    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sckClient = null;
                    sckClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IAsyncResult result = sckClient.BeginConnect(serverAddress, nServerPort, null, null);
                    //bool bConnected = result.AsyncWaitHandle.WaitOne(200, true);
                    bool bConnected = result.AsyncWaitHandle.WaitOne(Constants.PRT_TIMEOUT, true);
                    if (!bConnected)
                    {
                        //sckClient.Close();
                        //sckClient.Dispose();
                    }
					else
					{
						if (strPrtEqId == "PRT00008")
						{
							double[,] x = new double[,] { { 24, 35 }, { 24, 34.7 } };
							double[] y = new double[] { 3.4, 5.4, 7.8 };

							StringBuilder sbBuffer = new StringBuilder();

							// 1. NORMAL 모드 진입
							// NORMAL 모드에서의 명령어는 460p 참조
							sbBuffer.Append("~NORMAL												\r\n");
							// 2. CREATE 모드 진입
							// CREATE 모드에서의 명령어는 462p 참조
							// CREATE 상세설명은 61p 참조
							// CREATE; [/]formname [;FL] [;NOMOTION] [;DISK]
							// RFID: CREATE의 ALIAS, X: 프린터에 지정된 용지크기를 따름
							sbBuffer.Append("~CREATE;RFID;X											\r\n");

							// 3. CODE 39 TYPE의 바코드
							// 193p, 159p 참조
							// C3/9: CODE 39, XRD2:2:6:6  확대 속성, H6: 바코드 전체 높이, 4.9: Y축 위치, 3.6: X축 위치
							// X에 RD를 추가하여 사용자지정 바코드 비율 지정 가능(A:B:C:D => A: 짧은 바 길이, B: 짧은 공간 길이, C: 긴 바 길이, D: 긴 공간 길이)
							sbBuffer.Append("BARCODE												\r\n");
							sbBuffer.Append("C3/9;XRD2:2:6:6;H6;4.9;3.6								\r\n");

							// 바코드 내용
							sbBuffer.Append(string.Format("*{0}*										\r\n", request["PAMS_MGMT_NO"].ToString()));
							sbBuffer.Append("STOP													\r\n");

							// 4. FONT 지정
							// 84p 참조
							// NAME #: 파일명을 통한  폰트 지정
							sbBuffer.Append("FONT;NAME h2hdrm.ttf 									\r\n");
							// 5. 문자열(영문, 숫자)
							// 33p 참조
							sbBuffer.Append("ALPHA													\r\n");
							// POINT: 글자 크기 단위(1/72 inch), SR: Y축 위치, SC: X축 위치, VE: 글자 높이, HE: 글자 간격
							// *내용*
							sbBuffer.Append(string.Format("POINT;{0};{1};10;10;*{2}*				\r\n", y[0], x[0, 0], request["PRSDT_NM"].ToString()));

							if (request["PROD_ORG_NM"] != null && (request["PROD_ORG_NM"].ToString()).Length > 15)
							{
								sbBuffer.Append(string.Format("POINT;5.0;24;8;8;*{0}*				\r\n", (request["PROD_ORG_NM"].ToString()).Substring(0, 15)));
								sbBuffer.Append(string.Format("POINT;5.9;24;8;8;*{0}*				\r\n", (request["PROD_ORG_NM"].ToString()).Substring(15, (request["PROD_ORG_NM"].ToString()).Length - 15)));
							}
							else if (request["PROD_ORG_NM"] != null)
							{
								sbBuffer.Append(string.Format("POINT;{0};{1};9;8;*{2}*				\r\n", y[1], x[0, 0], request["PROD_ORG_NM"].ToString()));
							}

							//sbBuffer.Append(string.Format("POINT;{0};{1};17;17;*{2}*				\r\n", y[2], x[0, 0], request["PAMS_MGMT_NO"].ToString()));
							sbBuffer.Append(string.Format("POINT;{0};{1};14;14;*{2}*				\r\n", y[2], x[0, 0], request["PAMS_MGMT_NO"].ToString()));

							//sbBuffer.Append(string.Format("POINT;{0};{1};7;9;*{2}*				\r\n", 2.8, x[1, 1], request["PROD_STRT_YY"].ToString()));
							//sbBuffer.Append(string.Format("POINT;{0};{1};7;9;*{2}*				\r\n", 3.9, x[1, 1], request["PRSRV_PRD"].ToString()));

							if (request["PROD_STRT_YY"] != null && (request["PROD_STRT_YY"].ToString()).Length > 4)
							{
								sbBuffer.Append(string.Format("POINT;{0};{1};7;6;*{2}*				\r\n", 3.0, x[1, 1], request["PROD_STRT_YY"].ToString()));
							}
							else
							{
								sbBuffer.Append(string.Format("POINT;{0};{1};7;9;*{2}*				\r\n", 3.0, x[1, 1], request["PROD_STRT_YY"].ToString()));
							}

							sbBuffer.Append(string.Format("POINT;{0};{1};7;9;*{2}*				\r\n", 4.1, x[1, 1], request["PRSRV_PRD"].ToString()));

							// 문자열 명령 종료
							sbBuffer.Append("STOP													\r\n");

							// 5. RFID 태그 프로그래밍
							// 128p 참조
							// RFWTAG: 태그 쓰기, 96: RFID 태그에 기록할 바이트의 수
							sbBuffer.Append("RFWTAG;96												\r\n");
							// Bit Field
							// 96: 바이트 수, H: 16진수(Hex), *내용*
							sbBuffer.Append(string.Format("96;H;*{0}*								\r\n", request["TAG_ID"].ToString()));
							// RFID 종료
							sbBuffer.Append("STOP													\r\n");

							// RFRTAG: 태그 읽기, 96: RFID 태그 바이트의 수, EPC: set logical memory area(EPC 방식)
							sbBuffer.Append("RFRTAG;96;EPC											\r\n");
							// Bit Field
							// 96: 바이트 수, DF9: 결과 성공 시 9를 리턴, H: 16진수(Hex)
							sbBuffer.Append("96;DF9;H												\r\n");
							// RFID 종료
							sbBuffer.Append("STOP													\r\n");

							// 6. 프린터가 호스트에 ASCII 표현을 보내도록 진행
							// 153p 참조
							// DF9: DF9로 저장된 데이터, H: 16진수(Hex), * *: Replace 용도??
							sbBuffer.Append("VERIFY;DF9;H;* *										\r\n");
							// RFID 종료
							sbBuffer.Append("END													\r\n");

							// 7. EXECUTE 모드 진입
							// EXECUTE 모드에서의 명령어는 463p 참조
							// RFID: CREATE ALIAS, 1: 호출횟수
							// CREATE MODE에서 기록된 내용을 실행
							sbBuffer.Append("~EXECUTE;RFID;1										\r\n");
							// NORMAL 모드 진입
							sbBuffer.Append("~NORMAL												\r\n");

							;
							Encoding EUC_KR = System.Text.Encoding.UTF8;



							// 데이터 전송
							sckClient.Send(EUC_KR.GetBytes(sbBuffer.ToString()));
							byte[] bReceiveBuffer = new byte[25];
							int nReadByte = sckClient.Receive(bReceiveBuffer, 0, 25, SocketFlags.None);
							string sReceiveVal = EUC_KR.GetString(bReceiveBuffer);
							//string sReceiveVal2 = EUC_KR.GetString(bReceiveBuffer);

							if (sReceiveVal.Trim() == request["TAG_ID"].ToString())
							{
								bRtn = true;
							}
							else
							{
								sRtn = "1";
								sMessage = "VAL";
							}
						}
						else if (strPrtEqId == "PRT00000")
						{
							//double[,] x = new double[,] { { 24, 35 }, { 24, 34.7 } };
							//double[] y = new double[] { 3.4, 5.4, 7.7 };
							double[,] x = new double[,] { { 23, 34 }, { 23, 33.7 } };
							double[] y = new double[] { 2.9, 5.0, 7.0 };

							StringBuilder sbBuffer = new StringBuilder();

							// 1. NORMAL 모드 진입
							// NORMAL 모드에서의 명령어는 460p 참조
							sbBuffer.Append("~NORMAL												\r\n");
							// 2. CREATE 모드 진입
							// CREATE 모드에서의 명령어는 462p 참조
							// CREATE 상세설명은 61p 참조
							// CREATE; [/]formname [;FL] [;NOMOTION] [;DISK]
							// RFID: CREATE의 ALIAS, X: 프린터에 지정된 용지크기를 따름
							sbBuffer.Append("~CREATE;RFID;X											\r\n");

							// 3. CODE 39 TYPE의 바코드
							// 193p, 159p 참조
							// C3/9: CODE 39, XRD2:2:6:6  확대 속성, H6: 바코드 전체 높이, 4.9: Y축 위치, 3.6: X축 위치
							// X에 RD를 추가하여 사용자지정 바코드 비율 지정 가능(A:B:C:D => A: 짧은 바 길이, B: 짧은 공간 길이, C: 긴 바 길이, D: 긴 공간 길이)
							sbBuffer.Append("BARCODE												\r\n");
							sbBuffer.Append("C3/9;XRD2:2:6:6;H6;4.9;3.6								\r\n");

							// 바코드 내용
							sbBuffer.Append(string.Format("*{0}*										\r\n", request["PAMS_MGMT_NO"].ToString()));
							sbBuffer.Append("STOP													\r\n");

							// 4. FONT 지정
							// 84p 참조
							// NAME #: 파일명을 통한  폰트 지정
							sbBuffer.Append("FONT;NAME h2hdrm.ttf 									\r\n");
							// 5. 문자열(영문, 숫자)
							// 33p 참조
							sbBuffer.Append("ALPHA													\r\n");
							// POINT: 글자 크기 단위(1/72 inch), SR: Y축 위치, SC: X축 위치, VE: 글자 높이, HE: 글자 간격
							// *내용*
							sbBuffer.Append(string.Format("POINT;{0};{1};10;10;*{2}*				\r\n", y[0], x[0, 0], request["PRSDT_NM"].ToString()));

							if (request["PROD_ORG_NM"] != null && (request["PROD_ORG_NM"].ToString()).Length > 15)
							{
								sbBuffer.Append(string.Format("POINT;4.3;23;8;8;*{0}*				\r\n", (request["PROD_ORG_NM"].ToString()).Substring(0, 15)));
								//sbBuffer.Append(string.Format("POINT;5.8;24;8;8;*{0}*				\r\n", (request["PROD_ORG_NM"].ToString()).Substring(15, (request["PROD_ORG_NM"].ToString()).Length)));
								sbBuffer.Append(string.Format("POINT;5.2;23;8;8;*{0}*				\r\n", (request["PROD_ORG_NM"].ToString()).Substring(15, (request["PROD_ORG_NM"].ToString()).Length - 15)));
							}
							else if (request["PROD_ORG_NM"] != null)
							{
								sbBuffer.Append(string.Format("POINT;{0};{1};9;8;*{2}*				\r\n", y[1], x[0, 0], request["PROD_ORG_NM"].ToString()));
							}

							sbBuffer.Append(string.Format("POINT;{0};{1};14;14;*{2}*				\r\n", y[2], x[0, 0], request["PAMS_MGMT_NO"].ToString()));

							//sbBuffer.Append(string.Format("POINT;{0};{1};7;9;*{2}*				\r\n", 2.8, x[1, 1], request["PROD_STRT_YY"].ToString()));

							if (request["PROD_STRT_YY"] != null && (request["PROD_STRT_YY"].ToString()).Length > 4)
							{
								sbBuffer.Append(string.Format("POINT;{0};{1};7;6;*{2}*				\r\n", 2.2, x[1, 1], request["PROD_STRT_YY"].ToString()));
							}
							else
							{
								sbBuffer.Append(string.Format("POINT;{0};{1};7;9;*{2}*				\r\n", 2.2, x[1, 1], request["PROD_STRT_YY"].ToString()));
							}


							sbBuffer.Append(string.Format("POINT;{0};{1};7;9;*{2}*				\r\n", 3.2, x[1, 1], request["PRSRV_PRD"].ToString()));

							// 문자열 명령 종료
							sbBuffer.Append("STOP													\r\n");

							// 5. RFID 태그 프로그래밍
							// 128p 참조
							// RFWTAG: 태그 쓰기, 96: RFID 태그에 기록할 바이트의 수
							sbBuffer.Append("RFWTAG;96												\r\n");
							// Bit Field
							// 96: 바이트 수, H: 16진수(Hex), *내용*
							sbBuffer.Append(string.Format("96;H;*{0}*								\r\n", request["TAG_ID"].ToString()));
							// RFID 종료
							sbBuffer.Append("STOP													\r\n");

							// RFRTAG: 태그 읽기, 96: RFID 태그 바이트의 수, EPC: set logical memory area(EPC 방식)
							sbBuffer.Append("RFRTAG;96;EPC											\r\n");
							// Bit Field
							// 96: 바이트 수, DF9: 결과 성공 시 9를 리턴, H: 16진수(Hex)
							sbBuffer.Append("96;DF9;H												\r\n");
							// RFID 종료
							sbBuffer.Append("STOP													\r\n");

							// 6. 프린터가 호스트에 ASCII 표현을 보내도록 진행
							// 153p 참조
							// DF9: DF9로 저장된 데이터, H: 16진수(Hex), * *: Replace 용도??
							sbBuffer.Append("VERIFY;DF9;H;* *										\r\n");
							// RFID 종료
							sbBuffer.Append("END													\r\n");

							// 7. EXECUTE 모드 진입
							// EXECUTE 모드에서의 명령어는 463p 참조
							// RFID: CREATE ALIAS, 1: 호출횟수
							// CREATE MODE에서 기록된 내용을 실행
							sbBuffer.Append("~EXECUTE;RFID;1										\r\n");
							// NORMAL 모드 진입
							sbBuffer.Append("~NORMAL												\r\n");

							;
							Encoding EUC_KR = System.Text.Encoding.UTF8;



							// 데이터 전송
							sckClient.Send(EUC_KR.GetBytes(sbBuffer.ToString()));
							byte[] bReceiveBuffer = new byte[25];
							int nReadByte = sckClient.Receive(bReceiveBuffer, 0, 25, SocketFlags.None);
							string sReceiveVal = EUC_KR.GetString(bReceiveBuffer);
							//string sReceiveVal2 = EUC_KR.GetString(bReceiveBuffer);

							if (sReceiveVal.Trim() == request["TAG_ID"].ToString())
							{
								bRtn = true;
							}
							else
							{
								sRtn = "1";
								sMessage = "VAL";
							}
						}
						else if (strPrtEqId == "PRT00007")
						{
							//double[,] x = new double[,] { { 24, 35 }, { 24, 34.7 } };
							//double[] y = new double[] { 3.4, 5.4, 7.7 };
							//double[,] x = new double[,] { { 23, 34 }, { 23, 33.7 } };
							//double[] y = new double[] { 2.9, 5.0, 7.0 };

							double[,] x = new double[,] { { 23.4, 34.4 }, { 23.4, 34.4 } };
							double[] y = new double[] { 3.4, 5.4, 7.7 };

							StringBuilder sbBuffer = new StringBuilder();

							// 1. NORMAL 모드 진입
							// NORMAL 모드에서의 명령어는 460p 참조
							sbBuffer.Append("~NORMAL												\r\n");
							// 2. CREATE 모드 진입
							// CREATE 모드에서의 명령어는 462p 참조
							// CREATE 상세설명은 61p 참조
							// CREATE; [/]formname [;FL] [;NOMOTION] [;DISK]
							// RFID: CREATE의 ALIAS, X: 프린터에 지정된 용지크기를 따름
							sbBuffer.Append("~CREATE;RFID;X											\r\n");

							// 3. CODE 39 TYPE의 바코드
							// 193p, 159p 참조
							// C3/9: CODE 39, XRD2:2:6:6  확대 속성, H6: 바코드 전체 높이, 4.9: Y축 위치, 3.6: X축 위치
							// X에 RD를 추가하여 사용자지정 바코드 비율 지정 가능(A:B:C:D => A: 짧은 바 길이, B: 짧은 공간 길이, C: 긴 바 길이, D: 긴 공간 길이)
							sbBuffer.Append("BARCODE												\r\n");
							sbBuffer.Append("C3/9;XRD2:2:6:6;H6;4.9;3.6								\r\n");

							// 바코드 내용
							sbBuffer.Append(string.Format("*{0}*										\r\n", request["PAMS_MGMT_NO"].ToString()));
							sbBuffer.Append("STOP													\r\n");

							// 4. FONT 지정
							// 84p 참조
							// NAME #: 파일명을 통한  폰트 지정
							sbBuffer.Append("FONT;NAME h2hdrm.ttf 									\r\n");
							// 5. 문자열(영문, 숫자)
							// 33p 참조
							sbBuffer.Append("ALPHA													\r\n");
							// POINT: 글자 크기 단위(1/72 inch), SR: Y축 위치, SC: X축 위치, VE: 글자 높이, HE: 글자 간격
							// *내용*
							sbBuffer.Append(string.Format("POINT;{0};{1};10;10;*{2}*				\r\n", y[0], x[0, 0], request["PRSDT_NM"].ToString()));

							if (request["PROD_ORG_NM"] != null && (request["PROD_ORG_NM"].ToString()).Length > 15)
							{
								sbBuffer.Append(string.Format("POINT;4.9;23.4;8;8;*{0}*				\r\n", (request["PROD_ORG_NM"].ToString()).Substring(0, 15)));
								//sbBuffer.Append(string.Format("POINT;5.8;24;8;8;*{0}*				\r\n", (request["PROD_ORG_NM"].ToString()).Substring(15, (request["PROD_ORG_NM"].ToString()).Length)));
								sbBuffer.Append(string.Format("POINT;5.8;23.4;8;8;*{0}*				\r\n", (request["PROD_ORG_NM"].ToString()).Substring(15, (request["PROD_ORG_NM"].ToString()).Length - 15)));
							}
							else if (request["PROD_ORG_NM"] != null)
							{
								sbBuffer.Append(string.Format("POINT;{0};{1};9;8;*{2}*				\r\n", y[1], x[0, 0], request["PROD_ORG_NM"].ToString()));
							}

							sbBuffer.Append(string.Format("POINT;{0};{1};14;14;*{2}*				\r\n", y[2], x[0, 0], request["PAMS_MGMT_NO"].ToString()));

							//sbBuffer.Append(string.Format("POINT;{0};{1};7;9;*{2}*				\r\n", 2.8, x[1, 1], request["PROD_STRT_YY"].ToString()));

							if (request["PROD_STRT_YY"] != null && (request["PROD_STRT_YY"].ToString()).Length > 4)
							{
								sbBuffer.Append(string.Format("POINT;{0};{1};7;6;*{2}*				\r\n", 2.8, x[1, 1], request["PROD_STRT_YY"].ToString()));
							}
							else
							{
								sbBuffer.Append(string.Format("POINT;{0};{1};7;9;*{2}*				\r\n", 2.8, x[1, 1], request["PROD_STRT_YY"].ToString()));
							}


							sbBuffer.Append(string.Format("POINT;{0};{1};7;9;*{2}*				\r\n", 3.9, x[1, 1], request["PRSRV_PRD"].ToString()));

							// 문자열 명령 종료
							sbBuffer.Append("STOP													\r\n");

							// 5. RFID 태그 프로그래밍
							// 128p 참조
							// RFWTAG: 태그 쓰기, 96: RFID 태그에 기록할 바이트의 수
							sbBuffer.Append("RFWTAG;96												\r\n");
							// Bit Field
							// 96: 바이트 수, H: 16진수(Hex), *내용*
							sbBuffer.Append(string.Format("96;H;*{0}*								\r\n", request["TAG_ID"].ToString()));
							// RFID 종료
							sbBuffer.Append("STOP													\r\n");

							// RFRTAG: 태그 읽기, 96: RFID 태그 바이트의 수, EPC: set logical memory area(EPC 방식)
							sbBuffer.Append("RFRTAG;96;EPC											\r\n");
							// Bit Field
							// 96: 바이트 수, DF9: 결과 성공 시 9를 리턴, H: 16진수(Hex)
							sbBuffer.Append("96;DF9;H												\r\n");
							// RFID 종료
							sbBuffer.Append("STOP													\r\n");

							// 6. 프린터가 호스트에 ASCII 표현을 보내도록 진행
							// 153p 참조
							// DF9: DF9로 저장된 데이터, H: 16진수(Hex), * *: Replace 용도??
							sbBuffer.Append("VERIFY;DF9;H;* *										\r\n");
							// RFID 종료
							sbBuffer.Append("END													\r\n");

							// 7. EXECUTE 모드 진입
							// EXECUTE 모드에서의 명령어는 463p 참조
							// RFID: CREATE ALIAS, 1: 호출횟수
							// CREATE MODE에서 기록된 내용을 실행
							sbBuffer.Append("~EXECUTE;RFID;1										\r\n");
							// NORMAL 모드 진입
							sbBuffer.Append("~NORMAL												\r\n");

							;
							Encoding EUC_KR = System.Text.Encoding.UTF8;



							// 데이터 전송
							sckClient.Send(EUC_KR.GetBytes(sbBuffer.ToString()));
							byte[] bReceiveBuffer = new byte[25];
							int nReadByte = sckClient.Receive(bReceiveBuffer, 0, 25, SocketFlags.None);
							string sReceiveVal = EUC_KR.GetString(bReceiveBuffer);
							//string sReceiveVal2 = EUC_KR.GetString(bReceiveBuffer);

							if (sReceiveVal.Trim() == request["TAG_ID"].ToString())
							{
								bRtn = true;
							}
							else
							{
								sRtn = "1";
								sMessage = "VAL";
							}

						}
						else
						{
							//double[,] x = new double[,] { { 24, 35 }, { 24, 34.7 } };
							//double[] y = new double[] { 3.4, 5.4, 7.7 };

							double[,] x = new double[,] { { 23.5, 34.5 }, { 23.5, 34.5 } };
							double[] y = new double[] { 3.5, 5.5, 7.8 };

							StringBuilder sbBuffer = new StringBuilder();

							// 1. NORMAL 모드 진입
							// NORMAL 모드에서의 명령어는 460p 참조
							sbBuffer.Append("~NORMAL												\r\n");
							// 2. CREATE 모드 진입
							// CREATE 모드에서의 명령어는 462p 참조
							// CREATE 상세설명은 61p 참조
							// CREATE; [/]formname [;FL] [;NOMOTION] [;DISK]
							// RFID: CREATE의 ALIAS, X: 프린터에 지정된 용지크기를 따름
							sbBuffer.Append("~CREATE;RFID;X											\r\n");

							// 3. CODE 39 TYPE의 바코드
							// 193p, 159p 참조
							// C3/9: CODE 39, XRD2:2:6:6  확대 속성, H6: 바코드 전체 높이, 4.9: Y축 위치, 3.6: X축 위치
							// X에 RD를 추가하여 사용자지정 바코드 비율 지정 가능(A:B:C:D => A: 짧은 바 길이, B: 짧은 공간 길이, C: 긴 바 길이, D: 긴 공간 길이)
							sbBuffer.Append("BARCODE												\r\n");
							sbBuffer.Append("C3/9;XRD2:2:6:6;H6;4.9;3.6								\r\n");

							// 바코드 내용
							sbBuffer.Append(string.Format("*{0}*										\r\n", request["PAMS_MGMT_NO"].ToString()));
							sbBuffer.Append("STOP													\r\n");

							// 4. FONT 지정
							// 84p 참조
							// NAME #: 파일명을 통한  폰트 지정
							sbBuffer.Append("FONT;NAME h2hdrm.ttf 									\r\n");
							// 5. 문자열(영문, 숫자)
							// 33p 참조
							sbBuffer.Append("ALPHA													\r\n");
							// POINT: 글자 크기 단위(1/72 inch), SR: Y축 위치, SC: X축 위치, VE: 글자 높이, HE: 글자 간격
							// *내용*
							sbBuffer.Append(string.Format("POINT;{0};{1};10;10;*{2}*				\r\n", y[0], x[0, 0], request["PRSDT_NM"].ToString()));

							if (request["PROD_ORG_NM"] != null && (request["PROD_ORG_NM"].ToString()).Length > 15)
							{
								sbBuffer.Append(string.Format("POINT;5.0;23.4;8;8;*{0}*				\r\n", (request["PROD_ORG_NM"].ToString()).Substring(0, 15)));
								//sbBuffer.Append(string.Format("POINT;5.8;24;8;8;*{0}*				\r\n", (request["PROD_ORG_NM"].ToString()).Substring(15, (request["PROD_ORG_NM"].ToString()).Length)));
								sbBuffer.Append(string.Format("POINT;5.9;23.4;8;8;*{0}*				\r\n", (request["PROD_ORG_NM"].ToString()).Substring(15, (request["PROD_ORG_NM"].ToString()).Length - 15)));
							}
							else if (request["PROD_ORG_NM"] != null)
							{
								sbBuffer.Append(string.Format("POINT;{0};{1};9;8;*{2}*				\r\n", y[1], x[0, 0], request["PROD_ORG_NM"].ToString()));
							}

							sbBuffer.Append(string.Format("POINT;{0};{1};14;14;*{2}*				\r\n", y[2], x[0, 0], request["PAMS_MGMT_NO"].ToString()));

							//sbBuffer.Append(string.Format("POINT;{0};{1};7;9;*{2}*				\r\n", 2.8, x[1, 1], request["PROD_STRT_YY"].ToString()));

							if (request["PROD_STRT_YY"] != null && (request["PROD_STRT_YY"].ToString()).Length > 4)
							{
								sbBuffer.Append(string.Format("POINT;{0};{1};7;6;*{2}*				\r\n", 3.0, x[1, 1], request["PROD_STRT_YY"].ToString()));
							}
							else
							{
								sbBuffer.Append(string.Format("POINT;{0};{1};7;9;*{2}*				\r\n", 3.0, x[1, 1], request["PROD_STRT_YY"].ToString()));
							}


							sbBuffer.Append(string.Format("POINT;{0};{1};7;9;*{2}*				\r\n", 4.0, x[1, 1], request["PRSRV_PRD"].ToString()));

							// 문자열 명령 종료
							sbBuffer.Append("STOP													\r\n");

							// 5. RFID 태그 프로그래밍
							// 128p 참조
							// RFWTAG: 태그 쓰기, 96: RFID 태그에 기록할 바이트의 수
							sbBuffer.Append("RFWTAG;96												\r\n");
							// Bit Field
							// 96: 바이트 수, H: 16진수(Hex), *내용*
							sbBuffer.Append(string.Format("96;H;*{0}*								\r\n", request["TAG_ID"].ToString()));
							// RFID 종료
							sbBuffer.Append("STOP													\r\n");

							// RFRTAG: 태그 읽기, 96: RFID 태그 바이트의 수, EPC: set logical memory area(EPC 방식)
							sbBuffer.Append("RFRTAG;96;EPC											\r\n");
							// Bit Field
							// 96: 바이트 수, DF9: 결과 성공 시 9를 리턴, H: 16진수(Hex)
							sbBuffer.Append("96;DF9;H												\r\n");
							// RFID 종료
							sbBuffer.Append("STOP													\r\n");

							// 6. 프린터가 호스트에 ASCII 표현을 보내도록 진행
							// 153p 참조
							// DF9: DF9로 저장된 데이터, H: 16진수(Hex), * *: Replace 용도??
							sbBuffer.Append("VERIFY;DF9;H;* *										\r\n");
							// RFID 종료
							sbBuffer.Append("END													\r\n");

							// 7. EXECUTE 모드 진입
							// EXECUTE 모드에서의 명령어는 463p 참조
							// RFID: CREATE ALIAS, 1: 호출횟수
							// CREATE MODE에서 기록된 내용을 실행
							sbBuffer.Append("~EXECUTE;RFID;1										\r\n");
							// NORMAL 모드 진입
							sbBuffer.Append("~NORMAL												\r\n");

							;
							Encoding EUC_KR = System.Text.Encoding.UTF8;



							// 데이터 전송
							sckClient.Send(EUC_KR.GetBytes(sbBuffer.ToString()));
							byte[] bReceiveBuffer = new byte[25];
							int nReadByte = sckClient.Receive(bReceiveBuffer, 0, 25, SocketFlags.None);
							string sReceiveVal = EUC_KR.GetString(bReceiveBuffer);
							//string sReceiveVal2 = EUC_KR.GetString(bReceiveBuffer);

							if (sReceiveVal.Trim() == request["TAG_ID"].ToString())
							{
								bRtn = true;
							}
							else
							{
								sRtn = "1";
								sMessage = "VAL";
							}

						}
					}



				}
            }
            catch (SocketException se)
            {
                //Program.AppLog.Write(this, "소켓 연결 오류[" + se.Message + "]");
                //Program.AppLog.Write(this, "lock oSocketCO Exit");
                sRtn = "1";
                sMessage = "OUT";

            }
            catch (Exception e)
            {
                // Program.SysLog.Write(this, "예상 되지 않은 오류!", e);
                //Program.AppLog.Write(this, "lock oSocketCO Exit");
                sRtn = "1";
                sMessage = "ERR";

            }
            finally
            {
                sckClient.Close();
                sckClient.Dispose();
            }

            //IN_ISSUE_REQ_DTTM IN  VARCHAR2,  --ISSUE_REQ_DTTM
            //IN_EQ_ID IN  VARCHAR2,  --장비ID
            //IN_PRT_EQ_ID IN  VARCHAR2,  --프린터 장비 ID
            //IN_TAG_ID IN  VARCHAR2,  --TAG_ID
            //IN_ISSUE_RESULT IN  VARCHAR2,  --USER_ID
            //IN_TRANS_TP IN  VARCHAR2,  --TRANS_TP
            //IN_TRANS_ERR IN  VARCHAR2,  --TRANS_ERR

            Hashtable htParamsHashtable = new Hashtable();
            htParamsHashtable.Clear();
            htParamsHashtable.Add("IN_ISSUE_REQ_DTTM", request["ISSUE_REQ_DTTM"].ToString());
            htParamsHashtable.Add("IN_EQ_ID", request["EQ_ID"].ToString());
            htParamsHashtable.Add("IN_PRT_EQ_ID", request["PRT_EQ_ID"].ToString());
            htParamsHashtable.Add("IN_TAG_ID", request["TAG_ID"].ToString());
            htParamsHashtable.Add("IN_ISSUE_RESULT", sMessage);
            htParamsHashtable.Add("IN_TRANS_TP", request["TRANS_TP"] == null ? "" : request["TRANS_TP"].ToString());
            htParamsHashtable.Add("IN_TRANS_ERR", request["TRANS_ERR"] == null ? "" : request["TRANS_ERR"].ToString());

            ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_ISSUE_REQ_UPDATE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
            string strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
            string strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

            return bRtn;
        }

        public static byte[] HexToByte(string hex)
        {
            byte[] convert = new byte[hex.Length / 2];

            int length = convert.Length;
            for (int i = 0; i < length; i++)
            {
                convert[i] = Convert.ToByte(hex.Substring(i * 2), 16);
            }

            return convert;
        }

        public static string ByteHexToHexString(byte[] hex)
        {
            string result = string.Empty;
            foreach (byte c in hex)
                result += c.ToString("x2").ToUpper();
            return result;
        }

        public static byte[] MakePacket(string strSendBuffer)
        {
            byte[] sendBuffer;
            byte[] tempBuffer;
            ushort nCRCValue;
            //            string strBuffer;
            string strCRC;

            Encoding EUC_KR = Encoding.GetEncoding("euc-kr");

            //try
            {
                tempBuffer = EUC_KR.GetBytes(strSendBuffer);
                sendBuffer = new byte[tempBuffer.Length];

                System.Buffer.BlockCopy(tempBuffer, 0, sendBuffer, 1, tempBuffer.Length);
                return sendBuffer;
            }


        }
    }
}

