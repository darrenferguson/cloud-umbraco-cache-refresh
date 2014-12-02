using System;
using Moriyama.Cloud.Umbraco.Application;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;

namespace Moriyama.Cloud.Umbraco.Event
{
    public class PublishEvent : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Published += ContentServicePublished;
            ContentService.UnPublished += ContentService_UnPublished;
        }

        void ContentService_UnPublished(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            var host = Environment.MachineName;

            LogHelper.Info(typeof(PublishEvent), "Host " + host + " receieved an unpublish event");

            foreach (var document in e.PublishedEntities)
            {
                LogHelper.Info(typeof(PublishEvent), "Host " + host + " is unpublishing document " + document.Id);
                SqlBackedServerInstanceService.Instance.Publish(host, document.Id);

            }
        }

        static void ContentServicePublished(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            var host = Environment.MachineName;

            LogHelper.Info(typeof(PublishEvent), "Host " + host + " receieved a publish event");

            foreach (var document in e.PublishedEntities)
            {
                LogHelper.Info(typeof(PublishEvent), "Host " + host + " is publishing document " + document.Id);
                SqlBackedServerInstanceService.Instance.Publish(host, document.Id);

            }

        }
    }
}
