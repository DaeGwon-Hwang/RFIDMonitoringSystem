using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RFIDTagWriter
{
    

    public partial class MainForm : Form
    {
        private volatile IDatabaseManagement DBManagement;
        int DB_Timeout = 30000;
        private Thread PrintThread = null;

        bool bPrint = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            DBManagement = new DatabaseManagementOracle();


            if (!DBManagement.Connect())
            {
                Program.AppLog.Write(this, "DB Connection Error!");
                //MessageBox.Show("DB 연결 오류가 발생하였습니다. 시스템 로그를 확인하여 DB연결설정을 변경하시기 바랍니다!", "Database 연결오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AddLogList("DB 연결 오류가 발생하였습니다. 시스템 로그를 확인하여 DB연결설정을 변경하시기 바랍니다!");
            }

            DataSet dsTest = DBManagement.ExecuteSelectQuery("SELECT TO_CHAR(SYSTIMESTAMP, 'YYYYMMDDHH24MISSFF3') FROM DUAL", 1);

            PrintThread = new Thread(new ThreadStart(Print));
            PrintThread.IsBackground = true;
            PrintThread.Start();
            bPrint = true;

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bPrint = false;
        }

        private void Print()
        {
            while (bPrint)
            {
                try
                {


                    Hashtable htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();

                    DataSet dsResult = DBManagement.ExecuteSelectProcedure("PAMSDATA.TRFID_ISSUE_REQ_PRINT_LIST2", htParamsHashtable, DB_Timeout);

                    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in dsResult.Tables[0].Rows)
                        {
                            Dictionary<string, object> param = new Dictionary<string, object>();

                            foreach (DataColumn column in dsResult.Tables[0].Columns)
                            {
                                param.Add(column.ColumnName, row[column.ColumnName]);
                            }

                            printBt200(param);
                            Thread.Sleep(100);
                        }
                    }
                }
                catch (Exception e)
                {
                    Program.SysLog.Write(this, "print 오류", e);
                }


                Thread.Sleep(1000 * 5);

            }
        }

        static string sPrintFile = System.Windows.Forms.Application.StartupPath + "\\" + "TagPrintSetting" + "\\" + "BT200_PRINT_1.txt";
        static string sPrintFile2 = System.Windows.Forms.Application.StartupPath + "\\" + "TagPrintSetting" + "\\" + "BT200_PRINT_1_2.txt";
        static string sPrintFile3 = System.Windows.Forms.Application.StartupPath + "\\" + "TagPrintSetting" + "\\" + "BT200_PRINT_1_3.txt";

        static string sPrintFileExt = System.Windows.Forms.Application.StartupPath + "\\" + "TagPrintSetting" + "\\" + "BT200_PRINT_2.txt";
        static string sPrintFileExt2 = System.Windows.Forms.Application.StartupPath + "\\" + "TagPrintSetting" + "\\" + "BT200_PRINT_2_2.txt";
        static string sPrintFileExt3 = System.Windows.Forms.Application.StartupPath + "\\" + "TagPrintSetting" + "\\" + "BT200_PRINT_2_3.txt";


        public  bool printBt200(Dictionary<string, object> request)
        {
            string sRtn = "0";
            string sMessage = "ISS";
            string sModel = "BT-200";
            int iClosePort = 0;

            bool bRtn = false;

            string strServerIP = request["IP_ADDR"].ToString();//"192.168.200.238";
            string strPrtEqId = request["PRT_EQ_ID"].ToString();
            bool bConnect = false;

            string sVariable = string.Empty;
            StringBuilder sbResult = new StringBuilder(512);
            StringBuilder sbErrorMessage = new StringBuilder(128);

            try
            {

                #region 프린터 연결 상태 체크(BT-200)
                int iConnect = WrapperTagPrintX.IsConnected();

                if (iConnect == 1)
                {
                    bConnect = true;
                }
                else
                {
                    int iRtn = WrapperTagPrintX.Connect(2, 9100, strServerIP, sModel);
                    if (iRtn == 0)
                    {
                        bConnect = true;
                    }
                }
                #endregion

                //int iRtn = WrapperTagPrintX.Connect(2, 9100, strServerIP, sModel);
                //if (iRtn == 0)
                //{
                //    bConnect = true;
                //}

                if (!bConnect)
                {
                    //sckClient.Close();
                    //sckClient.Dispose();

                    sRtn = "1";
                    sMessage = "VAL";
                    Program.AppLog.Write(this, "Print 실패:프린터 연결 실패");
                    AddLogList("발행실패:" + request["PAMS_MGMT_NO"].ToString().Trim() + ", 프린터 연결 실패");
                }
                else
                {
                    ////대통령, 생산년도, 보존기간 중앙정렬 처리
                    //if (request["PRSDT_NM"].ToString().Trim().Length == 3)
                    //{
                    //    request["PRSDT_NM"] = "  " + request["PRSDT_NM"].ToString().Trim() + " ";
                    //}
                    //else if (request["PRSDT_NM"].ToString().Trim().Length == 4)
                    //{
                    //    request["PRSDT_NM"] = " " + request["PRSDT_NM"].ToString().Trim();
                    //}


                    //if (request["PROD_STRT_YY"].ToString().Trim().Length == 4)
                    //{
                    //    request["PROD_STRT_YY"] = "     " + request["PROD_STRT_YY"].ToString().Trim();
                    //}
                    //else
                    //{
                    //    request["PROD_STRT_YY"] = request["PROD_STRT_YY"].ToString().Trim();
                    //}

                    //if (request["PRSRV_PRD"].ToString().Trim().Length == 2)
                    //{
                    //    request["PRSRV_PRD"] = "      " + request["PRSRV_PRD"].ToString().Trim();
                    //}
                    //else if (request["PRSRV_PRD"].ToString().Trim().Length == 3)
                    //{
                    //    request["PRSRV_PRD"] = "    " + request["PRSRV_PRD"].ToString().Trim();
                    //}
                    //else if (request["PRSRV_PRD"].ToString().Trim().Length == 4)
                    //{
                    //    request["PRSRV_PRD"] = "    " + request["PRSRV_PRD"].ToString().Trim();
                    //}
                    //else
                    //{
                    //    request["PRSRV_PRD"] = request["PRSRV_PRD"].ToString().Trim();
                    //}

                    //생산기관 자리수 체크
                    string sPROD_ORG_NM = request["PROD_ORG_NM"].ToString().Trim();

                    if (sPROD_ORG_NM.Length < 15)
                    {
                        if (request["PAMS_MGMT_NO"].ToString().Trim().Length > 10)
                        {
                            WrapperTagPrintX.SetFileName(sPrintFileExt);
                        }
                        else
                        {
                            WrapperTagPrintX.SetFileName(sPrintFile);
                        }

                        

                        sVariable = request["PRSDT_NM"].ToString() + "!!" + request["PROD_STRT_YY"].ToString() + "!!" + request["PRSRV_PRD"].ToString()
                                  + "!!" + sPROD_ORG_NM + "!!" + request["PAMS_MGMT_NO"].ToString().Trim() + "!!" + request["TAG_ID"].ToString().Trim();
                    }
                    else if (sPROD_ORG_NM.Length >= 15 && sPROD_ORG_NM.Length <= 28)
                    {
                        int iLength = sPROD_ORG_NM.Length;

                        //int iRest = iLength % 2;
                        //int iLen = iLength / 2;

                        if (request["PAMS_MGMT_NO"].ToString().Trim().Length > 10)
                        {
                            WrapperTagPrintX.SetFileName(sPrintFileExt2);
                        }
                        else
                        {
                            WrapperTagPrintX.SetFileName(sPrintFile2);
                        }

                        string strPROD_ORG_NM1 = string.Empty;
                        string strPROD_ORG_NM2 = string.Empty;
                        //strPROD_ORG_NM1 = sPROD_ORG_NM.Substring(0, (iLen + iRest)).PadLeft(14);
                        //strPROD_ORG_NM2 = sPROD_ORG_NM.Substring((iLen + iRest), sPROD_ORG_NM.Length - (iLen + iRest)).PadLeft(14);

                        strPROD_ORG_NM1 = sPROD_ORG_NM.Substring(0, 14);
                        strPROD_ORG_NM2 = sPROD_ORG_NM.Substring(14);



                        sVariable = request["PRSDT_NM"].ToString() + "!!" + request["PROD_STRT_YY"].ToString() + "!!" + request["PRSRV_PRD"].ToString()
                                  + "!!" + strPROD_ORG_NM1 + "!!" + request["PAMS_MGMT_NO"].ToString().Trim() + "!!" + request["TAG_ID"].ToString().Trim()
                                             + "!!" + strPROD_ORG_NM2;

                    }
                    else
                    {
                        int iLength = sPROD_ORG_NM.Length;

                        //int iRest = iLength % 2;
                        //int iLen = iLength / 2;

                        if (request["PAMS_MGMT_NO"].ToString().Trim().Length > 10)
                        {
                            WrapperTagPrintX.SetFileName(sPrintFileExt3);
                        }
                        else
                        {
                            WrapperTagPrintX.SetFileName(sPrintFile3);
                        }

                        string strPROD_ORG_NM1 = string.Empty;
                        string strPROD_ORG_NM2 = string.Empty;
                        strPROD_ORG_NM1 = sPROD_ORG_NM.Substring(0, 19);
                        strPROD_ORG_NM2 = sPROD_ORG_NM.Substring(19);



                        sVariable = request["PRSDT_NM"].ToString() + "!!" + request["PROD_STRT_YY"].ToString() + "!!" + request["PRSRV_PRD"].ToString()
                                  + "!!" + strPROD_ORG_NM1 + "!!" + request["PAMS_MGMT_NO"].ToString().Trim() + "!!" + request["TAG_ID"].ToString().Trim()
                                             + "!!" + strPROD_ORG_NM2;

                    }

                    WrapperTagPrintX.SetVariableData(sVariable, "!!");

                    int iResult = WrapperTagPrintX.TagPrint(sbResult, sbErrorMessage);

                    StringBuilder sbResult2 = new StringBuilder(512);
                    WrapperTagPrintX.ReadResponse(sbResult2);

                    if (iResult == 0)
                    {
                        bRtn = true;
                        AddLogList("발행성공:" + sVariable);

                        //string[] sArrRst = sbResult.ToString().Split(',');

                        //if (sArrRst.Length > 0)
                        //{
                        //    if (sArrRst[0] == request["TAG_ID"].ToString())
                        //    {
                        //        bRtn = true;
                        //        AddLogList("발행성공:" + sVariable);

                        //    }

                        //}

                    }

                    if (!bRtn)
                    {
                        sRtn = "1";
                        sMessage = "VAL";
                        string shex = Convert.ToString(iResult, 16);
                        Program.AppLog.Write(this, "Print 실패:" + shex + ", " + sbResult2.ToString() + ", " + sbErrorMessage);
                        AddLogList("발행실패:" + request["PAMS_MGMT_NO"].ToString().Trim()  + ","+ shex + ", " + sbResult2.ToString() + ", " + sbErrorMessage);
                    }

                }




            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "예상 되지 않은 오류!", e);
                sRtn = "1";
                sMessage = "ERR";
                AddLogList("발행실패:" + request["PAMS_MGMT_NO"].ToString().Trim() + "," + e.ToString());

            }
            finally
            {
                iClosePort = WrapperTagPrintX.ClosePort("");
                if (iClosePort == 0)
                {
                    //lblMsg.Text = "프린터 연결 닫기";
                    //MessageBox.Show("프린터 연결 닫기 성공");
                }
                else
                {
                    Program.SysLog.Write(this, "프린터 연결 닫기 실패!");
                    sRtn = "1";
                    sMessage = "ERR";
                    AddLogList("발행실패:" + request["PAMS_MGMT_NO"].ToString().Trim() + "프린터 연결 닫기 실패");
                }

                //WrapperTagPrintX.ClosePort("");
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

            DBManagement.ExecuteProcedure("PAMSDATA.TRFID_ISSUE_REQ_UPDATE", htParamsHashtable, DB_Timeout);
            string strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
            string strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

            return bRtn;
        }

        delegate void SafetyAddLogList(string logMsg);

        int iAPP_MAXListCount = 200;

        public void AddLogList(string logMsg)
        {
            try
            {
                if (logListBox.InvokeRequired)
                {
                    //logListBox.Invoke(new SafetyAddLogList(AddLogList), logMsg);
                    logListBox.BeginInvoke(new SafetyAddLogList(AddLogList), logMsg);
                }
                else
                {
                    //lock을 사용하는 경우 이벤트 타이밍으로 인해 화면 멈춤 발생 가능함
                    int nTopIndex = logListBox.TopIndex;
                    logListBox.Items.Insert(0, string.Format("{0:yyyy-MM-dd HH:mm:ss}] {1}", DateTime.Now, logMsg));
                    while (logListBox.Items.Count >= iAPP_MAXListCount)
                    {
                        //logListBox.Items.RemoveAt(0);
                        logListBox.Items.RemoveAt(logListBox.Items.Count - 1);
                    }
                    logListBox.TopIndex = nTopIndex;
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "logListBox 로그 추가 오류!");
                Program.SysLog.Write(this, "logListBox 로그 추가 오류!", e);
            }
        }
    }
}
