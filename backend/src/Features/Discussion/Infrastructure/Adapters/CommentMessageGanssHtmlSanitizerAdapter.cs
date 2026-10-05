using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using AngleSharp.Xhtml;
using Ganss.Xss;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Options;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Adapters;

internal sealed class CommentMessageGanssHtmlSanitizerAdapter : ICommentMessageHtmlSanitizerAdapter
{
    private readonly HtmlSanitizer _sanitizer;

    public CommentMessageGanssHtmlSanitizerAdapter(
        IOptions<CommentMessageHtmlSanitizerOptions> commentMessageHtmlSanitizerOptions
    )
    {
        var messageHtmlSanitizerOptions = commentMessageHtmlSanitizerOptions.Value;

        var options = new HtmlSanitizerOptions
        {
            AllowedTags = new HashSet<string>(
                messageHtmlSanitizerOptions.AllowedTags,
                StringComparer.OrdinalIgnoreCase
            ),
            AllowedAttributes = new HashSet<string>(
                messageHtmlSanitizerOptions.AllowedAttributes,
                StringComparer.OrdinalIgnoreCase
            ),
            UriAttributes = new HashSet<string>(
                messageHtmlSanitizerOptions.UriAttributes,
                StringComparer.OrdinalIgnoreCase
            ),
            AllowedSchemes = new HashSet<string>(
                messageHtmlSanitizerOptions.AllowedSchemes,
                StringComparer.OrdinalIgnoreCase
            )
        };

        _sanitizer = new HtmlSanitizer(options);
    }

    public string Sanitize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return _sanitizer.Sanitize(input, outputFormatter: XhtmlMarkupFormatter.Instance);
    }
}
