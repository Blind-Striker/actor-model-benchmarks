namespace ActorModelBenchmarks.ProtoActor.Pi.Actors.Messages
{
    public class CalcOptions
    {
        public int Digits { get; set; }

        public int Iterations { get; set; }

        public string ReceiverAddress { get; set; }
    }
}