using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;

namespace sum
{
    internal static class Program
    {
        static readonly String SUM_OF_ODD_NUMBERS_TOPIC = "sum-of-odd-numbers-topic";
        static readonly String NUMBERS_TOPIC = "numbers-topic";

        static string GetEnvironmentVariable(string var, string @default)
        {
            return Environment.GetEnvironmentVariable(var) ?? @default;
        }

        static async Task Main(string[] args)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            string boostrapserver = GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVER", "localhost:9092");

            var config = new StreamConfig<Int32SerDes, Int32SerDes>();
            // Give the Streams application a unique name.  The name must be unique in the Kafka cluster
            // against which the application is run.
            config.ApplicationId = "sum-example";
            config.ClientId = "sum-example-client";
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
            // We assume the input topic contains records where the values are Integers.
            // We don't really care about the keys of the input records;  for simplicity, we assume them
            // to be Integers, too, because we will re-key the stream later on, and the new key will be
            // of type Integer.
            IKStream<byte[], string> input = builder.Stream<byte[], string, ByteArraySerDes, StringSerDes>(NUMBERS_TOPIC);

            IKTable<int, int> sumOfOddNumbers = input
             .MapValues((v) => Int32.Parse(v))
             // We want to compute the total sum across ALL numbers, so we must re-key all records to the
             // same key.  This re-keying is required because in Kafka Streams a data record is always a
             // key-value pair, and KStream aggregations such as `reduce` operate on a per-key basis.
             // The actual new key (here: `1`) we pick here doesn't matter as long it is the same across
             // all records.
             .SelectKey((k, v) => 1)
             // no need to specify explicit serdes because the resulting key and value types match our default serde settings
             .GroupByKey()
             // Add the numbers to compute the sum.
             .Reduce((v1, v2) => v1 + v2);

            sumOfOddNumbers
                .ToStream()
                .Map((k, v) => KeyValuePair.Create(k.ToString(), v.ToString()))
                .To<StringSerDes, StringSerDes>(SUM_OF_ODD_NUMBERS_TOPIC);

            return builder.Build();
        }
    }
}