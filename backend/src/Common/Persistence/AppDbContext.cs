using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using dZENcode.Forumish.Features.Discussion.Domain.Options;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Persistence.Configurations;

namespace dZENcode.Forumish.Common.Persistence;

internal sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IOptions<UserOptions> userOptions,
    IOptions<CommentOptions> commentOptions,
    IOptions<AttachmentOptions> attachmentOptions
) : DbContext(options)
{
    internal DbSet<User> Users => Set<User>();
    internal DbSet<Comment> Comments => Set<Comment>();
    internal DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(
            new UserConfiguration(userOptions.Value)
        );
        modelBuilder.ApplyConfiguration(
            new CommentConfiguration(commentOptions.Value)
        );
        modelBuilder.ApplyConfiguration(
            new AttachmentConfiguration(attachmentOptions.Value)
        );
    }
}
