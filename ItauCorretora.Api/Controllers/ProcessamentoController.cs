using ItauCorretora.Application.UseCases;
using ItauCorretora.Domain.Entities;
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

    [HttpPost("executar-rebalanceamento")]
    public async Task<IActionResult> ExecutarRebalanceamento(
        [FromServices] ProcessarRebalanceamentoUseCase useCase)
    {
        var novaCesta = new List<RecomendacaoAcao>
        {
            new("YDUQ3T", 0.10m), // Caiu de 20% para 10%
            new("WIZC3T", 0.20m),
            new("WHRL4T", 0.20m),
            new("ITUB4",  0.30m), // Subiu de 20% para 30%
            new("PETR4",  0.20m)  // Nova ação
        };

        string caminhoArquivo = @"C:\Users\gusta\source\repos\ItauCorretora.CompraProgramada\ItauCorretora.Infrastructure\Data\COTAHIST_D05032026.TXT";

        await useCase.ExecutarAsync(caminhoArquivo, novaCesta);

        return Ok(new { Mensagem = "Rebalanceamento e Apuração de IR executados com sucesso!" });
    }
}