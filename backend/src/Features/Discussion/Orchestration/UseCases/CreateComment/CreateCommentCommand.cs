using Mediator;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.UseCases.CreateComment;

internal record class CreateCommentCommand(
    long? ParentId,
    string Email,
    string Username,
    string? HomePage,
    string Message,
    AttachmentUpload? Attachment,
    string Ip,
    string Fingerprint
) : ICommand<Comment>;
