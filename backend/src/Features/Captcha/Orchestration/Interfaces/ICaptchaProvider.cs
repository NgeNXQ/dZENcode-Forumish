namespace dZENcode.Forumish.Features.Captcha.Orchestration.Interfaces;

internal interface ICaptchaProvider
{
    (string code, byte[] payload) Generate(int width, int height);
}
