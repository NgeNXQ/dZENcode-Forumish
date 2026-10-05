using System;
using dZENcode.Forumish.Shared.EntityFramework.Domain;
using dZENcode.Forumish.Features.Discussion.Domain.Events;

namespace dZENcode.Forumish.Features.Discussion.Domain.Entities;

internal sealed partial class Comment : Entity, IHasCreationTime
{
    internal long Id { get; private set; }
    internal long AuthorId { get; private set; }
    internal long? ParentId { get; private set; }
    internal Guid? AttachmentId => Attachment?.Id;
    internal string Email { get; private set; } = null!;
    internal string Username { get; private set; } = null!;
    internal string? HomePage { get; private set; }
    internal string Message { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; }

    internal User Author { get; private set; } = null!;
    internal Comment? Parent { get; private set; }
    internal Attachment? Attachment { get; private set; }

    private Comment()
    {
    }

    internal static Comment Create(
        User author,
        long? parentId,
        string email,
        string username,
        string? homePage,
        string message,
        Attachment? attachment
    )
    {
        Guard.Validate(author, parentId, email, username, homePage, message);

        email = email.Trim();
        message = message.Trim();
        username = username.Trim();
        homePage = string.IsNullOrWhiteSpace(homePage) ? null : homePage.Trim();

        var comment = new Comment
        {
            Email = email,
            Author = author,
            AuthorId = author.Id,
            ParentId = parentId,
            Username = username,
            HomePage = homePage,
            Message = message,
            Attachment = attachment,
        };

        attachment?.AssignTo(comment);
        author.Comments.Add(comment);
        comment.Fire(new CommentCreatedDeferredEvent());

        return comment;
    }
}
