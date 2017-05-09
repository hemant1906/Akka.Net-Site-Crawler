using System;
using Akka.Actor;
using AKKA_Crawler.Messages;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace AKKA_Crawler.Actors
{
    public class SiteCrawler : UntypedActor
    {
        Dictionary<string, int> PagesToScrap = new Dictionary<string, int>();
        int ScrappedPages = 0;
        int FailedPages = 0;
        string _site;
        Dictionary<string, IActorRef> childActors = new Dictionary<string, IActorRef>();

        private void CreateChidActor(string childUrl)
        {
            var pageScrapper = Context.ActorOf(Props.Create<PageScrapper>());
            childActors.Add(childUrl, pageScrapper);
            pageScrapper.Tell(new StartScrapping(childUrl));
            PagesToScrap[childUrl] += 1;
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
                            return Directive.Resume; // We could have checked inner exception but for simplicity we are keeping it
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
                CreateChidActor(scrapMessage.PageToScrap);

                Console.ResetColor();
                Console.WriteLine("{0} Started for {1} times", scrapMessage.PageToScrap, PagesToScrap[scrapMessage.PageToScrap]);
            }
            else if (message is CrawlSite)
            {

                var scrapMessage = message as CrawlSite;
                _site = scrapMessage.SiteUrl;
                AddPage(scrapMessage.SiteUrl);
                CreateChidActor(scrapMessage.SiteUrl);
                PagesToScrap[scrapMessage.SiteUrl] += 1;

                Console.ResetColor();
                Console.WriteLine("{0} Started for {1} times", scrapMessage.SiteUrl, PagesToScrap[scrapMessage.SiteUrl]);
            }
            else if (message is PageScrapped)
            {
                ScrappedPages++;
                var scrappedPage = message as PageScrapped;
                Console.WriteLine("{0} Scrap Complete", scrappedPage.PageName);

                Context.Stop(childActors[scrappedPage.PageName]);


                int childPages = 0;

                foreach (var url in scrappedPage.ChildPages)
                {
                    if (AddPage(url))
                    {
                        childPages++;
                        CreateChidActor(url);
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
                    childActors[msg.Url].Tell(new StartScrapping(msg.Url));
                    PagesToScrap[msg.Url] += 1;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error scrapping {0}. Stopping", msg.Url);
                    Console.ResetColor();
                    Context.Stop(childActors[msg.Url]);

                    FailedPages++;
                }
            }

            Console.WriteLine(PagesToScrap.Count - (ScrappedPages + FailedPages));// debugging only need to remove it


            if ((ScrappedPages + FailedPages) == PagesToScrap.Count)
                Context.Parent.Tell(new CrawlComplete(_site));
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
