namespace dZENcode.Forumish.Features.Captcha.Infrastructure.Options;

internal sealed class CaptchaStorageOptions
{
    internal const string Section = "Captcha:Storage";

    public required string EntryPrefix { get; init; }
    public required int EntryExpirationSeconds { get; init; }
}
