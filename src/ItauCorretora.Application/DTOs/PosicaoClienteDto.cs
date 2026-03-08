namespace ItauCorretora.Application.DTOs;

public class PosicaoClienteDto
{
    public string NomeCliente { get; set; }
    public string Cpf { get; set; }
    public decimal TotalInvestido { get; set; }
    public decimal PatrimonioTotal { get; set; } 
    public decimal LucroPrejuizoTotal { get; set; }
    public List<PosicaoAtivoDto> Ativos { get; set; } = new();
}

public class PosicaoAtivoDto
{
    public string Ticker { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoMedio { get; set; }
    public decimal PrecoAtual { get; set; }
    public decimal ValorInvestido { get; set; }
    public decimal ValorMercado { get; set; }
    public decimal LucroPrejuizo { get; set; }
    public decimal PercentualRentabilidade { get; set; }
}