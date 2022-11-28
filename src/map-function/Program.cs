using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;

namespace map_function
{
    public static class Program
    {
        static string GetEnvironmentVariable(string var, string @default)
        {
            return Environment.GetEnvironmentVariable(var) ?? @default;
        }
        
        static async Task Main(string[] args)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            string boostrapserver = GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVER", "localhost:9092");

            var config = new StreamConfig<ByteArraySerDes, StringSerDes>();
            // Give the Streams application a unique name.  The name must be unique in the Kafka cluster
            // against which the application is run.
            config.ApplicationId = "map-function-example";
            // Where to find Kafka broker(s).
            config.BootstrapServers = boostrapserver;

            // In the subsequent lines we define the processing topology of the Streams application.
            var builder = new StreamBuilder();

            // Read the input Kafka topic into a KStream instance
            var textLines = builder.Stream<byte[], string>("text-lines-topic");
            // Variant 1: using `MapValues`
            var upperCase = textLines.MapValues(v => v.ToUpper());

            // Write (i.e. persist) the results to a new Kafka topic called "UppercasedTextLinesTopic".
            // In this case we can rely on the default serializers for keys and values because their data
            // types did not change, i.e. we only need to provide the name of the output topic.
            upperCase.To("uppercased-text-lines-topic");

            // Variant 2: using `map`, modify value only (equivalent to variant 1)
            var uppercasedWithMap = textLines.Map((key, value) => KeyValuePair.Create(key, value.ToUpper()));

            // Variant 3: using `map`, modify both key and value
            //
            // Note: Whether, in general, you should follow this artificial example and store the original
            //       value in the key field is debatable and depends on your use case.  If in doubt, don't
            //       do it.
            var originalAndUppercased = textLines.Map((key, value) => KeyValuePair.Create(value, value.ToUpper()));

            // Write the results to a new Kafka topic "OriginalAndUppercasedTopic".
            //
            // In this case we must explicitly set the correct serializers because the default serializers
            // (cf. streaming configuration) do not match the type of this particular KStream instance.
            originalAndUppercased.To<StringSerDes, StringSerDes>("original-and-uppercased-topic");

            Topology t = builder.Build();
        
            KafkaStream stream = new(t, config);

            Console.CancelKeyPress += (o, e) => {
                source.Cancel();
            };

            await stream.StartAsync(source.Token);
        }
    }
}