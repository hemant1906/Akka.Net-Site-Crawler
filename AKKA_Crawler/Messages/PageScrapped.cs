using System;
using HtmlAgilityPack;
namespace AKKA_Crawler.Messages
{
    public class PageScrapped
    {
        public PageScrapped(string pageName, string[] pageContent)
        {
            PageName = pageName;
            ChildPages = pageContent;
        }

        public string PageName { get; private set; }
        public string[] ChildPages { get; private set; }
    }
}
