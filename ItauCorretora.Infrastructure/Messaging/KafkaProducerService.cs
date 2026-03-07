using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ItauCorretora.Domain.Interfaces;

namespace ItauCorretora.Infrastructure.Messaging;

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