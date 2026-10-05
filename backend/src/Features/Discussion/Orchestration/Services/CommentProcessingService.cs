using dZENcode.Forumish.Features.Discussion.Domain.Entities;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Services;

internal sealed class CommentProcessingService(
    ICommentsRepository commentsRepository,
    ICommentMessageHtmlSanitizerAdapter commentMessageHtmlSanitizerAdapter
) : ICommentProcessingService
{
    public Comment ProcessComment(
        User user,
        long? parentId,
        string email,
        string username,
        string? homePage,
        string message,
        Attachment? attachment
    )
    {
        var sanitizedMessage = commentMessageHtmlSanitizerAdapter.Sanitize(message);

        var comment = Comment.Create(
            user,
            parentId,
            email,
            username,
            homePage,
            sanitizedMessage,
            attachment
        );

        commentsRepository.Create(comment);

        return comment;
    }

}
