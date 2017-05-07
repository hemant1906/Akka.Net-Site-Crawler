using System;
namespace AKKA_Crawler.Messages
{
    public class CrawlFailure
    {
        public CrawlFailure(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; private set; }
    }
}
