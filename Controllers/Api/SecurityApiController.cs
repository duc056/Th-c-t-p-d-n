using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBook.Controllers.Api;

[ApiController]
[Route("api/security")]
[Authorize(Roles = "QuanTriVien,NhanVienTiepNhan")]
public class SecurityApiController : ControllerBase
{
    [HttpGet("csrf-token")]
    public IActionResult GetCsrfToken(
        [FromServices] IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new
        {
            requestToken = tokens.RequestToken,
            headerName = "X-CSRF-TOKEN"
        });
    }
}