using System;
using Akka.Actor;
using AKKA_Crawler.Messages;
using System.Collections.Generic;
using Akka.Routing;
using HtmlAgilityPack;

namespace AKKA_Crawler.Actors
{
    public class SiteCrawler : UntypedActor
    {
        Dictionary<string, int> PagesToScrap = new Dictionary<string, int>();
        int ScrappedPages = 0;
        int FailedPages = 0;
        string _site;

        IActorRef _parent;

        List<string> success = new List<string>();

        readonly ActorSelection router;


        public SiteCrawler()
        {
            router = Context.System.ActorSelection("akka.tcp://ScrapperServer@localhost:8080/user/scrapper");
        }


        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 3,
                withinTimeRange: TimeSpan.FromSeconds(30),
                decider: Decider.From(x =>
                    {
                        if (x is AggregateException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Exception occured processing a child. Stopping");
                            Console.ResetColor();
                            FailedPages++;
                            return Directive.Stop; // We could have checked inner exception but for simplicity we are keeping it
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;

                            Console.WriteLine("Exception occured processing a child. Restarting");
                            Console.ResetColor();
                            return Directive.Restart;
                          
                        }
                    }
                )

                );
        }


        protected override void OnReceive(object message)
        {
            

            if (message is StartScrapping)
            {
                var scrapMessage = message as StartScrapping;
                AddPage(scrapMessage.PageToScrap);
                router.Tell(new StartScrapping(scrapMessage.PageToScrap, Self));
                PagesToScrap[scrapMessage.PageToScrap] += 1;

                Console.ResetColor();
                Console.WriteLine("{0} Started for {1} times", scrapMessage.PageToScrap, PagesToScrap[scrapMessage.PageToScrap]);
            }
            else if (message is CrawlSite)
            {

                var scrapMessage = message as CrawlSite;

                _parent = scrapMessage.Parent;

                _site = scrapMessage.SiteUrl;
                AddPage(scrapMessage.SiteUrl);


                router.Tell(new StartScrapping(_site, Self));
                PagesToScrap[_site] += 1;

                Console.ResetColor();
                Console.WriteLine("{0} Started for {1} times", scrapMessage.SiteUrl, PagesToScrap[scrapMessage.SiteUrl]);
            }
            else if (message is PageScrapped)
            {
                ScrappedPages++;
                
                var scrappedPage = message as PageScrapped;
                Console.WriteLine("{0} Scrap Complete", scrappedPage.PageName);
                success.Add(scrappedPage.PageName);



                int childPages = 0;

                foreach (var url in scrappedPage.ChildPages)
                {
                    if (AddPage(url))
                    {
                        childPages++;
                        router.Tell(new StartScrapping(url, Self));
                        PagesToScrap[url] += 1;
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Page {0} Scrapped. It has {1} links", scrappedPage.PageName, childPages);
                Console.ResetColor();
            }
            else if (message is ScrappingError)
            {
                var msg = message as ScrappingError;
                if (PagesToScrap[msg.Url] <= 5)  // try 5 times
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error scrapping {0}. Trying again", msg.Url);
                    Console.ResetColor();
                    router.Tell(new StartScrapping(msg.Url,Self));
                    PagesToScrap[msg.Url] += 1;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error scrapping {0}. Stopping", msg.Url);
                    Console.ResetColor();
                    FailedPages++;
                }
            }



            if ((ScrappedPages + FailedPages) == PagesToScrap.Count)
               _parent.Tell(new CrawlComplete(_site));
        }

        private bool AddPage(string page)
        {
            bool added = false;
            if (!PagesToScrap.ContainsKey(page))
            {
                if (Uri.IsWellFormedUriString(page, UriKind.Absolute))
                {
                    PagesToScrap.Add(page, 0);

                    added = true;
                }
            }
            return added;

        }

    }
}
