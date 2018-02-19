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

