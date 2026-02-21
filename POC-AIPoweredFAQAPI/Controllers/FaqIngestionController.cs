using Microsoft.AspNetCore.Mvc;
using POC_AIPoweredFAQAPI.Interfaces;
using POC_AIPoweredFAQAPI.Models;

namespace POC.AIPoweredFAQAPI.Controllers;

[ApiController]
[Route("api/faq")]
public class FaqIngestionController : ControllerBase
{
    private readonly IFaqIngestionService _ingestionService;

    public FaqIngestionController(IFaqIngestionService ingestionService)
    {
        _ingestionService = ingestionService;
    }

    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest([FromBody] FaqIngestRequest request, CancellationToken cancellationToken)
    {
        await _ingestionService.IngestAsync(request, cancellationToken);
        return Accepted();
    }
}
