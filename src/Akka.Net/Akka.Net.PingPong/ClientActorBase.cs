using System;
using System.Threading.Tasks;
using Akka.Actor;

namespace ActorModelBenchmarks.Akka.Net.PingPong
{
    public class ClientActorBase : ActorBase
    {
        private readonly IActorRef _actor;
        private readonly TaskCompletionSource<bool> _latch;
        private readonly long _repeat;
        private long _received;
        private long _sent;

        public ClientActorBase(IActorRef actor, long repeat, TaskCompletionSource<bool> latch)
        {
            _actor = actor;
            _repeat = repeat;
            _latch = latch;
        }

        protected override bool Receive(object message)
        {
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
                        //       Console.WriteLine("done {0}", Self.Path);
                        _latch.SetResult(true);
                    }
                    return true;
                case Messages.Run _:
                    var msg = new Messages.Msg();
                    for (var i = 0; i < Math.Min(1000, _repeat); i++)
                    {
                        _actor.Tell(msg);
                        _sent++;
                    }
                    return true;
                case Messages.Started _:
                    Sender.Tell(message);
                    return true;
            }

            return false;
        }
    }
}