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
using SaralESuvidha.ViewModel;
using DocumentFormat.OpenXml.Spreadsheet;
using static Org.BouncyCastle.Math.EC.ECCurve;


namespace SaralESuvidha.Controllers;

[ApiController]
[Route("api/webauthn")]
public class FidoController : ControllerBase
{
    private static readonly Dictionary<string, CredentialCreateOptions> _registerOptionsCache = new();
    private static readonly Dictionary<string, AssertionOptions> _authOptionsCache = new();
    private static readonly Dictionary<string, FidoStoredCredential> _credentials = new();

    private readonly IFido2 _fido2;
    private readonly IConfiguration _config;

    public FidoController(IConfiguration config)
    {
        _config = config;
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
            var user = StaticData.CashFlowLogin(model.UserName);

            if (user != null && user.Message == "Success: Logedin successfully")
            {
                var tokenDetails = JWTHelper.GenerateJSONWebToken(user, _config);
                user.Token = tokenDetails.Item1;
                user.Expiry = tokenDetails.Item2;
            }
            return Ok(user);
        }

        return BadRequest(new { success = false });
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