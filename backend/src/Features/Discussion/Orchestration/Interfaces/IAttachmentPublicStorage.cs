using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

internal interface IAttachmentsPublicStorage
{
    Stream? OpenRead(string filename);

    void Delete(string filename);

    Task<string> SaveAsync(
        Guid id,
        string extension,
        byte[] content,
        CancellationToken token
    );
}
