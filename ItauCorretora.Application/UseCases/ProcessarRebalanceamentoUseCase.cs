using ItauCorretora.Domain.Entities;
using ItauCorretora.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ItauCorretora.Application.UseCases;

public class ProcessarRebalanceamentoUseCase
{
    private readonly IClienteRepository _clienteRepo;
    private readonly IB3ParserService _b3Parser;
    private readonly IKafkaService _kafkaService;
    private readonly ILogger<ProcessarRebalanceamentoUseCase> _logger;

    public ProcessarRebalanceamentoUseCase(
        IClienteRepository clienteRepo,
        IB3ParserService b3Parser,
        IKafkaService kafkaService,
        ILogger<ProcessarRebalanceamentoUseCase> logger)
    {
        _clienteRepo = clienteRepo;
        _b3Parser = b3Parser;
        _kafkaService = kafkaService;
        _logger = logger;
    }

    public async Task ExecutarAsync(string caminhoArquivoB3, List<RecomendacaoAcao> novaCesta)
    {
        _logger.LogInformation("Iniciando processamento de rebalanceamento. Arquivo: {Caminho}", caminhoArquivoB3);

        var precosMercado = await _b3Parser.ParseCotacoesAsync(caminhoArquivoB3);
        var clientes = await _clienteRepo.ListarAtivosAsync();

        foreach (var cliente in clientes)
        {
            if (cliente.ContaGrafica == null) continue;

            var custodiaAtual = await _clienteRepo.ObterCustodiaPorContaAsync(cliente.ContaGrafica.Id);

            decimal patrimonioInvestido = 0m;
            foreach (var item in custodiaAtual)
            {
                if (precosMercado.TryGetValue(item.Ticker, out decimal preco))
                    patrimonioInvestido += item.Quantidade * preco;
            }

            decimal patrimonioProjetado = patrimonioInvestido + cliente.ValorMensalAporte;
            decimal totalVendasMes = 0m;
            decimal lucroTotalMes = 0m;

            foreach (var item in custodiaAtual)
            {
                if (!precosMercado.TryGetValue(item.Ticker, out decimal precoAtual)) continue;

                var recomendacao = novaCesta.FirstOrDefault(c => c.Ticker == item.Ticker);
                decimal pesoAlvo = recomendacao?.Percentual ?? 0m;

                decimal valorAlvo = patrimonioProjetado * pesoAlvo;
                decimal valorAtual = item.Quantidade * precoAtual;

                if (valorAtual > valorAlvo)
                {
                    decimal valorParaVender = valorAtual - valorAlvo;
                    int qtdParaVender = (int)(valorParaVender / precoAtual);

                    if (qtdParaVender > 0)
                    {
                        var resultado = item.RegistrarVenda(qtdParaVender, precoAtual);
                        totalVendasMes += resultado.ValorArrecadado;
                        lucroTotalMes += resultado.LucroApurado;

                        await _clienteRepo.AtualizarCustodiaAsync(cliente.ContaGrafica.Id, item.Ticker, item.Quantidade, item.PrecoMedio);

                        _logger.LogInformation("Venda executada: {Quantidade} cotas de {Ticker} para o cliente {Cliente}",
                            qtdParaVender, item.Ticker, cliente.Nome);
                    }
                }
            }

            if (totalVendasMes > 20000m && lucroTotalMes > 0m)
            {
                decimal valorImposto = lucroTotalMes * 0.20m;

                var eventoIr = new { Cpf = cliente.Cpf, Nome = cliente.Nome, ValorImposto = valorImposto };
                await _kafkaService.EnviarEventoIRDedoDuro(eventoIr);

                _logger.LogWarning("Evento de IR enviado ao Kafka para o cliente {Cliente}. Valor: {Imposto}",
                    cliente.Nome, valorImposto);
            }

            foreach (var acao in novaCesta)
            {
                if (!precosMercado.TryGetValue(acao.Ticker, out decimal precoAtual)) continue;

                var itemCustodia = custodiaAtual.FirstOrDefault(c => c.Ticker == acao.Ticker);
                decimal valorAlvo = patrimonioProjetado * acao.Percentual;
                decimal valorAtual = (itemCustodia?.Quantidade ?? 0) * precoAtual;

                if (valorAtual < valorAlvo)
                {
                    decimal valorParaComprar = valorAlvo - valorAtual;
                    int qtdParaComprar = (int)(valorParaComprar / precoAtual);

                    if (qtdParaComprar > 0)
                    {
                        if (itemCustodia != null)
                        {
                            itemCustodia.AdicionarCompra(qtdParaComprar, precoAtual);
                            await _clienteRepo.AtualizarCustodiaAsync(cliente.ContaGrafica.Id, itemCustodia.Ticker, itemCustodia.Quantidade, itemCustodia.PrecoMedio);
                        }
                        else
                        {
                            await _clienteRepo.InserirCustodiaAsync(cliente.ContaGrafica.Id, acao.Ticker, qtdParaComprar, precoAtual);
                        }
                        _logger.LogInformation("Compra executada: {Quantidade} cotas de {Ticker} para o cliente {Cliente}",
                            qtdParaComprar, acao.Ticker, cliente.Nome);
                    }
                }
            }
        }

        _logger.LogInformation("Rebalanceamento finalizado com sucesso.");
    }
}