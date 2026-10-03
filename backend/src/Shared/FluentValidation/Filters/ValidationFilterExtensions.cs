using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

namespace dZENcode.Forumish.Shared.FluentValidation.Filters;

internal static class ValidationFilterExtensions
{
    extension(RouteHandlerBuilder builder)
    {
        internal RouteHandlerBuilder EnableValidation<TRequest>() where TRequest : class
        {
            return builder.AddEndpointFilter<ValidationFilter<TRequest>>();
        }
    }
}
