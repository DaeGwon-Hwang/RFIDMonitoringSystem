using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.DAC
{
    public class DBParams
    {
        string m_strName;
        object m_objValue;
        OracleType m_oracleType;

        public DBParams()
        {
            m_strName = "";
            m_oracleType = OracleType.VarChar;
            m_objValue = null;
        }

        public DBParams(string strName, OracleType oraType, object oValue)
        {
            m_strName = strName;
            m_oracleType = oraType;
            m_objValue = oValue;
        }

        public string ParamName
        {
            get { return this.m_strName; }
            set { this.m_strName = value; }
        }

        public OracleType ParamType
        {
            get { return this.m_oracleType; }
            set { this.m_oracleType = value; }
        }

        public object ParamValue
        {
            get { return this.m_objValue; }
            set { this.m_objValue = value; }
        }
    }

    public class RequestPacket
    {
        string m_strTableName;
        string m_strServiceName;

        CommandType m_CmdType;

        SqlParameter[] m_SqlParms;
        OracleParameter[] m_OracleParms;

        public RequestPacket()
        {
            m_strTableName = "";
            m_strServiceName = "";
            m_CmdType = CommandType.Text;
        }

        public RequestPacket(string strTableName, CommandType cmdType)
        {
            m_strTableName = strTableName;
            m_CmdType = cmdType;
        }

        public RequestPacket(string strTableName, CommandType cmdType, string strService)
        {
            m_strTableName = strTableName;
            m_CmdType = cmdType;
            m_strServiceName = strService;
        }

        public RequestPacket(string strTableName, CommandType cmdType, string strService, OracleParameter[] OraParm, SqlParameter[] SqlParm)
        {
            m_strTableName = strTableName;
            m_strServiceName = strService;
            m_CmdType = cmdType;
            m_OracleParms = OraParm;
            m_SqlParms = SqlParm;
        }

        public string TableName
        {
            get { return this.m_strTableName; }
            set { this.m_strTableName = value; }
        }

        public string Service
        {
            get { return this.m_strServiceName; }
            set { this.m_strServiceName = value; }
        }

        public CommandType CmdType
        {
            get { return this.m_CmdType; }
            set { this.m_CmdType = value; }
        }

        public OracleParameter[] OraParams
        {
            get { return m_OracleParms; }
            set { this.m_OracleParms = value; }
        }

        public SqlParameter[] SqlParams
        {
            get { return this.m_SqlParms; }
            set { this.m_SqlParms = value; }
        }
    }
}
