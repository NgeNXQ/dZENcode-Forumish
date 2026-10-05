using System;
using System.Reflection;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Hangfire;
using Polly;
using Serilog;
using Serilog.Events;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using StackExchange.Redis;
using dZENcode.Forumish.Shared.API.Exceptions;
using dZENcode.Forumish.Shared.EntityFramework.Hooks;
using dZENcode.Forumish.Shared.EntityFramework.SqlServer.Extensions;
using dZENcode.Forumish.Common.Persistence;
using dZENcode.Forumish.Common.Caching.Discussion.Policies;
using dZENcode.Forumish.Common.Resilience.Redis;
using dZENcode.Forumish.Common.Resilience.EntityFramework;
using dZENcode.Forumish.Common.Security.Captcha.Options;
using dZENcode.Forumish.Common.Security.Captcha.Policies;
using dZENcode.Forumish.Common.Security.Discussion.Policies;
using dZENcode.Forumish.Common.Security.Cors.Policies;
using dZENcode.Forumish.Features.Captcha.Infrastructure.Options;
using dZENcode.Forumish.Features.Captcha.Infrastructure.Providers;
using dZENcode.Forumish.Features.Captcha.Infrastructure.Repositories;
using dZENcode.Forumish.Features.Captcha.Orchestration.Services;
using dZENcode.Forumish.Features.Captcha.Orchestration.Interfaces;
using dZENcode.Forumish.Features.Captcha.Presentation.Groups;
using dZENcode.Forumish.Features.Captcha.Presentation.Mapping;
using dZENcode.Forumish.Features.Captcha.Presentation.Options;
using dZENcode.Forumish.Features.Captcha.Presentation.Schemas;
using dZENcode.Forumish.Features.Captcha.Presentation.Validators;
using dZENcode.Forumish.Features.Discussion.Domain.Options;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Jobs;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Models;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Options;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Adapters;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Storages;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Providers;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Interfaces;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Processors;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Schedulers;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Repositories;
using dZENcode.Forumish.Features.Discussion.Orchestration.Options;
using dZENcode.Forumish.Features.Discussion.Orchestration.Services;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;
using dZENcode.Forumish.Features.Discussion.Presentation.Groups;
using dZENcode.Forumish.Features.Discussion.Presentation.Options;
using dZENcode.Forumish.Features.Discussion.Presentation.Schemas;
using dZENcode.Forumish.Features.Discussion.Presentation.Profiles;
using dZENcode.Forumish.Features.Discussion.Presentation.Validators;

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

    builder.Services
        .Configure<UserOptions>(
            builder.Configuration.GetSection(UserOptions.Section))
        .Configure<CommentOptions>(
            builder.Configuration.GetSection(CommentOptions.Section))
        .Configure<AttachmentOptions>(
            builder.Configuration.GetSection(AttachmentOptions.Section))
        .Configure<CommentAttachmentOptions>(
            builder.Configuration.GetSection(CommentAttachmentOptions.Section))
        .Configure<DiscussionPaginationOptions>(
            builder.Configuration.GetSection(DiscussionPaginationOptions.Section))
        .Configure<CommentCacheOptions>(
            builder.Configuration.GetSection(CommentCacheOptions.Section))
        .Configure<CommentMessageHtmlSanitizerOptions>(
            builder.Configuration.GetSection(CommentMessageHtmlSanitizerOptions.Section))
        .Configure<AttachmentStorageOptions>(
            builder.Configuration.GetSection(AttachmentStorageOptions.Section))
        .Configure<ImageAttachmentProcessingOptions>(
            builder.Configuration.GetSection(ImageAttachmentProcessingOptions.Section))
        .Configure<TextAttachmentProcessingOptions>(
            builder.Configuration.GetSection(TextAttachmentProcessingOptions.Section));

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddCors(options => options.AddPolicy(
        CorsSecurityPolicies.SameOrigin,
        policy => policy.SetIsOriginAllowed(static _ => false).DisallowCredentials()
    ));

    builder.Services.AddOptions<AttachmentRecoveryOptions>()
        .Bind(builder.Configuration.GetSection(AttachmentRecoveryOptions.Section))
        .Validate(static options =>
            options.IntervalSeconds > 0 &&
            options.PendingAgeSeconds > 0,
            "Attachment recovery interval and pending age must be positive."
        )
        .ValidateOnStart();
    builder.Services.AddOptions<FormOptions>().Configure<IOptions<CommentAttachmentOptions>>(
        (options, attachmentOptions) =>
        {
            long maximumLength = 0;
            foreach (var length in attachmentOptions.Value.Mime.Values)
                maximumLength = Math.Max(maximumLength, length);
            options.MultipartBodyLengthLimit = maximumLength;
        });
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<DomainInvariantExceptionHandler>();
    builder.Services.AddOptions<CommentIdentityOptions>()
        .Bind(builder.Configuration.GetSection(CommentIdentityOptions.Section))
        .Validate(options => options.FingerprintMaximumLength > 0,
            "Fingerprint maximum length must be greater than zero.")
        .ValidateOnStart();

    builder.Services.AddRateLimiter(static options =>
    {
        options.AddPolicy(DiscussionSecurityPolicies.CreationRateLimit, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                }
            )
        );
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
                    CaptchaSecurityPolicies.CreationRateLimit,
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

    builder.Services.AddResiliencePipeline(
        StackExchangeRedisPipeline.Consumption,
        static pipeline => pipeline.AddTimeout(TimeSpan.FromSeconds(2))
    );

    builder.Services.AddMediator(static options =>
    {
        options.ServiceLifetime = ServiceLifetime.Scoped;
        options.GenerateTypesAsInternal = true;
    });

    builder.Services.AddAutoMapper((services, configuration) =>
    {
        var licenseKey = builder.Configuration["AutoMapper:LicenseKey"];

        if (!string.IsNullOrWhiteSpace(licenseKey))
            configuration.LicenseKey = licenseKey;

        configuration.AddProfile<CaptchaMappingsProfile>();
        configuration.AddProfile(
            new DiscussionMappingsProfile(services.GetRequiredService<IHttpContextAccessor>())
        );
    }, Array.Empty<Assembly>());

    builder.Services
        .AddScoped<IValidator<CaptchaCreationRequest>, CaptchaCreationRequestValidator>();

    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddCaptcha(builder.Configuration);

    builder.Services.AddSingleton<ICaptchaProvider, LazyCaptchaProvider>();

    builder.Services.AddSingleton<ICaptchasRepository, CaptchasRepository>();

    builder.Services.AddScoped<ICaptchaGeneratorService, CaptchaGeneratorService>();
    builder.Services.AddScoped<ICaptchaVerificationService, CaptchaVerificationService>();

    var sqlConnectionString = builder.Configuration.GetConnectionString("SqlServer");
    if (string.IsNullOrWhiteSpace(sqlConnectionString))
        throw new InvalidOperationException("ConnectionStrings:SqlServer is not configured.");

    builder.Services.AddScoped<AuditableTimestampsPopulationInterceptor>();
    builder.Services.AddScoped<DomainEventsDeferredPropagationInterceptor>();
    builder.Services.AddDbContext<AppDbContext>((services, options) => options
        .UseSqlServer(sqlConnectionString)
        .AddInterceptors(
            services.GetRequiredService<AuditableTimestampsPopulationInterceptor>(),
            services.GetRequiredService<DomainEventsDeferredPropagationInterceptor>()
        ));

    builder.Services.AddResiliencePipeline(EntityFrameworkPipeline.UniqueConstraintViolation,
        static pipeline => pipeline.AddRetry(new()
        {
            ShouldHandle = new PredicateBuilder().Handle<DbUpdateException>(
                exception => exception.IsUniqueConstraintViolation
            ),
            MaxRetryAttempts = 2,
            Delay = TimeSpan.FromMilliseconds(10),
            UseJitter = true,
        }));
    builder.Services.AddResiliencePipeline(EntityFrameworkPipeline.OptimisticConcurrency,
        static pipeline => pipeline.AddRetry(new()
        {
            ShouldHandle = new PredicateBuilder().Handle<DbUpdateConcurrencyException>(),
            MaxRetryAttempts = 2,
            Delay = TimeSpan.FromMilliseconds(10),
            UseJitter = true,
        }));

    builder.Services.AddHangfire(configuration => configuration
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(sqlConnectionString));
    builder.Services.AddHangfireServer();
    builder.Services.AddScoped<AttachmentProcessingJob>();
    builder.Services.AddHostedService<AttachmentRecoveryService>();
    builder.Services.AddScoped<IAttachmentProcessingScheduler, HangfireAttachmentProcessingScheduler>();
    builder.Services.AddScoped<ICommentsRepository, CommentsRepository>();
    builder.Services.AddScoped<IUsersRepository, UsersRepository>();
    builder.Services.AddScoped<IAttachmentsRepository, AttachmentsRepository>();
    builder.Services.AddScoped<IUserIdentityResolverService, UserIdentityResolverService>();
    builder.Services.AddScoped<ICommentProcessingService, CommentProcessingService>();
    builder.Services.AddScoped<ICommentMessageHtmlSanitizerAdapter, CommentMessageGanssHtmlSanitizerAdapter>();
    builder.Services.AddScoped<IAttachmentIntakeService, AttachmentIntakeService>();
    builder.Services.AddScoped<IAttachmentProcessingService, AttachmentProcessingService>();
    builder.Services.AddSingleton<IAttachmentsStagingStorage, AttachmentsStagingStorage>();
    builder.Services.AddSingleton<IAttachmentsPublicStorage, AttachmentsPublicStorage>();
    builder.Services.AddScoped<IAttachmentContentProcessorProvider, AttachmentContentProcessorProvider>();
    builder.Services.AddScoped<ImageSharpAttachmentContentProcessor>();
    builder.Services.AddScoped<IImageEncodingTargetProvider, ImageEncodingTargetProvider>();
    builder.Services.AddKeyedScoped<ImageEncodingTarget>(JpegFormat.Instance.Name,
        (services, _) => new ImageEncodingTarget(
            new JpegEncoder
            {
                Quality = services.GetRequiredService<IOptions<ImageAttachmentProcessingOptions>>().Value.JpegQuality,
            }, "image/jpeg", ".jpg"));
    builder.Services.AddKeyedScoped<ImageEncodingTarget>(PngFormat.Instance.Name,
        (_, _) => new ImageEncodingTarget(new PngEncoder(), "image/png", ".png"));
    builder.Services.AddKeyedScoped<ImageEncodingTarget>(GifFormat.Instance.Name,
        (_, _) => new ImageEncodingTarget(new GifEncoder(), "image/gif", ".gif"));
    builder.Services.AddScoped<PlainTextAttachmentContentProcessor>();
    foreach (var fileType in new[] { "image/jpeg", "image/png", "image/gif" })
    {
        builder.Services.AddKeyedScoped<IAttachmentContentProcessor>(fileType,
            (services, _) => services.GetRequiredService<ImageSharpAttachmentContentProcessor>());
    }
    builder.Services.AddKeyedScoped<IAttachmentContentProcessor>("text/plain",
        (services, _) => services.GetRequiredService<PlainTextAttachmentContentProcessor>());
    builder.Services.AddScoped<IValidator<CommentCreationRequest>, CommentCreationRequestValidator>();
    builder.Services.AddScoped<IValidator<CommentsReadingRequest>, CommentsReadingRequestValidator>();
    builder.Services.AddOutputCache(options => options.AddPolicy(
        DiscussionCachePolicies.ReadComments,
        policy => policy.Expire(TimeSpan.FromSeconds(
            builder.Configuration.GetValue<int>($"{CommentCacheOptions.Section}:ExpirationSeconds")
        )).SetVaryByQuery(DiscussionCachePolicies.ReadCommentsQueryKeys)
            .Tag(DiscussionCachePolicies.CommentsTag)
    ));

    var app = builder.Build();

    Comment.ConfigureValidation(
        app.Services.GetRequiredService<IOptions<CommentOptions>>().Value
    );
    Attachment.ConfigureValidation(
        app.Services.GetRequiredService<IOptions<AttachmentOptions>>().Value
    );
    User.ConfigureValidation(app.Services.GetRequiredService<IOptions<UserOptions>>().Value);

    if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
    {
        await DatabaseStartup.WaitUntilReadyAsync(
            sqlConnectionString,
            app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup"),
            app.Lifetime.ApplicationStopping
        );
        await using var scope = app.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await database.Database.MigrateAsync();
    }

    _ = app.Services.GetRequiredService<IConnectionMultiplexer>();

    app.UseSerilogRequestLogging(options =>
    {
        options.GetLevel = static (httpContext, _, exception) =>
            exception is not null || httpContext.Response.StatusCode >= 500
                ? LogEventLevel.Error
                : LogEventLevel.Information;
    });

    app.UseExceptionHandler();

    app.UseStatusCodePages();
    app.UseRouting();
    app.UseCors(CorsSecurityPolicies.SameOrigin);
    app.UseRateLimiter();
    app.UseOutputCache();

    app.MapCaptchaEndpoints();
    app.MapCommentsEndpoints();
    app.MapAttachmentsEndpoints();

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
