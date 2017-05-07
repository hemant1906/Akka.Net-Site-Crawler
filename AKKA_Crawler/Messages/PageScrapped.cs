using System;
using HtmlAgilityPack;
namespace AKKA_Crawler.Messages
{
    public class PageScrapped
    {
        public PageScrapped(string pageName, HtmlDocument pageContent )
        {
            PageName = pageName;
            PageContent = pageContent;
        }

        public string PageName {  get; private set; }
        public HtmlDocument PageContent { get; private set; }
    }
}
