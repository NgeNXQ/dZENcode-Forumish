using System;
using System.Threading;
using System.Threading.Tasks;

namespace dZENcode.Forumish.Features.Captcha.Orchestration.Interfaces;

internal interface ICaptchasRepository
{
    Task CreateEntryAsync(Guid id, string code, CancellationToken token);
    Task<string?> ConsumeEntryAsync(Guid id, CancellationToken token);
}
