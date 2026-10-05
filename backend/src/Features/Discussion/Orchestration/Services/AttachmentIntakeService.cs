using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using dZENcode.Forumish.Shared.Http;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;
using dZENcode.Forumish.Features.Discussion.Orchestration.Options;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Services;

internal sealed class AttachmentIntakeService(
    IAttachmentsStagingStorage stagingStorage,
    IOptions<CommentAttachmentOptions> attachmentOptions,
    ILogger<AttachmentIntakeService> logger
) : IAttachmentIntakeService
{
    public async Task<Attachment> AcceptAsync(
        AttachmentUpload upload,
        CancellationToken token
    )
    {
        var id = Guid.CreateVersion7();
        var mime = MimeType.Normalize(upload.ContentType);

        if (mime is null)
            throw new ArgumentException("Attachment type is not supported.", nameof(upload));

        var maximumLength = _GetMaximumLength(mime);
        var attachment = Attachment.CreatePending(id, mime);

        try
        {
            await stagingStorage.SaveAsync(id, upload.Content, maximumLength, token);
        }
        catch
        {
            try
            {
                stagingStorage.Delete(id);
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Could not delete staged attachment {AttachmentId}",
                    id
                );
            }

            throw;
        }

        return attachment;
    }

    public void Discard(Guid id)
    {
        stagingStorage.Delete(id);
    }

    private long _GetMaximumLength(string mime)
    {
        foreach (var pair in attachmentOptions.Value.Mime)
        {
            if (!string.Equals(pair.Key, mime, StringComparison.OrdinalIgnoreCase))
                continue;

            if (pair.Value <= 0)
                throw new InvalidOperationException("Attachment size limit must be positive.");

            return pair.Value;
        }

        throw new ArgumentException("Attachment type is not supported.", nameof(mime));
    }
}
