using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;

namespace shoes_shop
{
    static class Program
    {
        static readonly string ORDER_TOPIC = "orders";
        static readonly string CUSTOMER_TOPIC = "customers";
        static readonly string PRODUCT_TOPIC = "products";
        static readonly string CUSTOMER_STORE = "customer-store";
        static readonly string PRODUCT_STORE = "product-store";
        static readonly string ENRICHED_ORDER_TOPIC = "orders-enriched";

        static string GetEnvironmentVariable(string var, string @default)
        {
            return Environment.GetEnvironmentVariable(var) ?? @default;
        }

        static async Task Main(string[] args)
        {
            CancellationTokenSource source = new();
            string boostrapserver = GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVER", "localhost:9092");

            var config = new StreamConfig();
            config.ApplicationId = "shoes-shop-app";
            config.ClientId = "shoes-shop-app-client";
            // Where to find Kafka broker(s).
            config.BootstrapServers = boostrapserver;
            // Set to earliest so we don't miss any data that arrived in the topics before the process
            // started
            config.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;

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

            var orderStream = builder.Stream<string, Order, StringSerDes, JsonSerDes<Order>>(ORDER_TOPIC);

            var customers = builder.GlobalTable<string, Customer, StringSerDes, JsonSerDes<Customer>>(
                CUSTOMER_TOPIC, RocksDb.As<string, Customer>(CUSTOMER_STORE));

            var products = builder.GlobalTable<string, Product, StringSerDes, JsonSerDes<Product>>(
                PRODUCT_TOPIC, RocksDb.As<string, Product>(PRODUCT_STORE));

            var customerOrderStream =
                orderStream.Join(customers,
                                    (orderId, order) => order.customer_id,
                                    (order, customer) => new CustomerOrder(customer, order));

            var enrichedOrderStream =
                customerOrderStream.Join(products,
                                            (orderId, customerOrder) => customerOrder.Order.product_id,
                                            (customerOrder, product) => EnrichedOrderBuilder.Build(customerOrder, product));

            enrichedOrderStream
                .To<StringSerDes, JsonSerDes<EnrichedOrder>>(ENRICHED_ORDER_TOPIC);

            return builder.Build();
        }
    }
}