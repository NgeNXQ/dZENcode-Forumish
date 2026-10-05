using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using dZENcode.Forumish.Features.Discussion.Domain.Options;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration(
    UserOptions options
) : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedOnAdd();

        builder.Property(user => user.IpHash)
            .IsRequired()
            .HasMaxLength(options.Length)
            .IsFixedLength();

        builder.Property(user => user.FingerprintHash)
            .IsRequired()
            .HasMaxLength(options.Length)
            .IsFixedLength();

        builder.HasIndex(user => new { user.IpHash, user.FingerprintHash })
            .IsUnique();

        builder.HasMany(user => user.Comments)
            .WithOne(comment => comment.Author)
            .HasForeignKey(comment => comment.AuthorId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
