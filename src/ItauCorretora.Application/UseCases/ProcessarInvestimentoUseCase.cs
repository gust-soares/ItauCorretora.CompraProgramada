using ItauCorretora.Domain.Entities;
using ItauCorretora.Domain.Interfaces;

namespace ItauCorretora.Application.UseCases;

public class ProcessarInvestimentoUseCase
{
    private readonly IClienteRepository _clienteRepo;
    private readonly IB3ParserService _b3Parser;
    private readonly IKafkaService _kafkaService;
    private readonly ICestaRepository _cestaRepository; 

    public ProcessarInvestimentoUseCase(
        IClienteRepository clienteRepo,
        IB3ParserService b3Parser,
        IKafkaService kafkaService,
        ICestaRepository cestaRepository) 
    {
        _clienteRepo = clienteRepo;
        _b3Parser = b3Parser;
        _kafkaService = kafkaService;
        _cestaRepository = cestaRepository;
    }

    public async Task ExecutarAsync(string caminhoArquivoB3)
    {
        var cestaAtual = await _cestaRepository.ObterAtualAsync();

        if (cestaAtual == null || !cestaAtual.Itens.Any())
        {
            Console.WriteLine("[ERRO] Não há uma Cesta Top Five cadastrada. O processamento foi abortado.");
            return;
        }

        var precosMercado = await _b3Parser.ParseCotacoesAsync(caminhoArquivoB3);
        var clientes = await _clienteRepo.ListarAtivosAsync();

        foreach (var cliente in clientes)
        {
            if (cliente.ContaGrafica == null)
            {
                Console.WriteLine($"[AVISO] Cliente {cliente.Nome} não possui conta gráfica vinculada.");
                continue;
            }

            foreach (var item in cestaAtual.Itens)
            {
                if (!precosMercado.TryGetValue(item.Ticker, out decimal precoAtual))
                {
                    Console.WriteLine($"[AVISO] Ticker {item.Ticker} não encontrado no arquivo B3.");
                    continue;
                }

                decimal valorParaInvestir = cliente.ValorMensalAporte * (item.Percentual / 100);
                int quantidadeCalculada = (int)(valorParaInvestir / precoAtual);

                if (quantidadeCalculada <= 0) continue;

                int linhasAfetadas = await _clienteRepo.AtualizarCustodiaAsync(
                    cliente.ContaGrafica.Id, item.Ticker, quantidadeCalculada, precoAtual);

                if (linhasAfetadas == 0)
                {
                    await _clienteRepo.InserirCustodiaAsync(
                        cliente.ContaGrafica.Id, item.Ticker, quantidadeCalculada, precoAtual);
                }

                var valorTotalDaCompra = quantidadeCalculada * precoAtual;
                var eventoIr = new
                {
                    Cpf = cliente.Cpf,
                    Nome = cliente.Nome,
                    Ticker = item.Ticker,
                    Quantidade = quantidadeCalculada,
                    ValorTotal = valorTotalDaCompra,
                    ImpostoDedoDuro = valorTotalDaCompra * 0.00005m,
                    DataOperacao = DateTime.UtcNow
                };

                await _kafkaService.EnviarEventoIRDedoDuro(eventoIr);

                Console.WriteLine($"[DEBUG] Executado: {quantidadeCalculada} cotas de {item.Ticker} para {cliente.Nome}");
            }
        }

        Console.WriteLine("[DEBUG] Processamento dinâmico concluído com sucesso!");
    }
}