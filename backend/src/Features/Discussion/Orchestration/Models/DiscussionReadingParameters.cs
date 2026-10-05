using dZENcode.Forumish.Shared.API.Sorting;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Models;

internal sealed record class DiscussionReadingParameters(
    int Page,
    int Size,
    int Offset,
    DiscussionSortingField SortBy,
    SortingDirectionKey Direction
);
