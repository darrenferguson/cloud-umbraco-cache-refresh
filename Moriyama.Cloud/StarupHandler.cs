using System;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Moriyama.Cloud.Umbraco.Application;
using Moriyama.Cloud.Umbraco.Application.Module;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Moriyama.Cloud
{
    public class StarupHandler : ApplicationEventHandler
    {
        private static bool _modulesRegistered;

        public static void RegisterModules()
        {
            if (_modulesRegistered)
                return;

            DynamicModuleUtility.RegisterModule(typeof(UmbracoCacheRefreshingHttpModule));
            _modulesRegistered = true;
        }

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
