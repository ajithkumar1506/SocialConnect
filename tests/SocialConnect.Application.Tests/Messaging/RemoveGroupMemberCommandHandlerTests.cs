using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Messaging.Commands.RemoveGroupMember;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Tests.Messaging;

public class RemoveGroupMemberCommandHandlerTests
{
    private readonly Mock<IConversationRepository> _conversationRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public RemoveGroupMemberCommandHandlerTests()
    {
        _conversationRepositoryMock = new Mock<IConversationRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_UserLeavingSelf_ShouldSucceed()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = Conversation.CreateGroup("Test Group", currentUserId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _conversationRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var handler = new RemoveGroupMemberCommandHandler(
            _conversationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new RemoveGroupMemberCommand(conversationId, currentUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        conversation.Members.Should().NotContain(m => m.UserId == currentUserId);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AdminRemovingMember_ShouldSucceed()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = Conversation.CreateGroup("Test Group", adminId);
        conversation.AddMember(memberId, ConversationMemberRole.Member);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(adminId);
        _conversationRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var handler = new RemoveGroupMemberCommandHandler(
            _conversationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new RemoveGroupMemberCommand(conversationId, memberId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        conversation.Members.Should().NotContain(m => m.UserId == memberId);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonAdminRemovingMember_ShouldFail()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var memberId1 = Guid.NewGuid();
        var memberId2 = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = Conversation.CreateGroup("Test Group", adminId);
        conversation.AddMember(memberId1, ConversationMemberRole.Member);
        conversation.AddMember(memberId2, ConversationMemberRole.Member);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(memberId1);
        _conversationRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var handler = new RemoveGroupMemberCommandHandler(
            _conversationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new RemoveGroupMemberCommand(conversationId, memberId2);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Only admins can remove other members from the group.");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldFail()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _conversationRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        var handler = new RemoveGroupMemberCommandHandler(
            _conversationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new RemoveGroupMemberCommand(conversationId, Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Conversation not found.");
    }

    [Fact]
    public async Task Handle_WhenConversationIsOneToOne_ShouldFail()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = Conversation.CreateOneToOne(currentUserId, otherUserId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _conversationRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var handler = new RemoveGroupMemberCommandHandler(
            _conversationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new RemoveGroupMemberCommand(conversationId, otherUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Cannot remove members from a one-to-one conversation.");
    }
}
