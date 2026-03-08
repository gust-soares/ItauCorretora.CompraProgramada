using ItauCorretora.Application.DTOs;
using ItauCorretora.Domain.Interfaces;

namespace ItauCorretora.Application.UseCases;

public class ObterPosicaoCarteiraUseCase
{
    private readonly IClienteRepository _clienteRepo;
    private readonly IB3ParserService _b3Parser;

    public ObterPosicaoCarteiraUseCase(IClienteRepository clienteRepo, IB3ParserService b3Parser)
    {
        _clienteRepo = clienteRepo;
        _b3Parser = b3Parser;
    }

    public async Task<PosicaoClienteDto> ExecutarAsync(Guid clienteId)
    {
        var clientes = await _clienteRepo.ListarAtivosAsync();
        var cliente = clientes.FirstOrDefault(c => c.Id == clienteId);

        if (cliente == null || cliente.ContaGrafica == null)
            throw new Exception("Cliente ou conta gráfica não encontrados.");

        var precosMercado = await _b3Parser.ParseCotacoesAsync(null);

        var response = new PosicaoClienteDto
        {
            NomeCliente = cliente.Nome,
            Cpf = cliente.Cpf
        };

        var custodiaAtual = await _clienteRepo.ObterCustodiaPorContaAsync(cliente.ContaGrafica.Id);

        foreach (var item in custodiaAtual)
        {
            precosMercado.TryGetValue(item.Ticker, out decimal precoAtual);
            if (precoAtual == 0) precoAtual = item.PrecoMedio;

            var valorInvestido = item.Quantidade * item.PrecoMedio;
            var valorMercado = item.Quantidade * precoAtual;
            var lucro = valorMercado - valorInvestido;

            decimal rentabilidade = item.PrecoMedio > 0
                ? ((precoAtual / item.PrecoMedio) - 1) * 100
                : 0;

            response.Ativos.Add(new PosicaoAtivoDto
            {
                Ticker = item.Ticker,
                Quantidade = item.Quantidade,
                PrecoMedio = item.PrecoMedio,
                PrecoAtual = precoAtual,
                ValorInvestido = valorInvestido,
                ValorMercado = valorMercado,
                LucroPrejuizo = lucro,
                PercentualRentabilidade = Math.Round(rentabilidade, 2)
            });

            response.TotalInvestido += valorInvestido;
            response.PatrimonioTotal += valorMercado;
            response.LucroPrejuizoTotal += lucro;
        }

        return response;
    }
}