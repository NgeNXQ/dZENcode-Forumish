using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using dZENcode.Forumish.Features.Discussion.Infrastructure.Options;
using dZENcode.Forumish.Features.Discussion.Orchestration.Interfaces;

namespace dZENcode.Forumish.Features.Discussion.Infrastructure.Storages;

internal sealed class AttachmentsStagingStorage : IAttachmentsStagingStorage
{
    private readonly int _bufferSize;
    private readonly string _directoryRoot;

    public AttachmentsStagingStorage(
        IOptions<AttachmentStorageOptions> attachmentStorageOptions,
        IHostEnvironment environment
    )
    {
        var options = attachmentStorageOptions.Value;

        _bufferSize = options.BufferSize;
        _directoryRoot = Path.GetFullPath(options.StagingPath, environment.ContentRootPath);

        Directory.CreateDirectory(_directoryRoot);
    }

    public async Task SaveAsync(
        Guid id,
        Stream content,
        long maximumLength,
        CancellationToken token
    )
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumLength);

        await using var destination = new FileStream(_CombinePath(id), new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
            Share = FileShare.None,
            BufferSize = _bufferSize,
            Options = FileOptions.Asynchronous,
        });

        var bytesWritten = await _CopyWithSizeLimitAsync(content, destination, maximumLength, token);

        if (bytesWritten == 0)
            throw new InvalidDataException("Attachment must not be empty.");
    }

    public Stream? OpenRead(Guid id)
    {
        var path = _CombinePath(id);

        try
        {
            return new FileStream(path, new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read | FileShare.Delete,
                BufferSize = _bufferSize,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            });
        }
        catch (IOException exception) when (
            exception is FileNotFoundException or DirectoryNotFoundException
        )
        {
            return null;
        }
    }

    public void Delete(Guid id)
    {
        File.Delete(_CombinePath(id));
    }

    private async Task<long> _CopyWithSizeLimitAsync(
        Stream source,
        Stream destination,
        long maximumLength,
        CancellationToken token
    )
    {
        var buffer = new byte[_bufferSize];
        long bytesWritten = 0;

        while (true)
        {
            var bytesRead = await source.ReadAsync(buffer.AsMemory(), token);
            if (bytesRead == 0)
                return bytesWritten;

            var remainingCapacity = maximumLength - bytesWritten;
            if (bytesRead > remainingCapacity)
                throw new InvalidDataException("Attachment exceeds the permitted size.");

            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), token);
            bytesWritten += bytesRead;
        }
    }

    private string _CombinePath(Guid id)
    {
        return Path.Combine(_directoryRoot, id.ToString());
    }
}
