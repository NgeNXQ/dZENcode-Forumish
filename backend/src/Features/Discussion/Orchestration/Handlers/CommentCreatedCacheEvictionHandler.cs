using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OutputCaching;
using dZENcode.Forumish.Shared.EntityFramework.Application;
using dZENcode.Forumish.Common.Caching.Discussion.Policies;
using dZENcode.Forumish.Features.Discussion.Domain.Events;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Handlers;

internal sealed class CommentCreatedCacheEvictionHandler(
    IOutputCacheStore outputCacheStore
) : IDeferredEventHandler<CommentCreatedDeferredEvent>
{
    public ValueTask Handle(CommentCreatedDeferredEvent deferredEvent, CancellationToken token)
    {
        return outputCacheStore.EvictByTagAsync(DiscussionCachePolicies.CommentsTag, token);
    }
}
