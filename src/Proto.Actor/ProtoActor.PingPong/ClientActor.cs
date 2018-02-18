using System;
using System.Threading.Tasks;
using Proto;

namespace ActorModelBenchmarks.ProtoActor.PingPong
{
    public class ClientActor : IActor
    {
        private readonly PID _actor;
        private readonly TaskCompletionSource<bool> _latch;
        private readonly long _repeat;
        private long _received;
        private long _sent;

        public ClientActor(PID actor, long repeat, TaskCompletionSource<bool> latch)
        {
            _actor = actor;
            _repeat = repeat;
            _latch = latch;
        }

        public Task ReceiveAsync(IContext context)
        {
            var message = context.Message;

            switch (message)
            {
                case Messages.Msg _:
                    _received++;
                    if (_sent < _repeat)
                    {
                        _actor.Tell(message);
                        _sent++;
                    }
                    else if (_received >= _repeat)
                    {
                        _latch.SetResult(true);
                    }
                    return Actor.Done;
                case Messages.Run _:
                    var msg = new Messages.Msg {Sender = context.Self};
                    for (var i = 0; i < Math.Min(1000, _repeat); i++)
                    {
                        _actor.Tell(msg);
                        _sent++;
                    }
                    return Actor.Done;
                case Messages.Started started:
                    started.Sender.Tell(message);
                    return Actor.Done;
            }

            return Actor.Done;
        }
    }
}