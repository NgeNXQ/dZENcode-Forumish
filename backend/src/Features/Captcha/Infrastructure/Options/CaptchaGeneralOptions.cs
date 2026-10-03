namespace dZENcode.Forumish.Features.Captcha.Infrastructure.Options;

internal sealed record class CaptchaGeneralOptions(
    int IdLength,
    int CodeLength
)
{
    internal const string Section = "Captcha:General";
}
