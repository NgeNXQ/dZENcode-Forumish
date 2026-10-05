using System.Threading;
using System.Threading.Tasks;
using dZENcode.Forumish.Features.Captcha.Orchestration.Models;

namespace dZENcode.Forumish.Features.Captcha.Orchestration.Interfaces;

internal interface ICaptchaGeneratorService
{
    Task<CaptchaCreationResult> GenerateCaptchaAsync(
        int width,
        int height,
        CancellationToken token
    );
}
