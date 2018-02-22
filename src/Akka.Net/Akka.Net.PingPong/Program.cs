using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ActorModelBenchmarks.Utils;
using ActorModelBenchmarks.Utils.Settings;
using Akka.Actor;
using Akka.Configuration;
using Akka.Util.Internal;

namespace ActorModelBenchmarks.Akka.Net.PingPong
{
    public static class Messages
    {
        public class Msg
        {
            public override string ToString()
            {
                return "msg";
            }
        }

        public class Run
        {
            public override string ToString()
            {
                return "run";
            }
        }

        public class Started
        {
            public override string ToString()
            {
                return "started";
            }
        }
    }

    internal class Program
    {
        [Flags]
        public enum PrintStats
        {
            No = 0,
            LineStart = 1,
            Stats = 2,
            StartTimeOnly = 32768
        }

        private static void Main(string[] args)
        {
            var benchmarkSettings = Configuration.GetConfiguration<PingPongSettings>("PingPongSettings");

            Start(benchmarkSettings);

            Console.Read();
        }

        private static async void Start(PingPongSettings pingPongSettings)
        {
            const int repeatFactor = 500;
            const long repeat = 30000L * repeatFactor;

            var processorCount = Environment.ProcessorCount;
            if (processorCount == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to read processor count..");
                return;
            }

            ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);

            Console.WriteLine("Worker threads:          {0}", workerThreads);
            Console.WriteLine("Completion Port Threads: {0}", completionPortThreads);
            Console.WriteLine("OSVersion:               {0}", Environment.OSVersion);
            Console.WriteLine("ProcessorCount:          {0}", processorCount);
            Console.WriteLine("Actor Count:             {0}", processorCount * 2);
            Console.WriteLine("Messages sent/received:  {0}  ({0:0e0})", GetTotalMessagesReceived(repeat));
            Console.WriteLine();

            //Warm up
            ActorSystem.Create("WarmupSystem").Terminate();
            Console.Write("ActorBase    first start time: ");
            await Benchmark<ClientActorBase>(1, 1, 1, PrintStats.StartTimeOnly, -1, -1);
            Console.WriteLine(" ms");
            Console.Write("ReceiveActor first start time: ");
            await Benchmark<ClientReceiveActor>(1, 1, 1, PrintStats.StartTimeOnly, -1, -1);
            Console.WriteLine(" ms");

            Console.WriteLine();

            Console.Write("            ActorBase                          ReceiveActor");
            Console.WriteLine();
            Console.Write("Throughput, Msgs/sec, Start [ms], Total [ms],  Msgs/sec, Start [ms], Total [ms]");
            Console.WriteLine();

            int timesToRun = pingPongSettings.TimesToRun;
            int[] throughputs = pingPongSettings.Throughputs;

            for (var i = 0; i < timesToRun; i++)
            {
                var redCountActorBase = 0;
                var redCountReceiveActor = 0;
                var bestThroughputActorBase = 0L;
                var bestThroughputReceiveActor = 0L;

                foreach (var throughput in throughputs)
                {
                    var result1 = await Benchmark<ClientActorBase>(throughput, processorCount, repeat, PrintStats.LineStart | PrintStats.Stats, bestThroughputActorBase, redCountActorBase);
                    bestThroughputActorBase = result1.BestThroughput;
                    redCountActorBase = result1.RedCount;

                    Console.Write(",  ");

                    var result2 = await Benchmark<ClientReceiveActor>(throughput, processorCount, repeat, PrintStats.Stats, bestThroughputReceiveActor, redCountReceiveActor);
                    bestThroughputReceiveActor = result2.BestThroughput;
                    redCountReceiveActor = result2.RedCount;

                    Console.WriteLine();
                }

                Console.WriteLine("--------------------------");
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Done..");
        }

        private static async Task<BenchmarkResult> Benchmark<TActor>(int throughput, int numberOfClients,
            long numberOfRepeats, PrintStats printStats, long bestThroughput, int redCount) where TActor : ActorBase
        {
            var totalMessagesReceived = GetTotalMessagesReceived(numberOfRepeats);
            //times 2 since the client and the destination both send messages
            var repeatsPerClient = numberOfRepeats / numberOfClients;
            var totalWatch = Stopwatch.StartNew();

            var system = ActorSystem.Create("PingPong", ConfigurationFactory.ParseString("akka.loglevel = ERROR"));

            var countdown = new CountdownEvent(numberOfClients * 2);
            var waitForStartsActor =
                system.ActorOf(Props.Create(() => new WaitForStarts(countdown)), "wait-for-starts");
            var clients = new List<IActorRef>();
            var tasks = new List<Task>();
            var started = new Messages.Started();

            for (var i = 0; i < numberOfClients; i++)
            {
                var destination = (RepointableActorRef) system.ActorOf<Destination>("destination-" + i);
                SpinWait.SpinUntil(() => destination.IsStarted);
                destination.Underlying.AsInstanceOf<ActorCell>().Dispatcher.Throughput = throughput;

                var ts = new TaskCompletionSource<bool>();
                tasks.Add(ts.Task);
                var client =(RepointableActorRef) system.ActorOf(new Props(typeof(TActor), null, destination, repeatsPerClient, ts), "client-" + i);
                SpinWait.SpinUntil(() => client.IsStarted);
                client.Underlying.AsInstanceOf<ActorCell>().Dispatcher.Throughput = throughput;
                clients.Add(client);

                client.Tell(started, waitForStartsActor);
                destination.Tell(started, waitForStartsActor);
            }

            if (!countdown.Wait(TimeSpan.FromSeconds(10)))
            {
                Console.WriteLine("The system did not start in 10 seconds. Aborting.");
                return new BenchmarkResult {BestThroughput = bestThroughput, RedCount = redCount};
            }

            var setupTime = totalWatch.Elapsed;
            var sw = Stopwatch.StartNew();
            var run = new Messages.Run();
            clients.ForEach(c => c.Tell(run));

            await Task.WhenAll(tasks.ToArray());
            sw.Stop();

            system.Terminate();
            totalWatch.Stop();

            var elapsedMilliseconds = sw.ElapsedMilliseconds;
            var throughputResult = elapsedMilliseconds == 0 ? -1 : totalMessagesReceived / elapsedMilliseconds * 1000;
            var foregroundColor = Console.ForegroundColor;

            if (throughputResult >= bestThroughput)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                bestThroughput = throughputResult;
                redCount = 0;
            }
            else
            {
                redCount++;
                Console.ForegroundColor = ConsoleColor.Red;
            }

            if (printStats.HasFlag(PrintStats.StartTimeOnly))
            {
                Console.Write("{0,5}", setupTime.TotalMilliseconds.ToString("F2", CultureInfo.InvariantCulture));
            }
            else
            {
                if (printStats.HasFlag(PrintStats.LineStart))
                { Console.Write("{0,10}, ", throughput);}

                if (printStats.HasFlag(PrintStats.Stats))
                {
                    Console.Write("{0,8}, {1,10}, {2,10}", throughputResult, setupTime.TotalMilliseconds.ToString("F2", CultureInfo.InvariantCulture), totalWatch.Elapsed.TotalMilliseconds.ToString("F2", CultureInfo.InvariantCulture));
                }
            }

            Console.ForegroundColor = foregroundColor;

            return new BenchmarkResult {BestThroughput = bestThroughput, RedCount = redCount};
        }

        private static long GetTotalMessagesReceived(long numberOfRepeats)
        {
            return numberOfRepeats * 2;
        }

        public class Destination : UntypedActor
        {
            protected override void OnReceive(object message)
            {
                switch (message)
                {
                    case Messages.Msg _:
                        Sender.Tell(message);
                        break;
                    case Messages.Started _:
                        Sender.Tell(message);
                        break;
                }
            }
        }

        public class WaitForStarts : UntypedActor
        {
            private readonly CountdownEvent _countdown;

            public WaitForStarts(CountdownEvent countdown)
            {
                _countdown = countdown;
            }

            protected override void OnReceive(object message)
            {
                if (message is Messages.Started)
                {
                    _countdown.Signal();
                }
            }
        }

        public class BenchmarkResult
        {
            public long BestThroughput { get; set; }

            public int RedCount { get; set; }
        }
    }
}