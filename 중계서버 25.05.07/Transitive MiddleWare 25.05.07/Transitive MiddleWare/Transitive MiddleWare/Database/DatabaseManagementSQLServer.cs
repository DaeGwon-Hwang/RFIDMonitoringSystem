using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections;

namespace BPNS.TransitiveMiddleware
{
    public class DatabaseManagementSQLServer : IDatabaseManagement, IDisposable
    {
        private SqlConnection Connection = null;
        private object oDatabaseCO = new object();

        public bool bConnected
        {
            get
            {
                if (Connection != null && Connection.State != ConnectionState.Closed)
                {
                    return true;
                }
                else
                {
                    Program.AppLog.Write(this, "DB Connection State : " + ((Connection == null) ? "" : Connection.State.ToString()) + "]");
                    return false;
                }
            }
        }

        public bool Connect()
        {
            string connectionString;
            bool bResult = false;

            try
            {
                connectionString = string.Format("Data Source={0};User ID={1};Password={2};Persist Security Info=True;Initial Catalog={3};", Properties.Settings.Default.DB_HostName, Properties.Settings.Default.DB_UserID, Properties.Settings.Default.DB_Pass, Properties.Settings.Default.DB_Name);
                Connection = new SqlConnection(connectionString);
                Connection.Open();
                bResult = true;
            }
            catch (InvalidOperationException ioe)
            {
                Program.AppLog.Write(this, "Invalid Operation Exception![" + ioe.Message + "]");
                Program.SysLog.Write(this, "Invalid Operation Exception!", ioe);
            }
            catch (SqlException se)
            {
                Program.AppLog.Write(this, "SQL Server Exception![" + se.Message + "]");
                Program.SysLog.Write(this, "SQL ServerException!", se);
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "Unexpected Exception![" + e.Message + "]");
                Program.SysLog.Write(this, "Unexpected Exception!", e);
            }
            return bResult;
//            return false;
        }
        public void Disconnect()
        {
            try
            {
                if (bConnected)
                {
                    Connection.Close();
                    Connection.Dispose();
                    Connection = null;
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "Unexpected Exception![" + e.Message + "]");
                Program.SysLog.Write(this, "Unexpected Exception!", e);
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
                Program.AppLog.Write(this, "Unexpected Exception![" + e.Message + "]");
                Program.SysLog.Write(this, "Unexpected Exception!", e);
            }
        }

        public int ExecuteProcedure(string pstrCommandText, Hashtable htParamHashtable, int nTimeout)
        {
            int nReturnValue;

            SqlCommand command;

            try
            {
                Reconnect();
                if (!bConnected)
                {
                    Program.AppLog.Write(this, "DB연결이 불가능 합니다.[commandText:" + pstrCommandText + "]" + MakeHashtableToString(htParamHashtable));
                    return -1000;
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "Unexpected Exception![" + e.Message + "]");
                Program.SysLog.Write(this, "Unexpected Exception!", e);
                return -1000;
            }
            nReturnValue = 0;
            try
            {
                lock (oDatabaseCO)
                {
                    command = Connection.CreateCommand();
                    //command.CommandText = pstrCommandText;
                    if (pstrCommandText.IndexOf('.') == -1)
                        command.CommandText = pstrCommandText;
                    else
                        command.CommandText = pstrCommandText.Substring(pstrCommandText.IndexOf('.') + 1);
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = nTimeout;
                    foreach (string key in htParamHashtable.Keys)
                    {
                        if (htParamHashtable[key].GetType() == typeof(string))
                        {
                            //                        command.Parameters.Add(string.Format("{0}", key), OracleDbType.Varchar2).Value = paramHashtable[key];
                            SqlParameter parameter = command.Parameters.Add(string.Format("@{0}", key), SqlDbType.VarChar, ((string)htParamHashtable[key]).Length);
                            parameter.Direction = ParameterDirection.Input;
                            parameter.Value = htParamHashtable[key].ToString();

                            //command.Parameters[string.Format("{0}", key)].Size = ((string)paramHashtable[key]).Length;
                        }
                        else if (htParamHashtable[key].GetType() == typeof(int))
                        {
                            command.Parameters.Add(string.Format("@{0}", key), SqlDbType.Int).Value = htParamHashtable[key];
                        }
                        else if (htParamHashtable[key].GetType() == typeof(float))
                        {
                            command.Parameters.Add(string.Format("@{0}", key), SqlDbType.Float).Value = htParamHashtable[key];
                        }
                        else if (htParamHashtable[key].GetType() == typeof(System.Decimal))
                        {
                            command.Parameters.Add(string.Format("@{0}", key), SqlDbType.Decimal).Value = htParamHashtable[key];
                        }
                    }
                    command.Parameters.Add("@O_APP_CODE", SqlDbType.VarChar).Direction = ParameterDirection.Output;
                    command.Parameters["@O_APP_CODE"].Size = 100;
                    command.Parameters["@O_APP_CODE"].Value = DBNull.Value;
                    command.Parameters.Add("@O_APP_MSG", SqlDbType.VarChar).Direction = ParameterDirection.Output;
                    command.Parameters["@O_APP_MSG"].Size = 4000;
                    command.Parameters["@O_APP_CODE"].Value = DBNull.Value;

                    nReturnValue = command.ExecuteNonQuery();

                    if (command.Parameters["@O_APP_CODE"].Value != DBNull.Value)
                    {
                        htParamHashtable.Add("O_APP_CODE", command.Parameters["@O_APP_CODE"].Value);
                        htParamHashtable.Add("O_APP_MSG", command.Parameters["@O_APP_MSG"].Value);
                    }
                }
            }
            catch (SqlException se)
            {
                if (se.Number == 6550)
                {
                    Program.AppLog.Write(this, "SQL 오류[" + pstrCommandText + "가 없습니다.]");
                }
                else if (se.Number == 3113)
                {
                    Program.AppLog.Write(this, "DB 연결이 끊어졌습니다.");
                    return nReturnValue = -1000;
                }
                else
                {
                    Program.AppLog.Write(this, "SQL 오류[" + pstrCommandText + "]");
                    Program.SysLog.Write(this, "SQL 오류[" + pstrCommandText + "]", se);
                }
                nReturnValue = -1001;
            }
            catch (Exception e)
            {
                Program.AppLog.Write("ExecuteProcedure 오류[COMMAND:" + pstrCommandText + "]");
                Program.SysLog.Write(this, "ExecuteProcedure 오류[COMMAND:" + pstrCommandText + "]", e);
                nReturnValue = -9999;
            }

            return nReturnValue;
        }
        public DataSet ExecuteSelectProcedure(string pstrCommandText, Hashtable htParamHashtable, int nTimeout)
        {
            SqlCommand command;
            DataSet dataSet;
            try
            {
                Reconnect();
                if (!bConnected)
                {
                    Program.AppLog.Write(this, "DB 연결이 불가능 합니다.[commandText:" + pstrCommandText + "]" + MakeHashtableToString(htParamHashtable));
                    return null;
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "정의되지 않은 오류![" + e.Message + "]");
                Program.SysLog.Write(this, "정의되지 않은 오류!", e);
                return null;
            }
            try
            {
                lock (oDatabaseCO)
                {
                    command = Connection.CreateCommand();
                    dataSet = new DataSet();
                    //command.CommandText = pstrCommandText;
                    if (pstrCommandText.IndexOf('.') == -1)
                        command.CommandText = pstrCommandText;
                    else
                        command.CommandText = pstrCommandText.Substring(pstrCommandText.IndexOf('.') + 1);
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = nTimeout;
                    foreach (string key in htParamHashtable.Keys)
                    {
                        if (htParamHashtable[key].GetType() == typeof(string))
                        {
                            command.Parameters.Add(string.Format("@{0}", key), SqlDbType.VarChar).Value = htParamHashtable[key];
                            command.Parameters[string.Format("@{0}", key)].Size = 100;
                        }
                        else if (htParamHashtable[key].GetType() == typeof(int))
                        {
                            command.Parameters.Add(string.Format("@{0}", key), SqlDbType.Int).Value = htParamHashtable[key];
                        }
                        else if (htParamHashtable[key].GetType() == typeof(float))
                        {
                            command.Parameters.Add(string.Format("@{0}", key), SqlDbType.Float).Value = htParamHashtable[key];
                        }
                        else if (htParamHashtable[key].GetType() == typeof(System.Decimal))
                        {
                            command.Parameters.Add(string.Format("@{0}", key), SqlDbType.Decimal).Value = htParamHashtable[key];
                        }
                    }
                    //                command.Parameters.Add("O_CUR", SqlDbType.Var).Direction = ParameterDirection.Output;

                    //command.Parameters.Add("@O_APP_CODE", SqlDbType.Int).Direction = ParameterDirection.Output;
                    //command.Parameters.Add("@O_APP_MSG", SqlDbType.VarChar).Direction = ParameterDirection.Output;
                    //command.Parameters["@O_APP_CODE"].Size = 200;
                    //command.Parameters["@O_APP_MSG"].Size = 4000;

                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                    dataAdapter.Fill(dataSet);

                    //if (command.Parameters["@O_APP_CODE"].Value != DBNull.Value)
                    //{
                    //    htParamHashtable.Add("O_APP_CODE", command.Parameters["@O_APP_CODE"].Value);
                    //    htParamHashtable.Add("O_APP_MSG", command.Parameters["@O_APP_MSG"].Value);
                    //}
                }
            }
            catch (SqlException oe)
            {
                if (oe.Number == 6550)
                {
                    Program.AppLog.Write("ExecuteSelectProcedure 오류[COMMAND:" + pstrCommandText + "가 없습니다.]");
                }
                else if (oe.Number == 3113)
                {
                    Program.AppLog.Write(this, "DB 연결이 끊어졌습니다.");
                }
                else
                {
                    Program.AppLog.Write("ExecuteSelectProcedure 오류[COMMAND:" + pstrCommandText + "]");
                    Program.SysLog.Write(this, "ExecuteSelectProcedure 오류[COMMAND:" + pstrCommandText + "]", oe);
                }
                dataSet = null;
            }
            catch (Exception e)
            {
                Program.AppLog.Write("ExecuteSelectProcedure 오류[COMMAND:" + pstrCommandText + "]");
                Program.SysLog.Write(this, "ExecuteSelectProcedure 오류[COMMAND:" + pstrCommandText + "]", e);
                dataSet = null;
            }
            return dataSet;
        }

        public DataSet ExecuteSelectQuery(string pstrCommandText, int nTimeout)
        {
            SqlCommand command;
            DataSet dataSet;
            try
            {
                Reconnect();
                if (!bConnected)
                {
                    Program.AppLog.Write(this, "DB 연결이 불가능 합니다.[commandText:" + pstrCommandText + "]");
                    return null;
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "정의되지 않은 오류![" + e.Message + "]");
                Program.SysLog.Write(this, "정의되지 않은 오류!", e);
                return null;
            }
            try
            {
                lock (oDatabaseCO)
                {

                    command = Connection.CreateCommand();
                    dataSet = new DataSet();
                    command.CommandText = pstrCommandText;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = nTimeout;
                    //foreach (string key in paramHashtable.Keys)
                    //{
                    //    if (paramHashtable[key].GetType() == typeof(string))
                    //    {
                    //        command.Parameters.Add(string.Format("@{0}", key), SqlDbType.VarChar).Value = paramHashtable[key];
                    //        command.Parameters[string.Format("@{0}", key)].Size = 100;
                    //    }
                    //    else if (paramHashtable[key].GetType() == typeof(int))
                    //    {
                    //        command.Parameters.Add(string.Format("@{0}", key), SqlDbType.Int).Value = paramHashtable[key];
                    //    }
                    //    else if (paramHashtable[key].GetType() == typeof(float))
                    //    {
                    //        command.Parameters.Add(string.Format("@{0}", key), SqlDbType.Float).Value = paramHashtable[key];
                    //    }
                    //    else if (paramHashtable[key].GetType() == typeof(System.Decimal))
                    //    {
                    //        command.Parameters.Add(string.Format("@{0}", key), SqlDbType.Decimal).Value = paramHashtable[key];
                    //    }
                    //}
                    //                command.Parameters.Add("O_CUR", SqlDbType.Var).Direction = ParameterDirection.Output;

                    //command.Parameters.Add("@ERR_ CODE", SqlDbType.Int).Direction = ParameterDirection.Output;
                    //command.Parameters.Add("@ERR_MSG", SqlDbType.VarChar).Direction = ParameterDirection.Output;
                    //command.Parameters["@ERR_MSG"].Size = 4000;

                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                    dataAdapter.Fill(dataSet);

                    //if (command.Parameters["@O_ERR_ CODE"].Value != DBNull.Value)
                    //{
                    //    paramHashtable.Add("@O_ERR_CODE", command.Parameters["@O_ERR_ CODE"].Value);
                    //    paramHashtable.Add("@O_ERR_MSG", command.Parameters["@O_ERR_MSG"].Value);
                    //}
                }
            }
            catch (SqlException oe)
            {
                if (oe.Number == 6550)
                {
                    Program.AppLog.Write("ExecuteSelectQuery 오류[COMMAND:" + pstrCommandText + "가 없습니다.]");
                }
                else if (oe.Number == 3113)
                {
                    Program.AppLog.Write(this, "DB 연결이 끊어졌습니다.");
                }
                else
                {
                    Program.AppLog.Write("ExecuteSelectQuery 오류[COMMAND:" + pstrCommandText + "]");
                    Program.SysLog.Write(this, "ExecuteSelectQuery 오류[COMMAND:" + pstrCommandText + "]", oe);
                }
                dataSet = null;
            }
            catch (Exception e)
            {
                Program.AppLog.Write("ExecuteSelectQuery 오류[COMMAND:" + pstrCommandText + "]");
                Program.SysLog.Write(this, "ExecuteSelectQuery 오류[COMMAND:" + pstrCommandText + "]", e);
                dataSet = null;
            }
            return dataSet;
        }

        public int ExecuteNonQuery(string pstrCommandText, int nTimeout)
        {
            SqlCommand command;
            int iRtn = 0;
            try
            {
                Reconnect();
                if (!bConnected)
                {
                    Program.AppLog.Write(this, "DB 연결이 불가능 합니다.[commandText:" + pstrCommandText + "]");
                    return -9;
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write("데이터베이스 연결오류(ExecuteSelectQuery)[" + e.Message + "]");
                Program.SysLog.Write(this, "데이터베이스 연결오류(ExecuteSelectQuery)", e);
                return -9;
            }
            try
            {
                lock (oDatabaseCO)
                {

                    command = Connection.CreateCommand();
                    command.CommandText = pstrCommandText;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = nTimeout;

                    iRtn = command.ExecuteNonQuery();
                }
            }
            catch (SqlException oe)
            {
                if (oe.Number == 6550)
                {
                    Program.AppLog.Write(this, "SQL 오류[" + pstrCommandText + "가 없습니다.]");
                }
                else if (oe.Number == 3113)
                {
                    Program.AppLog.Write(this, "DB 연결이 끊어졌습니다.");
                }
                else if (oe.Number == 12570 || oe.Number == 12571)
                {
                    Program.AppLog.Write("데이터베이스 sync 오류 발생[Number:" + oe.Number + ",Message:" + oe.Message + "]");
                    Program.AppLog.Write("데이터베이스 연결 강제 종료");
                    Program.SysLog.Write(this, "데이터베이스 sync 오류 발생[Number:" + oe.Number + ",Message:" + oe.Message + "]", oe);
                    Program.SysLog.Write(this, "데이터베이스 연결 강제 종료", oe);
                    Disconnect();
                }
                else
                {
                    Program.AppLog.Write(this, "오라클 오류![" + oe.Message + "]");
                    Program.SysLog.Write(this, "오라클 오류!", oe);
                }
                iRtn = -99;
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "쿼리 실행(Select) 오류![" + e.Message + "]");
                Program.SysLog.Write(this, "쿼리 실행(Select) 오류!", e);
                iRtn = -999;
            }
            return iRtn;
        }

        public string ExecuteFunctionString(string pstrCommandText, Hashtable htParamHashtable, int nTimeout)
        {
            int nReturnValue;

            SqlCommand command;

            try
            {
                Reconnect();
                if (!bConnected)
                {
                    Program.AppLog.Write(this, "DB연결이 불가능 합니다.[commandText:" + pstrCommandText + "]" + MakeHashtableToString(htParamHashtable));
                    return "";
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "Unexpected Exception![" + e.Message + "]");
                Program.SysLog.Write(this, "Unexpected Exception!", e);
                return "";
            }
            nReturnValue = 0;
            try
            {
                lock (oDatabaseCO)
                {

                    command = Connection.CreateCommand();
                    if (pstrCommandText.IndexOf('.') == -1)
                        command.CommandText = pstrCommandText;
                    else
                        command.CommandText = pstrCommandText.Substring(pstrCommandText.IndexOf('.') + 1);

                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = nTimeout;
                    foreach (string key in htParamHashtable.Keys)
                    {
                        if (htParamHashtable[key].GetType() == typeof(string))
                        {
                            //                        command.Parameters.Add(string.Format("{0}", key), OracleDbType.Varchar2).Value = paramHashtable[key];
                            SqlParameter parameter = command.Parameters.Add(string.Format("@{0}", key), SqlDbType.VarChar, ((string)htParamHashtable[key]).Length);
                            parameter.Direction = ParameterDirection.Input;
                            parameter.Value = htParamHashtable[key].ToString();

                            //command.Parameters[string.Format("{0}", key)].Size = ((string)paramHashtable[key]).Length;
                        }
                        else if (htParamHashtable[key].GetType() == typeof(int))
                        {
                            command.Parameters.Add(string.Format("@{0}", key), SqlDbType.Int).Value = htParamHashtable[key];
                        }
                        else if (htParamHashtable[key].GetType() == typeof(float))
                        {
                            command.Parameters.Add(string.Format("@{0}", key), SqlDbType.Float).Value = htParamHashtable[key];
                        }
                        else if (htParamHashtable[key].GetType() == typeof(System.Decimal))
                        {
                            command.Parameters.Add(string.Format("@{0}", key), SqlDbType.Decimal).Value = htParamHashtable[key];
                        }
                    }
                    command.Parameters.Add("@DATESTRING", SqlDbType.VarChar).Direction = ParameterDirection.ReturnValue;
                    command.Parameters["@DATESTRING"].Size = 100;

                    nReturnValue = command.ExecuteNonQuery();
                    if (command.Parameters["@DATESTRING"].Value != DBNull.Value)
                    {
                        return command.Parameters["@DATESTRING"].Value.ToString();
                    }
                }
            }
            catch (SqlException se)
            {
                if (se.Number == 6550)
                {
                    Program.AppLog.Write("ExecuteFunctionString 오류[COMMAND:" + pstrCommandText + "가 없습니다.]");
                }
                else if (se.Number == 3113)
                {
                    Program.AppLog.Write(this, "DB 연결이 끊어졌습니다.");
                    return "";
                }
                else
                {
                    Program.AppLog.Write("ExecuteFunctionString 오류[COMMAND:" + pstrCommandText + "]");
                    Program.SysLog.Write(this, "ExecuteFunctionString 오류[COMMAND:" + pstrCommandText + "]", se);
                }
                return "";
            }
            catch (Exception e)
            {
                Program.AppLog.Write("ExecuteFunctionString 오류[COMMAND:" + pstrCommandText + "]");
                Program.SysLog.Write(this, "ExecuteFunctionString 오류[COMMAND:" + pstrCommandText + "]", e);
                return "";
            }

            return "";
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
        private string MakeHashtableToString(Hashtable htParamHashtable)
        {
            StringBuilder resultString = null;

            try
            {
                resultString = new StringBuilder();
                resultString.Append("[");
                foreach (string hashKey in htParamHashtable.Keys)
                {
                    resultString.AppendFormat("(Key:" + hashKey + ",Value:" + htParamHashtable[hashKey].ToString() + ")");
                }
                resultString.Append("[");
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "Unexpected Exception![" + e.Message + "]");
                Program.SysLog.Write(this, "Unexpected Exception!", e);
            }
            return resultString.ToString();
        }

        ~DatabaseManagementSQLServer()
        {
            Dispose(false);
        }

    }
}
