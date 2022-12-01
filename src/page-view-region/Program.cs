using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using com.dotnet.samples.avro;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SchemaRegistry.SerDes.Avro;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;

namespace page_view_region
{
    static class Program
    {
        static readonly string PAGE_VIEW_TOPIC = "page-views";
        static readonly string PAGE_VIEW_REGION_TOPIC = "page-views-by-region";
        static readonly string USER_PROFILE_TOPIC = "user-profiles";

        static string GetEnvironmentVariable(string var, string @default)
        {
            return Environment.GetEnvironmentVariable(var) ?? @default;
        }


        static async Task Main(string[] args)
        {
            CancellationTokenSource source = new();
            string boostrapserver = GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVER", "localhost:9092");
            string schemaRegistryUrl = GetEnvironmentVariable("KAFKA_SCHEMA_REGISTRY_URL", "http://localhost:8081");

            var config = new StreamConfig<StringSerDes, Int64SerDes>();
            config.ApplicationId = "pageview-region-lambda-example";
            config.ClientId = "pageview-region-lambda-example-client";
            // Where to find Kafka broker(s).
            config.BootstrapServers = boostrapserver;
            // Set to earliest so we don't miss any data that arrived in the topics before the process
            // started
            config.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
            config.SchemaRegistryUrl = schemaRegistryUrl;
            config.AutoRegisterSchemas = true;

            var t = GetTopology();

            KafkaStream stream = new(t, config);

            Console.CancelKeyPress += (o, e) => {
                source.Cancel();
            };

            await stream.StartAsync(source.Token);
        }

        static Topology GetTopology()
        {
            var builder = new StreamBuilder();

            var viewsByUser = builder
                .Stream<string, PageView, StringSerDes, SchemaAvroSerDes<PageView>>(PAGE_VIEW_TOPIC)
                .SelectKey((k, v) => v.user);

            var userRegions = builder.Stream<string, UserProfile, StringSerDes, SchemaAvroSerDes<UserProfile>>(USER_PROFILE_TOPIC)
                .MapValues((v) => v.region)
                .ToTable(
                    InMemory.As<string, string>($"{USER_PROFILE_TOPIC}-store")
                    .WithValueSerdes<StringSerDes>()
                    .WithKeySerdes<StringSerDes>());

            var viewsByRegion = viewsByUser.LeftJoin<string, ViewRegion, StringSerDes, JsonSerDes<ViewRegion>>(userRegions,
                (view, region) => new ViewRegion
                {
                    page = view.url,
                    user = view.user,
                    region = region
                })
                .Map((user, viewRegion) => KeyValuePair.Create(viewRegion.region, viewRegion))
                .GroupByKey()
                .WindowedBy(HoppingWindowOptions.Of(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1)))
                .Count();

            viewsByRegion
                .ToStream()
                .Map((w, c) => KeyValuePair.Create(w.ToString(), c.ToString()))
                .To<StringSerDes, StringSerDes>(PAGE_VIEW_REGION_TOPIC);

            return builder.Build();
        }
    }
}