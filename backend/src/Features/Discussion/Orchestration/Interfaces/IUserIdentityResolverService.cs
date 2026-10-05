using System.Threading;
using System.Threading.Tasks;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

internal interface IUserIdentityResolverService
{
    Task<User> ResolveUserAsync(
        string ip,
        string fingerprint,
        CancellationToken token
    );
}
