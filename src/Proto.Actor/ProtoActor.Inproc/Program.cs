using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using ActorModelBenchmarks.Utils;
using ActorModelBenchmarks.Utils.Settings;
using Proto;
using Proto.Mailbox;

namespace ActorModelBenchmarks.ProtoActor.Inproc
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var benchmarkSettings = Configuration.GetConfiguration<InprocBenchmarkSettings>("InprocBenchmarkSettings");

            Console.WriteLine($"Is Server GC {GCSettings.IsServerGC}");
            int messageCount = benchmarkSettings.MessageCount;
            int batchSize = benchmarkSettings.BatchSize;

            Console.WriteLine("Dispatcher\t\tElapsed\t\tMsg/sec");
            var tps = benchmarkSettings.Throughputs;

            var msgSecs = new List<int>();
            foreach (var t in tps)
            {
                var d = new ThreadPoolDispatcher {Throughput = t};

                var clientCount = Environment.ProcessorCount * 1;
                var clients = new PID[clientCount];
                var echos = new PID[clientCount];
                var completions = new TaskCompletionSource<bool>[clientCount];

                var echoProps = Actor.FromProducer(() => new EchoActor())
                    .WithDispatcher(d)
                    .WithMailbox(() => BoundedMailbox.Create(2048));
                //.WithMailbox(() => UnboundedMailbox.Create());

                for (var i = 0; i < clientCount; i++)
                {
                    var tsc = new TaskCompletionSource<bool>();
                    completions[i] = tsc;
                    var clientProps = Actor.FromProducer(() => new PingActor(tsc, messageCount, batchSize))
                        .WithDispatcher(d)
                        .WithMailbox(() => BoundedMailbox.Create(2048));
                    //.WithMailbox(() => UnboundedMailbox.Create());

                    clients[i] = Actor.Spawn(clientProps);
                    echos[i] = Actor.Spawn(echoProps);
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

            Console.ReadLine();
        }

        public class Msg
        {
            public Msg(PID sender)
            {
                Sender = sender;
            }

            public PID Sender { get; }
        }

        public class Start
        {
            public Start(PID sender)
            {
                Sender = sender;
            }

            public PID Sender { get; }
        }

        public class EchoActor : IActor
        {
            public Task ReceiveAsync(IContext context)
            {
                switch (context.Message)
                {
                    case Msg msg:
                        msg.Sender.Tell(msg);
                        break;
                }
                return Actor.Done;
            }
        }


        public class PingActor : IActor
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

            public Task ReceiveAsync(IContext context)
            {
                switch (context.Message)
                {
                    case Start s:
                        SendBatch(context, s.Sender);
                        break;
                    case Msg m:
                        _batch--;

                        if (_batch > 0)
                        {
                            break;
                        }

                        if (!SendBatch(context, m.Sender))
                        {
                            _wgStop.SetResult(true);
                        }
                        break;
                }
                return Actor.Done;
            }

            private bool SendBatch(IContext context, PID sender)
            {
                if (_messageCount == 0)
                {
                    return false;
                }

                var m = new Msg(context.Self);

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