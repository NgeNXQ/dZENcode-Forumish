using System.IO;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.Models;

internal sealed record class AttachmentReadingModel(Stream Content, string ContentType);
