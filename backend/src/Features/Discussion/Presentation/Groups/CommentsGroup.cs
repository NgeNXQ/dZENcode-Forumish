using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using AutoMapper;
using Mediator;
using dZENcode.Forumish.Shared.API.Pagination;
using dZENcode.Forumish.Features.Discussion.Presentation.Schemas;
using dZENcode.Forumish.Features.Discussion.Orchestration.UseCases.ReadComments;
using dZENcode.Forumish.Features.Discussion.Orchestration.UseCases.CreateComment;

namespace dZENcode.Forumish.Features.Discussion.Presentation.Groups;

internal static class CommentsGroup
{
    internal static async Task<Created<CommentCreationResponse>> CreateComment(
        [FromForm] CommentCreationRequest request,
        ISender sender,
        IMapper mapper,
        CancellationToken token
    )
    {
        var command = mapper.Map<CreateCommentCommand>(request);

        await using var content = command.Attachment?.Content;

        var result = await sender.Send(command, token);

        return TypedResults.Created(
            result.ParentId is { } parentId
                ? $"/api/discussion/comments?parentId={parentId}"
                : "/api/discussion/comments",
            new CommentCreationResponse(
                result.Id,
                result.ParentId,
                result.Email,
                result.Username,
                result.HomePage,
                result.Message,
                result.AttachmentId,
                result.CreatedAt
            )
        );
    }

    internal static async Task<Results<Ok<SimplePaginationMetadata<CommentReadingResponse>>, NotFound>> ReadComments(
        [AsParameters] CommentsReadingRequest request,
        ISender sender,
        IMapper mapper,
        CancellationToken token
    )
    {
        var query = mapper.Map<ReadCommentsQuery>(request);

        var result = await sender.Send(query, token);

        if (result is null)
            return TypedResults.NotFound();

        var items = result.Items.Select(mapper.Map<CommentReadingResponse>).ToList();

        return TypedResults.Ok(
            new SimplePaginationMetadata<CommentReadingResponse>(items, result.Page, result.Size)
        );
    }
}
