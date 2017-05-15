using System.IO;
using System.Net;
using AKKA_Crawler.Messages;

namespace AKKA_Crawler.Actors
{
    public class PageScrapResult
    {
        public string Page { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public Stream Result { get; private set; }


        public PageScrapResult(string page, HttpStatusCode statusCode)
        {
            this.Page = page;
            this.StatusCode = statusCode;
        }

        public PageScrapResult(string page, HttpStatusCode statusCode, Stream result) : this(page, statusCode)
        {

            this.Result = result;
        }
    }
}