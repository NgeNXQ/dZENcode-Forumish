using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using dZENcode.Forumish.Shared.EntityFramework.Domain;

namespace dZENcode.Forumish.Shared.EntityFramework.Hooks;

public sealed class AuditableTimestampsPopulationInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        _ApplyTimestamps(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        _ApplyTimestamps(eventData.Context);
        return new(result);
    }

    private static void _ApplyTimestamps(DbContext? context)
    {
        var time = DateTimeOffset.UtcNow;

        if (context is null)
            return;

        var entries = context.ChangeTracker.Entries<IEntity>().ToList();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    _ApplyCreationTime(entry, time);
                    _ApplyModificationTime(entry, time);
                    break;
                case EntityState.Modified:
                    _ApplyModificationTime(entry, time);
                    break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void _ApplyCreationTime(EntityEntry entry, DateTimeOffset value)
    {
        if (entry.Entity is IHasCreationTime)
            entry.Property(nameof(IHasCreationTime.CreatedAt)).CurrentValue = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void _ApplyModificationTime(EntityEntry entry, DateTimeOffset value)
    {
        if (entry.Entity is IHasModificationTime)
            entry.Property(nameof(IHasModificationTime.UpdatedAt)).CurrentValue = value;
    }
}
