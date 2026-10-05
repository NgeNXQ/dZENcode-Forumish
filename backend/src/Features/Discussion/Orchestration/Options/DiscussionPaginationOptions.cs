namespace dZENcode.Forumish.Features.Discussion.Orchestration.Options;

internal sealed class DiscussionPaginationOptions
{
    internal const string Section = "Discussion:Pagination";

    public required int DefaultPageSize { get; init; }
    public required int MaximumPageSize { get; init; }
}
