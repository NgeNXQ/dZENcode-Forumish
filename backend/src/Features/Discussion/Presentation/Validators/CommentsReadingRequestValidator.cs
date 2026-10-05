using Microsoft.Extensions.Options;
using FluentValidation;
using dZENcode.Forumish.Features.Discussion.Orchestration.Options;
using dZENcode.Forumish.Features.Discussion.Presentation.Schemas;

namespace dZENcode.Forumish.Features.Discussion.Presentation.Validators;

internal sealed class CommentsReadingRequestValidator : AbstractValidator<CommentsReadingRequest>
{
    public CommentsReadingRequestValidator(
        IOptions<DiscussionPaginationOptions> discussionPaginationOptions
    )
    {
        var paginationOptions = discussionPaginationOptions.Value;

        RuleFor(dto => dto.ParentId)
            .GreaterThan(0)
                .WithMessage("{PropertyName} must be greater than {ComparisonValue}.")
        .When(dto => dto.ParentId.HasValue);

        RuleFor(dto => dto.Page)
            .GreaterThan(0)
                .WithMessage("{PropertyName} must be greater than {ComparisonValue}.")
        .When(dto => dto.Page.HasValue);

        RuleFor(dto => dto.Size)
            .GreaterThan(0)
                .WithMessage("{PropertyName} must be greater than {ComparisonValue}.")
            .LessThanOrEqualTo(paginationOptions.MaximumPageSize)
                .WithMessage("{PropertyName} must not exceed {ComparisonValue}.")
        .When(dto => dto.Size.HasValue);

        RuleFor(dto => dto)
            .Must(dto =>
            {
                var page = dto.Page ?? 1;
                var size = dto.Size ?? paginationOptions.DefaultPageSize;
                return ((long)page - 1) * size <= int.MaxValue;
            })
            .WithMessage("{PropertyName} is out of range.")
        .When(dto =>
            dto.Page is > 0 && dto.Size is null or > 0);

        RuleFor(dto => dto.SortBy)
            .IsInEnum()
                .WithMessage("{PropertyName} has an invalid value.")
        .When(dto => dto.SortBy.HasValue);

        RuleFor(dto => dto.Direction)
            .IsInEnum()
                .WithMessage("{PropertyName} has an invalid value.")
        .When(dto => dto.Direction.HasValue);
    }
}
