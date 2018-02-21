using System.Threading.Tasks;
//using ActorModelBenchmarks.ProtoActor.Pi.Actors.Messages.Protobuf;
using ActorModelBenchmarks.ProtoActor.Pi.Actors.Messages;
using ActorModelBenchmarks.Utils;
using Proto;

namespace ActorModelBenchmarks.ProtoActor.Pi.Actors
{
    public partial class PiCalculatorActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is CalcOptions options)
            {
                var calculator = new PiCalculator();
                var pi = calculator.GetPi(options.Digits, options.Iterations);

                var strPi = pi.ToString();

                PID echoActor = new PID(options.ReceiverAddress, "echoActor");

                echoActor.Tell(new PiNumber {Pi = strPi});
            }

            return Actor.Done;
        }
    }
}