using System;
using System.Text;
using System.Collections;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace BPNS.EdgeMW
{
    public class DatabaseManagementOracle : IDisposable, IDatabaseManagement
    {
        private OracleConnection Connection = null;
        private volatile object oDatabaseCO = new object();

        public bool bConnected
        {
            get
            {
                //                ((MainForm)(Application.OpenForms["MainForm"])).setDBState(Connection.State.ToString());
                if (Connection != null && Connection.State == ConnectionState.Open)
                {
                    return true;
                }
                else
                {
                    //Program.AppLog.Write(this, "DB Connection State : " + ((Connection == null)? "" : Connection.State.ToString()) + "]");
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
                lock (oDatabaseCO)
                {

                    //connectionString = string.Format("Data Source={0};User ID={1};Password={2};Persist Security Info=True", Properties.Settings.Default.DB_Name, Properties.Settings.Default.DB_UserID, Properties.Settings.Default.DB_Pass);
                    //원래 Connection String
                    //connectionString = "Data Source=(DESCRIPTION =(LOAD_BALANCE = OFF)(FAILOVER = TRUE)(ADDRESS = (PROTOCOL = TCP)(HOST = 10.100.17.12)(PORT = 1521))(ADDRESS = (PROTOCOL = TCP)(HOST = 10.100.17.11)(PORT = 1521))(CONNECT_DATA =(SERVICE_NAME = HIDB)));User Id=C86E;Password = prodc86e;";
                    //connectionString = "Data Source=(DESCRIPTION =(LOAD_BALANCE = OFF)(FAILOVER = TRUE)(ADDRESS = (PROTOCOL = TCP)(HOST = 127.0.0.1)(PORT = 1521))(CONNECT_DATA =(SERVICE_NAME = xe)));User Id=rfidpams;Password = bpns0620;";
                    //connectionString = "Data Source=(DESCRIPTION =(LOAD_BALANCE = OFF)(FAILOVER = TRUE)(ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.200.27)(PORT = 1521))(CONNECT_DATA =(SERVICE_NAME = xe)));User Id=rfidpams;Password = rfidpams;";
                    //부산 사무실 김용길 부장 PC 오라클
                    //내부
                    //connectionString = "Data Source=(DESCRIPTION =(LOAD_BALANCE = OFF)(FAILOVER = TRUE)(ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.10.54)(PORT = 1521))(CONNECT_DATA =(SERVICE_NAME = orcl)));User Id=EMC;Password = BP0620;";
                    //외부
                    //connectionString = "Data Source=(DESCRIPTION =(LOAD_BALANCE = OFF)(FAILOVER = TRUE)(ADDRESS = (PROTOCOL = TCP)(HOST = 58.231.100.202)(PORT = 1521))(CONNECT_DATA =(SERVICE_NAME = orcl)));User Id=EMC;Password = BP0620;";

                    connectionString = "Data Source=(DESCRIPTION =(LOAD_BALANCE = OFF)(FAILOVER = TRUE)(ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.200.29)(PORT = 1521))(CONNECT_DATA =(SERVICE_NAME = orcl)));User Id=rfidpams;Password = rfidpams;";
                    //connectionString = "Data Source=(DESCRIPTION =(LOAD_BALANCE = OFF)(FAILOVER = TRUE)(ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.200.22)(PORT = 1521))(CONNECT_DATA =(SERVICE_NAME = rfidpams)));User Id=rfidpams;Password = rfidpams;";


                    if (Connection != null)
                    {
                        Connection.Close();
                        Program.AppLog.Write("데이터베이스 연결종료(기존 연결 리소스 제거):[ORACLE]");
                        Connection = null;
                    }
                    Connection = new OracleConnection(connectionString);
                    Connection.Open();
                    bResult = true;
                    Program.AppLog.Write("데이터베이스 연결:[오라클]");
                }
            }
            catch (InvalidOperationException ioe)
            {
                Program.AppLog.Write("데이터베이스 연결 오류[" + ioe.Message + "]");
                Program.SysLog.Write(this, "데이터베이스 연결 오류", ioe);
            }
            catch (OracleException oe)
            {
                Program.AppLog.Write("데이터베이스 연결 오류[" + oe.Message + "]");
                Program.SysLog.Write(this, "데이터베이스 연결 오류", oe);
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
                        Program.AppLog.Write("데이터베이스 연결종료:[ORACLE]");
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
            int nReturnValue;
            OracleCommand command;
            try
            {
                Reconnect();
                if (!bConnected)
                {
                    Program.AppLog.Write(this, "DB 연결이 불가능 합니다.[commandText:" + pstrCommandText + "]" + makeHashtableToString(paramHashtable));
                    return -1000;
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write("데이터베이스 연결오류(ExecuteProcedure)[" + e.Message + "]");
                Program.SysLog.Write(this, "데이터베이스 연결오류(ExecuteProcedure)", e);
                return -1000;
            }
            nReturnValue = 0;
            try
            {
                lock (oDatabaseCO)
                {
                    command = Connection.CreateCommand();
                    //            dataSet = new DataSet();
                    command.CommandText = pstrCommandText;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = nTimeout;
                    command.BindByName = true;
                    foreach (string key in paramHashtable.Keys)
                    {
                        if (paramHashtable[key] == null)
                        {
                            command.Parameters.Add(string.Format("{0}", key), OracleDbType.Int32).Value = null;
                        }
                        else if (paramHashtable[key].GetType() == typeof(string))
                        {
                            //                        command.Parameters.Add(string.Format("{0}", key), OracleDbType.Varchar2).Value = paramHashtable[key];
                            OracleParameter parameter = command.Parameters.Add(key, OracleDbType.Varchar2, ((string)paramHashtable[key]).Length);
                            parameter.Direction = ParameterDirection.Input;
                            parameter.Value = paramHashtable[key].ToString();

                            //command.Parameters[string.Format("{0}", key)].Size = ((string)paramHashtable[key]).Length;
                        }
                        else if (paramHashtable[key].GetType() == typeof(int))
                        {
                            command.Parameters.Add(string.Format("{0}", key), OracleDbType.Int32).Value = paramHashtable[key];
                        }
                        else if (paramHashtable[key].GetType() == typeof(float))
                        {
                            command.Parameters.Add(string.Format("{0}", key), OracleDbType.BinaryFloat).Value = paramHashtable[key];
                        }
                        else if (paramHashtable[key].GetType() == typeof(System.Decimal))
                        {
                            command.Parameters.Add(string.Format("{0}", key), OracleDbType.Decimal).Value = paramHashtable[key];
                        }
                    }
                    command.Parameters.Add("O_APP_CODE", OracleDbType.Varchar2).Direction = ParameterDirection.Output;
                    command.Parameters["O_APP_CODE"].Size = 100;
                    command.Parameters.Add("O_APP_MSG", OracleDbType.Varchar2).Direction = ParameterDirection.Output;
                    command.Parameters["O_APP_MSG"].Size = 500;

                    //            OracleDataAdapter dataAdapter = new OracleDataAdapter(command);
                    //            dataAdapter.Fill(dataSet);
                    nReturnValue = command.ExecuteNonQuery();
                    if (command.Parameters["O_APP_CODE"].Status != OracleParameterStatus.NullFetched)
                    {
                        paramHashtable.Add("O_APP_CODE", command.Parameters["O_APP_CODE"].Value);
                        paramHashtable.Add("O_APP_MSG", command.Parameters["O_APP_MSG"].Value);
                        return nReturnValue;
                    }
                }
            }
            catch (OracleException oe)
            {
                if (oe.Number == 6550)
                {
                    Program.AppLog.Write(this, "SQL 오류[" + pstrCommandText + "가 없습니다.]");
                    nReturnValue = -11;
                }
                else if (oe.Number == 3113)
                {
                    Program.AppLog.Write(this, "DB 연결이 끊어졌습니다.");
                    return nReturnValue = -1000;
                }
                else if (oe.Number == 12570 || oe.Number == 12571)
                {
                    Program.AppLog.Write("데이터베이스 sync 오류 발생[Number:" + oe.Number + ",Message:" + oe.Message + "]");
                    Program.AppLog.Write("데이터베이스 연결 강제 종료");
                    Program.SysLog.Write(this, "데이터베이스 sync 오류 발생[Number:" + oe.Number + ",Message:" + oe.Message + "]", oe);
                    Program.SysLog.Write(this, "데이터베이스 연결 강제 종료", oe);
                    Disconnect();
                    nReturnValue = -12;
                }
                else if (oe.Number == 01013 || oe.Number == 03111)
                {
                    Program.AppLog.Write("데이터베이스 Timeout");
                    Program.SysLog.Write(this, "데이터베이스 Timeout", oe);
                    nReturnValue = -13;
                }
                else
                {
                    Program.AppLog.Write(this, "SQL 오류[" + pstrCommandText + "]");
                    Program.SysLog.Write(this, "SQL 오류[" + pstrCommandText + "]", oe);
                    nReturnValue = -19;
                }
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
            OracleCommand command;
            DataSet dataSet;
            OracleDataAdapter dataAdapter;
            try
            {
                Reconnect();
                if (!bConnected)
                {
                    Program.AppLog.Write(this, "DB 연결이 불가능 합니다.[commandText:" + pstrCommandText + "]" + makeHashtableToString(paramHashtable));
                    return null;
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write("데이터베이스 연결오류(ExecuteSelectProcedure)[" + e.Message + "]");
                Program.SysLog.Write(this, "데이터베이스 연결오류(ExecuteSelectProcedure)", e);
                return null;
            }
            try
            {
                lock (oDatabaseCO)
                {
                    command = Connection.CreateCommand();
                    dataSet = new DataSet();
                    command.CommandText = pstrCommandText;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = nTimeout;
                    command.BindByName = true;
                    command.Parameters.Clear();
                    foreach (string key in paramHashtable.Keys)
                    {
                        if (paramHashtable[key].GetType() == typeof(string))
                        {
                            command.Parameters.Add(string.Format("{0}", key), OracleDbType.Varchar2).Value = paramHashtable[key];
                            command.Parameters[string.Format("{0}", key)].Size = 100;
                        }
                        else if (paramHashtable[key].GetType() == typeof(int))
                        {
                            command.Parameters.Add(string.Format("{0}", key), OracleDbType.Int32).Value = paramHashtable[key];
                        }
                        else if (paramHashtable[key].GetType() == typeof(float))
                        {
                            command.Parameters.Add(string.Format("{0}", key), OracleDbType.BinaryFloat).Value = paramHashtable[key];
                        }
                        else if (paramHashtable[key].GetType() == typeof(System.Decimal))
                        {
                            command.Parameters.Add(string.Format("{0}", key), OracleDbType.Decimal).Value = paramHashtable[key];
                        }
                    }
                    command.Parameters.Add("ORESULT_CUR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    dataAdapter = new OracleDataAdapter(command);
                    dataAdapter.Fill(dataSet);
                }
            }
            catch (OracleException oe)
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
                dataSet = null;
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "프로시저 실행(Select) 오류![" + e.Message + "]");
                Program.SysLog.Write(this, "프로시저 실행(Select) 오류!", e);
                dataSet = null;
            }
            return dataSet;
        }

        public DataSet ExecuteSelectQuery(string pstrCommandText, int nTimeout)
        {
            OracleCommand command;
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
                Program.AppLog.Write("데이터베이스 연결오류(ExecuteSelectQuery)[" + e.Message + "]");
                Program.SysLog.Write(this, "데이터베이스 연결오류(ExecuteSelectQuery)", e);
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
                    //        command.Parameters.Add(string.Format("{0}", key), OracleDbType.Varchar2).Value = paramHashtable[key];
                    //        command.Parameters[string.Format("{0}", key)].Size = 100;
                    //    }
                    //    else if (paramHashtable[key].GetType() == typeof(int))
                    //    {
                    //        command.Parameters.Add(string.Format("{0}", key), OracleDbType.Int32).Value = paramHashtable[key];
                    //    }
                    //    else if (paramHashtable[key].GetType() == typeof(float))
                    //    {
                    //        command.Parameters.Add(string.Format("{0}", key), OracleDbType.BinaryFloat).Value = paramHashtable[key];
                    //    }
                    //    else if (paramHashtable[key].GetType() == typeof(System.Decimal))
                    //    {
                    //        command.Parameters.Add(string.Format("{0}", key), OracleDbType.Decimal).Value = paramHashtable[key];
                    //    }
                    //}
                    //command.Parameters.Add("O_CUR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    OracleDataAdapter dataAdapter = new OracleDataAdapter(command);
                    dataAdapter.Fill(dataSet);
                }
            }
            catch (OracleException oe)
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
                dataSet = null;
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "쿼리 실행(Select) 오류![" + e.Message + "]");
                Program.SysLog.Write(this, "쿼리 실행(Select) 오류!", e);
                dataSet = null;
            }
            return dataSet;
        }

        public int ExecuteNonQuery(string pstrCommandText, int nTimeout)
        {
            OracleCommand command;
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
            catch (OracleException oe)
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

        public string ExecuteFunctionString(string pstrCommandText, Hashtable paramHashtable, int nTimeout)
        {
            OracleCommand ocCommand;
            try
            {
                Reconnect();
                if (!bConnected)
                {
                    Program.AppLog.Write(this, "DB 연결이 불가능 합니다.[commandText:" + pstrCommandText + "]" + makeHashtableToString(paramHashtable));
                    return "";
                }
            }
            catch (Exception e)
            {
                Program.AppLog.Write("데이터베이스 연결오류(ExecuteFunctionString)[" + e.Message + "]");
                Program.SysLog.Write(this, "데이터베이스 연결오류(ExecuteFunctionString)", e);
                return "";
            }
            try
            {
                lock (oDatabaseCO)
                {

                    ocCommand = Connection.CreateCommand();
                    //            dataSet = new DataSet();
                    ocCommand.CommandText = pstrCommandText;
                    ocCommand.CommandType = CommandType.StoredProcedure;
                    ocCommand.BindByName = true;
                    foreach (string key in paramHashtable.Keys)
                    {
                        if (paramHashtable[key].GetType() == typeof(string))
                        {
                            //command.Parameters.Add(string.Format("{0}", key), OracleDbType.Varchar2).Value = paramHashtable[key];
                            OracleParameter parameter = ocCommand.Parameters.Add(key, OracleDbType.Varchar2, ((string)paramHashtable[key]).Length);
                            parameter.Direction = ParameterDirection.Input;
                            parameter.Value = paramHashtable[key].ToString();
                            //command.Parameters[string.Format("{0}", key)].Size = ((string)paramHashtable[key]).Length;
                        }
                        else if (paramHashtable[key].GetType() == typeof(int))
                        {
                            ocCommand.Parameters.Add(string.Format("{0}", key), OracleDbType.Int32).Value = paramHashtable[key];
                        }
                        else if (paramHashtable[key].GetType() == typeof(float))
                        {
                            ocCommand.Parameters.Add(string.Format("{0}", key), OracleDbType.BinaryFloat).Value = paramHashtable[key];
                        }
                        else if (paramHashtable[key].GetType() == typeof(System.Decimal))
                        {
                            ocCommand.Parameters.Add(string.Format("{0}", key), OracleDbType.Decimal).Value = paramHashtable[key];
                        }
                    }
                    ocCommand.Parameters.Add("DATESTRING", OracleDbType.Varchar2).Direction = ParameterDirection.ReturnValue;
                    ocCommand.Parameters["DATESTRING"].Size = 100;

                    //            OracleDataAdapter dataAdapter = new OracleDataAdapter(command);
                    //            dataAdapter.Fill(dataSet);
                    ocCommand.ExecuteNonQuery();
                    if (ocCommand.Parameters["DATESTRING"].Status != OracleParameterStatus.NullFetched)
                    {

                        return ocCommand.Parameters["DATESTRING"].Value.ToString();
                    }
                }
            }
            catch (OracleException oe)
            {
                if (oe.Number == 6550)
                {
                    Program.AppLog.Write(this, "SQL 오류[" + pstrCommandText + "가 없습니다.]");
                }
                else if (oe.Number == 3113)
                {
                    Program.AppLog.Write(this, "DB 연결이 끊어졌습니다.");
                    return "";
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
                    Program.AppLog.Write(this, "SQL 오류[" + pstrCommandText + "]");
                    Program.SysLog.Write(this, "SQL 오류[" + pstrCommandText + "]", oe);
                }
                return "";
            }
            catch (Exception e)
            {
                Program.AppLog.Write(this, "함수 실행(String) 오류![" + e.Message + "]");
                Program.SysLog.Write(this, "함수 실행(String) 오류!", e);
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

        private string makeHashtableToString(Hashtable paramHashtable)
        {
            StringBuilder resultString = null;

            try
            {
                resultString = new StringBuilder();
                resultString.Append("[");
                foreach (string hashKey in paramHashtable.Keys)
                {
                    resultString.AppendFormat("(Key:" + hashKey + ",Value:" + paramHashtable[hashKey].ToString() + ")");
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
        ~DatabaseManagementOracle()
        {
            Dispose(false);
        }
    }
}
