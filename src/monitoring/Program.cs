using System;
using System.Threading;
using System.Threading.Tasks;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.Metrics.Prometheus;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;
using System.Reflection;

namespace monitoring
{
    public static class Program
    {
        static readonly String inputTopic = "input";
        static readonly String outputTopic = "output";
        static readonly long windowSize = (long)TimeSpan.FromHours(1).TotalMilliseconds;

        static string GetEnvironmentVariable(string var, string @default)
        {
            return Environment.GetEnvironmentVariable(var) ?? @default;
        }

        class StringWindowSerdes : TimeWindowedSerDes<string>
        {
            public StringWindowSerdes() :
                base(new StringSerDes(), windowSize)
            {
                
            }
        }

        public static async Task Main(string[] args)
        {
            CancellationTokenSource source = new();
            string boostrapserver = GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVER", "localhost:9092");
            
            var config = new StreamConfig<StringSerDes, StringSerDes>();
            // Give the Streams application a unique name.  The name must be unique in the Kafka cluster
            // against which the application is run.
            config.ApplicationId = "monitoring-example";
            // Where to find Kafka broker(s).
            config.BootstrapServers = boostrapserver;
            config.CommitIntervalMs = 10 * 1000;
            config.UsePrometheusReporter(9099, true);
            
            config.MetricsRecording = Streamiz.Kafka.Net.Metrics.MetricsRecordingLevel.DEBUG;

            Topology t = GetTopology();
            KafkaStream stream = new(t, config);

            Console.CancelKeyPress += (_, _) => {
                source.Cancel();
            };

            await stream.StartAsync(source.Token);
        }

        static Topology GetTopology()
        {
            StreamBuilder builder = new ();
            
            IKStream<string, string> stream = builder.Stream<string, string>(inputTopic);
            stream
                .GroupByKey()
                .WindowedBy(TumblingWindowOptions.Of(windowSize))
                .Count()
                .ToStream()
                .To<StringWindowSerdes, Int64SerDes>(outputTopic);

            stream
                .GroupByKey()
                .Count(
                    InMemory.As<string, long>("count-store")
                    .WithKeySerdes<StringSerDes>()
                    .WithValueSerdes<Int64SerDes>());
            
            return builder.Build();
        }
    }
}    
