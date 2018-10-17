using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DevExpress.DashboardAspNetCore;
using DevExpress.AspNetCore;
using DashboardServer.Utils;
using System.Data.SqlClient;
using DevExpress.DashboardWeb;
using DevExpress.DashboardCommon;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Web;

namespace DashboardServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddDefaultDashboardController((configurator, serviceProvider) =>
                {
                    //var connection = new SqlConnection(ConnectionString);
                    var dashboardStorage = new DataBaseEditaleDashboardStorage();
                    configurator.SetDashboardStorage(dashboardStorage);

                    var conexao = new SqlConnectionStringBuilder(Configuration.GetConnectionString("BrickConnStr"));
                    conexao.UserID = "usr_cdis_report";
                    conexao.Password = "123456";
                    var parameters = new CustomStringConnectionParameters(conexao.ConnectionString);
                    var sqlDataSource = new DashboardSqlDataSource("Fonte de dados padrão", parameters);

                    var dataSourceStorage = new DataSourceInMemoryStorage();
                    dataSourceStorage.RegisterDataSource("sqlDataSource1", sqlDataSource.SaveToXml());
                    configurator.SetDataSourceStorage(dataSourceStorage);

                    /*const string defaultSourceName = "Fonte de dados padrão";
                    const string connectionName = "mssql-connection";
                    var sqlDataSource = new DashboardSqlDataSource(defaultSourceName, connectionName);
                    var dataSourceStorage = new DataSourceInMemoryStorage();
                    dataSourceStorage.RegisterDataSource("sqlDataSource1", sqlDataSource.SaveToXml());
                    configurator.SetDataSourceStorage(dataSourceStorage);

                    configurator.ConfigureDataConnection += (s, e) =>
                    {
                        if (e.ConnectionName == connectionName)
                        {
                            var conexao = new SqlConnectionStringBuilder(ConnectionString);
                            conexao.UserID = "usr_cdis_report";
                            conexao.Password = "123456";
                            e.ConnectionParameters = new CustomStringConnectionParameters(conexao.ConnectionString);
                        }
                    };*/
                    //configurator.SetConnectionStringsProvider(new MyDataSourceWizardConnectionStringsProvider());

                    configurator.ConfigureDataConnection += (s, e) =>
                    {

                    };

                    configurator.AllowExecutingCustomSql = true;
                    DashboardConfigurator.PassCredentials = true;
                });
            services.AddDevExpressControls(settings => settings.Resources = ResourcesType.ThirdParty | ResourcesType.DevExtreme);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.UseDevExpressControls();
            app.UseCors(c =>
            {
                c.AllowAnyHeader();
                c.AllowAnyMethod();
                c.AllowAnyOrigin();
            });
            app.UseMvc(routes =>
            {
                // Map dashboard routes.
                routes.MapDashboardRoute("dashboard");
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            }
            );
        }
    }
}
