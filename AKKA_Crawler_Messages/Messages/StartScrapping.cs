using Akka.Actor;
using System;

namespace AKKA_Crawler.Messages
{
    public class StartScrapping
    {
        public StartScrapping(string pageToScrap,IActorRef parent)
        {
            PageToScrap = pageToScrap;
            Parent = parent;
        }

        public string PageToScrap { get; private set; }
        public IActorRef Parent { get; private set; }
    }
}
