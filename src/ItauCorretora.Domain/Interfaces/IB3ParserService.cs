namespace ItauCorretora.Domain.Interfaces;

public interface IB3ParserService
{
    Task<Dictionary<string, decimal>> ParseCotacoesAsync(string caminhoArquivo);
}