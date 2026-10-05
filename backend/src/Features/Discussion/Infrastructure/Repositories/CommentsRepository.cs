using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using dZENcode.Forumish.Shared.API.Sorting;
using dZENcode.Forumish.Common.Persistence;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Repositories;

internal sealed class CommentsRepository(AppDbContext context) : ICommentsRepository
{
    public void Create(Comment comment)
    {
        context.Add(comment);
    }

    public Task<bool> ExistsAsync(long id, CancellationToken token)
    {
        return context.Comments.AnyAsync(comment => comment.Id == id, token);
    }

    public async Task<IReadOnlyList<CommentReadingModel>> ReadCommentsAsync(
        long? parentId,
        DiscussionReadingParameters reading,
        CancellationToken token
    )
    {
        var query = context.Comments
            .AsNoTracking()
            .Where(comment => comment.ParentId == parentId);

        return await _ApplySorting(query, reading.SortBy, reading.Direction)
            .Skip(reading.Offset)
            .Take(reading.Size)
            .Select(comment => new CommentReadingModel(
                comment.Id,
                comment.ParentId,
                comment.Email,
                comment.Username,
                comment.HomePage,
                comment.Message,
                comment.Attachment == null ? null : comment.Attachment.Id,
                comment.CreatedAt
            ))
            .ToListAsync(token);
    }

    private static IQueryable<Comment> _ApplySorting(
        IQueryable<Comment> query,
        DiscussionSortingField sortBy,
        SortingDirectionKey direction
    )
    {
        return sortBy switch
        {
            DiscussionSortingField.Email => direction == SortingDirectionKey.Ascending
                ? query.OrderBy(c => c.Email).ThenBy(c => c.Id)
                : query.OrderByDescending(c => c.Email).ThenByDescending(c => c.Id),

            DiscussionSortingField.Username => direction == SortingDirectionKey.Ascending
                ? query.OrderBy(c => c.Username).ThenBy(c => c.Id)
                : query.OrderByDescending(c => c.Username).ThenByDescending(c => c.Id),

            DiscussionSortingField.CreatedAt => direction == SortingDirectionKey.Ascending
                ? query.OrderBy(c => c.CreatedAt).ThenBy(c => c.Id)
                : query.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Id),

            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }
}
