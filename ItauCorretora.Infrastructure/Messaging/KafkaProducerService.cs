using Confluent.Kafka;
using ItauCorretora.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace ItauCorretora.Infrastructure.Messaging;

[ExcludeFromCodeCoverage]
public class KafkaProducerService : IKafkaService
{
    private readonly IConfiguration _config;
    private readonly ProducerConfig _producerConfig;

    public KafkaProducerService(IConfiguration config)
    {
        _config = config;
        _producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };
    }

    public async Task EnviarEventoIRDedoDuro(object mensagem)
    {
        using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();

        var json = JsonSerializer.Serialize(mensagem);

        await producer.ProduceAsync("ir-dedo-duro-v1", new Message<Null, string>
        {
            Value = json
        });

        Console.WriteLine($">>> [KAFKA] Evento enviado para o tópico ir-dedo-duro-v1");
    }
}