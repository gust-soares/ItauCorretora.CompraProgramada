using ItauCorretora.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace ItauCorretora.Infrastructure.Services;

[ExcludeFromCodeCoverage]
public class B3ParserService : IB3ParserService
{
    public async Task<Dictionary<string, decimal>> ParseCotacoesAsync(string caminhoArquivo)
    {
        var cotacoes = new Dictionary<string, decimal>();
        var linhas = await File.ReadAllLinesAsync(caminhoArquivo);

        foreach (var linha in linhas)
        {
            if (linha.Length < 100) continue;

            var tipoRegistro = linha.Substring(0, 2);
            if (tipoRegistro != "01") continue;

            string tickerRaw = linha.Substring(12, 12);
            string ticker = tickerRaw.Trim();

            Console.WriteLine($"Lendo Linha: Ticker Encontrado='{ticker}' | Tamanho={linha.Length}");

            string precoStr = linha.Substring(108, 13);
            if (decimal.TryParse(precoStr, out decimal precoRaw))
            {
                decimal precoFinal = precoRaw / 100;
                cotacoes[ticker] = precoFinal;
                Console.WriteLine($"  -> Preço Adicionado: {ticker} = {precoFinal:C2}");
            }
        }

        return cotacoes;
    }
}