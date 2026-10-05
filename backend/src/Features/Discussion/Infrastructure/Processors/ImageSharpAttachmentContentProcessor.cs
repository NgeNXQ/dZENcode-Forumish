using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Options;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Interfaces;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Processors;

internal sealed class ImageSharpAttachmentContentProcessor(
    IOptions<ImageAttachmentProcessingOptions> imageAttachmentProcessingOptions,
    IImageEncodingTargetProvider imageEncodingTargetProvider
) : IAttachmentContentProcessor
{
    private readonly ImageAttachmentProcessingOptions processingOptions
        = imageAttachmentProcessingOptions.Value;

    public async Task<ProcessedAttachmentContent?> ProcessAsync(
        Stream source,
        CancellationToken token
    )
    {
        try
        {
            var decoderOptions = new DecoderOptions
            {
                MaxFrames = (uint)processingOptions.MaximumFrames + 1,
                SegmentIntegrityHandling = SegmentIntegrityHandling.Strict,
            };

            var info = await Image.IdentifyAsync(decoderOptions, source, token);

            var target = imageEncodingTargetProvider.GetTarget(info.Metadata.DecodedImageFormat);

            if (target is null)
                return null;

            var frameCount = Math.Max(1, info.FrameMetadataCollection.Count);

            var canvasPixels = (long)info.Width * info.Height;

            if (frameCount > processingOptions.MaximumFrames)
                return null;

            if (canvasPixels <= 0)
                return null;

            var maximumPixelsPerFrame = processingOptions.MaximumSourcePixels / frameCount;

            if (canvasPixels > maximumPixelsPerFrame)
                return null;

            source.Position = 0;

            using var image = await Image.LoadAsync(decoderOptions, source, token);

            if (image.Frames.Count > processingOptions.MaximumFrames)
                return null;

            image.Mutate(static context => context.AutoOrient());

            var exceedsMaximumWidth = image.Width > processingOptions.MaximumWidth;
            var exceedsMaximumHeight = image.Height > processingOptions.MaximumHeight;

            if (exceedsMaximumWidth || exceedsMaximumHeight)
            {
                image.Mutate(context => context.Resize(new ResizeOptions
                {
                    Size = new Size(
                        processingOptions.MaximumWidth,
                        processingOptions.MaximumHeight
                    ),
                    Mode = ResizeMode.Max,
                }));
            }

            image.Metadata.XmpProfile = null;
            image.Metadata.ExifProfile = null;
            image.Metadata.IptcProfile = null;

            using var output = new MemoryStream();

            await image.SaveAsync(output, target.Encoder, token);

            return new()
            {
                Payload = output.ToArray(),
                Extension = target.Extension,
                ContentType = target.ContentType,
            };
        }
        catch (Exception exception) when (
            exception is ImageFormatException or NotSupportedException
        )
        {
            return null;
        }
    }
}
