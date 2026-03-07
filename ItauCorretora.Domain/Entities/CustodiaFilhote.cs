namespace ItauCorretora.Domain.Entities;

public class CustodiaFilhote
{
    public Guid Id { get; private set; }
    public Guid ContaGraficaId { get; private set; }
    public string Ticker { get; private set; }
    public int Quantidade { get; private set; }
    public decimal PrecoMedio { get; private set; }

    protected CustodiaFilhote() { }

    public CustodiaFilhote(Guid contaGraficaId, string ticker)
    {
        Id = Guid.NewGuid();
        ContaGraficaId = contaGraficaId;
        Ticker = ticker;
        Quantidade = 0;
        PrecoMedio = 0m;
    }

    public void AdicionarCompra(int novaQuantidade, decimal precoCompra)
    {
        if (novaQuantidade <= 0 || precoCompra <= 0) return;

        decimal valorTotalAnterior = Quantidade * PrecoMedio;
        decimal valorTotalNovo = novaQuantidade * precoCompra;
        int quantidadeTotal = Quantidade + novaQuantidade;

        PrecoMedio = (valorTotalAnterior + valorTotalNovo) / quantidadeTotal;
        Quantidade = quantidadeTotal;
    }

    public (decimal ValorArrecadado, decimal LucroApurado) RegistrarVenda(int quantidadeVendida, decimal precoVenda)
    {
        if (quantidadeVendida <= 0)
            throw new InvalidOperationException("A quantidade vendida deve ser positiva.");

        if (quantidadeVendida > Quantidade)
            throw new InvalidOperationException($"Saldo insuficiente para venda de {Ticker}. Possui {Quantidade}, tentou vender {quantidadeVendida}.");

        decimal valorArrecadado = quantidadeVendida * precoVenda;

        decimal lucroApurado = (precoVenda - PrecoMedio) * quantidadeVendida;

        Quantidade -= quantidadeVendida;

        if (Quantidade == 0)
        {
            PrecoMedio = 0m;
        }

        return (valorArrecadado, lucroApurado);
    }

    public void Recalcular(IEnumerable<(int Quantidade, decimal Preco)> compras)
    {
        Quantidade = 0;
        PrecoMedio = 0m;

        foreach (var (qtd, preco) in compras)
            AdicionarCompra(qtd, preco);
    }
}