using System;

namespace dZENcode.Forumish.Features.Captcha.Presentation.Schemas;

internal sealed record class CaptchaCreationResponse(
    Guid Id,
    byte[] Payload
);
