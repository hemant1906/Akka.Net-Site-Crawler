using System;
using Akka.Actor;
using AKKA_Crawler.Actors;
using AKKA_Crawler.Messages;

namespace AKKA_Crawler
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var MyActorSystem = ActorSystem.Create("MyFirstActorSystem");

            Console.WriteLine("Starting Actor System");

            IActorRef siteSupervisor =
                MyActorSystem.ActorOf(Props.Create(() => new CrawlSupervisor()));

            siteSupervisor.Tell(new CrawlSite("http://www.landisgyr.com/"));

            MyActorSystem.WhenTerminated.Wait(TimeSpan.FromMinutes(5));

            Console.WriteLine("System Terminated or Time Over");

            siteSupervisor.Tell(PoisonPill.Instance);


            Console.ReadLine();
            
        }
    }
}
