using System.Threading.Tasks;
using Proto;
//using ActorModelBenchmarks.Messages;
using ActorModelBenchmarks.Messages.Protobuf;

namespace ActorModelBenchmarks.ProtoActor.Pi.Actors
{
    public class EchoActor : IActor
    {
        private readonly TaskCompletionSource<bool> _taskCompletionSource;
        private int _calculationCount;

        public EchoActor(int calculationCount, TaskCompletionSource<bool> taskCompletionSource)
        {
            _calculationCount = calculationCount;
            _taskCompletionSource = taskCompletionSource;
        }

        public Task ReceiveAsync(IContext context)
        {
            var contextSelf = context.Self;

            switch (context.Message)
            {
                case PiNumber _:
                    _calculationCount--;

                    if (_calculationCount == 0)
                    {
                        _taskCompletionSource.SetResult(true);
                    }
                    break;
            }

            return Actor.Done;
        }
    }
}