using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Mediator;
using dZENcode.Forumish.Shared.API.Sorting;
using dZENcode.Forumish.Shared.API.Pagination;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;
using dZENcode.Forumish.Features.Discussion.Orchestration.Options;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.UseCases.ReadComments;

internal sealed class ReadCommentsQueryHandler(
    ICommentsRepository repository,
    IOptions<DiscussionPaginationOptions> paginationOptions
) : IQueryHandler<ReadCommentsQuery, SimplePaginationMetadata<CommentReadingModel>?>
{
    public async ValueTask<SimplePaginationMetadata<CommentReadingModel>?> Handle(
        ReadCommentsQuery query,
        CancellationToken token
    )
    {
        if (query.ParentId is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query.ParentId), query.ParentId, "ParentId must be greater than zero."
            );
        }

        if (query.ParentId is { } parentId)
        {
            var parentExists = await repository.ExistsAsync(parentId, token);

            if (!parentExists)
                return null;
        }

        var page = query.Page ?? 1;
        var size = query.Size ?? paginationOptions.Value.DefaultPageSize;

        var reading = new DiscussionReadingParameters(
            page,
            size,
            (page - 1) * size,
            query.SortBy ?? DiscussionSortingField.CreatedAt,
            query.Direction ?? (
                query.ParentId is null ?
                SortingDirectionKey.Descending : SortingDirectionKey.Ascending
            )
        );

        var items = await repository.ReadCommentsAsync(
            query.ParentId,
            reading,
            token
        );

        return new SimplePaginationMetadata<CommentReadingModel>(
            items,
            reading.Page,
            reading.Size
        );
    }
}
