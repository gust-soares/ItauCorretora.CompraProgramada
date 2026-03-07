namespace ItauCorretora.Domain.Entities;

public class ContaGrafica
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public string NumeroConta { get; private set; }
    public DateTime DataCriacao { get; private set; }

    private readonly List<CustodiaFilhote> _custodias = new();
    public IReadOnlyCollection<CustodiaFilhote> Custodias => _custodias.AsReadOnly();

    protected ContaGrafica() { }

    public ContaGrafica(Guid clienteId)
    {
        Id = Guid.NewGuid();
        ClienteId = clienteId;
        DataCriacao = DateTime.UtcNow;
        var random = new Random();
        NumeroConta = $"{random.Next(1000, 9999)}-{random.Next(0, 9)}";
    }

    /// <summary>
    /// Registra uma compra avulsa, acumulando sobre a posição existente.
    /// Use para ordens de compra individuais do usuário.
    /// </summary>
    public void AdicionarOuAtualizarCustodia(string ticker, int quantidade, decimal precoCompra)
    {
        var custodiaExistente = _custodias.FirstOrDefault(c => c.Ticker == ticker);
        if (custodiaExistente != null)
        {
            custodiaExistente.AdicionarCompra(quantidade, precoCompra);
        }
        else
        {
            var novaCustodia = new CustodiaFilhote(this.Id, ticker);
            novaCustodia.AdicionarCompra(quantidade, precoCompra);
            _custodias.Add(novaCustodia);
        }
    }

    /// <summary>
    /// Substitui a posição atual pelo novo valor calculado.
    /// Use no job mensal de recálculo para evitar acúmulo a cada execução.
    /// </summary>
    public void AtualizarCustodiaCalculada(string ticker, int quantidadeCalculada, decimal precoAtual)
    {
        var custodiaExistente = _custodias.FirstOrDefault(c => c.Ticker == ticker);
        if (custodiaExistente != null)
        {
            // Substitui: zera e define o novo valor calculado
            custodiaExistente.Recalcular(new[] { (quantidadeCalculada, precoAtual) });
        }
        else
        {
            var novaCustodia = new CustodiaFilhote(this.Id, ticker);
            novaCustodia.AdicionarCompra(quantidadeCalculada, precoAtual);
            _custodias.Add(novaCustodia);
        }
    }
}