using System;
using dZENcode.Forumish.Shared.EntityFramework.Domain;
using dZENcode.Forumish.Features.Discussion.Domain.Options;

namespace dZENcode.Forumish.Features.Discussion.Domain.Entities;

internal sealed partial class User
{
    private static UserGuard? _guard;

    private static UserGuard Guard => _guard
        ?? throw new InvalidOperationException("User validation has not been configured.");

    internal static void ConfigureValidation(UserOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _guard = new UserGuard(options);
    }

    private sealed class UserGuard(UserOptions options)
    {
        internal void Validate(byte[] ipHash, byte[] fingerprintHash)
        {
            _ValidateHash(ipHash, nameof(IpHash));
            _ValidateHash(fingerprintHash, nameof(FingerprintHash));
        }

        private void _ValidateHash(byte[]? hash, string field)
        {
            if (hash is null)
                throw new DomainInvariantException($"{field} must not be null.");

            if (hash.Length == 0)
                throw new DomainInvariantException($"{field} must not be empty.");

            if (hash.Length != options.Length)
            {
                throw new DomainInvariantException(
                    $"{field} must match the configured length of {options.Length}."
                );
            }
        }
    }
}
