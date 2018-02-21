using System;
using System.Threading.Tasks;
using ActorModelBenchmarks.ProtoActor.Pi.Actors;
//using ActorModelBenchmarks.Messages;
using ActorModelBenchmarks.Messages.Protobuf;
using ActorModelBenchmarks.Utils;
using ActorModelBenchmarks.Utils.Settings;
using Proto;
using Proto.Remote;
using Proto.Router;
using Proto.Serialization.Wire;

namespace ActorModelBenchmarks.ProtoActor.Pi.Node2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var benchmarkSettings = Configuration.GetConfiguration<PiBenchmarkSettings>("PiBenchmarkSettings");

            var processorCount = Environment.ProcessorCount;

            SwitchToProtobuf();
            Remote.Start(benchmarkSettings.Node2Ip, benchmarkSettings.Node2Port);

            var piCalcProps = Router.NewRoundRobinPool(Actor.FromProducer(() => new PiCalculatorActor()), processorCount);
            var starterActorProps = Actor.FromProducer(() => new StarterActor());

            PID piActors = Actor.SpawnNamed(piCalcProps, "piActors");
            PID starterActor = Actor.SpawnNamed(starterActorProps, "starterActor");

            Console.ReadLine();
        }

        private static void SwitchToWire()
        {
            //Registering "knownTypes" is not required, but improves performance as those messages
            //do not need to pass any typename manifest
            var wire = new WireSerializer(new[]
            {
                typeof(CalcOptions),
                typeof(PiNumber),
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

    public class StarterActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is StartRemote)
            {
                Console.WriteLine("Starting");
                context.Respond(new Start());
                return Actor.Done;
            }

            return Actor.Done;
        }
    }
}