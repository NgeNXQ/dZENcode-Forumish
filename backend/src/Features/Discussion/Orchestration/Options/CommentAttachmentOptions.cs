using System.Collections.Generic;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Options;

internal sealed class CommentAttachmentOptions
{
    internal const string Section = "Attachment";

    public required IReadOnlyDictionary<string, long> Mime { get; init; }
}
