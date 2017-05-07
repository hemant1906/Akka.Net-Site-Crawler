using System;
namespace AKKA_Crawler.Messages
{
    public class ScrappingError
    {
        public ScrappingError(string url)
        {
            Url = url;
        }
        public string Url { get; private set; }
    }
}
