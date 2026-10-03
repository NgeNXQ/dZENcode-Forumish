namespace dZENcode.Forumish.Common.Security.Captcha.Options;

internal sealed record class CaptchaConcernsOptions(
    string IdHeader,
    string CodeHeader
)
{
    internal const string Section = "Captcha:Concerns";
}
