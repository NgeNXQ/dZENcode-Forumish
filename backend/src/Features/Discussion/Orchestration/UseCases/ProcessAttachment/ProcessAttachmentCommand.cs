using System;
using Mediator;
using dZENcode.Forumish.Features.Discussion.Domain.Enums;

namespace dZENcode.Forumish.Features.Discussion.Orchestration.UseCases.ProcessAttachment;

internal record class ProcessAttachmentCommand(Guid AttachmentId) : ICommand<AttachmentStatus?>;
