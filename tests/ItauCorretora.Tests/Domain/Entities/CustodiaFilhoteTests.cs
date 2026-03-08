using FluentAssertions;
using ItauCorretora.Domain.Entities;
using Xunit;

namespace ItauCorretora.Tests.Domain.Entities;

public class CustodiaFilhoteTests
{
    [Fact(DisplayName = "Deve calcular o Preço Médio corretamente ao fazer múltiplas compras")]
    public void AdicionarCompra_MultiplasCompras_DeveCalcularPrecoMedioCorreto()
    {
        var custodia = new CustodiaFilhote(Guid.NewGuid(), "ITUB4");

        custodia.AdicionarCompra(100, 10.00m); 
        custodia.AdicionarCompra(100, 12.00m); 

        custodia.Quantidade.Should().Be(200);
        custodia.PrecoMedio.Should().Be(11.00m);
    }

    [Fact(DisplayName = "Deve calcular o Lucro e o Valor Arrecadado corretamente em uma venda com lucro")]
    public void RegistrarVenda_ComLucro_DeveRetornarValoresCorretos()
    {
        var custodia = new CustodiaFilhote(Guid.NewGuid(), "PETR4");
        custodia.AdicionarCompra(100, 20.00m); 

        var resultado = custodia.RegistrarVenda(50, 25.00m);

        resultado.ValorArrecadado.Should().Be(1250.00m);
        resultado.LucroApurado.Should().Be(250.00m);
        custodia.Quantidade.Should().Be(50); 
        custodia.PrecoMedio.Should().Be(20.00m); 
    }

    [Fact(DisplayName = "Deve zerar o Preço Médio quando a quantidade chegar a zero")]
    public void RegistrarVenda_VendaTotal_DeveZerarPrecoMedioEQuantidade()
    {
        var custodia = new CustodiaFilhote(Guid.NewGuid(), "VALE3");
        custodia.AdicionarCompra(100, 50.00m);

        custodia.RegistrarVenda(100, 60.00m); 

        custodia.Quantidade.Should().Be(0);
        custodia.PrecoMedio.Should().Be(0m);
    }

    [Fact(DisplayName = "Deve lançar exceção ao tentar vender mais do que possui")]
    public void RegistrarVenda_QuantidadeMaiorQueSaldo_DeveLancarExcecao()
    {
        var custodia = new CustodiaFilhote(Guid.NewGuid(), "WEGE3");
        custodia.AdicionarCompra(50, 30.00m);

        Action acao = () => custodia.RegistrarVenda(100, 35.00m); 

        acao.Should().Throw<InvalidOperationException>()
            .WithMessage("*Saldo insuficiente*");
    }

    [Fact(DisplayName = "Não deve permitir venda de quantidade maior do que a disponível")]
    public void RegistrarVenda_QuantidadeInsuficiente_DeveLancarExcecao()
    {
        var custodia = new CustodiaFilhote(Guid.NewGuid(), "ITUB4");
        custodia.AdicionarCompra(10, 30.00m); 

        Action acao = () => custodia.RegistrarVenda(11, 35.00m);

        acao.Should().Throw<InvalidOperationException>()
            .WithMessage("*Saldo insuficiente*");
    }

    [Fact(DisplayName = "Não deve permitir adicionar compra com preço ou quantidade negativa")]
    public void AdicionarCompra_ValoresNegativos_DeveLancarExcecao()
    {
        var custodia = new CustodiaFilhote(Guid.NewGuid(), "VALE3");

        Action actQtd = () => custodia.AdicionarCompra(-1, 50m);
        actQtd.Should().Throw<ArgumentException>()
              .WithMessage("*quantidade deve ser maior que zero*");

        Action actPreco = () => custodia.AdicionarCompra(10, -5m);
        actPreco.Should().Throw<ArgumentException>()
                .WithMessage("*preço deve ser maior que zero*");
    }

    [Fact(DisplayName = "Deve registrar venda com prejuízo corretamente")]
    public void RegistrarVenda_ComPrejuizo_DeveRetornarValoresCorretos()
    {
        var custodia = new CustodiaFilhote(Guid.NewGuid(), "MGLU3");
        custodia.AdicionarCompra(100, 20.00m); // Pagou R$ 20,00

        var resultado = custodia.RegistrarVenda(50, 10.00m); // Vendeu na baixa por R$ 10,00

        resultado.ValorArrecadado.Should().Be(500.00m);
        resultado.LucroApurado.Should().Be(-500.00m); // O lucro tem que ser negativo
        custodia.Quantidade.Should().Be(50);
    }

    [Fact(DisplayName = "Não deve permitir venda com quantidade zero/negativa")]
    public void RegistrarVenda_ValoresInvalidos_DeveLancarExcecao()
    {
        var custodia = new CustodiaFilhote(Guid.NewGuid(), "VALE3");
        custodia.AdicionarCompra(10, 50m);

        // Testa apenas a quantidade negativa, que nós sabemos que o seu código bloqueia!
        Action actQtd = () => custodia.RegistrarVenda(-1, 60m);
        actQtd.Should().Throw<InvalidOperationException>();
    }
}