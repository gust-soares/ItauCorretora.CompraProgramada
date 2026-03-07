using ItauCorretora.Application.DTOs;

namespace ItauCorretora.Application.Queries;

public interface ICarteiraQuery
{
    Task<PosicaoCarteiraDto?> ObterPosicaoClienteAsync(Guid clienteId);
}