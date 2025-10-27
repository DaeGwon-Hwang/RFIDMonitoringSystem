using BPNS.COM;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.DAC
{
    public abstract class IDatabase : IDisposable
    {
        #region Propery
        public ILog Logger
        {
            get
            {
                StackFrame stackFrame = new StackFrame(1, false);
                MethodBase methodBase = stackFrame.GetMethod();
                Type type = methodBase.DeclaringType;

                return LogManager.GetLogger(type);
            }
        }

        public string ConnectionString { get; set; }

        public IDbConnection Connection { get; set; }

        public IDbCommand Command { get; set; }
        #endregion

        #region Connection 관리
        public abstract IDbConnection CreateConnection();

        public abstract IDbConnection CreateOpenConnection();

        public abstract IDbConnection CloseConnection();
        #endregion

        #region Command 관리
        public abstract IDbCommand CreateCommand();

        public abstract IDbCommand CreateCommand(string commandText, IDbConnection connection, CommandType commandType);
        #endregion

        #region Select
        public abstract DataSet ExecuteDataSet(string commandText, ParameterCollection parameters);

        public abstract object ExecuteFunction(string commandText, ParameterCollection parameters);
        #endregion

        #region ExecuteNonQuery
        public abstract OutParameter ExecuteNonQuery(string commandText, ParameterCollection parameters);
        #endregion

        #region Helper
        public abstract void SetDataParameter(IDbCommand command, ParameterCollection parameters);

        public abstract void SetOutParameter(IDbCommand command, ref OutParameter outParameter);
        public abstract void Dispose();
        #endregion
    }
}
