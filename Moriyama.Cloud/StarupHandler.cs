using System;
using Moriyama.Cloud.Umbraco.Application;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Moriyama.Cloud
{
    public class StarupHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication,ApplicationContext applicationContext)
        {
            var host = Environment.MachineName;

            LogHelper.Info(typeof(StarupHandler), "Host " + host + " is starting up");

            // TODO: Discover implementations of the interface
            SqlBackedServerInstanceService.Instance.Register(host);

            LogHelper.Info(typeof(StarupHandler), "Host " + host + " has started");
        }
    }
}
