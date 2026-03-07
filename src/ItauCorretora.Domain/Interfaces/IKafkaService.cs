namespace ItauCorretora.Domain.Interfaces;

public interface IKafkaService
{
    Task EnviarEventoIRDedoDuro(object mensagem);
}