using ItauCorretora.Domain.Interfaces;

namespace ItauCorretora.Application.UseCases;

public class CancelarAdesaoUseCase
{
    private readonly IClienteRepository _clienteRepository;

    public CancelarAdesaoUseCase(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task Executar(Guid clienteId)
    {
        var clientes = await _clienteRepository.ListarAtivosAsync();
        var cliente = clientes.FirstOrDefault(c => c.Id == clienteId);

        if (cliente == null)
            throw new Exception("Cliente não encontrado.");

        cliente.DesativarAdesao();

        await _clienteRepository.SalvarAlteracoesAsync();
    }
}