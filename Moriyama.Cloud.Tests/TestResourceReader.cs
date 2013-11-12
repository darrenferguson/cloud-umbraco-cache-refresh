using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moriyama.Cloud.Umbraco.Helper;

namespace Moriyama.Cloud.Tests
{
    [TestClass]
    public class TestResourceReader
    {
        [TestMethod]
        public void TestMethod1()
        {
            var resourceReader = TextResourceReader.Instance;

            var text = resourceReader.ReadResourceFile("Moriyama.Cloud.Umbraco.Sql.Create.sql");
            Assert.IsTrue(text.Length > 0);
        }
    }
}
