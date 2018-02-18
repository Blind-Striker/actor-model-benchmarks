namespace ActorModelBenchmarks.Akka.Net.Remote.Messages
{
    public class StartRemote
    {
        public StartRemote()
        {
        }

        public StartRemote(string senderAddress)
        {
            SenderAddress = senderAddress;
        }

        public string SenderAddress { get; set; }
    }
}