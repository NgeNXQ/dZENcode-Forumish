using System;
using Microsoft.Extensions.DependencyInjection;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Providers;

internal sealed class AttachmentContentProcessorProvider(
    IServiceProvider services
) : IAttachmentContentProcessorProvider
{
    public IAttachmentContentProcessor? GetProcessor(string fileType)
    {
        return services.GetKeyedService<IAttachmentContentProcessor>(
            fileType.Trim().ToLowerInvariant()
        );
    }
}
