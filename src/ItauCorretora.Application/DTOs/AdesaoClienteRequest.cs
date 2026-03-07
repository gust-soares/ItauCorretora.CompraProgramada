using System.Diagnostics.CodeAnalysis;

namespace ItauCorretora.Application.DTOs;

[ExcludeFromCodeCoverage]
public record AdesaoClienteRequest(
    string Nome,
    string Cpf,
    string Email,
    decimal ValorMensalAporte);