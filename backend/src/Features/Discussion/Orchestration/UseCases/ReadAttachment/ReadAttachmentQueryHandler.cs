using System.Threading;
using System.Threading.Tasks;
using Mediator;
using dZENcode.Forumish.Features.Discussion.Domain.Enums;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.UseCases.ReadAttachment;

internal sealed class ReadAttachmentQueryHandler(
    IAttachmentsRepository repository,
    IAttachmentsPublicStorage storage
) : IQueryHandler<ReadAttachmentQuery, AttachmentReadingModel?>
{
    public async ValueTask<AttachmentReadingModel?> Handle(
        ReadAttachmentQuery query, CancellationToken token
    )
    {
        var attachment = await repository.ReadAttachmentAsync(query.AttachmentId, token);

        if (attachment is null)
            return null;

        if (attachment.Status != AttachmentStatus.Processed)
            return null;

        if (attachment.Filepath is not { } filepath)
            return null;

        var stream = storage.OpenRead(filepath);

        if (stream is null)
            return null;

        return new AttachmentReadingModel(stream, attachment.Filetype);
    }
}
