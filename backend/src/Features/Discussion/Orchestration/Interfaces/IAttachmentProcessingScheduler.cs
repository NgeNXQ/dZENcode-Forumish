using System;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

internal interface IAttachmentProcessingScheduler
{
    void Schedule(Guid attachmentId);
}
