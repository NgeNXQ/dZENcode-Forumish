namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Options;

internal sealed class TextAttachmentProcessingOptions
{
    internal const string Section = "Attachment:Text:Processing";

    public required int MaximumLength { get; init; }
}
