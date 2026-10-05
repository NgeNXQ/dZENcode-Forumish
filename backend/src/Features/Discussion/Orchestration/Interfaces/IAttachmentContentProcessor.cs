using System.IO;
using System.Threading;
using System.Threading.Tasks;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

internal interface IAttachmentContentProcessor
{
    Task<ProcessedAttachmentContent?> ProcessAsync(
        Stream source,
        CancellationToken token
    );
}
