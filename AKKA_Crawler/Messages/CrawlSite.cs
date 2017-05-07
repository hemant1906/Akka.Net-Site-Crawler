using System;
namespace AKKA_Crawler.Messages
{
    public class CrawlSite
    {
        public CrawlSite(string siteUrl)
        {
            SiteUrl = siteUrl;
        }

        public string SiteUrl { get; private set; }
    }
}
