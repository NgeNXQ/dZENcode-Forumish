using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;
using dZENcode.Forumish.Common.Persistence;
using dZENcode.Forumish.Common.Resilience.EntityFramework;
using dZENcode.Forumish.Features.Discussion.Domain.Enums;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.UseCases.ProcessAttachment;

internal sealed class ProcessAttachmentCommandHandler(
    AppDbContext appDbContext,
    IAttachmentsRepository attachmentsRepository,
    IAttachmentsStagingStorage attachmentsStagingStorage,
    IAttachmentsPublicStorage attachmentsPublicStorage,
    IAttachmentProcessingService attachmentProcessingService,
    ResiliencePipelineProvider<EntityFrameworkPipeline> pipelineProvider
) : ICommandHandler<ProcessAttachmentCommand, AttachmentStatus?>
{
    private readonly ResiliencePipeline _concurrencyPipeline = pipelineProvider.GetPipeline(
        EntityFrameworkPipeline.OptimisticConcurrency
    );

    public async ValueTask<AttachmentStatus?> Handle(
        ProcessAttachmentCommand command,
        CancellationToken token
    )
    {
        return await _concurrencyPipeline.ExecuteAsync<AttachmentStatus?>(
            async contextToken =>
            {
                appDbContext.ChangeTracker.Clear();

                var attachment = await attachmentsRepository.ReadAttachmentAsync(
                    command.AttachmentId,
                    contextToken
                );

                if (attachment is null)
                {
                    attachmentsStagingStorage.Delete(command.AttachmentId);
                    return null;
                }

                if (attachment.Status == AttachmentStatus.Pending)
                {
                    await attachmentProcessingService.ProcessAsync(attachment, contextToken);

                    try
                    {
                        await appDbContext.SaveChangesAsync(contextToken);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (attachment.Filepath is { } unusedPath)
                            attachmentsPublicStorage.Delete(unusedPath);
                        throw;
                    }
                }

                attachmentsStagingStorage.Delete(attachment.Id);

                return attachment.Status;
            },
            token
        );
    }
}
