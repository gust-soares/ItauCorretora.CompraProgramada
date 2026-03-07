using ItauCorretora.Domain.Interfaces;

namespace ItauCorretora.Infrastructure.Services;

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

            // Pegamos um pedaço maior da linha para ver o que tem nela
            string tickerRaw = linha.Substring(12, 12);
            string ticker = tickerRaw.Trim();

            // Vamos imprimir no console para você ver enquanto o Swagger roda
            Console.WriteLine($"Lendo Linha: Ticker Encontrado='{ticker}' | Tamanho={linha.Length}");

            // Tente comentar a linha do tipoMercado temporariamente para testar
            // string tipoMercado = linha.Substring(24, 3);
            // if (tipoMercado != "010") continue; 

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