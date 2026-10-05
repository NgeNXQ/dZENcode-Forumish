using SixLabors.ImageSharp.Formats;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Models;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Interfaces;

internal interface IImageEncodingTargetProvider
{
    ImageEncodingTarget? GetTarget(IImageFormat? format);
}
