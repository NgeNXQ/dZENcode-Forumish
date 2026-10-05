using System;
using Mediator;
using dZENcode.Forumish.Features.Discussion.Orchestration.Models;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.UseCases.ReadAttachment;

internal sealed record class ReadAttachmentQuery(Guid AttachmentId)
    : IQuery<AttachmentReadingModel?>;
