using ItauCorretora.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace ItauCorretora.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessamentoController : ControllerBase
{
    private readonly ProcessarInvestimentoUseCase _investimentoUseCase;
    private readonly ProcessarRebalanceamentoUseCase _rebalanceamentoUseCase; 

    public ProcessamentoController(
        ProcessarInvestimentoUseCase investimentoUseCase,
        ProcessarRebalanceamentoUseCase rebalanceamentoUseCase)
    {
        _investimentoUseCase = investimentoUseCase;
        _rebalanceamentoUseCase = rebalanceamentoUseCase;
    }

    [HttpPost("executar-investimento-mensal")]
    public async Task<IActionResult> Executar(string caminhoArquivoB3)
    {
        try
        {
            if (!System.IO.File.Exists(caminhoArquivoB3))
                return BadRequest(new { erro = "Arquivo da B3 não encontrado no caminho especificado." });

            await _investimentoUseCase.ExecutarAsync(caminhoArquivoB3);
            return Ok(new { mensagem = "Processamento de investimentos concluído com sucesso!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = $"Falha no processamento: {ex.Message}" });
        }
    }

    [HttpPost("executar-rebalanceamento")]
    public async Task<IActionResult> ExecutarRebalanceamento(string caminhoArquivoB3)
    {
        try
        {
            if (!System.IO.File.Exists(caminhoArquivoB3))
                return BadRequest(new { erro = "Arquivo da B3 não encontrado no caminho especificado." });

            await _rebalanceamentoUseCase.ExecutarAsync(caminhoArquivoB3);

            return Ok(new { mensagem = "Rebalanceamento e Apuração de IR executados com sucesso!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = $"Falha no rebalanceamento: {ex.Message}" });
        }
    }
}