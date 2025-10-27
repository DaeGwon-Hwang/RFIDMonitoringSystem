using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BPNS.COM;

namespace BPNS.EdgeMW
{
    static class Program
    {
        private static Log SystemLog;

        public static Log SysLog
        {
            get { return Program.SystemLog; }
            //            set { Program.SystemLog = value; }
        }
        private static Log ApplicationLog;

        public static Log AppLog
        {
            get { return Program.ApplicationLog; }
            //            set { Program.ApplicationLog = value; }
        }

        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                SystemLog = new Log();
                SystemLog.FilePath = ProgramSettings.LOG_LogFilePath;
                SystemLog.FileName = ProgramSettings.LOG_SystemLogFilename;
                SystemLog.DateAppendToFileName = ProgramSettings.LOG_bDateAppendToLogFileName;
                ApplicationLog = new Log();
                ApplicationLog.FilePath = ProgramSettings.LOG_LogFilePath;
                ApplicationLog.FileName = ProgramSettings.LOG_ApplicationLogFilename;
                ApplicationLog.DateAppendToFileName = ProgramSettings.LOG_bDateAppendToLogFileName;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Program.SysLog.Write("Main Error : " + ex.ToString());
            }
        }
    }
}
