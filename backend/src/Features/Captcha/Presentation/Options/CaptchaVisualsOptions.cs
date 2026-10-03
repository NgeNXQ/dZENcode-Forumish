namespace dZENcode.Forumish.Features.Captcha.Presentation.Options;

internal sealed class CaptchaVisualsOptions
{
    internal const string Section = "Captcha:Visuals";

    public required int MaximumWidth { get; init; }
    public required int MaximumHeight { get; init; }
}
