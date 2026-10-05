using System;
using System.Threading;
using System.Threading.Tasks;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

internal interface IAttachmentsRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken token);

    Task<Attachment?> ReadAttachmentAsync(Guid id, CancellationToken token);
}
