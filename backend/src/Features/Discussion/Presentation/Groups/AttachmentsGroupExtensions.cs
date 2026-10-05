using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace dZENcode.Forumish.Features.Discussion.Presentation.Groups;

internal static class AttachmentsGroupExtensions
{
    extension(IEndpointRouteBuilder builder)
    {
        internal RouteGroupBuilder MapAttachmentsEndpoints()
        {
            var group = builder.MapGroup("api/discussion/attachments")
                .WithTags("Discussion");

            group.MapGet("/{id:guid}", AttachmentsGroup.ReadAttachment);

            return group;
        }
    }
}
