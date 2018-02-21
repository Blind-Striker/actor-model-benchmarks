using System;
using System.Threading.Tasks;
//using ActorModelBenchmarks.ProtoActor.Remote.Messages.Protobuf;
using ActorModelBenchmarks.ProtoActor.Remote.Messages;
using ActorModelBenchmarks.Utils;
using ActorModelBenchmarks.Utils.Settings;
using Proto;
using Proto.Remote;
using Proto.Serialization.Wire;

namespace ActorModelBenchmarks.ProtoActor.Remote.Node2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var benchmarkSettings = Configuration.GetConfiguration<RemoteBenchmarkSettings>("RemoteBenchmarkSettings");

            SwitchToWire();
            Proto.Remote.Remote.Start(benchmarkSettings.Node2Ip, benchmarkSettings.Node2Port);
            Actor.SpawnNamed(Actor.FromProducer(() => new EchoActor()), "remote");
            Console.ReadLine();
        }

        private static void SwitchToWire()
        {
            //Registering "knownTypes" is not required, but improves performance as those messages
            //do not need to pass any typename manifest
            var wire = new WireSerializer(new[]
            {
                typeof(Ping),
                typeof(Pong),
                typeof(StartRemote),
                typeof(Start)
            });
            Serialization.RegisterSerializer(wire, true);
        }

        private static void SwitchToProtobuf()
        {
            Serialization.RegisterFileDescriptor(Messages.Protobuf.ProtosReflection.Descriptor);
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
                    _sender = new PID(sr.SenderAddress, "local");
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