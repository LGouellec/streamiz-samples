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

        static async Task Main(string[] args)
        {
            CancellationTokenSource source = new();
            string bootstrapServers = args.Length > 0 ? args[0] : "localhost:9092";
            string schemaRegistryUrl = args.Length > 1 ? args[1] : "http://localhost:8081";

            var config = new StreamConfig<StringSerDes, Int64SerDes>();
            config.ApplicationId = "pageview-region-lambda-example";
            config.ClientId = "pageview-region-lambda-example-client";
            // Where to find Kafka broker(s).
            config.BootstrapServers = bootstrapServers;
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

            var userRegions = builder.Table<string, UserProfile, StringSerDes, SchemaAvroSerDes<UserProfile>>
                (USER_PROFILE_TOPIC, InMemory<string, UserProfile>.As($"{USER_PROFILE_TOPIC}-store"))
                .MapValues((v) => v.region);


            var viewsByRegion = viewsByUser.LeftJoin(userRegions,
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
                .ToStream((w, c) => w.ToString())
                .To<StringSerDes, Int64SerDes>(PAGE_VIEW_REGION_TOPIC);

            return builder.Build();
        }
    }
}