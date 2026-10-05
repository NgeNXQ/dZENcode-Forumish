namespace dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

internal interface ICommentMessageHtmlSanitizerAdapter
{
    string Sanitize(string input);
}
