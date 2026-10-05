using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using dZENcode.Forumish.Shared.FluentValidation.Filters;
using dZENcode.Forumish.Common.Caching.Discussion.Policies;
using dZENcode.Forumish.Common.Security.Captcha.Extensions;
using dZENcode.Forumish.Common.Security.Discussion.Policies;
using dZENcode.Forumish.Features.Discussion.Presentation.Schemas;

namespace dZENcode.Forumish.Features.Discussion.Presentation.Groups;

internal static class CommentsGroupExtensions
{
    extension(IEndpointRouteBuilder builder)
    {
        internal RouteGroupBuilder MapCommentsEndpoints()
        {
            var group = builder.MapGroup("api/discussion/comments")
                .WithTags("Discussion");

            group.MapGet("", CommentsGroup.ReadComments)
                .CacheOutput(DiscussionCachePolicies.ReadComments)
                .EnableValidation<CommentsReadingRequest>()
                .ProducesValidationProblem();

            group.MapPost("", CommentsGroup.CreateComment)
                .DisableAntiforgery()
                .RequireCaptcha()
                .EnableValidation<CommentCreationRequest>()
                .RequireRateLimiting(DiscussionSecurityPolicies.CreationRateLimit)
                .ProducesValidationProblem();

            return group;
        }
    }
}
