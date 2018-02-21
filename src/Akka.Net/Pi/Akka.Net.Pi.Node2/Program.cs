using System;
using System.IO;
using ActorModelBenchmarks.Akka.Net.Pi.Actors;
//using ActorModelBenchmarks.Messages;
using ActorModelBenchmarks.Messages.Protobuf;
using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;

namespace ActorModelBenchmarks.Akka.Net.Pi.Node2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var processorCount = Environment.ProcessorCount;

            var config = ConfigurationFactory.ParseString(File.ReadAllText("akka-config.hocon"));

            var actorSystem = ActorSystem.Create("remote-sys", config);

            var actorRef = actorSystem.ActorOf(Props.Create<PiCalculatorActor>().WithRouter(new RoundRobinPool(processorCount)), "piActors");
            var starterActor = actorSystem.ActorOf<StarterActor>("starterActor");

            Console.ReadLine();
        }
    }

    public class StarterActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            if (message is StartRemote)
            {
                Console.WriteLine("Starting");
                Sender.Tell(new Start(), Self);
            }
        }
    }
}