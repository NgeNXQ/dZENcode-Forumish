using dZENcode.Forumish.Shared.API.Sorting;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;

namespace dZENcode.Forumish.Features.Discussion.Presentation.Schemas;

internal sealed record class CommentsReadingRequest(
    long? ParentId,
    int? Page,
    int? Size,
    DiscussionSortingField? SortBy,
    SortingDirectionKey? Direction
);
