using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using ActorModelBenchmarks.ProtoActor.Pi.Actors;
using ActorModelBenchmarks.Utils;
using Proto;
using Proto.Remote;
using Proto.Serialization.Wire;

namespace ActorModelBenchmarks.ProtoActor.Pi.Node1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var processorCount = Environment.ProcessorCount;
            var calculationCount = 200_000;
            var piDigit = 1000;
            var piIteration = 10;

            //Registering "knownTypes" is not required, but improves performance as those messages
            //do not need to pass any typename manifest
            var wire = new WireSerializer(new[]
            {
                typeof(BigInteger),
                typeof(PiCalculatorActor.CalcOptions),
                typeof(PiCalculatorActor.PiNumber),
                typeof(StartRemote), typeof(Start)
            });
            Serialization.RegisterSerializer(wire, true);

            Remote.Start("127.0.0.1", 12000);

            var taskCompletionSource = new TaskCompletionSource<bool>();
            var echoProps = Actor.FromProducer(() => new EchoActor(calculationCount, taskCompletionSource));

            var piActors = new PID("127.0.0.1:12001", "piActors");
            var starterActor = new PID("127.0.0.1:12001", "starterActor");
            var echoActor = Actor.Spawn(echoProps);

            WriteBenchmarkInfo(processorCount, calculationCount);
            Console.WriteLine();
            WritePiBenchMark(piDigit, piIteration);
            Console.WriteLine();

            starterActor.RequestAsync<Start>(new StartRemote()).Wait();

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