using BPNS.COM;
using System;
using System.Data;


namespace BPNS.DAC
{
    class DatabaseOracle : IDatabase
    {
        #region Connection 관리
        public override IDbConnection CreateConnection()
        {
            throw new NotImplementedException();
        }

        public override IDbConnection CreateOpenConnection()
        {
            throw new NotImplementedException();
        }

        public override IDbConnection CloseConnection()
        {
            if (this.Connection?.State != ConnectionState.Closed)
                this.Connection.Close(); this.Connection.Dispose();

            return this.Connection;

        }
        #endregion

        #region Command 관리
        public override IDbCommand CreateCommand()
        {
            throw new NotImplementedException();
        }

        public override IDbCommand CreateCommand(string commandText, IDbConnection connection, CommandType commandType)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Select
        public override DataSet ExecuteDataSet(string strCommand, ParameterCollection parameters)
        {
            throw new NotImplementedException();
        }

        public override object ExecuteFunction(string strCommand, ParameterCollection parameters)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ExecuteNonQuery
        public override OutParameter ExecuteNonQuery(string strCommand, ParameterCollection parameters)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Helper
        public override void SetDataParameter(IDbCommand command, ParameterCollection parameters)
        {
            throw new NotImplementedException();
        }

        public override void SetOutParameter(IDbCommand command, ref OutParameter outParameter)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
