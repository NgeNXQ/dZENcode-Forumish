using dZENcode.Forumish.Features.Discussion.Domain.Entities;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

internal interface ICommentProcessingService
{
    Comment ProcessComment(
        User user,
        long? parentId,
        string email,
        string username,
        string? homePage,
        string message,
        Attachment? attachment
    );
}
