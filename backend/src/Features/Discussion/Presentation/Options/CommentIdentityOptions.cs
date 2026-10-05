namespace dZENcode.Forumish.Features.Discussion.Presentation.Options;

internal sealed class CommentIdentityOptions
{
    internal const string Section = "Comment:Identity";

    public required int FingerprintMaximumLength { get; init; }
}
