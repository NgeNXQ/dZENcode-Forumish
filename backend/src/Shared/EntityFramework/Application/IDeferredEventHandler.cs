using Mediator;
using dZENcode.Forumish.Shared.EntityFramework.Domain;

namespace dZENcode.Forumish.Shared.EntityFramework.Application;

public interface IDeferredEventHandler<in TEvent>
    : INotificationHandler<TEvent> where TEvent : IDeferredEvent;
