using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManageProc
{
    public partial class frmManageProc : Form
    {
        private Thread _thMain;         // 쓰레드


        ManualResetEvent _pauseEvent = new ManualResetEvent(false);

        private bool bExist = false;


        public frmManageProc()
        {
            InitializeComponent();
        }

        private void frmManageProc_Load(object sender, EventArgs e)
        {
            _thMain = new Thread(new ThreadStart(DataTransfer));
            _thMain.IsBackground = true;
            _thMain.Start();

            _pauseEvent.Set();

            Program.AppLog.Write(this, "프로그램 시작");
            Program.AppLog.Write(this, "프로그램 시작 체크일자" + dteLastRestart.ToString("yyyyMMdd"));
        }

        private void DataTransfer()
        {
            while (_pauseEvent.WaitOne())
            {
                try
                {
                    Program.AppLog.Write(this, "DataTransfer 처리");
                    ReStart();

                    Thread.Sleep(1000 * 60);
                }
                catch (Exception ex)
                {
                    Program.AppLog.Write(this, "데이터 처리 오류");
                    Program.SysLog.Write(this, "데이터 처리 오류", ex);
                }
            }
        }

        DateTime dteLastRestart = DateTime.Now.AddDays(-1);
        string _processName = "BPNS.TransitiveMiddleware";   //프로세스 이름

        private void ReStart()
        {
            //프로그램 재실행 처리

            //TimeSpan ts = DateTime.Now - dteLastEsabRestart;
            string sDate = DateTime.Now.ToString("yyyyMMdd");
            if (sDate != dteLastRestart.ToString("yyyyMMdd"))
            {
                bool bESABReStart = false;
                foreach (Process process in Process.GetProcesses())
                {
                    // "exe_name"라는 이름을 가진 프로세스가 존재하면 true를 리턴한다.
                    if (process.ProcessName.StartsWith(_processName))
                    {
                        if (process.MainModule.FileName.IndexOf(_processName) > 0)
                        {
                            process.Kill();

                            AddLogList("Process 처리1");

                            Thread.Sleep(1000 * 3);
                            //D:\실행파일\중계서버
                            //string sExePgm = @"D:\Project\배전반실시간생산관리\소스\배전반\24.04.05\BPNS.RFID_MW\BPNS.RFID_MW\bin\Debug\" + _processName + ".exe";
                            string sExePgm = @"D:\실행파일\중계서버\" + _processName + ".exe";
                            Process.Start(sExePgm);

                            AddLogList("Process 처리2");

                            //Program.SysLog.Write(this, "프로세스 실행" + "(" + _MachineIP + ")" + " : " + sExePgm);

                            dteLastRestart = DateTime.Now;
                            bESABReStart = true;
                        }
                    }
                }

                if (!bESABReStart)
                {
                    //string sExePgm = @"D:\Project\배전반실시간생산관리\소스\배전반\24.04.05\BPNS.RFID_MW\BPNS.RFID_MW\bin\Debug\" + _processName + ".exe";
                    string sExePgm = @"D:\실행파일\중계서버\" + _processName + ".exe";
                    Process.Start(sExePgm);

                    AddLogList("Process 처리3");

                    //Program.SysLog.Write(this, "프로세스 실행" + "(" + _MachineIP + ")" + " : " + sExePgm);

                    dteLastRestart = DateTime.Now;
                    bESABReStart = true;
                }
            }
            
        }

        delegate void SafetyAddLogList(string logMsg);
        int MaxLogListboxCount = 30;

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
                    while (logListBox.Items.Count >= MaxLogListboxCount)
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

        private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bExist = true;
            this.Close();
        }

        private void frmManageProc_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bExist)
            {
                Dispose();
                Program.AppLog.Write(this, "프로그램 종료");
            }
            else
            {
                e.Cancel = true;
                this.Visible = false;
            }
        }

        private void frmManageProc_Shown(object sender, EventArgs e)
        {
            this.Visible = false;
        }
    }
}
