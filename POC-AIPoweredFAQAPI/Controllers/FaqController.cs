using Microsoft.AspNetCore.Mvc;
using POC_AIPoweredFAQAPI.Interfaces;
using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.Controllers;

[ApiController]
[Route("api/faq")]
public class FaqController : ControllerBase
{
    private readonly IFaqService _faqService;

    public FaqController(IFaqService faqService)
    {
        _faqService = faqService;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<FaqAskResponse>> Ask([FromBody] FaqAskRequest req, CancellationToken cancellationToken)
    {
        var resp = await _faqService.AskAsync(req, cancellationToken);
        return Ok(resp);
    }
}
