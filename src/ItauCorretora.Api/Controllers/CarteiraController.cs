using ItauCorretora.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace ItauCorretora.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarteiraController : ControllerBase
{
    private readonly ObterPosicaoCarteiraUseCase _obterPosicaoUseCase;

    public CarteiraController(ObterPosicaoCarteiraUseCase obterPosicaoUseCase)
    {
        _obterPosicaoUseCase = obterPosicaoUseCase;
    }

    [HttpGet("{clienteId}/posicao")]
    public async Task<IActionResult> ObterPosicao(Guid clienteId)
    {
        try
        {
            var posicao = await _obterPosicaoUseCase.ExecutarAsync(clienteId);
            return Ok(posicao);
        }
        catch (Exception ex)
        {
            return NotFound(new { erro = ex.Message });
        }
    }
}