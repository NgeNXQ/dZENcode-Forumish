using System;
using System.Threading;
using System.Threading.Tasks;
using dZENcode.Forumish.Features.Captcha.Orchestration.Models;
using dZENcode.Forumish.Features.Captcha.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Captcha.Orchestration.Services;

internal sealed class CaptchaVerificationService(
    ICaptchasRepository captchasRepository
) : ICaptchaVerificationService
{
    public async Task<CaptchaVerificationResult> VerifyCaptchaAsync(
        Guid id,
        string code,
        CancellationToken token
    )
    {
        if (string.IsNullOrWhiteSpace(code))
            return new() { IsSuccess = false };

        var storedCode = await captchasRepository.ConsumeEntryAsync(id, token);

        if (storedCode is null)
            return new() { IsSuccess = false };

        var isMatch = string.Equals(storedCode, code?.Trim(), StringComparison.OrdinalIgnoreCase);

        return new() { IsSuccess = isMatch };
    }
}
