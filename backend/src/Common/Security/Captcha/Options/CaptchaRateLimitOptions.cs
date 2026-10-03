namespace dZENcode.Forumish.Common.Security.Captcha.Options;

internal sealed record class CaptchaRateLimitOptions(
    int PermitLimit,
    int WindowSeconds
)
{
    internal const string Section = "RateLimiting:Captcha";
}
