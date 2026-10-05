using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using dZENcode.Forumish.Features.Discussion.Domain.Options;

namespace dZENcode.Forumish.Common.Persistence;

internal sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = WebApplication.CreateSlimBuilder(args).Configuration;

        var connectionString = configuration.GetConnectionString("SqlServer");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:SqlServer is not configured.");

        return new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(
                    connectionString
                ).Options,
                Options.Create(
                    configuration.GetRequiredSection(
                        UserOptions.Section
                    ).Get<UserOptions>() ?? throw new InvalidOperationException(
                        $"{nameof(UserOptions)} are missing."
                    )
                ),
                Options.Create(
                    configuration.GetRequiredSection(
                        CommentOptions.Section
                    ).Get<CommentOptions>() ?? throw new InvalidOperationException(
                        $"{nameof(CommentOptions)} are missing."
                    )
                ),
                Options.Create(
                    configuration.GetRequiredSection(
                        AttachmentOptions.Section
                    ).Get<AttachmentOptions>() ?? throw new InvalidOperationException(
                        $"{nameof(AttachmentOptions)} are missing."
                    )
                )
        );
    }
}
