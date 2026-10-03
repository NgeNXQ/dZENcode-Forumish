using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Mediator;
using AutoMapper;
using dZENcode.Forumish.Features.Captcha.Presentation.Schemas;
using dZENcode.Forumish.Features.Captcha.Orchestration.UseCases.CreateCaptcha;

namespace dZENcode.Forumish.Features.Captcha.Presentation.Groups;

internal sealed class CaptchaGroup
{
    internal static async Task<Ok<CaptchaCreationResponse>> CreateCaptcha(
        CaptchaCreationRequest request,
        ISender sender,
        IMapper mapper,
        CancellationToken token
    )
    {
        var command = mapper.Map<CreateCaptchaCommand>(request);

        var result = await sender.Send(command, token);

        return TypedResults.Ok(
            new CaptchaCreationResponse(result.Id, result.Payload)
        );
    }
}
