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

            if (message is CrawlSite)
            {
                var msg = message as CrawlSite;
                if (crawlers.ContainsKey(msg.SiteUrl))
                {
                    //print that url already crawled/crawling
                }
                else
                {

                    //    IActorRef crawlActor = Context.ActorOf(Props.Create<SiteCrawler>(() => new SiteCrawler()));
                    IActorRef crawlActor = Context.System.ActorOf<SiteCrawler>();
                    crawlers.Add(msg.SiteUrl, crawlActor);
                    crawlActor.Tell(new CrawlSite(msg.SiteUrl,Self));
                }

            }
            else if (message is CrawlComplete)
            {
                var msg = message as CrawlComplete;
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Crawling Over for {0}",msg.SiteUrl);
                Console.ResetColor();
                Context.Stop(crawlers[((CrawlComplete)message).SiteUrl]);
                Context.Stop(crawlers[msg.SiteUrl]);
                
            }
            else if (message is CrawlFailure)
            {

            }
           
        }
    }
}
