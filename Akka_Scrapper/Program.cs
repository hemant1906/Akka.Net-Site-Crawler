using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using AKKA_Scrapper.Actors;
using Akka.Routing;

namespace Akka_Scrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
akka {
    actor {
    provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
}
    remote {
    dot-netty.tcp {
    port = 8080
    hostname = localhost
}
}
}");

            using (ActorSystem system = ActorSystem.Create("ScrapperServer", config))
            {
                  var props = Props.Create<PageScrapper>().WithRouter(new RoundRobinPool(10));
                  var actor = system.ActorOf(props, "scrapper");

                //var actor = Props.Create<PageScrapper>("scrapper");

                Console.ReadLine();
            }
        }
    }
}
