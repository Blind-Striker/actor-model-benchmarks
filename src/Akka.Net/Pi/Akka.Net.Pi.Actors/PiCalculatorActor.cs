//using ActorModelBenchmarks.Messages;
using ActorModelBenchmarks.Messages.Protobuf;
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
                var actorSelection = Context.ActorSelection(options.ReceiverAddress);

                actorSelection.Tell(new PiNumber {Pi = strPi});
            }
        }
    }
}