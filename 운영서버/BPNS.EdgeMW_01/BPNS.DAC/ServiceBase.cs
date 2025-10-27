using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.DAC
{
    public class ServiceBase : IServiceBase
    {
        DBType _DBType;

        SqlConnection _sqlConnection;
        OracleConnection _oraConnection;

        public ServiceBase()
        {
            _sqlConnection = null;
            _oraConnection = null;
        }

        /// <summary>
        /// DB 연결 정보
        /// </summary>
        /// <param name="service"></param>
        private void SetConnection(RequestService service)
        {
            string strConnectionString = ConfigurationManager.ConnectionStrings[service.DB].ToString();
            string[] strConnectionStrings = strConnectionString.Split(new string[] { "::" }, StringSplitOptions.None);

            switch (strConnectionStrings[0])
            {
                case "MSSQL":
                    this._DBType = DBType.MSSQL;
                    _sqlConnection = new SqlConnection(strConnectionStrings[1]);
                    break;
                case "ORACLE":
                    this._DBType = DBType.ORACLE;
                    _oraConnection = new OracleConnection(strConnectionStrings[1]);
                    break;
            }
        }

        /// <summary>
        /// Helper ExecuteDataSet Break Point
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(RequestService service)
        {
            SqlTransaction sqlTransaction = null;
            OracleTransaction oraTransaction = null;

            SetConnection(service);

            if (service.TransactionType == TxType.Required)
            {
                switch (_DBType)
                {
                    case DBType.MSSQL:
                        _sqlConnection.Open();
                        sqlTransaction = this._sqlConnection.BeginTransaction();
                        break;
                    case DBType.ORACLE:
                        _oraConnection.Open();
                        oraTransaction = this._oraConnection.BeginTransaction();
                        break;
                }
            }

            try
            {
                DataSet DsReturn = new DataSet();

                for (int nLoop = 0; nLoop < service.Count; nLoop++)
                {
                    if (service.TransactionType == TxType.NotSupported)
                    {
                        FillDataSet(service.GetService(nLoop), DsReturn);
                    }
                    else
                    {
                        switch (_DBType)
                        {
                            case DBType.MSSQL:
                                FillDataSet(sqlTransaction, service.GetService(nLoop), DsReturn);
                                break;
                            case DBType.ORACLE:
                                FillDataSet(oraTransaction, service.GetService(nLoop), DsReturn);
                                break;
                        }
                    }
                }

                if (service.TransactionType == TxType.Required)
                {
                    switch (_DBType)
                    {
                        case DBType.MSSQL:
                            sqlTransaction.Commit();
                            break;
                        case DBType.ORACLE:
                            oraTransaction.Commit();
                            break;
                    }
                }

                return DsReturn;
            }
            catch (Exception ex)
            {
                if (sqlTransaction != null)
                    sqlTransaction.Rollback();
                if (oraTransaction != null)
                    oraTransaction.Rollback();

                throw ex;
            }
            finally
            {
                if (_DBType == DBType.ORACLE)
                {
                    if (oraTransaction != null)
                        oraTransaction.Dispose();
                    if (this._oraConnection.State != ConnectionState.Closed)
                        this._oraConnection.Close();
                    this._oraConnection.Dispose();
                    this._oraConnection = null;
                }

                if (_DBType == DBType.MSSQL)
                {
                    if (oraTransaction != null)
                        oraTransaction.Dispose();
                    if (this._sqlConnection != null)
                    {
                        if (this._sqlConnection.State != ConnectionState.Closed)
                            this._sqlConnection.Close();
                        this._sqlConnection.Dispose();
                        this._sqlConnection = null;
                    }
                }
            }
        }

        /// <summary>
        /// NON Transaction ExcecuteDataSet
        /// </summary>
        /// <param name="QP"></param>
        /// <param name="Ds"></param>
        private void FillDataSet(RequestPacket QP, DataSet Ds)
        {
            DataSet DsTemp = new DataSet();

            if (_DBType == DBType.MSSQL)
            {
                if (QP.CmdType == CommandType.Text)
                {
                    DsTemp = SqlHelper.ExecuteDataset(_sqlConnection, CommandType.Text, QP.Service);
                }
                else if (QP.CmdType == CommandType.StoredProcedure)
                {
                    DsTemp = SqlHelper.ExecuteDataset(_sqlConnection, CommandType.StoredProcedure, QP.Service, QP.SqlParams);
                }
            }

            if (_DBType == DBType.ORACLE)
            {
                if (QP.CmdType == CommandType.Text)
                {
                    DsTemp = OracleHelper.ExecuteDataset(_oraConnection, CommandType.Text, QP.Service);
                }
                else if (QP.CmdType == CommandType.StoredProcedure)
                {
                    DsTemp = OracleHelper.ExecuteDataset(_oraConnection, CommandType.StoredProcedure, QP.Service, QP.OraParams);

                }
            }

            DsTemp.Tables[0].TableName = QP.TableName;
            Ds.Tables.Add(DsTemp.Tables[0].Copy());
        }

        /// <summary>
        /// ORACLE Transaction ExcecuteDataSet
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="QP"></param>
        /// <param name="Ds"></param>
        private void FillDataSet(OracleTransaction transaction, RequestPacket QP, DataSet Ds)
        {
            DataSet DsTemp = new DataSet();

            if (QP.CmdType == CommandType.Text)
            {
                DsTemp = OracleHelper.ExecuteDataset(transaction, CommandType.Text, QP.Service);
            }
            else if (QP.CmdType == CommandType.StoredProcedure)
            {
                DsTemp = OracleHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, QP.Service, QP.OraParams);
            }

            DsTemp.Tables[0].TableName = QP.TableName;
            Ds.Tables.Add(DsTemp.Tables[0].Copy());
        }

        /// <summary>
        /// SQL Transaction ExcecuteDataSet
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="QP"></param>
        /// <param name="Ds"></param>
        private void FillDataSet(SqlTransaction transaction, RequestPacket QP, DataSet Ds)
        {
            DataSet DsTemp = new DataSet();

            if (QP.CmdType == CommandType.Text)
            {
                DsTemp = SqlHelper.ExecuteDataset(transaction, CommandType.Text, QP.Service);
            }
            else if (QP.CmdType == CommandType.StoredProcedure)
            {
                DsTemp = SqlHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, QP.Service, QP.SqlParams);
            }

            DsTemp.Tables[0].TableName = QP.TableName;
            Ds.Tables.Add(DsTemp.Tables[0].Copy());
        }


        /// <summary>
        /// Helper ExecuteNonQuery
        /// </summary>
        /// <param name="service"></param>
        public void ExecuteNonQuery(RequestService service)
        {
            SqlTransaction sqlTransaction = null;
            OracleTransaction oraTransaction = null;

            //_DBType = service.DataBaseType;

            SetConnection(service);
            try
            {
                if (service.TransactionType == TxType.Required)
                {
                    switch (_DBType)
                    {
                        case DBType.MSSQL:
                            _sqlConnection.Open();
                            sqlTransaction = _sqlConnection.BeginTransaction();
                            break;
                        case DBType.ORACLE:
                            _oraConnection.Open();
                            oraTransaction = _oraConnection.BeginTransaction();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
               
                throw ex;
            }
           

            try
            {
                for (int nLoop = 0; nLoop < service.Count; nLoop++)
                {
                    if (service.TransactionType == TxType.NotSupported)
                    {
                        ExecuteNonQuery(service.GetService(nLoop));
                    }
                    else
                    {
                        switch (_DBType)
                        {
                            case DBType.MSSQL:
                                ExecuteNonQuery(sqlTransaction, service.GetService(nLoop));
                                break;
                            case DBType.ORACLE:
                                ExecuteNonQuery(oraTransaction, service.GetService(nLoop));
                                break;
                        }
                    }
                }

                if (service.TransactionType == TxType.Required)
                {
                    switch (_DBType)
                    {
                        case DBType.MSSQL:
                            sqlTransaction.Commit();
                            break;
                        case DBType.ORACLE:
                            oraTransaction.Commit();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (sqlTransaction != null)
                    sqlTransaction.Rollback();
                if (oraTransaction != null)
                    oraTransaction.Rollback();

                throw ex;
            }
            finally
            {
                if (_DBType == DBType.ORACLE)
                {
                    if (oraTransaction != null)
                        oraTransaction.Dispose();
                    if (this._oraConnection.State != ConnectionState.Closed)
                        this._oraConnection.Close();
                    this._oraConnection.Dispose();
                    this._oraConnection = null;
                }

                if (_DBType == DBType.MSSQL)
                {
                    if (oraTransaction != null)
                        oraTransaction.Dispose();
                    if (this._sqlConnection != null)
                    {
                        if (this._sqlConnection.State != ConnectionState.Closed)
                            this._sqlConnection.Close();
                        this._sqlConnection.Dispose();
                        this._sqlConnection = null;
                    }
                }
            }
        }

        /// <summary>
        /// Non Transaction ExecuteNonQuery
        /// </summary>
        /// <param name="QP"></param>
        private void ExecuteNonQuery(RequestPacket QP)
        {
            DataSet DsTemp = new DataSet();

            if (_DBType == DBType.MSSQL)
            {
                if (QP.CmdType == CommandType.Text)
                {
                    SqlHelper.ExecuteNonQuery(_sqlConnection, CommandType.Text, QP.Service);
                }
                else if (QP.CmdType == CommandType.StoredProcedure)
                {
                    //SqlHelper.ExecuteDataset(_sqlConnection, QP.Service, QP.Param);
                }
            }

            if (_DBType == DBType.ORACLE)
            {
                if (QP.CmdType == CommandType.Text)
                {
                    OracleHelper.ExecuteNonQuery(_oraConnection, CommandType.Text, QP.Service);
                }
                else if (QP.CmdType == CommandType.StoredProcedure)
                {
                    OracleHelper.ExecuteNonQuery(_oraConnection, CommandType.StoredProcedure, QP.Service, QP.OraParams);

                }
            }
        }

        /// <summary>
        /// Oracle Transaction ExecuteNonQuery
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="QP"></param>
        private void ExecuteNonQuery(OracleTransaction transaction, RequestPacket QP)
        {
            DataSet DsTemp = new DataSet();

            if (QP.CmdType == CommandType.Text)
            {
                OracleHelper.ExecuteNonQuery(transaction, CommandType.Text, QP.Service);
            }
            else if (QP.CmdType == CommandType.StoredProcedure)
            {
                OracleHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, QP.Service, QP.OraParams);
            }
        }

        /// <summary>
        /// SQL Transaction ExecuteNonQuery
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="QP"></param>
        private void ExecuteNonQuery(SqlTransaction transaction, RequestPacket QP)
        {
            try
            {
                DataSet DsTemp = new DataSet();

                if (QP.CmdType == CommandType.Text)
                {
                    SqlHelper.ExecuteNonQuery(transaction, CommandType.Text, QP.Service);
                }
                else if (QP.CmdType == CommandType.StoredProcedure)
                {
                    SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, QP.Service, QP.SqlParams);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }
    }
}
