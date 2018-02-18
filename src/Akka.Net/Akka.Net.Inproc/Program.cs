using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Util.Internal;

namespace ActorModelBenchmarks.Akka.Net.Inproc
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine($"Is Server GC {GCSettings.IsServerGC}");

            var config = ConfigurationFactory.ParseString(File.ReadAllText("akka-config.hocon"));
            var mainSystem = ActorSystem.Create("main", config);

            const int messageCount = 1_000_000;
            const int batchSize = 100;
            //string dispatcherType = "akka.custom-fork-join-dispatcher";
            var dispatcherType = "akka.custom-dispatcher";


            Console.WriteLine("Dispatcher\t\tElapsed\t\tMsg/sec");
            var tps = new[] {100, 200, 300, 400, 500, 600, 700, 800, 900};
            //var tps = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 200, 300, 400, 500, 600, 700, 800, 900 };
            //var tps = new[] { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100 };

            var msgSecs = new List<int>();
            foreach (var t in tps)
            {
                var clientCount = Environment.ProcessorCount * 1;
                var clients = new IActorRef[clientCount];
                var echos = new IActorRef[clientCount];
                var completions = new TaskCompletionSource<bool>[clientCount];

                var echoProps = Props.Create(typeof(EchoActor))
                    .WithDispatcher(dispatcherType)
                    .WithMailbox("akka.bounded-mailbox");

                for (var i = 0; i < clientCount; i++)
                {
                    var tsc = new TaskCompletionSource<bool>();
                    completions[i] = tsc;

                    var clientProps = Props.Create(() => new PingActor(tsc, messageCount, batchSize))
                        .WithDispatcher(dispatcherType)
                        .WithMailbox("akka.bounded-mailbox");

                    var clientLocalActorRef = (RepointableActorRef) mainSystem.ActorOf(clientProps);
                    SpinWait.SpinUntil(() => clientLocalActorRef.IsStarted);
                    clientLocalActorRef.Underlying.AsInstanceOf<ActorCell>().Dispatcher.Throughput = t;

                    var echoLocalActorRef = (RepointableActorRef) mainSystem.ActorOf(echoProps);
                    SpinWait.SpinUntil(() => echoLocalActorRef.IsStarted);
                    echoLocalActorRef.Underlying.AsInstanceOf<ActorCell>().Dispatcher.Throughput = t;

                    clients[i] = clientLocalActorRef;
                    echos[i] = echoLocalActorRef;
                }

                var tasks = completions.Select(tsc => tsc.Task).ToArray();
                var sw = Stopwatch.StartNew();
                for (var i = 0; i < clientCount; i++)
                {
                    var client = clients[i];
                    var echo = echos[i];

                    client.Tell(new Start(echo));
                }

                Task.WaitAll(tasks);

                sw.Stop();
                var totalMessages = messageCount * 2 * clientCount;

                var x = (int) (totalMessages / (double) sw.ElapsedMilliseconds * 1000.0d);
                Console.WriteLine($"{t}\t\t\t{sw.ElapsedMilliseconds}\t\t{x}");
                msgSecs.Add(x);
                Thread.Sleep(2000);
            }

            Console.WriteLine($"Avg Msg/sec : {msgSecs.Average()}");
        }

        public class Msg
        {
            public Msg(IActorRef sender)
            {
                Sender = sender;
            }

            public IActorRef Sender { get; }
        }

        public class Start
        {
            public Start(IActorRef sender)
            {
                Sender = sender;
            }

            public IActorRef Sender { get; }
        }

        public class EchoActor : UntypedActor
        {
            protected override void OnReceive(object message)
            {
                if (message is Msg msg)
                {
                    msg.Sender.Tell(msg);
                }
            }
        }

        public class PingActor : UntypedActor
        {
            private readonly int _batchSize;
            private readonly TaskCompletionSource<bool> _wgStop;
            private int _batch;
            private int _messageCount;

            public PingActor(TaskCompletionSource<bool> wgStop, int messageCount, int batchSize)
            {
                _wgStop = wgStop;
                _messageCount = messageCount;
                _batchSize = batchSize;
            }

            protected override void OnReceive(object message)
            {
                switch (message)
                {
                    case Start s:
                        SendBatch(s.Sender);
                        break;
                    case Msg m:
                        _batch--;
                        if (_batch > 0)
                        {
                            break;
                        }

                        if (!SendBatch(m.Sender))
                        {
                            _wgStop.SetResult(true);
                        }
                        break;
                }
            }

            private bool SendBatch(IActorRef sender)
            {
                if (_messageCount == 0)
                {
                    return false;
                }

                var m = new Msg(Context.Self);

                for (var i = 0; i < _batchSize; i++)
                {
                    sender.Tell(m);
                }

                _messageCount -= _batchSize;
                _batch = _batchSize;
                return true;
            }
        }
    }
}