using System;
using System.Numerics;
using System.Threading.Tasks;
using ActorModelBenchmarks.ProtoActor.Pi.Actors;
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

            //Registering "knownTypes" is not required, but improves performance as those messages
            //do not need to pass any typename manifest
            var wire = new WireSerializer(new[]
            {
                typeof(BigInteger),
                typeof(PiCalculatorActor.CalcOptions),
                typeof(PiCalculatorActor.PiNumber),
                typeof(StartRemote), typeof(Start)
            });
            Serialization.RegisterSerializer(wire, true);

            Remote.Start(benchmarkSettings.Node2Ip, benchmarkSettings.Node2Port);

            var piCalcProps = Router.NewRoundRobinPool(Actor.FromProducer(() => new PiCalculatorActor()), processorCount);
            var starterActorProps = Actor.FromProducer(() => new StarterActor());

            Actor.SpawnNamed(piCalcProps, "piActors");
            Actor.SpawnNamed(starterActorProps, "starterActor");

            Console.ReadLine();
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