using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using dZENcode.Forumish.Common.Persistence;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Repositories;

internal sealed class UsersRepository(AppDbContext context) : IUsersRepository
{
    public void Create(User user)
    {
        context.Add(user);
    }

    public Task<User?> ReadUserByIpAndFingerprintAsync(
        byte[] ipHash,
        byte[] fingerprintHash,
        CancellationToken token
    )
    {
        return context.Users.SingleOrDefaultAsync(user =>
            user.IpHash == ipHash &&
            user.FingerprintHash == fingerprintHash,
            token
        );
    }
}
