using Mediator;
using dZENcode.Forumish.Features.Captcha.Orchestration.Models;

namespace dZENcode.Forumish.Features.Captcha.Orchestration.UseCases.CreateCaptcha;

internal record class CreateCaptchaCommand(
    int Width,
    int Height
) : ICommand<CaptchaCreationResult>;
