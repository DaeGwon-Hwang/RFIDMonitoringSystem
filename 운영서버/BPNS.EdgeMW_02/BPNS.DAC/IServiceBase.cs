using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.DAC
{
    public interface IServiceBase
    {
        DataSet ExecuteDataSet(RequestService service);
        void ExecuteNonQuery(RequestService service);
    }
}
