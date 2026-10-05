using System;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Formats;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Models;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Providers;

internal sealed class ImageEncodingTargetProvider(IServiceProvider services)
    : IImageEncodingTargetProvider
{
    public ImageEncodingTarget? GetTarget(IImageFormat? format)
    {
        if (format is null)
            return null;

        return services.GetKeyedService<ImageEncodingTarget>(format.Name);
    }
}
