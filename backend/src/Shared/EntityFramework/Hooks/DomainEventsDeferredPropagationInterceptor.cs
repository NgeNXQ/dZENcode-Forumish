using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Mediator;
using dZENcode.Forumish.Shared.EntityFramework.Domain;

namespace dZENcode.Forumish.Shared.EntityFramework.Hooks;

public sealed class DomainEventsDeferredPropagationInterceptor(
    IPublisher publisher,
    ILogger<DomainEventsDeferredPropagationInterceptor> logger
) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not { } context)
            return result;

        var tracker = context.ChangeTracker;
        var events = new List<IDeferredEvent>();

        var wasAutoDetectChangesEnabled = tracker.AutoDetectChangesEnabled;
        tracker.AutoDetectChangesEnabled = false;

        try
        {
            while (true)
            {
                var entries = tracker.Entries<IEntity>()
                    .Where(entity => entity.Entity.DeferredEvents.Count != 0).ToList();

                if (entries.Count == 0)
                    break;

                foreach (var entry in entries)
                {
                    events.Clear();
                    events.AddRange(entry.Entity.DeferredEvents);

                    entry.Entity.ResetDeferredState();

                    foreach (var @event in events)
                    {
                        try
                        {
                            await publisher.Publish(@event, CancellationToken.None);
                        }
                        catch (Exception exception)
                        {
                            logger.LogError(exception,
                                "Failed to publish {EventType} after changes were committed",
                                @event.GetType().Name
                            );
                        }
                    }
                }
            }
        }
        finally
        {
            tracker.AutoDetectChangesEnabled = wasAutoDetectChangesEnabled;
        }

        return result;
    }
}
