using System;
using System.Threading;
using System.Threading.Tasks;
using ActorModelBenchmarks.ProtoActor.Remote.Messages;
using ActorModelBenchmarks.Utils;
using ActorModelBenchmarks.Utils.Settings;
using Proto;
using Proto.Remote;
using ProtosReflection = ActorModelBenchmarks.ProtoActor.Remote.Messages.ProtosReflection;

namespace ActorModelBenchmarks.ProtoActor.Remote.Node1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var benchmarkSettings = Configuration.GetConfiguration<RemoteBenchmarkSettings>("RemoteBenchmarkSettings");

            Serialization.RegisterFileDescriptor(ProtosReflection.Descriptor);
            Proto.Remote.Remote.Start(benchmarkSettings.Node1Ip, benchmarkSettings.Node1Port);

            var messageCount = benchmarkSettings.MessageCount;
            var wg = new AutoResetEvent(false);
            var props = Actor.FromProducer(() => new LocalActor(0, messageCount, wg));

            var pid = Actor.Spawn(props);
            var remote = new PID($"{benchmarkSettings.Node2Ip}:{benchmarkSettings.Node2Port}", "remote");
            remote.RequestAsync<Start>(new StartRemote {Sender = pid}).Wait();

            var start = DateTime.Now;
            Console.WriteLine("Starting to send");
            var msg = new Ping();
            for (var i = 0; i < messageCount; i++)
            {
                remote.Tell(msg);
            }
            wg.WaitOne();
            var elapsed = DateTime.Now - start;
            Console.WriteLine("Elapsed {0}", elapsed);

            var t = messageCount * 2.0 / elapsed.TotalMilliseconds * 1000;
            Console.WriteLine("Throughput {0} msg / sec", t);

            Console.ReadLine();
        }

        public class LocalActor : IActor
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


            public Task ReceiveAsync(IContext context)
            {
                switch (context.Message)
                {
                    case Pong _:
                        _count++;
                        if (_count % 5000 == 0)
                        {
                            Console.WriteLine(_count);
                        }

                        if (_count == _messageCount)
                        {
                            _wg.Set();
                        }
                        break;
                }
                return Actor.Done;
            }
        }
    }
}