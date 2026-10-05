using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using dZENcode.Forumish.Features.Discussion.Domain.Enums;
using dZENcode.Forumish.Features.Discussion.Domain.Options;
using dZENcode.Forumish.Features.Discussion.Domain.Entities;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Persistence.Configurations;

internal sealed class AttachmentConfiguration(
    AttachmentOptions options
) : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");

        builder.HasKey(attachment => attachment.Id);

        builder.Property(attachment => attachment.Id)
            .ValueGeneratedNever();

        builder.Property(attachment => attachment.CommentId)
            .IsRequired();

        builder.HasOne(attachment => attachment.Comment)
            .WithOne(comment => comment.Attachment)
            .HasForeignKey<Attachment>(attachment => attachment.CommentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(attachment => attachment.Status)
            .IsRequired()
            .IsConcurrencyToken()
            .HasConversion<string>()
            .HasMaxLength(Enum.GetNames<AttachmentStatus>().Max(name => name.Length));

        builder.Property(attachment => attachment.Filetype)
            .IsRequired()
            .HasMaxLength(options.FiletypeMaximumLength);

        builder.Property(attachment => attachment.Filepath)
            .HasMaxLength(options.FilepathMaximumLength);

        builder.Property(attachment => attachment.CreatedAt)
            .IsRequired();

        builder.Property(attachment => attachment.UpdatedAt)
            .IsRequired();
    }
}
