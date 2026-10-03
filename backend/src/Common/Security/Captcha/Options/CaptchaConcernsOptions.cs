namespace dZENcode.Forumish.Common.Security.Captcha.Options;

internal sealed class CaptchaConcernsOptions
{
    internal const string Section = "Captcha:Concerns";

    public required string IdHeader { get; init; }
    public required string CodeHeader { get; init; }
}
