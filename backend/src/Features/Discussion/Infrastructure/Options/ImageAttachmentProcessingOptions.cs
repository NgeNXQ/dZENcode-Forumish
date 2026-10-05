namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Options;

internal sealed class ImageAttachmentProcessingOptions
{
    internal const string Section = "Attachment:Image:Processing";

    public required int JpegQuality { get; init; }
    public required int MaximumWidth { get; init; }
    public required int MaximumHeight { get; init; }
    public required int MaximumFrames { get; init; }
    public required long MaximumSourcePixels { get; init; }
}
