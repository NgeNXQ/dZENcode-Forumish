namespace dZENcode.Forumish.Features.Discussion.Domain.Options;

internal sealed class CommentOptions
{
    internal const string Section = "Comment:Domain";

    public required int EmailMaximumLength { get; init; }
    public required int UsernameMinimumLength { get; init; }
    public required int UsernameMaximumLength { get; init; }
    public required int HomePageMaximumLength { get; init; }
    public required int MessageMaximumLength { get; init; }
}
