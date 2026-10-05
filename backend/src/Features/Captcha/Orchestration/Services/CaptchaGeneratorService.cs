using System;
using System.Threading;
using System.Threading.Tasks;
using dZENcode.Forumish.Features.Captcha.Orchestration.Models;
using dZENcode.Forumish.Features.Captcha.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Captcha.Orchestration.Services;

internal sealed class CaptchaGeneratorService(
    ICaptchaProvider captchaProvider,
    ICaptchasRepository captchasRepository
) : ICaptchaGeneratorService
{
    public async Task<CaptchaCreationResult> GenerateCaptchaAsync(
        int width,
        int height,
        CancellationToken token
    )
    {
        var id = Guid.NewGuid();

        (var code, var payload) = captchaProvider.Generate(width, height);

        await captchasRepository.CreateEntryAsync(id, code, token);

        return new()
        {
            Id = id,
            Payload = payload
        };
    }
}
