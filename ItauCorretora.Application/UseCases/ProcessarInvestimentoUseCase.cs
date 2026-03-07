using ItauCorretora.Domain.Entities;
using ItauCorretora.Domain.Interfaces;

namespace ItauCorretora.Application.UseCases;

public class ProcessarInvestimentoUseCase
{
    private readonly IClienteRepository _clienteRepo;
    private readonly IB3ParserService _b3Parser;
    private readonly IKafkaService _kafkaService;

    public ProcessarInvestimentoUseCase(
        IClienteRepository clienteRepo,
        IB3ParserService b3Parser,
        IKafkaService kafkaService)
    {
        _clienteRepo = clienteRepo;
        _b3Parser = b3Parser;
        _kafkaService = kafkaService;
    }

    public async Task ExecutarAsync(string caminhoArquivoB3)
    {
        var precosMercado = await _b3Parser.ParseCotacoesAsync(caminhoArquivoB3);

        var cestaTopFive = new List<RecomendacaoAcao>
        {
            new("YDUQ3T", 0.20m),
            new("WIZC3T", 0.20m),
            new("WHRL4T", 0.20m),
            new("ITUB4",  0.20m),
            new("VALE3",  0.20m)
        };

        var clientes = await _clienteRepo.ListarAtivosAsync();

        foreach (var cliente in clientes)
        {
            if (cliente.ContaGrafica == null)
            {
                Console.WriteLine($"[AVISO] Cliente {cliente.Nome} não possui conta gráfica vinculada.");
                continue;
            }

            foreach (var acao in cestaTopFive)
            {
                if (!precosMercado.TryGetValue(acao.Ticker, out decimal precoAtual))
                {
                    Console.WriteLine($"[AVISO] Ticker {acao.Ticker} não encontrado no arquivo B3.");
                    continue;
                }

                decimal valorParaInvestir = cliente.ValorMensalAporte * acao.Percentual;
                int quantidadeCalculada = (int)(valorParaInvestir / precoAtual);

                if (quantidadeCalculada <= 0) continue;

                int linhasAfetadas = await _clienteRepo.AtualizarCustodiaAsync(
                    cliente.ContaGrafica.Id, acao.Ticker, quantidadeCalculada, precoAtual);

                if (linhasAfetadas == 0)
                {
                    await _clienteRepo.InserirCustodiaAsync(
                        cliente.ContaGrafica.Id, acao.Ticker, quantidadeCalculada, precoAtual);
                }

                var valorTotalDaCompra = quantidadeCalculada * precoAtual;
                var eventoIr = new
                {
                    Cpf = cliente.Cpf,
                    Nome = cliente.Nome,
                    Ticker = acao.Ticker,
                    Quantidade = quantidadeCalculada,
                    ValorTotal = valorTotalDaCompra,
                    ImpostoDedoDuro = valorTotalDaCompra * 0.00005m,
                    DataOperacao = DateTime.UtcNow
                };

                await _kafkaService.EnviarEventoIRDedoDuro(eventoIr);

                Console.WriteLine($"[DEBUG] Calculado e Enviado ao Kafka: {quantidadeCalculada} cotas de {acao.Ticker} para o cliente {cliente.Nome}");
            }
        }

        Console.WriteLine("[DEBUG] Processamento e notificações Kafka concluídos com sucesso!");
    }
}