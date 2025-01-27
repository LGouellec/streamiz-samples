using Chr.Avro.Confluent;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.Errors;
using Streamiz.Kafka.Net.SchemaRegistry.SerDes;
using Streamiz.Kafka.Net.SerDes;

namespace Streamiz.Demo.Infrastructure;

public class AvroSerDes<T> : SchemaSerDes<T, AvroSerializerConfig, AvroDeserializerConfig>
{
    public AvroSerDes() :
        base("avro")
    {
    }

    protected override SchemaRegistryConfig GetConfig(ISchemaRegistryConfig config, IStreamConfig streamConfig)
    {
        var c = new SchemaRegistryConfig
        {
            Url = config.SchemaRegistryUrl
        };

        if (config.SchemaRegistryMaxCachedSchemas.HasValue)
        {
            c.MaxCachedSchemas = config.SchemaRegistryMaxCachedSchemas;
        }

        if (config.SchemaRegistryRequestTimeoutMs.HasValue)
        {
            c.RequestTimeoutMs = config.SchemaRegistryRequestTimeoutMs;
        }

        if (!string.IsNullOrEmpty(config.BasicAuthUserInfo))
        {
            c.BasicAuthUserInfo = config.BasicAuthUserInfo;
        }

        if (!string.IsNullOrEmpty(config.BasicAuthCredentialsSource) && 
            Enum.TryParse(config.BasicAuthCredentialsSource, out AuthCredentialsSource credentialsSource))
        {
            c.BasicAuthCredentialsSource = credentialsSource;
        }

        var explicitStreamConfig = streamConfig.GetPrefixScan("schema.registry.");
        foreach (var kv in explicitStreamConfig)
        {
            try
            {
                c.Set(kv.Key, kv.Value);
            }
            catch
            {
                // ignored
            }
        }

        return c;
    }

    /// <summary>
    /// Initialize method with a current context which contains <see cref="IStreamConfig"/>.
    /// Can be used to initialize the serdes according to some parameters present in the configuration such as the schema.registry.url
    /// </summary>
    /// <param name="context">SerDesContext with stream configuration</param>
    public override void Initialize(SerDesContext context)
    {
        if (isInitialized)
        {
            return;
        }

        if (context.Config is ISchemaRegistryConfig schemaConfig)
        {
            registryClient = GetSchemaRegistryClient(GetConfig(schemaConfig, context.Config));
            deserializer = new AsyncSchemaRegistryDeserializer<T>(registryClient);

            AutomaticRegistrationBehavior autoRegisterSchemas =
                schemaConfig.AutoRegisterSchemas.HasValue && schemaConfig.AutoRegisterSchemas.Value
                    ? AutomaticRegistrationBehavior.Always
                    : AutomaticRegistrationBehavior.Never;

            serializer =
                new AsyncSchemaRegistrySerializer<T>(registryClient, autoRegisterSchemas);

            isInitialized = true;
        }
        else
        {
            throw new StreamConfigException(
                $"Configuration must inherited from ISchemaRegistryConfig for HostedSchemaAvroSerDes<{typeof(T).Name}");
        }
    }
}
