using ItauCorretora.Domain.Interfaces;

namespace ItauCorretora.Application.UseCases;

public class AlterarValorAporteUseCase
{
    private readonly IClienteRepository _clienteRepository;

    public AlterarValorAporteUseCase(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task Executar(Guid clienteId, decimal novoValorMensal)
    {
        var clientes = await _clienteRepository.ListarAtivosAsync();
        var cliente = clientes.FirstOrDefault(c => c.Id == clienteId);

        if (cliente == null)
            throw new Exception("Cliente não encontrado ou inativo.");

        if (novoValorMensal <= 0)
            throw new Exception("O valor de aporte deve ser maior que zero.");

        cliente.AlterarAporteMensal(novoValorMensal);

        await _clienteRepository.SalvarAlteracoesAsync();
    }
}