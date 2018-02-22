using System;
using System.IO;
using ActorModelBenchmarks.Utils;
using ActorModelBenchmarks.Utils.Settings;
using Akka.Actor;
using Akka.Configuration;

namespace ActorModelBenchmarks.Akka.Net.Skynet
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var benchmarkSettings = Configuration.GetConfiguration<SkynetBenchmarkSettings>("SkynetBenchmarkSettings");
            var config = ConfigurationFactory.ParseString(File.ReadAllText("akka-config.hocon"));
            var mainSystem = ActorSystem.Create("main", config);

            var rootActor = mainSystem.ActorOf(Props.Create<RootActor>());
            var run = new RootActor.Run(benchmarkSettings.TimesToRun);

            rootActor.Tell(run);

            Console.ReadLine();
        }
    }

    public class SkynetActor : UntypedActor
    {
        public static readonly Props Props = Props.Create(() => new SkynetActor());
        private long _count;

        private int _todo = 10;

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Start start:
                    if (start.Level == 1)
                    {
                        Context.Parent.Tell(start.Num);
                        Context.Stop(Self);
                    }
                    else
                    {
                        var startNum = start.Num * 10;

                        for (var i = 0; i < 10; i++)
                        {
                            var childSkynetActor = Context.ActorOf(Props);
                            var childStart = new Start(start.Level - 1, startNum + i);

                            childSkynetActor.Tell(childStart);
                        }
                    }

                    break;
                case long l:
                    _todo -= 1;
                    _count += l;

                    if (_todo == 0)
                    {
                        Context.Parent.Tell(_count);
                        Context.Stop(Self);
                    }

                    break;
            }
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

    public class RootActor : UntypedActor
    {
        private int _num;
        private DateTime _startDateTime;

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Run run:
                    _startDateTime = DateTime.Now;
                    _num = run.Num - 1;

                    var skynetActor = Context.ActorOf(SkynetActor.Props);
                    var childStart = new SkynetActor.Start(7, 0);

                    skynetActor.Tell(childStart);

                    break;
                case long l:
                    var now = DateTime.Now;
                    var timeSpan = now - _startDateTime;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Result: {l} in {timeSpan.TotalMilliseconds} ms.");
                    Console.ForegroundColor = ConsoleColor.White;

                    if (_num == 0)
                    {
                        CoordinatedShutdown.Get(Context.System).Run().Wait(5000);
                        Console.WriteLine("Actor System Terminated");
                    }
                    else
                    {
                        Self.Tell(new Run(_num));
                    }

                    break;
            }
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