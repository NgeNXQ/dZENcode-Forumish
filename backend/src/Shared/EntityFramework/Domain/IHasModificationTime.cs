using System;

namespace dZENcode.Forumish.Shared.EntityFramework.Domain;

public interface IHasModificationTime
{
    DateTimeOffset UpdatedAt { get; }
}
