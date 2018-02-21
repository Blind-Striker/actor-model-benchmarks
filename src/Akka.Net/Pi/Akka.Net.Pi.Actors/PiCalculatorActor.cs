using ActorModelBenchmarks.Akka.Net.Pi.Actors.Messages;
using ActorModelBenchmarks.Utils;
using Akka.Actor;

namespace ActorModelBenchmarks.Akka.Net.Pi.Actors
{
    public partial class PiCalculatorActor : UntypedActor
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
    }
}