using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using dZENcode.Forumish.Common.Persistence;
using dZENcode.Forumish.Features.Discussion.Domain.Enums;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Options;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Jobs;

internal sealed class AttachmentRecoveryService(
    IServiceScopeFactory scopeFactory,
    IOptions<AttachmentRecoveryOptions> recoveryOptions,
    ILogger<AttachmentRecoveryService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = recoveryOptions.Value;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.IntervalSeconds));

        do
        {
            try
            {
                await _RecoverAsync(options, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Attachment recovery failed; the next pass will retry");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task _RecoverAsync(AttachmentRecoveryOptions options, CancellationToken token)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var scheduler = scope.ServiceProvider.GetRequiredService<IAttachmentProcessingScheduler>();
        var cutoff = DateTimeOffset.UtcNow.AddSeconds(-options.PendingAgeSeconds);

        var attachments = context.Attachments.AsNoTracking()
            .Where(item => item.Status == AttachmentStatus.Pending && item.CreatedAt <= cutoff)
            .Select(item => item.Id).AsAsyncEnumerable();

        await foreach (var id in attachments.WithCancellation(token))
        {
            try
            {
                scheduler.Schedule(id);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Could not enqueue pending attachment {AttachmentId}", id);
            }
        }
    }
}
