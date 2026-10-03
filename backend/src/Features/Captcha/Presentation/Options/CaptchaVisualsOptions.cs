namespace dZENcode.Forumish.Features.Captcha.Presentation.Options;

internal sealed record class CaptchaVisualsOptions(
    int MaximumWidth,
    int MaximumHeight
)
{
    internal const string Section = "Captcha:Visuals";
}
