using System.Threading.Tasks;
using Akka.Actor;

namespace ActorModelBenchmarks.Akka.Net.Pi.Actors
{
    public class EchoActor : UntypedActor
    {
        private readonly TaskCompletionSource<bool> _taskCompletionSource;
        private int _calculationCount;

        public EchoActor(int calculationCount, TaskCompletionSource<bool> taskCompletionSource)
        {
            _calculationCount = calculationCount;
            _taskCompletionSource = taskCompletionSource;
        }

        protected override void OnReceive(object message)
        {
            if (message is PiCalculatorActor.PiNumber)
            {
                _calculationCount--;

                if (_calculationCount == 0)
                {
                    _taskCompletionSource.SetResult(true);
                }
            }
        }
    }
}