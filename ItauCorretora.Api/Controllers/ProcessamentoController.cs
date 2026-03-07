using ItauCorretora.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace ItauCorretora.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessamentoController : ControllerBase
{
    private readonly ProcessarInvestimentoUseCase _useCase;

    public ProcessamentoController(ProcessarInvestimentoUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpPost("executar-investimento-mensal")]
    public async Task<IActionResult> Executar(string caminhoArquivoB3)
    {
        try
        {
            if (!System.IO.File.Exists(caminhoArquivoB3))
                return BadRequest(new { erro = "Arquivo da B3 não encontrado no caminho especificado." });

            await _useCase.ExecutarAsync(caminhoArquivoB3);

            return Ok(new { mensagem = "Processamento de investimentos concluído com sucesso!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = $"Falha no processamento: {ex.Message}" });
        }
    }
}