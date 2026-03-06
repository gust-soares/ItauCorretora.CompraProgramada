namespace ItauCorretora.Domain.Entities;

public class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Cpf { get; private set; }
    public string Email { get; private set; }
    public decimal ValorMensalAporte { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataAdesao { get; private set; }

    public ContaGrafica ContaGrafica { get; private set; }

    protected Cliente() { }

    public Cliente(string nome, string cpf, string email, decimal valorMensalAporte)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Cpf = cpf;
        Email = email;
        ValorMensalAporte = valorMensalAporte;
        Ativo = true;
        DataAdesao = DateTime.UtcNow;
    }

    public void AlterarValorAporte(decimal novoValor)
    {
        if (novoValor < 100)
            throw new ArgumentException("O valor mínimo de aporte é R$ 100,00.");

        ValorMensalAporte = novoValor;
    }

    public void CancelarAdesao()
    {
        Ativo = false;
    }
}