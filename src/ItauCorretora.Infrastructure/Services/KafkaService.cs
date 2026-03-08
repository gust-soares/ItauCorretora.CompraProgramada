using Confluent.Kafka;
using ItauCorretora.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace ItauCorretora.Infrastructure.Services;

[ExcludeFromCodeCoverage]
public class KafkaService : IKafkaService
{
    private readonly ProducerConfig _config;

    public KafkaService(IConfiguration configuration)
    {
        var bootstrapServers = configuration["KafkaSettings:BootstrapServers"] ?? "kafka:9092";

        _config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };
    }

    public async Task EnviarEventoIRDedoDuro(object evento)
    {
        using var producer = new ProducerBuilder<Null, string>(_config).Build();
        var message = System.Text.Json.JsonSerializer.Serialize(evento);

        await producer.ProduceAsync("ir-dedo-duro", new Message<Null, string> { Value = message });
    }
}