using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using dZENcode.Forumish.Shared.FluentValidation.Filters;
using dZENcode.Forumish.Common.Security.Captcha.Policies;
using dZENcode.Forumish.Features.Captcha.Presentation.Schemas;

namespace dZENcode.Forumish.Features.Captcha.Presentation.Groups;

internal static class CaptchaGroupExtensions
{
    extension(IEndpointRouteBuilder builder)
    {
        internal RouteGroupBuilder MapCaptchaEndpoints()
        {
            var group = builder.MapGroup("api/captcha")
                .WithTags("Captcha")
                .RequireRateLimiting(CaptchaSecurityPolicies.CreationRateLimit);

            group.MapPost("/", CaptchaGroup.CreateCaptcha)
                .EnableValidation<CaptchaCreationRequest>()
                .ProducesValidationProblem();

            return group;
        }
    }
}
