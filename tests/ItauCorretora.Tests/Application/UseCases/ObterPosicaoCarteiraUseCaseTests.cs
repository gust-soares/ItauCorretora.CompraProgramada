using FluentAssertions;
using ItauCorretora.Application.UseCases;
using ItauCorretora.Domain.Entities;
using ItauCorretora.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace ItauCorretora.Tests.Application.UseCases;

public class ObterPosicaoCarteiraUseCaseTests
{
    private readonly IClienteRepository _clienteRepoMock;
    private readonly IB3ParserService _b3ParserMock;
    private readonly ObterPosicaoCarteiraUseCase _useCase;

    public ObterPosicaoCarteiraUseCaseTests()
    {
        _clienteRepoMock = Substitute.For<IClienteRepository>();
        _b3ParserMock = Substitute.For<IB3ParserService>();

        _useCase = new ObterPosicaoCarteiraUseCase(_clienteRepoMock, _b3ParserMock);
    }

    [Fact(DisplayName = "Deve retornar a posição consolidada com lucro/prejuízo calculado")]
    public async Task ExecutarAsync_ComCustodiaEPrecosValidos_DeveCalcularRentabilidade()
    {
        // Arrange
        var clienteFake = new Cliente("Gustavo", "12345678901", "gustavo@email.com", 5000m);
        var contaFake = new ContaGrafica(clienteFake.Id);
        clienteFake.VincularContaGrafica(contaFake);

        var custodia = new CustodiaFilhote(contaFake.Id, "PETR4");
        custodia.AdicionarCompra(10, 30.00m); // Gastou 300,00

        _clienteRepoMock.ListarAtivosAsync().Returns(new List<Cliente> { clienteFake });
        _clienteRepoMock.ObterCustodiaPorContaAsync(contaFake.Id).Returns(new List<CustodiaFilhote> { custodia });

        // Simula que hoje a ação vale R$ 45,00 (Lucro de 15,00 por cota = 150,00 total)
        var precosB3 = new Dictionary<string, decimal> { { "PETR4", 45.00m } };
        _b3ParserMock.ParseCotacoesAsync(Arg.Any<string>()).Returns(precosB3);

        // Act
        var resultado = await _useCase.ExecutarAsync(clienteFake.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado.NomeCliente.Should().Be("Gustavo");
        resultado.TotalInvestido.Should().Be(300.00m);
        resultado.PatrimonioTotal.Should().Be(450.00m);
        resultado.LucroPrejuizoTotal.Should().Be(150.00m); // 450 - 300

        resultado.Ativos.Should().ContainSingle();
        var ativo = resultado.Ativos.First();
        ativo.Ticker.Should().Be("PETR4");
        ativo.PercentualRentabilidade.Should().Be(50.00m); // Subiu de 30 para 45 (50%)
    }

    [Fact(DisplayName = "Deve lançar exceção se o cliente não for encontrado")]
    public async Task ExecutarAsync_ClienteInexistente_DeveLancarExcecao()
    {
        // Arrange
        _clienteRepoMock.ListarAtivosAsync().Returns(new List<Cliente>());

        // Act
        Func<Task> acao = async () => await _useCase.ExecutarAsync(Guid.NewGuid());

        // Assert
        await acao.Should().ThrowAsync<Exception>().WithMessage("*Cliente ou conta gráfica não encontrados*");
    }
}