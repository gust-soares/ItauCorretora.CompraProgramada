using System.Data;
using Dapper;
using ItauCorretora.Application.DTOs;
using ItauCorretora.Application.Queries;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace ItauCorretora.Infrastructure.Queries;

public class CarteiraQuery : ICarteiraQuery
{
    private readonly string _connectionString;

    public CarteiraQuery(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string não encontrada.");
    }

    public async Task<PosicaoCarteiraDto?> ObterPosicaoClienteAsync(Guid clienteId)
    {
        using IDbConnection db = new MySqlConnection(_connectionString);

        var sql = @"
            SELECT 
                c.Nome as NomeCliente,
                c.Cpf,
                cf.Ticker,
                cf.Quantidade,
                cf.PrecoMedio
            FROM Clientes c
            INNER JOIN ContasGraficas cg ON c.Id = cg.ClienteId
            LEFT JOIN CustodiasFilhotes cf ON cg.Id = cf.ContaGraficaId
            WHERE c.Id = @ClienteId";

        var lookup = new Dictionary<string, PosicaoCarteiraDto>();

        await db.QueryAsync<PosicaoCarteiraDto, ItemCustodiaDto, PosicaoCarteiraDto>(
            sql,
            (carteira, item) =>
            {
                if (!lookup.TryGetValue(carteira.Cpf, out var carteiraEntry))
                {
                    carteiraEntry = carteira;
                    lookup.Add(carteiraEntry.Cpf, carteiraEntry);
                }

                if (item != null && item.Quantidade > 0)
                {
                    carteiraEntry.Ativos.Add(item);
                    carteiraEntry.TotalInvestido += (item.Quantidade * item.PrecoMedio);
                }

                return carteiraEntry;
            },
            new { ClienteId = clienteId },
            splitOn: "Ticker"
        );

        return lookup.Values.FirstOrDefault();
    }
}