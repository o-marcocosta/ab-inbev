using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Unit.Domain;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CreateUserCommandValidator"/> class.
/// Input validation runs in the MediatR pipeline, not in the handler, so the rules are tested here.
/// </summary>
public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact(DisplayName = "Valid command should pass all validation rules")]
    public void Given_ValidCommand_When_Validated_Then_ShouldNotHaveErrors()
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Empty command should fail on every required field")]
    public void Given_EmptyCommand_When_Validated_Then_ShouldHaveErrors()
    {
        var command = new CreateUserCommand();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
        result.ShouldHaveValidationErrorFor(c => c.Username);
        result.ShouldHaveValidationErrorFor(c => c.Password);
        result.ShouldHaveValidationErrorFor(c => c.Phone);
        result.ShouldHaveValidationErrorFor(c => c.Status);
        result.ShouldHaveValidationErrorFor(c => c.Role);
    }

    [Theory(DisplayName = "Undefined status or role should fail validation")]
    [InlineData(UserStatus.Unknown, UserRole.Customer)]
    [InlineData(UserStatus.Active, UserRole.None)]
    public void Given_UndefinedStatusOrRole_When_Validated_Then_ShouldHaveError(UserStatus status, UserRole role)
    {
        var command = CreateUserHandlerTestData.GenerateValidCommand();
        command.Status = status;
        command.Role = role;

        var result = _validator.TestValidate(command);

        result.ShouldHaveAnyValidationError();
    }
}
