using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPNS.DAC
{
    public class ConnectionSetting
    {
        public static string GetConnectionString(string strDB)
        {
            string strConnectionString = ConfigurationManager.ConnectionStrings[strDB].ConnectionString;
            string[] strConnectionStrings = strConnectionString.Split(new string[] { "::" }, StringSplitOptions.None);
            return strConnectionStrings[1];
        }
    }
}
