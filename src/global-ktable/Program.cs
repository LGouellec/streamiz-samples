using com.dotnet.samples.avro;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SchemaRegistry.SerDes.Avro;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;

namespace global_ktable
{
    static class Program
    {
        static readonly string ORDER_TOPIC = "order";
        static readonly string CUSTOMER_TOPIC = "customer";
        static readonly string PRODUCT_TOPIC = "product";
        static readonly string CUSTOMER_STORE = "customer-store";
        static readonly string PRODUCT_STORE = "product-store";
        static readonly string ENRICHED_ORDER_TOPIC = "enriched-order";

        static string GetEnvironmentVariable(string var, string @default)
        {
            return Environment.GetEnvironmentVariable(var) ?? @default;
        }

        static async Task Main(string[] args)
        {
            CancellationTokenSource source = new();
            string boostrapserver = GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVER", "localhost:9092");
            string schemaRegistryUrl = GetEnvironmentVariable("KAFKA_SCHEMA_REGISTRY_URL", "http://localhost:8081");

            var config = new StreamConfig<Int64SerDes, StringSerDes>();
            config.ApplicationId = "global-tables-example";
            config.ClientId = "global-tables-example-client";
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
            StreamBuilder builder = new();

            var orderStream = builder.Stream<string, Order, StringSerDes, SchemaAvroSerDes<Order>>(ORDER_TOPIC);

            var customers = builder.GlobalTable<string, Customer, StringSerDes, SchemaAvroSerDes<Customer>>(
                CUSTOMER_TOPIC, InMemory.As<string, Customer>(CUSTOMER_STORE));

            var products = builder.GlobalTable<string, Product, StringSerDes, SchemaAvroSerDes<Product>>(
                PRODUCT_TOPIC, InMemory.As<string, Product>(PRODUCT_STORE));

            var customerOrderStream =
                orderStream.Join(customers,
                                    (orderId, order) => order.customerId.ToString(),
                                    (order, customer) => new CustomerOrder(customer, order));

            var enrichedOrderStream =
                customerOrderStream.Join(products,
                                            (orderId, customerOrder) => customerOrder.Order.productId.ToString(),
                                            (customerOrder, product) =>
                                                 new EnrichedOrder(product.name,
                                                                    product.id,
                                                                    customerOrder.Customer.name,
                                                                    customerOrder.Customer.id,
                                                                    customerOrder.Order.id));

            enrichedOrderStream
                .To<StringSerDes, JsonSerDes<EnrichedOrder>>(ENRICHED_ORDER_TOPIC);

            return builder.Build();
        }
    }
}