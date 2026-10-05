using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

internal interface ICommentsRepository
{
    void Create(Comment comment);
    Task<bool> ExistsAsync(long id, CancellationToken token);

    Task<IReadOnlyList<CommentReadingModel>> ReadCommentsAsync(
        long? parentId,
        DiscussionReadingParameters reading,
        CancellationToken token
    );
}
