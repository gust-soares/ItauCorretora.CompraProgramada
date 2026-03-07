using ItauCorretora.Domain.Entities;
using ItauCorretora.Domain.Interfaces;
using ItauCorretora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ItauCorretora.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly CompraProgramadaDbContext _context;

    public ClienteRepository(CompraProgramadaDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Cliente cliente)
    {
        await _context.Clientes.AddAsync(cliente);
    }

    public async Task<bool> CpfJaExisteAsync(string cpf)
    {
        return await _context.Clientes.AnyAsync(c => c.Cpf == cpf);
    }

    public async Task SalvarAlteracoesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<Cliente>> ListarAtivosAsync()
    {
        return await _context.Clientes
            .Include(c => c.ContaGrafica)
                .ThenInclude(cg => cg.Custodias)
            .Where(c => c.Ativo)
            .ToListAsync();
    }

    public async Task<int> AtualizarCustodiaAsync(Guid contaGraficaId, string ticker, int quantidade, decimal precoMedio)
    {
        return await _context.CustodiasFilhotes
            .Where(c => c.ContaGraficaId == contaGraficaId && c.Ticker == ticker)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Quantidade, quantidade)
                .SetProperty(c => c.PrecoMedio, precoMedio));
    }

    public async Task InserirCustodiaAsync(Guid contaGraficaId, string ticker, int quantidade, decimal precoMedio)
    {
        var custodia = new CustodiaFilhote(contaGraficaId, ticker);
        custodia.AdicionarCompra(quantidade, precoMedio);
        await _context.CustodiasFilhotes.AddAsync(custodia);
        await _context.SaveChangesAsync();
    }
}