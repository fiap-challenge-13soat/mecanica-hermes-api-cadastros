using Mecanica.Hermes.Cadastros.Domain.Common.Clock;

namespace Mecanica.Hermes.Cadastros.Domain.Common.Abstractions;

public abstract class BaseDomain
{
    protected BaseDomain()
    {
    }

    protected BaseDomain(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; internal set; }
    public DateTime CreatedAt { get; protected set; } = DomainClock.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }

    protected internal void MarkAsUpdated()
    {
        UpdatedAt = DomainClock.UtcNow;
    }

    protected internal void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DomainClock.UtcNow;
    }

    protected internal void RestaurarBase(Guid id, DateTime createdAt, DateTime? updatedAt, bool isDeleted)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        IsDeleted = isDeleted;
    }
}
