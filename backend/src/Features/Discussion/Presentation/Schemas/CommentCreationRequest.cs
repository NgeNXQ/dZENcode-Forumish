using Microsoft.AspNetCore.Http;

namespace dZENcode.Forumish.Features.Discussion.Presentation.Schemas;

internal sealed class CommentCreationRequest
{
    public long? ParentId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string? HomePage { get; init; }
    public string Message { get; init; } = string.Empty;
    public string Fingerprint { get; init; } = string.Empty;
    public IFormFile? Attachment { get; init; }
}
