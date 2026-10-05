using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Services;

internal sealed class UserIdentityResolverService(
    IUsersRepository usersRepository
) : IUserIdentityResolverService
{
    public async Task<User> ResolveUserAsync(
        string ip,
        string fingerprint,
        CancellationToken token
    )
    {
        var ipHash = SHA256.HashData(Encoding.UTF8.GetBytes(ip));
        var fingerprintHash = SHA256.HashData(Encoding.UTF8.GetBytes(fingerprint));

        var user = await usersRepository.ReadUserByIpAndFingerprintAsync(
            ipHash,
            fingerprintHash,
            token
        );

        if (user is null)
        {
            user = User.Create(ipHash, fingerprintHash);
            usersRepository.Create(user);
        }

        return user;
    }
}
