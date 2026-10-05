using System.Threading;
using System.Threading.Tasks;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

internal interface IUsersRepository
{
    void Create(User user);

    Task<User?> ReadUserByIpAndFingerprintAsync(
        byte[] ipHash,
        byte[] fingerprintHash,
        CancellationToken token
    );
}
