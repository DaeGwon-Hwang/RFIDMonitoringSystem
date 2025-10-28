using DevExpress.Utils.Serializing.Helpers;
using DevExpress.XtraEditors.TextEditController.Win32;
using DevExpress.XtraExport.Helpers;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RFID_CONTROLLER.Collections;
using RFID_CONTROLLER.Controls.Grid;
using RFID_CONTROLLER.Frm;
using RFID_CONTROLLER.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Resources;
using DevExpress.Utils.Svg;
using WMPLib;

namespace RFID_CONTROLLER
{
    public partial class MainController : Form
    {
        private const string PAMS = "PAMS";//jck
        private const string RFID_MW = "RFID_MW";//jck
        private const string EMS = "EMS";//통합관제//jck
        /// <summary>
        /// 연계 상태 체크 오류 카운트. 지정 회수 이상 오류 시 연계상태 변경
        /// </summary>
        private int m_nConnectChkErrCnt = 0;

        ///// <summary>
        ///// 통합관제 연결상태 체크 오류 카운트. 지정 회수 이상 오류 시 통합관제 연결 상태 변경
        ///// </summary>
        //private int m_nEmsConnectChkErrCnt = 0;//v1002

        /// <summary>
        /// 지정횟수(60) 이상 연계상태 요청 오류 시 상태 변경
        /// </summary>
        private const int CONNECTION_CHK_CNT = 6 * 10;//10분. jck v1002. 
        //private const int CONNECTION_CHK_CNT = 20;//jck v1001

        private Thread thread;
        private delegate void threadFuncHandler();
        private static Dictionary<String, GateInfo> icons;
        public String gateID;
                
        int logDataOrder = 0;

        private Color m_colorError = Color.LightGray;

        private delegate void delegateUpdateConnectionStatus(bool bStatusConnection, bool bStatusPAMS, bool bStatusMW, bool bStatusEMS);

        private sttPanelCnt B2SttPanel = new sttPanelCnt(6);
        private sttPanelCnt B1SttPanel = new sttPanelCnt(11);
        private sttPanelCnt F1SttPanel = new sttPanelCnt(14);
        private sttPanelCnt F2SttPanel = new sttPanelCnt(6);

        private ArrayList inOutPanelList = new ArrayList();

        public static bool isOtherWindowOpen = false;
        public static bool isSocketRun = false;

        WindowsMediaPlayer wmp;

        public MainController()
        {
            InitializeComponent();
            LoadIniValue();
            gridSetting();

            GATLogGrid.DoubleBuffering(true);
            PRTLogGrid.DoubleBuffering(true);
            PDALogGrid.DoubleBuffering(true);
            CNTLogGrid.DoubleBuffering(true);

            thread = new Thread(delegate ()
            {
                run();
            });
            thread.IsBackground = true;
            thread.Start();

            SetSirenSound();
            //RingingSiren();
        }

        private void gridView_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            GridView view = sender as GridView;
            string status = view.GetRowCellValue(e.RowHandle, "STATUS").ToString();
            if (status == "ERR")
            {
                e.Appearance.BackColor = m_colorError;
            }
        }


        private void LoadIniValue()
        {
            //throw new NotImplementedException();
            string strIniFileName = "config.ini";
            try
            {
                //config.ini 파일 생성 정보
                /*
                Utils.SetIniValue("CONNECTION", "HOST_IP", "10.10.20.11", "config.ini");
                Utils.SetIniValue("CONNECTION", "HOST_PORT", "12012", "config.ini");
                Utils.SetIniValue("CONNECTION", "DISCONNECTION_CHECK_TIME", "10", "config.ini");
                */

                Utils.HOST_IP = Utils.GetIniValue("CONNECTION", "HOST_IP", "222.222.222.222", strIniFileName);
                Utils.HOST_PORT = Utils.GetIniValueInt("CONNECTION", "HOST_PORT", 22222, strIniFileName);
                Utils.DISCONNECTION_CHECK_TIME = Utils.GetIniValueInt("CONNECTION", "DISCONNECTION_CHECK_TIME", 22, strIniFileName);

                string jsonFilePath = "gateIdName.json";
                Utils.gateIdNameDic = DeserializeKeyValueDataFromJsonFile(jsonFilePath);

                Console.WriteLine("HOST_IP > " + Utils.HOST_IP);
                Console.WriteLine("HOST_PORT > " + Utils.HOST_PORT);
                Console.WriteLine("DISCONNECTION_CHECK_TIME > " + Utils.DISCONNECTION_CHECK_TIME);
            }
            catch
            {

            }
        }

        static void SerializeKeyValueDataToJsonFile(Dictionary<string, string> keyValueData, string filePath)
        {
            string jsonString = JsonConvert.SerializeObject(keyValueData);
            System.IO.File.WriteAllText(filePath, jsonString);
        }

        static Dictionary<string, string> DeserializeKeyValueDataFromJsonFile(string filePath)
        {
            string jsonString = System.IO.File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
        }

        private void gridSetting()
        {
            //그리드 읽기전용
            gridView1.OptionsBehavior.Editable = false;
            gridView2.OptionsBehavior.Editable = false;
            gridView3.OptionsBehavior.Editable = false;
            gridView4.OptionsBehavior.Editable = false;

            //이벤트
            gridView1.CustomDrawCell += gridView_CustomDrawCell;
            gridView2.CustomDrawCell += gridView_CustomDrawCell;
            gridView3.CustomDrawCell += gridView_CustomDrawCell;
            gridView4.CustomDrawCell += gridView_CustomDrawCell;
        }

        private void SetSirenSound()
        {
            string strFName = "siren.mp3";
            string filePath = Application.StartupPath + "\\" + strFName;

            if(!File.Exists(filePath)) 
            {
                MessageBox.Show(filePath + "\n파일을 찾을 수 없습니다.", "File Error");
            }

            wmp = new WindowsMediaPlayer();
            wmp.URL = filePath;
            wmp.controls.stop();
        }

        private void RingingSiren()
        {
            wmp.controls.play();
        }

        private void StopSiren()
        {
            wmp.controls.stop();
        }

        private void menuItemStartClick(object sender, EventArgs args)
        {
            Form sign = new SignManager();
            sign.StartPosition = FormStartPosition.CenterScreen;
            DialogResult result = sign.ShowDialog();

            if(result == DialogResult.OK)
            {
                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "004REQ");
                aData.Add("WK_TYPE", "STT");
                aData.Add("GATE_ID", this.gateID);

                JObject response = TCPClient.request(aData);

                if ((string)response["RESULT"] == "OK")
                {
                    MessageBox.Show("처리되었습니다.");
                }
                else
                {
                    MessageBox.Show((string)response["ERROR"]);
                }
            }
        }

        private void menuItemStopClick(object sender, EventArgs args)
        {

            Form sign = new SignManager();
            sign.StartPosition = FormStartPosition.CenterScreen;
            DialogResult result = sign.ShowDialog();

            if (result == DialogResult.OK)
            {
                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "004REQ");
                aData.Add("WK_TYPE", "STP");
                aData.Add("GATE_ID", this.gateID);

                JObject response = TCPClient.request(aData);

                if ((string)response["RESULT"] == "OK")
                {
                    MessageBox.Show("처리되었습니다.");
                }
                else
                {
                    MessageBox.Show((string)response["ERROR"]);
                }
            }

        }

        private void menuItemRestartClick(object sender, EventArgs args)
        {

            Form sign = new SignManager();
            sign.StartPosition = FormStartPosition.CenterScreen;
            DialogResult result = sign.ShowDialog();

            if (result == DialogResult.OK)
            {
                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "004REQ");
                aData.Add("WK_TYPE", "RBT");
                aData.Add("GATE_ID", this.gateID);

                JObject response = TCPClient.request(aData);

                if ((string)response["RESULT"] == "OK")
                {
                    MessageBox.Show("처리되었습니다.");
                }
                else
                {
                    MessageBox.Show((string)response["ERROR"]);
                }
            }
        }

        private void menuItemControlClick(object sender, EventArgs args)
        {

            Form sign = new SignManager();
            sign.StartPosition = FormStartPosition.CenterScreen;
            DialogResult result = sign.ShowDialog();

            if (result == DialogResult.OK)
            {
                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "004REQ");
                aData.Add("WK_TYPE", "OBZ");
                aData.Add("GATE_ID", this.gateID);

                JObject response = TCPClient.request(aData);

                if ((string)response["RESULT"] == "OK")
                {
                    MessageBox.Show("처리되었습니다.");
                }
                else
                {
                    MessageBox.Show((string)response["ERROR"]);
                }
            }
        }

        private void run()
        {
            try
            {
                // PDA 통신 확인
                Cursor = Cursors.WaitCursor;
                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "005REQ");
                aData.Add("EQ_TYPE", "PDA");
                aData.Add("SEQ", "1");

                JObject response = TCPClient.request(aData);

                Console.WriteLine("response.Count > " + response.Count);

                if (response.Count == 0)
                    this.Invoke(new delegateUpdateConnectionStatus(UpdateConnectionStatus), false, false, false, false);
            } catch (Exception ex)
            {
                Console.WriteLine("Exception 발생!");
            }
            finally { Cursor = Cursors.Default; }

            while (true)
            {
                if(!isSocketRun && !isOtherWindowOpen)
                {

                    Thread.Sleep(1000 * 15);

                    if (xtraTabControl1.SelectedTabPageIndex == 1)
                        Thread.Sleep(1000 * 20);

                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        if (xtraTabControl1.SelectedTabPageIndex == 0)
                        {
                            this.Invoke(new threadFuncHandler(display));
                        }
                        else if (xtraTabControl1.SelectedTabPageIndex == 1)
                        {
                            this.Invoke(new threadFuncHandler(reqLogData));
                        }
                        //this.Invoke(new DisplayHandler(displayTest));
                    }catch(Exception ex)
                    {
                        Console.WriteLine("Exception 발생!");
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }

            }
        }

        private void display()
        {
            Dictionary<string, object> aData = new Dictionary<string, object>();

            //jck start
            //연계 상태 요청
            aData = new Dictionary<string, object>();
            aData.Add("WORKDIV", "020REQ");
            //aData.Add("WORKDIV", "110REQ");

            JObject response = TCPClient.request(aData);

            if(response == null)
            {
                return;
            }

            if (response.Count == 0)
            {
                return;
            }

            JArray rows = (JArray)response["rows"];
            string strCurTime = (string)response["CurrentTime"];
            
            // 연계 접속 상태,pams 접속상태, MW  접속상태, 통합관제  접속상태
            bool bStatusConnection = false, bStatusPAMS = false, bStatusMW = false, bStatusEMS = false;
            
            if (rows != null)
            {
                DateTime dtCurTime = DateTime.Now;
                try 
                {
                    dtCurTime = Convert.ToDateTime(strCurTime);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception 발생!");
                }

                bStatusConnection = true;

                for (int i = 0; i < rows.Count; i++)
                {
                    JObject row = (JObject)rows[i];

                    String connectKnd = (string)row["connectionKind"];
                    String connectionStatus = (string)row["connectionStatus"];
                    String lastConnectionTime = (string)row["lastConnectionTime"];

                    bool bconnectionStatus = false;

                    try
                    {
                        //최종 통신 시간이 지정시간 이상 경과하였을 경우 통신 상태 오류
                        DateTime dtLstConnTime = Convert.ToDateTime(lastConnectionTime);
                        TimeSpan ts = dtCurTime - dtLstConnTime;

                        if (ts.TotalMinutes <= Utils.DISCONNECTION_CHECK_TIME)
                        {
                            if (connectionStatus.Equals("T"))
                                bconnectionStatus = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception 발생!");
                    }


                    if (connectKnd.Equals(PAMS))
                    {
                        bStatusPAMS = bconnectionStatus;
                    }
                    else if (connectKnd.Equals(RFID_MW))
                    {
                        bStatusMW = bconnectionStatus;
                    }
                    else if (connectKnd.Equals(EMS))
                    {
                        bStatusEMS = bconnectionStatus;//v1001
                    }
                }
            }
            bool bConnectionChange = false;
            if (bStatusConnection == false)
            {
                if (m_nConnectChkErrCnt >= CONNECTION_CHK_CNT)
                {
                    bConnectionChange = true;
                    m_nConnectChkErrCnt = 0;
                }
                else
                {
                    m_nConnectChkErrCnt++;
                    bStatusConnection = false;
                }
            }
            else
            {
                m_nConnectChkErrCnt = 0;
                bConnectionChange = true;
            }
            if (bConnectionChange)
                this.Invoke(new delegateUpdateConnectionStatus(UpdateConnectionStatus), bStatusConnection, bStatusPAMS, bStatusMW, bStatusEMS);

            // 무단반출 패널 초기화
            inOutPanelList.Clear();
            unauthOutPanelList.Controls.Clear();

            aData = new Dictionary<string, object>();
            aData.Add("WORKDIV", "118REQ");
            aData.Add("CRUD_TP", "R");

            response = TCPClient.request(aData);

            if (response == null)
            {
                return;
            }

            if (response.Count == 0)
            {
                Console.WriteLine("response count:" + response.Count.ToString());
                return;
            }

            if (response.ContainsKey("rows"))
            {
                Console.WriteLine("row 있음");
            }
            else
            {
                Console.WriteLine("row 없음");
                return;
            }


            rows = (JArray)response["rows"];

            if (rows != null)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    JObject row = (JObject)rows[i];

                    String key = (string)row["GATE_ID"];
                    
                    GateInfo gate = icons[key];

                    if (gate != null)
                    {
                        gate.updateUI((string)row["STATUS"]);

                        switch(key.Substring(0, 4))
                        {
                            case "RFB2":
                                if (row["STATUS"].ToString().Equals("NOR")) B2SttPanel.addNorCnt();
                                else if (row["STATUS"].ToString().Equals("ERR")) B2SttPanel.addErrCnt();
                                else if (row["STATUS"].ToString().Equals("IN"))
                                {
                                    B2SttPanel.addInCnt();
                                    addInoutPanel(row["INOUT_LIST"].ToString().Split('|'));
                                }
                                else if (row["STATUS"].ToString().Equals("OUT"))
                                {
                                    B2SttPanel.addInCnt();
                                    addInoutPanel(row["INOUT_LIST"].ToString().Split('|'));
                                }
                                else B2SttPanel.addErrCnt();
                                break;
                            case "RFB1":
                                if (row["STATUS"].ToString().Equals("NOR")) B1SttPanel.addNorCnt();
                                else if (row["STATUS"].ToString().Equals("ERR")) B1SttPanel.addErrCnt();
                                else if (row["STATUS"].ToString().Equals("IN"))
                                {
                                    B1SttPanel.addInCnt();
                                    addInoutPanel(row["INOUT_LIST"].ToString().Split('|'));
                                }
                                else if (row["STATUS"].ToString().Equals("OUT"))
                                {
                                    B1SttPanel.addInCnt();
                                    addInoutPanel(row["INOUT_LIST"].ToString().Split('|'));
                                }
                                else B1SttPanel.addErrCnt();
                                break;
                            case "RFF1":
                                if (row["STATUS"].ToString().Equals("NOR")) F1SttPanel.addNorCnt();
                                else if (row["STATUS"].ToString().Equals("ERR")) F1SttPanel.addErrCnt();
                                else if (row["STATUS"].ToString().Equals("IN"))
                                {
                                    F1SttPanel.addInCnt();
                                    addInoutPanel(row["INOUT_LIST"].ToString().Split('|'));
                                }
                                else if (row["STATUS"].ToString().Equals("OUT"))
                                {
                                    F1SttPanel.addInCnt();
                                    addInoutPanel(row["INOUT_LIST"].ToString().Split('|'));
                                }
                                else F1SttPanel.addErrCnt();
                                break;
                            case "RFF2":
                                if (row["STATUS"].ToString().Equals("NOR")) F2SttPanel.addNorCnt();
                                else if (row["STATUS"].ToString().Equals("ERR")) F2SttPanel.addErrCnt();
                                else if (row["STATUS"].ToString().Equals("IN"))
                                {
                                    F2SttPanel.addInCnt();
                                    addInoutPanel(row["INOUT_LIST"].ToString().Split('|'));
                                }
                                else if (row["STATUS"].ToString().Equals("OUT"))
                                {
                                    F2SttPanel.addInCnt();
                                    addInoutPanel(row["INOUT_LIST"].ToString().Split('|'));
                                }
                                else F2SttPanel.addErrCnt();
                                break;
                        }
                    }
                    
                }
            }

            B2SttPanel.cntCheck(); B1SttPanel.cntCheck(); F1SttPanel.cntCheck(); F2SttPanel.cntCheck();

            statusPanel1.norSttLabel = B2SttPanel.getNorCnt().ToString();
            statusPanel1.errSttLabel = B2SttPanel.getErrCnt().ToString();
            statusPanel1.inSttLabel = B2SttPanel.getInCnt().ToString();
            statusPanel1.outSttLabel = B2SttPanel.getOutCnt().ToString();

            statusPanel2.norSttLabel = B1SttPanel.getNorCnt().ToString();
            statusPanel2.errSttLabel = B1SttPanel.getErrCnt().ToString();
            statusPanel2.inSttLabel = B1SttPanel.getInCnt().ToString();
            statusPanel2.outSttLabel = B1SttPanel.getOutCnt().ToString();

            statusPanel3.norSttLabel = F1SttPanel.getNorCnt().ToString();
            statusPanel3.errSttLabel = F1SttPanel.getErrCnt().ToString();
            statusPanel3.inSttLabel = F1SttPanel.getInCnt().ToString();
            statusPanel3.outSttLabel = F1SttPanel.getOutCnt().ToString();

            statusPanel4.norSttLabel = F2SttPanel.getNorCnt().ToString();
            statusPanel4.errSttLabel = F2SttPanel.getErrCnt().ToString();
            statusPanel4.inSttLabel = F2SttPanel.getInCnt().ToString();
            statusPanel4.outSttLabel = F2SttPanel.getOutCnt().ToString();

            B2SttPanel.resetAllCnt(); B1SttPanel.resetAllCnt(); F1SttPanel.resetAllCnt(); F2SttPanel.resetAllCnt();

            // 무단반출 발생 체크
            displayStatus2Front();
        }

        private void displayTest()
        {
            inOutPanelList.Clear();
            unauthOutPanelList.Controls.Clear();

            addInoutPanel("2023-11-07 09:30:31|지상1층G03|PED0000010|반입".Split('|'));
            addInoutPanel("2023-11-06 09:30:31|지상1층G03|PED0000010|반입".Split('|'));
            addInoutPanel("2023-11-05 09:30:31|지상1층G03|PED0000010|반입".Split('|'));
            addInoutPanel("2023-11-10 09:30:31|지상1층G03|PED0000010|반입".Split('|'));

            displayStatus2Front();

            /*
            // 무단반출 패널 초기화
            unauthOutPanelList.Controls.Clear();

            //JArray rows;
            //            rows = (JArray)response["rows"];
            List<JObject> rows = new List<JObject>();
            JObject jTmp1 = JObject.Parse("{ GATE_ID:'RFB2G01',STATUS:'NOR',INOUT_LIST:''}");
            JObject jTmp2 = JObject.Parse("{ GATE_ID:'RFB2G02',STATUS:'NOR',INOUT_LIST:''}");
            JObject jTmp3 = JObject.Parse("{ GATE_ID:'RFB2G03',STATUS:'NOR',INOUT_LIST:''}");
            JObject jTmp4 = JObject.Parse("{ GATE_ID:'RFB2G04',STATUS:'NOR',INOUT_LIST:''}");
            JObject jTmp5 = JObject.Parse("{ GATE_ID:'RFB2G05',STATUS:'NOR',INOUT_LIST:'2023-11-07 09:32:29|지하2층G04|PED0000010|반출'}");
            JObject jTmp6 = JObject.Parse("{ GATE_ID:'RFB2G06',STATUS:'NOR',INOUT_LIST:'2023-11-07 09:32:29|지하9층G99|PED0000010|반입'}");

            rows.Add(jTmp1);
            rows.Add(jTmp2);
            rows.Add(jTmp3);
            rows.Add(jTmp4);
            rows.Add(jTmp5);
            rows.Add(jTmp6);

            if (rows != null)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    JObject row = (JObject)rows[i];

                    String key = (string)row["GATE_ID"];

                    GateInfo gate = icons[key];

                    if (gate != null)
                    {
                        gate.updateUI((string)row["STATUS"]);

                        switch (key.Substring(0, 4))
                        {
                            case "RFB2":
                                if (row["STATUS"].ToString().Equals("NOR")) B2SttPanel.addNorCnt();
                                else if (row["STATUS"].ToString().Equals("ERR")) B2SttPanel.addErrCnt();
                                else if (row["STATUS"].ToString().Equals("IN"))
                                {
                                    B2SttPanel.addInCnt();
                                    addInoutPanel(row["INOUT_LIST"].ToString().Split('|'));
                                }
                                else if (row["STATUS"].ToString().Equals("OUT")) 
                                {
                                    B2SttPanel.addOutCnt();
                                    addInoutPanel(row["INOUT_LIST"].ToString().Split('|'));
                                }
                                else B2SttPanel.addErrCnt();
                                break;
                            case "RFB1":
                                if (row["STATUS"].ToString().Equals("NOR")) B1SttPanel.addNorCnt();
                                else if (row["STATUS"].ToString().Equals("ERR")) B1SttPanel.addErrCnt();
                                else if (row["STATUS"].ToString().Equals("IN")) B2SttPanel.addInCnt();
                                else if (row["STATUS"].ToString().Equals("OUT")) B1SttPanel.addOutCnt();
                                else B1SttPanel.addErrCnt();
                                break;
                            case "RFF1":
                                if (row["STATUS"].ToString().Equals("NOR")) F1SttPanel.addNorCnt();
                                else if (row["STATUS"].ToString().Equals("ERR")) F1SttPanel.addErrCnt();
                                else if (row["STATUS"].ToString().Equals("IN")) F1SttPanel.addInCnt();
                                else if (row["STATUS"].ToString().Equals("OUT")) F1SttPanel.addOutCnt();
                                else F1SttPanel.addErrCnt();
                                break;
                            case "RFF2":
                                if (row["STATUS"].ToString().Equals("NOR")) F2SttPanel.addNorCnt();
                                else if (row["STATUS"].ToString().Equals("ERR")) F2SttPanel.addErrCnt();
                                else if (row["STATUS"].ToString().Equals("IN")) F2SttPanel.addInCnt();
                                else if (row["STATUS"].ToString().Equals("OUT")) F2SttPanel.addOutCnt();
                                else F2SttPanel.addErrCnt();
                                break;
                        }
                    }

                }
            }

            B2SttPanel.cntCheck(); B1SttPanel.cntCheck(); F1SttPanel.cntCheck(); F2SttPanel.cntCheck();

            statusPanel1.norSttLabel = B2SttPanel.getNorCnt().ToString();
            statusPanel1.errSttLabel = B2SttPanel.getErrCnt().ToString();
            statusPanel1.inSttLabel = B2SttPanel.getInCnt().ToString();
            statusPanel1.outSttLabel = B2SttPanel.getOutCnt().ToString();

            statusPanel2.norSttLabel = B1SttPanel.getNorCnt().ToString();
            statusPanel2.errSttLabel = B1SttPanel.getErrCnt().ToString();
            statusPanel2.inSttLabel = B1SttPanel.getInCnt().ToString();
            statusPanel2.outSttLabel = B1SttPanel.getOutCnt().ToString();

            statusPanel3.norSttLabel = F1SttPanel.getNorCnt().ToString();
            statusPanel3.errSttLabel = F1SttPanel.getErrCnt().ToString();
            statusPanel3.inSttLabel = F1SttPanel.getInCnt().ToString();
            statusPanel3.outSttLabel = F1SttPanel.getOutCnt().ToString();

            statusPanel4.norSttLabel = F2SttPanel.getNorCnt().ToString();
            statusPanel4.errSttLabel = F2SttPanel.getErrCnt().ToString();
            statusPanel4.inSttLabel = F2SttPanel.getInCnt().ToString();
            statusPanel4.outSttLabel = F2SttPanel.getOutCnt().ToString();

            B2SttPanel.resetAllCnt(); B1SttPanel.resetAllCnt(); F1SttPanel.resetAllCnt(); F2SttPanel.resetAllCnt();

            // 무단반출 발생 체크
            //displayStatus2Front();
            */
        }

        private void reqLogData()
        {
            switch (logDataOrder)
            {
                case 0: // 고정형 리더기
                    gridView1.ClearData();
                    Dictionary<string, object> aData = new Dictionary<string, object>();
                    aData.Add("WORKDIV", "119REQ");
                    aData.Add("CRUD_TP", "R");
                    aData.Add("EQ_TYPE", "GAT");

                    JObject response = TCPClient.request(aData);
                    JArray rows = (JArray)response["rows"];

                    if (rows != null)
                    {
                        DataList<Data<string>> datalist = new DataList<Data<string>>();
                        for (int i = 0; i < rows.Count; i++)
                        {
                            JObject row = (JObject)rows[i];
                            var data = new Data<string>();
                            data["STATUS"] = (string)row["STATUS"];
                            data["SEQ"] = (i + 1).ToString();
                            data["EQ_NM"] = (string)row["EQ_NM"];
                            data["STATUS_NM"] = (string)row["STATUS_NM"];
                            data["FW_NM"] = (string)row["FW_NM"];
                            data["IP_ADDR"] = (string)row["IP_ADDR"];
                            datalist.Add(data);
                        }
                        gridView1.SetDataList(datalist);
                    }

                    logDataOrder++;
                    break;
                case 1: // 태그 발행기
                    gridView2.ClearData();
                    aData = new Dictionary<string, object>();
                    aData.Add("WORKDIV", "119REQ");
                    aData.Add("CRUD_TP", "R");
                    aData.Add("EQ_TYPE", "PRT");

                    response = TCPClient.request(aData);
                    rows = (JArray)response["rows"];
                    string strStatus = "";
                    if (rows != null)
                    {
                        DataList<Data<string>> datalist = new DataList<Data<string>>();
                        for (int i = 0; i < rows.Count; i++)
                        {
                            JObject row = (JObject)rows[i];
                            var data = new Data<string>();
                            data["STATUS"] = (string)row["STATUS"];
                            data["SEQ"] = (i + 1).ToString();
                            data["EQ_NM"] = (string)row["EQ_NM"];
                            strStatus = (string)row["STATUS_NM"];
                            strStatus = strStatus.Replace("에러", "단절");
                            data["STATUS_NM"] = strStatus;
                            data["IP_ADDR"] = (string)row["IP_ADDR"];
                            datalist.Add(data);
                        }
                        gridView2.SetDataList(datalist);
                    }
                    logDataOrder++;
                    break;
                case 2: // 휴대형 리더기
                    gridView3.ClearData();
                    aData = new Dictionary<string, object>();
                    aData.Add("WORKDIV", "119REQ");
                    aData.Add("CRUD_TP", "R");
                    aData.Add("EQ_TYPE", "PDA");

                    response = TCPClient.request(aData);
                    rows = (JArray)response["rows"];

                    if (rows != null)
                    {
                        DataList<Data<string>> datalist = new DataList<Data<string>>();
                        for (int i = 0; i < rows.Count; i++)
                        {
                            JObject row = (JObject)rows[i];
                            var data = new Data<string>();
                            data["STATUS"] = (string)row["STATUS"];
                            data["SEQ"] = (i + 1).ToString();
                            data["EQ_NM"] = (string)row["EQ_NM"];
                            strStatus = (string)row["STATUS_NM"];
                            strStatus = strStatus.Replace("에러", "단절");
                            data["STATUS_NM"] = strStatus;
                            data["IP_ADDR"] = (string)row["IP_ADDR"];
                            datalist.Add(data);
                        }
                        gridView3.SetDataList(datalist);
                    }
                    logDataOrder++;
                    break;
                case 3: // 정수점검기
                    gridView4.ClearData();
                    aData = new Dictionary<string, object>();
                    aData.Add("WORKDIV", "119REQ");
                    aData.Add("CRUD_TP", "R");
                    aData.Add("EQ_TYPE", "CNT");

                    response = TCPClient.request(aData);
                    rows = (JArray)response["rows"];

                    if (rows != null)
                    {
                        DataList<Data<string>> datalist = new DataList<Data<string>>();
                        for (int i = 0; i < rows.Count; i++)
                        {
                            JObject row = (JObject)rows[i];
                            var data = new Data<string>();
                            data["STATUS"] = (string)row["STATUS"];
                            data["SEQ"] = (i + 1).ToString();
                            data["EQ_NM"] = (string)row["EQ_NM"];
                            strStatus = (string)row["STATUS_NM"];
                            strStatus = strStatus.Replace("에러", "단절");
                            data["STATUS_NM"] = strStatus;
                            data["IP_ADDR"] = (string)row["IP_ADDR"];
                            datalist.Add(data);
                        }
                        gridView4.SetDataList(datalist);
                    }
                    logDataOrder = 0;
                    break;
            }
        }

        private void MainController_FormClosing(object sender, FormClosingEventArgs e)
        {
            thread.Abort();
            this.Dispose();
        }


        internal void setTooltip(Control c, string p)
        {
            toolTip1.SetToolTip(c, p);
        }

        private void MainController_Load(object sender, EventArgs e)
        {

            //게이트 아이콘 표시
            icons = new Dictionary<String, GateInfo>();

            ScaleImageToFitPictureBox(pictureBox1);
            ScaleImageToFitPictureBox(pictureBox2);
            ScaleImageToFitPictureBox(pictureBox3);
            ScaleImageToFitPictureBox(pictureBox4);
            /**
             * 지하 2층
             */
            icons.Add("RFB2G01",
                new GateInfo(pictureBox1,
                Convert.ToInt32(pictureBox1.Size.Width * 58 / 100),
                Convert.ToInt32(pictureBox1.Size.Height * 70 / 100)
                )
            );

            icons.Add("RFB2G02", new GateInfo(
                pictureBox1,
                Convert.ToInt32(pictureBox1.Size.Width * 45 / 100),
                Convert.ToInt32(pictureBox1.Size.Height * 35 / 100)
                )
            );

            icons.Add("RFB2G03", new GateInfo(
                pictureBox1,
                Convert.ToInt32(pictureBox1.Size.Width * 79 / 100),
                Convert.ToInt32(pictureBox1.Size.Height * 33 / 100)
                )
            );

            icons.Add("RFB2G04", new GateInfo(
                pictureBox1,
                Convert.ToInt32(pictureBox1.Size.Width * 28 / 100),
                Convert.ToInt32(pictureBox1.Size.Height * 36 / 100)
                )
            );

            icons.Add("RFB2G05", new GateInfo(
                pictureBox1,
                Convert.ToInt32(pictureBox1.Size.Width * 24 / 100),
                Convert.ToInt32(pictureBox1.Size.Height * 41 / 100)
                )
            );

            icons.Add("RFB2G06", new GateInfo(
                pictureBox1,
                Convert.ToInt32(pictureBox1.Size.Width * 19.5 / 100),
                Convert.ToInt32(pictureBox1.Size.Height * 65.5 / 100)
                )
            );

            /**
             * 지하 1층
             */
            icons.Add("RFB1G01", new GateInfo(
                pictureBox2,
                Convert.ToInt32(pictureBox2.Size.Width * 23.5 / 100),
                Convert.ToInt32(pictureBox2.Size.Height * 42 / 100)
                )
            );

            icons.Add("RFB1G02", new GateInfo(
                pictureBox2,
                Convert.ToInt32(pictureBox2.Size.Width * 63 / 100),
                Convert.ToInt32(pictureBox2.Size.Height * 83 / 100)
                )
            );

            icons.Add("RFB1G03", new GateInfo(
                pictureBox2,
                Convert.ToInt32(pictureBox2.Size.Width * 47.5 / 100),
                Convert.ToInt32(pictureBox2.Size.Height * 66 / 100)
                )
            );

            icons.Add("RFB1G04", new GateInfo(
                pictureBox2,
                Convert.ToInt32(pictureBox2.Size.Width * 59 / 100),
                Convert.ToInt32(pictureBox2.Size.Height * 32 / 100)
                )
            );

            icons.Add("RFB1G05", new GateInfo(
                pictureBox2,
                Convert.ToInt32(pictureBox2.Size.Width * 51 / 100),
                Convert.ToInt32(pictureBox2.Size.Height * 43 / 100)
                )
            );

            icons.Add("RFB1G06", new GateInfo(
                pictureBox2,
                Convert.ToInt32(pictureBox2.Size.Width * 56.5 / 100),
                Convert.ToInt32(pictureBox2.Size.Height * 32.5 / 100)
                )
            );

            icons.Add("RFB1G07", new GateInfo(
                pictureBox2,
                Convert.ToInt32(pictureBox2.Size.Width * 26 / 100),
                Convert.ToInt32(pictureBox2.Size.Height * 24.5 / 100)
                )
            );

            icons.Add("RFB1G08", new GateInfo(
                pictureBox2,
                Convert.ToInt32(pictureBox2.Size.Width * 47 / 100),
                Convert.ToInt32(pictureBox2.Size.Height * 37.5 / 100)
                )
            );

            icons.Add("RFB1G09", new GateInfo(
                pictureBox2,
                Convert.ToInt32(pictureBox2.Size.Width * 66.5 / 100),
                Convert.ToInt32(pictureBox2.Size.Height * 11.5 / 100)
                )
            );

            icons.Add("RFB1G10", new GateInfo(
                pictureBox2,
                Convert.ToInt32(pictureBox2.Size.Width * 83.5 / 100),
                Convert.ToInt32(pictureBox2.Size.Height * 35 / 100)
                )
            );

            icons.Add("RFB1G11", new GateInfo(
                pictureBox2,
                Convert.ToInt32(pictureBox2.Size.Width * 82 / 100),
                Convert.ToInt32(pictureBox2.Size.Height * 38 / 100)
                )
            );

            /**
             * 지상 1층
             */
            icons.Add("RFF1G01", new GateInfo(
                pictureBox3,
                Convert.ToInt32(pictureBox3.Size.Width * 84.5 / 100),
                Convert.ToInt32(pictureBox3.Size.Height * 65 / 100)
                )
            );

            icons.Add("RFF1G02", new GateInfo(
                pictureBox3,
                Convert.ToInt32(pictureBox3.Size.Width * 78.5 / 100),
                Convert.ToInt32(pictureBox3.Size.Height * 52 / 100)
                )
            );

            icons.Add("RFF1G03", new GateInfo(
                pictureBox3,
                Convert.ToInt32(pictureBox3.Size.Width * 86 / 100),
                Convert.ToInt32(pictureBox3.Size.Height * 41 / 100)
                )
            );

            icons.Add("RFF1G04", new GateInfo(
                pictureBox3,
                Convert.ToInt32(pictureBox3.Size.Width * 78 / 100),
                Convert.ToInt32(pictureBox3.Size.Height * 44.5 / 100)
                )
            );

            icons.Add("RFF1G05", new GateInfo(
                pictureBox3,
                Convert.ToInt32(pictureBox3.Size.Width * 65.5 / 100),
                Convert.ToInt32(pictureBox3.Size.Height * 34 / 100)
                )
            );

            icons.Add("RFF1G06", new GateInfo(
                pictureBox3,
                Convert.ToInt32(pictureBox3.Size.Width * 62.5 / 100),
                Convert.ToInt32(pictureBox3.Size.Height * 40 / 100)
                )
            );

            icons.Add("RFF1G07", new GateInfo(
                pictureBox3,
                Convert.ToInt32(pictureBox3.Size.Width * 61.5 / 100),
                Convert.ToInt32(pictureBox3.Size.Height * 19 / 100)
                )
            );


            icons.Add("RFF1G08", new GateInfo(
                pictureBox3,
                Convert.ToInt32(pictureBox3.Size.Width * 56.5 / 100),
                Convert.ToInt32(pictureBox3.Size.Height * 12 / 100)
                )
            );

            icons.Add("RFF1G09", new GateInfo(
                pictureBox3,
                Convert.ToInt32(pictureBox3.Size.Width * 43 / 100),
                Convert.ToInt32(pictureBox3.Size.Height * 29.5 / 100)
                )
            );

            icons.Add("RFF1G10", new GateInfo(
                pictureBox3,
                Convert.ToInt32(pictureBox3.Size.Width * 39.3 / 100),
                Convert.ToInt32(pictureBox3.Size.Height * 68 / 100)
                )
            );

            icons.Add("RFF1G11", new GateInfo(
                pictureBox3,
                Convert.ToInt32(pictureBox3.Size.Width * 41 / 100),
                Convert.ToInt32(pictureBox3.Size.Height * 38 / 100)
                )
            );

            icons.Add("RFF1G12", new GateInfo(
                pictureBox3,
                Convert.ToInt32(pictureBox3.Size.Width * 30.5 / 100),
                Convert.ToInt32(pictureBox3.Size.Height * 50 / 100)
                )
            );

            icons.Add("RFF1G13", new GateInfo(
                pictureBox3,
                Convert.ToInt32(pictureBox3.Size.Width * 70 / 100),
                Convert.ToInt32(pictureBox3.Size.Height * 63 / 100)
                )
            );

            icons.Add("RFF1G14", new GateInfo(
                pictureBox3,
                Convert.ToInt32(pictureBox3.Size.Width * 18 / 100),
                Convert.ToInt32(pictureBox3.Size.Height * 75 / 100)
                )
            );

            /**
             * 지상 2층
             */
            icons.Add("RFF2G01", new GateInfo(
                pictureBox4,
                Convert.ToInt32(pictureBox4.Size.Width * 9.5 / 100),
                Convert.ToInt32(pictureBox4.Size.Height * 38 / 100)
                )
            );

            icons.Add("RFF2G02", new GateInfo(
                pictureBox4,
                Convert.ToInt32(pictureBox4.Size.Width * 29.2 / 100),
                Convert.ToInt32(pictureBox4.Size.Height * 43 / 100)
                )
            );

            icons.Add("RFF2G03", new GateInfo(
                pictureBox4,
                Convert.ToInt32(pictureBox4.Size.Width * 33 / 100),
                Convert.ToInt32(pictureBox4.Size.Height * 34 / 100)
                )
            );

            icons.Add("RFF2G04", new GateInfo(
                pictureBox4,
                Convert.ToInt32(pictureBox4.Size.Width * 35.5 / 100),
                Convert.ToInt32(pictureBox4.Size.Height * 23 / 100)
                )
            );

            icons.Add("RFF2G05", new GateInfo(
                pictureBox4,
                Convert.ToInt32(pictureBox4.Size.Width * 81.5 / 100),
                Convert.ToInt32(pictureBox4.Size.Height * 52 / 100)
                )
            );

            icons.Add("RFF2G06", new GateInfo(
                pictureBox4,
                Convert.ToInt32(pictureBox4.Size.Width * 83.5 / 100),
                Convert.ToInt32(pictureBox4.Size.Height * 50 / 100)
                )
            );

            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem("시작", (sndr, args) =>
                menuItemStartClick(sender, args)
            ));
            menu.MenuItems.Add(new MenuItem("중지", (sndr, args) =>
                menuItemStopClick(sender, args)
            ));
            menu.MenuItems.Add(new MenuItem("재시작", (sndr, args) =>
                menuItemRestartClick(sender, args)
            ));
            menu.MenuItems.Add(new MenuItem("경광등제어", (sndr, args) =>
                menuItemControlClick(sender, args)
            ));

            foreach (KeyValuePair<string, GateInfo> icon in icons)
            {
                icon.Value.setGateID(icon.Key);
                icon.Value.setContextMenu(menu);
                icon.Value.setParent(this);
            }

            unauthOutPanelList.AutoScroll = false;
            unauthOutPanelList.VerticalScroll.Enabled = false;
            unauthOutPanelList.VerticalScroll.Visible = false;
            unauthOutPanelList.VerticalScroll.Maximum = 0;
            unauthOutPanelList.AutoScroll = true;
        }

        private void ScaleImageToFitPictureBox(PictureBox pictureBox)
        {
            if (pictureBox.Image != null)
            {
                pictureBox.SizeMode = PictureBoxSizeMode.Normal; // 원래 이미지 크기로 설정

                // 이미지 크기가 PictureBox 크기보다 큰 경우 이미지를 PictureBox에 맞게 스케일링
                if (pictureBox.Image.Width > pictureBox.Width || pictureBox.Image.Height > pictureBox.Height)
                {
                    pictureBox.Image = new System.Drawing.Bitmap(pictureBox.Image, pictureBox.Width - (pictureBox.Width / 25), pictureBox.Height);
                }

                pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // 이미지 크기를 PictureBox에 맞게 조절
            }
        }

        private void UpdateConnectionStatus(bool bStatusConnection, bool bStatusPAMS, bool bStatusMW, bool bStatusEMS)
        {
            if (bStatusConnection)
                this.pictureBoxConnection.Image = global::RFID_CONTROLLER.Properties.Resources.img_head_btn_I_ov;
            else
                this.pictureBoxConnection.Image = global::RFID_CONTROLLER.Properties.Resources.img_head_btn_I;

            if (bStatusPAMS)
                this.pictureBoxPAMS.Image = global::RFID_CONTROLLER.Properties.Resources.img_head_btn_P_ov;
            else
                this.pictureBoxPAMS.Image = global::RFID_CONTROLLER.Properties.Resources.img_head_btn_P;

            if (bStatusMW)
                this.pictureBoxMW.Image = global::RFID_CONTROLLER.Properties.Resources.img_head_btn_M_ov;
            else
                this.pictureBoxMW.Image = global::RFID_CONTROLLER.Properties.Resources.img_head_btn_M;

            if (bStatusEMS)
                this.pictureBoxEMS.Image = global::RFID_CONTROLLER.Properties.Resources.img_head_btn_C_ov;
            else
                this.pictureBoxEMS.Image = global::RFID_CONTROLLER.Properties.Resources.img_head_btn_C;

        }

        private void mapScaleUp(object sender, EventArgs e)
        {

            PictureBox pictureBox = sender as PictureBox;
            string pictureBoxName = pictureBox.Name;

            switch (pictureBoxName)
            {
                case "pictureBox1":
                    Form mapB2FForm = new MapB2F();
                    mapB2FForm.TopLevel = false;
                    mapB2FForm.Dock = DockStyle.Fill;
                    mapPanel.Controls.Add(mapB2FForm);
                    mapB2FForm.Show();
                    mapB2FForm.BringToFront();
                    break;
                case "pictureBox2":
                    Form mapB1FForm = new MapB1F();
                    mapB1FForm.TopLevel = false;
                    mapB1FForm.Dock= DockStyle.Fill;
                    mapPanel.Controls.Add(mapB1FForm); 
                    mapB1FForm.Show();
                    mapB1FForm.BringToFront();
                    break;
                case "pictureBox3":
                    Form map1FForm = new Map1F();
                    map1FForm.TopLevel = false;
                    map1FForm.Dock = DockStyle.Fill;
                    mapPanel.Controls.Add(map1FForm);
                    map1FForm.Show();
                    map1FForm.BringToFront();
                    break;
                case "pictureBox4":
                    Form map2FForm = new Map2F();
                    map2FForm.TopLevel = false;
                    map2FForm.Dock = DockStyle.Fill;
                    mapPanel.Controls.Add(map2FForm);
                    map2FForm.Show();
                    map2FForm.BringToFront();
                    break;
            }
        }

        private void equipmentManagerBtn_Click(object sender, EventArgs e)
        {
            // 경고 이미지, 타이머 무효화
            if (blinkTimer.Enabled)
            {
                monitoringTableLayoutPanel.BackColor = Color.FromArgb(2, 11, 35);
                logTableLayoutPanel.BackColor = SystemColors.Control;
                xtraTabControl1.BringToFront();
                alertImgPanel.SendToBack();
                alertImgPanel.Visible = false;
                blinkTimer.Stop();
            }

            EquipmentManager equipmentManager = new EquipmentManager();
            equipmentManager.StartPosition = FormStartPosition.CenterScreen;
            equipmentManager.ShowDialog();
        }

        private void SWManagerBtn_Click(object sender, EventArgs e)
        {
            // 경고 이미지, 타이머 무효화
            if (blinkTimer.Enabled)
            {
                monitoringTableLayoutPanel.BackColor = Color.FromArgb(2, 11, 35);
                logTableLayoutPanel.BackColor = SystemColors.Control;
                xtraTabControl1.BringToFront();
                alertImgPanel.SendToBack();
                alertImgPanel.Visible = false;
                blinkTimer.Stop();
            }

            SWManager swManager = new SWManager();
            swManager.StartPosition = FormStartPosition.CenterScreen;
            swManager.ShowDialog();
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("프로그램을 종료하시겠습니까?", "종료 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
                == DialogResult.Yes)
                this.Close();
        }

        private void unauthOutPanelList_Resize(object sender, EventArgs e)
        {

            if (unauthOutPanelList.HorizontalScroll.Visible)
            {
                Control unauthOutPanelList = sender as Control;
                foreach(Control control in unauthOutPanelList.Controls)
                {
                    if(control is UnauthOutPanel)
                    {
                        UnauthOutPanel unauthOutPanel = (UnauthOutPanel) control;
                        unauthOutPanel.shrinkTableLayout();
                    }
                }
            }
            else
            {
                Control unauthOutPanelList = sender as Control;
                foreach (Control control in unauthOutPanelList.Controls)
                {
                    if (control is UnauthOutPanel)
                    {
                        UnauthOutPanel unauthOutPanel = (UnauthOutPanel)control;
                        unauthOutPanel.setOriginTableLayout();
                    }
                }
            }
        }

        private void addInoutPanel(String[] inoutList)
        {
            string dtTm = inoutList[0];
            int gateSplitIndex = inoutList[1].IndexOf('G');
            string floor = inoutList[1].Substring(0, gateSplitIndex);
            string gateId = inoutList[1].Substring(gateSplitIndex);
            string mgmtNo = inoutList[2];
            string inOut = inoutList[3];

            // 모니터링 하단 무단반출 내역 패널 추가


            Dictionary<String, String> tmpUnauthPanelInfo = new Dictionary<String, String>();
            tmpUnauthPanelInfo.Add("dtTm", dtTm);
            tmpUnauthPanelInfo.Add("floor", floor);
            tmpUnauthPanelInfo.Add("gateId", gateId);
            tmpUnauthPanelInfo.Add("mgmtNo", mgmtNo);
            tmpUnauthPanelInfo.Add("inOut", inOut);

            inOutPanelList.Add(tmpUnauthPanelInfo);
            
            /*
            Color panelColor = tmpUnauthPanelInfo["inOut"].Equals("반출") ? Color.Red : Color.SteelBlue;
            UnauthOutPanel unauthOutPanel = new UnauthOutPanel(panelColor, tmpUnauthPanelInfo);
            unauthOutPanel.Dock = DockStyle.Left;
            unauthOutPanelList.Controls.Add(unauthOutPanel);
            */

            /*
            for (int i=0; i<3; i++)
            {
                UnauthOutPanel unauthOutPanel = new UnauthOutPanel(colors[colorsIndex], tmpLocationList[tmpLocationListIndex]);
                unauthOutPanel.Dock = DockStyle.Left;
                unauthOutPanelList.Controls.Add(unauthOutPanel);
            }
            */

            // monitoringTableLayoutPanel 다시 그리기
            //displayStatus2Front();
            //monitoringTableLayoutPanel.Invalidate();
        }

        private ArrayList sortArrayList(ArrayList arrayList, string key)
        {

            // LINQ을 사용하여 ArrayList의 Dictionary를 Date 값으로 정렬
            var sortedList = arrayList.Cast<Dictionary<string, string>>()
                                       .OrderBy(dict => DateTime.Parse(dict[key]))
                                       .ToList();

            // 정렬된 List를 다시 ArrayList로 변환
            ArrayList resultArrayList = new ArrayList(sortedList);

            return resultArrayList;
        }

        private void displayStatus2Front()
        {
            if (inOutPanelList.Count > 0)
            {
                inOutPanelList = sortArrayList(inOutPanelList, "dtTm");
                
                for(int i=0; i<inOutPanelList.Count; i++)
                {
                    if (inOutPanelList[i] is Dictionary<string, string> inOutPanel)
                    {
                        Color panelColor = inOutPanel["inOut"].Equals("반출") ? Color.Red : Color.SteelBlue;
                        UnauthOutPanel unauthOutPanel = new UnauthOutPanel(panelColor, inOutPanel);
                        unauthOutPanel.Dock = DockStyle.Left;
                        unauthOutPanelList.Controls.Add(unauthOutPanel);
                    }

                    if(i < inOutPanelList.Count - 1)
                    {
                        DevExpress.XtraEditors.SvgImageBox arrowImg = new DevExpress.XtraEditors.SvgImageBox();
                        arrowImg.Dock = System.Windows.Forms.DockStyle.Left;
                        arrowImg.Location = new System.Drawing.Point(0, 0);
                        arrowImg.Size = new System.Drawing.Size(50, 85);
                        arrowImg.SvgImage = global::RFID_CONTROLLER.Properties.Resources.actions_arrow3right;
                        arrowImg.TabIndex = 0;
                        unauthOutPanelList.Controls.Add(arrowImg);
                    }
                }

                monitoringTableLayoutPanel.BackColor = Color.Red;
                logTableLayoutPanel.BackColor = Color.Red;

                // 경고 이미지
                alertImgPanel.Location = new Point(
                    (mapPanel.Width - alertImgPanel.Width) / 2,
                    (mapPanel.Height - alertImgPanel.Height) / 2
                );
                xtraTabControl1.SendToBack();
                alertImgPanel.BringToFront();

                // 타이머 설정
                if (!blinkTimer.Enabled)
                {
                    alertImgPanel.Visible = true;
                    blinkTimer.Start();
                    
                }
                //StopSiren();
                RingingSiren();
            }
            else
            {
                monitoringTableLayoutPanel.BackColor = Color.FromArgb(2, 11, 35);
                logTableLayoutPanel.BackColor = SystemColors.Control;

                // 경고 이미지 숨기기
                //xtraTabControl1.BringToFront();
                alertImgPanel.SendToBack();
                
                // 타이머 설정
                if (blinkTimer.Enabled)
                {
                    alertImgPanel.Visible = false;
                    blinkTimer.Stop();
                    
                }
                StopSiren();
                //RingingSiren();
            }
            
        }

        private void BlinkTimer_Tick(object sender, EventArgs e)
        {
            alertImgPanel.Visible = (alertImgPanel.Visible) ? false : true;
        }

        public static Dictionary<String, GateInfo> getIcons()
        {
            return icons;
        }

        private void xtraTabControl1_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (xtraTabControl1.SelectedTabPageIndex == 1)
            {
                // 경고 이미지, 타이머 무효화
                if (blinkTimer.Enabled)
                {
                    monitoringTableLayoutPanel.BackColor = Color.FromArgb(2, 11, 35);
                    logTableLayoutPanel.BackColor = SystemColors.Control;
                    xtraTabControl1.BringToFront();
                    alertImgPanel.SendToBack();
                    alertImgPanel.Visible = false;
                    blinkTimer.Stop();
                }
            }
        }
    }

    public class sttPanelCnt
    {
        private int totalCnt = 0;
        private int errCnt, norCnt, inCnt, outCnt;

        public sttPanelCnt(int totalCnt)
        {
            this.totalCnt = totalCnt;
            errCnt = norCnt = inCnt = outCnt = 0;
        }

        public void resetAllCnt()
        {
            errCnt = norCnt = inCnt = outCnt = 0;
        }

        public void addErrCnt()
        {
            errCnt++;
        }

        public void addNorCnt()
        {
            norCnt++;
        }

        public void addInCnt()
        {
            inCnt++;
        }

        public void addOutCnt()
        {
            outCnt++;
        }

        public int getErrCnt()
        {
            return errCnt;
        }

        public int getNorCnt()
        {
            return norCnt;
        }

        public int getInCnt()
        {
            return inCnt;
        }

        public int getOutCnt()
        {
            return outCnt;
        }

        public void cntCheck()
        {
            int responseCnt = norCnt + errCnt + inCnt + outCnt;
            if(totalCnt > responseCnt)
            {
                errCnt += totalCnt - responseCnt;
            }
        }

    }

    public class GateInfo
    {
        private MainController parent;
        private string gateID;
        private static Image iconNOR = Resources.img_circle_g;
        private static Image iconERR = Resources.img_circle_o;
        private static Image iconOUT = Resources.img_circle_r;
        private static Image iconIN = Resources.img_circle_b;

        protected PictureBox icon
        {
            get; set;
        }

        public GateInfo(Control c, int x, int y)
        {
            icon = new PictureBox();
            icon.BackgroundImageLayout = ImageLayout.None;
            icon.SizeMode = PictureBoxSizeMode.AutoSize;
            icon.Location = new Point(x, y);

            c.Controls.Add(icon);

            icon.Click += new EventHandler(icon_Click);
            icon.MouseEnter += new EventHandler(icon_Enter);

            updateUI(iconERR);
        }

        public void setGateID(String gateID)
        {
            this.gateID = gateID;
        }

        public void icon_Click(object sender, EventArgs e)
        {
            parent.gateID = this.gateID;
            icon.ContextMenu.Show(icon, new Point(5, 5));
        }

        public void icon_Enter(object sender, EventArgs e)
        {
            parent.setTooltip(icon, this.gateID.Substring(this.gateID.Length - 5) + "\n" + Utils.gateIdNameDic[this.gateID]);
        }

        public void setContextMenu(ContextMenu cs)
        {
            icon.ContextMenu = cs;
        }

        public void updateUI(Image status)
        {
            icon.Image = status;
            icon.BackColor = Color.Transparent;
        }

        public void updateUI(String status)
        {
            if(status == "NOR") updateUI(iconNOR);
            else if(status == "IN") updateUI(iconIN);
            else if(status == "OUT") updateUI(iconOUT);
            else updateUI(iconERR);
        }
        public void setParent(MainController c)
        {
            this.parent = c;
        }
    }
    
}

public static class ControlExtensions
{
    public static void DoubleBuffering(this Control control, bool enable)
    {
        var method = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(control, new object[] { ControlStyles.OptimizedDoubleBuffer, enable });
    }
}