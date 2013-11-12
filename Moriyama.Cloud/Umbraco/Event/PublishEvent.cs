using System;
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
        }

        void ContentServicePublished(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            var host = Environment.MachineName;

            LogHelper.Info(typeof(PublishEvent), "Host " + host + " receieved a publish event");

            foreach (var document in e.PublishedEntities)
            {
                LogHelper.Info(typeof(PublishEvent), "Host " + host + " is publishing document " + document.Id);
            }

        }
    }
}
