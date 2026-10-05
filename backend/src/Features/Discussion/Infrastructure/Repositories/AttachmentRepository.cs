using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using dZENcode.Forumish.Common.Persistence;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Repositories;

internal sealed class AttachmentsRepository(AppDbContext context) : IAttachmentsRepository
{
    public Task<bool> ExistsAsync(Guid id, CancellationToken token)
    {
        return context.Attachments.AnyAsync(attachment => attachment.Id == id, token);
    }

    public Task<Attachment?> ReadAttachmentAsync(Guid id, CancellationToken token)
    {
        return context.Attachments.SingleOrDefaultAsync(
            attachment => attachment.Id == id,
            token
        );
    }
}
