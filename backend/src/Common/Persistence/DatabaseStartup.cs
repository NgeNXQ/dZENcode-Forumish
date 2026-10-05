using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace dZENcode.Forumish.Common.Persistence;

internal static class DatabaseStartup
{
    // A connection to master can succeed before user databases finish recovery.
    // EF treats an inaccessible database as missing and attempts CREATE DATABASE.
    internal static async Task WaitUntilReadyAsync(
        string connectionString,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        var settings = new SqlConnectionStringBuilder(connectionString);
        var databaseName = settings.InitialCatalog;
        if (string.IsNullOrWhiteSpace(databaseName))
            throw new InvalidOperationException("The SQL Server connection must specify a database.");

        settings.InitialCatalog = "master";
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromMinutes(2));

        try
        {
            await using var connection = new SqlConnection(settings.ConnectionString);
            await connection.OpenAsync(deadline.Token);
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT CASE
                    WHEN DB_ID(@database) IS NULL THEN 1
                    WHEN EXISTS (
                        SELECT 1 FROM sys.databases WHERE name = @database AND state = 0
                    ) AND HAS_DBACCESS(@database) = 1 THEN 1
                    ELSE 0
                END;
                """;
            command.Parameters.Add("@database", SqlDbType.NVarChar, 128).Value = databaseName;

            var logged = false;
            while (Convert.ToInt32(await command.ExecuteScalarAsync(deadline.Token)) != 1)
            {
                if (!logged)
                {
                    logger.LogInformation("Waiting for database {Database} to finish recovery", databaseName);
                    logged = true;
                }
                await Task.Delay(TimeSpan.FromSeconds(1), deadline.Token);
            }
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"Database '{databaseName}' did not become available within two minutes.", exception
            );
        }
    }
}
