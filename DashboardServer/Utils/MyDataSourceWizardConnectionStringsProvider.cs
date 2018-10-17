using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardServer.Utils
{
    public class MyDataSourceWizardConnectionStringsProvider : IDataSourceWizardConnectionStringsProvider
    {
        public Dictionary<string, string> GetConnectionDescriptions()
        {
            Dictionary<string, string> connections = new Dictionary<string, string>();
            connections.Add("BrickConnStr", "MS SQL Connection");
            return connections;
        }

        public DataConnectionParametersBase GetDataConnectionParameters(string name)
        {
            if (name == "BrickConnStr")
            {
                return new MsSqlConnectionParameters("sql2008r2", "desenv_PortalEstrategia", "usr_desenvolvimento", "12345678", MsSqlAuthorizationType.Windows);
            }
            throw new System.Exception("The connection string is undefined.");
        }
    }
}
