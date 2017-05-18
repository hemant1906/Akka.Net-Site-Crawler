using System;
using Akka.Actor;
using AKKA_Crawler.Actors;
using AKKA_Crawler.Messages;
using Akka.Configuration;

namespace AKKA_Crawler
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
            akka {
                actor {
                         provider = ""Akka.Remote.RemoteActorRefProvider,Akka.Remote""
                    }

                 remote {
             dot-netty.tcp {
                        port = 8090
                        hostname = localhost
                    }
                }
               }");

            using(var system = ActorSystem.Create("CrawlerClient",config))
            {

                IActorRef siteSupervisor =
                  system.ActorOf(Props.Create(() => new CrawlSupervisor()));


                //  //  siteSupervisor.Tell(new CrawlSite("http://www.smallsites.com/"));
                 siteSupervisor.Tell(new CrawlSite("http://www.hcl.com/",siteSupervisor));
                  siteSupervisor.Tell(new CrawlSite("https://www.infosys.com/",siteSupervisor));
                Console.ReadLine();
            }

           

        }
    }
}
