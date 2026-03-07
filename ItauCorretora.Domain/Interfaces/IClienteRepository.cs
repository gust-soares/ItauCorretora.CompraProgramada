using ItauCorretora.Domain.Entities;

namespace ItauCorretora.Domain.Interfaces;

public interface IClienteRepository
{
    Task AdicionarAsync(Cliente cliente);
    Task<bool> CpfJaExisteAsync(string cpf);
    Task<List<Cliente>> ListarAtivosAsync();
    Task SalvarAlteracoesAsync();

    Task<int> AtualizarCustodiaAsync(Guid contaGraficaId, string ticker, int quantidade, decimal precoMedio);

    Task InserirCustodiaAsync(Guid contaGraficaId, string ticker, int quantidade, decimal precoMedio);
}