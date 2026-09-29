using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="AuthenticateUserValidator"/> class.
/// Input validation runs in the MediatR pipeline, not in the handler, so the rules are tested here.
/// </summary>
public class AuthenticateUserValidatorTests
{
    private readonly AuthenticateUserValidator _validator = new();

    [Fact(DisplayName = "Valid credentials should pass all validation rules")]
    public void Given_ValidCommand_When_Validated_Then_ShouldNotHaveErrors()
    {
        var command = new AuthenticateUserCommand { Email = "user@example.com", Password = "any" };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Empty command should fail on every required field")]
    public void Given_EmptyCommand_When_Validated_Then_ShouldHaveErrors()
    {
        var command = new AuthenticateUserCommand();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
        result.ShouldHaveValidationErrorFor(c => c.Password);
    }

    [Fact(DisplayName = "Malformed email should fail validation")]
    public void Given_InvalidEmail_When_Validated_Then_ShouldHaveError()
    {
        var command = new AuthenticateUserCommand { Email = "not-an-email", Password = "any" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }
}
