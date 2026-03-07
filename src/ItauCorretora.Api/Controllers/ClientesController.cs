using ItauCorretora.Application.DTOs;
using ItauCorretora.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace ItauCorretora.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly AderirProdutoUseCase _useCase;

    public ClientesController(AderirProdutoUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpPost("adesao")]
    public async Task<IActionResult> AderirAoProduto([FromBody] AdesaoClienteRequest request)
    {
        try
        {
            var cliente = await _useCase.ExecutarAsync(request);

            return Created(string.Empty, new
            {
                cliente.Id,
                cliente.Nome,
                cliente.Cpf,
                cliente.ValorMensalAporte,
                cliente.Ativo,
                cliente.DataAdesao
            });
        }
        catch (ArgumentException ex) 
        {
            return BadRequest(new { erro = ex.Message });
        }
        catch (InvalidOperationException ex) 
        {
            return BadRequest(new { erro = ex.Message });
        }
    }
}