using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using log4net.Repository.Hierarchy;
using Moriyama.Cloud.Umbraco.Helper;
using Moriyama.Cloud.Umbraco.Interfaces.Application;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.interfaces.skinning;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
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
            try
            {
                LogHelper.Info(typeof(SqlBackedServerInstanceService), "Registering host " + hostName);

                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // Create the database tables upon startup if they don't exist.
                    CreateSchema(connection);

                    // Run SQL to add the host to the list of alive hosts
                    RegisterHost(connection, hostName);

                    // Cleanup any "old hosts"
                    DeleteHosts(connection);

                    connection.Close();
                    connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(SqlBackedServerInstanceService), "Register", ex);
            }
        }

        public void Publish(string hostName, int documentId)
        {
            Publish(hostName,documentId,"Content");
        }
        
        public void Publish(string hostName, int documentId, string refreshType)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    var publishSql = TextResourceReader.Instance.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.Publish.sql");
                    var command = new SqlCommand(publishSql, connection);
                    command.Parameters.AddWithValue("@DocumentId", documentId);
                    command.Parameters.AddWithValue("@PublishingHost", hostName);
                    command.Parameters.AddWithValue("@RefreshType", refreshType);
                    command.ExecuteNonQuery();

                    LogHelper.Info(typeof(SqlBackedServerInstanceService),
                        "Host " + hostName + " registered the publish of " + documentId);

                    KeepAlive(connection, hostName);
                    DeleteHosts(connection);

                    connection.Close();
                    connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(SqlBackedServerInstanceService), "Publish", ex);
            }
        }

        public void RefreshCache(string hostName)
        {
            try
            {
            LogHelper.Info(typeof(SqlBackedServerInstanceService), "Host " + hostName + " is refreshing it's cache");

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var processedPublishses = new List<Guid>();

                var publishSql =
                    TextResourceReader.Instance.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.RefreshCache.sql");

                var command = new SqlCommand(publishSql, connection);
                command.Parameters.AddWithValue("@HostId", hostName);

                var publishes = command.ExecuteReader();

                while (publishes.Read())
                {
                    var documentId = (int) publishes["DocumentId"];
                    var refreshType = (string) publishes["RefreshType"];
                    var identifier = (Guid) publishes["PublishId"];

                    LogHelper.Debug(typeof(SqlBackedServerInstanceService), "Host " + hostName + " is publishing " + documentId);
                    if (refreshType == "Dictionary")
                    {

                        LogHelper.Info(typeof(SqlBackedServerInstanceService), "Host " + hostName + " is refreshing it's dictionary cache");
                        RefreshDictionaryCache(identifier);
                    }
                    else
                    {
                        LogHelper.Info(typeof(SqlBackedServerInstanceService), "Host " + hostName + " is refreshing it's Content cache");
                        umbraco.library.UpdateDocumentCache(documentId);
                    }
                    LogHelper.Debug(typeof(SqlBackedServerInstanceService), "Host " + hostName + " finished publishing");
                    
                    processedPublishses.Add(identifier);
                }
                publishes.Close();

                if (processedPublishses.Count > 0)
                {
                    var deleteSql =
                        TextResourceReader.Instance.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.DeletePublishes.sql");


                    foreach (var publish in processedPublishses)
                    {
                        command = new SqlCommand("Delete from MoriyamaPublishes where PublishId = @Publishes", connection);
                        command.Parameters.AddWithValue("@Publishes", publish);
                        command.ExecuteNonQuery();
                    }

                    //command = new SqlCommand(deleteSql, connection);
                    //command.Parameters.AddWithValue("@Publishes", string.Join(",", processedPublishses.ToArray()));
                    //command.ExecuteNonQuery();

                    LogHelper.Debug(typeof(SqlBackedServerInstanceService), "Host " + hostName + " removed processed publishses.");
                }

                KeepAlive(connection, hostName);
                DeleteHosts(connection);

                connection.Close();
                connection.Dispose();
                LogHelper.Debug(typeof(SqlBackedServerInstanceService), "Host " + hostName + " finished publishing");

            }
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(SqlBackedServerInstanceService), "RefreshCache", ex);
            }
        }

        private void KeepAlive(SqlConnection connection, string hostName)
        {   
            try
            {
            var updateHostSql = TextResourceReader.Instance.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.UpdateHost.sql");
            var command = new SqlCommand(updateHostSql, connection);
            command.Parameters.AddWithValue("@HostId", hostName);
            command.Parameters.AddWithValue("@AccessTime", DateTime.Now);

            command.ExecuteNonQuery();

            LogHelper.Info(typeof(SqlBackedServerInstanceService), "Host " + hostName + " is alive");
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(SqlBackedServerInstanceService), "RefreshCache", ex);
            }
        }

        private void DeleteHosts(SqlConnection connection)
        {
            try
            {
            // TODO: extract this out to configuration somewhere
            // Ten Minutes
            var expired = DateTime.Now.Subtract(TimeSpan.FromHours(12));

            // This will delete hosts that haven't been active for a while.
            var deleteHostSql = TextResourceReader.Instance.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.DeleteHost.sql");
            var command = new SqlCommand(deleteHostSql, connection);
            command.Parameters.AddWithValue("@AccessTime", expired);
            command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(SqlBackedServerInstanceService), "RefreshCache", ex);
            }
        }

        private void CreateSchema(SqlConnection connection)
        {
             try
            {
            // This will create the database tables upon first run
            var createTableSql = TextResourceReader.Instance.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.Create.sql");
            var command = new SqlCommand(createTableSql, connection);
            command.ExecuteNonQuery();
            }
             catch (Exception ex)
             {
                 LogHelper.Error(typeof(SqlBackedServerInstanceService), "RefreshCache", ex);
             }
        }

        private void RegisterHost(SqlConnection connection, string hostName)
        {
             try
            {
            var createHostSql = TextResourceReader.Instance.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.CreateHost.sql");
            var command = new SqlCommand(createHostSql, connection);
            command.Parameters.AddWithValue("@HostId", hostName);
            command.Parameters.AddWithValue("@AccessTime", DateTime.Now);
            command.ExecuteNonQuery();

            LogHelper.Info(typeof(SqlBackedServerInstanceService), "Host " + hostName + " has been registered");
            }
             catch (Exception ex)
             {
                 LogHelper.Error(typeof(SqlBackedServerInstanceService), "RefreshCache", ex);
             }
        }

        private void RefreshDictionaryCache(Guid publishId )
        {
            //currently the hashtable that contains the cache of dictionary items in memory
            // only gets cleared when new items are added or removed
            // there isn't the function to clear the cache
            //sadly this is coming in 7.3
            //ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IDictionaryItem>();
          // so it would be a horrible hack here to temporarily create / update an existing DictionaryItem called "TempRefreshCache"...
            // .... apologies
            var ls = ApplicationContext.Current.Services.LocalizationService;
            if (!ls.DictionaryItemExists("TempRefreshCache"))
            {
                DictionaryItem newItem = new DictionaryItem("TempRefreshCache");
                foreach (var translation in newItem.Translations)
                {
                    translation.Value = publishId.ToString();
                }
                ls.Save(newItem);
            }
            else
            {
                var dicItem = ls.GetDictionaryItemByKey("TempRefreshCache");
                   foreach (var translation in dicItem.Translations)
                {
                    translation.Value = publishId.ToString();
                }
              ls.Save(dicItem);
            }
        }
    }
}
