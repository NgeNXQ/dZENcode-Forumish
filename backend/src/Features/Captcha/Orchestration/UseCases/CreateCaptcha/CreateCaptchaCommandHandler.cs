using System.Threading;
using System.Threading.Tasks;
using Mediator;
using dZENcode.Forumish.Features.Captcha.Orchestration.Models;
using dZENcode.Forumish.Features.Captcha.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Captcha.Orchestration.UseCases.CreateCaptcha;

internal sealed class CreateCaptchaCommandHandler(
    ICaptchaGeneratorService captchaGeneratorService
) : ICommandHandler<CreateCaptchaCommand, CaptchaCreationResult>
{
    public async ValueTask<CaptchaCreationResult> Handle(
        CreateCaptchaCommand command,
        CancellationToken token
    )
    {
        return await captchaGeneratorService.GenerateCaptchaAsync(
            command.Width,
            command.Height,
            token
        );
    }
}
