using System.Collections.Generic;

namespace dZENcode.Forumish.Shared.API.Pagination;

internal sealed record class SimplePaginationMetadata<T>(
    IReadOnlyList<T> Items,
    int Page,
    int Size
);
