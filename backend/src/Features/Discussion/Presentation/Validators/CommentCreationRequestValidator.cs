using Microsoft.Extensions.Options;
using FluentValidation;
using dZENcode.Forumish.Shared.FluentValidation;
using dZENcode.Forumish.Features.Discussion.Domain.Options;
using dZENcode.Forumish.Features.Discussion.Presentation.Schemas;
using dZENcode.Forumish.Features.Discussion.Presentation.Options;
using dZENcode.Forumish.Features.Discussion.Orchestration.Options;

namespace dZENcode.Forumish.Features.Discussion.Presentation.Validators;

internal sealed class CommentCreationRequestValidator : AbstractValidator<CommentCreationRequest>
{
    public CommentCreationRequestValidator(
        IOptions<CommentOptions> commentOptions,
        IOptions<CommentAttachmentOptions> commentAttachmentOptions,
        IOptions<CommentIdentityOptions> commentIdentityOptions
    )
    {
        var domainOptions = commentOptions.Value;
        var identityOptions = commentIdentityOptions.Value;
        var attachmentOptions = commentAttachmentOptions.Value;

        RuleFor(dto => dto.ParentId)
            .GreaterThan(0)
                .WithMessage("{PropertyName} must be greater than {ComparisonValue}.")
        .When(dto => dto.ParentId.HasValue);

        RuleFor(dto => dto.Email)
            .NotEmpty()
                .WithMessage("{PropertyName} is required.")
            .EmailAddress()
                .WithMessage("{PropertyName} must be a valid email address.")
            .MaximumLength(domainOptions.EmailMaximumLength)
                .WithMessage("{PropertyName} must not exceed {MaxLength} symbols.");

        RuleFor(dto => dto.Username)
            .NotEmpty()
                .WithMessage("{PropertyName} is required.")
            .Matches("^[A-Za-z0-9]+$")
                .WithMessage("{PropertyName} can contain only English letters and digits.")
            .MinimumLength(domainOptions.UsernameMinimumLength)
                .WithMessage("{PropertyName} must be at least {MinLength} symbols.")
            .MaximumLength(domainOptions.UsernameMaximumLength)
                .WithMessage("{PropertyName} must not exceed {MaxLength} symbols.");

        RuleFor(dto => dto.HomePage)
            .WebUrl()
                .WithMessage("{PropertyName} must be a valid URL.")
            .MaximumLength(domainOptions.HomePageMaximumLength)
                .WithMessage("{PropertyName} must not exceed {MaxLength} symbols.")
        .When(dto => !string.IsNullOrWhiteSpace(dto.HomePage));

        RuleFor(dto => dto.Message)
            .NotEmpty()
                .WithMessage("{PropertyName} is required.")
            .MaximumLength(domainOptions.MessageMaximumLength)
                .WithMessage("{PropertyName} must not exceed {MaxLength} symbols.");

        RuleFor(dto => dto.Fingerprint)
            .NotEmpty()
                .WithMessage("{PropertyName} is required.")
            .MaximumLength(identityOptions.FingerprintMaximumLength)
                .WithMessage("{PropertyName} must not exceed {MaxLength} symbols.");

        RuleFor(dto => dto.Attachment)
            .FileMime(attachmentOptions.Mime)
                .WithMessage("{PropertyName} has an invalid type or size.")
        .When(dto => dto.Attachment is not null);
    }
}
