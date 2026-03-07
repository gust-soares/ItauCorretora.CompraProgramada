using ItauCorretora.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace ItauCorretora.Infrastructure.Services
{
    public class B3ParserService : IB3ParserService
    {
        private readonly string _directoryPath;

        public B3ParserService(IConfiguration configuration)
        {
            var relativePath = configuration["B3Settings:CotacoesPath"] ?? "cotacoes";
            _directoryPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativePath));
        }

        public async Task<Dictionary<string, decimal>> ParseCotacoesAsync(string folderPath = null)
        {
            var mapaCotacoes = new Dictionary<string, decimal>();
            var pathParaUsar = folderPath ?? _directoryPath;

            if (!Directory.Exists(pathParaUsar))
                return mapaCotacoes;

            var arquivos = Directory.GetFiles(pathParaUsar, "COTAHIST_*.TXT");

            foreach (var caminhoArquivo in arquivos)
            {
                var linhas = await File.ReadAllLinesAsync(caminhoArquivo);

                for (int i = 1; i < linhas.Length - 1; i++)
                {
                    var linha = linhas[i];
                    try
                    {
                        string ticker = linha.Substring(12, 12).Trim();
                        decimal preco = decimal.Parse(linha.Substring(108, 13), CultureInfo.InvariantCulture) / 100m;

                        mapaCotacoes[ticker] = preco;
                    }
                    catch { /* Linha malformada, ignora e segue */ }
                }
            }

            return mapaCotacoes;
        }
    }
}