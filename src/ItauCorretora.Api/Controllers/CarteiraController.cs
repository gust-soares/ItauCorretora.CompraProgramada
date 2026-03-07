using ItauCorretora.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ItauCorretora.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarteiraController : ControllerBase
{
    private readonly ICarteiraQuery _carteiraQuery;

    public CarteiraController(ICarteiraQuery carteiraQuery)
    {
        _carteiraQuery = carteiraQuery;
    }

    [HttpGet("{clienteId}/posicao")]
    public async Task<IActionResult> ObterPosicao(Guid clienteId)
    {
        var posicao = await _carteiraQuery.ObterPosicaoClienteAsync(clienteId);

        if (posicao == null)
            return NotFound(new { Mensagem = "Cliente não encontrado ou sem posições na carteira." });

        return Ok(posicao);
    }
}