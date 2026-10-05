using System;

namespace dZENcode.Forumish.Features.Discussion.Presentation.Schemas;

internal sealed record class CommentReadingResponse(
    long Id,
    long? ParentId,
    string Email,
    string Username,
    string? HomePage,
    string Message,
    Guid? AttachmentId,
    DateTimeOffset CreatedAt
);
