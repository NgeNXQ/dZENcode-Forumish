using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace dZENcode.Forumish.Common.Resilience.Redis;

internal static partial class RedisConnectionFactory
{
    internal static IConnectionMultiplexer Create(string connectionString, ILogger logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(logger);

        var options = ConfigurationOptions.Parse(connectionString);

        options.ClientName ??= "dZENcode.Forumish";
        options.AbortOnConnectFail = false;

        var multiplexer = ConnectionMultiplexer.Connect(options);

        multiplexer.ErrorMessage += (_, e) =>
            LogServerError(logger, e.EndPoint?.ToString(), e.Message);

        multiplexer.InternalError += (_, e) =>
            LogInternalError(logger, e.Exception, e.Origin);

        multiplexer.ConnectionFailed += (_, e) =>
            LogConnectionFailed(logger, e.Exception, e.EndPoint?.ToString(), e.FailureType);

        multiplexer.ConnectionRestored += (_, e) =>
            LogConnectionRestored(logger, e.EndPoint?.ToString());

        var endpoints = string.Join(
            ", ",
            multiplexer.GetEndPoints().Select(static endpoint => endpoint.ToString())
        );

        if (multiplexer.IsConnected)
            LogConnected(logger, endpoints);
        else
            LogInitialConnectionPending(logger, endpoints);

        return multiplexer;
    }

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Connected to Redis ({Endpoints})")
    ]
    private static partial void LogConnected(
        ILogger logger,
        string endpoints
    );

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Error,
        Message = "Redis server error. Endpoint: {Endpoint}, Message: {ServerMessage}")
    ]
    private static partial void LogServerError(
        ILogger logger,
        string? endpoint,
        string serverMessage
    );

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Error,
        Message = "Redis internal error. Origin: {Origin}")
    ]
    private static partial void LogInternalError(
        ILogger logger,
        Exception? exception,
        string? origin
    );

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Redis connection failed. Endpoint: {Endpoint}, FailureType: {FailureType}")
    ]
    private static partial void LogConnectionFailed(
        ILogger logger,
        Exception? exception,
        string? endpoint,
        ConnectionFailureType failureType
    );

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "Redis connection restored. Endpoint: {Endpoint}")
    ]
    private static partial void LogConnectionRestored(
        ILogger logger,
        string? endpoint
    );

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Redis is not reachable yet ({Endpoints}); retrying in the background")
    ]
    private static partial void LogInitialConnectionPending(
        ILogger logger,
        string endpoints
    );
}
