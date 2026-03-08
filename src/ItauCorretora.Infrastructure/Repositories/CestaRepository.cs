using ItauCorretora.Domain.Entities;
using ItauCorretora.Domain.Interfaces;
using ItauCorretora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ItauCorretora.Infrastructure.Repositories; 

public class CestaRepository : ICestaRepository 
{
    private readonly CompraProgramadaDbContext _context;

    public CestaRepository(CompraProgramadaDbContext context)
    {
        _context = context;
    }

    public async Task SalvarAsync(CestaTopFive cesta)
    {
        await _context.CestasTopFive.AddAsync(cesta);
        await _context.SaveChangesAsync();
    }

    public async Task<CestaTopFive?> ObterAtualAsync()
    {
        return await _context.CestasTopFive
            .Include(c => c.Itens)
            .OrderByDescending(c => c.DataCriacao)
            .FirstOrDefaultAsync();
    }

    public async Task<List<CestaTopFive>> ObterHistoricoAsync()
    {
        return await _context.CestasTopFive
            .Include(c => c.Itens)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync();
    }
}