using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SaralESuvidha.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SaralESuvidha.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly PushNotificationService _notificationService;
    private readonly IConfiguration _config;

    public NotificationsController(IConfiguration config, PushNotificationService service)
    {
        _notificationService = service;
        _config = config;
    }

    [HttpPost("subscribe")]
    [JwtAuthentication]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscription subscription)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
        var user = JWTHelper.GetCurrentUserDetails(authHeader, _config);

        if (string.IsNullOrEmpty(user?.Id))
        {
            return Unauthorized(new { message = "Invalid user" });
        }

        await _notificationService.SaveSubscriptionAsync(user.Id, subscription);
        return Ok(new { message = "Subscribed successfully" });
    }

    [HttpPost("send-all")]
    [JwtAuthentication]
    public async Task<IActionResult> SendToAll([FromBody] PushPayloadModel payloadModel)
    {
        var payload = JsonSerializer.Serialize(new
        {
            title = payloadModel.Title,
            message = payloadModel.Message
        });

        var subscriptions = await _notificationService.GetAllSubscriptionsAsync();

        foreach (var sub in subscriptions)
        {
            await _notificationService.SendNotificationAsync(sub, payload);
        }

        return Ok(new { message = "Notifications sent to all users." });
    }

    [HttpPost("send/{userId}")]
    [JwtAuthentication]
    public async Task<IActionResult> SendToUser(string userId, [FromBody] PushPayloadModel payloadModel)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { message = "User ID is required." });
        }

        var subscriptions = await _notificationService.GetSubscriptionsByUserIdAsync(userId);

        if (!subscriptions.Any())
        {
            return NotFound(new { message = "No subscriptions found for this user." });
        }

        var payload = JsonSerializer.Serialize(new
        {
            title = payloadModel.Title,
            message = payloadModel.Message
        });

        foreach (var sub in subscriptions)
        {
            await _notificationService.SendNotificationAsync(sub, payload);
        }

        return Ok(new { message = "Notification sent to user." });
    }
}


public class PushNotificationService
{
    private readonly string _connectionString;
    private readonly PushServiceClient _pushClient;

    public PushNotificationService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");

        _pushClient = new PushServiceClient
        {
            DefaultAuthentication = new VapidAuthentication(
                config["VAPID:PublicKey"],
                config["VAPID:PrivateKey"]
            )
        };
    }

    public async Task SaveSubscriptionAsync(string userId, PushSubscription subscription)
    {
        using SqlConnection conn = new(_connectionString);
        using SqlCommand cmd = new("SavePushSubscription", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Endpoint", subscription.Endpoint);
        cmd.Parameters.AddWithValue("@P256dh", subscription.Keys["p256dh"]);
        cmd.Parameters.AddWithValue("@Auth", subscription.Keys["auth"]);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<PushSubscription>> GetAllSubscriptionsAsync()
    {
        return await GetSubscriptionsByUserIdAsync(null);
    }

    public async Task<List<PushSubscription>> GetSubscriptionsByUserIdAsync(string userId)
    {
        var subscriptions = new List<PushSubscription>();

        using SqlConnection conn = new(_connectionString);
        using SqlCommand cmd = new("GetPushSubscriptions", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        if (!string.IsNullOrEmpty(userId))
        {
            cmd.Parameters.AddWithValue("@UserId", userId);
        }

        await conn.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            subscriptions.Add(new PushSubscription
            {
                Endpoint = reader.GetString(0),
                Keys = new Dictionary<string, string>
                {
                    { "p256dh", reader.GetString(1) },
                    { "auth", reader.GetString(2) }
                }
            });
        }

        return subscriptions;
    }

    public async Task SendNotificationAsync(PushSubscription subscription, string payload)
    {
        try
        {
            await _pushClient.RequestPushMessageDeliveryAsync(subscription, new PushMessage(payload));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Push failed: {ex.Message}");
        }
    }
}

