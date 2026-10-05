using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
using StackExchange.Redis;
using dZENcode.Forumish.Common.Resilience.Redis;
using dZENcode.Forumish.Features.Captcha.Infrastructure.Options;
using dZENcode.Forumish.Features.Captcha.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Captcha.Infrastructure.Repositories;

internal sealed class CaptchasRepository(
    IConnectionMultiplexer redisConnectionMultiplexer,
    IOptions<CaptchaStorageOptions> captchaStorageOptions,
    ResiliencePipelineProvider<StackExchangeRedisPipeline> pipelineProvider
) : ICaptchasRepository
{
    private readonly CaptchaStorageOptions _storageOptions = captchaStorageOptions.Value;

    private readonly ResiliencePipeline _redisDefaultPipeline = pipelineProvider.GetPipeline(
        StackExchangeRedisPipeline.Default
    );

    private readonly ResiliencePipeline _redisConsumptionPipeline = pipelineProvider.GetPipeline(
        StackExchangeRedisPipeline.Consumption
    );

    public async Task CreateEntryAsync(Guid id, string code, CancellationToken token)
    {
        await _redisDefaultPipeline.ExecuteAsync(
            async contextToken =>
            {
                var database = redisConnectionMultiplexer.GetDatabase();

                await database.StringSetAsync(
                    $"{_storageOptions.EntryPrefix}:{id}",
                    code,
                    TimeSpan.FromSeconds(_storageOptions.EntryExpirationSeconds)
                ).WaitAsync(contextToken);
            },
            token
        );
    }

    public async Task<string?> ConsumeEntryAsync(Guid id, CancellationToken token)
    {
        return await _redisConsumptionPipeline.ExecuteAsync(
            async contextToken =>
            {
                var database = redisConnectionMultiplexer.GetDatabase();

                var value = await database.StringGetDeleteAsync(
                    $"{_storageOptions.EntryPrefix}:{id}"
                ).WaitAsync(contextToken);

                return value.IsNullOrEmpty ? null : value.ToString();
            },
            token
        );
    }
}
