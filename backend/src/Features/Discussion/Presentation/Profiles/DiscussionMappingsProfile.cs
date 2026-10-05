using Microsoft.AspNetCore.Http;
using AutoMapper;
using dZENcode.Forumish.Features.Discussion.Presentation.Schemas;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;
using dZENcode.Forumish.Features.Discussion.Orchestration.UseCases.ReadComments;
using dZENcode.Forumish.Features.Discussion.Orchestration.UseCases.CreateComment;

namespace dZENcode.Forumish.Features.Discussion.Presentation.Profiles;

internal sealed class DiscussionMappingsProfile : Profile
{
    public DiscussionMappingsProfile(IHttpContextAccessor httpContextAccessor)
    {
        CreateMap<IFormFile?, AttachmentUpload?>()
            .ConvertUsing(static file =>
                file == null
                    ? null
                    : new AttachmentUpload(file.OpenReadStream(), file.ContentType)
        );

        CreateMap<CommentsReadingRequest, ReadCommentsQuery>();

        CreateMap<CommentReadingModel, CommentReadingResponse>();

        CreateMap<CommentCreationRequest, CreateCommentCommand>()
            .ForCtorParam(
                nameof(CreateCommentCommand.Ip),
                options => options.MapFrom(
                    (_, _) => httpContextAccessor.HttpContext!
                        .Connection.RemoteIpAddress?.ToString() ?? "unknown"
            )
        );
    }
}
