using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Mediator;
using dZENcode.Forumish.Features.Discussion.Orchestration.UseCases.ProcessAttachment;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Jobs;

internal sealed class AttachmentProcessingJob(ISender sender)
{
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ProcessAsync(Guid attachmentId, CancellationToken token)
    {
        await sender.Send(new ProcessAttachmentCommand(attachmentId), token);
    }
}
