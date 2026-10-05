using SixLabors.ImageSharp.Formats;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Models;

internal sealed record class ImageEncodingTarget(
    IImageEncoder Encoder,
    string ContentType,
    string Extension
);
