using Akka.Actor;

namespace ActorModelBenchmarks.Akka.Net.Pi.Actors.Messages
{
    public class CalcOptions
    {
        public CalcOptions(int digits, int iterations, IActorRef receiver = null)
        {
            Digits = digits;
            Iterations = iterations;
            Receiver = receiver;
        }

        public int Digits { get; }

        public int Iterations { get; }

        public IActorRef Receiver { get; }
    }
}