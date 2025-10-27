using BPNS.DAC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.BIZ.Base
{
    class TxBaseService
    {
        /// <summary>
        /// Text 기반의 SQL Query 전송
        /// </summary>
        /// <param name="strDB"></param>
        /// <param name="arSql"></param>
        public static void ExecuteNoneQuery(string strDB, ArrayList arSql)
        {
            try
            {
                IServiceBase SB = new ServiceBase();

                RequestService service = new RequestService();
                service.DB = strDB;
                service.TransactionType = TxType.Required;

                for (int nLoop = 0; nLoop < arSql.Count; nLoop++)
                    service.AddService(new RequestPacket("", System.Data.CommandType.Text, arSql[nLoop].ToString()));

                SB.ExecuteNonQuery(service);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Procedure 기반의 SQL Query 전송
        /// </summary>
        /// <param name="service"></param>
        public static void ExecuteNoneQueryBySP(string strDB, string strPGName, OracleParameter[] OraParm, SqlParameter[] SqlParm)
        {
            try
            {
                IServiceBase SB = new ServiceBase();

                RequestService service = new RequestService();
                service.DB = strDB;
                service.TransactionType = TxType.Required;
                service.AddService(new RequestPacket("", CommandType.StoredProcedure, strPGName, OraParm, SqlParm));

                SB.ExecuteNonQuery(service);
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }

        /// <summary>
        /// Procedure 기반의 SQL Query 전송
        /// </summary>
        /// <param name="service"></param>
        public static void ExecuteNoneQueryBySP(string strDB, RequestService service)
        {
            try
            {
                IServiceBase SB = new ServiceBase();

                service.DB = strDB;
                service.TransactionType = TxType.Required;

                SB.ExecuteNonQuery(service);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Excecute DataSet
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static DataSet ExecuteDataSet(string strDB, params string[] strQuery)
        {
            try
            {
                IServiceBase SB = new ServiceBase();

                RequestService service = new RequestService();
                service.DB = strDB;
                service.TransactionType = TxType.Required;

                for (int nLoop = 0; nLoop < strQuery.Length; nLoop++)
                    service.AddService(new RequestPacket("Table" + nLoop.ToString(), CommandType.Text, strQuery[nLoop]));

                return SB.ExecuteDataSet(service);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Excecute DataSet Stored Procedure
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static DataSet ExecuteDataSetBySP(string strDB, string strPGName, OracleParameter[] OraParm, SqlParameter[] SqlParm)
        {
            try
            {
                IServiceBase SB = new ServiceBase();

                RequestService service = new RequestService();
                service.DB = strDB;
                service.TransactionType = TxType.Required;

                service.AddService(new RequestPacket("TABLE1", CommandType.StoredProcedure, strPGName, OraParm, SqlParm));

                return SB.ExecuteDataSet(service);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
