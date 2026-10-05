namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Options;

internal sealed class AttachmentRecoveryOptions
{
    internal const string Section = "Attachment:Processing:Recovery";

    public int IntervalSeconds { get; init; } = 60;
    public int PendingAgeSeconds { get; init; } = 120;
}
