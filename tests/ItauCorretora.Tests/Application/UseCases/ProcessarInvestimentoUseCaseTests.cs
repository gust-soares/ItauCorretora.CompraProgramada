using FluentAssertions;
using ItauCorretora.Application.UseCases;
using ItauCorretora.Domain.Entities;
using ItauCorretora.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace ItauCorretora.Tests.Application.UseCases;

public class ProcessarInvestimentoUseCaseTests
{
    private readonly IClienteRepository _clienteRepoMock;
    private readonly IB3ParserService _b3ParserMock;
    private readonly IKafkaService _kafkaServiceMock;
    private readonly ProcessarInvestimentoUseCase _useCase;

    public ProcessarInvestimentoUseCaseTests()
    {
        _clienteRepoMock = Substitute.For<IClienteRepository>();
        _b3ParserMock = Substitute.For<IB3ParserService>();
        _kafkaServiceMock = Substitute.For<IKafkaService>();

        _useCase = new ProcessarInvestimentoUseCase(
            _clienteRepoMock, _b3ParserMock, _kafkaServiceMock);
    }

    [Fact(DisplayName = "Deve calcular e investir o aporte do cliente corretamente chamando o Kafka")]
    public async Task ExecutarAsync_ComClienteAtivoEPrecosValidos_DeveProcessarInvestimento()
    {
        var clienteFake = new Cliente("Gustavo", "11122233344", "gustavo@email.com", 5000m);
        var contaFake = new ContaGrafica(clienteFake.Id);
        clienteFake.VincularContaGrafica(contaFake);

        _clienteRepoMock.ListarAtivosAsync().Returns(new List<Cliente> { clienteFake });

        var precosFalsos = new Dictionary<string, decimal>
        {
            { "ITUB4", 10.00m }, // Se o cliente tem 5000 e a cesta diz 20% pra ITUB4 (R$ 1000). Deve comprar 100 ações.
            { "VALE3", 50.00m }
        };
        _b3ParserMock.ParseCotacoesAsync(Arg.Any<string>()).Returns(precosFalsos);

        await _useCase.ExecutarAsync("caminho_fake.txt");


        await _clienteRepoMock.Received().InserirCustodiaAsync(
            contaFake.Id, "ITUB4", 100, 10.00m);

        await _kafkaServiceMock.ReceivedWithAnyArgs(2).EnviarEventoIRDedoDuro(Arg.Any<object>());
    }
}