using System;
using System.Threading.Tasks;
using ActorModelBenchmarks.ProtoActor.Remote.Messages;
using Proto;
using Proto.Remote;
using ProtosReflection = ActorModelBenchmarks.ProtoActor.Remote.Messages.ProtosReflection;

namespace ActorModelBenchmarks.ProtoActor.Remote.Node2
{
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

    internal class Program
    {
        private static void Main(string[] args)
        {
            Serialization.RegisterFileDescriptor(ProtosReflection.Descriptor);
            Proto.Remote.Remote.Start("127.0.0.1", 12000);
            Actor.SpawnNamed(Actor.FromProducer(() => new EchoActor()), "remote");
            Console.ReadLine();
        }
    }
}