using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using dZENcode.Forumish.Features.Discussion.Domain.Options;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Persistence.Configurations;

internal sealed class CommentConfiguration(
    CommentOptions options
) : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(comment => comment.Id);

        builder.Property(comment => comment.Id)
            .ValueGeneratedOnAdd();

        builder.Property(comment => comment.AuthorId)
            .IsRequired();

        builder.Property(comment => comment.ParentId);

        builder.Ignore(comment => comment.AttachmentId);

        builder.Property(comment => comment.Email)
            .IsRequired()
            .HasMaxLength(options.EmailMaximumLength);

        builder.Property(comment => comment.Username)
            .IsRequired()
            .HasMaxLength(options.UsernameMaximumLength);

        builder.Property(comment => comment.HomePage)
            .HasMaxLength(options.HomePageMaximumLength);

        builder.Property(comment => comment.Message)
            .IsRequired()
            .HasMaxLength(options.MessageMaximumLength);

        builder.Property(comment => comment.CreatedAt)
            .IsRequired();

        builder.HasOne(comment => comment.Parent)
            .WithMany()
            .HasForeignKey(comment => comment.ParentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(comment => comment.ParentId);
        builder.HasIndex(comment => comment.Email);
        builder.HasIndex(comment => comment.Username);
        builder.HasIndex(comment => comment.CreatedAt);
    }
}
