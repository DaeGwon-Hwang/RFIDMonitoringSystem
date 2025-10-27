using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RFIDTagWriter
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
			SystemLog = new Log();
			SystemLog.FilePath = "Logs";
			SystemLog.FileName = "SYS";
			SystemLog.DateAppendToFileName = true;
			ApplicationLog = new Log();
			ApplicationLog.FilePath = "Logs";
			ApplicationLog.FileName = "APP";
			ApplicationLog.DateAppendToFileName = true;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1()); 
			//Application.Run(new MainForm());
		}
	}
}
