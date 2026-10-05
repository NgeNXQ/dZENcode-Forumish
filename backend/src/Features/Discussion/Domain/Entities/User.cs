using System.Collections.Generic;
using dZENcode.Forumish.Shared.EntityFramework.Domain;

namespace dZENcode.Forumish.Features.Discussion.Domain.Entities;

internal sealed partial class User : Entity
{
    internal long Id { get; private set; }
    internal byte[] IpHash { get; private init; } = null!;
    internal byte[] FingerprintHash { get; private init; } = null!;

    internal ICollection<Comment> Comments { get; init; } = new List<Comment>();

    private User()
    {
    }

    internal static User Create(
        byte[] ipHash,
        byte[] fingerprintHash
    )
    {
        Guard.Validate(ipHash, fingerprintHash);

        return new()
        {
            IpHash = (byte[])ipHash.Clone(),
            FingerprintHash = (byte[])fingerprintHash.Clone(),
        };
    }

}
