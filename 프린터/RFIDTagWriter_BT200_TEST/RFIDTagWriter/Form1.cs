using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RFIDTagWriter
{
	public partial class Form1 : Form
	{

		private bool m_bStarted;


		private int m_iConnectionType;
		private string m_strIpAddr;
		private int iPort = 9100;
		private string sModel = "BT-200";

		private bool bConnect = false;


		private DataTable m_dataTable;

		private string sPjtPath = System.Windows.Forms.Application.StartupPath;

		/// <summary>
		/// 기본 처리 서식
		/// </summary>
		string sDefaultFile = System.Windows.Forms.Application.StartupPath + "\\" + "TagPrintSetting" + "\\" + "BT200_TEST_FORM.txt";
		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			rdoConnUSB.Checked = false;
			rdoConnTCP.Checked = true;
			m_iConnectionType = 2;
		}

		private void Form1_Shown(object sender, EventArgs e)
		{

		}

		private void rdoConnUSB_Click(object sender, EventArgs e)
		{
			m_iConnectionType = 1;
		}

		private void rdoConnTCP_CheckedChanged(object sender, EventArgs e)
		{
			m_iConnectionType = 2;
		}



		private void TestData()
		{
			DataTable dt = new DataTable();

			dt.Columns.Add("TAG_ID", typeof(string));       //태그ID
			dt.Columns.Add("PAMS_MGMT_NO", typeof(string)); //관리번호
			dt.Columns.Add("PRSDT_NM", typeof(string));     //대통령
			dt.Columns.Add("PROD_ORG_NM", typeof(string));  //샌산기관
			dt.Columns.Add("PROD_STRT_YY", typeof(string)); //생산년도
			dt.Columns.Add("PRSRV_PRD", typeof(string));    //보존기관


			dt.Rows.Add("0544092CB48E04B430C313B8", "PAA0000001", "김대중", "대통령비서실", "(2021,2022)", "영구");
			dt.Rows.Add("0544092CB48E04B430CF3DB1", "PED0000008", "김대중", "대통령비서실", "(2021,2022)", "영구");
			dt.Rows.Add("0544092CB48E04B430D723D9", "PKE0000104", "김대중", "대통령비서실", "(2021,2022)", "영구");

			//dt.Rows.Add("0544092CB48E04B430D723D9", "PKE0000104", "김대중", "대통령비서실", "(2021,2022)", "영구");
			//dt.Rows.Add("0544092CB48E04B430D723D8", "PLE0000005", "문재인", "대통령비서실", "(2021,2022)", "영구");
			//dt.Rows.Add("0544092CB48E04B430D723D8", "PLE0000160", "문재인", "대통령비서실", "(2021,2022)", "영구");



			//dt.Rows.Add("0544092CB48E04B430D7221A", "PLE0000234", "김대중", "대통령비서실", "(2021,2022)", "영구");
			//dt.Rows.Add("0544092CB48E04B430D70188", "PNE0000214", "문재인", "대통령비서실", "(2021,2022)", "영구");
			//dt.Rows.Add("0544092CB48E04B430D054C4", "POE0000276", "문재인", "대통령비서실", "(2021,2022)", "영구");


			//23.11.07 반출 데이터
			//dt.Rows.Add("0544092CB48E04B430D8F14B", "PED0000010", "김대중", "대통령비서실", "2004", "2023-10-23");
			//dt.Rows.Add("0544092CB48E04B430D11489", "PQA0000001", "문재인", "대통령비서실", "(2021,2022)", "영구");
			//dt.Rows.Add("0544092CB48E04B430C34482", "TEST000008", "문재인", "대통령비서실", "(2021,2022)", "영구");

			//dt.Rows.Add("0544092CB48E04B430C34482", "TEST000008", "문재인", "대통령비서실", "(2021,2022)", "영구");
			//dt.Rows.Add("0544092CB48E04B430CD2C4F", "PED0000008", "김대중", "대통령비서실", "2004", "2023-10-23");
			//dt.Rows.Add("0544092CB48E04B430CD21CF", "PED0000008", "박정희", "대통령비서실", "1979", "준영구");


			m_dataTable = dt;
			
		}


		

		private void TagPrintTest3()
		{
			int iType = 2;
			int iPort = 9100;
			string sModel = "BT-200";
			int iConnect = 0;   // 0 : 연결안됨, 1:연결됨

			try
			{
				string sFileName = string.Empty;

				for (int iRow = 0; iRow < 3; iRow++)
				{
					iConnect = WrapperTagPrintX.IsConnected();
					if (iConnect != 1)
					{
						m_strIpAddr = tbIpAddr.Text;
						int iRtn = WrapperTagPrintX.Connect(2, 9100, m_strIpAddr, sModel);
						if (iRtn == 0) break;
					}

				}

				iConnect = WrapperTagPrintX.IsConnected();

				
				//명령어 전달
				string strCmd = "&V1 getval \"eth_ip\"";
				//string strCmd = "&V1 getval \"printer\"";
				StringBuilder sb = new StringBuilder();
				int iVal = WrapperTagPrintX.ExecuteBT200InternalCommand(strCmd, sb);
				string strTestName = sb.ToString();

                StringBuilder sbRsp = new StringBuilder();
                WrapperTagPrintX.ReadResponse(sbRsp);


            }
			catch (Exception ex)
			{
				MessageBox.Show("테스트중 에러가 발생하였습니다.\n에러코드 : " + ex.ToString());
			}
			finally
			{

			}
		}




		private string GetVariableDataFromDataTable(int idx)
		{
			/*
			dt.Columns.Add("TAG_ID", typeof(string));       //태그ID
			dt.Columns.Add("PAMS_MGMT_NO", typeof(string)); //관리번호
			dt.Columns.Add("PRSDT_NM", typeof(string));     //대통령
			dt.Columns.Add("PROD_ORG_NM", typeof(string));  //샌산기관
			dt.Columns.Add("PROD_STRT_YY", typeof(string)); //생산년도
			dt.Columns.Add("PRSRV_PRD", typeof(string));    //보존기관


			dt.Rows.Add("0544092CB48E04B430C313B8", "PAA0000001", "김대중", "대통령비서실", "(2021,2022)", "영구");
			 */


			string strVariable = "";
			//24.07.23 수정
			//if (!m_bUseVdp)
			//	return strVariable;

			//foreach (DataColumn col in m_dataTable.Columns)
			//{
			//	strVariable += (m_dataTable.Rows[idx][col].ToString() + "!!");
			//}

			
			strVariable += (m_dataTable.Rows[idx]["TAG_ID"].ToString() + "!!");
			strVariable += (m_dataTable.Rows[idx]["PAMS_MGMT_NO"].ToString() + "!!");
			strVariable += (m_dataTable.Rows[idx]["PRSDT_NM"].ToString() + "!!");
			strVariable += (m_dataTable.Rows[idx]["PROD_ORG_NM"].ToString() + "!!");
			strVariable += (m_dataTable.Rows[idx]["PROD_STRT_YY"].ToString() + "!!");
			strVariable += (m_dataTable.Rows[idx]["PRSRV_PRD"].ToString());


			return strVariable;
		}

		

		

		



		

		

		

        private void btnSelect_Click(object sender, EventArgs e)
        {

			WrapperTagPrintX.SetUp();
		}




		private void btnConnect_Click(object sender, EventArgs e)
        {
			if (Connect())
			{
				bConnect = true;
				lblMsg.Text = "프린터 연결 성공";
			}
			else
			{
				bConnect = false;
				lblMsg.Text = "프린터 연결 실패";
			}

		}

		private bool Connect()
		{
			bool btn = false;
			int iConnect = 0;   // 0 : 연결안됨, 1:연결됨

			for (int iRow = 0; iRow < 3; iRow++)
			{
				iConnect = WrapperTagPrintX.IsConnected();

				if (iConnect == 1)
				{
					btn = true;
					break;
				}
				else
				{
					if (m_iConnectionType == 2)
					{
						m_strIpAddr = tbIpAddr.Text;
						int iRtn = WrapperTagPrintX.Connect(2, 9100, m_strIpAddr, sModel);
						if (iRtn == 0)
						{
							btn = true;
							break;
						}
					}
					else if (m_iConnectionType == 1)
					{
						int iRtn = WrapperTagPrintX.Connect(1, 0, "", sModel);
						if (iRtn == 0)
						{
							btn = true;
							break;
						}
					}
				}

			}

			return btn;
		}

        private void bntAlive_Click(object sender, EventArgs e)
        {
			bool btn = false;
			int iConnect = 0;   // 0 : 연결안됨, 1:연결됨

			iConnect = WrapperTagPrintX.IsConnected();

			if (iConnect == 1)
			{
				btn = true;
			}
			else
			{
				if (m_iConnectionType == 2)
				{
					m_strIpAddr = tbIpAddr.Text;
					int iRtn = WrapperTagPrintX.Connect(2, 9100, m_strIpAddr, sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
				else if (m_iConnectionType == 1)
				{
					int iRtn = WrapperTagPrintX.Connect(1, 0, "", sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
			}

			if (btn)
			{
				lblMsg.Text = "프린터 연결 성공";
				MessageBox.Show("프린터 연결 성공");
			}
			else
			{
				lblMsg.Text = "프린터 연결 실패";
				MessageBox.Show("프린터 연결 실패");
			}
		}

		private void btnClosePort_Click(object sender, EventArgs e)
		{
			bool btn = false;
			int iConnect = 0;   // 0 : 연결안됨, 1:연결됨
			int iClosePort = -1;      //수행결과  0 : 성공, 1 : 실패

			iConnect = WrapperTagPrintX.IsConnected();

			if (iConnect == 1)
			{
				btn = true;

				iClosePort = WrapperTagPrintX.ClosePort("");
			}
			

			if (iClosePort == 0)
			{
				lblMsg.Text = "프린터 연결 닫기";
				MessageBox.Show("프린터 연결 닫기 성공");
			}
			else
			{
				lblMsg.Text = "프린터 연결 닫기 실패";
				MessageBox.Show("프린터 연결 닫기 실패");
			}
		}

        private void btnSendStr_Click(object sender, EventArgs e)
        {
			bool btn = false;
			int iConnect = WrapperTagPrintX.IsConnected();

			if (iConnect == 1)
			{
				btn = true;
			}
			else
			{
				if (m_iConnectionType == 2)
				{
					m_strIpAddr = tbIpAddr.Text;
					int iRtn = WrapperTagPrintX.Connect(2, 9100, m_strIpAddr, sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
				else if (m_iConnectionType == 1)
				{
					int iRtn = WrapperTagPrintX.Connect(1, 0, "", sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
			}

			if (!btn)
			{
				MessageBox.Show("프린터 연결이 실패.");
				return;
			}


            string zpl = @"^XA
^LL599
^LH75,130
^A@N,35,33,E:YOONGOTHIC_540.TTF
^FO0,15^GB1020,250,6^FS
^FO0,63^GB1020,0,5^FS
^FO0,111^GB1020,0,5^FS
^FO0,159^GB1020,0,5^FS
^FO0,207^GB1020,0,5^FS
^FO148,15^GB0,192,5^FS
^FO592,15^GB0,100,5^FS
^FO742,15^GB0,100,5^FS
^FO20,28^A@N,35,33
^FD분류번호^FS
^FO612,28^A@N,35,33
^FD취득단가^FS
^FO20,76^A@N,35,33
^FD품     명^FS
^FO612,76^A@N,35,33
^FD취득일자^FS
^FO20,124^A@N,35,33
^FD규 격 명^FS
^FO20,172^A@N,35,33
^FD비     고^FS
^FO162,28^A@N,35,33
^FD^KKR-PA-00000001^FS
^FO756,28^A@N,35,33
^FD^1,000,000원^FS
^FO162,76^A@N,35,33
^FD^업무용 노트북^FS
^FO756,76^A@N,35,33
^FD^2022-02-01^FS
^FO162,124^A@N,35,33
^FD^삼성전자, 노트북 i7, 1GB, 규격^FS
^FO162,172^A@N,35,33
^FD^TEST1111^FS
^FO20,220^A@N,35,33
^FD^TEST1112^FS
^RFW,H,1,18,1
^41B1112233445566778899AABBCCDDEE0001^FS
^PQ1
^XZ
";

            //			string zpl = @"^XA

            //^FX Top section with logo, name and address.
            //^CF0,60
            //^FO50,50^GB100,100,100^FS
            //^FO75,75^FR^GB100,100,100^FS
            //^FO93,93^GB40,40,40^FS
            //^FO220,50^FDIntershipping, Inc.^FS
            //^CF0,30
            //^FO220,115^FD1000 Shipping Lane^FS
            //^FO220,155^FDShelbyville TN 38102^FS
            //^FO220,195^FDUnited States (USA)^FS
            //^FO50,250^GB700,3,3^FS

            //^FX Second section with recipient address and permit information.
            //^CFA,30
            //^FO50,300^FDJohn Doe^FS
            //^FO50,340^FD100 Main Street^FS
            //^FO50,380^FDSpringfield TN 39021^FS
            //^FO50,420^FDUnited States (USA)^FS
            //^CFA,15
            //^FO600,300^GB150,150,3^FS
            //^FO638,340^FDPermit^FS
            //^FO638,390^FD123456^FS
            //^FO50,500^GB700,3,3^FS

            //^FX Third section with bar code.
            //^BY5,2,270
            //^FO100,550^BC^FD12345678^FS

            //^FX Fourth section (the two boxes on the bottom).
            //^FO50,900^GB700,250,3^FS
            //^FO400,900^GB3,250,3^FS
            //^CF0,40
            //^FO100,960^FDCtr. X34B-1^FS
            //^FO100,1010^FDREF1 F00B47^FS
            //^FO100,1060^FDREF2 BL4H8^FS
            //^CF0,190
            //^FO470,955^FDCA^FS

            //^XZ";

            WrapperTagPrintX.SendStr(zpl);


			//StringBuilder sb = new StringBuilder();
			//int iVal = WrapperTagPrintX.ExecuteBT200InternalCommand(zpl, sb);

		}

        private void btnInternalCommand_Click(object sender, EventArgs e)
        {
			bool btn = false;
			int iConnect = WrapperTagPrintX.IsConnected();

			if (iConnect == 1)
			{
				btn = true;
			}
			else
			{
				if (m_iConnectionType == 2)
				{
					m_strIpAddr = tbIpAddr.Text;
					int iRtn = WrapperTagPrintX.Connect(2, 9100, m_strIpAddr, sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
				else if (m_iConnectionType == 1)
				{
					int iRtn = WrapperTagPrintX.Connect(1, 0, "", sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
			}

			if (!btn)
			{
				MessageBox.Show("프린터 연결이 실패.");
				return;
			}

			string strCmd = "&V1 getval \"printer\"";
			
			StringBuilder sb = new StringBuilder();
			WrapperTagPrintX.ExecuteBT200InternalCommand(strCmd, sb);
			string sVal = sb.ToString();

			MessageBox.Show(sVal);

		}

        private void button1_Click(object sender, EventArgs e)
        {
			bool btn = false;
			int iConnect = WrapperTagPrintX.IsConnected();

			if (iConnect == 1)
			{
				btn = true;
			}
			else
			{
				if (m_iConnectionType == 2)
				{
					m_strIpAddr = tbIpAddr.Text;
					int iRtn = WrapperTagPrintX.Connect(2, 9100, m_strIpAddr, sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
				else if (m_iConnectionType == 1)
				{
					int iRtn = WrapperTagPrintX.Connect(1, 0, "", sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
			}

			if (!btn)
			{
				MessageBox.Show("프린터 연결이 실패.");
				return;
			}

			string strCmd = "&V1 getval \"eth_ip\"";

			StringBuilder sb = new StringBuilder();
			WrapperTagPrintX.ExecuteBT200InternalCommand(strCmd, sb);
			string sVal = sb.ToString();

			MessageBox.Show(sVal);
		}

        private void button2_Click(object sender, EventArgs e)
        {
			bool btn = false;
			int iConnect = WrapperTagPrintX.IsConnected();

			if (iConnect == 1)
			{
				btn = true;
			}
			else
			{
				if (m_iConnectionType == 2)
				{
					m_strIpAddr = tbIpAddr.Text;
					int iRtn = WrapperTagPrintX.Connect(2, 9100, m_strIpAddr, sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
				else if (m_iConnectionType == 1)
				{
					int iRtn = WrapperTagPrintX.Connect(1, 0, "", sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
			}

			if (!btn)
			{
				MessageBox.Show("프린터 연결이 실패.");
				return;
			}

			string strCmd = "&V1 getval \"speed\"";

			StringBuilder sb = new StringBuilder();
			WrapperTagPrintX.ExecuteBT200InternalCommand(strCmd, sb);
			string sVal = sb.ToString();

			MessageBox.Show(sVal);
		}

        private void btnPrint_Click(object sender, EventArgs e)
        {
			string sVariable = "1" + "!!" 
								+ "112233445566778899AABBCCDDEE0001" + "!!" 
								+ "KKR-PA-00000001" + "!!" 
								+ "1,000,000원" + "!!" 
								+ "업무용 노트북" + "!!" 
								+ "2022-02-01"+ "!!"
								+ "삼성전자, 노트북 i7, 1GB, 규격" + "!!"
								+ "" + "!!"
								+ "";

			StringBuilder sbResult = new StringBuilder(512);
			StringBuilder sbErrorMessage = new StringBuilder(128);

			WrapperTagPrintX.SetFileName(sDefaultFile);
			WrapperTagPrintX.SetVariableData(sVariable, "!!");

			bool btn = false;
			int iConnect = WrapperTagPrintX.IsConnected();

			if (iConnect == 1)
			{
				btn = true;
			}
			else
			{
				if (m_iConnectionType == 2)
				{
					m_strIpAddr = tbIpAddr.Text;
					int iRtn = WrapperTagPrintX.Connect(2, 9100, m_strIpAddr, sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
				else if (m_iConnectionType == 1)
				{
					int iRtn = WrapperTagPrintX.Connect(1, 0, "", sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
			}

			if (!btn)
			{
				MessageBox.Show("프린터 연결이 실패.");
				return;
			}

			int iResult = WrapperTagPrintX.TagPrint(sbResult, sbErrorMessage);

			if (iResult == 0)
			{
				MessageBox.Show("Print 성공");
			}
			else
			{
				MessageBox.Show("Print 실패" + iResult.ToString());
			}


		}

        private void button3_Click(object sender, EventArgs e)
        {
			string sVariable = "문재인" + "!!" + "2018" + "!!" + "영구" + "!!" + "대통령비서실대통령비서실하나" + "!!" + "PQD0000354" + "!!" + "0544092CB48E04B430DF00B3";
			//string sVariable = "박정희" + "!!" + "2012" + "!!"+ "준영구" + "!!" + "대통령비서실" + "!!" + "PQA0027984" + "!!" + "0544092CB48E04B430DF0042";
			//string sVariable = "김대중" + "!!" + "2014" + "!!" + "준영구" + "!!" + "국정기획자문위원회하나둘셋넷위원회하나둘셋" + "!!" + "PQA0007984" + "!!" + "0544092CB48E04B430DF0042";
			//string sVariable = "김대중" + "!!" + "2014";

			StringBuilder sbResult = new StringBuilder(512);
			StringBuilder sbErrorMessage = new StringBuilder(128);

			string[] separatingStrings = { "!!" };
			string[] sArrData = sVariable.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);

			//string sPrintFile = @"D:\Project\대통령기록관\개발팀 정리\RfidPrint\수정\RFIDTagWriter_BT200_TEST\RFIDTagWriter\bin\Debug\TagPrintSetting\BT200_PRINT_1.txt";
			string sPrintFile = System.Windows.Forms.Application.StartupPath + "\\" + "TagPrintSetting" + "\\" + "BT200_PRINT_1.txt";
			string sPrintFile2 = System.Windows.Forms.Application.StartupPath + "\\" + "TagPrintSetting" + "\\" + "BT200_PRINT_1_2.txt";
			string sPrintFile3 = System.Windows.Forms.Application.StartupPath + "\\" + "TagPrintSetting" + "\\" + "BT200_PRINT_1_3.txt";
			//string sPrintFile = @"D:\Project\대통령기록관\개발팀 정리\RfidPrint\수정\RFIDTagWriter_BT200_TEST\RFIDTagWriter\bin\Debug\TagPrintSetting\BT200_PRINT_2.txt";

			if (sArrData[3].Trim().Length < 15)
			{
				WrapperTagPrintX.SetFileName(sPrintFile);
			}
			else if (sArrData[3].Trim().Length >= 15 && sArrData[3].Trim().Length <= 28)
			{
				int iLength = sArrData[3].Trim().Length;

				//int iRest = iLength % 2;
				//int iLen = iLength / 2;


				WrapperTagPrintX.SetFileName(sPrintFile2);
				string strPROD_ORG_NM1 = string.Empty;
				string strPROD_ORG_NM2 = string.Empty;
				//strPROD_ORG_NM1 = sArrData[3].Substring(0, (iLen + iRest)).PadLeft(14);
				//strPROD_ORG_NM2 = sArrData[3].Substring( (iLen + iRest), sArrData[3].Length - (iLen + iRest)).PadLeft(14);

				strPROD_ORG_NM1 = sArrData[3].Substring(0, 14);
				strPROD_ORG_NM2 = sArrData[3].Substring(14);



				sVariable = sArrData[0] + "!!" + sArrData[1] + "!!" + sArrData[2] + "!!" + strPROD_ORG_NM1 + "!!" + sArrData[4] + "!!" + sArrData[5]
									 + "!!" + strPROD_ORG_NM2;

			}
			else
			{
				WrapperTagPrintX.SetFileName(sPrintFile3);

				string strPROD_ORG_NM1 = string.Empty;
				string strPROD_ORG_NM2 = string.Empty;
				strPROD_ORG_NM1 = sArrData[3].Substring(0, 19);
				strPROD_ORG_NM2 = sArrData[3].Substring(19);

				sVariable = sArrData[0] + "!!" + sArrData[1] + "!!" + sArrData[2] + "!!" + strPROD_ORG_NM1 + "!!" + sArrData[4] + "!!" + sArrData[5]
									 + "!!" + strPROD_ORG_NM2;
			}

			
			WrapperTagPrintX.SetVariableData(sVariable, "!!");

			bool btn = false;
			int iConnect = WrapperTagPrintX.IsConnected();

			if (iConnect == 1)
			{
				btn = true;
			}
			else
			{
				if (m_iConnectionType == 2)
				{
					m_strIpAddr = tbIpAddr.Text;
					int iRtn = WrapperTagPrintX.Connect(2, 9100, m_strIpAddr, sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
				else if (m_iConnectionType == 1)
				{
					int iRtn = WrapperTagPrintX.Connect(1, 0, "", sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
			}

			if (!btn)
			{
				//MessageBox.Show("프린터 연결이 실패.");
				lblMsg.Text = "프린터 연결이 실패.";
				return;
			}

			StringBuilder sbResult2 = new StringBuilder(512);
			WrapperTagPrintX.ReadResponse(sbResult2);

			lblMsg.Text = "ReadResponse :" + sbResult2.ToString();

			int iResult = WrapperTagPrintX.TagPrint(sbResult, sbErrorMessage);

			if (iResult == 0)
			{
				//MessageBox.Show("Print 성공");
				lblMsg.Text = "Print 성공";
			}
			else
			{
				string shex = Convert.ToString(iResult, 16);
				//MessageBox.Show("Print 실패" + iResult.ToString() + "hex:" + shex);
				lblMsg.Text = "Print 실패" + iResult.ToString() + "hex: " + shex;
			}

		}

        private void button4_Click(object sender, EventArgs e)
        {
			string sVariable = "문재인" + "!!" + "2019" + "!!" + "미분류" + "!!" + "3·1운동 및 대한민국임시정부수립100주년 기념사업추진위원회" + "!!" + "PQA0012373-99" + "!!" + "1111092CB48E04B430D54384";
			//string sVariable = "문재인" + "!!" + "2021" + "!!" + "영구" + "!!" + "정책기획위원회정책기획위원회정책기획위원회정책기획위원회원회" + "!!" + "PQA0029891-02" + "!!" + "0544092CB48E04B430DF0042";
			//string sVariable = "김대중" + "!!" + "2014" + "!!" + "준영구" + "!!" + "국정기획자문위원회하나둘셋넷다섯" + "!!" + "PQA0008153-01" + "!!" + "0544092CB48E04B430DF0041";
			//string sVariable = "김대중" + "!!" + "2014";

			StringBuilder sbResult = new StringBuilder(512);
			StringBuilder sbErrorMessage = new StringBuilder(128);

			string[] separatingStrings = { "!!" };
			string[] sArrData = sVariable.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);

			//string sPrintFile = @"D:\Project\대통령기록관\개발팀 정리\RfidPrint\수정\RFIDTagWriter_BT200_TEST\RFIDTagWriter\bin\Debug\TagPrintSetting\BT200_PRINT_1.txt";
			//string sPrintFile = @"D:\Project\대통령기록관\개발팀 정리\RfidPrint\수정\RFIDTagWriter_BT200_TEST\RFIDTagWriter\bin\Debug\TagPrintSetting\BT200_PRINT_2.txt";
			string sPrintFile = System.Windows.Forms.Application.StartupPath + "\\" + "TagPrintSetting" + "\\" + "BT200_PRINT_2.txt";
			string sPrintFile2 = System.Windows.Forms.Application.StartupPath + "\\" + "TagPrintSetting" + "\\" + "BT200_PRINT_2_2.txt";
			string sPrintFile3 = System.Windows.Forms.Application.StartupPath + "\\" + "TagPrintSetting" + "\\" + "BT200_PRINT_2_3.txt";

			if (sArrData[3].Trim().Length < 15)
			{
				WrapperTagPrintX.SetFileName(sPrintFile);
			}
			else if (sArrData[3].Trim().Length >= 15 && sArrData[3].Trim().Length <= 28)
			{
				int iLength = sArrData[3].Trim().Length;

				//int iRest = iLength % 2;
				//int iLen = iLength / 2;


				WrapperTagPrintX.SetFileName(sPrintFile2);
				string strPROD_ORG_NM1 = string.Empty;
				string strPROD_ORG_NM2 = string.Empty;
				//strPROD_ORG_NM1 = sArrData[3].Substring(0, (iLen + iRest)).PadLeft(14);
				//strPROD_ORG_NM2 = sArrData[3].Substring( (iLen + iRest), sArrData[3].Length - (iLen + iRest)).PadLeft(14);

				strPROD_ORG_NM1 = sArrData[3].Substring(0, 14);
				strPROD_ORG_NM2 = sArrData[3].Substring(14);



				sVariable = sArrData[0] + "!!" + sArrData[1] + "!!" + sArrData[2] + "!!" + strPROD_ORG_NM1 + "!!" + sArrData[4] + "!!" + sArrData[5]
									 + "!!" + strPROD_ORG_NM2;

			}
			else
			{
				WrapperTagPrintX.SetFileName(sPrintFile3);

				string strPROD_ORG_NM1 = string.Empty;
				string strPROD_ORG_NM2 = string.Empty;
				strPROD_ORG_NM1 = sArrData[3].Substring(0, 19);
				strPROD_ORG_NM2 = sArrData[3].Substring(19);

				sVariable = sArrData[0] + "!!" + sArrData[1] + "!!" + sArrData[2] + "!!" + strPROD_ORG_NM1 + "!!" + sArrData[4] + "!!" + sArrData[5]
									 + "!!" + strPROD_ORG_NM2;
			}


			WrapperTagPrintX.SetVariableData(sVariable, "!!");

			bool btn = false;
			int iConnect = WrapperTagPrintX.IsConnected();

			if (iConnect == 1)
			{
				btn = true;
			}
			else
			{
				if (m_iConnectionType == 2)
				{
					m_strIpAddr = tbIpAddr.Text;
					int iRtn = WrapperTagPrintX.Connect(2, 9100, m_strIpAddr, sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
				else if (m_iConnectionType == 1)
				{
					int iRtn = WrapperTagPrintX.Connect(1, 0, "", sModel);
					if (iRtn == 0)
					{
						btn = true;
					}
				}
			}

			if (!btn)
			{
				//MessageBox.Show("프린터 연결이 실패.");
				lblMsg.Text = "프린터 연결이 실패.";
				return;
			}

			StringBuilder sbResult2 = new StringBuilder(512);
			WrapperTagPrintX.ReadResponse(sbResult2);

			lblMsg.Text = "ReadResponse :" + sbResult2.ToString();

			int iResult = WrapperTagPrintX.TagPrint(sbResult, sbErrorMessage);

			if (iResult == 0)
			{
				//MessageBox.Show("Print 성공");
				lblMsg.Text = "Print 성공";
			}
			else
			{
				string shex = Convert.ToString(iResult, 16);
				//MessageBox.Show("Print 실패" + iResult.ToString() + "hex:" + shex);
				lblMsg.Text = "Print 실패" + iResult.ToString() + "hex:" + shex;
			}

		}
    }
}
