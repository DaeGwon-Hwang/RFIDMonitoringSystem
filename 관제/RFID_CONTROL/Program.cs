using RFID_CONTROLLER.Frm;
using System;
using System.Windows.Forms;

namespace RFID_CONTROLLER
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //중복실행방지
            try
            {
                if (IsRunningAppCheck() == false)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainController());
                }
                else
                {
                    MessageBox.Show("이미 프로그램이 실행 중 입니다.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        public static bool IsRunningAppCheck() 
        {
            System.Diagnostics.Process Process = System.Diagnostics.Process.GetCurrentProcess();
            string ProcName = Process.ProcessName;
            if (System.Diagnostics.Process.GetProcessesByName(ProcName).Length > 1) 
            { return true; } 
            else { return false; } 
        }
    }
}
