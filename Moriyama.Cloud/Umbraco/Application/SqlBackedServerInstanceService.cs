using System;
using System.Configuration;
using System.Data.SqlClient;
using Moriyama.Cloud.Umbraco.Helper;
using Moriyama.Cloud.Umbraco.Interfaces.Application;
using Umbraco.Core.Logging;

namespace Moriyama.Cloud.Umbraco.Application
{
    public class SqlBackedServerInstanceService : IServiceInstanceService
    {
        private static readonly SqlBackedServerInstanceService TheInstance = new SqlBackedServerInstanceService();

        private SqlBackedServerInstanceService()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["umbracoDbDSN"].ConnectionString;
        }

        public static SqlBackedServerInstanceService Instance { get { return TheInstance; } }

        private string ConnectionString { get; set; }

        public void Register(string hostName)
        {
            LogHelper.Info(typeof(SqlBackedServerInstanceService), "Registering host " + hostName);
            
            var resourceReader = TextResourceReader.Instance;
            
            var connection = new SqlConnection(ConnectionString);
            connection.Open();

            // This will create the database tables upon first run
            var createTableSql = resourceReader.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.Create.sql");
            var command = new SqlCommand(createTableSql, connection);
            command.ExecuteNonQuery();

            // Run SQL to add the host to the list of alive hosts
            var createHostSql = resourceReader.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.CreateHost.sql");
            command = new SqlCommand(createHostSql, connection);
            command.Parameters.AddWithValue("@HostId", hostName);
            command.Parameters.AddWithValue("@AccessTime", DateTime.Now);
            command.ExecuteNonQuery();

            // TODO: extract this out to configuration somewhere
            var expired = DateTime.Now.Subtract(TimeSpan.FromSeconds(1200));

            // This will delete hosts that haven't been active for a while.
            var deleteHostSql = resourceReader.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.DeleteHost.sql");
            command = new SqlCommand(deleteHostSql, connection);
            command.Parameters.AddWithValue("@AccessTime", expired);
            command.ExecuteNonQuery();
            
            connection.Close();
        }
    }
}
