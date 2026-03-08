using FluentAssertions;
using ItauCorretora.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace ItauCorretora.Tests.Infrastructure.Services;

public class B3ParserServiceTests : IDisposable
{
    private readonly B3ParserService _parserService;
    private readonly string _caminhoArquivoTemp;

    public B3ParserServiceTests()
    {
        _caminhoArquivoTemp = Path.GetTempFileName();

        // 1. Criamos a string falsa e preenchemos com espaços no final (PadRight) 
        // para garantir que a linha tenha no mínimo 100 caracteres e o Substring não quebre.
        var linhaHeader = "00COTAHIST.2026BOVESPA 20260305".PadRight(200, ' ');
        var linhaPetr4 = "012026030502PETR4       010PETROBRAS   PN  ATZ N2   R$  000000000405300000000040770000000003998000000000404100000000040690000000004065000000000406957435000000000053079000000000214521293200000000000000009999123100000010000000000000BRPETRACNPR6224".PadRight(200, ' ');
        var linhaItub4 = "012026030502ITUB4       010ITAUUNIBANCOPN  EJ  N1   R$  000000000448000000000045000000000004333000000000436700000000043510000000004350000000000435242610000000000037085400000000161967228700000000000000009999123100000010000000000000BRITUBACNPR1366".PadRight(200, ' ');
        var linhaTrailer = "99COTAHIST.2026BOVESPA 2026030500000000003".PadRight(200, ' ');

        var linhasB3 = new[] { linhaHeader, linhaPetr4, linhaItub4, linhaTrailer };
        File.WriteAllLines(_caminhoArquivoTemp, linhasB3);

        var configMock = Substitute.For<IConfiguration>();
        _parserService = new B3ParserService(configMock);
    }

    [Fact(DisplayName = "Deve ler arquivo da B3 e extrair dicionário de cotações corretamente")]
    public async Task ParseCotacoesAsync_ArquivoValido_DeveRetornarPrecos()
    {
        var cotacoes = await _parserService.ParseCotacoesAsync(_caminhoArquivoTemp);

        cotacoes.Should().NotBeNull();
        cotacoes.Should().HaveCount(2);

        // Verifica se extraiu os ativos (as chaves limpas)
        cotacoes.ContainsKey("PETR4").Should().BeTrue();
        cotacoes.ContainsKey("ITUB4").Should().BeTrue();
    }

    [Fact(DisplayName = "Deve retornar dicionário vazio se arquivo não for encontrado")]
    public async Task ParseCotacoesAsync_ArquivoInexistente_DeveRetornarVazio()
    {
        // Seu serviço não lança exceção, ele é seguro. Ele retorna vazio. Vamos testar isso!
        var cotacoes = await _parserService.ParseCotacoesAsync("C:\\caminho_fake.txt");

        cotacoes.Should().NotBeNull();
        cotacoes.Should().BeEmpty();
    }

    public void Dispose()
    {
        if (File.Exists(_caminhoArquivoTemp)) File.Delete(_caminhoArquivoTemp);
    }
}