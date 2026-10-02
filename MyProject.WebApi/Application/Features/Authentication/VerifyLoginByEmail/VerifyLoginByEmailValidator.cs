using FluentValidation;

namespace MyProject.WebApi.Application.Features.Authentication.VerifyLoginByEmail;

public sealed class VerifyLoginByEmailValidator : AbstractValidator<VerifyLoginByEmailCommand>
{
    public VerifyLoginByEmailValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6);
    }
}