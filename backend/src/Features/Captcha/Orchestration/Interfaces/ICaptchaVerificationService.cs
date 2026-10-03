using System.Threading;
using System.Threading.Tasks;
using dZENcode.Forumish.Features.Captcha.Orchestration.Models;

namespace dZENcode.Forumish.Features.Captcha.Orchestration.Interfaces;

internal interface ICaptchaVerificationService
{
    Task<CaptchaVerificationResult> VerifyCaptchaAsync(
        string id,
        string code,
        CancellationToken token
    );
}
