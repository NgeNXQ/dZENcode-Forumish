using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Mediator;
using dZENcode.Forumish.Features.Discussion.Orchestration.UseCases.ReadAttachment;

namespace dZENcode.Forumish.Features.Discussion.Presentation.Groups;

internal static class AttachmentsGroup
{
    internal static async Task<Results<FileStreamHttpResult, NotFound>> ReadAttachment(
        Guid id,
        ISender sender,
        HttpContext context,
        CancellationToken token
    )
    {
        var attachment = await sender.Send(new ReadAttachmentQuery(id), token);

        if (attachment is null)
            return TypedResults.NotFound();

        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        return TypedResults.File(attachment.Content, attachment.ContentType);
    }
}
