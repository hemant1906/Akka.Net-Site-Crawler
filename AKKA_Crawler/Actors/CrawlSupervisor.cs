using System;
using Akka.Actor;
using AKKA_Crawler.Messages;
using System.Collections.Generic;
namespace AKKA_Crawler.Actors
{
    public class CrawlSupervisor : UntypedActor
    {

        Dictionary<string, IActorRef> crawlers = new Dictionary<string, IActorRef>();

        protected override void OnReceive(object message)
        {
         
            if(message is CrawlSite)
            {
				var msg = message as CrawlSite;
                if(crawlers.ContainsKey(msg.SiteUrl))
                {
                    //print that url already crawled/crawling
                }
                else
                {
                    
                    IActorRef crawlActor =  Context.ActorOf( Props.Create<SiteCrawler>(() => new SiteCrawler()));
                    crawlers.Add(msg.SiteUrl, crawlActor);
                    crawlActor.Tell(new CrawlSite(msg.SiteUrl));
                }
                
            }
            else if(message is IndexComplete)
            {
                
            }
            else if(message is CrawlComplete)
            {
                
            }
            else if (message is CrawlFailure)
            {
                
            }
        }
    }
}
