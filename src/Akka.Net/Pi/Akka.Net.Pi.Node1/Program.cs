using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActorModelBenchmarks.Akka.Net.Pi.Actors;
using ActorModelBenchmarks.Utils;
using ActorModelBenchmarks.Utils.Settings;
using Akka.Actor;
using Akka.Configuration;

namespace ActorModelBenchmarks.Akka.Net.Pi.Node1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var benchmarkSettings = Configuration.GetConfiguration<PiBenchmarkSettings>("PiBenchmarkSettings");
            var config = ConfigurationFactory.ParseString(File.ReadAllText("akka-config.hocon"));

            var processorCount = Environment.ProcessorCount;
            var calculationCount = benchmarkSettings.CalculationCount;
            var piDigit = benchmarkSettings.PiDigit;
            var piIteration = benchmarkSettings.PiIteration;
            var node2Address = $"{benchmarkSettings.Node2Ip}:{benchmarkSettings.Node2Port}";

            var actorSystem = ActorSystem.Create("remote-sys", config);

            var taskCompletionSource = new TaskCompletionSource<bool>();
            var echoActor = actorSystem.ActorOf(Props.Create(() => new EchoActor(calculationCount, taskCompletionSource)));
            var piActors = actorSystem.ActorSelection($"akka.tcp://remote-sys@{node2Address}/user/piActors");
            var starterActor = actorSystem.ActorSelection($"akka.tcp://remote-sys@{node2Address}/user/starterActor");

            WriteBenchmarkInfo(processorCount, calculationCount);
            Console.WriteLine();
            WritePiBenchMark(piDigit, piIteration);
            Console.WriteLine();

            starterActor.Ask<Start>(new StartRemote()).Wait();

            var options = new PiCalculatorActor.CalcOptions(piDigit, piIteration, echoActor);

            Console.WriteLine("Routee\t\t\tElapsed\t\tMsg/sec");
            var tasks = taskCompletionSource.Task;
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < calculationCount; i++)
            {
                piActors.Tell(options);
            }
            Task.WaitAll(tasks);
            sw.Stop();

            var totalMessages = calculationCount * 2;
            var x = (int) (totalMessages / (double) sw.ElapsedMilliseconds * 1000.0d);
            Console.WriteLine($"{processorCount}\t\t\t{sw.ElapsedMilliseconds}\t\t{x}");

            Console.Read();
        }

        private static double WritePiBenchMark(int piDigit, int piIteration)
        {
            var iteration = 100;
            var miliSecs = new long[iteration];

            var sw1 = Stopwatch.StartNew();
            for (var i = 0; i < iteration; i++)
            {
                sw1.Start();

                var calculator = new PiCalculator();
                var pi = calculator.GetPi(piDigit, piIteration);

                sw1.Stop();
                miliSecs[i] = sw1.ElapsedMilliseconds;
                sw1.Reset();
            }
            var average = miliSecs.Average();
            Console.WriteLine("Pi digit\t\tPi Iteration\t\tAvgCalc Milliseconds");
            Console.WriteLine($"{piDigit}\t\t\t{piIteration}\t\t\t{average}");

            return average;
        }

        private static void WriteBenchmarkInfo(int processorCount, int calculationCount)
        {
            ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);

            Console.WriteLine("Worker threads:          {0}", workerThreads);
            Console.WriteLine("Completion Port Threads: {0}", completionPortThreads);
            Console.WriteLine("OSVersion:               {0}", Environment.OSVersion);
            Console.WriteLine("ProcessorCount:          {0}", processorCount);
            Console.WriteLine("Pi Calculation Count:    {0}  ({0:0e0})", calculationCount);
            Console.WriteLine();
        }
    }
}