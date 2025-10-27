using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;

namespace BPNS.TransitiveMiddleware
{
    class Utils
    {
        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileInt(string section, string key, int def, string filePath);

        public const string DEF_INI_FILENAME = "config.ini";

        /// <summary>
        /// 연계서버 IP 
        /// </summary>
        public static string HOST_IP = "127.0.0.1";//"192.168.200.21";
        /// <summary>
        /// 연계서버 Port no
        /// </summary>
        public static int HOST_PORT = 12012;

        /// <summary>
        /// 연계 단절 판단 시간(초)
        /// </summary>
        public static int DISCONNECTION_CHECK_TIME = 60;

        public static int GetIniValueInt(string section, string key, int Default, string strFName)
        {
            string filePath = Application.StartupPath + "\\" + strFName;

            int nCount = GetPrivateProfileInt(section, key, Default, filePath);
            return nCount;
        }

        public static string GetIniValue(string section, string key, string Default, string strFName)
        {
            string filePath = Application.StartupPath + "\\" + strFName;

            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, Default, temp, 255, filePath);
            return temp.ToString();
        }

        public static void SetIniValue(string section, string key, string val, string strFName)
        {
            string filePath = Application.StartupPath + "\\" + strFName;

            WritePrivateProfileString(section, key, val, filePath);
        }

        public static void comboBoxByKey(ComboBox combo, string key)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (((KeyValuePair<string, string>)combo.Items[i]).Key == key)
                {
                    combo.SelectedIndex = i;
                    break;
                }
            }
        }

        public static void setComboBox(ComboBox combo, Dictionary<string, string> source)
        {
            try
            {
                combo.DataSource = new BindingSource(source, null);
                combo.DisplayMember = "Value";
                combo.ValueMember = "Key";
            }
            catch { }
        }

        public static string parseIPAddress(string data)
        {
            String result = "";
            String[] datas = data.Split('.');

            if (datas != null)
            {
                for (int i = 0; i < datas.Length; i++)
                {
                    String tmp = datas[i];

                    for (int j = tmp.Length; j < 3; j++)
                    {
                        tmp = "0" + tmp;
                    }

                    result += tmp;

                    if (i < (datas.Length - 1))
                    {
                        result += ".";
                    }
                }
            }

            return result;
        }

        public static string fileToBase64(String filename)
        {
            string result = "";
            FileStream fs = null;

            try
            {
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                byte[] buff = new Byte[fs.Length];
                fs.Read(buff, 0, buff.Length);

                result = Convert.ToBase64String(buff, 0, buff.Length);
                byte[] buff_test = Convert.FromBase64String(result);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }

            return result;
        }


    }
    //ListView 의 특정 컬럼의 값을 기반으로 정렬을 수행토록 정의

    class ListViewItemComparer : IComparer
    {

        private int col;

        public string sort = "asc";

        public ListViewItemComparer()
        {

            col = 0;

        }

        public ListViewItemComparer(int column, string sort)
        {

            col = column;

            this.sort = sort;

        }

        public int Compare(object x, object y)
        {

            if (sort == "asc")

                return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);

            else

                return String.Compare(((ListViewItem)y).SubItems[col].Text, ((ListViewItem)x).SubItems[col].Text);

        }

    }
}
