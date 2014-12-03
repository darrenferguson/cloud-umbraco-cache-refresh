using System;
using System.Collections.Generic;
using Moriyama.Cloud.Umbraco.Application;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Moriyama.Cloud.Umbraco.Event
{
    public class PublishEvent : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Published += (sender, args) => ContentServiceEvent(args.PublishedEntities, "publish");
            ContentService.UnPublished += (sender, args) => ContentServiceEvent(args.PublishedEntities, "unpublish");
            ContentService.Copied += (sender, args) => ContentServiceEvent(new List<IContent>(new[] { args.Copy }), "copy");
            ContentService.Deleted += (sender, args) => ContentServiceEvent(args.DeletedEntities, "delete");
            ContentService.Moved += (sender, args) => ContentServiceEvent(new List<IContent>(new[] { args.Entity }), "move");
            ContentService.RolledBack += (sender, args) => ContentServiceEvent(new List<IContent>(new[] { args.Entity }), "rollback");
            ContentService.Trashed += (sender, args) => ContentServiceEvent(new List<IContent>(new[] { args.Entity }), "trashed");

            MediaService.Deleted += (sender, args) => MediaServiceEvent(args.DeletedEntities, "delete media");
            MediaService.Saved += (sender, args) => MediaServiceEvent(args.SavedEntities, "saved media");
            MediaService.Trashed += (sender, args) => MediaServiceEvent(new List<IMedia>(new[] { args.Entity }), "delete media");
        }



        static void ContentServiceEvent(IEnumerable<IContent> documents, string eventName)
        {
            var host = Environment.MachineName;

            LogHelper.Info(typeof(PublishEvent), "Host " + host + " receieved a " + eventName + " event");

            foreach (var document in documents)
            {
                LogHelper.Info(typeof(PublishEvent), "Host " + host + " requesting cache update due to " + eventName + " of document " + document.Id);
                SqlBackedServerInstanceService.Instance.Publish(host, document.Id);
            }

        }

        static void MediaServiceEvent(IEnumerable<IMedia> media, string eventName)
        {
            var host = Environment.MachineName;

            LogHelper.Info(typeof(PublishEvent), "Host " + host + " receieved a " + eventName + " event");

            foreach (var mediaItem in media)
            {
                LogHelper.Info(typeof(PublishEvent), "Host " + host + " requesting cache update due to " + eventName + " of media item " + mediaItem.Id);
                SqlBackedServerInstanceService.Instance.Publish(host, mediaItem.Id);
            }

        }
    }
}
