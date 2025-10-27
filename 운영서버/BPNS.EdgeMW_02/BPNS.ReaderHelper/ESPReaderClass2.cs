using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.ReaderHelper
{
    public class ESPReaderClass2
    {
        //Reader 연결 정보
        private string _IP = "192.168.14.102";
        private string _ID = "ubists";
        private string _PW = "0000";
        private int _port = 1500;

        //private string _ID = "cuint";
        //private string _PW = "0000";
        //private int _port = 50511;


        
        
        public bool m_connection = false;

        

        private String msTags;

        ESPRFID reader = new ESPRFID();

        //Tag Data Table
        public DataTable _TagTable;

        int m_hHandle;
        byte m_nConnMethod = 0;                     // 0: Serial        1: Ethernet
        int m_nConnCheck = 1;                       // 0: Version       1: Ping
        int m_nUniqueTagCount = 0;
        
        bool m_fTestMode = false;
        int m_flTimerCount = 0;
        int m_nTestTime = 0;
        int m_nTestSecond = 0;

        private volatile IDatabaseManagement DBManagement;

        internal IDatabaseManagement dbManagement
        {
            get { return DBManagement; }
            set { DBManagement = value; }
        }

        enum ButtonType
        {
            CONNECT,
            BEGINREAD
        }

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

        public bool bSensor = false;

        private string _sFilter = "0544092CB48E04B430";

        public ESPReaderClass2()
        {
            try
            {

                DBManagement = new DatabaseManagementOracle();

                if (!DBManagement.Connect())
                { }
                

                DataSet dsTest = DBManagement.ExecuteSelectQuery("SELECT TO_CHAR(SYSTIMESTAMP, 'YYYYMMDDHH24MISSFF3') FROM DUAL", 1);
            }
            catch (OutOfMemoryException)
            {


            }
            catch (Exception) { }


        }

        /// <summary>
        /// 미들웨어와 리더기 장비와의 연결 상태 확인
        /// </summary>
        /// <returns>bool connection 상태</returns>
        public bool ConnectionCheck()
        {
            bool bConnect = true;

            if (m_nConnCheck == 0)
            {
                try
                {
                    //string readerVersion = RFIDLib.RFIDGetReaderVersion(m_hHandle);
                    //if (readerVersion.Equals(""))
                    bConnect = false;
                }
                catch (System.Net.NetworkInformation.PingException) { }
                catch (OutOfMemoryException) { }
                catch (Exception) { }

            }
            else
            {
                try
                {
                    if (!reader.CheckConnection())
                    {
                        if (reader.Connect(_IP, _port))
                        {
                            bConnect = true;
                        }
                        else
                        {
                            bConnect = false;
                        }
                    }
                }
                catch (System.Net.NetworkInformation.PingException) { }
                catch (OutOfMemoryException) { }
                catch (Exception) { }

            }
            m_connection = bConnect;
            return bConnect;
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
                int nPort = _port;
                
                bool result = reader.Connect(_IP, _port);
                m_connection = result;
                if (!result)
                {
                    return false;
                }

                return true;

            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception ex)
            {

                throw;
            }



        }


        /// <summary>
        /// 에어리언의 경우 endreading 할 필요없음
        /// </summary>
        /// <returns></returns>
        public bool Endreading()
        {
            try
            {
                
                reader.FinishRead();
                return true;
            }
            catch (OutOfMemoryException)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void SetDefaultSignal()
        {
            try
            {
                reader.SetDefaultSignal();
            }
            catch (Exception ex)
            {
                throw ex;
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
                reader.SetExceptionSignal();
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
                    reader.SetNormalSignal();
                }
                else
                {
                    //mReader.ExternalOutput = "0";
                    //reader.SetDefaultSignal(); //23.12.07
                }
            }
        }

        public void ReaderTagDataGet()
        {
            int iDurSec = 10;
            try
            {

                //List<TagDataRaw> aTags = new List<TagDataRaw>();

                //센서 체크
                reader.GetReadExtInputValue();
                bSensor = reader.bSensor;

                List<TagDataRaw> aTags = reader.GetTagList();

                int cnt = aTags.Count;

                //센서 사용일 경우 처리
                if (_USE_SENSOR == "Y")
                {
                    //비정상 신호가 잡힌 경우 체크
                    DateTime tSensor = reader._dteSensor;
                    DateTime tSensorNow = DateTime.Now;
                    TimeSpan TsSensor = tSensorNow - tSensor;

                    double SensorDiffSec = TsSensor.TotalSeconds;
                    if (SensorDiffSec > _SensorTiemSec)
                    {
                        CheckSignal();
                        return;
                    }
                }

                if (cnt == 0)
                {
                    CheckSignal();
                    return;
                }

                //int cnt = reader.GetTagList(out aTags);

                for (int i = 0; i < cnt; i++)
                {
                    try
                    {
                        if (aTags[i].TagID.Substring(4, 24).IndexOf(_sFilter) < 0)
                        {
                            string sExTagUID = "필터제외";
                            continue;
                        }

                        DateTime dt = CDateTime(aTags[i].Date, aTags[i].Time, aTags[i].MilliSecond);
                        ///리더기의 시간이 표준시에 맞춰있어 해당 시간을 현지 시간으로 변경이 필요... 
                        ///현재는 9시간을 더해서 사용...
                        ///

                        //센서 감지 시간 체크
                        DateTime t1 = reader._dteSensor;//_dteSensor; //reader.DteSensor;
                        DateTime t2 = DateTime.Now;
                        TimeSpan TS = t2 - t1;

                        double diffSec = TS.TotalSeconds;
                        if (0 < diffSec && diffSec < iDurSec)
                        {
                            InsertTagData(aTags[i].TagID.Substring(4, 24), aTags[i].LastReadAnt, aTags[i].AntCnt[aTags[i].LastReadAnt - 1], 0, dt.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        }

                        
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    //reader.SetExceptionSignal();
                    //reader.GetReadExtOutputValue();




                }

                //if (cnt == 0)
                //{
                //    reader.SetDefaultSignal();
                //    reader.GetReadExtOutputValue();
                //}

                //reader.SetOn2Value("");
                //reader.GetReadExtOutputValue();
                //reader.GetReadExtInputValue();
            }
            catch (Exception ex)
            {
                return;
            }

        }

        private DateTime CDateTime(string day, string Time, int miss)
        {
            try
            {
                string ReturnValue = "";

                ReturnValue += day.Substring(0, 4) + "-" + day.Substring(4, 2) + "-" + day.Substring(6, 2) + " ";

                ReturnValue += Time.Substring(0, 2) + ":" + Time.Substring(2, 2) + ":" + Time.Substring(4, 2);

                ReturnValue += "." + miss.ToString();



                return Convert.ToDateTime(ReturnValue);
            }
            catch(NullReferenceException e)
            {
                throw e;
            }
            catch (Exception ex)
            {

                throw ex;
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

                    if (_GATE_TYPE == "CHK")
                    {
                        _dteAbnormalSignal = DateTime.Now;
                        _bAbnormalSignal = true;
                        //mReader.ExternalOutput = "2";
                        reader.SetExceptionSignal();
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
                                reader.SetNormalSignal();
                            }
                            else
                            {
                                //mReader.ExternalOutput = "2";
                                reader.SetExceptionSignal();
                            }
                        }
                        else
                        {
                            _dteAbnormalSignal = DateTime.Now;
                            _bAbnormalSignal = true;
                            //mReader.ExternalOutput = "2";
                            reader.SetExceptionSignal();
                        }
                    }

                }
                else
                {
                    //mReader.ExternalOutput = "0";
                    //reader.SetDefaultSignal(); //12.12.07
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

        

        

        private string ByteToString(byte[] strByte)
        {
            string strRtn = Encoding.Default.GetString(strByte);
            return strRtn;
        }

        private byte[] StringToByteArray(string strHex)
        {
            return System.Linq.Enumerable.Range(0, strHex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(strHex.Substring(x, 2), 16)).ToArray();
        }

        public void RemoveTagUID()
        {
            foreach (DataRow lvi1 in _TagTable.Rows)
            {
                _TagTable.Rows.Remove(lvi1);
            }
        }
    }
}
