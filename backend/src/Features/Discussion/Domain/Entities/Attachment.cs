using System;
using dZENcode.Forumish.Shared.EntityFramework.Domain;
using dZENcode.Forumish.Features.Discussion.Domain.Enums;

namespace dZENcode.Forumish.Features.Discussion.Domain.Entities;

internal sealed partial class Attachment : Entity, IHasCreationTime, IHasModificationTime
{
    internal Guid Id { get; private set; }
    internal long CommentId { get; private set; }
    internal AttachmentStatus Status { get; private set; }
    internal string Filetype { get; private set; } = null!;
    internal string? Filepath { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; }

    internal Comment Comment { get; private set; } = null!;

    private Attachment()
    {
    }

    internal static Attachment CreatePending(
        Guid id,
        string declaredFiletype
    )
    {
        Guard.ValidatePending(id, declaredFiletype);

        return new()
        {
            Id = id,
            Filetype = declaredFiletype,
            Status = AttachmentStatus.Pending,
        };
    }

    internal void AssignTo(Comment comment)
    {
        Guard.EnsurePending(Id, Status);
        Guard.ValidateComment(Comment, comment);

        Comment = comment;
        CommentId = comment.Id;
    }

    internal void MarkAsRejected()
    {
        Guard.EnsurePending(Id, Status);

        Status = AttachmentStatus.Rejected;
        Filepath = null;
    }

    internal void MarkProcessed(
        string filetype,
        string filepath
    )
    {
        Guard.EnsurePending(Id, Status);
        Guard.ValidateProcessed(filetype, filepath);

        Status = AttachmentStatus.Processed;
        Filetype = filetype;
        Filepath = filepath;
    }

}
