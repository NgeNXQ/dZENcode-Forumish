using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using FluentValidation;
using dZENcode.Forumish.Shared.Http;

namespace dZENcode.Forumish.Shared.FluentValidation;

internal static class RuleBuilderExtensions
{
    extension<T>(IRuleBuilder<T, string?> builder)
    {
        internal IRuleBuilderOptions<T, string?> WebUrl()
        {
            return builder.Must(value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
                    return false;

                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            });
        }
    }

    extension<T>(IRuleBuilder<T, IFormFile?> builder)
    {
        internal IRuleBuilderOptions<T, IFormFile?> FileMime(
            IReadOnlyDictionary<string, long> configuration
        )
        {
            return builder.Must(file =>
            {
                if (file is null)
                    return false;

                var mime = MimeType.Normalize(file.ContentType);
                foreach (var pair in configuration)
                {
                    if (string.Equals(pair.Key, mime, StringComparison.OrdinalIgnoreCase))
                        return file.Length > 0 && file.Length <= pair.Value;
                }
                return false;
            });
        }
    }
}
