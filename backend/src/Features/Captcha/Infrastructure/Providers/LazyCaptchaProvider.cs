using Microsoft.Extensions.Options;
using Lazy.Captcha.Core;
using Lazy.Captcha.Core.Generator.Code;
using Lazy.Captcha.Core.Generator.Image;
using dZENcode.Forumish.Features.Captcha.Infrastructure.Options;
using dZENcode.Forumish.Features.Captcha.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Captcha.Infrastructure.Providers;

internal sealed class LazyCaptchaProvider : ICaptchaProvider
{
    private readonly CaptchaGeneralOptions _generalOptions;
    private readonly ICaptchaCodeGenerator _captchaCodeGenerator;
    private readonly ICaptchaImageGenerator _captchaImageGenerator;

    public LazyCaptchaProvider(IOptions<CaptchaGeneralOptions> captchaGeneralOptions)
    {
        _generalOptions = captchaGeneralOptions.Value;
        _captchaCodeGenerator = new DefaultCaptchaCodeGenerator();
        _captchaImageGenerator = new DefaultCaptchaImageGenerator();
    }

    public (string code, byte[] payload) Generate(int width, int height)
    {
        (var renderCode, var realCode) = _captchaCodeGenerator.Generate(_generalOptions.CodeLength);

        var payload = _captchaImageGenerator.Generate(renderCode, new()
        {
            Width = width,
            Height = height,
            ForegroundColors = DefaultColors.Colors
        });

        return (realCode, payload);
    }
}
