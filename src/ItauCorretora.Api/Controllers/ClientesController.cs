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

    [HttpPut("{id}/valor-aporte")]
    public async Task<IActionResult> AlterarValorAporte(
    Guid id,
    [FromBody] decimal novoValor,
    [FromServices] AlterarValorAporteUseCase useCase)
    {
        try
        {
            await useCase.Executar(id, novoValor);
            return Ok(new { Mensagem = "Valor mensal de aporte alterado com sucesso." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Erro = ex.Message });
        }
    }

    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> CancelarAdesao(
        Guid id,
        [FromServices] CancelarAdesaoUseCase useCase)
    {
        try
        {
            await useCase.Executar(id);
            return Ok(new { Mensagem = "Adesão cancelada. O cliente não receberá novas compras mensais, mas a custódia foi mantida." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Erro = ex.Message });
        }
    }
}