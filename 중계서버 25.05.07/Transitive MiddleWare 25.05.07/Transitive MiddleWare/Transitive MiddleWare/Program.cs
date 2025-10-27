using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace BPNS.TransitiveMiddleware
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

        public static CRC16 crc16;

        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {

            SystemLog = new Log();
            SystemLog.FilePath = Properties.Settings.Default.LOG_LogFilePath;
            SystemLog.FileName = Properties.Settings.Default.LOG_SystemLogFilename;
            SystemLog.DateAppendToFileName = Properties.Settings.Default.LOG_bDateAppendToLogFileName;
            ApplicationLog = new Log();
            ApplicationLog.FilePath = Properties.Settings.Default.LOG_LogFilePath;
            ApplicationLog.FileName = Properties.Settings.Default.LOG_ApplicationLogFilename;
            ApplicationLog.DateAppendToFileName = Properties.Settings.Default.LOG_bDateAppendToLogFileName;
            crc16 = new CRC16();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

        }
    }
}
