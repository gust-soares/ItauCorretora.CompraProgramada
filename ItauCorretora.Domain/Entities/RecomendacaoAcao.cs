namespace ItauCorretora.Domain.Entities;

public class RecomendacaoAcao
{
    public Guid Id { get; private set; }
    public string Ticker { get; private set; }
    public decimal Percentual { get; private set; } 

    public RecomendacaoAcao(string ticker, decimal percentual)
    {
        Id = Guid.NewGuid();
        Ticker = ticker;
        Percentual = percentual;
    }
}