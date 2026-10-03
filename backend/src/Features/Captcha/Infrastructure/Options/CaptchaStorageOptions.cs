namespace dZENcode.Forumish.Features.Captcha.Infrastructure.Options;

internal sealed record class CaptchaStorageOptions(
    string EntryPrefix,
    int EntryExpirationSeconds
)
{
    internal const string Section = "Captcha:Storage";
}
