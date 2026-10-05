using System;
using System.Threading;
using Hangfire;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Jobs;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Schedulers;

internal sealed class HangfireAttachmentProcessingScheduler(
    IBackgroundJobClient backgroundJobClient
) : IAttachmentProcessingScheduler
{
    public void Schedule(Guid attachmentId)
    {
        backgroundJobClient.Enqueue<AttachmentProcessingJob>(
            job => job.ProcessAsync(attachmentId, CancellationToken.None)
        );
    }
}
