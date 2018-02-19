namespace ActorModelBenchmarks.Utils.Settings
{
    public class InprocBenchmarkSettings
    {
        public int MessageCount { get; set; }

        public int BatchSize { get; set; }

        public string AkkaDispatcherType { get; set; }

        public int[] Throughputs { get; set; }
    }
}
