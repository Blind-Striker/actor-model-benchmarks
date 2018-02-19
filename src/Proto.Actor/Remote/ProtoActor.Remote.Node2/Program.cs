using System;
using System.Threading.Tasks;
using ActorModelBenchmarks.ProtoActor.Remote.Messages;
using ActorModelBenchmarks.Utils;
using ActorModelBenchmarks.Utils.Settings;
using Proto;
using Proto.Remote;
using ProtosReflection = ActorModelBenchmarks.ProtoActor.Remote.Messages.ProtosReflection;

namespace ActorModelBenchmarks.ProtoActor.Remote.Node2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var benchmarkSettings = Configuration.GetConfiguration<RemoteBenchmarkSettings>("RemoteBenchmarkSettings");

            Serialization.RegisterFileDescriptor(ProtosReflection.Descriptor);
            Proto.Remote.Remote.Start(benchmarkSettings.Node2Ip, benchmarkSettings.Node2Port);
            Actor.SpawnNamed(Actor.FromProducer(() => new EchoActor()), "remote");
            Console.ReadLine();
        }
    }

    public class EchoActor : IActor
    {
        private PID _sender;

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case StartRemote sr:
                    Console.WriteLine("Starting");
                    _sender = sr.Sender;
                    context.Respond(new Start());
                    return Actor.Done;
                case Ping _:
                    _sender.Tell(new Pong());
                    return Actor.Done;
                default:
                    return Actor.Done;
            }
        }
    }
}