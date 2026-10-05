using System;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Models;

internal sealed record class CommentReadingModel(
    long Id,
    long? ParentId,
    string Email,
    string Username,
    string? HomePage,
    string Message,
    Guid? AttachmentId,
    DateTimeOffset CreatedAt
);
