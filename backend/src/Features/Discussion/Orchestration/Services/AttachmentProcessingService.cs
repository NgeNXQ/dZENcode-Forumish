using System.Threading;
using System.Threading.Tasks;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Services;

internal sealed class AttachmentProcessingService(
    IAttachmentsPublicStorage attachmentsPublicStorage,
    IAttachmentsStagingStorage attachmentsStagingStorage,
    IAttachmentContentProcessorProvider attachmentContentProcessorProvider
) : IAttachmentProcessingService
{
    public async Task ProcessAsync(Attachment attachment, CancellationToken token)
    {
        await using var source = attachmentsStagingStorage.OpenRead(attachment.Id);

        if (source is null)
        {
            attachment.MarkAsRejected();
            return;
        }

        var processor = attachmentContentProcessorProvider.GetProcessor(attachment.Filetype);

        if (processor is null)
        {
            attachment.MarkAsRejected();
            return;
        }

        var content = await processor.ProcessAsync(source, token);

        if (content is null)
        {
            attachment.MarkAsRejected();
            return;
        }

        var filepath = await attachmentsPublicStorage.SaveAsync(
            attachment.Id,
            content.Extension,
            content.Payload,
            token
        );

        attachment.MarkProcessed(content.ContentType, filepath);
    }
}
