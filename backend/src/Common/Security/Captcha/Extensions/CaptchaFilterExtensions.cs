using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using dZENcode.Forumish.Common.Security.Captcha.Filters;

namespace dZENcode.Forumish.Common.Security.Captcha.Extensions;

internal static class CaptchaFilterExtensions
{
    extension<TBuilder>(TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        internal TBuilder RequireCaptcha()
        {
            return builder.AddEndpointFilter<TBuilder, CaptchaFilter>();
        }
    }
}
