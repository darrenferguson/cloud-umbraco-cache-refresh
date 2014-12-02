using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Moriyama.Cloud.Umbraco.Helper;
using Moriyama.Cloud.Umbraco.Interfaces.Application;
using umbraco.BusinessLogic;
using Umbraco.Core.Logging;
using UmbracoCms = Umbraco;

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

            var connection = new SqlConnection(ConnectionString);
            connection.Open();

            // Create the database tables upon startup if they don't exist.
            CreateSchema(connection);

            // Run SQL to add the host to the list of alive hosts
            RegisterHost(connection, hostName);

            // Cleanup any "old hosts"
            DeleteHosts(connection);

            connection.Close();
        }

        public void Publish(string hostName, int documentId)
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();

            var publishSql = TextResourceReader.Instance.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.Publish.sql");
            var command = new SqlCommand(publishSql, connection);
            command.Parameters.AddWithValue("@DocumentId", documentId);
            command.Parameters.AddWithValue("@PublishingHost", hostName);

            command.ExecuteNonQuery();

            LogHelper.Info(typeof(SqlBackedServerInstanceService), "Host " + hostName + " registered the publish of " + documentId);

            KeepAlive(connection, hostName);
            DeleteHosts(connection);

            connection.Close();
        }

        public void RefreshCache(string hostName)
        {
            LogHelper.Info(typeof(SqlBackedServerInstanceService), "Host " + hostName + " is refreshing it's cache");

            var connection = new SqlConnection(ConnectionString);
            connection.Open();

            var processedPublishses = new List<Guid>();

            var publishSql = TextResourceReader.Instance.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.RefreshCache.sql");
            var command = new SqlCommand(publishSql, connection);
            command.Parameters.AddWithValue("@HostId", hostName);

            var publishes = command.ExecuteReader();

            while (publishes.Read())
            {
                var documentId = (int)publishes["DocumentId"];
                var identifier = (Guid)publishes["PublishId"];

                // do the actual cache refresh here!
                var user = User.GetUser(0);
                var webService = new umbraco.presentation.webservices.CacheRefresher();
                webService.RefreshAll(new Guid(UmbracoCms.Web.Cache.DistributedCache.PageCacheRefresherId), user.LoginName, user.GetPassword());
//                umbraco.library.UpdateDocumentCache(documentId);
                processedPublishses.Add(identifier);
            }
            publishes.Close();

            if (processedPublishses.Count > 0)
            {
                var deleteSql =
                    TextResourceReader.Instance.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.DeletePublishes.sql");

                command = new SqlCommand(deleteSql, connection);
                command.Parameters.AddWithValue("@Publishes", string.Join(",", processedPublishses.ToArray()));
                command.ExecuteNonQuery();
            }

            KeepAlive(connection, hostName);
            DeleteHosts(connection);

            connection.Close();
        }

        private void KeepAlive(SqlConnection connection, string hostName)
        {
            var updateHostSql = TextResourceReader.Instance.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.UpdateHost.sql");
            var command = new SqlCommand(updateHostSql, connection);
            command.Parameters.AddWithValue("@HostId", hostName);
            command.Parameters.AddWithValue("@AccessTime", DateTime.Now);

            command.ExecuteNonQuery();
        }

        private void DeleteHosts(SqlConnection connection)
        {
            // TODO: extract this out to configuration somewhere
            // Ten Minutes
            var expired = DateTime.Now.Subtract(TimeSpan.FromSeconds(600));

            // This will delete hosts that haven't been active for a while.
            var deleteHostSql = TextResourceReader.Instance.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.DeleteHost.sql");
            var command = new SqlCommand(deleteHostSql, connection);
            command.Parameters.AddWithValue("@AccessTime", expired);
            command.ExecuteNonQuery();
        }

        private void CreateSchema(SqlConnection connection)
        {
            // This will create the database tables upon first run
            var createTableSql = TextResourceReader.Instance.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.Create.sql");
            var command = new SqlCommand(createTableSql, connection);
            command.ExecuteNonQuery();
        }

        private void RegisterHost(SqlConnection connection, string hostName)
        {
            var createHostSql = TextResourceReader.Instance.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.CreateHost.sql");
            var command = new SqlCommand(createHostSql, connection);
            command.Parameters.AddWithValue("@HostId", hostName);
            command.Parameters.AddWithValue("@AccessTime", DateTime.Now);
            command.ExecuteNonQuery();
        }
    }
}
