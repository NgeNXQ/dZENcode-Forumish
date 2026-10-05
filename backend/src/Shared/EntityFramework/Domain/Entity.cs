using System;
using System.Collections.Generic;

namespace dZENcode.Forumish.Shared.EntityFramework.Domain;

public abstract class Entity : IEntity
{
    private readonly List<IDeferredEvent> _deferredEvents = new();

    IReadOnlyList<IDeferredEvent> IEntity.DeferredEvents => _deferredEvents;

    protected void Fire(IDeferredEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _deferredEvents.Add(domainEvent);
    }

    void IEntity.ResetDeferredState()
    {
        _deferredEvents.Clear();
    }
}
