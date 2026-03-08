using ItauCorretora.Domain.Entities;
using ItauCorretora.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ItauCorretora.Application.UseCases;

public class ProcessarRebalanceamentoUseCase
{
    private readonly IClienteRepository _clienteRepo;
    private readonly IB3ParserService _b3Parser;
    private readonly IKafkaService _kafkaService;
    private readonly ICestaRepository _cestaRepo; 
    private readonly ILogger<ProcessarRebalanceamentoUseCase> _logger;

    public ProcessarRebalanceamentoUseCase(
        IClienteRepository clienteRepo,
        IB3ParserService b3Parser,
        IKafkaService kafkaService,
        ICestaRepository cestaRepo, 
        ILogger<ProcessarRebalanceamentoUseCase> logger)
    {
        _clienteRepo = clienteRepo;
        _b3Parser = b3Parser;
        _kafkaService = kafkaService;
        _cestaRepo = cestaRepo;
        _logger = logger;
    }

    public async Task ExecutarAsync(string caminhoArquivoB3)
    {
        _logger.LogInformation("Iniciando rebalanceamento dinâmico. Arquivo: {Caminho}", caminhoArquivoB3);

        var cestaAtual = await _cestaRepo.ObterAtualAsync();
        if (cestaAtual == null)
        {
            _logger.LogError("Falha: Nenhuma Cesta Top Five cadastrada no sistema.");
            return;
        }

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

                var recomendacao = cestaAtual.Itens.FirstOrDefault(c => c.Ticker == item.Ticker);
                decimal pesoAlvo = (recomendacao?.Percentual ?? 0m) / 100m; 

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

                        _logger.LogInformation("[VENDA] {Ticker}: {Qtd} cotas para {Cliente}", item.Ticker, qtdParaVender, cliente.Nome);
                    }
                }
            }

            if (totalVendasMes > 20000m && lucroTotalMes > 0m)
            {
                decimal valorImposto = lucroTotalMes * 0.20m;
                await _kafkaService.EnviarEventoIRDedoDuro(new { Cpf = cliente.Cpf, Nome = cliente.Nome, Imposto = valorImposto });
            }

            foreach (var itemCesta in cestaAtual.Itens)
            {
                if (!precosMercado.TryGetValue(itemCesta.Ticker, out decimal precoAtual)) continue;

                var itemCustodia = custodiaAtual.FirstOrDefault(c => c.Ticker == itemCesta.Ticker);
                decimal valorAlvo = patrimonioProjetado * (itemCesta.Percentual / 100m);
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
                            await _clienteRepo.InserirCustodiaAsync(cliente.ContaGrafica.Id, itemCesta.Ticker, qtdParaComprar, precoAtual);
                        }
                        _logger.LogInformation("[COMPRA] {Ticker}: {Qtd} cotas para {Cliente}", itemCesta.Ticker, qtdParaComprar, cliente.Nome);
                    }
                }
            }
        }
        _logger.LogInformation("Rebalanceamento dinâmico concluído.");
    }
}