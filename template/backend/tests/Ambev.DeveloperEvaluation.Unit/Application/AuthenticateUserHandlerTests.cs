using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="AuthenticateUserHandler"/> class.
/// </summary>
public class AuthenticateUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly AuthenticateUserHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticateUserHandlerTests"/> class.
    /// Uses the real AutoMapper profile so the User to AuthenticateUserResult mapping is exercised too.
    /// </summary>
    public AuthenticateUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<AuthenticateUserProfile>()).CreateMapper();
        _handler = new AuthenticateUserHandler(_userRepository, _passwordHasher, _jwtTokenGenerator, mapper);
    }

    [Fact(DisplayName = "Given valid credentials of an active user When authenticating Then returns token and user data")]
    public async Task Handle_ValidCredentials_ReturnsTokenAndUser()
    {
        // Given
        var user = GivenStoredUser(UserStatus.Active);
        _passwordHasher.VerifyPassword("secret", user.Password).Returns(true);
        _jwtTokenGenerator.GenerateToken(user).Returns("jwt-token");

        // When
        var result = await _handler.Handle(new AuthenticateUserCommand { Email = user.Email, Password = "secret" }, CancellationToken.None);

        // Then
        result.Token.Should().Be("jwt-token");
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.Username.Should().Be(user.Username);
        result.Role.Should().Be(user.Role.ToString());
    }

    [Fact(DisplayName = "Given an unknown email When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_UnknownEmail_ThrowsUnauthorized()
    {
        // Given
        _userRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        // When
        var act = () => _handler.Handle(new AuthenticateUserCommand { Email = "nobody@example.com", Password = "secret" }, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid credentials");
    }

    [Fact(DisplayName = "Given a wrong password When authenticating Then throws UnauthorizedAccessException")]
    public async Task Handle_WrongPassword_ThrowsUnauthorized()
    {
        // Given
        var user = GivenStoredUser(UserStatus.Active);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // When
        var act = () => _handler.Handle(new AuthenticateUserCommand { Email = user.Email, Password = "wrong" }, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid credentials");
        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<IUser>());
    }

    [Theory(DisplayName = "Given a user that is not active When authenticating Then throws UnauthorizedAccessException")]
    [InlineData(UserStatus.Inactive)]
    [InlineData(UserStatus.Suspended)]
    public async Task Handle_InactiveUser_ThrowsUnauthorized(UserStatus status)
    {
        // Given
        var user = GivenStoredUser(status);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        // When
        var act = () => _handler.Handle(new AuthenticateUserCommand { Email = user.Email, Password = "secret" }, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User is not active");
        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<IUser>());
    }

    private User GivenStoredUser(UserStatus status)
    {
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();
        user.Status = status;
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }
}
