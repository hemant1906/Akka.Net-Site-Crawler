using System;
namespace AKKA_Crawler.Messages
{
    public class CrawlComplete
    {
        public CrawlComplete(string siteUrl)
        {
            SiteUrl = siteUrl;
        }

        public string SiteUrl { get; private set; }
    }
}
