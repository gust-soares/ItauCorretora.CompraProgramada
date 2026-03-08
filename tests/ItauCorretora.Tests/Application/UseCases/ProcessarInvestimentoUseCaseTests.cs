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
    private readonly ICestaRepository _cestaRepoMock; 
    private readonly ProcessarInvestimentoUseCase _useCase;

    public ProcessarInvestimentoUseCaseTests()
    {
        _clienteRepoMock = Substitute.For<IClienteRepository>();
        _b3ParserMock = Substitute.For<IB3ParserService>();
        _kafkaServiceMock = Substitute.For<IKafkaService>();
        _cestaRepoMock = Substitute.For<ICestaRepository>(); 

        _useCase = new ProcessarInvestimentoUseCase(
            _clienteRepoMock, _b3ParserMock, _kafkaServiceMock, _cestaRepoMock);
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
            { "ITUB4", 10.00m }, 
            { "VALE3", 50.00m }
        };
        _b3ParserMock.ParseCotacoesAsync(Arg.Any<string>()).Returns(precosFalsos);

        var cestaFake = new CestaTopFive();
        cestaFake.AdicionarItem("ITUB4", 20);
        _cestaRepoMock.ObterAtualAsync().Returns(cestaFake);

        await _useCase.ExecutarAsync("caminho_fake.txt");

        await _clienteRepoMock.Received().InserirCustodiaAsync(
            contaFake.Id, "ITUB4", 100, 10.00m);
    }
}