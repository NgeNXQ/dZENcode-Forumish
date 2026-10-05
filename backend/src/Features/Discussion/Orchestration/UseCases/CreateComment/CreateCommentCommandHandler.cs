using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mediator;
using Polly;
using Polly.Registry;
using dZENcode.Forumish.Common.Persistence;
using dZENcode.Forumish.Common.Resilience.EntityFramework;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.UseCases.CreateComment;

internal sealed class CreateCommentCommandHandler(
    AppDbContext appDbContext,
    IAttachmentIntakeService attachmentIntakeService,
    ICommentProcessingService commentProcessingService,
    IUserIdentityResolverService userIdentityResolverService,
    ICommentsRepository commentsRepository,
    IAttachmentsRepository attachmentsRepository,
    IAttachmentProcessingScheduler attachmentProcessingScheduler,
    ResiliencePipelineProvider<EntityFrameworkPipeline> pipelineProvider,
    ILogger<CreateCommentCommandHandler> logger
) : ICommandHandler<CreateCommentCommand, Comment>
{
    private readonly ResiliencePipeline _uniqueConstraintViolationPipeline
        = pipelineProvider.GetPipeline(
            EntityFrameworkPipeline.UniqueConstraintViolation
        );

    public async ValueTask<Comment> Handle(
        CreateCommentCommand command,
        CancellationToken token
    )
    {
        if (command.ParentId is { } parentId)
        {
            if (!await commentsRepository.ExistsAsync(parentId, token))
            {
                throw new InvalidOperationException("Parent comment does not exist.");
            }
        }

        var attachment = await _AcceptAttachmentAsync(command.Attachment, token);
        var comment = await _PersistCommentAsync(command, attachment, token);

        if (attachment is not null)
        {
            try
            {
                attachmentProcessingScheduler.Schedule(attachment.Id);
            }
            catch (Exception exception)
            {
                logger.LogError(exception,
                    "Could not enqueue attachment {AttachmentId}; recovery will retry",
                    attachment.Id
                );
            }
        }

        return comment;
    }

    private async ValueTask<Attachment?> _AcceptAttachmentAsync(
        AttachmentUpload? upload,
        CancellationToken token
    )
    {
        if (upload is null)
            return null;

        return await attachmentIntakeService.AcceptAsync(upload, token);
    }

    private async ValueTask<Comment> _PersistCommentAsync(
        CreateCommentCommand command,
        Attachment? attachment,
        CancellationToken token
    )
    {
        try
        {
            return await _uniqueConstraintViolationPipeline.ExecuteAsync(
                async contextToken =>
                {
                    appDbContext.ChangeTracker.Clear();

                    var attemptAttachment = attachment is null
                        ? null
                        : Attachment.CreatePending(attachment.Id, attachment.Filetype);

                    var user = await userIdentityResolverService.ResolveUserAsync(
                        command.Ip,
                        command.Fingerprint,
                        contextToken
                    );

                    var comment = commentProcessingService.ProcessComment(
                        user,
                        command.ParentId,
                        command.Email,
                        command.Username,
                        command.HomePage,
                        command.Message,
                        attemptAttachment
                    );

                    await appDbContext.SaveChangesAsync(contextToken);

                    return comment;
                },
                token
            );
        }
        catch when (attachment is not null)
        {
            await _DiscardUnpersistedAttachmentAsync(attachment.Id);
            throw;
        }
    }

    private async Task _DiscardUnpersistedAttachmentAsync(Guid attachmentId)
    {
        try
        {
            var persisted = await attachmentsRepository.ExistsAsync(
                attachmentId,
                CancellationToken.None
            );

            if (!persisted)
                attachmentIntakeService.Discard(attachmentId);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Could not confirm or clean up staged attachment {AttachmentId}",
                attachmentId
            );
        }
    }
}
