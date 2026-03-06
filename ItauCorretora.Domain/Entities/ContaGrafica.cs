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

    public ContaGrafica(Guid clienteId, string numeroConta)
    {
        Id = Guid.NewGuid();
        ClienteId = clienteId;
        NumeroConta = numeroConta;
        DataCriacao = DateTime.UtcNow;
    }
}