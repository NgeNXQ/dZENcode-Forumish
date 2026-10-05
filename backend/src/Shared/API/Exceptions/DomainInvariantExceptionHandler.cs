using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using dZENcode.Forumish.Shared.EntityFramework.Domain;

namespace dZENcode.Forumish.Shared.API.Exceptions;

internal sealed class DomainInvariantExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken token
    )
    {
        if (exception is not DomainInvariantException invariantException)
            return false;

        var result = Results.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "A domain invariant was violated.",
            detail: invariantException.Message
        );

        await result.ExecuteAsync(context);
        return true;
    }
}
