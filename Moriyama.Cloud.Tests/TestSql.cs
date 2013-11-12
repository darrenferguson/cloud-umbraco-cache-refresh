using System;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moriyama.Cloud.Umbraco.Helper;

namespace Moriyama.Cloud.Tests
{
    [TestClass]
    public class TestSql
    {
        [TestMethod]
        public void TestMethod1()
        {
            var resourceReader = TextResourceReader.Instance;
            var host = System.Environment.MachineName;

            var text = resourceReader.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.Create.sql");
            Assert.IsTrue(text.Length > 0);

            var c = new SqlConnection("Data Source=localhost; Initial Catalog=master; Integrated Security=SSPI");
            c.Open();

            //var uid = "moriyamaunittest4";

            //var cmnd = new SqlCommand("create database " + uid, c);
            //cmnd.ExecuteNonQuery();

            //cmnd = new SqlCommand(text, c);
            //cmnd.ExecuteNonQuery();


            var createHostSql = resourceReader.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.CreateHost.sql");
            var cmnd = new SqlCommand(createHostSql, c);
            cmnd.Parameters.AddWithValue("@HostId", host);
            cmnd.Parameters.AddWithValue("@AccessTime", DateTime.Now);
            cmnd.ExecuteNonQuery();


            var updateHostSql = resourceReader.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.UpdateHost.sql");
            cmnd = new SqlCommand(updateHostSql, c);
            cmnd.Parameters.AddWithValue("@HostId", host);
            cmnd.Parameters.AddWithValue("@AccessTime", DateTime.Now);
            cmnd.ExecuteNonQuery();

            var deleteHostSql = resourceReader.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.DeleteHost.sql");
            cmnd = new SqlCommand(deleteHostSql, c);
            cmnd.Parameters.AddWithValue("@AccessTime", DateTime.Now);
            cmnd.ExecuteNonQuery();

            //cmnd = new SqlCommand("drop database " + uid, c);
            //cmnd.ExecuteNonQuery();
            
            c.Close();
        }
    }
}
