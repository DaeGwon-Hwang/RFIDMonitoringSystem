using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using BPNS.COM;

namespace BPNS.ReaderHelper
{
    public partial class UCRFIDReader : UserControl
    {

        #region ---- 변수선언

        /// <summary>
        /// 리더기 장비 타입
        /// </summary>
        public RFIDType _ReaderType;

        /// <summary>
        /// Tag 데이터 정보 목록
        /// </summary>
        private DataTable _TagTable; 

        /// <summary>
        /// RFID 리더기 객체
        /// </summary>
        public Object ReaderClass;

        public bool readerCheck = false;

        /// <summary>
        /// 리더기에 시간 설정 날리기 위한 변수
        /// </summary>
        public bool bSetDatetime = false;

        private string sProc = string.Empty;

        ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        ManualResetEvent _pauseEvent = new ManualResetEvent(true);

        /// <summary>
        /// 부모 창 선택
        /// </summary>
        public Form _parentForm
        {
            set; get;
        }

        

        /// <summary>
        /// 장비 아이디
        /// </summary>
        public string _EQ_ID
        { get; set; }

        /// <summary>
        /// 장비 명
        /// </summary>
        public string _EQ_NM
        { get; set; }

        /// <summary>
        /// 장비 IP
        /// </summary>
        public string _EQ_IP
        { get; set; }

        /// <summary>
        /// GATE ID
        /// </summary>
        public string _GATE_ID
        { get; set; }
        
        /// <summary>
        /// 게이트명
        /// </summary>
        public string _GATE_NM
        { get; set; }

        public string _GATE_TYPE
        { get; set; }

        public string _USE_SENSOR
        { get; set; }

        public string _NOTFY_TYPE
        { get; set; }

        /// <summary>
        /// 안테나 수
        /// </summary>
        public string _ANT_CNT
        { get; set; }

        /// <summary>
        /// 안테나 순서
        /// </summary>
        public string _ANT_SEQ
        { get; set; }

        /// <summary>
        /// IN ZONE ID
        /// </summary>
        public string _IN_ZN_ID
        { get; set; }

        /// <summary>
        /// OUT ZONE ID
        /// </summary>
        public string _OUT_ZN_ID
        { get; set; }

        /// <summary>
        /// IN ZONE NM
        /// </summary>
        public string _IN_ZN_NM
        { get; set; }

        /// <summary>
        /// OUT ZONE NM
        /// </summary>
        public string _OUT_ZN_NM
        { get; set; }

        /// <summary>
        /// 서고 여부
        /// </summary>
        public string _SG_YN
        { get; set; }

        /// <summary>
        /// MW TagList
        /// </summary>
        public string _TagList
        { get; set; }

        public string SProc
        {
            get => sProc;
            set => sProc = value;
        }


        /// <summary>
        /// Thread Stop 여부 
        /// </summary>
        public bool _bStopThread;

        private Thread _th;

        private Thread _thHealth; //사용안함

        /// <summary>
        /// 최초 Connect 체크 여부(최초에 한번은 로그를 남기기 위해 사용)
        /// </summary>
        private bool bFirstConectCheck = true;

        /// <summary>
        /// 비정상 신호 시작 시간
        /// </summary>
        DateTime _dteAbnormalSignal;
        bool _bAbnormalSignal = false;

        /// <summary>
        /// 센서 인식후 태그 없을 경우 알람 울릴 시간
        /// </summary>
        public int _iSensorAtferNoTagAlram = 10;

        TagReport trReport;

        DateTime _dteLastSensor;

        /// <summary>
        /// 센서 인식후 태그 인식 체크 시간
        /// </summary>
        public int _iSensorAtferNoTag = 8;

        #endregion 

        /// <summary>
        /// Usercontrol class 선언부
        /// </summary>
        /// <param name="ReaderType"></param>
        public UCRFIDReader(RFIDType ReaderType)
        {
            _ReaderType = ReaderType;
            InitializeComponent();

        }       

        #region ----- 크로스 스레드 처리
        delegate void SetTextCallback(string p_strt);

        /// <summary>
        /// 리더기 명
        /// </summary>
        /// <param name="p_sMsg"></param>
        public void SetLabelTitle(string p_sMsg)
        {
            if (this.lblTitle.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetLabelTitle);
                this.Invoke(d, new object[] { p_sMsg });
            }
            else
            {
                this.lblTitle.Text = p_sMsg;
                this.lblTitle.AutoSize = true;
                
            }
        }

        delegate void SetLoopCallback(bool check);

        /// <summary>
        /// loop control
        /// </summary>
        /// <param name="check"></param>
        public void SetFormLoadingLoop(bool check)
        {
            if(this.iFormLoadingLoop1.InvokeRequired)
            {
                SetLoopCallback d = new SetLoopCallback(SetFormLoadingLoop);
                this.Invoke(d, new object[] { check });
           
            }
            else
            {
                if(check)
                {
                    this.iFormLoadingLoop1.Run();
                }
                else
                {
                    this.iFormLoadingLoop1.Stop();
                }
                
            }
        }

        /// <summary>
        /// 연결 아이피
        /// </summary>
        /// <param name="p_sMsg"></param>
        public void SetLabelConnectIP(string p_sMsg)
        {
            if (this.lbConnectIP.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetLabelConnectIP);
                this.Invoke(d, new object[] { p_sMsg });
            }
            else
            {
                this.lbConnectIP.Text = p_sMsg;
                this.lbConnectIP.AutoSize = true;
            }
        }

        /// <summary>
        /// 시작시간
        /// </summary>
        /// <param name="p_sMsg"></param>
        public void SetLabelStartDT(string p_sMsg)
        {
            if (this.lbStartDT.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetLabelStartDT);
                this.Invoke(d, new object[] { p_sMsg });
            }
            else
            {
                this.lbStartDT.Text = p_sMsg;
                this.lbStartDT.AutoSize = true;
            }
        }

        /// <summary>
        /// 상태 표시 
        /// </summary>
        /// <param name="p_sMsg"></param>
        public void SetLabelStatus(string p_sMsg)
        {
            if (this.lbStatus.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetLabelStatus);
                this.Invoke(d, new object[] { p_sMsg });
            }
            else
            {
                this.lbStatus.Text = p_sMsg;
                this.lbStatus.AutoSize = true;
            }
        }
        #endregion

        /// <summary>
        /// 리더기에 기본 생성 값을 지정한다. 
        /// </summary>
        public void SetDefaultvalue()
        {
            _TagTable = BPNS.COM.CreateTableHelper.CreateReaderTable();
            ReaderTypeSet(_ReaderType);

        }

        public void StartThread()
        {
            _th.Start();
        }

        /// <summary>
        /// Reader Type 설정 Reader 타입에 따라 리더기별 class를 사용한다.
        /// </summary>
        /// <param name="ReaderType"></param>
        private void ReaderTypeSet(RFIDType ReaderType)
        {
            _ReaderType = ReaderType;

            if (_ReaderType == RFIDType.ITP_U9R)
            {
                try
                {
                    ReaderClass = new ESPReaderClass2();

                    ESPReaderClass2 reader = (ESPReaderClass2)ReaderClass;

                    reader._TagTable = this._TagTable;

                    reader._EQ_ID = _EQ_ID;
                    reader._EQ_NM = _EQ_NM;
                    reader.IP = _EQ_IP;
                    
                    reader._GATE_ID = _GATE_ID;
                    reader._GATE_NM = _GATE_NM;
                    reader._GATE_TYPE = _GATE_TYPE;

                    reader._ANT_CNT = _ANT_CNT;
                    reader._ANT_SEQ = _ANT_SEQ;

                    reader._USE_SENSOR = _USE_SENSOR;
                    reader._NOTFY_TYPE = _NOTFY_TYPE;

                    reader._IN_ZN_ID = _IN_ZN_ID;
                    reader._OUT_ZN_ID = _OUT_ZN_ID;
                    reader._IN_ZN_NM = _IN_ZN_NM;
                    reader._OUT_ZN_NM = _OUT_ZN_NM;
                    reader._SG_YN = _SG_YN;


                    this.lbConnectIP.Text = _EQ_IP;
                    this.lblTitle.Text = _EQ_NM;

                    if (!reader.EthernetConnection())
                    {
                        SetLabelStatus("Not Found");
                        readerCheck = false;
                    }
                    else
                    {
                        SetLabelStatus("Connect");
                        readerCheck = true;
                    }
                }
                catch (Exception ex)
                {
                    return;
                }
              
            }
            else if (_ReaderType == RFIDType.Alien)
            {
                try
                {
                    ReaderClass = new AlienReaderClass();

                    AlienReaderClass reader = (AlienReaderClass)ReaderClass;

                    reader._TagTable = this._TagTable;

                    reader._EQ_ID = _EQ_ID;
                    reader._EQ_NM = _EQ_NM;
                    reader.IP = _EQ_IP;

                    reader._GATE_ID = _GATE_ID;
                    reader._GATE_NM = _GATE_NM;
                    reader._GATE_TYPE = _GATE_TYPE;

                    reader._ANT_CNT = _ANT_CNT;
                    reader._ANT_SEQ = _ANT_SEQ;

                    reader._USE_SENSOR = _USE_SENSOR;
                    reader._NOTFY_TYPE = _NOTFY_TYPE;

                    reader._IN_ZN_ID = _IN_ZN_ID;
                    reader._OUT_ZN_ID = _OUT_ZN_ID;
                    reader._IN_ZN_NM = _IN_ZN_NM;
                    reader._OUT_ZN_NM = _OUT_ZN_NM;
                    reader._SG_YN = _SG_YN;


                    this.lbConnectIP.Text = _EQ_IP;
                    this.lblTitle.Text = _EQ_NM;


                    if (!reader.EthernetConnection())
                    {
                        this.SetLabelStatus("Not Found");
                        readerCheck = false;
                    }
                    else
                    {
                        this.SetLabelStatus("Connect");
                        readerCheck = true;
                    }
                }
                catch (Exception ex)
                {
                    return;
                }
              

            }
            else if (_ReaderType == RFIDType.IDRO)
            {
                ReaderClass = new IdroReaderClass();
                IdroReaderClass reader = (IdroReaderClass)ReaderClass;

                reader._TagTable = this._TagTable;

                reader._EQ_ID = _EQ_ID;
                reader._EQ_NM = _EQ_NM;
                reader.IP = _EQ_IP;

                reader._GATE_ID = _GATE_ID;
                reader._GATE_NM = _GATE_NM;
                reader._GATE_TYPE = _GATE_TYPE;

                reader._ANT_CNT = _ANT_CNT;
                reader._ANT_SEQ = _ANT_SEQ;

                reader._USE_SENSOR = _USE_SENSOR;
                reader._NOTFY_TYPE = _NOTFY_TYPE;

                reader._IN_ZN_ID = _IN_ZN_ID;
                reader._OUT_ZN_ID = _OUT_ZN_ID;
                reader._IN_ZN_NM = _IN_ZN_NM;
                reader._OUT_ZN_NM = _OUT_ZN_NM;
                reader._SG_YN = _SG_YN;

                this.lbConnectIP.Text = _EQ_IP;
                this.lblTitle.Text = _EQ_NM;

                this.SetLabelStatus("Not Found");

                if (!reader.Connection())
                {
                    this.SetLabelStatus("Not Found");
                    readerCheck = false;
                }
                else
                {
                    this.SetLabelStatus("Connect");
                    readerCheck = true;
                }
            }
            else
            {

            }

        }

        /// <summary>
        /// 스레드 시작 (메인에서 호출)
        /// </summary>
        public void ThreadStart()
        {
            
            _th = new Thread(new System.Threading.ThreadStart(ReaderThread_Start));
            _th.Name = "DataGet";
            _th.IsBackground = true;

            _th.Start();

            //_thHealth = new Thread(new System.Threading.ThreadStart(ReaderHealth_Start));
            //_thHealth.Name = "HealthCheck";
            //_thHealth.IsBackground = true;

            //_thHealth.Start();
           
            //LoopConnect(true);
            
        }        

      
        /// <summary>
        /// 리더 데이터 가져오기 + connection check 
        /// </summary>
        public void ReaderThread_Start()
        {
            while (_pauseEvent.WaitOne())
            {
                _pauseEvent.WaitOne(Timeout.Infinite);

                if (_shutdownEvent.WaitOne(0))
                    break;

                if (_ReaderType == RFIDType.ITP_U9R)
                {
                    ESPReaderClass2 reader = (ESPReaderClass2)ReaderClass;

                    try
                    {
                        if (reader.ConnectionCheck())
                        {
                            if (sProc == "OBZ")
                            {
                                reader.SetDefaultSignal();
                                sProc = "";
                            }

                            //SetLabelStatus("Connect");
                            

                            string sSensorInfo = reader.bSensor == true ? "센서 On" : "센서 Off";
                            this.SetLabelStatus("Connect" + ":" + sSensorInfo);

                            SetLabelStartDT(DateTime.Now.ToString());
                            SetFormLoadingLoop(true);
                            reader.ReaderTagDataGet();
                            
                        }
                        else
                        {
                            SetLabelStatus("NonConnect");
                            _bStopThread = true;
                            SetFormLoadingLoop(false);
                        }

                    }
                    catch (Exception ex)
                    {
                        SetLabelStatus("NonConnect");
                        _bStopThread = true;
                        SetFormLoadingLoop(false);
                    }
                }
                else if (_ReaderType == RFIDType.Alien)
                {
                    AlienReaderClass reader = (AlienReaderClass)ReaderClass;

                    try
                    {
                        //reader.ReaderTagDataGet();

                        if (!reader.EthernetConnection())
                        {
                            if (readerCheck == true || bFirstConectCheck == true)
                            {
                                this.SetLabelStatus("Not Found");
                                readerCheck = false;

                                //AddDeviceError(_MWID, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmssfff"), "N", string.Format("{0:D4}", (int)DevErrorCode.Disconnected), "연결끊김");
                            }
                            if (bFirstConectCheck == true) bFirstConectCheck = false;
                        }
                        else
                        {
                            if (readerCheck == false || bFirstConectCheck == true)
                            {
                                string sSensorInfo = reader.sInout;//reader.sInout == "1" ? "센서 On" : "센서 Off";
                                this.SetLabelStatus("Connect" + ":" + sSensorInfo);
                                readerCheck = true;
                                //AddDeviceError(_MWID, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmssfff"), "Y", string.Format("{0:D4}", (int)DevErrorCode.Connected), "연결됨");
                            }
                            if (bFirstConectCheck == true) bFirstConectCheck = false;

                            if (!bSetDatetime)
                            {
                                reader.ReaderTagDataGet();
                                string sSensorInfo = reader.sInout;//reader.sInout == "1" ? "센서 On" : "센서 Off";
                                this.SetLabelStatus("Connect" + ":" + sSensorInfo);
                            }
                            else
                            {
                                reader.ReaderTagDataGet(true);
                                bSetDatetime = false;
                                string sSensorInfo = reader.sInout;//reader.sInout == "1" ? "센서 On" : "센서 Off";
                                this.SetLabelStatus("Connect" + ":" + sSensorInfo);
                            }

                        }
                    }
                    catch (TimeoutException exTO)
                    {
                        SetLabelStatus("NonConnect");
                        _bStopThread = true;
                        SetFormLoadingLoop(false);
                    }
                    catch (Exception ex)
                    {
                        SetLabelStatus("NonConnect");
                        _bStopThread = true;
                        SetFormLoadingLoop(false);
                    }
                }
                else if (_ReaderType == RFIDType.IDRO)
                {
                    IdroReaderClass reader = (IdroReaderClass)ReaderClass;

                    try
                    {
                        #region 리더기의 마지막 받은 이벤트 처리 시간(리더기 이벤트가 비정상적으로 죽었을 경우 체크해야함) : 전원 Off
                        DateTime tLastEvent = reader._dteLastEvent;
                        DateTime tNow = DateTime.Now;
                        TimeSpan TSChk = tNow - tLastEvent;
                        double diffLastEventSec = TSChk.TotalSeconds;
                        if (diffLastEventSec > 35)
                        {
                            reader.BConnect = false;
                            reader._dteLastEvent = tNow;
                            /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("Health Check(리더기 접속 확인)");
                        }
                        #endregion


                        if (!reader.BConnect)
                        {
                            reader.Connection();
                            if (readerCheck == true || bFirstConectCheck == true)
                            {
                                this.SetLabelStatus("Not Found");
                                readerCheck = false;
                                SetFormLoadingLoop(readerCheck);

                                //AddDeviceError(_MWID, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmssfff"), "N", string.Format("{0:D4}", (int)DevErrorCode.Disconnected), "연결끊김");
                                /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("리더기 연결끊김");
                            }

                            if (bFirstConectCheck == true) bFirstConectCheck = false;
                        }
                        else
                        {
                            if (readerCheck == false || bFirstConectCheck == true)
                            {
                                this.SetLabelStatus("Connect");
                                readerCheck = true;
                                SetFormLoadingLoop(readerCheck);
                                //AddDeviceError(_MWID, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmssfff"), "Y", string.Format("{0:D4}", (int)DevErrorCode.Connected), "연결됨");
                                /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("리더기 연결");
                            }

                            if (bFirstConectCheck == true) bFirstConectCheck = false;

                            if (!bSetDatetime)
                            {
                                reader.ReaderTagDataGet();
                                
                                string sSensorInfo = reader._sActiveMsg;//reader.bSensor == true ? "센서 On" : "센서 Off";
                                this.SetLabelStatus("Connect" + ":" + sSensorInfo);
                            }
                            else
                            {
                                reader.ReaderTagDataGet();
                                bSetDatetime = false;
                                string sSensorInfo = reader._sActiveMsg;//reader.bSensor == true ? "센서 On" : "센서 Off";
                                this.SetLabelStatus("Connect" + ":" + sSensorInfo);
                            }

                            //trReport = reader.ReaderTagDataGet();

                            

                        }



                        //string sTime = Convert.ToString(_TagTable.Compute("max(time)", string.Empty));
                        //DataRow[] dr = _TagTable.Select("[Time] > '" + DateTime.Now.AddSeconds(-10).ToString("yyyy-MM-dd HH:mm:ss.fff") + "' and " + "time ='" + sTime + "' and UPD_YN <>'Y'");

                    }
                    catch (TimeoutException te)
                    {
                        //SetLog(GetType().ToString(), "Reader", "리더 데이터 가져오기 쓰레드", _MWID + " : " + te.ToString());
                        /////Program.SysLog.Write(this, "리더 데이터 가져오기 쓰레드 TimeoutException", te);
                    }
                    catch (Exception ex)
                    {
                        //SetLog(GetType().ToString(), "Reader", "리더 데이터 가져오기 쓰레드", _MWID + " : " + ex.ToString());
                        /////Program.SysLog.Write(this, "리더 데이터 가져오기 쓰레드", ex);
                        /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList(ex.ToString());

                    }
                }


                Thread.Sleep(100);

                //Application.DoEvents();
            }







            //if (_ReaderType == RFIDType.ITP_U9R)
            //{
            //    while (_pauseEvent.WaitOne())
            //    {
            //        _pauseEvent.WaitOne(Timeout.Infinite);

            //        if (_shutdownEvent.WaitOne(0))
            //            break;
            //        ESPReaderClass2 reader = (ESPReaderClass2)ReaderClass;
            //        reader._USE_SENSOR = this._USE_SENSOR;
            //        reader._GATE_TYPE = this._GATE_TYPE;

            //        try
            //        {
            //            if (reader.ConnectionCheck())
            //            {
            //                if (sProc == "OBZ")
            //                {
            //                    reader.SetDefaultSignal();
            //                    sProc = "";
            //                }

            //                SetLabelStatus("Connected");
            //                SetLabelStartDT(DateTime.Now.ToString());
            //                SetFormLoadingLoop(true);
            //                reader.ReaderTagDataGet();
            //            }
            //            else
            //            {
            //                SetLabelStatus("NonConnect");
            //                _bStopThread = true;
            //                SetFormLoadingLoop(false);
            //            }

            //        }
            //        catch (Exception ex)
            //        {
            //            throw;
            //        }

            //        Thread.Sleep(100);

            //        //Application.DoEvents();
            //    }
            //}
            //else if (_ReaderType == RFIDType.Alien)
            //{
            //    while (_pauseEvent.WaitOne())
            //    {
            //        _pauseEvent.WaitOne(Timeout.Infinite);

            //        if (_shutdownEvent.WaitOne(0))
            //            break;
            //        AlienReaderClass reader = (AlienReaderClass)ReaderClass;

            //        try
            //        {
            //            reader.ReaderTagDataGet();
            //        }
            //        catch (TimeoutException ex1)
            //        {
            //            throw ex1;
            //        }
            //        catch (Exception ex)
            //        {
            //            throw ex;
            //        }

                   

            //        Thread.Sleep(100);

            //        //Application.DoEvents();
            //    }
            //}
            //else if (_ReaderType == RFIDType.IDRO)
            //{
            //    while (_pauseEvent.WaitOne())
            //    {
            //        _pauseEvent.WaitOne(Timeout.Infinite);

            //        if (_shutdownEvent.WaitOne(0))
            //            break;
            //        IdroReaderClass reader = (IdroReaderClass)ReaderClass;

            //        try
            //        {
            //            #region 리더기의 마지막 받은 이벤트 처리 시간(리더기 이벤트가 비정상적으로 죽었을 경우 체크해야함) : 전원 Off
            //            DateTime tLastEvent = reader._dteLastEvent;
            //            DateTime tNow = DateTime.Now;
            //            TimeSpan TSChk = tNow - tLastEvent;
            //            double diffLastEventSec = TSChk.TotalSeconds;
            //            if (diffLastEventSec > 35)
            //            {
            //                reader.BConnect = false;
            //                reader._dteLastEvent = tNow;
            //                /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("Health Check(리더기 접속 확인)");
            //            }
            //            #endregion


            //            if (!reader.BConnect)
            //            {
            //                reader.Connection();
            //                if (readerCheck == true || bFirstConectCheck == true)
            //                {
            //                    this.SetLabelStatus("Not Found");
            //                    readerCheck = false;
            //                    SetFormLoadingLoop(readerCheck);

            //                    //AddDeviceError(_MWID, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmssfff"), "N", string.Format("{0:D4}", (int)DevErrorCode.Disconnected), "연결끊김");
            //                    /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("리더기 연결끊김");
            //                }

            //                if (bFirstConectCheck == true) bFirstConectCheck = false;
            //            }
            //            else
            //            {
            //                if (readerCheck == false || bFirstConectCheck == true)
            //                {
            //                    this.SetLabelStatus("Connect");
            //                    readerCheck = true;
            //                    SetFormLoadingLoop(readerCheck);
            //                    //AddDeviceError(_MWID, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmssfff"), "Y", string.Format("{0:D4}", (int)DevErrorCode.Connected), "연결됨");
            //                    /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("리더기 연결");
            //                }

            //                if (bFirstConectCheck == true) bFirstConectCheck = false;

            //                #region 비정상 신호가 울린 경우 10초이상 지나면 신호 끄기 
            //                //비정상 신호가 울린 경우 10초이상 지나면 신호 끄기 
            //                DateTime t11 = _dteAbnormalSignal;
            //                DateTime t21 = DateTime.Now;
            //                TimeSpan TS1 = t21 - t11;

            //                double dDiffAbnormalSignalAtfer = TS1.TotalSeconds;
            //                if (_bAbnormalSignal)
            //                {
            //                    if (dDiffAbnormalSignalAtfer > _iSensorAtferNoTagAlram)
            //                    {
            //                        reader.SetIintSignal();
            //                        _bAbnormalSignal = false;
            //                    }
            //                }
            //                #endregion

            //                trReport = reader.ReaderTagDataGet();

            //                int iChk = _dteLastSensor.CompareTo(reader.DteSensor);

            //                if (iChk > 0)
            //                {
            //                    //최초 센서값 마지막 센서 인식 일시 동기화
            //                    _dteLastSensor = reader.DteSensor;
            //                }
            //                else if (iChk < 0)
            //                {
            //                    if (trReport == null)
            //                    {
            //                        //Tag 정보가 없을 겨우 처리 
            //                        DateTime t1 = reader.DteSensor;
            //                        DateTime t2 = DateTime.Now;
            //                        TimeSpan TS = t2 - t1;

            //                        double diffDay = TS.TotalSeconds;
            //                        if (diffDay > _iSensorAtferNoTag)
            //                        {
            //                            reader.SetAbnormalSignal();
            //                            _dteAbnormalSignal = DateTime.Now;
            //                            _bAbnormalSignal = true;
            //                            _dteLastSensor = reader.DteSensor;
            //                            //reader.SetNoTagReport(); //23.09.08 제외
            //                        }
            //                    }
            //                    //센서 인식후 태그 인식된 경우 정상 처리
            //                    else if (trReport.DteTime.CompareTo(reader.DteSensor.AddSeconds(-2)) >= 0)
            //                    {
            //                        //정상
            //                        _dteLastSensor = reader.DteSensor;
            //                    }
            //                    else
            //                    {
            //                        //태그 인식시간 보다 센서 인식 시간이 뒤일 경우 처리
            //                        DateTime t1 = reader.DteSensor;
            //                        DateTime t2 = DateTime.Now;
            //                        TimeSpan TS = t2 - t1;

            //                        double diffDay = TS.TotalSeconds;
            //                        if (diffDay > _iSensorAtferNoTag)
            //                        {
            //                            reader.SetAbnormalSignal();
            //                            _dteAbnormalSignal = DateTime.Now;
            //                            _bAbnormalSignal = true;
            //                            _dteLastSensor = reader.DteSensor;
            //                            // reader.SetNoTagReport(); //23.09.08 제외
            //                        }
            //                    }

            //                }

            //            }



            //            //string sTime = Convert.ToString(_TagTable.Compute("max(time)", string.Empty));
            //            //DataRow[] dr = _TagTable.Select("[Time] > '" + DateTime.Now.AddSeconds(-10).ToString("yyyy-MM-dd HH:mm:ss.fff") + "' and " + "time ='" + sTime + "' and UPD_YN <>'Y'");

            //        }
            //        catch (TimeoutException te)
            //        {
            //            //SetLog(GetType().ToString(), "Reader", "리더 데이터 가져오기 쓰레드", _MWID + " : " + te.ToString());
            //            /////Program.SysLog.Write(this, "리더 데이터 가져오기 쓰레드 TimeoutException", te);
            //        }
            //        catch (Exception ex)
            //        {
            //            //SetLog(GetType().ToString(), "Reader", "리더 데이터 가져오기 쓰레드", _MWID + " : " + ex.ToString());
            //            /////Program.SysLog.Write(this, "리더 데이터 가져오기 쓰레드", ex);
            //            /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList(ex.ToString());

            //        }




            //        Thread.Sleep(100);

            //        //Application.DoEvents();
            //    }
            //}
            //else
            //{
            //}

            /*
             if (_ReaderType == RFIDType.ITP_U9R)
            {
                while (_pauseEvent.WaitOne())
                {
                    _pauseEvent.WaitOne(Timeout.Infinite);

                    if (_shutdownEvent.WaitOne(0))
                        break;
                    ESPReaderClass2 reader = (ESPReaderClass2)ReaderClass;

                    try
                    {
                        if (reader.ConnectionCheck())
                        {
                            if (sProc == "OBZ")
                            {
                                reader.SetDefaultSignal();
                                sProc = "";
                            }

                            SetLabelStatus("Connected");
                            SetLabelStartDT(DateTime.Now.ToString());
                            SetFormLoadingLoop(true);
                            reader.ReaderTagDataGet();
                        }
                        else
                        {
                            SetLabelStatus("NonConnect");
                            _bStopThread = true;
                            SetFormLoadingLoop(false);
                        }

                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    Thread.Sleep(100);

                    //Application.DoEvents();
                }
            }
            else if (_ReaderType == RFIDType.Alien)
            {
                while (_pauseEvent.WaitOne())
                {
                    _pauseEvent.WaitOne(Timeout.Infinite);

                    if (_shutdownEvent.WaitOne(0))
                        break;
                    AlienReaderClass reader = (AlienReaderClass)ReaderClass;

                    try
                    {
                        reader.ReaderTagDataGet();
                    }
                    catch (TimeoutException ex1)
                    {
                        throw ex1;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                   

                    Thread.Sleep(100);

                    //Application.DoEvents();
                }
            }
            else if (_ReaderType == RFIDType.IDRO)
            {
                while (_pauseEvent.WaitOne())
                {
                    _pauseEvent.WaitOne(Timeout.Infinite);

                    if (_shutdownEvent.WaitOne(0))
                        break;
                    IdroReaderClass reader = (IdroReaderClass)ReaderClass;

                    try
                    {
                        #region 리더기의 마지막 받은 이벤트 처리 시간(리더기 이벤트가 비정상적으로 죽었을 경우 체크해야함) : 전원 Off
                        DateTime tLastEvent = reader._dteLastEvent;
                        DateTime tNow = DateTime.Now;
                        TimeSpan TSChk = tNow - tLastEvent;
                        double diffLastEventSec = TSChk.TotalSeconds;
                        if (diffLastEventSec > 35)
                        {
                            reader.BConnect = false;
                            reader._dteLastEvent = tNow;
                            /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("Health Check(리더기 접속 확인)");
                        }
                        #endregion


                        if (!reader.BConnect)
                        {
                            reader.Connection();
                            if (readerCheck == true || bFirstConectCheck == true)
                            {
                                this.SetLabelStatus("Not Found");
                                readerCheck = false;
                                SetFormLoadingLoop(readerCheck);

                                //AddDeviceError(_MWID, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmssfff"), "N", string.Format("{0:D4}", (int)DevErrorCode.Disconnected), "연결끊김");
                                /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("리더기 연결끊김");
                            }

                            if (bFirstConectCheck == true) bFirstConectCheck = false;
                        }
                        else
                        {
                            if (readerCheck == false || bFirstConectCheck == true)
                            {
                                this.SetLabelStatus("Connect");
                                readerCheck = true;
                                SetFormLoadingLoop(readerCheck);
                                //AddDeviceError(_MWID, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmssfff"), "Y", string.Format("{0:D4}", (int)DevErrorCode.Connected), "연결됨");
                                /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("리더기 연결");
                            }

                            if (bFirstConectCheck == true) bFirstConectCheck = false;

                            #region 비정상 신호가 울린 경우 10초이상 지나면 신호 끄기 
                            //비정상 신호가 울린 경우 10초이상 지나면 신호 끄기 
                            DateTime t11 = _dteAbnormalSignal;
                            DateTime t21 = DateTime.Now;
                            TimeSpan TS1 = t21 - t11;

                            double dDiffAbnormalSignalAtfer = TS1.TotalSeconds;
                            if (_bAbnormalSignal)
                            {
                                if (dDiffAbnormalSignalAtfer > _iSensorAtferNoTagAlram)
                                {
                                    reader.SetIintSignal();
                                    _bAbnormalSignal = false;
                                }
                            }
                            #endregion

                            trReport = reader.ReaderTagDataGet();

                            int iChk = _dteLastSensor.CompareTo(reader.DteSensor);

                            if (iChk > 0)
                            {
                                //최초 센서값 마지막 센서 인식 일시 동기화
                                _dteLastSensor = reader.DteSensor;
                            }
                            else if (iChk < 0)
                            {
                                if (trReport == null)
                                {
                                    //Tag 정보가 없을 겨우 처리 
                                    DateTime t1 = reader.DteSensor;
                                    DateTime t2 = DateTime.Now;
                                    TimeSpan TS = t2 - t1;

                                    double diffDay = TS.TotalSeconds;
                                    if (diffDay > _iSensorAtferNoTag)
                                    {
                                        reader.SetAbnormalSignal();
                                        _dteAbnormalSignal = DateTime.Now;
                                        _bAbnormalSignal = true;
                                        _dteLastSensor = reader.DteSensor;
                                        //reader.SetNoTagReport(); //23.09.08 제외
                                    }
                                }
                                //센서 인식후 태그 인식된 경우 정상 처리
                                else if (trReport.DteTime.CompareTo(reader.DteSensor.AddSeconds(-2)) >= 0)
                                {
                                    //정상
                                    _dteLastSensor = reader.DteSensor;
                                }
                                else
                                {
                                    //태그 인식시간 보다 센서 인식 시간이 뒤일 경우 처리
                                    DateTime t1 = reader.DteSensor;
                                    DateTime t2 = DateTime.Now;
                                    TimeSpan TS = t2 - t1;

                                    double diffDay = TS.TotalSeconds;
                                    if (diffDay > _iSensorAtferNoTag)
                                    {
                                        reader.SetAbnormalSignal();
                                        _dteAbnormalSignal = DateTime.Now;
                                        _bAbnormalSignal = true;
                                        _dteLastSensor = reader.DteSensor;
                                        // reader.SetNoTagReport(); //23.09.08 제외
                                    }
                                }

                            }

                        }



                        //string sTime = Convert.ToString(_TagTable.Compute("max(time)", string.Empty));
                        //DataRow[] dr = _TagTable.Select("[Time] > '" + DateTime.Now.AddSeconds(-10).ToString("yyyy-MM-dd HH:mm:ss.fff") + "' and " + "time ='" + sTime + "' and UPD_YN <>'Y'");

                    }
                    catch (TimeoutException te)
                    {
                        //SetLog(GetType().ToString(), "Reader", "리더 데이터 가져오기 쓰레드", _MWID + " : " + te.ToString());
                        /////Program.SysLog.Write(this, "리더 데이터 가져오기 쓰레드 TimeoutException", te);
                    }
                    catch (Exception ex)
                    {
                        //SetLog(GetType().ToString(), "Reader", "리더 데이터 가져오기 쓰레드", _MWID + " : " + ex.ToString());
                        /////Program.SysLog.Write(this, "리더 데이터 가져오기 쓰레드", ex);
                        /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList(ex.ToString());

                    }




                    Thread.Sleep(100);

                    //Application.DoEvents();
                }
            }
            else
            {
            }
            */
        }



        ///// <summary>
        ///// health check 사용 안함... 
        ///// </summary>
        //private void ReaderHealth_Start()
        //{
        //    if (_ReaderType == RFIDType.ITP_U9R)
        //    {

        //        ESPReaderClass2 reader = (ESPReaderClass2)ReaderClass;

        //        while (_pauseEvent.WaitOne())
        //        {
        //            if (reader.ConnectionCheck())
        //            {
        //                SetLabelStatus("Connected");
        //                _bStopThread = false;
        //                SetFormLoadingLoop(true);
        //            }
        //            else
        //            {
        //                SetLabelStatus("NonConnect");
        //                _bStopThread = true;
        //                SetFormLoadingLoop(false);
        //            }

        //            Thread.Sleep(100);
        //        }

        //    }
        //    else if (_ReaderType == RFIDType.Alien)
        //    {

        //    }
        //    else if (_ReaderType == RFIDType.IDRO)
        //    {

        //    }
        //    else
        //    {
        //    }
        //}

        #region Pause, Resume, Stop, Close

        /// <summary>
        /// 중지(메인페이지에서 호출)
        /// </summary>
        public void Pause()
        {
            try
            {
                if (_ReaderType == RFIDType.ITP_U9R)
                {
                    ESPReaderClass2 reader = (ESPReaderClass2)ReaderClass;
                    reader.Endreading();
                    //this.Dispose();

                }
                else if (_ReaderType == RFIDType.Alien)
                {
                    AlienReaderClass reader = (AlienReaderClass)ReaderClass;
                    reader.Endreading();
                    //this.Dispose();
                }
                else if (_ReaderType == RFIDType.IDRO)
                {
                    IdroReaderClass reader = (IdroReaderClass)ReaderClass;
                    reader.Endreading();
                    //this.Dispose();
                }

                _pauseEvent.Reset();

                SetFormLoadingLoop(false);
                _bStopThread = true;
                SetLabelStartDT("중지 : " + DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }

        /// <summary>
        /// 재시작 (메인페이지에서 호출)
        /// </summary>
        public void Resume()
        {
            _pauseEvent.Set();

            SetFormLoadingLoop(true);
            
            SetLabelStartDT("시작 : " + DateTime.Now.ToString());
        }

        /// <summary>
        /// 종료(메인페이지에서 호출)
        /// </summary>
        public void Stop()
        {
            _shutdownEvent.Set();

            _pauseEvent.Set();

            SetFormLoadingLoop(false);
        }

        /// <summary>
        /// 창닫음 (메인페이지에서 호출)
        /// </summary>
        public void Close()
        {
            try
            {
                if (_ReaderType == RFIDType.ITP_U9R)
                {
                    //Stop(); 테스트 후 전부 적용 필요 
                    ESPReaderClass2 reader = (ESPReaderClass2)ReaderClass;
                    reader.Endreading();
                    this.Dispose();

                }
                else if (_ReaderType == RFIDType.Alien)
                {
                    AlienReaderClass reader = (AlienReaderClass)ReaderClass;
                    reader.Endreading();
                    this.Dispose();
                }
                else if (_ReaderType == RFIDType.IDRO)
                {
                    IdroReaderClass reader = (IdroReaderClass)ReaderClass;
                    reader.Endreading();
                    this.Dispose();
                }
                else
                {
                }
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        #endregion

    }
}
