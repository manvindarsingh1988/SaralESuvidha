using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;


namespace SaralESuvidha.Controllers;

[ApiController]
[Route("api/webauthn")]
public class FidoController : ControllerBase
{
    private static readonly Dictionary<string, CredentialCreateOptions> _registerOptionsCache = new();
    private static readonly Dictionary<string, AssertionOptions> _authOptionsCache = new();
    private static readonly Dictionary<string, FidoStoredCredential> _credentials = new();

    private readonly IFido2 _fido2;

    public FidoController(IConfiguration config)
    {
        _fido2 = new Fido2(new Fido2Configuration
        {
            ServerDomain = config["Fido2:ServerDomain"],
            ServerName = config["Fido2:ServerName"],
            Origins = new HashSet<string>() { config["Fido2:Origin"] }
        });
    }

    [HttpPost("register/start")]
    public ActionResult StartRegistration([FromBody] CredentialRequest request)
    {
        var user = new Fido2User
        {
            DisplayName = request.UserName,
            Name = request.UserName,
            Id = System.Text.Encoding.UTF8.GetBytes(request.UserName)
        };

        var options = _fido2.RequestNewCredential(user, new List<PublicKeyCredentialDescriptor>(), AuthenticatorSelection.Default, AttestationConveyancePreference.None, null);
        _registerOptionsCache[request.UserName] = options;
        return Ok(options);
    }

    [HttpPost("authenticate/start")]
    public ActionResult StartLogin([FromBody] CredentialRequest request)
    {
        if (!_credentials.ContainsKey(request.UserName))
            return Ok(new { error = "NotRegistered" });

        var cred = _credentials[request.UserName];
        var options = _fido2.GetAssertionOptions(new List<PublicKeyCredentialDescriptor>
    {
        new() { Id = Convert.FromBase64String(cred.CredentialId), Type = PublicKeyCredentialType.PublicKey }
    }, UserVerificationRequirement.Preferred);

        _authOptionsCache[request.UserName] = options;
        return Ok(options);
    }

    [HttpPost("register/verify")]
    public async Task<ActionResult> RegisterVerify([FromBody] RegisterVerifyRequest model)
    {
        try
        {
            if (!_registerOptionsCache.TryGetValue(model.UserName, out var origChallenge))
                return BadRequest(new { success = false, error = "No registration options found" });

            var result = await _fido2.MakeNewCredentialAsync(model.Credential, origChallenge, (args, ct) =>
            {
                // Accept all registrations (you can implement checks here)
                return Task.FromResult(true);
            });

            if (result.Result != null)
            {
                _credentials[model.UserName] = new FidoStoredCredential
                {
                    CredentialId = Convert.ToBase64String(result.Result.CredentialId),
                    PublicKey = result.Result.PublicKey,
                    UserHandle = result.Result.User.Id
                };

                return Ok(new { success = true });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }

        return BadRequest(new { success = false });
    }

    [HttpPost("authenticate/verify")]
    public async Task<ActionResult> AuthenticateVerify([FromBody] VerifyRequest model)
    {
        if (!_authOptionsCache.TryGetValue(model.UserName, out var origChallenge))
            return BadRequest(new { success = false, error = "No login options found" });

        if (!_credentials.TryGetValue(model.UserName, out var storedCred))
            return BadRequest(new { success = false, error = "Credential not found" });

        var result = await _fido2.MakeAssertionAsync(
            model.Credential,
            origChallenge,
            storedCred.PublicKey,
            0, // counter (use stored counter if available)
            (args, ct) =>
            {
                // Validate user handle belongs to credential ID
                return Task.FromResult(
                    model.Credential.Response.UserHandle == null ||
                    Convert.ToBase64String(args.CredentialId) == storedCred.CredentialId
                );
            },
            storedCred.UserHandle // optional user handle
        );

        if (result.Status == "ok")
        {
            // TODO: replace with actual token generation logic (JWT)
            var token = GenerateDummyJwt(model.UserName); // Replace with your real JWT generation

            return Ok(new
            {
                success = true,
                token,
                userName = model.UserName
            });
        }

        return BadRequest(new { success = false });
    }

    private string GenerateDummyJwt(string userName)
    {
        // ⚠️ Replace this with real JWT logic (using JWT package or identity framework)
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"token-for-{userName}"));
    }
}

public class JwtHelper
{
    private readonly IConfiguration _config;

    public JwtHelper(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string userName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:ExpiryMinutes"])),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}


public class RegisterVerifyRequest
{
    public AuthenticatorAttestationRawResponse Credential { get; set; }
    public string UserName { get; set; }
}


public class CredentialRequest
{
    public string UserName { get; set; }
}

public class VerifyRequest
{
    public AuthenticatorAssertionRawResponse Credential { get; set; }
    public string UserName { get; set; }
}

public class FidoStoredCredential
{
    public string CredentialId { get; set; }
    public byte[] PublicKey { get; set; }
    public byte[] UserHandle { get; set; }
}