using System;
using HtmlAgilityPack;
using Akka.Actor;
using System.Net.Http;
using System.Net;
using AKKA_Crawler.Messages;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AKKA_Crawler.Actors
{
    public class PageScrapper : ReceiveActor
    {
        HttpClient _httpClient;
        public PageScrapper()
        {
            _httpClient = new HttpClient();


            Receive<StartScrapping>(page =>
           {
               // print message   

               Uri myUri = new Uri(page.PageToScrap);


               var pageUrl = page.PageToScrap;

               _httpClient.GetAsync(page.PageToScrap).ContinueWith(httpRequest =>
                  {
                      var response = httpRequest.Result;

                  //successful img download
                  if (response.StatusCode == HttpStatusCode.OK)
                      {
                          var contentStream = response.Content.ReadAsStreamAsync();
                          try
                          {
                              contentStream.Wait(TimeSpan.FromSeconds(1));
                              return new PageScrapResult(page, response.StatusCode, contentStream.Result);
                          }
                          catch //timeout exceptions!
                      {
                              return new PageScrapResult(page, HttpStatusCode.PartialContent);
                          }
                      }

                      return new PageScrapResult(page, response.StatusCode);
                  },
                    TaskContinuationOptions.ExecuteSynchronously)
                   .PipeTo(Self);
           }


                                   );


            Receive<PageScrapResult>(scrapResult =>
            {
                if (scrapResult.StatusCode == HttpStatusCode.OK)
                {
                    HtmlDocument document = new HtmlDocument();
                    document.Load(scrapResult.Result);

                    List<string> childLinks = new List<string>();
                    try
                    {
                        Uri uri = new Uri(scrapResult.Page.PageToScrap);
                        var host = uri.Host;

                        var links = document.DocumentNode.SelectNodes("//a[@href]");


                        if (links != null)
                        {
                            foreach (HtmlNode link in links)
                            {
                                HtmlAttribute att = link.Attributes["href"];
                                if (att != null)
                                {
                                    if (att.Value.StartsWith("http", StringComparison.CurrentCulture)  && Uri.IsWellFormedUriString(att.Value, UriKind.Absolute))
                                    {
                                        
                                        Uri myUri = new Uri(att.Value);
                                        string linkhost = myUri.Host;
                                        if (linkhost == host)  // restrict the crawl within the host
                                        {
                                            childLinks.Add(att.Value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Unable to parse child urls"); 
                    }


                    Context.Parent.Tell(new PageScrapped(scrapResult.Page.PageToScrap, childLinks.ToArray()));
                }
                else
                {
                    Context.Parent.Tell(new ScrappingError(scrapResult.Page.PageToScrap));
                }
            }

                                     );

        }
    }
}
