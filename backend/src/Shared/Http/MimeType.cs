using Microsoft.Net.Http.Headers;

namespace dZENcode.Forumish.Shared.Http;

internal static class MimeType
{
    internal static string? Normalize(string? contentType)
    {
        if (MediaTypeHeaderValue.TryParse(contentType, out var header))
            return header.MediaType.Value?.ToLowerInvariant();

        return null;
    }
}
