using Moriyama.Cloud.Umbraco.Interfaces.Application;
using Umbraco.Core.Logging;

namespace Moriyama.Cloud.Umbraco.Application
{
    public class SqlBackedServerInstanceService : IServiceInstanceService
    {
        private static readonly SqlBackedServerInstanceService TheInstance = new SqlBackedServerInstanceService();

        private SqlBackedServerInstanceService()
        {
            
        }

        public static SqlBackedServerInstanceService Instance { get { return TheInstance; } }

        public void Register(string hostName)
        {
            LogHelper.Info(typeof(SqlBackedServerInstanceService), "Registering host " + hostName);
        }
    }
}
