using AutoMapper;
using dZENcode.Forumish.Features.Captcha.Presentation.Schemas;
using dZENcode.Forumish.Features.Captcha.Orchestration.UseCases.CreateCaptcha;

namespace dZENcode.Forumish.Features.Captcha.Presentation.Mapping;

internal sealed class CaptchaMappingsProfile : Profile
{
    public CaptchaMappingsProfile()
    {
        CreateMap<CaptchaCreationRequest, CreateCaptchaCommand>();
    }
}
