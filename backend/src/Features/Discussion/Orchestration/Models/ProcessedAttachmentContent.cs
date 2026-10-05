namespace dZENcode.Forumish.Features.Discussion.Orchestration.Models;

internal sealed class ProcessedAttachmentContent
{
    internal required byte[] Payload { get; init; }
    internal required string Extension { get; init; }
    internal required string ContentType { get; init; }
}
