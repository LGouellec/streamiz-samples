using System;
using System.Threading;
using System.Threading.Tasks;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;

namespace word_count
{
    static class Program
    {
        static readonly String inputTopic = "plaintext-input";
        static readonly String outputTopic = "wordcount-output";

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
            config.ApplicationId = "wordcount-example";
            config.ClientId = "wordcount-example-client";
            // Where to find Kafka broker(s).
            config.BootstrapServers = boostrapserver;
            config.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
            config.CommitIntervalMs = 10 * 1000;

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

            // Construct a `KStream` from the input topic "streams-plaintext-input", where message values
            // represent lines of text (for the sake of this example, we ignore whatever may be stored
            // in the message keys).  The default key and value serdes will be used.
             IKStream<string, string> textLines = builder.Stream<string, string>(inputTopic);

             IKTable<String, long> wordCounts = textLines
              // Split each text line, by whitespace, into words.  The text lines are the record
              // values, i.e. we can ignore whatever data is in the record keys and thus invoke
              // `flatMapValues()` instead of the more generic `flatMap()`.
              .FlatMapValues((value, c) => value.ToLower().Split(" "))
              // Group the split data by word so that we can subsequently count the occurrences per word.
              // This step re-keys (re-partitions) the input data, with the new record key being the words.
              // Note: No need to specify explicit serdes because the resulting key and value types
              // (String and String) match the application's default serdes.
              .GroupBy((k, w, c) => w)
              // Count the occurrences of each word (record key).
              .Count();

            // Write the `KTable<String, Long>` to the output topic.
            wordCounts.ToStream()
                .MapValues((t, c) => t.ToString())
                .To(outputTopic);

            return builder.Build();
        }
    }
}