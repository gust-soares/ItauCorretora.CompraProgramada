using FluentAssertions;
using ItauCorretora.Domain.Entities;
using Xunit;

namespace ItauCorretora.Tests.Domain.Entities;

public class ClienteTests
{
    [Fact(DisplayName = "Deve criar um cliente válido com todas as propriedades")]
    public void CriarCliente_ComDadosValidos_DeveAtribuirPropriedades()
    {
        var cliente = new Cliente("Gustavo", "11122233344", "gustavo@email.com", 5000m);

        cliente.Nome.Should().Be("Gustavo");
        cliente.Cpf.Should().Be("11122233344");
        cliente.ValorMensalAporte.Should().Be(5000m);
        cliente.ContaGrafica.Should().NotBeNull();
        cliente.ContaGrafica.ClienteId.Should().Be(cliente.Id);
    }

    [Fact(DisplayName = "Deve lançar exceção ao vincular uma conta gráfica nula")]
    public void VincularContaGrafica_ContaNula_DeveLancarExcecao()
    {
        var cliente = new Cliente("Gustavo", "11122233344", "gustavo@email.com", 5000m);

        Action acao = () => cliente.VincularContaGrafica(null!);

        acao.Should().Throw<ArgumentNullException>()
            .WithMessage("*A conta gráfica não pode ser nula*");
    }

    [Fact(DisplayName = "Deve vincular a conta gráfica corretamente ao cliente")]
    public void VincularContaGrafica_ContaValida_DeveVincularComSucesso()
    {
        var cliente = new Cliente("Gustavo", "11122233344", "gustavo@email.com", 5000m);
        var conta = new ContaGrafica(cliente.Id);

        cliente.VincularContaGrafica(conta);

        cliente.ContaGrafica.Should().NotBeNull();
        cliente.ContaGrafica.Id.Should().Be(conta.Id);
    }
}