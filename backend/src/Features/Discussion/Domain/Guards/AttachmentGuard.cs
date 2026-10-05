using System;
using dZENcode.Forumish.Shared.EntityFramework.Domain;
using dZENcode.Forumish.Features.Discussion.Domain.Enums;
using dZENcode.Forumish.Features.Discussion.Domain.Options;

namespace dZENcode.Forumish.Features.Discussion.Domain.Entities;

internal sealed partial class Attachment
{
    private static AttachmentGuard? _guard;

    private static AttachmentGuard Guard => _guard
        ?? throw new InvalidOperationException("Attachment validation has not been configured.");

    internal static void ConfigureValidation(AttachmentOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _guard = new AttachmentGuard(options);
    }

    private sealed class AttachmentGuard(AttachmentOptions options)
    {
        internal void ValidatePending(Guid id, string filetype)
        {
            if (id == Guid.Empty)
                throw new DomainInvariantException("Attachment ID must not be empty.");

            _ValidateFiletype(filetype);
        }

        internal void ValidateComment(Comment? current, Comment comment)
        {
            ArgumentNullException.ThrowIfNull(comment);

            if (current is not null)
                throw new DomainInvariantException("Attachment already belongs to a comment.");
        }

        internal void ValidateProcessed(string filetype, string filepath)
        {
            _ValidateFiletype(filetype);
            _ValidateFilepath(filepath);
        }

        internal void EnsurePending(Guid id, AttachmentStatus status)
        {
            if (status != AttachmentStatus.Pending)
            {
                throw new DomainInvariantException(
                    $"Attachment {id} has already been finalized with status {status}."
                );
            }
        }

        private void _ValidateFiletype(string filetype)
        {
            if (string.IsNullOrWhiteSpace(filetype))
                throw new DomainInvariantException("Attachment file type must not be empty.");

            if (filetype.Length > options.FiletypeMaximumLength)
            {
                throw new DomainInvariantException(
                    "Attachment file type exceeds the configured limit."
                );
            }
        }

        private void _ValidateFilepath(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
                throw new DomainInvariantException("Attachment file path must not be empty.");

            if (filepath.Length > options.FilepathMaximumLength)
            {
                throw new DomainInvariantException(
                    "Attachment file path exceeds the configured limit."
                );
            }
        }
    }
}
