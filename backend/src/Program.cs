using System;
using System.Globalization;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FluentValidation;
using Lazy.Captcha.Core.Generator.Code;
using Lazy.Captcha.Core.Generator.Image;
using Polly;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using dZENcode.Forumish.Common.Resilience.Redis;
using dZENcode.Forumish.Common.Security.Captcha.Options;
using dZENcode.Forumish.Common.Security.Captcha.Policies;
using dZENcode.Forumish.Features.Captcha.Infrastructure.Options;
using dZENcode.Forumish.Features.Captcha.Infrastructure.Providers;
using dZENcode.Forumish.Features.Captcha.Infrastructure.Repositories;
using dZENcode.Forumish.Features.Captcha.Orchestration.Interfaces;
using dZENcode.Forumish.Features.Captcha.Orchestration.Services;
using dZENcode.Forumish.Features.Captcha.Presentation.Groups;
using dZENcode.Forumish.Features.Captcha.Presentation.Mapping;
using dZENcode.Forumish.Features.Captcha.Presentation.Options;
using dZENcode.Forumish.Features.Captcha.Presentation.Schemas;
using dZENcode.Forumish.Features.Captcha.Presentation.Validators;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateSlimBuilder(args);

    builder.Services.AddSerilog(
        (services, configuration) => configuration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
    );

    builder.Services
        .Configure<CaptchaGeneralOptions>(
            builder.Configuration.GetSection(CaptchaGeneralOptions.Section))
        .Configure<CaptchaStorageOptions>(
            builder.Configuration.GetSection(CaptchaStorageOptions.Section))
        .Configure<CaptchaVisualsOptions>(
            builder.Configuration.GetSection(CaptchaVisualsOptions.Section))
        .Configure<CaptchaConcernsOptions>(
            builder.Configuration.GetSection(CaptchaConcernsOptions.Section))
        .Configure<CaptchaRateLimitOptions>(
            builder.Configuration.GetSection(CaptchaRateLimitOptions.Section));

    builder.Services.AddProblemDetails();

    builder.Services.AddRateLimiter(static options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = static (context, _) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter =
                    ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(
                        CultureInfo.InvariantCulture
                    );
            }

            return ValueTask.CompletedTask;
        };
    });
    builder.Services
        .AddOptions<RateLimiterOptions>()
        .Configure<IOptions<CaptchaRateLimitOptions>>(
            static (limiter, captchaRateLimitOptions) =>
            {
                var captchaRateLimit = captchaRateLimitOptions.Value;

                limiter.AddPolicy(
                    CaptchaPolicies.CreationRateLimit,
                    httpContext => RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = captchaRateLimit.PermitLimit,
                            Window = TimeSpan.FromSeconds(captchaRateLimit.WindowSeconds),
                            QueueLimit = 0,
                            AutoReplenishment = true,
                        }
                    )
                );
            }
        );

    var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

    if (string.IsNullOrWhiteSpace(redisConnectionString))
        throw new InvalidOperationException("ConnectionStrings:Redis is not configured.");

    builder.Services.AddSingleton(
        services => RedisConnectionFactory.Create(
            redisConnectionString,
            services.GetRequiredService<ILoggerFactory>().CreateLogger("dZENcode.Forumish.Redis")
        )
    );

    builder.Services.AddResiliencePipeline(
        StackExchangeRedisPipeline.Default,
        static pipeline =>
        {
            pipeline.AddRetry(new()
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<RedisTimeoutException>()
                    .Handle<RedisConnectionException>(),
                Delay = TimeSpan.FromMilliseconds(5),
                UseJitter = true,
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 2,
            })
            .AddTimeout(TimeSpan.FromSeconds(2));
        }
    );

    builder.Services.AddMediator(static options =>
    {
        options.ServiceLifetime = ServiceLifetime.Scoped;
        options.GenerateTypesAsInternal = true;
    });

    builder.Services.AddAutoMapper(configuration =>
    {
        var licenseKey = builder.Configuration["AutoMapper:LicenseKey"];

        if (!string.IsNullOrWhiteSpace(licenseKey))
            configuration.LicenseKey = licenseKey;

        configuration.AddProfile<CaptchaMappingsProfile>();
    });

    builder.Services
        .AddScoped<IValidator<CaptchaCreationRequest>, CaptchaCreationRequestValidator>();

    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddCaptcha(builder.Configuration);

    builder.Services.AddSingleton<ICaptchaCodeGenerator, DefaultCaptchaCodeGenerator>();
    builder.Services.AddSingleton<ICaptchaImageGenerator, DefaultCaptchaImageGenerator>();
    builder.Services.AddSingleton<ICaptchaProvider, LazyCaptchaProvider>();

    builder.Services.AddSingleton<ICaptchasRepository, CaptchasRepository>();

    builder.Services.AddScoped<ICaptchaGeneratorService, CaptchaGeneratorService>();
    builder.Services.AddScoped<ICaptchaVerificationService, CaptchaVerificationService>();

    var app = builder.Build();

    _ = app.Services.GetRequiredService<IConnectionMultiplexer>();

    app.UseSerilogRequestLogging(options =>
    {
        options.GetLevel = static (httpContext, _, exception) =>
            exception is not null || httpContext.Response.StatusCode >= 500
                ? LogEventLevel.Error
                : LogEventLevel.Information;
    });

    if (!app.Environment.IsDevelopment())
        app.UseExceptionHandler();

    app.UseStatusCodePages();
    app.UseRateLimiter();

    app.MapCaptchaEndpoints();

    app.Run();
    return 0;
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "Host terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
