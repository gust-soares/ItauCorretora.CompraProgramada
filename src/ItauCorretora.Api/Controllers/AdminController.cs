using Microsoft.AspNetCore.Mvc;
using ItauCorretora.Domain.Interfaces;
using ItauCorretora.Domain.Entities;

namespace ItauCorretora.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ICestaRepository _cestaRepository;

    public AdminController(ICestaRepository cestaRepository)
    {
        _cestaRepository = cestaRepository;
    }

    [HttpPost("cesta")]
    public async Task<IActionResult> CadastrarCesta([FromBody] List<ItemCestaDto> itens)
    {
        var novaCesta = new CestaTopFive();
        foreach (var item in itens)
            novaCesta.AdicionarItem(item.Ticker, item.Percentual);

        if (!novaCesta.ValidarCesta())
            return BadRequest("A cesta deve ter exatamente 5 ativos e a soma dos percentuais deve ser 100%.");

        await _cestaRepository.SalvarAsync(novaCesta);
        return Ok("Cesta Top Five atualizada com sucesso.");
    }

    [HttpGet("cesta/atual")]
    public async Task<IActionResult> ObterAtual()
    {
        var cesta = await _cestaRepository.ObterAtualAsync();
        return cesta != null ? Ok(cesta) : NotFound("Nenhuma cesta cadastrada.");
    }

    [HttpGet("cesta/historico")]
    public async Task<IActionResult> ObterHistorico()
    {
        var historico = await _cestaRepository.ObterHistoricoAsync();
        return Ok(historico);
    }
}

public record ItemCestaDto(string Ticker, decimal Percentual);