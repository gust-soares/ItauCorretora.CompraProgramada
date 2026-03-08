namespace ItauCorretora.Domain.Entities;

public class CestaTopFive
{
    public Guid Id { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public List<ItemCesta> Itens { get; private set; } = new();

    public CestaTopFive()
    {
        Id = Guid.NewGuid();
        DataCriacao = DateTime.Now;
    }

    public void AdicionarItem(string ticker, decimal percentual)
    {
        Itens.Add(new ItemCesta(ticker, percentual));
    }

    public bool ValidarCesta() => Itens.Sum(x => x.Percentual) == 100 && Itens.Count == 5;
}

public class ItemCesta
{
    public Guid Id { get; private set; }
    public string Ticker { get; private set; }
    public decimal Percentual { get; private set; }

    public ItemCesta(string ticker, decimal percentual)
    {
        Id = Guid.NewGuid();
        Ticker = ticker;
        Percentual = percentual;
    }
}