using FluentValidation;

namespace MyProject.WebApi.Application.Features.Authentication.LoginByEmail;

public sealed class LoginByEmailValidator : AbstractValidator<LoginByEmailCommand>
{
    public LoginByEmailValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}