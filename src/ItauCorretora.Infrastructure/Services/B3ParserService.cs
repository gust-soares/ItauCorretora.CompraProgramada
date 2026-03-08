using ItauCorretora.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
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

        public async Task<Dictionary<string, decimal>> ParseCotacoesAsync(string pathParaUsar = null)
        {
            var mapaCotacoes = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            var targetPath = pathParaUsar ?? _directoryPath;

            List<string> arquivosParaProcessar = new();

            if (File.Exists(targetPath))
            {
                arquivosParaProcessar.Add(targetPath);
            }
            else if (Directory.Exists(targetPath))
            {
                var arquivos = Directory.GetFiles(targetPath, "COTAHIST_*.TXT")
                                        .OrderBy(f => f)
                                        .ToList();
                arquivosParaProcessar.AddRange(arquivos);
            }
            else
            {
                return mapaCotacoes;
            }

            foreach (var caminhoArquivo in arquivosParaProcessar)
            {
                var linhas = await File.ReadAllLinesAsync(caminhoArquivo);

                for (int i = 1; i < linhas.Length - 1; i++)
                {
                    var linha = linhas[i];

                    if (linha.Length < 121) continue;

                    try
                    {
                        string ticker = linha.Substring(12, 12).Trim();

                        string precoBruto = linha.Substring(108, 13);
                        decimal preco = decimal.Parse(precoBruto, CultureInfo.InvariantCulture) / 100m;

                        if (!string.IsNullOrWhiteSpace(ticker))
                        {
                            mapaCotacoes[ticker] = preco;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return mapaCotacoes;
        }
    }
}