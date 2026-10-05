using System;

namespace dZENcode.Forumish.Shared.EntityFramework.Domain;

public abstract class DomainException(string message) : Exception(message);
