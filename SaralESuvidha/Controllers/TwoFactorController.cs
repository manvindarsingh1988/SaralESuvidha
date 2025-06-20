using Microsoft.AspNetCore.Mvc;
using OtpNet;
using System.Web;
using System.Text;
using Microsoft.Extensions.Configuration;
using SaralESuvidha.Services;

namespace SaralESuvidha.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TwoFactorController : ControllerBase
{
    private readonly TwoFactorService _service;

    public TwoFactorController(IConfiguration config)
    {
        _service = new TwoFactorService(config.GetConnectionString("DefaultConnection"));
    }

    [HttpPost("initiate")]
    public IActionResult Initiate([FromQuery] string userId)
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(key);

        _service.SaveSecret(userId, base32Secret);

        string issuer = "SaralCashFlow";
        string userEmail = $"manvinder@saralcashflow.com";
        string uri = $"otpauth://totp/{issuer}:{userEmail}?secret={base32Secret}&issuer={issuer}";

        return Ok(new
        {
            secret = base32Secret,
            qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?data={HttpUtility.UrlEncode(uri)}&size=200x200"
        });
    }

    [HttpPost("verify")]
    public IActionResult Verify([FromQuery] string userId, [FromQuery] string code)
    {
        var (secret, _) = _service.GetSecret(userId);
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        bool valid = totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

        if (valid)
        {
            _service.EnableTwoFactor(userId);
            return Ok(new { success = true });
        }

        return BadRequest(new { success = false, message = "Invalid code" });
    }

    [HttpPost("validate-login")]
    public IActionResult ValidateLoginCode([FromQuery] string userId, [FromQuery] string code)
    {
        var (secret, isEnabled) = _service.GetSecret(userId);
        if (!isEnabled) return Ok(new { success = true }); // skip if not enabled

        var totp = new Totp(Base32Encoding.ToBytes(secret));
        bool valid = totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        return valid
            ? Ok(new { success = true })
            : BadRequest(new { success = false, message = "Invalid 2FA code" });
    }
}