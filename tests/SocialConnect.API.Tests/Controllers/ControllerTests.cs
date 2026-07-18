using Moq;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.API.Controllers;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Auth.Commands.Login;
using SocialConnect.Application.Features.Auth.Commands.Register;

namespace SocialConnect.API.Tests.Controllers;

public class ControllerTests
{
    private readonly Mock<ISender> _mediatorMock;
    private readonly AuthController _authController;

    public ControllerTests()
    {
        _mediatorMock = new Mock<ISender>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(ISender))).Returns(_mediatorMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.RequestServices).Returns(serviceProviderMock.Object);

        _authController = new AuthController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            }
        };
    }

    [Fact]
    public async Task Register_WithValidCommand_ShouldReturnOk()
    {
        // Arrange
        var command = new RegisterCommand("new@example.com", "Password123!", "new_user", "John", "Doe");
        var authResult = new AuthResult { Token = "access_token", RefreshToken = "refresh_token" };

        _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResult);

        // Act
        var actionResult = await _authController.Register(command);

        // Assert
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<AuthResult>>().Subject;

        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be(authResult);
        apiResponse.Message.Should().Be("User registered successfully.");
    }

    [Fact]
    public async Task Login_WithValidCommand_ShouldReturnOk()
    {
        // Arrange
        var command = new LoginCommand("login@example.com", "Password123!");
        var authResult = new AuthResult { Token = "access_token", RefreshToken = "refresh_token" };

        _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResult);

        // Act
        var actionResult = await _authController.Login(command);

        // Assert
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<AuthResult>>().Subject;

        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be(authResult);
        apiResponse.Message.Should().Be("Login successful.");
    }
}
