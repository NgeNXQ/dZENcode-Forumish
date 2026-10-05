namespace dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

internal interface IAttachmentContentProcessorProvider
{
    IAttachmentContentProcessor? GetProcessor(string fileType);
}
