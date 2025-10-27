using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Data;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections;

using idrp.reader.api;

namespace BPNS.EdgeMW
{
    public class IdroReaderClass
    {
        #region 전역 변수
        Reader_IDRP mReader = null;
        InventoryType_IDRP inventoryType = InventoryType_IDRP.None;

        string _sIP = "192.168.10.210";

        int _iPort = 5578;

        

        

        //TagReport report;

        TagReportCollection reports = new TagReportCollection();
        TagReport report = null;

        

        string _sConnectLastState = string.Empty;

        /// <summary>
        /// 리더기 이벤트 발생시간
        /// </summary>
        public DateTime _dteLastEvent;

        /// <summary>
        /// 상태정보 처리
        /// </summary>
        string _sState = string.Empty;

        //bool bSensorChk = true;

        

        /// <summary>
        /// 장비 연결 상태
        /// </summary>
        bool _bConnect = false;

        /// <summary>
        /// 현재 Tag ID
        /// </summary>
        string _sTagID = string.Empty;

        /// <summary>
        /// 변환된 Tag No
        /// </summary>
        string _sTagNo = string.Empty;

        /// <summary>
        /// 이벤트에서 처리된 메세지 
        /// </summary>
        public string _sActiveMsg = string.Empty;

        public string _EQ_ID
        { get; set; }

        public string _EQ_NM
        { get; set; }

        public string IP 
        { get => _sIP; set => _sIP = value; }

        public string _GATE_ID
        { get; set; }

        public string _GATE_NM
        { get; set; }

        public string _GATE_TYPE
        { get; set; }

        public string _ANT_CNT
        { get; set; }

        public string _ANT_SEQ
        { get; set; }

        public string _USE_SENSOR
        { get; set; }

        public string _NOTFY_TYPE
        { get; set; }

        public string _IN_ZN_ID
        { get; set; }

        public string _OUT_ZN_ID
        { get; set; }

        public string _IN_ZN_NM
        { get; set; }

        public string _OUT_ZN_NM
        { get; set; }

        public string _SG_YN
        { get; set; }


        public bool BConnect { get => _bConnect; set => _bConnect = value; }
        //public string MWID { get => _EQ_ID; set => _EQ_ID = value; }
        //public string MWName { get => _EQ_NM; set => _EQ_NM = value; }

        //Tag Data Table
        public DataTable _TagTable;

        public bool bSensor = false;

        public bool bInSensor = false;

        public bool bOutSendor = false;

        /// <summary>
        /// 센서 처리 이름
        /// </summary>
        public string _sSenserFlag = string.Empty;

        /// <summary>
        /// 센서 On시 DateTime
        /// </summary>
        public DateTime _dteSensor;

        /// <summary>
        /// 비정상 Tag인식 DateTime
        /// </summary>
        private DateTime _dteAbnormalSignal = DateTime.Now.AddDays(-1);

        /// <summary>
        /// 정상 Tag인식 DateTime
        /// </summary>
        private DateTime _dteNormalSignal = DateTime.Now.AddDays(-1);

        /// <summary>
        /// 비정상 신호 체크 
        /// </summary>
        private bool _bAbnormalSignal = false;

        /// <summary>
        /// 정상 신호 체크 
        /// </summary>
        private bool _bNormalSignal = false;


        /// <summary>
        /// 비정상 경광등 처리 시간(초)
        /// </summary>
        public int _AbnormalSignalSec = 10;
        /// <summary>
        /// 정상 경광등 처리 시간(초)
        /// </summary>
        public int _NormalSignalSec = 10;

        /// <summary>
        /// 센서 인식후 처리 시간(초)
        /// </summary>
        public int _SensorTiemSec = 10;

        /// <summary>
        /// 센서 인신후 마지막 센서를 찾는 시간(초)
        /// </summary>
        public int _LastSensorTimeSec = 2;

        /// <summary>
        /// RFID Tag Filter(해당정보로 시작되는 태그만 처리)
        /// </summary>
        private string _sFilter = "0544092CB48E04B430";

        private volatile IDatabaseManagement DBManagement;

        internal IDatabaseManagement dbManagement
        {
            get { return DBManagement; }
            set { DBManagement = value; }
        }

        #endregion


        #region P/Invoke...
        [DllImport("User32.dll")]
        static extern Boolean MessageBeep(UInt32 beepType);
        #endregion

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public IdroReaderClass()
        {
            _dteSensor = DateTime.Now.AddDays(-1);
        }

        public bool Connection()
        {
            /////Program.AppLog.Write("Reader Connect Start 1" + _bConnect.ToString());
            if (!_bConnect)
            {

                /////Program.AppLog.Write("Reader Connect 2 상태값 체크 , " + _bConnect.ToString());
                try
                {
                    if (mReader != null)
                    {
                        //연결해지 처리를 합니다.
                        /////Program.AppLog.Write(this, "Reader Connect 3 mReader not null");
                        //if (mReader.IsHandling)//연결되어 있는 지를 확인하고
                        //{
                        /////Program.AppLog.Write(this, "Reader Connect 4 IsHandling True");
                        _sConnectLastState = "Close B";
                        mReader.Close(CloseType_IDRP.Close);//연결을 해지합니다.
                        mReader = null;
                        Thread.Sleep(1000);
                        _sConnectLastState = "Close C";
                        //} 
                    }

                    /////Program.AppLog.Write(this, "Reader Connect 5 mReader 생성 및 초기화 시작");
                    //리더를 생성하고 초기화 합니다.
                    //리더에서 발생되는 이벤트을 처리하기 위한 핸들러를 추가해 줍니다.
                    _sConnectLastState = "New Reader B";
                    mReader = new Reader_IDRP();
                    mReader.ReaderEvent += new ReaderEventHandler_IDRP(OnReaderEvent);
                    mReader.ModelType = ModelType;
                    mReader.ConnectType = ConnectType;
                    mReader.TagType = TagType;
                    _sConnectLastState = "New Reader C";


                    if (ConnectType == ConnectType_IDRP.Tcp)
                    {
                        _sConnectLastState = "Open B";
                        mReader.Open(_sIP, _iPort);
                        /////Program.AppLog.Write(this, "Reader Connect 6 mReader Open");
                        _sConnectLastState = "Open C";
                    }


                    // Null 체크, IsHandling 체크 추가
                    if (mReader != null)
                    {
                        //if (mReader.IsHandling)
                        //{
                        _sConnectLastState = "RunRead B";
                        /////Program.AppLog.Write(this, "Reader Connect7");
                        RunRead();
                        _sConnectLastState = "RunRead C";
                        //SetIintSignal();
                        //}
                    }
                    else
                    {
                        /////Program.AppLog.Write(this, "Reader Connect8(mReader null)");
                    }

                    _sConnectLastState = "";
                }
                catch (Exception ex)
                {
                    /////Program.SysLog.Write(this, "리더 Connection Exception", ex);
                    /////Program.AppLog.Write(this, ex.ToString());
                    _bConnect = false;
                    mReader = null;
                }
            }

            return _bConnect;
        }

        public void Disconnect()
        {
            if (mReader != null)
            {//연결해지 처리를 합니다.
                mReader.Close(CloseType_IDRP.Close);//연결을 해지합니다.
                mReader = null;
            }
        }

        public void RunRead()
        {
            if (mReader != null)
            {
                try
                {
                    SetIintSignal();

                    inventoryType = InventoryType_IDRP.Multiple;
                    mReader.Reader_InventoryMultiple();
                    //mReader.InventoryInventoryNRead(3, 0, 10, null);
                    //mReader.InventoryInventoryNRead(3, 0, 8, null);
                    //inventoryType = InventoryType_IDRP.InventoryNRead;
                    //reader.Reader_InventoryNRead(1, 1, 3, null);

                    mReader.System_SetHeartBitTime(60);
                    //mReader.SetHealthCheckTime(30);
                }
                catch (Exception ex)
                {
                    /////Program.SysLog.Write(this, "리더 RunRead() Exception", ex);
                }
            }
        }

        public void StopRead()
        {
            if (mReader != null)
            {
                try
                {
                    //mReader.ReadMemory(MemoryType.User, 0, 10);
                    //mReader.ReadMemory(MemoryType.User, 0, 8); //23.12.02 주석 처리
                    mReader.StopOperation();
                    //mReader.GetOperationMode();   //이번버젼 없음
                }
                catch (Exception ex)
                {
                    /////Program.SysLog.Write(this, "리더 StopOperation() Exception", ex);
                }
            }

        }

        /// <summary>
        /// 정상 신호 Output 출력(경광등, 비프음)
        /// </summary>
        /// <returns></returns>
        public bool SetNormalSignal()
        {
            bool bRtn = false;
            try
            {


                if (mReader != null) mReader.System_SetGPIOOutputControl(1, 0);   //비정상 해제
                if (mReader != null) mReader.System_SetGPIOOutputControl(2, 1);   //.정상 실행
                
               
            }
            catch (Exception ex)
            {
                /////Program.SysLog.Write(this, "리더 SetNormalSignal() Exception", ex);
                return false;
            }
            //mReader.GetGPIOControl();
            if (_sActiveMsg == "g02") bRtn = true;
            else bRtn = false;

            return bRtn;
        }

        /// <summary>
        ///  비정상 신호 Outout 출력(경광등, 비프음)
        /// </summary>
        /// <returns></returns>
        public bool SetAbnormalSignal()
        {
            bool bRtn = false;
            try
            {
                //if (mReader != null) mReader.System_SetGPIOOutputControl(2, 1);//mReader.SetGPIOControl("02");

                if (mReader != null) mReader.System_SetGPIOOutputControl(2, 0);   //정상 해제
                if (mReader != null) mReader.System_SetGPIOOutputControl(1, 1);   //.비정상 실행
            }
            catch (Exception ex)
            {
                /////Program.SysLog.Write(this, "리더 SetAbnormalSignal() Exception", ex);
                return false;
            }
            //mReader.GetGPIOControl();
            if (_sActiveMsg == "g01") bRtn = true;
            else bRtn = false;

            return bRtn;
        }

        /// <summary>
        /// 경광등 초기화(경광등 Off)
        /// </summary>
        /// <returns></returns>
        public bool SetIintSignal()
        {
            bool bRtn = false;
            try
            {
                if (mReader != null)
                {
                    //mReader.SetGPIOControl("00");
                    //mReader.System_SetGPIOOutputControl(1, 0);
                    //mReader.System_SetGPIOOutputControl(2, 0);


                    if (mReader != null) mReader.System_SetGPIOOutputControl(2, 0); //정상 종료
                    if (mReader != null) mReader.System_SetGPIOOutputControl(1, 0); //비정상 종료
                    
                    
                }
            }
            catch (Exception ex)
            {
                /////Program.SysLog.Write(this, "리더 SetAbnormalSignal() Exception", ex);
                return false;
            }
            //mReader.GetGPIOControl();
            if (_sActiveMsg == "g00") bRtn = true;
            else bRtn = false;

            return bRtn;
        }

        public string GetSensor()
        {
            return _sActiveMsg;
        }

        public ModelType_IDRP ModelType
        {
            get
            {
                ModelType_IDRP modelType = (ModelType_IDRP)Enum.Parse(typeof(ModelType_IDRP), "IDRO900FE");
                return modelType;
            }
        }
        public ConnectType_IDRP ConnectType
        {
            get
            {
                ConnectType_IDRP connectType = ConnectType_IDRP.Tcp;
                return connectType;
            }
        }
        public TagType_IDRP TagType
        {
            get
            {
                TagType_IDRP tagType = (TagType_IDRP)Enum.Parse(typeof(TagType_IDRP), "ISO18000_6C_GEN2");
                return tagType;
            }
        }

        //public bool BSensorChk { get => bSensorChk; set => bSensorChk = value; }
        public DateTime DteSensor { get => _dteSensor; set => _dteSensor = value; }

        /// <summary>
        /// 리더기 연결을 종료
        /// 경광등 초기화
        /// </summary>
        /// <returns></returns>
        public bool Endreading()
        {
            bool bBtn = false;
            try
            {
                if (mReader != null)
                {//연결해지 처리를 합니다.
                    if (mReader.IsHandling)//연결되어 있는 지를 확인하고
                    {
                        if (_bConnect) SetIintSignal();//경광등 초기화

                        StopRead();

                        mReader.Close(CloseType_IDRP.Close);//연결을 해지합니다.
                        mReader = null;
                        bBtn = true;
                    }
                }

                return bBtn;
            }
            catch (OutOfMemoryException exOut)
            {
                /////Program.SysLog.Write(this, "리더 Endreading OutOfMemoryException", exOut);
                return false;
            }
            catch (Exception ex)
            {
                /////Program.SysLog.Write(this, "리더 Endreading Exception", ex);
                return false;
            }
        }

        public void GetReaderState()
        {
            //if(mReader != null)
            //{
            //    //mReader.SetHealthCheckTime(2000);
            //    //mReader.GetHealthCheckTime();
            //}

        }

        string _sRGState = "";

        private void CheckSignal()
        {
            //비정상 신호가 잡힌 경우 체크
            DateTime t1 = _dteAbnormalSignal;
            DateTime t2 = DateTime.Now;
            TimeSpan TS = t2 - t1;

            double diffSec = TS.TotalSeconds;
            if (diffSec < _AbnormalSignalSec)
            {
                //경광등 비정상
                SetAbnormalSignal();
            }
            else
            {
                //정상 신호가 잡힌 경우 체크
                DateTime tNormal = _dteNormalSignal;
                DateTime tNow = DateTime.Now;
                TimeSpan TS2 = tNow - tNormal;

                double diffSecNomal = TS2.TotalSeconds;
                if (diffSecNomal < _NormalSignalSec)
                {
                    //경광등 정상
                    SetNormalSignal();
                }
                else
                {
                    //경광등 Default
                    SetIintSignal();
                }
            }
        }

        public void ReaderTagDataGet()
        {
            string sFlag = string.Empty;

            //센서 체크
            //센서 사용일 경우 처리
            if (_USE_SENSOR == "Y")
            {
                //센서 신호가 잡힌 경우 체크
                DateTime tSensor = _dteSensor;
                DateTime tSensorNow = DateTime.Now;
                TimeSpan TsSensor = tSensorNow - tSensor;

                double SensorDiffSec = TsSensor.TotalSeconds;

                //센서인식이 지난 경우 처리
                if (SensorDiffSec > _SensorTiemSec)
                {
                    _sSenserFlag = string.Empty;
                    CheckSignal();
                    reports.ClearAll();
                    return;
                }

                //센서가 인식후 마지막센서 기준시간보다 적을 경우(Default : 2초) 
                if (SensorDiffSec <= _LastSensorTimeSec)
                {
                    //센서가 2개 인식된 경우 Flag 처리
                    if (_sSenserFlag.IndexOf('|') > 0)
                    {
                        sFlag = _sSenserFlag.Substring(_sSenserFlag.IndexOf('|') + 1);
                    }
                }

                //Flag 처리(센서2개 인식 안된경우)
                if (sFlag.Length == 0)
                {
                    //센서가 인식후 마지막센서 기준시간보다 크고
                    //센서처리시간 보다 적을 경우
                    if (SensorDiffSec > _LastSensorTimeSec && SensorDiffSec <= _SensorTiemSec)
                    {
                        //2개 센서다 인식되었을 경우 마지막, 하나만 인식될 경우 처리

                        if (_sSenserFlag.IndexOf('|') > 0)
                        {
                            sFlag = _sSenserFlag.Substring(_sSenserFlag.IndexOf('|') + 1);
                        }
                        else
                        {
                            sFlag = _sSenserFlag;
                        }
                    }
                }

                //Flag 없을 경우 처리
                if (sFlag.Length == 0)
                {
                    CheckSignal();
                    return;
                }


            }
            else
            {
                //_sSenserFlag = string.Empty;
                CheckSignal();
            }

            List<TagReport> TagList = new List<TagReport>();
            

            //태그 정보 가져오기
            for (int iRow = 0; iRow < reports.Count; iRow++)
            {
                TagList.Add(reports[iRow]);
                Program.AppLog.Write(this, _sIP + ":TagList.Add(reports[iRow]);" + reports[iRow].TagIdHex);
            }

            reports.ClearAll();

            

            //Reader의 Tag 목록
            try
            {
                for (int i = 0; i < TagList.Count; i++)
                {

                    if (TagList[i].TagIdHex.IndexOf(_sFilter) < 0)
                    {
                        string sExTagUID = "필터제외";
                        continue;
                    }

                    InsertTagData(TagList[i].TagIdHex, sFlag);

                    
                }

            }
            catch (Exception ex)
            {

            }


        }

        public void InsertTagData(string strTagUID, string sSensorFlag)
        {
            if (strTagUID.IndexOf(_sFilter) < 0)
            {
                string sExTagUID = "필터제외";
                return;
            }

            Program.AppLog.Write(this, _sIP + ":DB처리" + strTagUID);

            Hashtable htParamsHashtable;
            DataTable dtRtn = null;
            DataSet dsResult;

            try
            {
                htParamsHashtable = new Hashtable();
                htParamsHashtable.Clear();
                htParamsHashtable.Add("IN_TAG_ID", strTagUID.Substring(0, 24));
                htParamsHashtable.Add("IN_GATE_ID", _GATE_ID);
                htParamsHashtable.Add("IN_SENSOR", sSensorFlag);

                if (DBManagement == null)
                {
                    DBManagement = new DatabaseManagementOracle();
                }

                dsResult = dbManagement.ExecuteSelectProcedure("PKG_MW_PROC.TAG_READING", htParamsHashtable, 3000);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                {
                    dtRtn = dsResult.Tables[0];

                    //목지점인 경우 무조건 적색불(비정상 신호)를 .울린다

                    if (_GATE_TYPE == "CHK")
                    {
                        _dteAbnormalSignal = DateTime.Now;
                        _bAbnormalSignal = true;
                        SetAbnormalSignal();
                    }
                    else
                    {
                        //NOR
                        if (dtRtn.Rows[0]["NOR_ILG_FL"].ToString() == "NOR")
                        {
                            //정상 태그일 경우 처리

                            //비정상 신호가 잡힌 경우 체크
                            DateTime t1 = _dteAbnormalSignal;
                            DateTime t2 = DateTime.Now;
                            TimeSpan TS = t2 - t1;

                            double diffSec = TS.TotalSeconds;
                            if (diffSec > _AbnormalSignalSec)
                            {
                                _dteNormalSignal = DateTime.Now;
                                _bNormalSignal = true;
                                SetNormalSignal();
                            }
                            else
                            {
                                SetAbnormalSignal();
                            }



                        }
                        else if (dtRtn.Rows[0]["NOR_ILG_FL"].ToString() == "ILG")
                        {
                            //비정상 태그 상태일 경우 처리

                            _dteAbnormalSignal = DateTime.Now;
                            _bAbnormalSignal = true;
                            SetAbnormalSignal();
                        }
                        else if (dtRtn.Rows[0]["NOR_ILG_FL"].ToString() == "PASS")
                        {
                            //아무 동작도 하지 않을 경우 처리
                        }
                    }

                    Program.AppLog.Write(this, _sIP + ", " + dtRtn.Rows[0]["NOR_ILG_FL"].ToString() + ", " + dtRtn.Rows[0]["MGMT_NO"].ToString());
                    ((MainForm)(Application.OpenForms["MainForm"])).AddLogList(_sIP + ", " + dtRtn.Rows[0]["NOR_ILG_FL"].ToString() + ", " + dtRtn.Rows[0]["MGMT_NO"].ToString());
                }
                else
                {
                    Program.AppLog.Write(this, _sIP + "데이터없음:" + strTagUID.Substring(0, 24) + "," + _GATE_ID);
                    ((MainForm)(Application.OpenForms["MainForm"])).AddLogList(_sIP + "데이터없음:" + strTagUID.Substring(0, 24) + "," + _GATE_ID);
                    //mReader.ExternalOutput = "0";
                    SetIintSignal();
                }

            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, _sIP + ":" + e.ToString());
            }

            
        }


        private double GetRssi(string rssi)
        {
            int v = Convert.ToInt16(rssi, 16);
            double p = (double)v / 10;
            return p;
        }

        string _sType = string.Empty;

        //리더에서 발생된 이벤트를 처리합니다.
        //별도의 쓰레드에서 발생된 이벤트임에 유의하시기 바랍니다.
        private void OnReaderEvent(object sender, ReaderEventArgs_IDRP e)
        {
            //확인 필요
            //if (this.InvokeRequired)
            //{
            //    this.BeginInvoke(new ReaderEventHandler_IDRP(OnReaderEvent), new object[] { sender, e });
            //    return;
            //}

            //Console.WriteLine("e.Type = " + e.Type);
            _dteLastEvent = DateTime.Now;

            if (_sType != e.Type.ToString())
            {
                _sType = e.Type.ToString();
                /////Program.AppLog.Write(this, _sType);
            }

            Program.AppLog.Write(this, _sIP + ":Type:" + e.Type.ToString());

            switch (e.Type)
            {
                case EventType_IDRP.Connected:
                    //상태 메시지를 표시한다.
                    _sState = e.Message;
                    _bConnect = true;
                    Program.AppLog.Write(this, _sIP +  ":_bConnect = true");
                    break;
                case EventType_IDRP.Disconnected:
                    //상태 메시지를 표시한다.
                    _sState = e.Message;
                    //해지상태로 UI 조정합니다.
                    _bConnect = false;

                    if (mReader != null)
                    {//연결해지 처리를 합니다.
                        mReader.Close(CloseType_IDRP.Close);//연결을 해지합니다.
                        mReader = null;
                        /////Program.AppLog.Write(this, "EventType.Disconnected mReader.Close");
                    }
                    else
                    {
                        /////Program.AppLog.Write(this, "EventType.Disconnected mReader null");
                    }
                    Program.AppLog.Write(this, _sIP + ":_bConnect = false");
                    break;
                case EventType_IDRP.Timeout:



                    if (mReader != null)//consider access operation timeout
                        mReader.StopOperation();

                    _sState = e.Message;
                    //Program.AppLog.Write(this, _sIP + ":Timeout");
                    break;

                case EventType_IDRP.HeartBit:
                    ((MainForm)(Application.OpenForms["MainForm"])).lstData.Add(_GATE_ID);
                    break;

                // 이번 버젼에서는 없어짐. 확인 필요
                //case EventType.HeartBit:
                //    // "1#" check...
                //    mReader.HealthCheckAck();
                //    /////Program.AppLog.Write(this, "HealthCheck");
                //    break;
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                case EventType_IDRP.Command:
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                // BASIC OPERATIONS EVENTS
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                case EventType_IDRP.Inventory:
                //case EventType_IDRP.HandShackInventory:
                //case EventType_IDRP.GetStoredData:
                case EventType_IDRP.ReadMemory:
                case EventType_IDRP.WriteMemory:
                case EventType_IDRP.Lock:
                case EventType_IDRP.Kill:
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                // CONFIGURATIONS EVENTS
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                case EventType_IDRP.ConfigReader_Power:
                case EventType_IDRP.ConfigReader_Version:
                case EventType_IDRP.ConfigReader_AccessPwd:
                case EventType_IDRP.ConfigReader_GlobalBand:
                //case EventType_IDRP.Buzzer:
                case EventType_IDRP.ConfigReader_ContinueMode:
                case EventType_IDRP.ConfigReader_ReportingType: //추가

                case EventType_IDRP.ConfigReader_Port:
                case EventType_IDRP.ConfigReader_Selection:
                case EventType_IDRP.ConfigReader_Algorithm:
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                case EventType_IDRP.ConfigReader_HoppingMode:
                case EventType_IDRP.ConfigReader_ChNumber:
                case EventType_IDRP.ConfigReader_LinkProfile:
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                // config system
                case EventType_IDRP.ConfigSystem_Basic:
                case EventType_IDRP.ConfigSystem_Connection:
                case EventType_IDRP.ConfigSystem_StartOption:
                case EventType_IDRP.ConfigSystem_Version:
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                // config ethernet
                case EventType_IDRP.ConfigEthernet_Connection:
                case EventType_IDRP.ConfigSystem_GPIO:



                    Program.AppLog.Write(this, _sIP + ":" + "데이터 처리1111");

                    /*byte[] payload = new byte[] {
                        //0x3e, - 제거되서 넘어온다.
                        0x31,0x32,0x33,0x34,0x35, 0x0d,0x0a,
                        0x3e, 0x35,0x34,0x33,0x32,0x31, 0x0d,0x0a,
                        0x3e, 0x31,0x32,0x33,0x34,0x35,
                        //0x0d,0x0a, - 제거되서 넘어온다
                    };*/
                    //상기와 같이 여러 개의 응답이 담겨 있을 수 있는 데 먼저 분리해 준다.
                    string szPayload = Encoding.ASCII.GetString(e.Payload);
                    string[] szResponses = szPayload.Split(new string[] { "\r\n>" }, StringSplitOptions.RemoveEmptyEntries);

                    Program.AppLog.Write(this, _sIP + ":" + "상단:" +  szPayload);
                    ///////////////////////////////////////////////////////////////////////////////////////////////////
                    // Tag Memory : [#]T3000111122223333444455556666[##]
                    // Response Code : [#]C##
                    // Settings Values : p0, c1, ...
                    ///////////////////////////////////////////////////////////////////////////////////////////////////
                    char code;
                    string szValue = string.Empty;
                    string tagID;
                    string port ="";

                    int nPos = 1;

                    //각 응답에 대해서 처리를 한다.
                    foreach (string szResponse in szResponses)
                    {
                        code = szResponse[nPos];
                        switch (code)
                        {
                            case 'T'://Tag Memory
                                szValue = szResponse.Substring(nPos + 1, szResponse.Length - 2);

                                if (0 < szValue.IndexOf(';'))
                                {
                                    string[] dataInfo = szValue.Split(';');
                                    tagID = dataInfo[0];
                                    if (dataInfo.Length > 1)
                                    {
                                        for (int i = 1; i < dataInfo.Length; i++)
                                        {
                                            char type = dataInfo[i][0];

                                            switch (type)
                                            {
                                                case 'A':
                                                    // antenna port
                                                    port = dataInfo[i].Substring(1);
                                                    break;
                                                case 'R':
                                                    // rssi value
                                                    string szRssi = dataInfo[i].Substring(1);
                                                    double dRssi = GetRssi(szRssi); // rssi value
                                                    break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    tagID = szValue;
                                    port = "0";
                                }

                                if (tagID.Length >= 28)
                                {
                                    tagID = tagID.Substring(4, 24);
                                }

                                if (tagID.IndexOf(_sFilter) < 0)
                                {
                                    Program.AppLog.Write(this, _sIP + ", 필터제외: " + tagID);
                                    //((MainForm)(Application.OpenForms["MainForm"])).AddLogList(_sIP + ", 필터제외: " + tagID);

                                }
                                else
                                {
                                    report = reports.Find(tagID);

                                    //report = new TagReport(_sTagID, _sTagNo, 1, 0, "", "");

                                    if (report != null)//찾은 경우에는 반드시 '하나'인 데
                                    {
                                        int iPort = 0;
                                        int.TryParse(port, out iPort);
                                        report.Port = iPort;  //sdk.ToInt32(port, 0);
                                        report.TotalRead++;

                                        szValue = tagID;

                                        //lbxResponses.Items.Insert(0, szValue);
                                    }
                                    else//찾지 못한 경우에는 새로 추가한다.
                                    {
                                        //추가한다.
                                        int iPort = 0;
                                        int.TryParse(port, out iPort);
                                        report = new TagReport(tagID, 1, iPort, "", "");
                                        reports.Add(report);


                                        szValue = tagID;
                                        Program.AppLog.Write(this, _sIP + ", 태그인식: " + tagID);
                                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList(_sIP + ", 태그인식: " + tagID);

                                        //lbxResponses.Items.Insert(0, szValue);

                                    }
                                }


                                break;

                            case 'C'://Response Code
                                szValue = szResponse.Substring(nPos + 1, szResponse.Length - (nPos + 1));//exclude [#]T/C
                                string szCode = szValue.Substring(0, 2);
                                string szVerify = szValue.Length > 2 ? szValue.Substring(2, szValue.Length - 2) : string.Empty;
                                //display result code-descriptions
                                szValue = szCode + "-" + mReader.Responses(szCode);

                                ////debug : display elapsed time of access-operations
                                //if (IsWatching())
                                //{
                                //    szValue = szValue + "-asdf" + WatchElapsed() + "ms";
                                //    WatchReset();
                                //}
                                ////debug : display read values if exists : access-write
                                //if (szVerify.Length > 0)
                                //{
                                //    szValue = szValue + "-" + szVerify;
                                //}
                                //lbxResponses.Items.Insert(0, szValue);

                                _sActiveMsg = "C:" + szResponse;
                                Program.AppLog.Write(this, _sIP + ":" + _sActiveMsg);

                                break;

                            //case 'A':
                            //    if (szResponse[1] == '#')
                            //        _sActiveMsg = szResponse;
                            //    break;
                            case 'V':
                                // get, set
                                // config result value
                                // ex)
                                // [config type]V[value]

                                _sActiveMsg = "V:" + szResponse;
                                Program.AppLog.Write(this, _sIP + ":" + _sActiveMsg);
                                break;

                            default:
                                _sActiveMsg = szResponse;
                                Program.AppLog.Write(this, _sIP + ":Default1:" + _sActiveMsg);
                                break;
                        }
                    }

                    break;

                default:
                    string sPayload = Encoding.ASCII.GetString(e.Payload);
                    _sActiveMsg = sPayload;
                    Program.AppLog.Write(this, _sIP + ":Default2:" + _sActiveMsg);
                    break;
            }

            if (_sActiveMsg.Trim().Length == 5)
            {
                if (_sActiveMsg.Trim() == "G-1 1" || _sActiveMsg.Trim() == "G-2 1")
                {
                    if (bSensor)
                    {
                        bSensor = false;
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList(_sIP + ":센서 Off" + "(" + _sActiveMsg.Trim() + ")" + "(" + _sSenserFlag + ")");
                        Program.AppLog.Write(this, _sIP + ":센서 Off" + "(" + _sActiveMsg.Trim() + ")" + "(" + _sSenserFlag + ")");
                    }

                }
                else if (_sActiveMsg.Trim() == "G-1 0" || _sActiveMsg.Trim() == "G-2 0")
                {
                    if (!bSensor)
                    {
                        bSensor = true;

                        if (_USE_SENSOR == "Y")
                        {
                            if (_sSenserFlag.Length == 0) _sSenserFlag = _sActiveMsg.Trim();
                            else _sSenserFlag += "|" + _sActiveMsg.Trim();
                        }

                        _dteSensor = DateTime.Now;
                        ((MainForm)(Application.OpenForms["MainForm"])).AddLogList(_sIP + ":센서 On" + "(" + _sActiveMsg.Trim() + ")" + "(" + _sSenserFlag + ")");
                        Program.AppLog.Write(this, _sIP + ":센서 On" + "(" + _sActiveMsg.Trim() + ")" + "(" + _sSenserFlag + ")");
                    }
                }
            }
            else
            {
                Program.AppLog.Write(this, _sIP + ":기타:" + _sActiveMsg.Trim());
            }
        }
    }

    public class TagReport// : IEditableObject
    {
        private string tagIdHex;
        private long totalRead;
        private int port;
        private string sensorData;
        private string sensorState;

        public object Tag;


        public TagReport()
        {
        }

        public TagReport(/*TagReportCollection records, */string tagIdHex, long totalRead, int port, string sensorData, string sensorState)
        {
            this.tagIdHex = tagIdHex;
            this.totalRead = totalRead;
            this.port = port;
            this.sensorData = sensorData;
            this.sensorState = sensorState;
        }

        public string TagIdHex
        {
            get { return tagIdHex; }
            set
            {
                tagIdHex = value;
            }
        }
        public long TotalRead
        {
            get { return totalRead; }
            set
            {
                totalRead = value;
            }
        }
        public int Port
        {
            get { return port; }
            set
            {
                port = value;
            }
        }
        public string SensorData
        {
            get { return sensorData; }
            set
            {
                sensorData = value;
            }
        }
        public string SensorState
        {
            get { return sensorState; }
            set
            {
                sensorState = value;
            }
        }
    }

    public class TagReportCollection : CollectionBase//, IBindingList
    {
        Dictionary<string, TagReport> keys = new Dictionary<string, TagReport>();

        public void Add(TagReport record)
        {
            base.List.Add(record);
            keys[record.TagIdHex] = record;
        }
        public TagReport this[int index]
        {
            get { return (TagReport)base.List[index]; }
        }
        public int IndexOf(TagReport record)
        {
            return List.IndexOf(record);
        }

        public TagReport Find(string szTagId)
        {
            TagReport report;
            return keys.TryGetValue(szTagId, out report) ? report : null;
        }
        public void ClearAll()
        {
            base.Clear();
            keys.Clear();
        }
        public void RemoveAt(int index, string szTagId)
        {
            base.RemoveAt(index);
            keys.Remove(szTagId);
        }

    }

    //public class TagReport
    //{
    //    private string tagIdHex;
    //    private string tagNo;
    //    private long totalRead;
    //    private int port;
    //    private string sensorData;
    //    private string sensorState;

    //    /// <summary>
    //    /// 태그 인식 처리 시간(문자열)
    //    /// </summary>
    //    private string sTime;

    //    /// <summary>
    //    /// 태그 인식 처리 시간
    //    /// </summary>
    //    private DateTime dteTime;

    //    /// <summary>
    //    /// 서버 전송 성공 여부
    //    /// </summary>
    //    private bool bSendOk = false;

    //    /// <summary>
    //    /// 서버 전송 시간
    //    /// </summary>
    //    private DateTime dteSendServer;

    //    /// <summary>
    //    /// 서버 전송 여부
    //    /// </summary>
    //    private bool bSendServer = false;

    //    public object Tag;


    //    public TagReport()
    //    {
    //    }

    //    public TagReport(string tagIdHex, string stagNo, long totalRead, int port, string sensorData, string sensorState)
    //    {
    //        this.tagIdHex = tagIdHex;
    //        this.tagNo = stagNo;
    //        this.totalRead = totalRead;
    //        this.port = port;
    //        this.sensorData = sensorData;
    //        this.sensorState = sensorState;
    //        this.DteTime = DateTime.Now;
    //        this.sTime = this.dteTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
    //        this.bSendOk = false;
    //        this.bSendServer = false;

    //        /////Program.AppLog.Write(this, "인식 : " + stagNo + "(" + this.STime + ")");
    //        /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("인식 : " + stagNo + "(" + this.STime + ")");
    //    }

    //    public void SetTagReport(string tagIdHex, string stagNo, long totalRead, int port, string sensorData, string sensorState)
    //    {
    //        if (this.tagIdHex == tagIdHex)
    //        {
    //            this.totalRead += 1;

    //            //Program.AppLog.Write(this, "인식 : " + stagNo + "(" + this.STime + ")");
    //            //((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("인식 : " + stagNo + "(" + this.STime + ")");
    //        }
    //        else
    //        {
    //            this.tagIdHex = tagIdHex;
    //            this.tagNo = stagNo;
    //            this.totalRead = totalRead;
    //            this.port = port;
    //            this.sensorData = sensorData;
    //            this.sensorState = sensorState;
    //            this.DteTime = DateTime.Now;
    //            this.sTime = this.dteTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
    //            this.bSendOk = false;
    //            this.bSendServer = false;

    //            /////Program.AppLog.Write(this, "인식 : " + stagNo + "(" + this.STime + ")");
    //            /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("인식 : " + stagNo + "(" + this.STime + ")");
    //        }
    //    }



    //    public void Clear()
    //    {
    //        this.tagIdHex = string.Empty;
    //        this.tagNo = string.Empty;
    //        this.totalRead = 0;
    //        this.port = 0;
    //        this.sensorData = string.Empty;
    //        this.sensorState = string.Empty;
    //        this.sTime = string.Empty;
    //        this.bSendOk = false;
    //        this.BSendServer = false;

    //    }

    //    public void SetSend(string sTagNo)
    //    {
    //        if (this.tagNo == sTagNo)
    //        {
    //            this.bSendOk = true;
    //        }
    //    }

    //    public string TagIdHex
    //    {
    //        get { return tagIdHex; }
    //        set
    //        {
    //            tagIdHex = value;
    //        }
    //    }

    //    public string TagNo
    //    {
    //        get => tagNo;
    //        set => tagNo = value;
    //    }

    //    public long TotalRead
    //    {
    //        get { return totalRead; }
    //        set
    //        {
    //            totalRead = value;
    //        }
    //    }
    //    public int Port
    //    {
    //        get { return port; }
    //        set
    //        {
    //            port = value;
    //        }
    //    }
    //    public string SensorData
    //    {
    //        get { return sensorData; }
    //        set
    //        {
    //            sensorData = value;
    //        }
    //    }
    //    public string SensorState
    //    {
    //        get { return sensorState; }
    //        set
    //        {
    //            sensorState = value;
    //        }
    //    }

    //    public string STime
    //    {
    //        get => sTime;
    //        set => sTime = value;
    //    }

    //    public DateTime DteTime
    //    {
    //        get => dteTime;
    //        set => dteTime = value;
    //    }

    //    public bool BSendOk { get => bSendOk; set => bSendOk = value; }
    //    public DateTime DteSendServer { get => dteSendServer; set => dteSendServer = value; }
    //    public bool BSendServer { get => bSendServer; set => bSendServer = value; }
    //}
}
