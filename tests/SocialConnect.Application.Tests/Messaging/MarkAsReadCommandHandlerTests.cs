using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Messaging.Commands.MarkAsRead;
using SocialConnect.Application.Tests.Users.Commands;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Tests.Messaging;

public class MarkAsReadCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public MarkAsReadCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateLastReadAtAndMarkMessagesAsRead()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = Conversation.CreateOneToOne(currentUserId, otherUserId);
        // Reflect conversationId onto conversation
        typeof(Conversation).GetProperty("Id")?.SetValue(conversation, conversationId);

        // Members list will contain the conversation members
        var membersList = conversation.Members.ToList();
        foreach (var member in membersList)
        {
            typeof(ConversationMember).GetProperty("ConversationId")?.SetValue(member, conversationId);
        }

        var msgFromOther = Message.Create(conversationId, otherUserId, "Hello", MessageType.Text);
        var msgFromSelf = Message.Create(conversationId, currentUserId, "Hey", MessageType.Text);
        var msgOtherConv = Message.Create(Guid.NewGuid(), otherUserId, "Other", MessageType.Text);

        var messagesList = new List<Message> { msgFromOther, msgFromSelf, msgOtherConv };

        var membersDbSet = DbSetMockHelper.CreateMockDbSet(membersList);
        var messagesDbSet = DbSetMockHelper.CreateMockDbSet(messagesList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _contextMock.Setup(x => x.ConversationMembers).Returns(membersDbSet);
        _contextMock.Setup(x => x.Messages).Returns(messagesDbSet);

        var handler = new MarkAsReadCommandHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object
        );

        var command = new MarkAsReadCommand(conversationId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        var myMember = membersList.First(m => m.UserId == currentUserId);
        myMember.LastReadAt.Should().NotBeNull();

        msgFromOther.Status.Should().Be(MessageStatus.Read);
        msgFromSelf.Status.Should().Be(MessageStatus.Sent); // remains untouched
        msgOtherConv.Status.Should().Be(MessageStatus.Sent); // remains untouched

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMemberNotFound_ShouldReturnFailure()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var membersList = new List<ConversationMember>();
        var membersDbSet = DbSetMockHelper.CreateMockDbSet(membersList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _contextMock.Setup(x => x.ConversationMembers).Returns(membersDbSet);

        var handler = new MarkAsReadCommandHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object
        );

        var command = new MarkAsReadCommand(conversationId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Conversation member not found.");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
