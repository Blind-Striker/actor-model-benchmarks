using System.Threading.Tasks;
using ActorModelBenchmarks.Utils;
using Proto;

namespace ActorModelBenchmarks.ProtoActor.Pi.Actors
{
    public class PiCalculatorActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is CalcOptions options)
            {
                var calculator = new PiCalculator();
                var pi = calculator.GetPi(options.Digits, options.Iterations);

                var strPi = pi.ToString();

                options.Receiver?.Tell(new PiNumber(strPi));
            }

            return Actor.Done;
        }

        public class CalcOptions
        {
            public CalcOptions(int digits, int iterations, PID receiver = null)
            {
                Digits = digits;
                Iterations = iterations;
                Receiver = receiver;
            }

            public int Digits { get; }

            public int Iterations { get; }

            public PID Receiver { get; }
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