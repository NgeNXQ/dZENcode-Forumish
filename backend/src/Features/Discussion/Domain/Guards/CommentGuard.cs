using System;
using System.Linq;
using System.Net.Mail;
using dZENcode.Forumish.Shared.EntityFramework.Domain;
using dZENcode.Forumish.Features.Discussion.Domain.Options;

namespace dZENcode.Forumish.Features.Discussion.Domain.Entities;

internal sealed partial class Comment
{
    private static CommentGuard? _guard;

    private static CommentGuard Guard => _guard
        ?? throw new InvalidOperationException("Comment validation has not been configured.");

    internal static void ConfigureValidation(CommentOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _guard = new CommentGuard(options);
    }

    private sealed class CommentGuard(CommentOptions options)
    {
        internal void Validate(
            User author,
            long? parentId,
            string email,
            string username,
            string? homePage,
            string message
        )
        {
            ArgumentNullException.ThrowIfNull(author);

            if (parentId is <= 0)
                throw new DomainInvariantException("ParentId must be greater than 0.");

            _EnsureLength(email, 1, options.EmailMaximumLength, nameof(Email));

            _EnsureLength(
                username,
                options.UsernameMinimumLength,
                options.UsernameMaximumLength,
                nameof(Username)
            );

            _EnsureLength(message, 1, options.MessageMaximumLength, nameof(Message));

            if (!_IsValidEmail(email.Trim()))
                throw new DomainInvariantException("Email must be a valid address.");

            if (!username.Trim().All(char.IsAsciiLetterOrDigit))
                throw new DomainInvariantException(
                    "Username can contain only English letters and digits."
                );

            if (!string.IsNullOrWhiteSpace(homePage))
            {
                _EnsureLength(homePage, 1, options.HomePageMaximumLength, nameof(HomePage));

                if (!_IsWebUrl(homePage.Trim()))
                    throw new DomainInvariantException("HomePage must be a web URL.");
            }
        }

        private static bool _IsValidEmail(string email)
        {
            if (!MailAddress.TryCreate(email, out var address))
                return false;

            return string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
        }

        private static bool _IsWebUrl(string homePage)
        {
            if (!Uri.TryCreate(homePage, UriKind.Absolute, out var uri))
                return false;

            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }

        private static void _EnsureLength(string? value, int minimum, int maximum, string field)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainInvariantException($"{field} must not be empty.");

            var length = value.Trim().Length;
            if (length < minimum || length > maximum)
            {
                throw new DomainInvariantException(
                    $"{field} length must be between {minimum} and {maximum}."
                );
            }
        }
    }
}
