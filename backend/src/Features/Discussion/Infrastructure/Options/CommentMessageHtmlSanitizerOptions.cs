namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Options;

internal sealed class CommentMessageHtmlSanitizerOptions
{
    internal const string Section = "Comment:MessageSanitizer";

    public required string[] AllowedTags { get; init; }
    public required string[] UriAttributes { get; init; }
    public required string[] AllowedSchemes { get; init; }
    public required string[] AllowedAttributes { get; init; }
}
