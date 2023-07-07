using System;
using System.Threading;
using System.Threading.Tasks;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.Processors.Public;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.State;
using Streamiz.Kafka.Net.Stream;

namespace de_duplication
{
    public class MappedEventValue
    {
        public string FinalKey { get; set; }
        public string EventId { get; set; }
        public long Value { get; set; }
    }
    
    internal static class Program
    {
        static readonly String INPUT_TOPIC = "input-topic";
        static readonly String OUTPUT_TOPIC = "output-topic";

        static string GetEnvironmentVariable(string var, string @default)
        {
            return Environment.GetEnvironmentVariable(var) ?? @default;
        }

        static async Task Main(string[] args)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            string boostrapserver = GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVER", "localhost:9092");

            var config = new StreamConfig<StringSerDes, StringSerDes>();
            // Give the Streams application a unique name.  The name must be unique in the Kafka cluster
            // against which the application is run.
            config.ApplicationId = "de-duplication-example";
            config.ClientId = "de-duplication--client";
            // Where to find Kafka broker(s).
            config.BootstrapServers = boostrapserver;
            config.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
            config.CommitIntervalMs = 10 * 1000;

            Topology t = GetTopology();
            KafkaStream stream = new(t, config);

            Console.CancelKeyPress += (o, e) => {
                source.Cancel();
            };

            await stream.StartAsync(source.Token);
        }

        static Topology GetTopology()
        {
            StreamBuilder builder = new();
            string storeName = "de-duplication-store";
            TimeSpan windowSize = TimeSpan.FromHours(1);
            
            IStoreBuilder dedupStoreBuilder = Stores.WindowStoreBuilder(
                Stores.InMemoryWindowStore(
                    storeName,
                    windowSize,
                    windowSize
                ),
                new StringSerDes(),
                new JsonSerDes<MappedEventValue>()
            );
         
            builder.Stream(INPUT_TOPIC, new StringSerDes(), new JsonSerDes<MappedEventValue>())
                .SelectKey((k,v) => v.FinalKey)
                .Repartition()
                .Transform(new TransformerBuilder<string, MappedEventValue, string, MappedEventValue>()
                    .StateStore(dedupStoreBuilder)
                    .Transformer<DeduplicationTransformer>(storeName, windowSize.TotalMilliseconds)
                    .Build()
                )
                .To(OUTPUT_TOPIC, new StringSerDes(), new JsonSerDes<MappedEventValue>());

            return builder.Build();
        }
    }
}