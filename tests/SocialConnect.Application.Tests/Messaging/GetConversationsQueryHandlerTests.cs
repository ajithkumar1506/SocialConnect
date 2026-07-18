using AutoMapper;
using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Mappings;
using SocialConnect.Application.Features.Messaging.Queries.GetConversations;
using SocialConnect.Application.Tests.Users.Commands;
using SocialConnect.Domain.Entities.Messaging;

namespace SocialConnect.Application.Tests.Messaging;

public class GetConversationsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly IMapper _mapper;

    public GetConversationsQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_WithValidUser_ShouldReturnConversationsWithLastMessageAndUnreadCount()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        var conversation = Conversation.CreateGroup("Test Group", currentUserId);
        conversation.AddMember(otherUserId);
        typeof(Conversation).GetProperty("Id")?.SetValue(conversation, conversationId);

        // Update members ConversationId
        foreach (var member in conversation.Members)
        {
            typeof(ConversationMember).GetProperty("ConversationId")?.SetValue(member, conversationId);
        }

        var convsList = new List<Conversation> { conversation };
        var convsDbSet = DbSetMockHelper.CreateMockDbSet(convsList);

        // Last message and an unread message
        var lastMsg = Message.Create(conversationId, otherUserId, "Last message", Domain.Enums.MessageType.Text);
        // Make lastMsg created 2 seconds ago
        typeof(Message).GetProperty("CreatedAt")?.SetValue(lastMsg, DateTimeOffset.UtcNow.AddSeconds(-2));

        var unreadMsg = Message.Create(conversationId, otherUserId, "Unread message", Domain.Enums.MessageType.Text);
        typeof(Message).GetProperty("CreatedAt")?.SetValue(unreadMsg, DateTimeOffset.UtcNow);

        var messagesList = new List<Message> { lastMsg, unreadMsg };
        var messagesDbSet = DbSetMockHelper.CreateMockDbSet(messagesList);

        _contextMock.Setup(x => x.Conversations).Returns(convsDbSet);
        _contextMock.Setup(x => x.Messages).Returns(messagesDbSet);

        var handler = new GetConversationsQueryHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _mapper
        );

        var query = new GetConversationsQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Test Group");
        result.Items.First().UnreadCount.Should().Be(2);
        result.Items.First().LastMessage.Should().NotBeNull();
        result.Items.First().LastMessage!.Content.Should().Be("Unread message");
    }

    [Fact]
    public async Task Handle_WhenUserNotLoggedIn_ShouldReturnEmptyList()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        var handler = new GetConversationsQueryHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _mapper
        );

        var query = new GetConversationsQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
