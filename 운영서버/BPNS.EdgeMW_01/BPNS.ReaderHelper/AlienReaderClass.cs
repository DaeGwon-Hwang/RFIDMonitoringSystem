using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nsAlienRFID2;
using System.Collections;

namespace BPNS.ReaderHelper
{
    public class AlienReaderClass
    {
        //Reader 연결 정보
        private string _IP = "192.168.14.102";
        private string _ID = "alien";
        private string _PW = "password";
        

        private clsReader mReader;
        private ReaderInfo mReaderInfo;
        private ComInterface meReaderInterface = ComInterface.enumTCPIP;
        private ArrayList malTags;
        private String msTags;

        private bool bConnect = false;

        public string sInout = string.Empty;

        private List<AlienStatistics> mlMyStatistics = null;

        //Tag Data Table
        public DataTable _TagTable;

        public string _EQ_ID
        { get; set; }

        public string _EQ_NM
        { get; set; }

        public string IP
        {
            get { return _IP; }
            set { _IP = value; }
        }

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

        /// <summary>
        /// 비정상 신호 체크 Datetime
        /// </summary>
        private DateTime _dteAbnormalSignal = DateTime.Now.AddDays(-1);

        private DateTime _dteNormalSignal = DateTime.Now.AddDays(-1);

        private DateTime _dteSensor = DateTime.Now.AddDays(-1);

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

        public AlienReaderClass()
        {
            try
            {
                mReader = new clsReader(true);
                mReaderInfo = mReader.ReaderSettings;

            }
            catch (OutOfMemoryException)
            {


            }
            catch (Exception)
            {
            
            }


        }

        /// <summary>
        /// 미들웨어와 리더기 장비와의 연결 상태 확인
        /// </summary>
        /// <returns>bool connection 상태</returns>
        public bool ConnectionCheck()
        {
            return mReader.IsConnected;            
        }

        /// <summary>
        /// 연결 
        /// </summary>
        public void Connect()
        {
            EthernetConnection();
        }


        /// <summary>
        /// IP를 통한 인터넷 연결 시작
        /// </summary>
        /// <returns></returns>
        public bool EthernetConnection()
        {
            try
            {
                string szIpAddress = _IP;
                int nPort = 23;
                mReader.InitOnNetwork(szIpAddress, nPort);
                string result = mReader.Connect();
                if (mReader.IsConnected)
                {
                    if (meReaderInterface == ComInterface.enumTCPIP)
                    {
                        //lblStatus.Text = "Logging in...";
                        //this.Cursor = Cursors.WaitCursor;
                        //if (!mReader.Login("alien", "password"))        //returns result synchronously
                        if (!mReader.Login(_ID, _PW))        //returns result synchronously
                        {
                            //lblStatus.Text = "Login failed! Calling Disconnect()...";
                            mReader.Disconnect();
                            bConnect = false;
                            return false;             //------------>
                        }
                    }

                    //AutoMode = "Off" 처리 : AutoMode = "On" 일경우 경광등이 On 될때 리더기 작동이 멈춘다.
                    if (mReader.AutoMode == "On")
                    {
                        mReader.AutoMode = "Off";
                    }

                    
                   
                }
                else
                {
                    bConnect = false;
                    return false;
                }

                bConnect = true;
                return true;


            }
            catch (OutOfMemoryException)
            {
                bConnect = false;
                throw;
            }
            catch (Exception ex)
            {
                bConnect = false;
                throw;
            }



        }

        private string SetNormalSignal()
        {
            string sRtn = string.Empty;
            sRtn = mReader.ExternalOutput = "1";
            return sRtn;
        }

        private string SetAbnormalSignal()
        {
            string sRtn = string.Empty;
            sRtn = mReader.ExternalOutput = "2";
            return sRtn;
        }

        private string SetIintSignal()
        {
            string sRtn = string.Empty;
            sRtn = mReader.ExternalOutput = "0";
            return sRtn;
        }


        /// <summary>
        /// 에어리언의 경우 연결을 종료함..
        /// </summary>
        /// <returns></returns>
        public bool Endreading()
        {
            try
            {
                if (bConnect)
                {
                    //mReader.ExternalOutput = "0";
                    SetIintSignal();
                }


                bool bBtn = false;
                int iCnt = 0;
                while (!bBtn && iCnt < 5)
                {
                    iCnt++;
                    string ValueofDisConnect = mReader.Disconnect();
                    if (ValueofDisConnect == null || ValueofDisConnect == string.Empty) bBtn = true;

                }

                mReader.Dispose();

                return bBtn;
            }
            catch (OutOfMemoryException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void CheckSignal()
        {
            //비정상 신호가 잡힌 경우 체크
            DateTime t1 = _dteAbnormalSignal;
            DateTime t2 = DateTime.Now;
            TimeSpan TS = t2 - t1;

            double diffSec = TS.TotalSeconds;
            if (diffSec < _AbnormalSignalSec)
            {
                //mReader.ExternalOutput = "2";
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
                    //mReader.ExternalOutput = "1";
                    SetNormalSignal();
                }
                else
                {
                    //mReader.ExternalOutput = "0";
                    SetIintSignal();
                    
                }
            }
        }

        /// <summary>
        /// TAG 데이터 가져오기
        /// </summary>
        public void ReaderTagDataGet(bool bSetDatetime = false)
        {
            string result = null;
            try
            {
                if (mReader.IsConnected)
                {
                    //시간 셋팅 함수
                    //mReader.DateTime 

                    if (bSetDatetime)
                    {
                        string sDatetime = DateTime.Now.AddHours(-9).ToString("yyyy/MM/dd HH:mm:ss").Replace('-', '/');
                        mReader.DateTime = sDatetime;
                    }

                    //센서 체크
                    sInout = mReader.ExternalInput;

                    if (sInout == "1" || sInout == "2" || sInout == "3")
                    {
                        //1:in , 2:out, 3:동시, 0:없음
                        _dteSensor = DateTime.Now;
                    }

                    mReader.TagListFormat = "Text";
                    result = mReader.TagList;
                    mReader.ClearTagList();

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

                }
                else
                {

                    string ValueofDisConnect = mReader.Disconnect();
                    if (ValueofDisConnect == null || ValueofDisConnect == string.Empty)
                        SetLog(GetType().ToString(), "ReaderTagDataGet", string.Format("{0}-{1}", _EQ_ID, "연결종료"), "연결상태 이상으로 인한 연결종료");
                    else if (ValueofDisConnect == "Not connected")
                        SetLog(GetType().ToString(), "ReaderTagDataGet", string.Format("{0}-{1}", _EQ_ID, "연결이상"), "연결상태 이상 발생 연결 찾을 수 없음");
                    else if (ValueofDisConnect.Length > 13 && ValueofDisConnect.Substring(0, 13) == "Alien caught")
                        SetLog(GetType().ToString(), "ReaderTagDataGet", string.Format("{0}-{1}", _EQ_ID, "리더기 에러"), "");

                    return;
                }
            }
            catch (TimeoutException te)
            {
                SetLog(GetType().ToString(), "ReaderTagDataGet", string.Format("{0}-{1}", _EQ_ID, "TimeoutException"), te.ToString());

                //throw te;
            }
            catch (Exception ex)
            {
                SetLog(GetType().ToString(), "ReaderTagDataGet", string.Format("{0}-{1}", _EQ_ID, "Exception"), ex.ToString());

                //throw ex;
            }

            

            //Alien Reader의 Tag 목록
            TagInfo[] aTags;
            int cnt = 0;

            if ((result != null) &&
                (result.Length > 0) &&
                (result.IndexOf("No Tags") == -1))
            {
                msTags = result;

                cnt = AlienUtils.ParseTagList(result, out aTags);
            }
            else
            {
                CheckSignal();
                return;
            }

            try
            {
                for (int i = 0; i < cnt; i++)
                {
                    if (aTags[i].TagID.Replace(" ", "").IndexOf(_sFilter) < 0)
                    {
                        string sExTagUID = "필터제외";
                        continue;
                    }

                    ///리더기의 시간이 표준시에 맞춰있어 해당 시간을 현지 시간으로 변경이 필요... 
                    ///현재는 9시간을 더해서 사용...
                    //InsertTagData(aTags[i].TagID, aTags[i].Antenna, aTags[i].ReadCount, aTags[i].RSSI, Convert.ToDateTime(aTags[i].LastSeenTime).AddHours(9).ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    var time = aTags[i].LastSeenTime;

                    DateTime aaa;
                    //Debug.WriteLine(string.Format("Time Test : {0}", time));
                    if (DateTime.TryParseExact(time, "yyyy-MM-dd HH:mm:ss.fff", null, System.Globalization.DateTimeStyles.None, out aaa) == false)
                        if (DateTime.TryParseExact(time, "yyyy/MM/dd HH:mm:ss.fff", null, System.Globalization.DateTimeStyles.None, out aaa) == false)
                            if (DateTime.TryParseExact(time, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out aaa) == false)
                                DateTime.TryParseExact(time, "yyyy/MM/dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out aaa);

                    if (aaa != null)
                    {
                        
                        InsertTagData(aTags[i].TagID.Replace(" ", ""), aTags[i].Antenna, aTags[i].ReadCount, aTags[i].RSSI, Convert.ToDateTime(aTags[i].LastSeenTime).AddHours(9).ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        //InsertTagData(aTags[i].TagID.Replace(" ", ""), aTags[i].Antenna, aTags[i].ReadCount, aTags[i].RSSI, Convert.ToDateTime(aTags[i].LastSeenTime).ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        
                    }
                    else
                    {

                    }
                }

            }
            catch (Exception ex)
            {
                SetLog(GetType().ToString(), "ReaderTagDataGet", string.Format("{0}-{1}", _EQ_ID, "Exception"), ex.ToString());

                //throw ex;
            }


        }

        

        public void InsertTagData(string strTagUID, int nTagReadAnt, int nTagReadCount, double dTagRSSI, string strTagReadTime)
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
                htParamsHashtable.Add("IN_TAG_ID", strTagUID);
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
                        //mReader.ExternalOutput = "2";
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
                                //mReader.ExternalOutput = "1";
                                SetNormalSignal();
                            }
                            else
                            {
                                //mReader.ExternalOutput = "2";
                                SetAbnormalSignal();
                            }



                        }
                        else
                        {
                            _dteAbnormalSignal = DateTime.Now;
                            _bAbnormalSignal = true;
                            //mReader.ExternalOutput = "2";
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

            //string strTagReadCount;
            //string strTagReadAnt;
            //string strTagRSSI;

            //BIZ.BizMWwithService _biz = new BIZ.BizMWwithService();

            //int nResult = 1;

            //DataRow[] drDel = _TagTable.Select("[Time] < '" + DateTime.Now.AddSeconds(-3).ToString("yyyy/MM/dd HH:mm:ss.fff") + "'");

            //for (int i = 0; i < drDel.Length; i++)
            //{
            //    _TagTable.Rows.Remove(drDel[i]);
            //}


            //DataRow[] dr = _TagTable.Select("[ReaderID] = '" + _MWID + "' and [TagUID] = '" + strTagUID + "' and [Ant] = '" + nTagReadAnt.ToString() + "'");

            ////DataRow[] dr = _TagTable.Select("[ReaderID] = '" + _MWID + "' and [TagUID] = '" + strTagUID + "'");

            //nResult = dr.Length;


            //strTagReadCount = nTagReadCount.ToString();
            //strTagReadAnt = nTagReadAnt.ToString();
            //strTagRSSI = string.Format("{0:F1}", dTagRSSI) + "dB";


            //if (nResult == 0)
            //{
            //    InsertTagList(dr, strTagUID, strTagReadAnt, strTagReadCount, strTagRSSI, strTagReadTime);
            //}
            //else if (nResult < 0)
            //{ }
            //else
            //{
            //    //int nCurTagCount = int.Parse(_TagTable.Rows[nMidKey][3].ToString());
            //    int nCurTagCount = int.Parse(dr[0][3].ToString());
            //    nCurTagCount += nTagReadCount;

            //    SetTagData(dr, strTagReadAnt, nCurTagCount.ToString(), strTagRSSI, strTagReadTime);
            //}
        }


        public void RemoveTagUID()
        {
            foreach (DataRow lvi1 in _TagTable.Rows)
            {
                _TagTable.Rows.Remove(lvi1);
            }
        }

        private string GetTagNo(string response)
        {
            try
            {
                string Tag_No = string.Empty;
                response = response.Replace(",", "").Replace("\r\n", "");
                Tag_No = ByteToString(StringToByteArray(response));
                return Tag_No;
            }
            catch (Exception)
            {
                return "";
            }
        }

        private string ByteToString(byte[] strByte)
        {
            string str = Encoding.Default.GetString(strByte);
            return str;
        }

        static private byte[] StringToByteArray(string strHex)
        {
            return System.Linq.Enumerable.Range(0, strHex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(strHex.Substring(x, 2), 16)).ToArray();
        }

        #region Log
        /// <summary>
        /// 로그 수집
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventLog"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public void SetLog(string sender, string eventLog, string title, string message)
        {
            //BizMWwithService _biz = new BizMWwithService();
            //_biz.SetLogData(DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss"), sender, eventLog, title, message);
        }

        /// <summary>
        /// 로그 수집
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventLog"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public void SetLog(string date, string sender, string eventLog, string title, string message)
        {
            //BizMWwithService _biz = new BizMWwithService();
            //_biz.SetLogData(date, sender, eventLog, title, message);
        }
        #endregion
    }
}
