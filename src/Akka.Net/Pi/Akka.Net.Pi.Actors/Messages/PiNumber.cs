namespace ActorModelBenchmarks.Akka.Net.Pi.Actors.Messages
{
    public class PiNumber
    {
        public PiNumber(string pi)
        {
            Pi = pi;
        }

        public string Pi { get; }
    }
}