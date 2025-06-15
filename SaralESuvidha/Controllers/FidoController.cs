using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SaralESuvidha.Controllers;

[ApiController]
[Route("api/webauthn")]
public class FidoController : ControllerBase
{
    private static readonly Dictionary<string, CredentialCreateOptions> _registerOptionsCache = new();
    private static readonly Dictionary<string, AssertionOptions> _authOptionsCache = new();

    private readonly IFido2 _fido2;
    private readonly IConfiguration _config;

    public FidoController(IConfiguration config)
    {
        _config = config;
        _fido2 = new Fido2(new Fido2Configuration
        {
            ServerDomain = config["Fido2:ServerDomain"],
            ServerName = config["Fido2:ServerName"],
            Origins = new HashSet<string> { config["Fido2:Origin"] }
        });
    }

    [HttpPost("register/start")]
    public ActionResult StartRegistration([FromBody] CredentialRequest request)
    {
        var user = new Fido2User
        {
            DisplayName = request.UserName,
            Name = request.UserName,
            Id = Encoding.UTF8.GetBytes(request.UserName)
        };

        var options = _fido2.RequestNewCredential(
            user,
            new List<PublicKeyCredentialDescriptor>(),
            AuthenticatorSelection.Default,
            AttestationConveyancePreference.None,
            null
        );

        _registerOptionsCache[request.UserName] = options;
        return Ok(options);
    }

    [HttpPost("register/verify")]
    public async Task<ActionResult> RegisterVerify([FromBody] RegisterVerifyRequest model)
    {
        if (!_registerOptionsCache.TryGetValue(model.UserName, out var origChallenge))
            return BadRequest(new { success = false, error = "No registration options found" });

        try
        {
            var result = await _fido2.MakeNewCredentialAsync(
                model.Credential,
                origChallenge,
                (args, ct) => Task.FromResult(true)
            );

            if (result.Result is null)
                return BadRequest(new { success = false, error = "Credential generation failed" });

            var credential = new FidoCredential
            {
                UserName = model.UserName,
                CredentialId = Convert.ToBase64String(result.Result.CredentialId),
                PublicKey = result.Result.PublicKey,
                UserHandle = result.Result.User.Id
            };

            await StaticData.SaveFidoCredentialAsync(credential);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("authenticate/start")]
    public async Task<ActionResult> StartLogin([FromBody] CredentialRequest request)
    {
        var cred = await StaticData.GetFidoCredentialByUserNameAsync(request.UserName);
        if (cred is null)
            return Ok(new { error = "NotRegistered" });

        var options = _fido2.GetAssertionOptions(
            new List<PublicKeyCredentialDescriptor>
            {
                new()
                {
                    Id = Convert.FromBase64String(cred.CredentialId),
                    Type = PublicKeyCredentialType.PublicKey
                }
            },
            UserVerificationRequirement.Preferred
        );

        _authOptionsCache[request.UserName] = options;
        return Ok(options);
    }

    [HttpPost("authenticate/verify")]
    public async Task<ActionResult> AuthenticateVerify([FromBody] VerifyRequest model)
    {
        if (!_authOptionsCache.TryGetValue(model.UserName, out var origChallenge))
            return BadRequest(new { success = false, error = "No login options found" });

        var storedCred = await StaticData.GetFidoCredentialByUserNameAsync(model.UserName);
        if (storedCred is null)
            return BadRequest(new { success = false, error = "Credential not found" });

        var result = await _fido2.MakeAssertionAsync(
            model.Credential,
            origChallenge,
            storedCred.PublicKey,
            0,
            (args, ct) =>
            {
                var credentialIdMatch = Convert.ToBase64String(args.CredentialId) == storedCred.CredentialId;
                return Task.FromResult(model.Credential.Response.UserHandle == null || credentialIdMatch);
            },
            storedCred.UserHandle
        );

        if (result.Status != "ok")
            return BadRequest(new { success = false, error = "Authentication failed" });

        var user = StaticData.CashFlowLogin(model.UserName);

        if (user?.Message == "Success: Logedin successfully")
        {
            var (token, expiry) = JWTHelper.GenerateJSONWebToken(user, _config);
            user.Token = token;
            user.Expiry = expiry;
        }

        return Ok(user);
    }
}
