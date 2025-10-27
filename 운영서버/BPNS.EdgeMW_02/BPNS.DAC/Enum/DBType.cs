using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.DAC
{
    /// <summary>
    /// 트랜잭션타입
    /// </summary>
    public enum DBType
    {
        ORACLE,
        MSSQL
    }

    /// <summary>
	/// 트랜잭션타입
	/// </summary>
	public enum TxType
    {
        Required,
        NotSupported
    }
}
