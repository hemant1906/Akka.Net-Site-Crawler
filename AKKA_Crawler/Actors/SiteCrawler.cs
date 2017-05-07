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
	        Dictionary<string, IActorRef> childActors = new Dictionary<string, IActorRef>();
	      
	        private void CreateChidActor(string childUrl)
	        {
				var pageScrapper = Context.ActorOf(Props.Create<PageScrapper>());

	            pageScrapper.Tell(new StartScrapping(childUrl));			
	            PagesToScrap[childUrl] += 1;
	        }


	        protected override void OnReceive(object message)
	        {
	            if(message is StartScrapping)
	            {
	                var scrapMessage = message as StartScrapping;
	                AddPage(scrapMessage.PageToScrap);
	                CreateChidActor(scrapMessage.PageToScrap);

                Console.ResetColor();
	                Console.WriteLine("{0} Started for {1} times",scrapMessage.PageToScrap,PagesToScrap[scrapMessage.PageToScrap]);
	            }
	            else if (message is CrawlSite)
	            {
					var scrapMessage = message as CrawlSite;

	                AddPage(scrapMessage.SiteUrl);
	                CreateChidActor(scrapMessage.SiteUrl);
	                PagesToScrap[scrapMessage.SiteUrl] += 1;

                Console.ResetColor();
	                Console.WriteLine("{0} Started for {1} times", scrapMessage.SiteUrl, PagesToScrap[scrapMessage.SiteUrl]);
	            }
	            else if (message is PageScrapped)
	                {

	                try
	                {
	                    
	                    var scrappedPage = message as PageScrapped;
	                    Console.WriteLine("{0} Scrap Complete", scrappedPage.PageName);
	                    int childPages = 0;
                    if (scrappedPage.PageContent != null)
                    {
                        foreach (HtmlNode link in scrappedPage.PageContent.DocumentNode.SelectNodes("//a[@href]"))
                        {
                            HtmlAttribute att = link.Attributes["href"];
                            if (att != null)
                            {
                                if (att.Value.StartsWith("http", StringComparison.CurrentCulture))
                                {
                                    childPages++;

                                    if (AddPage(att.Value))
                                    {
                                        CreateChidActor(att.Value);
                                    }
                                }
                            }
                        }
                    }


                    Console.ForegroundColor = ConsoleColor.Green;
	                    Console.WriteLine("Page {0} Scrapped. It has {1} links", scrappedPage.PageName, childPages);
                    Console.ResetColor();
	                }catch(Exception ex)
	                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unable to process child due to {0}", ex.Message);  
                    Console.ResetColor();
	                }

	                }
	                else if (message is ScrappingError)
	                {
	                var msg = message as ScrappingError;
	                if(  PagesToScrap[msg.Url] <= 5)  // try 5 times
	                {
                    Console.ForegroundColor = ConsoleColor.Red;   
	                    Console.WriteLine("Error scrapping {0}. Trying again",msg.Url);
                    Console.ResetColor();
	                }
	                }
	        }

	        private bool AddPage(string page)
	        {
	            bool added = false;
	            if(!PagesToScrap.ContainsKey(page))
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
