using System;
using System.Collections;

namespace BPNS.EdgeMW
{
    interface IDatabaseManagement
    {
        bool bConnected { get; }
        bool Connect();
        void Disconnect();
        void Dispose();
        int ExecuteProcedure(string commandText, Hashtable paramHashtable, int nTimeout);
        System.Data.DataSet ExecuteSelectProcedure(string commandText, Hashtable paramHashtable, int nTimeout);

        System.Data.DataSet ExecuteSelectQuery(string commandText, int nTimeout);

        int ExecuteNonQuery(string commandText, int nTimeout);
        string ExecuteFunctionString(string commandText, Hashtable paramHashtable, int nTimeout);
        void Reconnect();
    }
}
