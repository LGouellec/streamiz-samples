namespace Streamiz.Tests;

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Streamiz.Demo.Infrastructure;
using Streamiz.Demo.KStream;
using Streamiz.Demo.KStream.Models;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.Mock;
using Streamiz.Kafka.Net.SerDes;
using Xunit.Abstractions;

public class WordSplitterTests
{
    private readonly TestOutputLoggerProvider outputProvider;

    public WordSplitterTests(ITestOutputHelper output)
    {
        this.outputProvider = new TestOutputLoggerProvider(output);
    }

    private TopologyTestDriver CreateTestTopology(Action<StreamBuilder> configureTopology, [CallerMemberName] string? name = null)
    {
        StreamConfig config = new StreamConfig
        {
            ApplicationId = $"{GetType().Name}",
            SchemaRegistryUrl = "mock://test",
            AutoRegisterSchemas = true,
            Logger = LoggerFactory.Create(c => c.AddProvider(outputProvider)),
            StateDir = "KStreamTestState-" + name
        };

        if (Directory.Exists(config.StateDir))
        {
            Directory.Delete(config.StateDir, true);
        }

        var builder = new StreamBuilder();
        configureTopology(builder);

        return new TopologyTestDriver(builder.Build(), config);
    }

    [Fact]
    public void TestTopol()
    {
        using var topology = CreateTestTopology(WordSplitter.ConfigureTopology);

        var input = topology.CreateInputTopic<string, string, StringSerDes, StringSerDes>("input");
        var output = topology.CreateOuputTopic<string, DemoValue, StringSerDes, AvroSerDes<DemoValue>>("output");

        var message = "this is a test";
        var expectation = new DemoValue
        {
            Input = message,
            ToUpperWords = new[] { "THIS", "IS", "A", "TEST" }
        };

        input.PipeInput("key", message);

        output
            .ReadValueList()
            .Should().HaveCount(1)
            .And.SatisfyRespectively(
                v => v.Should().BeEquivalentTo(expectation));

        topology
            .GetKeyValueStore<string, string>("store_demo")
            .Get("key")
            .Should().Be(string.Join(" ", expectation.ToUpperWords));
    }
}
