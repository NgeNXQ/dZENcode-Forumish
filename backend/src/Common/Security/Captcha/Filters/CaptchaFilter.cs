using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using dZENcode.Forumish.Common.Security.Captcha.Options;
using dZENcode.Forumish.Features.Captcha.Orchestration.Interfaces;
using dZENcode.Forumish.Features.Captcha.Infrastructure.Options;

namespace dZENcode.Forumish.Common.Security.Captcha.Filters;

internal sealed class CaptchaFilter(
    ICaptchaVerificationService captchaVerificationService,
    IOptions<CaptchaGeneralOptions> captchaGeneralOptions,
    IOptions<CaptchaConcernsOptions> captchaConcernsOptions
) : IEndpointFilter
{
    private readonly CaptchaGeneralOptions _generalOptions = captchaGeneralOptions.Value;
    private readonly CaptchaConcernsOptions _concernsOptions = captchaConcernsOptions.Value;

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        if (!_TryGetCaptchaValues(context.HttpContext, out var id, out var code))
            return Results.BadRequest("CAPTCHA is required");

        var result = await captchaVerificationService.VerifyCaptchaAsync(
            id,
            code,
            context.HttpContext.RequestAborted
        );

        if (!result.IsSuccess)
        {
            return Results.Problem(
                title: "CAPTCHA verification failed.",
                statusCode: StatusCodes.Status403Forbidden
            );
        }

        return await next(context);
    }

    private bool _TryGetCaptchaValues(HttpContext httpContext, out string id, out string code)
    {
        id = string.Empty;
        code = string.Empty;
        var headers = httpContext.Request.Headers;

        if (!headers.TryGetValue(_concernsOptions.IdHeader, out var idValue))
            return false;

        if (!headers.TryGetValue(_concernsOptions.CodeHeader, out var codeValue))
            return false;

        if (idValue.Count != 1 || codeValue.Count != 1)
            return false;

        id = idValue[0]!;
        code = codeValue[0]!;

        if (string.IsNullOrEmpty(id) || id.Length != _generalOptions.IdLength)
            return false;

        if (string.IsNullOrEmpty(code) || code.Length != _generalOptions.CodeLength)
            return false;

        return true;
    }
}
