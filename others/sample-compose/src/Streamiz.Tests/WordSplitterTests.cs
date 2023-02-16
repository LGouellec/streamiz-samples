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
    private readonly ITestOutputHelper output;

    public WordSplitterTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    private TopologyTestDriver CreateTestTopology(Action<StreamBuilder> configureTopology, [CallerMemberName] string? name = null)
    {
        var appId = $"{GetType().Name}.{name}";
        StreamConfig config = new StreamConfig
        {
            ApplicationId = GetType().Name,
            SchemaRegistryUrl = "mock://test",
            AutoRegisterSchemas = true,
            Logger = LoggerFactory.Create(c => c.AddProvider(new TestOutputLoggerProvider(output))),
            StateDir = "KStreamTestState-" + appId
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
        var topology = CreateTestTopology(WordSplitter.ConfigureTopology);

        var input = topology.CreateInputTopic<string, string, StringSerDes, StringSerDes>("input");
        var output = topology.CreateOuputTopic<string, DemoValue, StringSerDes, AvroSerDes<DemoValue>>("output");

        var message = "this is a test";
        var expectedWords = new[] { "THIS", "IS", "A", "TEST" };
        input.PipeInput(message);
        var values = output.ReadValueList().ToArray();
        values.Should().HaveCount(1);
        values[0].Input.Should().Be(message);
        values[0].ToUpperWords.Should().BeEquivalentTo(expectedWords);
    }
}
