using FluentAssertions;
using ItauCorretora.Application.UseCases;
using ItauCorretora.Domain.Entities;
using ItauCorretora.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ItauCorretora.Tests.Application.UseCases;

public class ProcessarRebalanceamentoUseCaseTests
{
    private readonly IClienteRepository _clienteRepoMock;
    private readonly IB3ParserService _b3ParserMock;
    private readonly IKafkaService _kafkaServiceMock;
    private readonly ILogger<ProcessarRebalanceamentoUseCase> _loggerMock;
    private readonly ProcessarRebalanceamentoUseCase _useCase;

    public ProcessarRebalanceamentoUseCaseTests()
    {
        _clienteRepoMock = Substitute.For<IClienteRepository>();
        _b3ParserMock = Substitute.For<IB3ParserService>();
        _kafkaServiceMock = Substitute.For<IKafkaService>();
        _loggerMock = Substitute.For<ILogger<ProcessarRebalanceamentoUseCase>>();

        _useCase = new ProcessarRebalanceamentoUseCase(
            _clienteRepoMock, _b3ParserMock, _kafkaServiceMock, _loggerMock);
    }

    [Fact(DisplayName = "Deve vender ativo que saiu da cesta e calcular IR se lucro for superior a 20k")]
    public async Task ExecutarAsync_VendaAcimaDe20k_DeveEnviarKafka()
    {
        var clienteFake = new Cliente("Gustavo", "11122233344", "gustavo@email.com", 0);
        var contaFake = new ContaGrafica(clienteFake.Id);
        clienteFake.VincularContaGrafica(contaFake);

        var custodiaVale = new CustodiaFilhote(contaFake.Id, "VALE3");
        custodiaVale.AdicionarCompra(1000, 20.00m);

        _clienteRepoMock.ListarAtivosAsync().Returns(new List<Cliente> { clienteFake });
        _clienteRepoMock.ObterCustodiaPorContaAsync(contaFake.Id).Returns(new List<CustodiaFilhote> { custodiaVale });

        var precosMercado = new Dictionary<string, decimal> { { "VALE3", 50.00m } };
        _b3ParserMock.ParseCotacoesAsync(Arg.Any<string>()).Returns(precosMercado);

        var novaCesta = new List<RecomendacaoAcao>();

        await _useCase.ExecutarAsync("caminho_fake", novaCesta);

        await _clienteRepoMock.Received().AtualizarCustodiaAsync(contaFake.Id, "VALE3", 0, 0m);

        await _kafkaServiceMock.Received(1).EnviarEventoIRDedoDuro(Arg.Any<object>());
    }
}