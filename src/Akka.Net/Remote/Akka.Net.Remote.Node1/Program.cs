using System;
using System.IO;
using System.Threading;
using ActorModelBenchmarks.Messages;
//using ActorModelBenchmarks.Messages.Protobuf;
using ActorModelBenchmarks.Utils;
using ActorModelBenchmarks.Utils.Settings;
using Akka.Actor;
using Akka.Configuration;

namespace ActorModelBenchmarks.Akka.Net.Remote.Node1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var benchmarkSettings = Configuration.GetConfiguration<RemoteBenchmarkSettings>("RemoteBenchmarkSettings");
            var config = ConfigurationFactory.ParseString(File.ReadAllText("akka-config.hocon"));

            string node1Ip = benchmarkSettings.Node1Ip;
            string node2Ip = benchmarkSettings.Node2Ip;
            int node1Port = benchmarkSettings.Node1Port;
            int node2Port = benchmarkSettings.Node2Port;

            var actorSystem = ActorSystem.Create("remote-sys", config);

            var messageCount = benchmarkSettings.MessageCount;
            var wg = new AutoResetEvent(false);
            var actorRef = actorSystem.ActorOf(Props.Create(() => new LocalActor(0, messageCount, wg)), "local");
            var remoteActor = actorSystem.ActorSelection($"akka.tcp://remote-sys@{node2Ip}:{node2Port}/user/remote");

            var address = actorRef.Path.ToStringWithAddress(new Address("akka.tcp", "remote-sys", node1Ip, node1Port));

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
            if (!(message is Pong))
            {
                return;
            }

            _count++;

            if (_count % 5000 == 0)
            {
                Console.WriteLine(_count);
            }

            if (_count == _messageCount)
            {
                _wg.Set();
            }
        }
    }
}