using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Options;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Storages;

internal sealed class AttachmentsPublicStorage : IAttachmentsPublicStorage
{
    private readonly string _directory;
    private readonly ILogger<AttachmentsPublicStorage> _logger;

    public AttachmentsPublicStorage(
        IOptions<AttachmentStorageOptions> attachmentStorageOptions,
        IHostEnvironment environment,
        ILogger<AttachmentsPublicStorage> logger
    )
    {
        _logger = logger;

        _directory = Path.GetFullPath(
            attachmentStorageOptions.Value.PublicPath,
            environment.ContentRootPath
        );

        Directory.CreateDirectory(_directory);
    }

    public async Task<string> SaveAsync(
        Guid id,
        string extension,
        byte[] content,
        CancellationToken token
    )
    {
        if (!_IsValidExtension(extension))
            throw new ArgumentException("The attachment extension is invalid.", nameof(extension));

        var filename = $"{id}.{Guid.NewGuid():N}{extension}";
        var temporaryPath = Path.Combine(_directory, $"{id}.{Guid.NewGuid():N}.tmp");

        try
        {
            await File.WriteAllBytesAsync(temporaryPath, content, token);
            File.Move(temporaryPath, Path.Combine(_directory, filename));
        }
        catch
        {
            try
            {
                File.Delete(temporaryPath);
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException
            )
            {
                _logger.LogWarning(
                    exception,
                    "Could not remove temporary attachment {Path}",
                    temporaryPath
                );
            }

            throw;
        }

        return filename;
    }

    public Stream? OpenRead(string filename)
    {
        if (!_IsValidFilename(filename))
            return null;

        try
        {
            return new FileStream(Path.Combine(_directory, filename), new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read | FileShare.Delete,
            });
        }
        catch (IOException exception) when (
            exception is FileNotFoundException or DirectoryNotFoundException
        )
        {
            return null;
        }
    }

    private static bool _IsValidFilename(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return false;

        if (filename is "." or "..")
            return false;

        if (Path.GetFileName(filename) != filename)
            return false;

        return filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
    }

    public void Delete(string filename)
    {
        if (!_IsValidFilename(filename))
            throw new ArgumentException("The attachment filename is invalid.", nameof(filename));

        try
        {
            File.Delete(Path.Combine(_directory, filename));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Cleanup must not replace the concurrency exception and prevent its retry.
            _logger.LogWarning(exception, "Could not remove unused attachment {Filename}", filename);
        }
    }

    private static bool _IsValidExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return false;

        if (extension.Length < 2)
            return false;

        if (extension[0] != '.')
            return false;

        return extension.Skip(1).All(char.IsAsciiLetterOrDigit);
    }
}
