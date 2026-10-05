using System;

namespace dZENcode.Forumish.Shared.EntityFramework.Domain;

public interface IHasCreationTime
{
    DateTimeOffset CreatedAt { get; }
}
