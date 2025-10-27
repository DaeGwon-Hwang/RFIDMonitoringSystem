using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using idro.reader.api;
using System.Diagnostics;
using System.Threading;
using System.Data;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections;

namespace BPNS.ReaderHelper
{
    public class IdroReaderClass
    {
        #region 전역 변수
        Reader mReader = null;
        InventoryType inventoryType = InventoryType.None;

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

        public DateTime _dteSensor;

        private DateTime _dteAbnormalSignal = DateTime.Now.AddDays(-1);

        private DateTime _dteNormalSignal = DateTime.Now.AddDays(-1);

        /// <summary>
        /// 비정상 신호 체크 
        /// </summary>
        private bool _bAbnormalSignal = false;

        /// <summary>
        /// 정상 신호 체크 
        /// </summary>
        private bool _bNormalSignal = false;



        public int _AbnormalSignalSec = 10;
        public int _NormalSignalSec = 10;

        public int _SensorTiemSec = 10;

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
            //3C3400BACEBBEA3938BBE732343336 03
            //string TCode = "2C3400BACEBBEA3939BBE735333336 03";

            //int iChkLast = TCode.IndexOf(" 03");

            //if (iChkLast > 0) iChkLast = iChkLast - 6;
            //else iChkLast = TCode.Length - 6;

            //string Testaa1 = Reader.MakeTextFromHex(TCode.Substring(6, iChkLast));
            //string Testaa2 = Reader.MakeTextEuckrFromHex(TCode.Substring(6, iChkLast)); //정상

            //2C4400EBB680EC82B03939EC82AC3530333720 03
            //string Testaa1 = Reader.MakeTextFromHex("EBB680EC82B03939EC82AC3530333720");//정상
            //string Testaa2 = Reader.MakeTextEuckrFromHex("EBB680EC82B03939EC82AC3530333720");



            //EBB680EC82B03938EC82AC3137393700
            //string Testaa1 = Reader.MakeTextFromHex("EBB680EC82B03939EBB0943837343520").Replace("\0", ""); ;
            //string Testaa2 = Reader.MakeTextEuckrFromHex("EBB680EC82B03939EBB0943837343520").Replace("\0", ""); ;


            _dteSensor = DateTime.Now.AddDays(-1);
            try
            {
                //if (mReader != null)
                //{   //연결해지 처리를 합니다.
                //    if (mReader.IsHandling)              //연결되어 있는 지를 확인하고
                //    {
                //        mReader.Close(CloseType.Close);  //연결을 해지합니다.
                //        mReader = null;
                //    }
                //}

                ////리더를 생성하고 초기화 합니다.
                ////리더에서 발생되는 이벤트을 처리하기 위한 핸들러를 추가해 줍니다.
                //mReader = new Reader();
                //mReader.ReaderEvent += new ReaderEventHandler(OnReaderEvent);
                //mReader.ModelType = ModelType;
                //mReader.ConnectType = ConnectType;
                //mReader.TagType = TagType;

                //if (ConnectType == ConnectType.Tcp)
                //    mReader.Open(_sIP, _iPort);

                //RunRead();
                //SetAbnormalSignal();

            }
            catch (OutOfMemoryException) { }
            catch (Exception) { }



            //try
            //{
            //    if (mReader != null)
            //    {   //연결해지 처리를 합니다.
            //        if (mReader.IsHandling)              //연결되어 있는 지를 확인하고
            //        {
            //            mReader.Close(CloseType.Close);  //연결을 해지합니다.
            //            mReader = null;
            //        }
            //    }

            //    //리더를 생성하고 초기화 합니다.
            //    //리더에서 발생되는 이벤트을 처리하기 위한 핸들러를 추가해 줍니다.
            //    mReader = new Reader();
            //    mReader.ReaderEvent += new ReaderEventHandler(OnReaderEvent);
            //    mReader.ModelType = ModelType;
            //    mReader.ConnectType = ConnectType;
            //    mReader.TagType = TagType;

            //    if (ConnectType == ConnectType.Tcp)
            //        mReader.Open(_sIP, _iPort);

            //    RunRead();
            //    SetAbnormalSignal();

            //}
            //catch (OutOfMemoryException) { }
            //catch (Exception) { }
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
                    {//연결해지 처리를 합니다.
                        /////Program.AppLog.Write(this, "Reader Connect 3 mReader not null");
                        //if (mReader.IsHandling)//연결되어 있는 지를 확인하고
                        //{
                        /////Program.AppLog.Write(this, "Reader Connect 4 IsHandling True");
                        _sConnectLastState = "Close B";
                        mReader.Close(CloseType.Close);//연결을 해지합니다.
                        mReader = null;
                        Thread.Sleep(1000);
                        _sConnectLastState = "Close C";
                        //} 
                    }

                    /////Program.AppLog.Write(this, "Reader Connect 5 mReader 생성 및 초기화 시작");
                    //리더를 생성하고 초기화 합니다.
                    //리더에서 발생되는 이벤트을 처리하기 위한 핸들러를 추가해 줍니다.
                    _sConnectLastState = "New Reader B";
                    mReader = new Reader();
                    mReader.ReaderEvent += new ReaderEventHandler(OnReaderEvent);
                    mReader.ModelType = ModelType;
                    mReader.ConnectType = ConnectType;
                    mReader.TagType = TagType;
                    _sConnectLastState = "New Reader C";


                    if (ConnectType == ConnectType.Tcp)
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
                mReader.Close(CloseType.Close);//연결을 해지합니다.
                mReader = null;
            }
        }

        public void RunRead()
        {
            //inventoryType = InventoryType.HandShake;
            //mReader.InventoryHandShake();
            if (mReader != null)
            {
                try
                {
                    SetIintSignal();

                    inventoryType = InventoryType.HandShake;
                    mReader.InventoryHandShake();
                    //mReader.InventoryInventoryNRead(3, 0, 10, null);
                    //mReader.InventoryInventoryNRead(3, 0, 8, null);
                    mReader.SetHealthCheckTime(30);
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
                    mReader.GetOperationMode();
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
                if (mReader != null) mReader.SetGPIOControl("01");
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
                if (mReader != null) mReader.SetGPIOControl("02");
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
                if (mReader != null) mReader.SetGPIOControl("00");
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

        public ModelType ModelType
        {
            get
            {
                ModelType modelType = (ModelType)Enum.Parse(typeof(ModelType), "IDRO900F");
                return modelType;
            }
        }
        public ConnectType ConnectType
        {
            get
            {
                ConnectType connectType = ConnectType.Tcp;
                return connectType;
            }
        }
        public TagType TagType
        {
            get
            {
                TagType tagType = (TagType)Enum.Parse(typeof(TagType), "ISO18000_6C_GEN2");
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

                        mReader.Close(CloseType.Close);//연결을 해지합니다.
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
            List<TagReport> TagList = new List<TagReport>();
            

            //태그 정보 가져오기
            for (int iRow = 0; iRow < reports.Count; iRow++)
            {
                TagList.Add(reports[iRow]);
            }

            reports.ClearAll();

            //센서 체크
            //센서 사용일 경우 처리
            if (_USE_SENSOR == "Y")
            {
                //비정상 신호가 잡힌 경우 체크
                DateTime tSensor = _dteSensor;
                DateTime tSensorNow = DateTime.Now;
                TimeSpan TsSensor = tSensorNow - tSensor;

                double SensorDiffSec = TsSensor.TotalSeconds;
                if (SensorDiffSec > _SensorTiemSec)
                {
                    CheckSignal();
                    return;
                }
            }

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

                    InsertTagData(TagList[i].TagIdHex);

                    
                }

            }
            catch (Exception ex)
            {

            }


        }

        public void InsertTagData(string strTagUID)
        {
            if (strTagUID.IndexOf(_sFilter) < 0)
            {
                string sExTagUID = "필터제외";
                return;
            }

            Hashtable htParamsHashtable;
            DataTable dtRtn = null;
            DataSet dsResult;

            try
            {
                htParamsHashtable = new Hashtable();
                htParamsHashtable.Clear();
                htParamsHashtable.Add("IN_TAG_ID", strTagUID.Substring(0, 24));
                htParamsHashtable.Add("IN_GATE_ID", _GATE_ID);

                if (DBManagement == null)
                {
                    DBManagement = new DatabaseManagementOracle();
                }

                dsResult = dbManagement.ExecuteSelectProcedure("PKG_MW_PROC.TAG_READING", htParamsHashtable, 3000);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                {
                    dtRtn = dsResult.Tables[0];

                    //목지점인 경우 무조건 적색불(비정상 신호)를 울린다.
                    if (_GATE_TYPE == "CHK")
                    {
                        _dteAbnormalSignal = DateTime.Now;
                        _bAbnormalSignal = true;
                        SetAbnormalSignal();
                    }
                    else
                    {
                        if (dtRtn.Rows[0]["TAG_STATUS"].ToString() == "PASS")
                        {
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
                        else
                        {
                            _dteAbnormalSignal = DateTime.Now;
                            _bAbnormalSignal = true;
                            SetAbnormalSignal();
                        }
                    }

                }
                else
                {
                    //mReader.ExternalOutput = "0";
                    SetIintSignal();
                }

            }
            catch (Exception e)
            {

            }

            
        }

        private bool carNumCheck(string sCarNum)
        {
            bool bRtn = false;
            string sCheckCarNo = sCarNum;
            // sCarNum = "T1234";

            //마산 추가 요청 사항
            //22.12.08 차량번호 맨앞이 M일 경우 제외하고 차량번호 체크 한다.

            if (sCheckCarNo.Length >= 7)
            {
                if (sCheckCarNo.Substring(0, 1) == "M")
                {
                    sCheckCarNo = sCheckCarNo.Substring(1);
                }

                if (sCheckCarNo.Length == 7 || sCheckCarNo.Length == 8 || sCheckCarNo.Length == 9)
                {
                    Regex regex = new Regex(@"(([0-9]{2}[가-힣]{1}[0-9]{4}|[0-9]{3}[가-힣]{1}[0-9]{4}|[가-힣]{2}[0-9]{2}[가-힣]{1}[0-9]{4}))");
                    //Regex regex = new Regex(@"(([0-9]{2}[가-힣]{1}[0-9]{4}|[가-힣]{2}[0-9]{2}[가-힣]{1}[0-9]{4}))");
                    Match m = regex.Match(sCheckCarNo);
                    if (m.Success)
                    {
                        bRtn = true;
                    }
                    else
                    {
                        bRtn = false;
                    }
                }
                //else
                //{
                //    if (sCarNum.Trim().Length >= 5)
                //    {
                //        bRtn = true;
                //    }
                //    else
                //    {
                //        bRtn = false;
                //    }


                //}
            }



            return bRtn;
        }

        private bool carNumCheck_ori(string sCarNum)
        {
            bool bRtn = false;
            string sCheckCarNo = sCarNum;
            // sCarNum = "T1234";

            //마산 추가 요청 사항
            //22.12.08 차량번호 맨앞이 M일 경우 제외하고 차량번호 체크 한다.

            if (sCheckCarNo.Length >= 7)
            {
                if (sCheckCarNo.Substring(0, 1) == "M")
                {
                    sCheckCarNo = sCheckCarNo.Substring(1);
                }

                if (sCheckCarNo.Length == 7 || sCheckCarNo.Length == 8 || sCheckCarNo.Length == 9)
                {
                    Regex regex = new Regex(@"(([0-9]{2}[가-힣]{1}[0-9]{4}|[0-9]{3}[가-힣]{1}[0-9]{4}|[가-힣]{2}[0-9]{2}[가-힣]{1}[0-9]{4}))");
                    //Regex regex = new Regex(@"(([0-9]{2}[가-힣]{1}[0-9]{4}|[가-힣]{2}[0-9]{2}[가-힣]{1}[0-9]{4}))");
                    Match m = regex.Match(sCheckCarNo);
                    if (m.Success)
                    {
                        bRtn = true;
                    }
                    else
                    {
                        bRtn = false;
                    }
                }
                //else
                //{
                //    if (sCarNum.Trim().Length >= 5)
                //    {
                //        bRtn = true;
                //    }
                //    else
                //    {
                //        bRtn = false;
                //    }


                //}
            }



            return bRtn;
        }

        string _sType = string.Empty;

        //리더에서 발생된 이벤트를 처리합니다.
        //별도의 쓰레드에서 발생된 이벤트임에 유의하시기 바랍니다.
        private void OnReaderEvent(object sender, ReaderEventArgs e)
        {

            //Console.WriteLine("e.Type = " + e.Type);
            _dteLastEvent = DateTime.Now;

            if (_sType != e.Type.ToString())
            {
                _sType = e.Type.ToString();
                /////Program.AppLog.Write(this, _sType);
            }

            switch (e.Type)
            {
                case EventType.Connected:
                    //상태 메시지를 표시한다.
                    _sState = e.Message;
                    _bConnect = true;
                    break;
                case EventType.Disconnected:
                    //상태 메시지를 표시한다.
                    _sState = e.Message;
                    //해지상태로 UI 조정합니다.
                    _bConnect = false;

                    if (mReader != null)
                    {//연결해지 처리를 합니다.
                        mReader.Close(CloseType.Close);//연결을 해지합니다.
                        mReader = null;
                        /////Program.AppLog.Write(this, "EventType.Disconnected mReader.Close");
                    }
                    else
                    {
                        /////Program.AppLog.Write(this, "EventType.Disconnected mReader null");
                    }

                    break;
                case EventType.Timeout:



                    if (mReader != null)//consider access operation timeout
                        mReader.StopOperation();

                    _sState = e.Message;
                    break;
                case EventType.HeartBit:
                    // "1#" check...
                    mReader.HealthCheckAck();
                    /////Program.AppLog.Write(this, "HealthCheck");
                    break;
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                // BASIC OPERATIONS EVENTS
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                case EventType.Inventory:
                case EventType.HandShackInventory:
                case EventType.GetStoredData:
                case EventType.ReadMemory:
                case EventType.WriteMemory:
                case EventType.Lock:
                case EventType.Kill:
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                // CONFIGURATIONS EVENTS
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                case EventType.Power:
                case EventType.Version:
                case EventType.AccessPwd:
                case EventType.GlobalBand:
                case EventType.Buzzer:
                case EventType.ContinueMode:

                case EventType.Port:
                case EventType.Selection:
                case EventType.Filtering:
                case EventType.Algorithm:
                case EventType.Gpio:
                case EventType.TcpIp:

                case EventType.HealthCheckTime:

                case EventType.HandShake:
                case EventType.ReportTime:
                case EventType.Command:
                case EventType.ReaderMode:

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
                    ///////////////////////////////////////////////////////////////////////////////////////////////////
                    // Tag Memory : [#]T3000111122223333444455556666[##]
                    // Response Code : [#]C##
                    // Settings Values : p0, c1, ...
                    ///////////////////////////////////////////////////////////////////////////////////////////////////
                    char code;
                    string szValue = string.Empty;
                    string tagID;
                    string port;


                    bool bMultiPort = mReader.IsMultiPort();
                    int nPos = bMultiPort ? 1 : 0;//Port Number를 제외한다.

                    //각 응답에 대해서 처리를 한다.
                    foreach (string szResponse in szResponses)
                    {
                        code = szResponse[nPos];
                        switch (code)
                        {
                            case 'T'://Tag Memory
                                        //case 'C'://23.09.07 추가로 처리
                            case 'Q':
                            case 'B':


                                /////Program.AppLog.Write(this, "Event Tag 처리 In 1");
                                port = bMultiPort ? szResponse.Substring(0, 1) : string.Empty;

                                tagID = szResponse.Substring(nPos + 1, szResponse.Length - 2);
                                if (tagID.Length > 30)
                                {
                                    tagID = tagID.Substring(4, 24);
                                }

                                /////Program.AppLog.Write(this, "Event Tag 처리 szResponse : " + szResponse);

                                string szTxt = string.Empty;

                                int iGbnChk = tagID.IndexOf(" ");

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

                                    //lbxResponses.Items.Insert(0, szValue);

                                }


                                //if (tagID.Length > 50)
                                //{
                                //    _sTagID = tagID.Substring(0, iGbnChk);
                                //    szTxt = Reader.MakeTextFromHex(tagID.Substring(iGbnChk + 1)).Replace("\0", "");
                                //    /////Program.AppLog.Write(this, "Event Tag 처리 In 2 : " + _sTagID + "(" + szTxt + ")");
                                //}
                                //else
                                //{
                                //    _sTagID = tagID;//tagID.Substring(0, iGbnChk);
                                //    szTxt = Reader.MakeTextFromHex(tagID.Substring(iGbnChk + 1)).Replace("\0", "");
                                //    /////Program.AppLog.Write(this, "Event Tag 처리 In 2_2 : " + _sTagID + "(" + szTxt + ")");
                                //}

                                //                        if (inventoryType == InventoryType.HandShake)
                                //                        {
                                //                            // handshake ack!
                                //                            //reader.HandShakeAck(tagID);
                                //                            tagID = tagID.Substring(0, tagID.Length - 11);
                                //                        }

                                //                        _sTagID = tagID;


                                //string szTxt = string.Empty;
                                //if (tagID.Length > 4)
                                //{
                                //	//HEX 형태의 TagId 에서 PC값을 제거하고
                                //	string hex = tagID.Substring(4, tagID.Length - 4);
                                //	//TEXT 형태로 디코딩한다.
                                //	szTxt = Reader.MakeTextFromHex(hex);
                                //}

                                //테스트 태그 ID 
                                //if (_sTagID == "3000010101010200020001212515") szTxt = "38도1416";


                                //if (string.IsNullOrEmpty(szTxt) == false)
                                //{
                                //    _sTagNo = szTxt.Trim();

                                //    if (carNumCheck(_sTagNo))
                                //    {
                                //        //차량 번호일 경우 처리

                                //        if (bSensorChk)
                                //        {
                                //            if (report == null)
                                //                report = new TagReport(_sTagID, _sTagNo, 1, 0, "", "");
                                //            else
                                //                report.SetTagReport(_sTagID, _sTagNo, 1, 0, "", "");

                                //            //if (bSensor)
                                //            //{
                                //            //    if (report == null)
                                //            //        report = new TagReport(_sTagID, _sTagNo, 1, 0, "", "");
                                //            //    else
                                //            //        report.SetTagReport(_sTagID, _sTagNo, 1, 0, "", "");
                                //            //}
                                //            //else
                                //            //{
                                //            //    DateTime t1 = _dteSensor;
                                //            //    DateTime t2 = DateTime.Now;
                                //            //    TimeSpan TS = t2 - t1;
                                //            //    double diffDay = TS.TotalSeconds;
                                //            //    if (diffDay < 8)
                                //            //    {
                                //            //        if (report == null)
                                //            //            report = new TagReport(_sTagID, _sTagNo, 1, 0, "", "");
                                //            //        else
                                //            //            report.SetTagReport(_sTagID, _sTagNo, 1, 0, "", "");
                                //            //    }
                                //            //    else
                                //            //    {
                                //            //        Program.AppLog.Write(this, "Event Tag 처리 In 3(인식제외, 센서미인식) : " + "(" + _sTagNo + ")");
                                //            //    }

                                //            //}
                                //        }
                                //        else
                                //        {


                                //            if (report == null)
                                //                report = new TagReport(_sTagID, _sTagNo, 1, 0, "", "");
                                //            else
                                //                report.SetTagReport(_sTagID, _sTagNo, 1, 0, "", "");
                                //        }
                                //    }
                                //    else
                                //    {
                                //        // 차량번호가 아닐 경우 처리할 로직
                                //        /////Program.AppLog.Write(this, "Event Tag 차량번호 아닌 경우 : " + _sTagNo);
                                //    }

                                //}
                                //else
                                //{
                                //    // 빈값일 경우 처리할 로직
                                //    /////Program.AppLog.Write(this, "Event Tag 빈값일 경우 TagID: " + _sTagID);
                                //}

                                break;

                            //case 'C'://Response Code
                            //    szValue = szResponse.Substring(nPos + 1, szResponse.Length - (nPos + 1));//exclude [#]T/C
                            //    string szCode = szValue.Substring(0, 2);
                            //    string szVerify = szValue.Length > 2 ? szValue.Substring(2, szValue.Length - 2) : string.Empty;
                            //    //display result code-descriptions
                            //    szValue = szCode + "-" + mReader.Responses(szCode);

                            //    //debug : display elapsed time of access-operations 
                            //    if (IsWatching())
                            //    {
                            //        szValue = szValue + "-asdf" + WatchElapsed() + "ms";
                            //        WatchReset();
                            //    }
                            //    //debug : display read values if exists : access-write
                            //    if (szVerify.Length > 0)
                            //    {
                            //        szValue = szValue + "-" + szVerify;
                            //    }
                            //    lbxResponses.Items.Insert(0, szValue);

                            //case 'C'://Response Code

                            //    //3C3400BACEBBEA3938BBE732343336 03
                            //    //string Testaa1 = Reader.MakeTextFromHex("BACEBBEA3938BBE732343336");
                            //    //string Testaa2 = Reader.MakeTextEuckrFromHex("BACEBBEA3938BBE732343336"); //정상

                            //    //2C4400EBB680EC82B03939EC82AC3530333720 03
                            //    //string Testaa1 = Reader.MakeTextFromHex("EBB680EC82B03939EC82AC3530333720");//정상
                            //    //string Testaa2 = Reader.MakeTextEuckrFromHex("EBB680EC82B03939EC82AC3530333720");

                            //    /////Program.AppLog.Write(this, "Event Tag 처리 In 1");
                            //    port = bMultiPort ? szResponse.Substring(0, 1) : string.Empty;

                            //    string sChk = szResponse.Substring(0, 2);
                            //    /////Program.AppLog.Write(this, "Event Tag 처리 szResponse(CType) : " + szResponse);

                            //    tagID = szResponse;
                            //    szTxt = string.Empty;

                            //    int iChkLast = tagID.IndexOf(" 03");

                            //    if (iChkLast > 0) iChkLast = iChkLast - 6;
                            //    else iChkLast = tagID.Length - 6;

                            //    if (sChk == "3C")
                            //    {

                            //        szTxt = Reader.MakeTextEuckrFromHex(tagID.Substring(6, iChkLast)).Replace("\0", "");
                            //        /////Program.AppLog.Write(this, "Event Tag 처리 In 2(3CMakeTextEuckrFromHex) : " + tagID + "(" + szTxt + ")");
                            //        if (carNumCheck(szTxt))
                            //        {

                            //        }
                            //        else
                            //        {
                            //            szTxt = Reader.MakeTextFromHex(tagID.Substring(6, iChkLast)).Replace("\0", "");
                            //            /////Program.AppLog.Write(this, "Event Tag 처리 In 2(3CMakeTextFromHex) : " + tagID + "(" + szTxt + ")");
                            //        }

                            //    }
                            //    else if (sChk == "2C")
                            //    {
                            //        szTxt = Reader.MakeTextFromHex(tagID.Substring(6, iChkLast)).Replace("\0", "");
                            //        /////Program.AppLog.Write(this, "Event Tag 처리 In 2(2CMakeTextFromHex) : " + tagID + "(" + szTxt + ")");
                            //        if (carNumCheck(szTxt))
                            //        {

                            //        }
                            //        else
                            //        {
                            //            szTxt = Reader.MakeTextEuckrFromHex(tagID.Substring(6, iChkLast)).Replace("\0", "");
                            //            /////Program.AppLog.Write(this, "Event Tag 처리 In 2(2CMakeTextEuckrFromHex) : " + tagID + "(" + szTxt + ")");
                            //        }


                            //    }

                            //    //                        if (inventoryType == InventoryType.HandShake)
                            //    //                        {
                            //    //                            // handshake ack!
                            //    //                            //reader.HandShakeAck(tagID);
                            //    //                            tagID = tagID.Substring(0, tagID.Length - 11);
                            //    //                        }

                            //    //                        _sTagID = tagID;


                            //    //string szTxt = string.Empty;
                            //    //if (tagID.Length > 4)
                            //    //{
                            //    //	//HEX 형태의 TagId 에서 PC값을 제거하고
                            //    //	string hex = tagID.Substring(4, tagID.Length - 4);
                            //    //	//TEXT 형태로 디코딩한다.
                            //    //	szTxt = Reader.MakeTextFromHex(hex);
                            //    //}

                            //    //테스트 태그 ID 
                            //    //if (_sTagID == "3000010101010200020001212515") szTxt = "38도1416";


                            //    if (string.IsNullOrEmpty(szTxt) == false)
                            //    {
                            //        _sTagNo = szTxt.Trim();

                            //        if (carNumCheck(_sTagNo))
                            //        {
                            //            //차량 번호일 경우 처리

                            //            if (bSensorChk)
                            //            {
                            //                if (report == null)
                            //                    report = new TagReport(_sTagID, _sTagNo, 1, 0, "", "");
                            //                else
                            //                    report.SetTagReport(_sTagID, _sTagNo, 1, 0, "", "");

                            //                //if (bSensor)
                            //                //{
                            //                //    if (report == null)
                            //                //        report = new TagReport(_sTagID, _sTagNo, 1, 0, "", "");
                            //                //    else
                            //                //        report.SetTagReport(_sTagID, _sTagNo, 1, 0, "", "");
                            //                //}
                            //                //else
                            //                //{
                            //                //    DateTime t1 = _dteSensor;
                            //                //    DateTime t2 = DateTime.Now;
                            //                //    TimeSpan TS = t2 - t1;
                            //                //    double diffDay = TS.TotalSeconds;
                            //                //    if (diffDay < 8)
                            //                //    {
                            //                //        if (report == null)
                            //                //            report = new TagReport(_sTagID, _sTagNo, 1, 0, "", "");
                            //                //        else
                            //                //            report.SetTagReport(_sTagID, _sTagNo, 1, 0, "", "");
                            //                //    }
                            //                //    else
                            //                //    {
                            //                //        Program.AppLog.Write(this, "Event Tag 처리 In 3(인식제외, 센서미인식) : " + "(" + _sTagNo + ")");
                            //                //    }

                            //                //}
                            //            }
                            //            else
                            //            {


                            //                if (report == null)
                            //                    report = new TagReport(_sTagID, _sTagNo, 1, 0, "", "");
                            //                else
                            //                    report.SetTagReport(_sTagID, _sTagNo, 1, 0, "", "");
                            //            }
                            //        }
                            //        else
                            //        {
                            //            // 차량번호가 아닐 경우 처리할 로직
                            //            /////Program.AppLog.Write(this, "Event Tag 차량번호 아닌 경우 : " + _sTagNo);
                            //        }




                            //    }
                            //    else
                            //    {
                            //        // 빈값일 경우 처리할 로직
                            //        /////Program.AppLog.Write(this, "Event Tag 빈값일 경우 TagID: " + _sTagID);
                            //    }

                            //    break;

                            case 'A':
                                if (szResponse[1] == '#')
                                    _sActiveMsg = szResponse;
                                break;

                            default:
                                _sActiveMsg = szResponse;
                                break;
                        }
                    }

                    break;

                default:
                    string sPayload = Encoding.ASCII.GetString(e.Payload);
                    _sActiveMsg = sPayload;
                    break;
            }

            if (_sActiveMsg.Length == 3)
            {
                if (_sActiveMsg.Substring(0, 2) == "g0")
                {
                    if (bSensor)
                    {
                        bSensor = false;
                        /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("센서 Off");
                        /////Program.AppLog.Write(this, "센서 Off");
                    }

                }
                else if (_sActiveMsg.Substring(0, 2) == "g1" || _sActiveMsg.Substring(0, 2) == "g2" || _sActiveMsg.Substring(0, 2) == "g3")
                {
                    if (!bSensor)
                    {
                        bSensor = true;
                        _dteSensor = DateTime.Now;
                        /////((RFID_MW)(Application.OpenForms["RFID_MW"])).AddLogList("센서 On");
                        /////Program.AppLog.Write(this, "센서 On");
                    }
                }
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
