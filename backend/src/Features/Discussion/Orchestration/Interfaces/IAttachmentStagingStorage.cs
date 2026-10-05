using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

internal interface IAttachmentsStagingStorage
{
    Task SaveAsync(Guid id, Stream content, long maximumLength, CancellationToken token);

    Stream? OpenRead(Guid id);

    void Delete(Guid id);
}
