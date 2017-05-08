using System.IO;
using System.Net;
using AKKA_Crawler.Messages;

namespace AKKA_Crawler.Actors
{
    public class PageScrapResult
    {
        public StartScrapping Page { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public Stream Result { get; private set; }


        public PageScrapResult(StartScrapping page, HttpStatusCode statusCode)
        {
            this.Page = page;
            this.StatusCode = statusCode;
        }

        public PageScrapResult(StartScrapping page, HttpStatusCode statusCode, Stream result) : this(page, statusCode)
        {

            this.Result = result;
        }
    }
}