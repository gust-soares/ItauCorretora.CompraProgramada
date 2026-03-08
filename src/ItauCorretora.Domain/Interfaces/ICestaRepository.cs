using ItauCorretora.Domain.Entities;

namespace ItauCorretora.Domain.Interfaces;

public interface ICestaRepository
{
    Task SalvarAsync(CestaTopFive cesta);
    Task<CestaTopFive?> ObterAtualAsync();
    Task<List<CestaTopFive>> ObterHistoricoAsync();
}