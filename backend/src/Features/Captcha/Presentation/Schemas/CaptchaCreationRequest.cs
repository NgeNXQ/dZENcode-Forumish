namespace dZENcode.Forumish.Features.Captcha.Presentation.Schemas;

internal sealed record class CaptchaCreationRequest(
    int Width,
    int Height
);
