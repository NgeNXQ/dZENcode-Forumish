using System;

namespace dZENcode.Forumish.Features.Captcha.Orchestration.Models;

internal sealed class CaptchaCreationResult
{
    internal required Guid Id { get; init; }
    internal required byte[] Payload { get; init; }
}
