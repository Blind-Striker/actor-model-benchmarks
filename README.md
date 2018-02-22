# Actor Model Benchmarks
The aim of this project is to measure the performances of actor model frameworks such as  [Akka.Net](https://github.com/akkadotnet/akka.net), [Proto.Actor](https://github.com/AsynkronIT/protoactor-dotnet), [Orleans](https://github.com/dotnet/orleans). 
At the moment there are only Akka.Net and Proto.Actor benchmarks.

## Skynet Benchmark
[Skynet](https://github.com/atemerev/skynet) is a benchmark measuring actor creation performance.

> Creates an actor (goroutine, whatever), which spawns 10 new actors, each of them spawns 10 more actors, etc. until one million actors are created on the final level. Then, each of them returns back its ordinal number (from 0 to 999999), which are summed on the previous level and sent back upstream, until reaching the root actor. (The answer should be 499999500000).

Scala akka codes in the skynet repository are adapted to Akka.Net and Proto.Actor.

## Ping Pong Benchmark
Creates receive and echo actors as many as the number of cores, maps them to each other and sends messages between them. Measures how long to create N number of messages and how long it takes to send messages between receive and echo actors.

There are two kinds of ping pong benchmark and both of them have both in process and remote benchmarks. The benchmark named InProc is adapted from Proto.Actor repository and PingPong is adepted from Akka.Net repository.

## Pi Calculation Benchmark
Like the ping pong benchmark, actors and n number of messages are created. Actors calculates pi number as many as messages and send it back. It is aimed to simulate real world scenarios by making a calculation in Actors. There are both in process and remote benchmark examples.

## How to build and run
You can use the `dotnet` CLI commands, or use Visual Studio 2017, or use Visual Studio Code. For VS Code user both `launch.json` and `tasks.json` files created and configurations added for all different benchmarks. 


# Benchmark Guide and Results
* Solution strucutered under two solution folders, Akka.Net and Proto.Actor. 
* All benchmarks were run on a computer with Intel i7-6700HQ @ 2.6 GHz and 16 RAM configurations.
* Benchmark settings are separated into sections in the `benchmark-settings.json` file that added as link to all benchmark projects. Changes made in this file apply to both the Akka.Net and Proto.Actor benchmarks for the same benchmark.
* There is also seperate .hocon files for Akka.Net benchmarks.

## Skynet Benchmark Guide
You can change `SkynetBenchmarkSettings.TimesToRun` value in `benchmark-settings.json` for benchmark repeats. 

### Akka.Net Results
19,269.76 ms avg for 3 repeats.

### Proto.Actor Results
1,808 ms avg for 3 repeats.

## Ping Pong Benchmark Guide (Adapted from Akka.Net repository)
You can change `PingPongSettings.TimesToRun` value for benchmark repeats and `PingPongSettings.Throughputs` for set throughput values in `benchmark-settings.json`. 

### Akka.Net Results
The benchmark actor was separately tested, derived from both ActorBase and ReceiveActor.

ActorBase    first start time: 15.12 ms
ReceiveActor first start time: 42.80 ms

|            | ActorBase  |            |              |ReceiveActor|            |            |
|------------|------------|------------|--------------|------------|------------|------------|
| Throughput | Msgs/sec   | Start [ms] | Total [ms]   | Msgs/sec   | Start [ms] | Total [ms] |
| 20         | 19,710,000 | 10.58      | 1532.90      | 19,749,000 | 7.78       | 1527.66    |
| 30         | 22,058,000 | 7.13       | 1367.89      | 22,075,000 | 5.36       | 1364.61    |
| 40         | 23,183,000 | 5.76       | 1300.53      | 21,786,000 | 5.53       | 1383.11    |
| 50         | 22,865,000 | 5.20       | 1317.86      | 20,632,000 | 5.24       | 1459.46    |
| 60         | 19,986,000 | 5.08       | 1506.14      | 19,736,000 | 6.41       | 1526.61    |
| 70         | 19,379,000 | 7.35       | 1556.23      | 19,788,000 | 7.72       | 1524.71    |
| 80         | 20,120,000 | 7.38       | 1499.06      | 21,246,000 | 6.21       | 1419.08    |
| 90         | 21,291,000 | 7.23       | 1417.23      | 22,658,000 | 4.99       | 1330.00    |
| 100        | 23,148,000 | 6.34       | 1303.22      | 22,271,000 | 4.85       | 1352.40    |
| 200        | 24,115,000 | 7.03       | 1251.97      | 24,038,000 | 4.76       | 1252.97    |
| 300        | 23,809,000 | 4.29       | 1264.37      | 23,059,000 | 5.25       | 1306.93    |
| 400        | 24,896,000 | 6.40       | 1211.94      | 23,980,000 | 4.86       | 1256.52    |
| 500        | 24,370,000 | 4.87       | 1236.63      | 24,174,000 | 4.53       | 1245.97    |
| 600        | 24,529,000 | 4.98       | 1228.14      | 23,566,000 | 7.30       | 1280.65    |
| 700        | 23,734,000 | 4.35       | 1269.08      | 23,310,000 | 7.11       | 1294.48    |
| 800        | 24,650,000 | 6.41       | 1223.73      | 23,273,000 | 5.08       | 1294.85    |
| 900        | 24,291,000 | 4.36       | 1239.76      | 24,671,000 | 4.40       | 1221.06    |

### Proto.Actor Results
The benchmark actor is derived from IActor.

Actor    first start time: 24.10 ms

| Throughput | Msgs/sec   | Start [ms] | Total [ms] |
|------------|------------|------------|------------|
| 20         | 33,783,000 | 0.26       | 888.66     |
| 30         | 37,546,000 | 0.12       | 799.86     |
| 40         | 37,974,000 | 0.09       | 790.13     |
| 50         | 35,756,000 | 0.11       | 839.64     |
| 60         | 40,650,000 | 0.09       | 738.41     |
| 70         | 42,075,000 | 0.15       | 713.34     |
| 80         | 42,613,000 | 0.10       | 704.40     |
| 90         | 37,641,000 | 0.54       | 797.63     |
| 100        | 42,253,000 | 0.10       | 710.52     |
| 200        | 44,313,000 | 0.10       | 678.06     |
| 300        | 45,592,000 | 0.09       | 658.12     |
| 400        | 45,045,000 | 0.10       | 666.70     |
| 500        | 44,709,000 | 0.08       | 671.28     |
| 600        | 44,444,000 | 0.09       | 675.65     |
| 700        | 44,776,000 | 0.09       | 670.84     |
| 800        | 44,910,000 | 0.10       | 668.99     |
| 900        | 45,248,000 | 0.10       | 663.41     |
