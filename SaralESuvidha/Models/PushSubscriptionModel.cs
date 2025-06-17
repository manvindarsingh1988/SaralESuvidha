namespace SaralESuvidha.Models;

public class PushSubscriptionModel
{
    public string Endpoint { get; set; }
    public string P256dh { get; set; }
    public string Auth { get; set; }
}

public class PushPayloadModel
{
    public string Title { get; set; }
    public string Message { get; set; }
}

