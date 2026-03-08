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
        this.ContaGrafica = new ContaGrafica(this.Id);
    }

    public void AlterarValorAporte(decimal novoValor)
    {
        if (novoValor < 100)
            throw new ArgumentException("O valor mínimo de aporte é R$ 100,00.");

        ValorMensalAporte = novoValor;
    }

    public void VincularContaGrafica(ContaGrafica contaGrafica)
    {
        if (contaGrafica == null)
            throw new ArgumentNullException(nameof(contaGrafica), "A conta gráfica não pode ser nula.");

        ContaGrafica = contaGrafica;
    }
    public void CancelarAdesao()
    {
        Ativo = false;
    }

    public void AlterarAporteMensal(decimal novoValor)
    {
        if (novoValor <= 0)
            throw new Exception("O valor de aporte deve ser maior que zero.");

        this.ValorMensalAporte = novoValor;
    }

    public void DesativarAdesao()
    {
        this.Ativo = false;
    }
}