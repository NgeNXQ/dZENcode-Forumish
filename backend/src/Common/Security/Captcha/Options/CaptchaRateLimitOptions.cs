namespace dZENcode.Forumish.Common.Security.Captcha.Options;

internal sealed class CaptchaRateLimitOptions
{
    internal const string Section = "RateLimiting:Captcha";

    public required int PermitLimit { get; init; }
    public required int WindowSeconds { get; init; }
}
