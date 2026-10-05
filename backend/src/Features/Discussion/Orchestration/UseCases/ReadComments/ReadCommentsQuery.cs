using Mediator;
using dZENcode.Forumish.Shared.API.Sorting;
using dZENcode.Forumish.Shared.API.Pagination;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.UseCases.ReadComments;

internal sealed record class ReadCommentsQuery(
    long? ParentId,
    int? Page,
    int? Size,
    SortingDirectionKey? Direction,
    DiscussionSortingField? SortBy
) : IQuery<SimplePaginationMetadata<CommentReadingModel>?>;
