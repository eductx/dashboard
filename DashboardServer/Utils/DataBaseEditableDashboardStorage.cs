using Dapper;
using DevExpress.DashboardWeb;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DashboardServer.Utils
{
    public class DataBaseEditaleDashboardStorage : IEditableDashboardStorage
    {
        protected Dictionary<string, string> DashboardDictionary { get; } = new Dictionary<string, string>();

        protected IDbConnection Connection => new SqlConnection("Data Source=sql2008r2;Initial Catalog=desenv_PortalEstrategia;Persist Security Info=True;User ID=usr_desenvolvimento;Password=12345678");

        public DataBaseEditaleDashboardStorage(/* IDbConnection connection*/)
            : base()
        {
            //Connection = connection;
        }

        public string AddDashboard(XDocument dashboard, string dashboardName)
        {
            const string sql = @"
INSERT INTO [dbo].[Dashboard]
           ([IDDashboard]
           ,[XMLDashboard]
           ,[TituloDashboard]
           ,[Descricao]
           ,[IniciaisControle]
           ,[TipoAssociacao])
     VALUES
           (@id
           , @dashboard
           , @dashboardName
           , NULL
           , NULL
           , NULL)";

            // if (Connection.State == ConnectionState.Closed)
            //     Connection.Open();

            var dashboardID = Guid.NewGuid();
            Connection.Execute(sql,
                param: new
                {
                    id = dashboardID,
                    dashboardName,
                    dashboard = dashboard.ToString()
                });

            return dashboardID.ToString();
        }

        public XDocument LoadDashboard(string dashboardID)
        {
            if (!DashboardDictionary.ContainsKey(dashboardID))
            {
                DashboardDictionary.Add(dashboardID, string.Empty);
            }

            string dashboard;
            if (string.IsNullOrEmpty(dashboard = DashboardDictionary[dashboardID]))
            {
                string GetDashboardContent()
                {
                    const string sql = "SELECT [XMLDashboard] FROM [dbo].[Dashboard] WHERE [IDDashboard] = @id";
                    return Connection.ExecuteScalar<string>(sql, new { id = dashboardID });
                }
                dashboard = GetDashboardContent();
                DashboardDictionary[dashboardID] = dashboard;
            }

            XDocument xDocument = GetDashboardXmlDocument(dashboard);

            return xDocument;
        }

        private XDocument GetDashboardXmlDocument(string dashboard)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(Connection.ConnectionString);
            var dicionarioParametros = new Dictionary<string, string>
            {
                { "userid", connectionStringBuilder.UserID },
                { "password", connectionStringBuilder.Password },
                { "server", connectionStringBuilder.DataSource },
                { "database", connectionStringBuilder.InitialCatalog }
            };
            var xDocument = XDocument.Parse(dashboard);
            var parameters = xDocument
                .XPathSelectElements("/Dashboard/DataSources/SqlDataSource/Connection/Parameters/Parameter")
                .Where(el => dicionarioParametros.Keys.Contains(el.Attribute("Name").Value));

            foreach (var parameter in parameters)
            {
                var parameterName = parameter.Attribute("Name").Value;
                var parameterValue = dicionarioParametros[parameterName];
                parameter.SetAttributeValue("Value", parameterValue);
            }

            return xDocument;
        }

        public IEnumerable<DashboardInfo> GetAvailableDashboardsInfo()
        {
            const string sql = "SELECT [IDDashboard], [TituloDashboard] FROM [dbo].[Dashboard]";
            var reader = Connection.ExecuteReader(sql);
            while (reader.Read())
            {
                var dashboardName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                var dashboardId = reader.GetGuid(0).ToString();
                // if (!DashboardDictionary.ContainsKey(dashboardId))
                //     DashboardDictionary.Add(dashboardId, string.Empty);

                yield return new DashboardInfo
                {
                    Name = dashboardName,
                    ID = dashboardId
                };
            }
            reader.Close();
        }

        public void SaveDashboard(string dashboardID, XDocument document)
        {
            const string sql = @"
UPDATE [dbo].[Dashboard]
   SET [XMLDashboard] = @dashboard
 WHERE [IDDashboard] = @id";
            var dashboard = document.ToString();
            Connection.Execute(sql,
                param: new
                {
                    id = dashboardID,
                    dashboard
                });

            if (DashboardDictionary.ContainsKey(dashboardID))
                DashboardDictionary[dashboardID] = dashboard;
            else
                DashboardDictionary.Add(dashboardID, dashboard);
        }
    }
}