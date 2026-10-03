namespace dZENcode.Forumish.Features.Captcha.Infrastructure.Options;

internal sealed class CaptchaGeneralOptions
{
    internal const string Section = "Captcha:General";

    public required int IdLength { get; init; }
    public required int CodeLength { get; init; }
}
