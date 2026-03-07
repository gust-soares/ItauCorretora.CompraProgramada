namespace ItauCorretora.Application.DTOs;

public record AdesaoClienteRequest(
    string Nome,
    string Cpf,
    string Email,
    decimal ValorMensalAporte);