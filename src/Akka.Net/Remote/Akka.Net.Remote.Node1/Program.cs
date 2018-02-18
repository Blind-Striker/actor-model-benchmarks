using System;
using System.IO;
using System.Threading;
using ActorModelBenchmarks.Akka.Net.Remote.Messages;
using Akka.Actor;
using Akka.Configuration;

//using Start = Akka.Net.Messages.Protobuf.ProtoStart;
//using StartRemote = Akka.Net.Messages.Protobuf.ProtoRemote;
//using Ping = Akka.Net.Messages.Protobuf.ProtoPing;
//using Pong = Akka.Net.Messages.Protobuf.ProtoPong;

namespace ActorModelBenchmarks.Akka.Net.Remote.Node1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(File.ReadAllText("akka-config.hocon"));

            var actorSystem = ActorSystem.Create("remote-sys", config);

            var messageCount = 100_000;
            var wg = new AutoResetEvent(false);
            var actorRef = actorSystem.ActorOf(Props.Create(() => new LocalActor(0, messageCount, wg)));
            var remoteActor = actorSystem.ActorSelection("akka.tcp://remote-sys@127.0.0.1:8091/user/remote");

            var address = actorRef.Path.ToStringWithAddress(new Address("akka.tcp", "remote-sys", "127.0.0.1", 8090));

            remoteActor.Ask<Start>(new StartRemote {SenderAddress = address}).Wait();

            var start = DateTime.Now;
            Console.WriteLine("Starting to send");
            var msg = new Ping();
            for (var i = 0; i < messageCount; i++)
            {
                remoteActor.Tell(msg);
            }
            wg.WaitOne();
            var elapsed = DateTime.Now - start;
            Console.WriteLine("Elapsed {0}", elapsed);

            var t = messageCount * 2.0 / elapsed.TotalMilliseconds * 1000;
            Console.WriteLine("Throughput {0} msg / sec", t);

            Console.ReadLine();
        }
    }

    public class LocalActor : UntypedActor
    {
        private readonly int _messageCount;
        private readonly AutoResetEvent _wg;
        private int _count;

        public LocalActor(int count, int messageCount, AutoResetEvent wg)
        {
            _count = count;
            _messageCount = messageCount;
            _wg = wg;
        }

        protected override void OnReceive(object message)
        {
            if (!(message is Pong pong))
                return;

            _count++;
            if (_count % 5000 == 0)
                Console.WriteLine(_count);
            if (_count == _messageCount)
                _wg.Set();
        }
    }
}