using System;
using System.Text;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace BPNS.EdgeMW
{
    public class DatabaseManagementMSSQL : IDisposable, IDatabaseManagement
    {
        private SqlConnection Connection = null;
        private volatile object oDatabaseCO = new object();

        public bool bConnected
        {
            get
            {
                if (Connection != null && Connection.State == ConnectionState.Open)
                    return true;
                else
                    return false;
            }
        }

        public bool Connect()
        {
            string connectionString;
            bool bResult = false;
            try
            {
                lock (oDatabaseCO)
                {
                    // SQL Server 연결 문자열 예시 (필요 시 수정)
                    // connectionString = "Server=192.168.0.10;Database=RFIDPAMS;User Id=rfidpams;Password=rfidpams;";
                    // 또는 Windows 인증을 사용할 경우:
                    // connectionString = "Server=localhost;Database=RFIDPAMS;Integrated Security=True;";

                    connectionString = "Server=192.168.200.29;Database=RFIDPAMS;User Id=rfidpams;Password=rfidpams;";

                    if (Connection != null)
                    {
                        Connection.Close();
                        Program.AppLog.Write("데이터베이스 연결종료(기존 연결 리소스 제거):[MSSQL]");
                        Connection = null;
                    }
                    Connection = new SqlConnection(connectionString);
                    Connection.Open();
                    bResult = true;
                    Program.AppLog.Write("데이터베이스 연결:[MSSQL]");
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write("데이터베이스 연결 오류[" + e.Message + "]");
                Program.SysLog.Write(this, "데이터베이스 연결 오류", e);
            }
            return bResult;
        }

        public void Disconnect()
        {
            try
            {
                lock (oDatabaseCO)
                {
                    if (Connection != null)
                    {
                        Connection.Close();
                        Connection.Dispose();
                        Connection = null;
                        Program.AppLog.Write("데이터베이스 연결종료:[MSSQL]");
                    }
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write("데이터베이스 연결종료 오류[" + e.Message + "]");
                Program.SysLog.Write(this, "데이터베이스 연결종료 오류", e);
            }
        }

        public void Reconnect()
        {
            try
            {
                if (!bConnected)
                    Connect();
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "데이터베이스 재연결 오류[" + e.Message + "]");
                Program.SysLog.Write(this, "데이터베이스 재연결 오류", e);
            }
        }

        public int ExecuteProcedure(string pstrCommandText, Hashtable paramHashtable, int nTimeout)
        {
            int nReturnValue = 0;
            try
            {
                Reconnect();
                if (!bConnected)
                {
                    Program.AppLog.Write(this, "DB 연결이 불가능 합니다.[commandText:" + pstrCommandText + "]" + makeHashtableToString(paramHashtable));
                    return -1000;
                }

                lock (oDatabaseCO)
                {
                    using (SqlCommand command = new SqlCommand(pstrCommandText, Connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = nTimeout;

                        foreach (string key in paramHashtable.Keys)
                        {
                            object val = paramHashtable[key] ?? DBNull.Value;
                            command.Parameters.AddWithValue(key, val);
                        }

                        // 출력 파라미터 예시
                        SqlParameter appCode = new SqlParameter("@O_APP_CODE", SqlDbType.VarChar, 100)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(appCode);

                        SqlParameter appMsg = new SqlParameter("@O_APP_MSG", SqlDbType.VarChar, 500)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(appMsg);

                        nReturnValue = command.ExecuteNonQuery();

                        paramHashtable["O_APP_CODE"] = command.Parameters["@O_APP_CODE"].Value.ToString();
                        paramHashtable["O_APP_MSG"] = command.Parameters["@O_APP_MSG"].Value.ToString();
                    }
                }
            }
            catch (SqlException se)
            {
                Program.AppLog.Write(this, $"SQL 오류[{pstrCommandText}]: {se.Message}");
                Program.SysLog.Write(this, "SQL 오류", se);
                nReturnValue = -19;
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "프로시저 실행 오류[" + e.Message + "]");
                Program.SysLog.Write(this, "프로시저 실행 오류", e);
                nReturnValue = -9999;
            }

            return nReturnValue;
        }

        public DataSet ExecuteSelectProcedure(string pstrCommandText, Hashtable paramHashtable, int nTimeout)
        {
            DataSet dataSet = new DataSet();

            try
            {
                Reconnect();
                if (!bConnected)
                {
                    Program.AppLog.Write(this, "DB 연결이 불가능 합니다.[commandText:" + pstrCommandText + "]" + makeHashtableToString(paramHashtable));
                    return null;
                }

                lock (oDatabaseCO)
                {
                    using (SqlCommand command = new SqlCommand(pstrCommandText, Connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = nTimeout;

                        foreach (string key in paramHashtable.Keys)
                        {
                            object val = paramHashtable[key] ?? DBNull.Value;
                            command.Parameters.AddWithValue(key, val);
                        }

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dataSet);
                    }
                }
            }
            catch (SqlException se)
            {
                Program.AppLog.Write(this, "SQL 오류[" + se.Message + "]");
                Program.SysLog.Write(this, "SQL 오류", se);
                return null;
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "프로시저 실행(Select) 오류![" + e.Message + "]");
                Program.SysLog.Write(this, "프로시저 실행(Select) 오류!", e);
                return null;
            }

            return dataSet;
        }

        public DataSet ExecuteSelectQuery(string pstrCommandText, int nTimeout)
        {
            DataSet dataSet = new DataSet();
            try
            {
                Reconnect();
                if (!bConnected)
                {
                    Program.AppLog.Write(this, "DB 연결이 불가능 합니다.[commandText:" + pstrCommandText + "]");
                    return null;
                }

                lock (oDatabaseCO)
                {
                    using (SqlCommand command = new SqlCommand(pstrCommandText, Connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = nTimeout;
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dataSet);
                    }
                }
            }
            catch (SqlException se)
            {
                Program.AppLog.Write(this, "SQL 오류[" + se.Message + "]");
                Program.SysLog.Write(this, "SQL 오류", se);
                return null;
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "쿼리 실행(Select) 오류![" + e.Message + "]");
                Program.SysLog.Write(this, "쿼리 실행(Select) 오류!", e);
                return null;
            }
            return dataSet;
        }

        public int ExecuteNonQuery(string pstrCommandText, int nTimeout)
        {
            int iRtn = 0;
            try
            {
                Reconnect();
                if (!bConnected)
                {
                    Program.AppLog.Write(this, "DB 연결이 불가능 합니다.[commandText:" + pstrCommandText + "]");
                    return -9;
                }

                lock (oDatabaseCO)
                {
                    using (SqlCommand command = new SqlCommand(pstrCommandText, Connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = nTimeout;
                        iRtn = command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException se)
            {
                Program.AppLog.Write(this, "SQL 오류[" + se.Message + "]");
                Program.SysLog.Write(this, "SQL 오류", se);
                iRtn = -99;
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "쿼리 실행 오류![" + e.Message + "]");
                Program.SysLog.Write(this, "쿼리 실행 오류!", e);
                iRtn = -999;
            }
            return iRtn;
        }

        public string ExecuteFunctionString(string pstrCommandText, Hashtable paramHashtable, int nTimeout)
        {
            string result = "";
            try
            {
                Reconnect();
                if (!bConnected)
                {
                    Program.AppLog.Write(this, "DB 연결이 불가능 합니다.[commandText:" + pstrCommandText + "]");
                    return "";
                }

                lock (oDatabaseCO)
                {
                    using (SqlCommand command = new SqlCommand(pstrCommandText, Connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = nTimeout;

                        foreach (string key in paramHashtable.Keys)
                        {
                            object val = paramHashtable[key] ?? DBNull.Value;
                            command.Parameters.AddWithValue(key, val);
                        }

                        SqlParameter returnParam = new SqlParameter("@RETURN_VALUE", SqlDbType.VarChar, 100)
                        {
                            Direction = ParameterDirection.ReturnValue
                        };
                        command.Parameters.Add(returnParam);

                        command.ExecuteNonQuery();
                        result = returnParam.Value?.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "함수 실행(String) 오류![" + e.Message + "]");
                Program.SysLog.Write(this, "함수 실행(String) 오류!", e);
            }
            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool bDisposing)
        {
            if (bDisposing)
            {
                if (Connection != null)
                {
                    Connection.Dispose();
                    Connection = null;
                }
            }
        }

        private string makeHashtableToString(Hashtable paramHashtable)
        {
            StringBuilder resultString = new StringBuilder();
            try
            {
                resultString.Append("[");
                foreach (string hashKey in paramHashtable.Keys)
                {
                    resultString.AppendFormat("(Key:{0},Value:{1})", hashKey, paramHashtable[hashKey]);
                }
                resultString.Append("]");
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "Unexpected Exception![" + e.Message + "]");
                Program.SysLog.Write(this, "Unexpected Exception!", e);
            }
            return resultString.ToString();
        }

        ~DatabaseManagementMSSQL()
        {
            Dispose(false);
        }
    }
}
