namespace dZENcode.Forumish.Features.Discussion.Domain.Options;

internal sealed class AttachmentOptions
{
    internal const string Section = "Attachment:Domain";

    public required int FiletypeMaximumLength { get; init; }
    public required int FilepathMaximumLength { get; init; }
}
