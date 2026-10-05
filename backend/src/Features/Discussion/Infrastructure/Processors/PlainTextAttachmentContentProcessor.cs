using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Options;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Processors;

internal sealed class PlainTextAttachmentContentProcessor(
    IOptions<TextAttachmentProcessingOptions> textAttachmentProcessingOptions
) : IAttachmentContentProcessor
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true
    );

    private readonly TextAttachmentProcessingOptions _processingOptions
        = textAttachmentProcessingOptions.Value;

    public async Task<ProcessedAttachmentContent?> ProcessAsync(
        Stream source,
        CancellationToken token
    )
    {
        if (source.Length == 0)
            return null;

        if (source.Length > _processingOptions.MaximumLength)
            return null;

        using var buffer = new MemoryStream((int)source.Length);

        await source.CopyToAsync(buffer, token);

        var bytes = buffer.ToArray();

        string text;

        try
        {
            text = StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return null;
        }

        if (_ContainsUnsupportedControlCharacters(text))
            return null;

        return new()
        {
            Payload = bytes,
            Extension = ".txt",
            ContentType = "text/plain; charset=utf-8",
        };
    }

    private static bool _ContainsUnsupportedControlCharacters(string text)
    {
        foreach (var symbol in text)
        {
            if (!char.IsControl(symbol))
                continue;

            if (symbol is '\t' or '\r' or '\n')
                continue;

            return true;
        }

        return false;
    }
}
