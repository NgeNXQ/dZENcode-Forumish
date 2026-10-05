using dZENcode.Forumish.Shared.EntityFramework.Domain;

namespace dZENcode.Forumish.Features.Discussion.Domain.Events;

internal sealed record class CommentCreatedDeferredEvent : IDeferredEvent;
