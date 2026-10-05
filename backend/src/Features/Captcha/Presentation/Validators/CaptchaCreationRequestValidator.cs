using Microsoft.Extensions.Options;
using FluentValidation;
using dZENcode.Forumish.Features.Captcha.Presentation.Options;
using dZENcode.Forumish.Features.Captcha.Presentation.Schemas;

namespace dZENcode.Forumish.Features.Captcha.Presentation.Validators;

internal sealed class CaptchaCreationRequestValidator : AbstractValidator<CaptchaCreationRequest>
{
    public CaptchaCreationRequestValidator(
        IOptions<CaptchaVisualsOptions> captchaVisualsOptions
    )
    {
        var visualsOptions = captchaVisualsOptions.Value;

        RuleFor(dto => dto.Width)
            .GreaterThan(0)
                .WithMessage("{PropertyName} must be greater than {ComparisonValue}.")
            .LessThanOrEqualTo(visualsOptions.MaximumWidth)
                .WithMessage("{PropertyName} must not exceed {ComparisonValue}.");

        RuleFor(dto => dto.Height)
            .GreaterThan(0)
                .WithMessage("{PropertyName} must be greater than {ComparisonValue}.")
            .LessThanOrEqualTo(visualsOptions.MaximumHeight)
                .WithMessage("{PropertyName} must not exceed {ComparisonValue}.");
    }
}
