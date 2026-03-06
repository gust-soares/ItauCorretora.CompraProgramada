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

    public void AbaterVenda(int quantidadeVendida)
    {
        if (quantidadeVendida > Quantidade)
            throw new InvalidOperationException("Saldo insuficiente na custódia.");

        Quantidade -= quantidadeVendida;
    }
}