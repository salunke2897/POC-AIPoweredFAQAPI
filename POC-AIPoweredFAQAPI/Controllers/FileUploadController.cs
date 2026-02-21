using Microsoft.AspNetCore.Mvc;

namespace POC_AIPoweredFAQAPI.Controllers;

[ApiController]
[Route("api/FileUpload")]
public class FileUploadController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) return BadRequest("No file");
        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) return BadRequest("Only PDF allowed");

        var dir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, Path.GetFileName(file.FileName));
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream, cancellationToken);
        return Ok();
    }
}
