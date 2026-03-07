using ItauCorretora.Application.DTOs;
using ItauCorretora.Domain.Entities;
using ItauCorretora.Domain.Interfaces;

namespace ItauCorretora.Application.UseCases;

public class AderirProdutoUseCase
{
    private readonly IClienteRepository _clienteRepository;

    public AderirProdutoUseCase(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<Cliente> ExecutarAsync(AdesaoClienteRequest request)
    {
        if (await _clienteRepository.CpfJaExisteAsync(request.Cpf))
        {
            throw new InvalidOperationException("CPF já cadastrado no sistema.");
        }

        var cliente = new Cliente(request.Nome, request.Cpf, request.Email, request.ValorMensalAporte);

        await _clienteRepository.AdicionarAsync(cliente);
        await _clienteRepository.SalvarAlteracoesAsync();

        return cliente;
    }
}