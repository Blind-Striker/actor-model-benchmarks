using System;
using System.Threading.Tasks;
using ActorModelBenchmarks.Utils;
using ActorModelBenchmarks.Utils.Settings;
using Proto;

namespace ActorModelBenchmarks.ProtoActor.Skynet
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var benchmarkSettings = Configuration.GetConfiguration<SkynetBenchmarkSettings>("SkynetBenchmarkSettings");

            var props = Actor.FromProducer(() => new RootActor());
            var pid = Actor.Spawn(props);

            var run = new RootActor.Run(benchmarkSettings.TimesToRun);

            pid.Tell(run);

            Console.ReadLine();
        }
    }

    public class SkynetActor : IActor
    {
        public static readonly Props Props = Actor.FromProducer(ProduceActor);
        private long _count;

        private int _todo = 10;

        public Task ReceiveAsync(IContext context)
        {
            var msg = context.Message;

            switch (msg)
            {
                case Start startMessage:
                    if (startMessage.Level == 1)
                    {
                        context.Parent.Tell(startMessage.Num);
                        context.Self.Stop();

                        return Actor.Done;
                    }
                    else
                    {
                        var startNum = startMessage.Num * 10;

                        for (var i = 0; i < 10; i++)
                        {
                            var pid = context.Spawn(Props);
                            var childStart = new Start(startMessage.Level - 1, startNum + i);

                            pid.Tell(childStart);
                        }

                        return Actor.Done;
                    }

                case long l:
                    _todo -= 1;
                    _count += l;

                    if (_todo == 0)
                    {
                        context.Parent.Tell(_count);
                        context.Self.Stop();
                    }

                    return Actor.Done;
            }

            return Actor.Done;
        }

        private static SkynetActor ProduceActor()
        {
            return new SkynetActor();
        }

        public class Start
        {
            public Start(int level, long num)
            {
                Level = level;
                Num = num;
            }

            public int Level { get; }

            public long Num { get; }
        }
    }

    public class RootActor : IActor
    {
        private int _num;
        private DateTime _startDateTime;

        public Task ReceiveAsync(IContext context)
        {
            var msg = context.Message;

            switch (msg)
            {
                case Run runMessage:
                    _startDateTime = DateTime.Now;
                    _num = runMessage.Num - 1;

                    var pid = context.Spawn(SkynetActor.Props);
                    var childStart = new SkynetActor.Start(7, 0);

                    pid.Tell(childStart);

                    break;
                case long l:
                    var now = DateTime.Now;
                    var timeSpan = now - _startDateTime;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Result: {l} in {timeSpan.TotalMilliseconds} ms.");
                    Console.ForegroundColor = ConsoleColor.White;

                    if (_num == 0)
                    {
                        Console.WriteLine("Actor System Terminated");
                    }
                    else
                    {
                        context.Self.Tell(new Run(_num));
                    }

                    break;
            }

            return Actor.Done;
        }

        public class Run
        {
            public Run(int num)
            {
                Num = num;
            }

            public int Num { get; }
        }
    }
}