using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using dZENcode.Forumish.Common.Security.Captcha.Options;
using dZENcode.Forumish.Features.Captcha.Infrastructure.Options;
using dZENcode.Forumish.Features.Captcha.Orchestration.Interfaces;

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
        EndpointFilterDelegate next
    )
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

    private bool _TryGetCaptchaValues(HttpContext httpContext, out Guid id, out string code)
    {
        id = Guid.Empty;
        code = string.Empty;
        var headers = httpContext.Request.Headers;

        if (!_TryGetHeaderValue(
            headers, _concernsOptions.IdHeader, _generalOptions.IdLength, out var idValue
        ))
        {
            return false;
        }

        if (!_TryGetHeaderValue(
            headers, _concernsOptions.CodeHeader, _generalOptions.CodeLength, out code
        ))
        {
            return false;
        }

        return Guid.TryParse(idValue, out id);
    }

    private static bool _TryGetHeaderValue(
        IHeaderDictionary headers,
        string name,
        int expectedLength,
        out string value
    )
    {
        value = string.Empty;

        if (!headers.TryGetValue(name, out var headerValues))
            return false;

        if (headerValues.Count != 1)
            return false;

        var headerValue = headerValues[0];
        if (string.IsNullOrWhiteSpace(headerValue))
            return false;

        if (headerValue.Length != expectedLength)
            return false;

        value = headerValue;
        return true;
    }
}
