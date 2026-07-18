using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using SocialConnect.Application.Common.Behaviors;

namespace SocialConnect.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    public class TestCommand : IRequest<string> { }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallNextDelegate()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestCommand>>();
        var behavior = new ValidationBehavior<TestCommand, string>(validators);
        var request = new TestCommand();
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var mockValidator = new Moq.Mock<IValidator<TestCommand>>();
        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Prop", "Error") }));

        var validators = new[] { mockValidator.Object };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);
        var request = new TestCommand();
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

        // Act
        Func<Task> action = async () => await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<SocialConnect.Application.Common.Exceptions.ValidationException>()
            .Where(e => e.Errors.ContainsKey("Prop"));
    }
}
