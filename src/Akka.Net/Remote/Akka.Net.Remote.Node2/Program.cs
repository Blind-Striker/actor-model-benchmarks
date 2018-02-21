using System;
using System.IO;
using ActorModelBenchmarks.Messages;
//using ActorModelBenchmarks.Messages.Protobuf;
using Akka.Actor;
using Akka.Configuration;

namespace ActorModelBenchmarks.Akka.Net.Remote.Node2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(File.ReadAllText("akka-config.hocon"));

            var actorSystem = ActorSystem.Create("remote-sys", config);

            actorSystem.ActorOf<EchoActor>("remote");

            Console.ReadLine();
        }
    }

    public class EchoActor : UntypedActor
    {
        private ActorSelection _sender;

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case StartRemote sr:
                    Console.WriteLine("Starting");
                    _sender = Context.ActorSelection(sr.SenderAddress);
                    Sender.Tell(new Start(), Self);
                    break;
                case Ping ping:
                    _sender.Tell(new Pong());
                    break;
            }
        }
    }
}