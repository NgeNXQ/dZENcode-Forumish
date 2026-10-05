using System.Collections.Generic;

namespace dZENcode.Forumish.Shared.EntityFramework.Domain;

internal interface IEntity
{
    IReadOnlyList<IDeferredEvent> DeferredEvents { get; }

    void ResetDeferredState();
}
