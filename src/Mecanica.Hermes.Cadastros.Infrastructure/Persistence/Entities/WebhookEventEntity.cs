namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Entities;

internal sealed class WebhookEventEntity
{
    private WebhookEventEntity()
    {
    }

    public WebhookEventEntity(string scope, string externalId, string status, DateTime receivedAt)
    {
        Id = Guid.NewGuid();
        Scope = scope;
        ExternalId = externalId;
        Status = status;
        ReceivedAt = receivedAt;
    }

    public Guid Id { get; private set; }
    public string Scope { get; private set; } = null!;
    public string ExternalId { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public DateTime ReceivedAt { get; private set; }
}
