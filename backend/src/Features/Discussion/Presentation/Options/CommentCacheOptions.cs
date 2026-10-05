namespace dZENcode.Forumish.Features.Discussion.Presentation.Options;

internal sealed class CommentCacheOptions
{
    internal const string Section = "Discussion:Comments:Cache";

    public required int ExpirationSeconds { get; init; }
}
