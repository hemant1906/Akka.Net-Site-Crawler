using Akka.Actor;
using System;
namespace AKKA_Crawler.Messages
{
    public class CrawlSite
    {
        public CrawlSite(string siteUrl, IActorRef parent)
        {
            SiteUrl = siteUrl;
            Parent = parent;
        }

        public string SiteUrl { get; private set; }
        public IActorRef Parent { get; private set; }
    }
}
