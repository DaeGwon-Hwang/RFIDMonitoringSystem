using BPNS.COM;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace BPNS.DAC
{
    class DatabaseSqlServer : IDatabase
    {
        #region Connection 관리
        public override IDbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public override IDbConnection CreateOpenConnection()
        {
            if (this.Connection == null)
                this.Connection = (SqlConnection)CreateConnection();

            if (this.Connection.State != ConnectionState.Open)
                this.Connection.Open();

            return this.Connection;
        }

        public override IDbConnection CloseConnection()
        {
            if (this.Connection?.State != ConnectionState.Closed)
            {
                this.Connection.Dispose();
                this.Connection.Close();
            }


            return this.Connection;

        }
        #endregion

        #region Command 관리
        public override IDbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        public override IDbCommand CreateCommand(string commandText, IDbConnection connection, CommandType commandType)
        {
            SqlCommand command = null;

            string[] commandTextToken = commandText.Split('.');

            command = (SqlCommand)CreateCommand();
            command.CommandText = commandText.Substring(commandText.IndexOf('.') + 1); //commandTextToken[commandTextToken.Length - 1];
            command.Connection = (SqlConnection)connection;
            command.CommandType = commandType;

            return command;
        }
        #endregion

        #region Select
        public override DataSet ExecuteDataSet(string commandText, ParameterCollection parameters)
        {
            DataSet dataSet = new DataSet();

            if (this.Connection.State != ConnectionState.Open)
                this.Connection.Open();

            this.Command = CreateCommand(commandText, this.Connection, CommandType.StoredProcedure);

            SetDataParameter(this.Command, parameters);

            SqlDataAdapter dataAdapter = new SqlDataAdapter((SqlCommand)this.Command);
            dataAdapter.Fill(dataSet);

            dataAdapter.Dispose();

            return dataSet;
        }

        public override object ExecuteFunction(string commandText, ParameterCollection parameters)
        {
            if (this.Connection.State != ConnectionState.Open)
                this.Connection.Open();

            this.Command = CreateCommand(commandText, this.Connection, CommandType.StoredProcedure);

            SetDataParameter(this.Command, parameters);

            Command.ExecuteScalar();

            object objReturnValue = null;

            IEnumerator enumerator = parameters.GetEnumerator();

            while (enumerator.MoveNext())
            {
                Parameter param = enumerator.Current as Parameter;

                if (param.Direction == ParameterDirection.ReturnValue)
                {
                    objReturnValue = (Command as SqlCommand).Parameters[param.isGridColumn ? parameters.GridColumnPreFix + param.ParamName : param.ParamName].Value;
                    break;
                }
            }

            return objReturnValue;
        }
        #endregion

        #region ExecuteNonQuery
        public override OutParameter ExecuteNonQuery(string commandText, ParameterCollection parameters)
        {
            try
            {
                OutParameter outParameter = new OutParameter();

                if (this.Connection.State != ConnectionState.Open)
                    this.Connection.Open();

                this.Command = CreateCommand(commandText, this.Connection, CommandType.StoredProcedure);

                SetDataParameter(this.Command, parameters);

                this.Command.ExecuteNonQuery();

                SetOutParameter(this.Command, ref outParameter);

                Command.Dispose();

                return outParameter;
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }
        #endregion

        #region Helper
        public override void SetDataParameter(IDbCommand command, ParameterCollection parameters)
        {
            SqlCommand sqlCommand = command as SqlCommand;

            IEnumerator enumerator = parameters.GetEnumerator();

            while (enumerator.MoveNext())
            {
                Parameter param = enumerator.Current as Parameter;

                if (param.ParamName.StartsWith("I_") || param.isGridColumn)
                {
                    SqlParameter sqlParameter = new SqlParameter();

                    sqlParameter.ParameterName = string.Format("@{0}{1}", param.isGridColumn ? parameters.GridColumnPreFix : string.Empty, param.ParamName);
                    sqlParameter.Direction = ParameterDirection.Input;
                    sqlParameter.SqlDbType = SqlDbType.VarChar;
                    sqlParameter.Value = param.Value;

                    sqlCommand.Parameters.Add(sqlParameter);
                }
                else if (param.ParamName.StartsWith("O_"))
                {
                    if (!param.DbType.ToUpper().StartsWith("REFCURSOR"))
                    {
                        SqlParameter sqlParameter = new SqlParameter();

                        sqlParameter.ParameterName = string.Format("@{0}", param.ParamName);
                        sqlParameter.Direction = param.Direction;
                        sqlParameter.Size = param.Size;
                        sqlParameter.SqlDbType = SqlDbType.VarChar;

                        sqlCommand.Parameters.Add(sqlParameter);
                    }
                }
            }
        }

        public override void SetOutParameter(IDbCommand command, ref OutParameter outParameter)
        {
            SqlCommand sqlCommand = command as SqlCommand;

            foreach (SqlParameter param in sqlCommand.Parameters)
                if (param.Direction == ParameterDirection.Output)
                    outParameter.Add(param.ParameterName, param.Value);
        }

        public override void Dispose()
        {
            this.Connection.Dispose();
        }
        #endregion
    }
}
