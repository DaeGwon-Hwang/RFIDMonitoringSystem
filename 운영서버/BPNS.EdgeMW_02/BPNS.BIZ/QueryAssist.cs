using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BPNS.COM;
using BPNS.DAC;
using log4net;
using System.Data;

namespace BPNS.BIZ
{
    public class QueryAssist
    {

        #region Const
        private static readonly ILog Logger = LogManager.GetLogger(typeof(QueryAssist));
        #endregion

        #region Select       

        public DataSet ExecuteDataSet(string connectionStringName, string commandText, ParameterCollection parameters)
        {
            DataSet dataSet = null;

            try
            {
                using (IDatabase db = DbProviderFactory.GetInstance(connectionStringName))
                {
                    dataSet = db.ExecuteDataSet(commandText, parameters);

                    db.Dispose();
                    db.CloseConnection();
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.Message);
                //MessageBox.Show(ex.Message);
            }

            return dataSet;
        }

        /// <summary>
        /// 모니터링 DB연결 실패시 특정시간후 메세지종료
        /// </summary>
        /// <param name="connectionStringName"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public class AutoClosingMessageBox
        {
            System.Threading.Timer _timeoutTimer;
            string _caption;
            AutoClosingMessageBox(string text, string caption, int timeout)
            {
                _caption = caption;
                _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                    null, timeout, System.Threading.Timeout.Infinite);
                //using (_timeoutTimer)
                    //MessageBox.Show(text, caption);
            }
            public static void Show(string text, string caption, int timeout)
            {
                new AutoClosingMessageBox(text, caption, timeout);
            }
            void OnTimerElapsed(object state)
            {
                IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
                if (mbWnd != IntPtr.Zero)
                    SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                _timeoutTimer.Dispose();
            }
            const int WM_CLOSE = 0x0010;
            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        }

        public DataSet ExecuteDataSet_Monitor(string connectionStringName, string commandText, ParameterCollection parameters)
        {

            DataSet dataSet = null;

            try
            {
                using (IDatabase db = DbProviderFactory.GetInstance(connectionStringName))
                {
                    dataSet = db.ExecuteDataSet(commandText, parameters);
                    db.Dispose();
                    db.CloseConnection();
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.Message);
                //MessageBox.Show(ex.Message);
                AutoClosingMessageBox.Show(ex.Message, "", 2000);

            }

            return dataSet;
        }
        #endregion

        #region ExecuteNonQuery
        public OutParameter ExecuteNonQuery(string connectionStringName, string commandText, ParameterCollection parameters)
        {
            OutParameter outParameter = new OutParameter();

            try
            {
                using (IDatabase db = DbProviderFactory.GetInstance(connectionStringName))
                {
                    OutParameter currentOutParameter = new OutParameter();
                    currentOutParameter = db.ExecuteNonQuery(commandText, parameters);

                    //SetOutPameterList(ref outParameter, currentOutParameter);

                    db.Dispose();
                    db.CloseConnection();
                }
            }
            catch (Exception ex)
            {
                
                Logger.Debug(ex.Message);
                //MessageBox.Show("excuteNonQuery" + commandText + "\r\n" +ex.Message);

                throw ex;
            }

            return outParameter;
        }

        public OutParameter ExecuteNonQuery(string connectionStringName, string commandText, ParameterCollection parameters, DataTable dt)
        {
            OutParameter outParameter = new OutParameter();

            try
            {
                using (IDatabase db = DbProviderFactory.GetInstance(connectionStringName))
                {
                    OutParameter currentOutParameter = new OutParameter();

                    int i = 0;
                    foreach (DataRow dataRow in dt.Rows)
                    {
                        if (!dataRow.Table.Columns.Contains("CHK") || dataRow["CHK"].ToString().Equals("Y"))
                        {
                            dataRow.ClearErrors();

                            foreach (Parameter param in parameters)
                            {
                                if (param.isGridColumn)
                                {
                                    param.Value = dataRow[param.ParamName];
                                }
                            }


                            currentOutParameter = db.ExecuteNonQuery(commandText, parameters);
                            currentOutParameter["INDEX"] = currentOutParameter["O_APP_CODE"].ToString() == "0" ? i : -1;
                            SetOutPameterList(ref outParameter, currentOutParameter);

                            if (currentOutParameter["O_APP_CODE"].ToString() != "0")
                            {
                                dataRow.RowError = currentOutParameter["O_APP_MSG"].ToString();
                            }
                            else
                            {
                                if (dataRow.Table.Columns.Contains("SAVE_FLAG"))
                                    dataRow["SAVE_FLAG"] = string.Empty;
                                if (dataRow.Table.Columns.Contains("CHK"))
                                    dataRow["CHK"] = "N";
                            }
                        }

                        i++;
                    }

                    db.Dispose();
                    db.CloseConnection();

                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.Message);
                //MessageBox.Show(ex.Message);
            }

            return outParameter;
        }

        public object ExecuteFunction(string connectionStringName, string commandText, ParameterCollection parameters)
        {
            object result = null;

            try
            {
                using (IDatabase db = DbProviderFactory.GetInstance(connectionStringName))
                {
                    result = db.ExecuteFunction(commandText, parameters);

                    db.Dispose();
                    db.CloseConnection();
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.Message);
                //MessageBox.Show(ex.Message);
            }

            return result;
        }

        public void ExecuteAccesLog(string connectionStringName, string commandText, ParameterCollection parameters)
        {
            try
            {
                using (IDatabase db = DbProviderFactory.GetInstance(connectionStringName))
                {
                    db.ExecuteNonQuery(commandText, parameters);
                    db.Dispose();
                    db.CloseConnection();
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.Message);
            }
        }
        #endregion

        #region Helper
        public void SetOutPameterList(ref OutParameter outParameter, OutParameter currentOutParameter)
        {
            Dictionary<string, object>.Enumerator enumerator = currentOutParameter.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (outParameter.ContainsKey(enumerator.Current.Key))
                {
                    object[] outParam = outParameter[enumerator.Current.Key] as object[];
                    object[] temp = new object[outParam.Length + 1];

                    Array.Copy(outParam, temp, outParam.Length);
                    temp[temp.Length - 1] = enumerator.Current.Value;
                    outParameter[enumerator.Current.Key] = temp;
                }
                else
                    outParameter.Add(enumerator.Current.Key, new object[] { enumerator.Current.Value });
            }
        }
        #endregion
    }
}
