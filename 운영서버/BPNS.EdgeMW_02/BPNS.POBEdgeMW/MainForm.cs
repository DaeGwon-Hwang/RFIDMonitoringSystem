using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using BPNS.COM;
using System.Threading;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace BPNS.EdgeMW
{
    //public enum StringProtocolState
    //{
    //    Cursor = 0,
    //    Error = 1,
    //    Fail = 2,
    //    Login = 3,
    //    Timeout = 4,
    //}

    public partial class MainForm : Form, IBaseForm
    {
        #region ---- 전역 변수 선언

        /// <summary>
        /// 사용자 컨트롤의 배열 리더기의 갯수 만큼 생성하여 관리 한다.
        /// </summary>
        UCRFIDReader[] ucRFIDReaderArr;
        //Dictionary<string, object> ucRFIDReaderArr;

        /// <summary>
        /// 리더기 갯수
        /// </summary>
        private int _iReaderCnt;

        /// <summary>
        /// Tag 데이터 정보 목록 - 중앙관리에서 각 UControl 관리로 변경
        /// </summary>
        //public DataTable TagTable;

        /// <summary>
        /// Grid  Data 쓰기를 위한 대리자 선언
        /// </summary>
        delegate void LoadGrid(DataTable dt);
        LoadGrid Logdelegate;

        /// <summary>
        ///  로그 조회를 위한 스레드
        /// </summary>
        private Thread _thLog;              //로그 처리 쓰레드
        private Thread _th;                 // ESP 리더 쓰레드
        private Thread _thTransfer;         // TAG 데이터 전송 쓰레드
        //private Thread DataProcThread;  // 전송 데이터 처리 쓰레드

        /// <summary>
        /// 스레드 정지 및 실행 이벤트
        /// </summary>
        ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        ManualResetEvent _pauseEvent = new ManualResetEvent(true);

        /// <summary>
        /// 스레드 생성여부
        /// </summary>
        bool CreateThread = false;

        /// <summary>
        /// UPS check Thread loop
        /// </summary>
        private volatile bool bStopDataProcThreadLoop = false;

        #endregion 


        private Socket listener;
        //private int iFROM_MW_PORT = 11812;
        private int iFROM_MW_PORT = 12012;
        private int iNET_ListeningSocketCount = 100;

        private volatile IDatabaseManagement DBManagement;

        internal IDatabaseManagement dbManagement
        {
            get { return DBManagement; }
            set { DBManagement = value; }
        }

        public List<string> lstData = new List<string>();

        public List<JObject> lstJOData = new List<JObject>();

        int iDB_Timeout = 3;
        public MainForm()
        {
            InitializeComponent();

        }



        #region ---- 이벤트 처리 부분

        /// <summary>
        /// 각 쓰레드를 중지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, EventArgs e)
        {
            //리더기 연결 종료
            InitStop();

            ///쓰레드 재시작
            _pauseEvent.Reset();


        }

        /// <summary>
        /// 각 쓰레드를 시작해야 함.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            InitStart();
            Application.DoEvents();
            ///쓰레드 종료
            _pauseEvent.Set();
        }

        /// <summary>
        /// 페이지 종료 시 모든 컨트롤의 연결을 종료 한다. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (ucRFIDReaderArr != null && ucRFIDReaderArr.Length > 0)
            {
                for (int i = 0; i < _iReaderCnt; i++)
                {
                    try
                    {
                        ucRFIDReaderArr[i].Close();
                    }
                    catch (NullReferenceException ex)
                    {

                    }

                }
            }

            this.Dispose();
        }

        /// <summary>
        /// 페이지 로딩 이벤트 - 기본 정보를 가져온다. 
        /// 로딩 이벤트에서 처리하면화면 자체가 늦게 뜨게 됨으로 shown으로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {

            btnStart.Enabled = false;


            DBManagement = new DatabaseManagementOracle();

            if (!DBManagement.Connect())
            {
                Program.AppLog.Write(this, "DB Connection Error!");
                //MessageBox.Show("DB 연결 오류가 발생하였습니다. 시스템 로그를 확인하여 DB연결설정을 변경하시기 바랍니다!", "Database 연결오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AddLogList("DB 연결 오류가 발생하였습니다. 시스템 로그를 확인하여 DB연결설정을 변경하시기 바랍니다!");
            }

            DataSet dsTest = DBManagement.ExecuteSelectQuery("SELECT TO_CHAR(SYSTIMESTAMP, 'YYYYMMDDHH24MISSFF3') FROM DUAL", 1);
        }

        /// <summary>
        /// 로딩시 유저 컨트롤을 그리던 것을 form loading 이후로 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Shown(object sender, EventArgs e)
        {
            // 페이지 로딩 후 리더기 목록을 가져와서 userControl로 생성한다. 
            // 해당 컨트롤의 상태를 정리한다. 상태는 healthcheck를 진행한다. 
            // esp 리더기의 경우 ping 테스트를 진행한다. 
            // alien 리더기의 경우 로그인 시도를 한다.

            //컨트롤 생성 및 기본 정보 세팅
            initUserControl();

            btnStart.Enabled = true;
        }
        #endregion

        #region ---- 메소드 처리 부분

        /// <summary>
        /// UserControl Rendering 시작
        /// </summary>
        private void InitStart()
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                SetStartOrResume();
                btnStart.Enabled = false;
                btnStop.Enabled = true;

            }
            catch (Exception ex)
            {
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
            finally
            {
                if (Cursor.Current != Cursors.Default) Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// 수집 종료
        /// </summary>
        private void InitStop()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                SetPause();
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
            catch (Exception ex)
            {

                btnStart.Enabled = false;
                btnStop.Enabled = true;
                throw ex;
            }
            finally
            {
                if (Cursor.Current != Cursors.Default) Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// 쓰레드 시작 또는 재시작 처리
        /// </summary>
        private void SetStartOrResume()
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                if (!CreateThread)
                {
                    //유저 컨트롤 쓰레드 시작
                    for (int i = 0; i < _iReaderCnt; i++)
                    {
                        ucRFIDReaderArr[i].SetDefaultvalue();
                        ucRFIDReaderArr[i].SetFormLoadingLoop(true);
                        ucRFIDReaderArr[i].ThreadStart();

                        ucRFIDReaderArr[i].SetLabelStartDT("시작 : " + DateTime.Now.ToString());
                        Program.AppLog.Write(this, ucRFIDReaderArr[i]._EQ_NM + " : Thread Start");

                        #region 이전 코드
                        //if (ucRFIDReaderArr[i]._ReaderType == RFIDType.ITP_U9R)
                        //{
                        //    ucRFIDReaderArr[i].SetDefaultvalue();
                        //    ucRFIDReaderArr[i].SetFormLoadingLoop(true);
                        //    ucRFIDReaderArr[i].ThreadStart();


                        //}
                        //else if (ucRFIDReaderArr[i]._ReaderType == RFIDType.Alien)
                        //{
                        //    ucRFIDReaderArr[i].SetDefaultvalue();
                        //    ucRFIDReaderArr[i].ThreadStart();
                        //    ucRFIDReaderArr[i].SetFormLoadingLoop(true);
                        //}
                        //else if (ucRFIDReaderArr[i]._ReaderType == RFIDType.IDRO)
                        //{
                        //    ucRFIDReaderArr[i].SetDefaultvalue();
                        //    ucRFIDReaderArr[i].ThreadStart();
                        //    ucRFIDReaderArr[i].SetFormLoadingLoop(true);
                        //}


                        //ucRFIDReaderArr[i].SetLabelStartDT("시작 : " + DateTime.Now.ToString());
                        //Program.AppLog.Write(this, ucRFIDReaderArr[i]._EQ_NM + " : Thread Start"); 
                        #endregion
                    }

                    CreateThread = true;
                }
                else
                {
                    //유저 컨트롤 재시작
                    foreach (UCRFIDReader uc in ucRFIDReaderArr)
                    {
                        uc.Resume();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.SysLog.Write(this, "SetStartOrResume Exception 오류", ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }

        private void initUserControl()
        {


            //데이터 전송 스레드 종료

            //유저 컨트롤이 null 또는 없을 경우 유저 컨트롤을 설정한다.
            if (ucRFIDReaderArr == null || ucRFIDReaderArr.Length == 0)
            {

                ////로그 스레드 시작
                //_thLog = new Thread(new System.Threading.ThreadStart(GetLogData));
                //_thLog.IsBackground = true;
                //_thLog.Start();
                ////로그 스레드 시작 부분

                //데이터 전송 스레드 시작
                _thTransfer = new Thread(new System.Threading.ThreadStart(DataTransfer));
                _thTransfer.IsBackground = true;
                _thTransfer.Start();



                //함수내 사용 변수 선언 및 초기화
                int j = 0;
                int iCnt = 0;
                int _top = 0;
                int _left = 0;

                //Batch Job 수량 전역변수에 저장
                //_iReaderCnt = _dtBatchJob.Rows.Count;

                //DataTable dt = ReaderList().Copy();

                /*
                DataTable dt = new DataTable("ReaderList");
                
                dt.Columns.Add("READER_NM", typeof(string));
                dt.Columns.Add("READER_IP", typeof(string));
                dt.Columns.Add("READER_TYPE", typeof(string));
                dt.Columns.Add("READER_ID", typeof(string));
                dt.Columns.Add("GATE_ID", typeof(string));
                dt.Columns.Add("IO_FLAG", typeof(string));

                dt.Rows.Add("READER_NM", "192.168.200.241", "ITP_U9R", "READER_ID1", "GATE_ID1", "I");
                //dt.Rows.Add("READER_NM", "192.168.200.242", "ITP_U9R", "READER_ID2", "GATE_ID2", "I");
                //dt.Rows.Add("READER_NM", "192.168.200.243", "ITP_U9R", "READER_ID3", "GATE_ID3", "I");
                */

                DataTable dt = ReaderList();
                _iReaderCnt = dt.Rows.Count;

                //유저 컨트롤 배열 생성
                ucRFIDReaderArr = new UCRFIDReader[_iReaderCnt];
                iCnt = 0;
                _top = 5;
                _left = 0;

                for (int i = 0; i < _iReaderCnt; i++)
                {

                    UCRFIDReader uws;

                    if (dt.Rows[i]["READER_TYPE"].ToString() == "ITP_U9R")
                    {
                        uws = new UCRFIDReader(RFIDType.ITP_U9R);   //ESP 리더기용
                    }
                    else if (dt.Rows[i]["READER_TYPE"].ToString() == "Alien")
                    {
                        uws = new UCRFIDReader(RFIDType.Alien); //Alien  리더기용
                    }
                    else if (dt.Rows[i]["READER_TYPE"].ToString() == "IDRO")
                    {
                        uws = new UCRFIDReader(RFIDType.IDRO); //생성후 변경 필요
                    }
                    else
                    {
                        uws = new UCRFIDReader(RFIDType.ITP_U9R);
                    }

                    uws._parentForm = this;

                    uws.Parent = this.pnlArea;
                    uws._EQ_ID = dt.Rows[i]["EQ_ID"].ToString();
                    uws._EQ_NM = dt.Rows[i]["EQ_NM"].ToString();
                    uws._EQ_IP = dt.Rows[i]["IP_ADDR"].ToString();

                    uws._GATE_ID = dt.Rows[i]["GATE_ID"].ToString();
                    uws._GATE_NM = dt.Rows[i]["GATE_NM"].ToString();
                    uws._GATE_TYPE = dt.Rows[i]["GATE_TYPE"].ToString();

                    uws._ANT_CNT = dt.Rows[i]["ANT_CNT"].ToString();
                    uws._ANT_SEQ = dt.Rows[i]["ANT_SEQ"].ToString();

                    uws._USE_SENSOR = dt.Rows[i]["USE_SENSOR"].ToString();//USE_SENSOR
                    //uws._USE_SENSOR = dt.Rows[i]["ORI_USE_SENSOR"].ToString();//ORI_USE_SENSOR
                    uws._NOTFY_TYPE = dt.Rows[i]["NOTFY_TYPE"].ToString();

                    uws._IN_ZN_ID = dt.Rows[i]["IN_ZN_ID"].ToString();
                    uws._OUT_ZN_ID = dt.Rows[i]["OUT_ZN_ID"].ToString();
                    uws._IN_ZN_NM = dt.Rows[i]["IN_ZN_NM"].ToString();
                    uws._OUT_ZN_NM = dt.Rows[i]["OUT_ZN_NM"].ToString();
                    uws._SG_YN = dt.Rows[i]["SG_YN"].ToString();

                    //메인폼 하나에서 처리 중이었으나 각 UserControl내로 변경 2017-07-04
                    //uws._TagTable = TagTable;

                    uws.SetDefaultvalue(); //현재 부분에서 정보를 넣고 연결을 체크 하는 정도만 처리 필요 리더기 UserControl class에서 처리 필요

                    //컨트롤 배열에 생성된 컨트롤 추가 
                    ucRFIDReaderArr[iCnt] = uws;

                    j = j + 1;

                    //유저 컨트롤 위치 정보 및 디자인 설정
                    ucRFIDReaderArr[iCnt].Left = _left;
                    ucRFIDReaderArr[iCnt].Top = _top;

                    _left = _left + (ucRFIDReaderArr[iCnt].Width);

                    if (j == 5)
                    {
                        j = 0;
                        _top = _top + (ucRFIDReaderArr[iCnt].Height);
                        _left = 0;
                    }

                    iCnt++;
                }
            }
        }

        /// <summary>
        /// 유저 컨트롤 정지 - ESP 리더기의 경우는 제외한다. 
        /// </summary>
        private void SetPause()
        {
            try
            {
                if (ucRFIDReaderArr != null && ucRFIDReaderArr.Length > 0)
                {
                    for (int i = 0; i < _iReaderCnt; i++)
                    {
                        ucRFIDReaderArr[i].Pause();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.SysLog.Write(this, "SetPause 실행오류", ex);
            }
        }

        /// <summary>
        /// delegate에서 사용하는 databinding용 메소드
        /// </summary>
        /// <param name="dt"></param>
        public void GridviewBind(DataTable dt)
        {
            //try
            //{
            //    this.gvHistoryList.DataSource = dt;
            //}
            //catch (Exception e)
            //{
            //    SetLogData(e.ToString(), "MainPage", "Log Grid Binding");
            //    throw;
            //}
        }

        /// <summary>
        /// 로그 데이터 가져오기 
        /// </summary>
        private void GetLogData()
        {

            //while (_pauseEvent.WaitOne())
            //{

            //    if (chkLog.Checked)
            //    {
            //        DataTable dt = BIZ.bizMW.GetLogData();

            //        if (this.gvHistoryList.InvokeRequired)
            //        {
            //            gvHistoryList.Invoke(Logdelegate, new object[] { dt });
            //        }
            //        else
            //        {
            //            this.gvHistoryList.DataSource = dt;
            //        }

            //        Thread.Sleep(1000); 
            //    }
            //}

        }

        /// <summary>
        /// 로그 처리 
        /// </summary>
        /// <param name="LogMsg"></param>
        /// <param name="LogSender"></param>
        /// <param name="Log_Event"></param>
        private void SetLogData(string LogMsg, string LogSender, string Log_Event)
        {
            try
            {
                Program.AppLog.Write(LogSender, "logListBox 로그 추가 오류!");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //try
            //{               
            //    BIZ.BizMWwithService _biz = new BIZ.BizMWwithService();

            //    _biz.SetLogData(DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss"), LogMsg, LogSender, Log_Event);
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}

        }

        #endregion


        #region ----- Thread 용 메소드 

        /// <summary>
        /// 리더기의 정보를 조회
        /// </summary>
        /// <returns></returns>
        private DataTable ReaderList()
        {
            Hashtable htParamsHashtable;
            DataTable dtRtn = null;
            DataSet dsResult;

            try
            {
                htParamsHashtable = new Hashtable();
                htParamsHashtable.Clear();

                dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PKG_MW_PROC.READER_INFO2", htParamsHashtable, iDB_Timeout);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                {
                    dtRtn = dsResult.Tables[0];
                }
                else
                {

                }

            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "ReaderList 오류", e);
            }

            return dtRtn;

            //BIZ.BizMWwithService _biz = new BIZ.BizMWwithService();
            //return _biz.GetReaderData();
        }

        /// <summary>
        /// 리더기 정보 전송을 위한 class를 사용하여 쓰레드 처리
        /// </summary>
        private void DataTransfer()
        {
            while (_pauseEvent.WaitOne())
            {
                //ReadDataTransfer RDT = new ReadDataTransfer();
                //RDT.ReadDataSender();

                if (lstData != null)
                {
                    while (lstData.Count > 0)
                    {
                        SetEQStatus(lstData[0]);
                        lstData.RemoveAt(0);
                    }
                }



                Thread.Sleep(1000);
            }
        }



        private void SetEQStatus(string sGateId)
        {
            try
            {
                Hashtable htParamsHashtable = new Hashtable();
                htParamsHashtable.Clear();
                htParamsHashtable.Add("IN_GATE_ID", sGateId);

                if (DBManagement == null)
                {
                    DBManagement = new DatabaseManagementOracle();
                }

                DBManagement.ExecuteProcedure("PKG_MW_PROC.EQ_STATUS_UPDATE", htParamsHashtable, iDB_Timeout);
                string strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                string strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();
            }
            catch (Exception ex)
            {
                Program.SysLog.Write(this, "SetEQStatus(PKG_MW_PROC.EQ_STATUS_UPDATE) 오류", ex);
            }
        }

        public void DataProcWorkThread()
        {

            while (_pauseEvent.WaitOne())
            {
                //데이터 처리
                if (lstJOData != null)
                {
                    while (lstJOData.Count > 0)
                    {

                        switch (lstJOData[0]["WORKDIV"].ToString())
                        {
                            case "004REQ":

                                C004REQ(lstJOData[0]["WK_TYPE"].ToString(), lstJOData[0]["GATE_ID"].ToString());

                                break;
                        }

                        lstJOData.RemoveAt(0);
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private void C004REQ(string sWK_TYPE, string sGATE_ID)
        {
            //시작: "STT", 중지: "STP", 재시작: "RBT", 경광등제어: "OBZ"
            switch (sWK_TYPE)
            {
                case "STT":
                    SetStart(sGATE_ID);
                    break;

                case "STP":
                    SetStop(sGATE_ID);
                    break;

                case "RBT":
                    SetReStart(sGATE_ID);
                    break;

                case "OBZ":
                    SetOBZ(sGATE_ID);
                    break;

                default:
                    Program.AppLog.Write(this, "게이트 제어 : sGATE_ID : " + sGATE_ID + " sWK_TYPE : " + sWK_TYPE + " 상태값 처리 제외");
                    break;
            }

        }

        #endregion


        delegate void SafetyAddLogList(string logMsg);

        int APP_MAXListCount = 100;

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
                    while (logListBox.Items.Count >= APP_MAXListCount)
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

        private void SetStart(string sGateID)
        {
            if (ucRFIDReaderArr != null && ucRFIDReaderArr.Length > 0)
            {
                for (int i = 0; i < _iReaderCnt; i++)
                {
                    try
                    {
                        if (ucRFIDReaderArr[i]._GATE_ID == sGateID)
                        {
                            ucRFIDReaderArr[i].SProc = "STT";
                            ucRFIDReaderArr[i].Resume();
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
        }

        private void SetReStart(string sGateID)
        {
            if (ucRFIDReaderArr != null && ucRFIDReaderArr.Length > 0)
            {
                for (int i = 0; i < _iReaderCnt; i++)
                {
                    try
                    {
                        if (ucRFIDReaderArr[i]._GATE_ID == sGateID)
                        {
                            ucRFIDReaderArr[i].SProc = "RBT";
                            ucRFIDReaderArr[i].Resume();
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
        }

        private void SetStop(string sGateID)
        {
            if (ucRFIDReaderArr != null && ucRFIDReaderArr.Length > 0)
            {
                for (int i = 0; i < _iReaderCnt; i++)
                {
                    try
                    {
                        if (ucRFIDReaderArr[i]._GATE_ID == sGateID)
                        {
                            ucRFIDReaderArr[i].SProc = "STP";
                            ucRFIDReaderArr[i].Pause();
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
        }

        /// <summary>
        /// 경광등제어
        /// </summary>
        /// <param name="sGateID"></param>
        private void SetOBZ(string sGateID)
        {
            if (ucRFIDReaderArr != null && ucRFIDReaderArr.Length > 0)
            {
                for (int i = 0; i < _iReaderCnt; i++)
                {
                    try
                    {
                        if (ucRFIDReaderArr[i]._GATE_ID == sGateID)
                        {
                            ucRFIDReaderArr[i].SProc = "OBZ";
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
        }


    }
}
