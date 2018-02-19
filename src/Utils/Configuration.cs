using System;
using Microsoft.Extensions.Configuration;


namespace ActorModelBenchmarks.Utils
{
    public static class Configuration
    {
        public static TSetting GetConfiguration<TSetting>(string section)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("benchmark-settings.json");

            var configuration = builder.Build();

            TSetting settings = configuration.GetSection(section).Get<TSetting>();

            return settings;
        }
    }
}
