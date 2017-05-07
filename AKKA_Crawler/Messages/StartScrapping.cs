using System;

namespace AKKA_Crawler.Messages
{
    public class StartScrapping
    {
        public StartScrapping(string pageToScrap)
        {
            PageToScrap = pageToScrap;
        }

        public string PageToScrap { get; private set; }
    }
}
