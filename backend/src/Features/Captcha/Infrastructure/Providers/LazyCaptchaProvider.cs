using Microsoft.Extensions.Options;
using Lazy.Captcha.Core.Generator.Code;
using Lazy.Captcha.Core.Generator.Image;
using dZENcode.Forumish.Features.Captcha.Orchestration.Interfaces;
using dZENcode.Forumish.Features.Captcha.Infrastructure.Options;

namespace dZENcode.Forumish.Features.Captcha.Infrastructure.Providers;

internal sealed class LazyCaptchaProvider(
    ICaptchaCodeGenerator captchaCodeGenerator,
    ICaptchaImageGenerator captchaImageGenerator,
    IOptions<CaptchaGeneralOptions> captchaGeneralOptions
) : ICaptchaProvider
{
    private readonly CaptchaGeneralOptions _generalOptions = captchaGeneralOptions.Value;

    public (string code, byte[] payload) Generate(int width, int height)
    {
        (var renderCode, var realCode) = captchaCodeGenerator.Generate(_generalOptions.CodeLength);

        var payload = captchaImageGenerator.Generate(renderCode, new()
        {
            Width = width,
            Height = height
        });

        return (realCode, payload);
    }
}
