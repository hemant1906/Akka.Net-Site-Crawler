using System;
using HtmlAgilityPack;
using Akka.Actor;
using System.Net.Http;
using System.Net;
using AKKA_Crawler.Messages;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace AKKA_Scrapper.Actors
{
    public class PageScrapper : ReceiveActor
    {
        HttpClient _httpClient;
        public PageScrapper()
        {
            _httpClient = new HttpClient();

            Dictionary<string, string> restrictedExtensions = new Dictionary<string, string>();

            restrictedExtensions.Add(".zip", null);
            restrictedExtensions.Add(".pdf", null);
            restrictedExtensions.Add(".doc", null);
            restrictedExtensions.Add(".docx", null);
            restrictedExtensions.Add(".xls", null);
            restrictedExtensions.Add(".xlsx", null);


            Receive<StartScrapping>(page =>
           {
              var name = page.Parent;
               Console.WriteLine("Start scrapping {0}", page.PageToScrap);

               Uri myUri = new Uri(page.PageToScrap);


               var pageUrl = page.PageToScrap;

               _httpClient.GetAsync(page.PageToScrap).ContinueWith(httpRequest =>
                  {
                      var response = httpRequest.Result;

                      //successful  download
                      if (response.StatusCode == HttpStatusCode.OK)
                      {
                          var contentStream = response.Content.ReadAsStreamAsync();
                          try
                          {
                              contentStream.Wait(TimeSpan.FromSeconds(1));
                              return new PageScrapResult(page.PageToScrap, response.StatusCode, contentStream.Result,name);
                          }
                          catch //timeout exceptions!
                          {
                              return new PageScrapResult(page.PageToScrap, HttpStatusCode.PartialContent,name);
                          }
                      }

                      return new PageScrapResult(page.PageToScrap, response.StatusCode,name);
                  },
                    TaskContinuationOptions.ExecuteSynchronously)
                   .PipeTo(Self);
           }


                                   );


            Receive<PageScrapResult>(scrapResult =>
            {
                var parent = scrapResult.Parent;
                if (scrapResult.StatusCode == HttpStatusCode.OK)
                {
                    HtmlDocument document = new HtmlDocument();
                    document.Load(scrapResult.Result);

                    List<string> childLinks = new List<string>();
                    try
                    {
                        Uri uri = new Uri(scrapResult.Page);
                        var host = uri.Host;

                        var links = document.DocumentNode.SelectNodes("//a[@href]");


                        if (links != null)
                        {
                            foreach (HtmlNode link in links)
                            {
                                HtmlAttribute att = link.Attributes["href"];
                                if (att != null)
                                {
                                    if (att.Value.StartsWith("http", StringComparison.CurrentCulture) && Uri.IsWellFormedUriString(att.Value, UriKind.Absolute))
                                    {

                                        Uri myUri = new Uri(att.Value);
                                        string linkhost = myUri.Host;
                                        var extention = Path.GetExtension(att.Value);
                                        if (!restrictedExtensions.ContainsKey(extention) && linkhost == host)  // restrict the crawl within the host
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


                    parent.Tell(new PageScrapped(scrapResult.Page, childLinks.ToArray()));
                }
                else
                {

                    parent.Tell(new ScrappingError(scrapResult.Page));
                }
            }

                                     );

        }


    }
}
