namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Options;

internal sealed class AttachmentStorageOptions
{
    internal const string Section = "Attachment:Storage";

    public required int BufferSize { get; init; }
    public required string PublicPath { get; init; }
    public required string StagingPath { get; init; }
}
