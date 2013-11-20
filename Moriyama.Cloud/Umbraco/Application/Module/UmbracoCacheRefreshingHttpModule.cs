using System;
using System.Web;

namespace Moriyama.Cloud.Umbraco.Application.Module
{
    public class UmbracoCacheRefreshingHttpModule :IHttpModule
    {
        private const string AppVarName = "MoriyamaLastCacheRefreshTime";

        public void Init(HttpApplication context)
        {
            if (context.Application[AppVarName] == null)
                context.Application[AppVarName] = DateTime.Now;

            context.PostReleaseRequestState += ContextPostReleaseRequestState;
        }

        static void ContextPostReleaseRequestState(object sender, EventArgs e)
        {
            var context = sender as HttpApplication;
            if (!context.Response.ContentType.Contains("text/html")) return;

            lock (context.Application[AppVarName])
            {
                var lastCheck = (DateTime)context.Application[AppVarName];
                var tenSecondsAgo = DateTime.Now.Subtract(TimeSpan.FromSeconds(10));

                if (lastCheck >= tenSecondsAgo) return;

                context.Application[AppVarName] = DateTime.Now;
                var host = Environment.MachineName;
                SqlBackedServerInstanceService.Instance.RefreshCache(host);
            }
        }

        public void Dispose()
        {
            
        }
    }
}
