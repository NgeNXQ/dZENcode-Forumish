namespace dZENcode.Forumish.Common.Caching.Discussion.Policies;

internal static class DiscussionCachePolicies
{
    internal const string CommentsTag = "discussion-comments";
    internal const string ReadComments = "discussion-read-comments";

    internal static readonly string[] ReadCommentsQueryKeys =
    [
        "page",
        "size",
        "sortBy",
        "parentId",
        "direction",
    ];
}
