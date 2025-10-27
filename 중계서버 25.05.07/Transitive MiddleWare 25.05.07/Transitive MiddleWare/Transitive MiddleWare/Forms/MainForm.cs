
namespace BPNS.TransitiveMiddleware
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Windows.Forms;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Reflection;
    using System.Collections;
    using System.Runtime.InteropServices;
    using Comm;
    using System.Text;
    using Newtonsoft.Json.Linq;

    public partial class MainForm : Form
    {
        private Socket listener;
        private Socket Mwlistener;
        private Socket Pamslistener;
        private List<ClientSocketHandler> clientSockets;
        private List<MwClientSocketHandler> MwclientSockets;
        private List<PamsClientSocketHandler> PamsclientSockets;
        private volatile IDatabaseManagement DBManagement;

        private bool bSystemClosing = false;
        private Mutex runCheckMutex;
        private bool bMutexRelease = false;
        private object logListCO = new object();
        private string strLastLogClearanceDate = null;
        private volatile bool bStopBatchThreadLoop = false;
        private volatile bool bStopBatteryStateCheckThreadLoop = false;

        private Thread BatchDataProcessThread = null;
        private Thread BatchSumThread = null;
        private Thread BatteryStateCheckThread = null;

        private Thread EquipStatusThread = null;
        private Thread PamsAliveCheckThread = null;
        private Thread ConnectionStatusSendToEMSThread = null;

        private Thread PrintAliveCheckThread = null;

        /// <summary>
        /// 게이트(리더) 상태 전송 쓰레드
        /// </summary>
        private Thread GateSttThread = null;

        /// <summary>
        /// 서버 접속 상태 전송 쓰레드
        /// </summary>
        private Thread ConectSttThread = null;


        private Thread PrintThread = null;

        private Thread ReadTagSendThread = null;

        /// <summary>
        /// 반입/반출 목록 리스트 처리 쓰레드
        /// </summary>
        private Thread InOutDataThread = null;

        /// <summary>
        /// 반입 목록 리스트
        /// </summary>
        private JObject joInList;

        public JObject JoInList { get => joInList; set => joInList = value; }

        /// <summary>
        /// 반출 목록 리스트
        /// </summary>
        private JObject joOutList;

        public JObject JoOutList { get => joOutList; set => joOutList = value; }

        /// <summary>
        /// 재발행 리스트
        /// </summary>
        public JArray Ja001REQ = new JArray();

        /// <summary>
        /// 재발행 tagNo List
        /// </summary>
        public List<string> lstReIssueTagNoList = new List<string>();

        ManualResetEvent _pauseEvent = new ManualResetEvent(false);

        private List<EQErrorState> lEQErrorState;
        public List<EQErrorState> EQErrorStateList
        {
            get { return lEQErrorState; }
            set { lEQErrorState = value; }
        }

        private object oEQErrorStateCO = new object();
        public object EQErrorStateCO
        {
            get { return oEQErrorStateCO; }
            set { oEQErrorStateCO = value; }
        }

        internal IDatabaseManagement dbManagement
        {
            get { return DBManagement; }
            set { DBManagement = value; }
        }

        



        /// <summary>
        /// 게이트 장비
        /// </summary>
        Dictionary<string, Dictionary<string, Dictionary<string, object>>> gateEquipStatus = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();

        /// <summary>
        /// 상태
        /// </summary>
        public Dictionary<string, object> htConnectionStatus = new Dictionary<string, object>();

        public string PAMS = "PAMS";
        public string RFID_MW = "RFID_MW";
        public string EMS = "EMS";


        public DataTable _dtENVInfo;
        public DataTable _dtCrInfo;

        /// <summary>
        /// 환경정보 업데이트 일자
        /// </summary>
        private string _sENVCheckDate = string.Empty;
        private string _sCRCheckDate = string.Empty;

        public MainForm()
        {
            //string DBName = BPNS.TransitiveMiddleware.Properties.Settings.Default.DBName;
            InitializeComponent();
            clientSockets = new List<ClientSocketHandler>();
            MwclientSockets = new List<MwClientSocketHandler>();
            PamsclientSockets = new List<PamsClientSocketHandler>();
            //lBatchJobRunInfo = new List<BatchJobRunInfo>();
        }

        public static string ConvertDictionaryToString<DKey, DValue>(Dictionary<DKey, DValue> dict)
        {
            string format = "{0}='{1}',";

            StringBuilder itemString = new StringBuilder();
            foreach (KeyValuePair<DKey, DValue> kv in dict)
            {
                itemString.AppendFormat(format, kv.Key, kv.Value);
            }
            itemString.Remove(itemString.Length - 1, 1);

            return itemString.ToString();
        }

        private void SetInit()
        {
            //Constants.SERVICE_STATE = ServiceState.RUNNING;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {


            //string sTestMsg = "{Java=1995, C++=1980, Ruby=1991}";


            //var test11 = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(sTestMsg);

            //Dictionary<string, int> dict = new Dictionary<string, int>() {
            //{"A", 1}, {"B", 2}, {"C", 3}, {"D", 4}, {"E", 5}
            //};

            //string sTest111 = Newtonsoft.Json.JsonConvert.SerializeObject(dict, Newtonsoft.Json.Formatting.Indented);

            //Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(sTest111);

            //JObject JoTest = new JObject();
            //JoTest.Add("WORKDIV", "TEST1");
            //JoTest.Add("BINDINFO", JObject.FromObject(dict));

            //foreach (JProperty x in JoTest["BINDINFO"])
            //{ // Where 'obj' and 'obj["otherObject"]' are both JObjects
            //    string name = x.Name;
            //    JToken value = x.Value;
            //    JoTest.Add(name, value);
            //}





            //string sTmp = dict.ConvertDictionaryToString();


            //string dictString = ConvertDictionaryToString<string, int>(dict);




            IPAddress hostIP;
            IPEndPoint ep;

            IPAddress PamshostIP;
            IPEndPoint Pamsep;



            bool bNew;
            Assembly assembly;
            GuidAttribute attribute;
            bool bClose = false;

            try
            {
                if (Properties.Settings.Default.APP_bPreventDoubleRun)
                {
                    assembly = Assembly.GetExecutingAssembly();
                    attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
                    runCheckMutex = new Mutex(true, attribute.Value, out bNew);
                    bMutexRelease = true;
                    if (!bNew)
                    {
                        MessageBox.Show("프로그램이 이미 실행중입니다. 확인 후 다시 실행하시기 바랍니다.", "프로그램 중복 실행", MessageBoxButtons.OK);
                        bSystemClosing = true;
                        bMutexRelease = false;
                        bClose = true;
                    }
                }

                //SetInit();
                Constants.SERVICE_STATE = ServiceState.RUNNING;

                lEQErrorState = new List<EQErrorState>();

                ////PAMS 연동
                PamshostIP = IPAddress.Any;
                Pamsep = new IPEndPoint(PamshostIP, Properties.Settings.Default.FROM_PAMS_PORT);
                Pamslistener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Pamslistener.Bind(Pamsep);
                Pamslistener.Listen(Properties.Settings.Default.NET_ListeningSocketCount);
                Pamslistener.BeginAccept(new AsyncCallback(PamsAcceptCallback), Pamslistener);

                ////MW 연동
                //hostIP = IPAddress.Any;
                //ep = new IPEndPoint(hostIP, Properties.Settings.Default.FROM_MW_PORT);
                //Mwlistener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //Mwlistener.Bind(ep);
                //Mwlistener.Listen(Properties.Settings.Default.NET_ListeningSocketCount);
                //Mwlistener.BeginAccept(new AsyncCallback(MwAcceptCallback), Mwlistener);

                //JSON 연동
                hostIP = IPAddress.Any;
                ep = new IPEndPoint(hostIP, Properties.Settings.Default.FROM_JSON_PORT);
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(ep);
                listener.Listen(Properties.Settings.Default.NET_ListeningSocketCount);
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                ////RFID 연동
                //hostIP = IPAddress.Any;
                //ep = new IPEndPoint(hostIP, Properties.Settings.Default.FROM_RFID_PORT);
                //listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //listener.Bind(ep);
                //listener.Listen(Properties.Settings.Default.NET_ListeningSocketCount);
                //listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);


                if (Properties.Settings.Default.DB_Type.Equals("ORACLE"))
                {
                    DBManagement = new DatabaseManagementOracle();
                }
                else if (Properties.Settings.Default.DB_Type.Equals("SQLSERVER"))
                {
                    DBManagement = new DatabaseManagementSQLServer();
                }
                if (!DBManagement.Connect())
                {
                    Program.AppLog.Write(this, "DB Connection Error!");
                    //MessageBox.Show("DB 연결 오류가 발생하였습니다. 시스템 로그를 확인하여 DB연결설정을 변경하시기 바랍니다!", "Database 연결오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    AddLogList("DB 연결 오류가 발생하였습니다. 시스템 로그를 확인하여 DB연결설정을 변경하시기 바랍니다!");
                }

                DataSet dsTest = DBManagement.ExecuteSelectQuery("SELECT TO_CHAR(SYSTIMESTAMP, 'YYYYMMDDHH24MISSFF3') FROM DUAL", 1);


                ////Test 24.08.05'
                //Dictionary<string, object> paramTest = new Dictionary<string, object>();
                //paramTest.Add("PAMS_MGMT_NO", "PED0000010");
                //paramTest.Add("TAG_ID", "0544092CB48E04B430D91143");
                //paramTest.Add("GATE_ID", "RFF2G05");
                //paramTest.Add("ZONE_ID", "ZN000005");
                //paramTest.Add("IN_OUT_FL", "OUT");
                //paramTest.Add("NOR_ILG_FL", "ILG");


                //C1400REQ(paramTest);

                //string sLane = "01111";
                //string sNo = "38도1416";
                //string sQty = "INSERT INTO RFID_RECV_LOG(GTR_RECV_DT, GTR_LANE, GTR_TRUCKNO) VALUES(SYSDATE, '" + sLane + "', '" + sNo + "') ";
                //int iChk = DBManagement.ExecuteNonQuery(sQty, 3);

                //EnvInfo();
                //if (CrProcChk == false) CrInfoAll(true);

                //EquipStatusThread = new Thread(new ThreadStart(EquipStatus));
                //EquipStatusThread.IsBackground = true;
                //EquipStatusThread.Start();

                //PamsAliveCheckThread = new Thread(new ThreadStart(PamsAliveCheck));
                //PamsAliveCheckThread.IsBackground = true;
                //PamsAliveCheckThread.Start();

                //ConnectionStatusSendToEMSThread = new Thread(new ThreadStart(ConnectionStatusSendToEMS));
                //ConnectionStatusSendToEMSThread.IsBackground = true;
                //ConnectionStatusSendToEMSThread.Start();


                //PrintAliveCheckThread = new Thread(new ThreadStart(PrintAliveCheck));
                //PrintAliveCheckThread.IsBackground = true;
                //PrintAliveCheckThread.Start();

                //프린터 처리 쓰레드 시작
                PrintThread = new Thread(new ThreadStart(Print));
                PrintThread.IsBackground = true;
                PrintThread.Start();

                //Reading Tag 데이터 처리 쓰레드 시작
                ReadTagSendThread = new Thread(new ThreadStart(ReadTagSend));
                ReadTagSendThread.IsBackground = true;
                ReadTagSendThread.Start();

                //반입/반출 목록 리스트 조회 쓰레드 시작
                InOutDataThread = new Thread(new ThreadStart(GetInoutList));
                InOutDataThread.IsBackground = true;
                InOutDataThread.Start();

                //서버 상태 전송 쓰레드
                ConectSttThread = new Thread(new ThreadStart(ConnectionStatus));
                ConectSttThread.IsBackground = true;
                ConectSttThread.Start();

                //게이트 상태 전송 쓰레드 GateSttThread
                GateSttThread = new Thread(new ThreadStart(GateStt));
                GateSttThread.IsBackground = true;
                GateSttThread.Start();


                _pauseEvent.Set();

                this.CommDataTimer.Tick += new System.EventHandler(this.CommDataTimer_Tick);
                CommDataTimer.Enabled = true;

                CrAllTimer.Tick += new System.EventHandler(CrAllTimer_Tick);
                CrAllTimer.Enabled = true;

                mwStateCheckTimer.Enabled = true;
                ScheduleCheckTimer.Enabled = true;

                mwListView.Columns[0].Width = 0;
                mwListView.Columns[1].Width = 90;
                mwListView.Columns[2].Width = 90;
                mwListView.Columns[3].Width = 90;
                mwListView.Columns[4].Width = 134;
                //mwListView.Columns[4].Width = 60;
                mwListView.Columns[5].Width = 134;
                //BatchDataProcessThread = new Thread(new ParameterizedThreadStart(BatchWorkThread));
                //BatchDataProcessThread.Start("D");
                //BatchSumThread = new Thread(new ParameterizedThreadStart(BatchWorkThread));
                //BatchSumThread.Start("S");
                //BatteryStateCheckThread = new Thread(BatteryStateCheckWorkThread);
                //BatteryStateCheckThread.Start();
                Program.AppLog.Write("프로그램 시작");
            }
            catch (SocketException se)
            {
                Program.AppLog.Write(this, "Socket Error![" + se.Message + "]");
                Program.SysLog.Write(this, "Socket Error!", se);
                if (se.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    MessageBox.Show("통신 포트가 이미 사용중입니다. 설정을 변경한 후 다시 시작하십시요.", "프로그램 초기화 오류!", MessageBoxButtons.OK);
                    bSystemClosing = true;
                }
                else
                {
                    MessageBox.Show("통신 초기화 오류로 인하여 프로그램을 종료합니다. 시스템 로그를 확인하여 오류 상황을 해결한 후 재시작 하십시요.", "프로그램 초기화 오류!", MessageBoxButtons.OK);
                    bSystemClosing = true;
                }
                bClose = true;
            }
            catch (Exception ex)
            {
                Program.AppLog.Write(this, "Unexpected Exception![" + ex.Message + "]");
                Program.SysLog.Write(this, "Unexpected Exception!", ex);
                MessageBox.Show("프로그램 초기화 오류(Exception)로 인하여 프로그램을 종료 합니다.", "프로그램 초기화 오류!", MessageBoxButtons.OK);
            }
            finally
            {
                if (bClose)
                {
                    Close();
                    Program.AppLog.Write("프로그램 종료[시작 오류로 인한 종료]");
                }
            }
        }



        private void MainForm_Closing(object sender, FormClosingEventArgs e)
        {
            PasswordCheck passwordCheckForm;
            DialogResult passwordCheckResult;

            try
            {
                passwordCheckForm = new PasswordCheck();
                e.Cancel = true;

                if (bSystemClosing == false)
                {
                    passwordCheckResult = passwordCheckForm.ShowDialog(this);
                    if (passwordCheckResult == System.Windows.Forms.DialogResult.OK)
                    {
                        if (passwordCheckForm.getPasswordString().Equals(Properties.Settings.Default.APP_Exit_Password))
                        {
                            e.Cancel = false;
                            bStopBatchThreadLoop = true;
                            //if (!BatchDataProcessThread.Join(3000))
                            //{
                            //    Program.AppLog.Write("BatchWorkThread[D] 종료 오류!");
                            //    BatchDataProcessThread.Abort();
                            //}
                            //if (!BatchSumThread.Join(3000))
                            //{
                            //    Program.AppLog.Write("BatchWorkThread[S] 종료 오류!");
                            //    BatchSumThread.Abort();
                            //}
                            bStopBatteryStateCheckThreadLoop = true;
                            //if (!BatteryStateCheckThread.Join(3000))
                            //{
                            //    Program.AppLog.Write("BatteryStateCheckThread 종료 오류!");
                            //    BatteryStateCheckThread.Abort();
                            //}
                            Program.AppLog.Write("프로그램 종료");
                        }
                        else
                        {
                            MessageBox.Show(this, "잘못된 비밀번호를 입력하셨습니다. 프로그램이 종료되지 않습니다.", "비밀번호 입력 오류", MessageBoxButtons.OK);
                        }
                    }
                }
                else
                {
                    e.Cancel = false;
                    bStopBatchThreadLoop = true;
                    //if (!BatchDataProcessThread.Join(3000))
                    //{
                    //    Program.AppLog.Write("BatchWorkThread[D] 종료 오류!");
                    //    BatchDataProcessThread.Abort();
                    //}
                    //if (!BatchSumThread.Join(3000))
                    //{
                    //    Program.AppLog.Write("BatchWorkThread[S] 종료 오류!");
                    //    BatchSumThread.Abort();
                    //}
                    bStopBatteryStateCheckThreadLoop = true;
                    //if (!BatteryStateCheckThread.Join(3000))
                    //{
                    //    Program.AppLog.Write("BatteryStateCheckThread 종료 오류!");
                    //    BatteryStateCheckThread.Abort();
                    //}
                    Program.AppLog.Write("프로그램 종료");
                }
                if (e.Cancel == false && bMutexRelease && Properties.Settings.Default.APP_bPreventDoubleRun)
                {
                    runCheckMutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                Program.SysLog.Write(this, "MainForm_Closing 함수 오류", ex);
            }
        }



        public void AcceptCallback(IAsyncResult ar)
        {
            Socket currListener = (Socket)ar.AsyncState;
            //Socket clientSocket = currListener.EndAccept(ar);
            ClientSocketHandler clientSocketHandler;
            try
            {
                clientSocketHandler = new ClientSocketHandler(currListener.EndAccept(ar));
                clientSocketHandler.setAsyncReceive();
                clientSocketHandler.SetLastCommTime();
                clientSockets.Add(clientSocketHandler);
                currListener.BeginAccept(new AsyncCallback(AcceptCallback), currListener);
                Program.AppLog.Write("소켓 연결(" + clientSocketHandler.ClientSocket.Handle.ToInt64().ToString() + ")");
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "AcceptCallback 함수 오류", e);

                if (listener != null) listener.Dispose();

                IPAddress hostIP;
                IPEndPoint ep;

                hostIP = IPAddress.Any;
                ep = new IPEndPoint(hostIP, Properties.Settings.Default.FROM_JSON_PORT);
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(ep);
                listener.Listen(Properties.Settings.Default.NET_ListeningSocketCount);
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            }
        }

        public void MwAcceptCallback(IAsyncResult ar)
        {
            Socket currListener = (Socket)ar.AsyncState;
            //Socket clientSocket = currListener.EndAccept(ar);
            MwClientSocketHandler MwclientSocketHandler;
            try
            {
                MwclientSocketHandler = new MwClientSocketHandler(currListener.EndAccept(ar));
                MwclientSocketHandler.setAsyncReceive();
                MwclientSocketHandler.SetLastCommTime();
                MwclientSockets.Add(MwclientSocketHandler);
                currListener.BeginAccept(new AsyncCallback(MwAcceptCallback), currListener);
                Program.AppLog.Write("소켓 연결(" + MwclientSocketHandler.ClientSocket.Handle.ToInt64().ToString() + ")");
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "MwAcceptCallback 함수 오류", e);

                if (Mwlistener != null) Mwlistener.Dispose();

                IPAddress hostIP;
                IPEndPoint ep;

                hostIP = IPAddress.Any;
                ep = new IPEndPoint(hostIP, Properties.Settings.Default.FROM_MW_PORT);
                Mwlistener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Mwlistener.Bind(ep);
                Mwlistener.Listen(Properties.Settings.Default.NET_ListeningSocketCount);
                Mwlistener.BeginAccept(new AsyncCallback(MwAcceptCallback), Mwlistener);
            }
        }

        public void PamsAcceptCallback(IAsyncResult ar)
        {
            Socket currListener = (Socket)ar.AsyncState;
            //Socket clientSocket = currListener.EndAccept(ar);
            PamsClientSocketHandler PamsclientSocketHandler;
            try
            {
                PamsclientSocketHandler = new PamsClientSocketHandler(currListener.EndAccept(ar));
                PamsclientSocketHandler.setAsyncReceive();
                PamsclientSocketHandler.SetLastCommTime();
                PamsclientSockets.Add(PamsclientSocketHandler);
                currListener.BeginAccept(new AsyncCallback(PamsAcceptCallback), currListener);
                Program.AppLog.Write("소켓 연결(" + PamsclientSocketHandler.ClientSocket.Handle.ToInt64().ToString() + ")");
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "PamsAcceptCallback 함수 오류", e);

                if (Pamslistener != null) Pamslistener.Dispose();

                IPAddress hostIP;
                IPEndPoint ep;

                hostIP = IPAddress.Any;
                ep = new IPEndPoint(hostIP, Properties.Settings.Default.FROM_MW_PORT);
                Pamslistener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Pamslistener.Bind(ep);
                Pamslistener.Listen(Properties.Settings.Default.NET_ListeningSocketCount);
                Pamslistener.BeginAccept(new AsyncCallback(PamsAcceptCallback), Pamslistener);
            }
        }

        public void CheckHealthState()
        {

        }
        public void CheckSocketTimeout()
        {
            try
            {
                for (int i = 0; i < clientSockets.Count; i++)
                {
                    if (!clientSockets[i].bCheckConnection() || clientSockets[i].bCheckTimeout())
                    {
                        //23.09.12 수정
                        //clientSockets[i].Dispose();
                    }
                }
            }
            catch (Exception e)
            {

                Program.SysLog.Write(this, "CheckSocketTimeout 함수 오류", e);
            }
        }

        public void RemoveDisconnectedSocket()
        {
            try
            {
                for (int i = 0; i < clientSockets.Count; i++)
                {
                    if (clientSockets[i].bComplete)
                    {
                        //TimeSpan disconnectTimeSpan = new TimeSpan(0, 0, Properties.Settings.Default.APP_DisconnectDislayTime);
                        TimeSpan disconnectTimeSpan = new TimeSpan(0, 10, 0);
                        if (clientSockets[i].DisconnectedDateTime + disconnectTimeSpan < DateTime.Now)
                        {
                            //24.02.14 수정
                            clientSockets[i].Dispose();
                            clientSockets.RemoveAt(i);
                        }
                    }
                    while (clientSockets.Count < mwListView.Items.Count)
                        mwListView.Items.RemoveAt(mwListView.Items.Count - 1);


                    //if (!clientSockets[i].bCheckConnection())
                    //{
                    //    TimeSpan disconnectTimeSpan = new TimeSpan(0, 0, Properties.Settings.Default.APP_DisconnectDislayTime);
                    //    if (clientSockets[i].DisconnectedDateTime + disconnectTimeSpan < DateTime.Now)
                    //    {
                    //        //23.09.12 수정
                    //        //clientSockets[i].Dispose();
                    //        //clientSockets.RemoveAt(i);
                    //    }
                    //}
                    //while (clientSockets.Count < mwListView.Items.Count)
                    //    mwListView.Items.RemoveAt(mwListView.Items.Count - 1);
                }

                for (int i = 0; i < PamsclientSockets.Count; i++)
                {
                    if (PamsclientSockets[i].bComplete)
                    {
                        //TimeSpan disconnectTimeSpan = new TimeSpan(0, 0, Properties.Settings.Default.APP_DisconnectDislayTime);
                        TimeSpan disconnectTimeSpan = new TimeSpan(0, 10, 0);
                        if (PamsclientSockets[i].DisconnectedDateTime + disconnectTimeSpan < DateTime.Now)
                        {
                            //24.02.14 수정
                            PamsclientSockets[i].Dispose();
                            PamsclientSockets.RemoveAt(i);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "CheckSocketTimeout 함수 오류", e);
            }



            //try
            //{
            //    for (int i = 0; i < clientSockets.Count; i++)
            //    {
            //        if (!clientSockets[i].bCheckConnection())
            //        {
            //            TimeSpan disconnectTimeSpan = new TimeSpan(0, 0, Properties.Settings.Default.APP_DisconnectDislayTime);
            //            if (clientSockets[i].DisconnectedDateTime + disconnectTimeSpan < DateTime.Now)
            //            {
            //                //23.09.12 수정
            //                //clientSockets[i].Dispose();
            //                //clientSockets.RemoveAt(i);
            //            }
            //        }
            //        while (clientSockets.Count < mwListView.Items.Count)
            //            mwListView.Items.RemoveAt(mwListView.Items.Count - 1);
            //    }
            //}
            //catch (Exception e)
            //{
            //    Program.SysLog.Write(this, "CheckSocketTimeout 함수 오류", e);
            //}
        }


        private int FindListViewIndex(string socketID)
        {
            try
            {
                for (int i = 0; i < mwListView.Items.Count; i++)
                {
                    if (mwListView.Items[i].SubItems[0].Text.Equals(socketID))
                        return i;
                }
            }
            catch (Exception e
            )
            {
                Program.SysLog.Write(this, "FindListViewIndex 함수 오류", e);
                return -1;
            }
            return -1;
        }

        delegate void SafetyUpdateMWState();

        public void UpdateMWState()
        {
            string[] ItemString;
            int nItemIndex;

            int iConnectCnt = 0;
            try
            {
                //for (int i = 0; i < clientSockets.Count; i++)
                //{
                //    if (clientSockets[i].bCheckConnection()) iConnectCnt = iConnectCnt + 1;
                //}

                //AddConnectList("서버연결 장비수 : " + iConnectCnt.ToString() + " 대");
                //ConnCount("서버연결 장비수 : " + iConnectCnt.ToString() + " 대");
                //ConnCount("서버 데이터 처리수 : " + clientSockets.Count.ToString() + " 건");
                ConnCount("서버 통신:" + clientSockets.Count.ToString() + ", " + PamsclientSockets.Count.ToString() + ", " + MwclientSockets.Count.ToString());

                //if (mwListView.InvokeRequired)
                //    mwListView.BeginInvoke(new SafetyUpdateMWState(UpdateMWState));
                //else
                //{
                //    mwListView.Items.Clear();

                //    for (int i = 0; i < clientSockets.Count; i++)
                //    {
                //        ItemString = new string[6];
                //        ItemString[0] = i.ToString();
                //        ItemString[1] = clientSockets[i].ClientSocket.Handle.ToInt64().ToString();
                //        ItemString[2] = clientSockets[i].MiddlewareID;
                //        ItemString[3] = (clientSockets[i].bCheckConnection()) ? "connected" : "disconnected";
                //        ItemString[4] = string.Format("{0:yyyy-MM-dd HH:mm:ss}", clientSockets[i].ConnectedDateTime);
                //        //                    ItemString[4] = "normal";
                //        ItemString[5] = string.Format("{0:yyyy-MM-dd HH:mm:ss}", clientSockets[i].LastCommunicationTime);
                //        //                    mwListView.Items.Add(new ListViewItem(ItemString));
                //        nItemIndex = FindListViewIndex(i.ToString());
                //        if (nItemIndex == -1)
                //            mwListView.Items.Add(new ListViewItem(ItemString));
                //        else
                //        {
                //            for (int j = 1; j < 6; j++)
                //                mwListView.Items[nItemIndex].SubItems[j].Text = ItemString[j];
                //        }
                //        ItemString = null;
                //    }
                //}
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "미들웨어 상태 업데이트 오류", e);
            }
        }

        delegate void SafetyAddLogListTime(bool clear, string Time, string logMsg);

        delegate void SafetyAddLogList(string logMsg);

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
                    while (logListBox.Items.Count >= Properties.Settings.Default.APP_MAXListCount)
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

        delegate void SafetyConnCont(string logMsg);

        public void ConnCount(string logMsg)
        {
            try
            {
                if (txtConnCount.InvokeRequired)
                {
                    //logListBox.Invoke(new SafetyAddLogList(AddLogList), logMsg);
                    txtConnCount.BeginInvoke(new SafetyConnCont(ConnCount), logMsg);
                }
                else
                {
                    //lock을 사용하는 경우 이벤트 타이밍으로 인해 화면 멈춤 발생 가능함
                    txtConnCount.Text = string.Format("{0:yyyy-MM-dd HH:mm:ss}] {1}", DateTime.Now, logMsg);
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "Connect Count 로그 오류!");
                Program.SysLog.Write(this, "Connect Count 로그 오류!", e);
            }
        }

        //public void AddConnectList(string logMsg)
        //{
        //    try
        //    {
        //        if (ConnectListBox.InvokeRequired)
        //        {
        //            //ConnectListBox.Invoke(new SafetyAddLogList(AddLogList), logMsg);
        //            ConnectListBox.BeginInvoke(new SafetyAddLogList(AddConnectList), logMsg);
        //        }
        //        else
        //        {
        //            //lock을 사용하는 경우 이벤트 타이밍으로 인해 화면 멈춤 발생 가능함
        //            int nTopIndex = ConnectListBox.TopIndex;
        //            ConnectListBox.Items.Insert(0, string.Format("{0:yyyy-MM-dd HH:mm:ss}] {1}", DateTime.Now, logMsg));
        //            while (ConnectListBox.Items.Count >= 2)
        //            {
        //                //ConnectListBox.Items.RemoveAt(0);
        //                ConnectListBox.Items.RemoveAt(ConnectListBox.Items.Count - 1);
        //            }
        //            ConnectListBox.TopIndex = nTopIndex;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Program.AppLog.Write(this, "ConnectListBox 로그 추가 오류!");
        //        Program.SysLog.Write(this, "ConnectListBox 로그 추가 오류!", e);
        //    }
        //}

        private void mwStateCheckTimer_Tick(object sender, EventArgs e)
        {
            CheckSocketTimeout();
            RemoveDisconnectedSocket();
            UpdateMWState();
        }

        private void ScheduleCheckTimer_Tick(object sender, EventArgs e)
        {
            string strCurrentDate;

            try
            {
                strCurrentDate = DateTime.Now.ToString("yyyyMMdd");

                if (string.IsNullOrEmpty(strLastLogClearanceDate) || !strLastLogClearanceDate.Equals(strCurrentDate))
                {
                    Program.AppLog.Write("로그 파일 정리[DATE:" + strCurrentDate + "]");
                    Program.SysLog.Clearance();
                    Program.AppLog.Clearance();
                    strLastLogClearanceDate = strCurrentDate;

                    DeleteEventProcHit();
                }
            }
            catch (Exception ex)
            {
                Program.SysLog.Write(this, "Schedule 실행오류", ex);
            }
        }


        private void CommDataTimer_Tick(object sender, EventArgs e)
        {
            //if (_sENVCheckDate != DateTime.Now.ToString("yyyyMMdd"))
            //{
            //    //EnvInfo();
            //}
        }

        private void CrAllTimer_Tick(object sender, EventArgs s)
        {
            //if (_sCRCheckDate == DateTime.Now.ToString("yyyyMMdd"))
            //{
            //    if (CrProcChk == false) CrInfoAll(false);
            //}
            //else
            //{
            //    if (CrProcChk == false) CrInfoAll(true);
            //}

        }

        // 장치 상태 구조체 추가 함수(멀티쓰레드를 위한 lock block은 상위함수에서 제어)
        public void AddEQErrorState(EQErrorState esErrorState)
        {
            try
            {
                lEQErrorState.Add(esErrorState);
            }
            catch (Exception e)
            {

                Program.SysLog.Write(this, "AddEQErrorState 실행오류", e);
            }
        }


        public EQErrorState GetEQErrorState(string pstrEQID)
        {
            int i;
            try
            {
                for (i = 0; i < lEQErrorState.Count; i++)
                {
                    if (lEQErrorState[i] != null && !string.IsNullOrEmpty(lEQErrorState[i].EQID) && lEQErrorState[i].EQID.Equals(pstrEQID))
                    {
                        return lEQErrorState[i];
                    }
                    if (lEQErrorState[i] == null)
                        Program.AppLog.Write(this, "NULL EQErrorState Exist[Count:" + lEQErrorState.Count.ToString() + ",Index:" + i.ToString() + "]");
                }
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "GetEQErrorState 오류", e);
            }
            return null;
        }





        #region 장비 상태 체크
        public Dictionary<string, Dictionary<string, object>> GetGateEquipStatus(string sGateID)
        {
            Dictionary<string, Dictionary<string, object>> result = null;

            result = gateEquipStatus[sGateID];
            return result;
        }

        /// <summary>
        /// Command1210 명령어 처리시 사용
        /// </summary>
        /// <param name="request"></param>
        public void SetGateEquipStatus(Dictionary<string, object> request)
        {
            if (request == null) return;

            string sGateID = request["GATE_ID"] == null ? "" : request["GATE_ID"].ToString();
            string sReaderID = request["READER_ID"] == null ? "" : request["READER_ID"].ToString();

            Dictionary<string, Dictionary<string, object>> equipStatus = GetGateEquipStatus(sGateID);

            if (equipStatus == null)
            {
                equipStatus = new Dictionary<string, Dictionary<string, object>>();
            }
            equipStatus.Add(sReaderID, request);
            gateEquipStatus.Add(sGateID, equipStatus);
            Program.AppLog.Write(request.ToString());
            Program.AppLog.Write("gateEquipStatus.size() : " + gateEquipStatus.Count);

        }


        /// <summary>
        /// Control Manager 구동 : 각 gateway, reader기 상태를 database에 update(2분)
        /// </summary>
        private void EquipStatus()
        {
            while (_pauseEvent.WaitOne())
            {
                try
                {
                    //장비 상태 체크
                    //게이트

                    foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, object>>> dic1 in gateEquipStatus)
                    {
                        string sGateID = dic1.Key;
                        Program.AppLog.Write("SEND GATE STATUS :: " + sGateID);

                        Dictionary<string, Dictionary<string, object>> equipStatus = GetGateEquipStatus(sGateID);
                        if (equipStatus == null)
                        {
                            continue;
                        }

                        bool readerError = false;

                        Dictionary<string, object> request = null;

                        //리더기별 장비리스트
                        foreach (KeyValuePair<string, Dictionary<string, object>> dic2 in equipStatus)
                        {
                            try
                            {
                                string sReaderID = dic2.Key;
                                request = dic2.Value;

                                if (request == null) continue;

                                string sStatus = request["STATUS"] == null ? "" : request["STATUS"].ToString();

                                if (sStatus.Trim() == "ERR")
                                {
                                    readerError = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                        }

                        if (readerError)
                        {
                            request["STATUS"] = "ERR";
                        }
                        else
                        {
                            request["STATUS"] = "NOR";
                        }

                        //프로시져 호출
                        /*
                            request.put("queryid", "TRFID_GATE_INFO.update");							
							SqlMapClient.update(request); 
                         */

                        string sCurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        if (htConnectionStatus != null && htConnectionStatus.ContainsKey(EMS))
                        {
                            htConnectionStatus.Remove(EMS);
                        }

                        try
                        {
                            CtrlClient.connect(request);
                            htConnectionStatus.Add(EMS, new ConnectionStatus(EMS, "T", sCurrentTime));
                        }
                        catch (Exception ex)
                        {
                            Program.SysLog.Write(this, "EquipStatus CtrlClient.connect Exception", ex);
                            htConnectionStatus.Add(EMS, new ConnectionStatus(EMS, "F", sCurrentTime));
                        }

                    }

                }
                catch (Exception ex)
                {
                    Program.SysLog.Write(this, "EquipStatus() Exception", ex);
                }

                Thread.Sleep(12000);
            }
        }

        /// <summary>
        /// 연계상태 통합관제로 전송
        /// </summary>
        private void ConnectionStatusSendToEMS()
        {
            while (_pauseEvent.WaitOne())
            {

                string sCurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                try
                {
                    if (htConnectionStatus != null && htConnectionStatus.ContainsKey(EMS))
                    {
                        htConnectionStatus.Remove(EMS);
                    }

                    Dictionary<string, object> ems = new Dictionary<string, object>();
                    ems.Add("WORKDIV", "CONNECTION_STT");

                    ConnectionStatus pams = (ConnectionStatus)htConnectionStatus[PAMS];
                    ems.Add("PAMS_STT", pams.getConnectionStatus());

                    ConnectionStatus rfidMw = (ConnectionStatus)htConnectionStatus[RFID_MW];
                    ems.Add("RFIDMW_STT", rfidMw.getConnectionStatus());

                    CtrlClient.connect(ems);

                    htConnectionStatus.Add(EMS, new ConnectionStatus(EMS, "T", sCurrentTime));

                }
                catch (Exception ex)
                {
                    Program.SysLog.Write(this, "ConnectionStatusSendToEMS() Exception", ex);
                    htConnectionStatus.Add(EMS, new ConnectionStatus(EMS, "F", sCurrentTime));
                }

                Thread.Sleep(60000);
            }
        }
        #endregion

        #region PAMS 체크
        /// <summary>
        /// Pams 상태 체크
        /// </summary>
        private void PamsAliveCheck()
        {
            while (_pauseEvent.WaitOne())
            {
                Program.AppLog.Write("PamsClient.alive Check");


                string sCurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                try
                {
                    JObject request = JObject.FromObject("{\r\n  \"WORKDIV\": \"ALIVECHECK\",\r\n }");


                    JObject response = PamsClient.connect("com.hs.pams.common.server.bkroom.RfidPamsAlivecheckMsg", request);
                    response["WORKDIV"] = "010REP";

                    if (response.Count > 0 && response["RESULT"].ToString() == "0")
                    {
                        if (htConnectionStatus.ContainsKey(PAMS))
                        {
                            htConnectionStatus.Remove(PAMS);
                            htConnectionStatus.Add(PAMS, new ConnectionStatus(PAMS, "T", sCurrentTime));
                        }
                        else
                        {
                            htConnectionStatus.Add(PAMS, new ConnectionStatus(PAMS, "T", sCurrentTime));
                        }
                    }
                    else
                    {
                        if (htConnectionStatus.ContainsKey(PAMS))
                        {
                            htConnectionStatus.Remove(PAMS);
                            htConnectionStatus.Add(PAMS, new ConnectionStatus(PAMS, "F", sCurrentTime));
                        }
                        else
                        {
                            htConnectionStatus.Add(PAMS, new ConnectionStatus(PAMS, "F", sCurrentTime));
                        }
                    }



                }
                catch (Exception ex)
                {
                    Program.SysLog.Write(this, "PamsClient.alive Exception", ex);
                    if (htConnectionStatus.ContainsKey(PAMS))
                    {
                        htConnectionStatus.Remove(PAMS);
                        htConnectionStatus.Add(PAMS, new ConnectionStatus(PAMS, "F", sCurrentTime));
                    }
                    else
                    {
                        htConnectionStatus.Add(PAMS, new ConnectionStatus(PAMS, "F", sCurrentTime));
                    }
                }

                Thread.Sleep(10000);
            }
        }
        #endregion


        /// <summary>
        /// Print 상태 체크
        /// </summary>
        private void PrintAliveCheck()
        {
            while (_pauseEvent.WaitOne())
            {
                Program.AppLog.Write("Print.alive Check");

                try
                {
                    string sEQ_TYPE = "PRT";

                    Hashtable htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();
                    htParamsHashtable.Add("IN_EQ_TYPE", sEQ_TYPE);

                    DataSet dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C119REQ_SELECT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

                    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                    {
                        for (int iRow = 0; iRow < dsResult.Tables[0].Rows.Count; iRow++)
                        {
                            string sIP = dsResult.Tables[0].Rows[iRow]["IP_ADDR"] == null ? "" : dsResult.Tables[0].Rows[iRow]["IP_ADDR"].ToString();
                            string sEQ_ID = dsResult.Tables[0].Rows[iRow]["EQ_ID"] == null ? "" : dsResult.Tables[0].Rows[iRow]["EQ_ID"].ToString();
                            string sSTATUS = string.Empty;

                            if (sIP.Trim().Length > 0)
                            {
                                bool bRtn = PrinterAgent.alive(sIP, "Default");

                                if (bRtn)
                                {
                                    sSTATUS = "NOR";
                                }
                                else
                                {
                                    sSTATUS = "ERR";
                                }

                                try
                                {
                                    htParamsHashtable = new Hashtable();
                                    htParamsHashtable.Clear();
                                    htParamsHashtable.Add("IN_EQ_ID", sEQ_ID);
                                    htParamsHashtable.Add("IN_STATUS", sSTATUS);

                                    ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_EQUIP_INFO_STATUS_UPDATE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                                    string strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                                    string strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();
                                }
                                catch (Exception e)
                                {
                                    //Program.SysLog.Write(this, "C119REQ_UPDATE 오류" + ", RcvData:" + request.ToString(), e);
                                    Program.SysLog.Write(this, "PrintAliveCheck TRFID_EQUIP_INFO_STATUS_UPDATE 오류", e);
                                }

                            }

                        }
                    }

                    //PDA, 정수점검기 PING Command 일정 시간 안온 데이터 ERR 처리
                    try
                    {
                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();

                        ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.PDA_CNT_STATUS_UPDATE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                        string strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                        string strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();
                    }
                    catch (Exception e)
                    {
                        //Program.SysLog.Write(this, "C119REQ_UPDATE 오류" + ", RcvData:" + request.ToString(), e);
                        Program.SysLog.Write(this, "PrintAliveCheck PDA_CNT_STATUS_UPDATE 오류", e);
                    }

                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "장비 관리 조회 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "PrintAliveCheck 오류", e);
                }


                Thread.Sleep(30 * 1000);
            }
        }


        private void Print()
        {
            while (_pauseEvent.WaitOne())
            {
                //Program.AppLog.Write("Print");

                try
                {


                    Hashtable htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();

                    DataSet dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.TRFID_ISSUE_REQ_PRINT_LIST", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

                    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in dsResult.Tables[0].Rows)
                        {
                            Dictionary<string, object> param = new Dictionary<string, object>();

                            foreach (DataColumn column in dsResult.Tables[0].Columns)
                            {
                                param.Add(column.ColumnName, row[column.ColumnName]);
                            }

                            PrinterAgent.print(param);
                        }
                    }

                    // 출력된 데이터 PAMS 전송
                    try
                    {
                        htParamsHashtable = new Hashtable();
                        htParamsHashtable.Clear();
                        string sTRANS_TP = string.Empty;
                        string sTRANS_ERR = string.Empty;

                        dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.PAMS_PRINT_RESULT_LIST", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

                        if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in dsResult.Tables[0].Rows)
                            {
                                //Dictionary<string, object> param = new Dictionary<string, object>();
                                JObject param = new JObject();

                                foreach (DataColumn column in dsResult.Tables[0].Columns)
                                {
                                    param.Add(column.ColumnName, JToken.FromObject(row[column.ColumnName]));
                                }

                                param.Add("WORKDIV", "PRINTRESULT");

                                try
                                {
                                    //Dictionary<string, object> response = PamsClient.connect("com.hs.pams.common.server.bkroom.RfidTagRslthMsg", param);

                                    JObject response = PRINTRESULT(param);

                                    if (response["RESULT"].ToString() == "0")
                                    {
                                        sTRANS_TP = "001";
                                    }
                                    else
                                    {
                                        sTRANS_TP = "002";
                                        sTRANS_ERR = response["ERROR"].ToString(); //"Exception";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    sTRANS_TP = "002";
                                    sTRANS_ERR = ex.ToString();
                                }

                                htParamsHashtable = new Hashtable();
                                htParamsHashtable.Clear();
                                htParamsHashtable.Add("IN_ISSUE_REQ_DTTM", param["ISSUE_REQ_DTTM"].ToString());
                                htParamsHashtable.Add("IN_EQ_ID", param["EQ_ID"].ToString());
                                htParamsHashtable.Add("IN_PRT_EQ_ID", param["PRT_EQ_ID"].ToString());
                                htParamsHashtable.Add("IN_TAG_ID", param["TAG_ID"].ToString());
                                htParamsHashtable.Add("IN_ISSUE_RESULT", "");
                                //수정 안익태 24.02.01
                                if (sTRANS_TP == "001")
                                {
                                    htParamsHashtable.Add("IN_TRANS_TP", sTRANS_TP);
                                    htParamsHashtable.Add("IN_TRANS_ERR", sTRANS_ERR);
                                }
                                else
                                {
                                    htParamsHashtable.Add("IN_TRANS_TP", "000");
                                    htParamsHashtable.Add("IN_TRANS_ERR", sTRANS_TP + "," + sTRANS_ERR);
                                }

                                ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteProcedure("PAMSDATA.TRFID_ISSUE_REQ_UPDATE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                                string strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                                string strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();

                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //Program.SysLog.Write(this, "C119REQ_UPDATE 오류" + ", RcvData:" + request.ToString(), e);
                        Program.SysLog.Write(this, "출력된 데이터 PAMS 전송 오류", e);
                    }

                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "장비 관리 조회 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "print 오류", e);
                }


                Thread.Sleep(1000 * 5);
            }
        }

        public JObject PRINTRESULT(JObject request)
        {
            string sWORKDIV = "PRINTRESULT";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            try
            {

                if (request.ContainsKey("WORKDIV"))
                {
                    request["WORKDIV"] = "PRINTRESULT";
                }
                else
                {
                    request.Add("WORKDIV", "PRINTRESULT");
                }
                //response = PamsClient.connect("com.hs.pams.common.server.bkroom.RfidTagReExecImp", request);

                string sCall = "http://172.30.234.6:9091/rfid/iss/rfidTagRslthMsg.do?";  //USER_ID=pams2&PWD=pams1!";
                string sPara1 = request["PAMS_MGMT_NO"] == null ? "" : request["PAMS_MGMT_NO"].ToString();
                string sPara2 = request["TAG_ID"] == null ? "" : request["TAG_ID"].ToString();
                string sPara3 = string.Empty;

                string sPara = string.Format("PAMS_MGMT_NO={0}&TAG_ID={1}", sPara1, sPara2);

                RestAPI RA = new RestAPI();
                response = RA.CallAPI(sCall + sPara, "PRINTRESULT");

                if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                else response.Add("WORKDIV", sWORKDIV + "REP");

                JArray rows = new JArray();
                if (response["LIST"] != null)
                {
                    rows = (JArray)response["LIST"];
                    response.Remove("LIST");
                }

                response.Add("count", JToken.FromObject(rows.Count));
                response.Add("rows", JToken.FromObject(rows));


                return response;
                //string sRtn = response.ToString();
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "PRINTRESULT 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);
                return SendErrResponse(sWORKDIV, "1", e.ToString());
            }
        }

        private JObject SendErrResponse(string sWORKDIV, string sErrCD = "1", string sErrMsg = "Server 예외 에러 발생")
        {
            var list = new List<JObject>();
            Dictionary<string, object> aData = new Dictionary<string, object>();
            aData.Add("WORKDIV", sWORKDIV + "REP");

            aData.Add("RESULT", sErrCD);
            aData.Add("ERROR", sErrMsg);

            JObject json = new JObject();

            if (aData != null)
            {
                foreach (string strKey in aData.Keys)
                {
                    JProperty property = new JProperty(strKey, aData[strKey]);

                    json.Add(property);
                }
            }

            return json;

        }

        private void GateStt()
        {
            while (_pauseEvent.WaitOne())
            {
                Program.AppLog.Write("GateStt");
                try
                {
                    Hashtable htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();
                    htParamsHashtable.Add("IN_EQ_TYPE", "GAT");

                    DataSet dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PAMSDATA.C119REQ_SELECT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

                    Dictionary<string, object> dicGateStt = new Dictionary<string, object>();

                    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                    {
                        for (int iRow = 0; iRow < dsResult.Tables[0].Rows.Count; iRow++)
                        {
                            dicGateStt.Clear();
                            dicGateStt.Add("WORKDIV", "GATE_STT");
                            dicGateStt.Add("GATE_ID", dsResult.Tables[0].Rows[iRow]["GATE_ID"].ToString().Trim());
                            dicGateStt.Add("READER_ID", dsResult.Tables[0].Rows[iRow]["EQ_ID"].ToString().Trim());
                            dicGateStt.Add("STATUS", dsResult.Tables[0].Rows[iRow]["STATUS"].ToString().Trim());

                            JObject ctrlResult2 = CtrlClient.connect(dicGateStt);
                            Dictionary<string, object> ctrlDic2 = JObject.FromObject(ctrlResult2).ToObject<Dictionary<string, object>>();

                            string sCtrlResult2 = ctrlDic2["RESULT"] == null ? "" : ctrlDic2["RESULT"].ToString().Trim().Replace("\0", "");

                            if (sCtrlResult2 == "OK")
                            {
                                //Program.AppLog.Write(this, "ConnectionStatus 전송 성공");
                            }
                            else
                            {
                                Program.AppLog.Write(this, "ConnectionStatus 전송 실패");
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "장비 관리 조회 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "ReadTagSend 오류", e);
                }


                Thread.Sleep(60000 * 2);
            }
        }

        private void ConnectionStatus()
        {
            while (_pauseEvent.WaitOne())
            {
                Program.AppLog.Write("ConnectionStatus");
                try
                {
                    Dictionary<string, object> NewParam = new Dictionary<string, object>();
                    NewParam.Add("WORKDIV", "CONNECTION_STT");
                    NewParam.Add("PAMS_STT", "T");
                    NewParam.Add("RFIDMW_STT", "T");

                    JObject ctrlResult = CtrlClient.connect(NewParam);
                    Dictionary<string, object> ctrlDic = JObject.FromObject(ctrlResult).ToObject<Dictionary<string, object>>();

                    string sCtrlResult = ctrlDic["RESULT"] == null ? "" : ctrlDic["RESULT"].ToString().Trim().Replace("\0", "");

                    if (sCtrlResult == "OK")
                    {
                        //Program.AppLog.Write(this, "ConnectionStatus 전송 성공");
                    }
                    else
                    {
                        Program.AppLog.Write(this, "ConnectionStatus 전송 실패");
                    }

                }
                catch (Exception e)
                {
                    Program.SysLog.Write(this, "ConnectionStatus 오류", e);
                }


                Thread.Sleep(60000);
            }
        }

        private void ReadTagSend()
        {
            while (_pauseEvent.WaitOne())
            {
                Program.AppLog.Write("ReadTagSend");
                try
                {
                    Hashtable htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();

                    DataSet dsResult = ((MainForm)(Application.OpenForms["MainForm"])).dbManagement.ExecuteSelectProcedure("PKG_MW_PROC.READ_TAG_SELECT", htParamsHashtable, Properties.Settings.Default.DB_Timeout);

                    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in dsResult.Tables[0].Rows)
                        {
                            Dictionary<string, object> param = new Dictionary<string, object>();

                            foreach (DataColumn column in dsResult.Tables[0].Columns)
                            {
                                param.Add(column.ColumnName, row[column.ColumnName]);
                            }

                            C1400REQ(param);
                        }
                    }

                }
                catch (Exception e)
                {
                    //Program.SysLog.Write(this, "장비 관리 조회 오류" + ", RcvData:" + request.ToString(), e);
                    Program.SysLog.Write(this, "ReadTagSend 오류", e);
                }


                Thread.Sleep(1000 * 3);
            }
        }

        private void GetInoutList()
        {
            while (_pauseEvent.WaitOne())
            {
                //Program.AppLog.Write("GetInoutList");

                try
                {
                    C600REQ();

                    C700REQ();
                }
                catch (Exception e)
                {
                    Program.SysLog.Write(this, "GetInoutList 오류", e);
                }
                finally
                {
                    Thread.Sleep(1000 * 5);
                }
            }
        }

        public void C1400REQ(Dictionary<string, object> request)
        {
            string sWORKDIV = "ILG";

            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;
            JObject response = null;

            string sPAMS_MGMT_NO = string.Empty;
            string sFILE_BREAK_ID = string.Empty;

            try
            {


                if (request.ContainsKey("WORKDIV"))
                {
                    request["WORKDIV"] = "ILG";
                }
                else
                {
                    request.Add("WORKDIV", "ILG");
                }

                if (request["PAMS_MGMT_NO"].ToString().IndexOf('-') >= 0)
                {
                    sPAMS_MGMT_NO = request["PAMS_MGMT_NO"].ToString().Substring(0, request["PAMS_MGMT_NO"].ToString().IndexOf('-'));
                    sFILE_BREAK_ID = request["PAMS_MGMT_NO"].ToString().Substring(request["PAMS_MGMT_NO"].ToString().IndexOf('-') + 1);
                    if (sFILE_BREAK_ID.Trim() == "00") sFILE_BREAK_ID = "";
                }
                else
                {
                    sPAMS_MGMT_NO = request["PAMS_MGMT_NO"].ToString();
                }


                string sCall = "http://172.30.234.6:9091/rfid//iss/rfidBindArryInfoMsg.do?WORKDIV=ILG&";
                string sPara1 = sPAMS_MGMT_NO;//request["PAMS_MGMT_NO"].ToString();
                string sPara2 = request["TAG_ID"].ToString();
                string sPara3 = request["GATE_ID"].ToString();
                string sPara4 = request["ZONE_ID"].ToString();
                string sPara5 = request["IN_OUT_FL"].ToString();
                string sPara6 = request["NOR_ILG_FL"].ToString();
                string sPara7 = sFILE_BREAK_ID;


                //string sPara = string.Format("PAMS_MGMT_NO={0}&TAG_ID={1}&GATE_ID={2}&ZONE_ID={3}&IN_OUT_FL={4}&NOR_ILG_FL={5}"
                //                             , sPara1, sPara2, sPara3, sPara4, sPara5, sPara6);

                string sPara = string.Format("PAMS_MGMT_NO={0}&TAG_ID={1}&GATE_ID={2}&ZONE_ID={3}&IN_OUT_FL={4}&NOR_ILG_FL={5}&FILE_BREAK_ID={6}"
                                             , sPara1, sPara2, sPara3, sPara4, sPara5, sPara6, sPara7);

                RestAPI RA = new RestAPI();
                response = RA.CallAPI(sCall + sPara, sWORKDIV);

                if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                else response.Add("WORKDIV", sWORKDIV + "REP");

                JArray rows = new JArray();
                if (response["LIST"] != null)
                {
                    rows = (JArray)response["LIST"];
                    response.Remove("LIST");
                }

                response.Add("count", JToken.FromObject(rows.Count));
                response.Add("rows", JToken.FromObject(rows));

                string sRtn = response.ToString();

                //성공 메세지 확인 필요
                if (sRtn.IndexOf(sPara1) >= 0)
                {
                    Hashtable htParamsHashtable = new Hashtable();
                    htParamsHashtable.Clear();
                    htParamsHashtable.Add("IN_TAG_ID", request["TAG_ID"].ToString());
                    htParamsHashtable.Add("IN_MGMT_NO", request["PAMS_MGMT_NO"].ToString());
                    htParamsHashtable.Add("IN_GATE_ID", request["GATE_ID"].ToString());
                    htParamsHashtable.Add("IN_PROC_CODE", "SEND_CONN");
                    htParamsHashtable.Add("IN_PROC_VAL", request["NOR_ILG_FL"].ToString() + "/" + request["IN_OUT_FL"].ToString() + "/" + request["ZONE_ID"].ToString() + "/완료");

                    htParamsHashtable.Add("IN_TRANS_VAL", "OK");


                    //PAMS 전송후 저장
                    dbManagement.ExecuteProcedure("PKG_MW_PROC.READ_TAG_TRANS_SAVE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                    strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                    strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();
                }

                if (request["NOR_ILG_FL"].ToString() != "NOR")
                {
                    //IL_DOC
                    Dictionary<string, object> NewParam = new Dictionary<string, object>();
                    NewParam.Add("WORKDIV", "IL_DOC");
                    NewParam.Add("GATE_ID", request["GATE_ID"].ToString().Trim());
                    NewParam.Add("MGMT_ID", sPAMS_MGMT_NO);    //request["PAMS_MGMT_NO"].ToString()
                    NewParam.Add("BIND_NO", request["BIND_NO"].ToString());
                    NewParam.Add("DOC_TTL", request["DOC_TTL"].ToString());
                    NewParam.Add("MED_TYPE", request["MED_TYPE"].ToString());
                    NewParam.Add("LOC", request["LOC"].ToString());
                    NewParam.Add("IN_OUT_FL", request["IN_OUT_FL"].ToString());

                    JObject ctrlResult = CtrlClient.connect(NewParam);
                    Dictionary<string, object> ctrlDic = JObject.FromObject(ctrlResult).ToObject<Dictionary<string, object>>();

                    string sCtrlResult = ctrlDic["RESULT"] == null ? "" : ctrlDic["RESULT"].ToString().Trim().Replace("\0", "");

                    if (sCtrlResult == "OK")//if (sCtrlResult == "OK" && sMwResult == "OK")
                    {
                        Program.AppLog.Write(this, "IL_DOC 전송 성공");
                    }
                    else
                    {
                        Program.AppLog.Write(this, "IL_DOC 전송 실패:" + request["GATE_ID"].ToString() + "," + request["PAMS_MGMT_NO"].ToString() + "," + request["BIND_NO"].ToString() + "," + request["DOC_TTL"].ToString()
                                                + "," + request["MED_TYPE"].ToString() + "," + request["LOC"].ToString() + "," + request["IN_OUT_FL"].ToString());
                    }
                }

            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "태그 정보 전송 오류(" + sWORKDIV + ")" + ", RcvData:" + request.ToString(), e);

                SendErrResponse(sWORKDIV);
            }
        }

        /// <summary>
        /// //600REQ(반출 : 반출의뢰서 목록 요청)
        /// </summary>
        /// <param name="request"></param>
        public void C600REQ()
        {
            /*
             * 	=================== summary ===================
             * 	 	업무명     : 반출
             * 		서브업무명 : 반출서 목록 요청
             *  ===============================================
             *  
             * 	============= request parameters ==============
             * 		USER_ID : 신청자ID
             *  ===============================================
             *  
             * 	============= response parameters =============
             * 		OUT_APPL_ID    : 반출서ID
             * 		APPL_ORG_NM    : 신청자부서명
             * 		APPL_USER_ID   : 신청자ID
             * 		APPL_USER_NM   : 신청자명 
             * 		OUT_APPL_DT    : 반출의뢰일
             * 		OUT_DT 		   : 반출일
             *  ===============================================
             *  
            */

            string sWORKDIV = "600";
            JObject request = null;
            JObject response = null;

            try
            {
                request = new JObject();

                if (request.ContainsKey("WORKDIV"))
                {
                    request["WORKDIV"] = "INOUTLISTOUT";
                }
                else
                {
                    request.Add("WORKDIV", "INOUTLISTOUT");
                }
                //response = PamsClient.connect("com.hs.pams.common.server.bkroom.InoutViewSttsMsg", request);

                string sCall = "http://172.30.234.6:9091/rfid/inout/inoutViewSttsMsg.do?WORKDIV=INOUTLISTOUT";

                RestAPI RA = new RestAPI();
                response = RA.CallAPI(sCall, "INOUTLISTOUT");

                if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                else response.Add("WORKDIV", sWORKDIV + "REP");

                JArray rows = new JArray();
                if (response["LIST"] != null)
                {
                    rows = (JArray)response["LIST"];
                    response.Remove("LIST");
                }

                response.Add("count", JToken.FromObject(rows.Count));
                response.Add("rows", JToken.FromObject(rows));

                joInList = response;

                //string sRtn = response.ToString();

                //SendResponse_New(sRtn, sWORKDIV + "REQ");

            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "반출서 목록 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + response.ToString(), e);
                SendErrResponse(sWORKDIV);
            }
        }


        /// <summary>
        /// //700REQ(반입 : 반입서 목록 요청)
        /// </summary>
        /// <param name="request"></param>
        public void C700REQ()
        {
            /*
	         * 	=================== summary ===================
	         * 	 	업무명     : 반입
	         * 		서브업무명 : 반입서 목록 요청
	         *  ===============================================
	         *  
	         * 	============= request parameters ==============
	         * 		USER_ID : 신청자ID
	         *  ===============================================
	         *  
	         * 	============= response parameters =============
	         * 		OUT_APPL_ID  	: 반출서ID
	         * 		IN_NO    		: 반입서번호
	         * 		APPL_ORG_NM  	: 신청자부서명
	         * 		APPL_USER_ID   	: 신청자ID 
	         * 		APPL_USER_NM 	: 신청자명
	         * 		OUT_APPL_DT     : 반출의뢰일
	         * 		OUT_DT 		    : 반출일
	         *  ===============================================
             *  
            */

            string sWORKDIV = "700";
            JObject request = null;
            JObject response = null;

            try
            {
                request = new JObject();

                if (request.ContainsKey("WORKDIV"))
                {
                    request["WORKDIV"] = "INOUTLISTIN";
                }
                else
                {
                    request.Add("WORKDIV", "INOUTLISTIN");
                }
                //response = PamsClient.connect("com.hs.pams.common.server.bkroom.InoutViewSttsMsg", request);

                string sCall = "http://172.30.234.6:9091/rfid/inout/inoutViewSttsMsg.do?WORKDIV=INOUTLISTIN";


                RestAPI RA = new RestAPI();
                response = RA.CallAPI(sCall, "INOUTLISTIN");

                if (response.ContainsKey("WORKDIV")) response["WORKDIV"] = sWORKDIV + "REP";
                else response.Add("WORKDIV", sWORKDIV + "REP");

                JArray rows = new JArray();
                if (response["LIST"] != null)
                {
                    rows = (JArray)response["LIST"];
                    response.Remove("LIST");
                }

                response.Add("count", JToken.FromObject(rows.Count));
                response.Add("rows", JToken.FromObject(rows));

                joOutList = response;

                //string sRtn = response.ToString();

                //SendResponse_New(sRtn, sWORKDIV + "REQ");

            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "반입서 목록 요청 요청 오류(" + sWORKDIV + ")" + ", RcvData:" + response.ToString(), e);
                SendErrResponse(sWORKDIV);
            }
        }


        
        public void DeleteEventProcHit()
        {
            string strAppCode = string.Empty;
            string strAppMsg = string.Empty;

            try
            {
                Hashtable htParamsHashtable = new Hashtable();
                htParamsHashtable.Clear();

                dbManagement.ExecuteProcedure("PAMSDATA.TRFID_EVENT_PROC_HIS_DELETE", htParamsHashtable, Properties.Settings.Default.DB_Timeout);
                strAppCode = htParamsHashtable["O_APP_CODE"].ToString();
                strAppMsg = htParamsHashtable["O_APP_MSG"].ToString();
            }
            catch (Exception e)
            {
                Program.SysLog.Write(this, "DeleteEventProcHit 오류", e);
            }
        }
    }
}
