using ActorModelBenchmarks.Utils;
using Akka.Actor;

namespace ActorModelBenchmarks.Akka.Net.Pi.Actors
{
    public class PiCalculatorActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            if (message is CalcOptions options)
            {
                var calculator = new PiCalculator();
                var pi = calculator.GetPi(options.Digits, options.Iterations);

                var strPi = pi.ToString();

                options.Receiver?.Tell(new PiNumber(strPi));
            }
        }

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

        public class PiNumber
        {
            public PiNumber(string pi)
            {
                Pi = pi;
            }

            public string Pi { get; }
        }
    }
}