		using System;
		using HtmlAgilityPack;
		using Akka.Actor;
		using System.Net.Http;
		using System.Net;
		using AKKA_Crawler.Messages;
		using System.Threading.Tasks;
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
	                if(scrapResult.StatusCode == HttpStatusCode.OK)
	                {
	                    HtmlDocument document = new HtmlDocument();
	                    document.Load(scrapResult.Result);

                        Context.Parent.Tell(new PageScrapped(scrapResult.Page.PageToScrap, document));
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
