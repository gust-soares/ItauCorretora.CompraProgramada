using System.Diagnostics.CodeAnalysis;

namespace ItauCorretora.Application.DTOs;

[ExcludeFromCodeCoverage]
public class PosicaoCarteiraDto
{
    public string NomeCliente { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public decimal TotalInvestido { get; set; }
    public List<ItemCustodiaDto> Ativos { get; set; } = new();
}

public class ItemCustodiaDto
{
    public string Ticker { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoMedio { get; set; }
    public decimal ValorTotal => Quantidade * PrecoMedio;
}