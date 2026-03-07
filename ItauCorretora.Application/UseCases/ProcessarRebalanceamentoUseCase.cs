using ItauCorretora.Domain.Entities;
using ItauCorretora.Domain.Interfaces;

namespace ItauCorretora.Application.UseCases;

public class ProcessarRebalanceamentoUseCase
{
    private readonly IClienteRepository _clienteRepo;
    private readonly IB3ParserService _b3Parser;
    private readonly IKafkaService _kafkaService;

    public ProcessarRebalanceamentoUseCase(
        IClienteRepository clienteRepo,
        IB3ParserService b3Parser,
        IKafkaService kafkaService)
    {
        _clienteRepo = clienteRepo;
        _b3Parser = b3Parser;
        _kafkaService = kafkaService;
    }

    public async Task ExecutarAsync(string caminhoArquivoB3, List<RecomendacaoAcao> novaCesta)
    {
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
                {
                    patrimonioInvestido += item.Quantidade * preco;
                }
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
                        var (valorArrecadado, lucroApurado) = item.RegistrarVenda(qtdParaVender, precoAtual);

                        totalVendasMes += valorArrecadado;
                        lucroTotalMes += lucroApurado;

                        await _clienteRepo.AtualizarCustodiaAsync(cliente.ContaGrafica.Id, item.Ticker, item.Quantidade, item.PrecoMedio);

                        Console.WriteLine($"[VENDA] {qtdParaVender} cotas de {item.Ticker} vendidas para o cliente {cliente.Nome}.");
                    }
                }
            }
            if (totalVendasMes > 20000m && lucroTotalMes > 0m)
            {
                decimal valorImposto = lucroTotalMes * 0.20m; 

                var eventoIr = new
                {
                    Cpf = cliente.Cpf,
                    Nome = cliente.Nome,
                    TotalVendasNoMes = totalVendasMes,
                    LucroTributavel = lucroTotalMes,
                    ImpostoDevido = valorImposto,
                    DataApuracao = DateTime.UtcNow,
                    Motivo = "Venda mensal superior a R$ 20.000,00"
                };

                await _kafkaService.EnviarEventoIRDedoDuro(eventoIr);
                Console.WriteLine($"[KAFKA] DARF Gerado! IR de {valorImposto:C} enviado para {cliente.Nome}.");
            }

            foreach (var acao in novaCesta)
            {
                if (!precosMercado.TryGetValue(acao.Ticker, out decimal precoAtual)) continue;

                var itemCustodia = custodiaAtual.FirstOrDefault(c => c.Ticker == acao.Ticker);
                int qtdAtual = itemCustodia?.Quantidade ?? 0;
                decimal valorAtual = qtdAtual * precoAtual;

                decimal valorAlvo = patrimonioProjetado * acao.Percentual;

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
                        Console.WriteLine($"[COMPRA] {qtdParaComprar} cotas de {acao.Ticker} compradas para o cliente {cliente.Nome}.");
                    }
                }
            }
        }

        Console.WriteLine("[DEBUG] Rebalanceamento e Apuração de IR concluídos com sucesso!");
    }
}