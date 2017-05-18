using System.IO;
using System.Net;
using AKKA_Crawler.Messages;
using Akka.Actor;

namespace AKKA_Crawler.Messages
{
    public class PageScrapResult
    {
        public string Page { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public Stream Result { get; private set; }
        public IActorRef Parent { get; private set; }


        public PageScrapResult(string page, HttpStatusCode statusCode,IActorRef parent)
        {
            this.Page = page;
            this.StatusCode = statusCode;
            this.Parent = parent;
        }

        public PageScrapResult(string page, HttpStatusCode statusCode, Stream result, IActorRef parent) : this(page, statusCode,parent)
        {

            this.Result = result;
        }
    }
}