using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace BPNS.DAC
{
    public class DbProviderFactory
    {
        public static IDatabase GetInstance(string connectionStringName)
        {
            IDatabase iDatabase = null;

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConnectionStringsSection section = config.ConnectionStrings;

            if (section.SectionInformation.IsProtected)
                section.SectionInformation.UnprotectSection();

            string dbProviderName = section.ConnectionStrings[connectionStringName].ProviderName.ToUpper();

            if (dbProviderName.Equals("ORACLE"))
                iDatabase = new DatabaseOracle();
            else if (dbProviderName.Equals("SQLSERVER"))
                iDatabase = new DatabaseSqlServer();

            if (iDatabase != null)
            {
                iDatabase.ConnectionString = section.ConnectionStrings[connectionStringName].ConnectionString;
                iDatabase.Connection = iDatabase.CreateOpenConnection();
            }

            return iDatabase;
        }
    }
}
