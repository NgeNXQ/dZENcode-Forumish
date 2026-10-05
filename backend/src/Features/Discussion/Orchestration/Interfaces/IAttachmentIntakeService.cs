using System;
using System.Threading;
using System.Threading.Tasks;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

internal interface IAttachmentIntakeService
{
    Task<Attachment> AcceptAsync(AttachmentUpload upload, CancellationToken token);

    void Discard(Guid id);
}
