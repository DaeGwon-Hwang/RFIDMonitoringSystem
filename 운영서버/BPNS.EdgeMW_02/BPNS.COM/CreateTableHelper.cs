using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.COM
{
    public class CreateTableHelper
    {
        public static DataTable CreateReaderTable()
        {
            DataTable dt = new DataTable();

            DataColumn DCReaderId = new DataColumn();
            DCReaderId.ColumnName = "ReaderID";

            dt.Columns.Add(DCReaderId);

            DataColumn DCUID = new DataColumn();
            DCUID.ColumnName = "TagUID";

            dt.Columns.Add(DCUID);

            DataColumn DCAnt = new DataColumn();
            DCAnt.ColumnName = "Ant";

            dt.Columns.Add(DCAnt);

            DataColumn DCCount = new DataColumn();
            DCCount.ColumnName = "Count";

            dt.Columns.Add(DCCount);

            DataColumn DCRssI = new DataColumn();
            DCRssI.ColumnName = "RSSI";

            dt.Columns.Add(DCRssI);

            DataColumn DCTime = new DataColumn();
            DCTime.ColumnName = "Time";

            dt.Columns.Add(DCTime);


            return dt;
        }
    }
}
