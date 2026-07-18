using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Messaging.Commands.CreateConversation;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Repositories;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Messaging;

public class CreateConversationCommandHandlerTests
{
    private readonly Mock<IConversationRepository> _conversationRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public CreateConversationCommandHandlerTests()
    {
        _conversationRepoMock = new Mock<IConversationRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_WithValidRecipient_ShouldCreateNewConversation()
    {
        var currentUserId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var recipient = User.Create(Email.Create("recipient@example.com"), "recipient", Password.Create("hash"));

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _userRepoMock.Setup(x => x.GetByIdAsync(recipientId, It.IsAny<CancellationToken>())).ReturnsAsync(recipient);
        _conversationRepoMock.Setup(x => x.GetOneToOneConversationAsync(currentUserId, recipientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        var handler = new CreateConversationCommandHandler(
            _conversationRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var result = await handler.Handle(new CreateConversationCommand(recipientId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _conversationRepoMock.Verify(x => x.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
