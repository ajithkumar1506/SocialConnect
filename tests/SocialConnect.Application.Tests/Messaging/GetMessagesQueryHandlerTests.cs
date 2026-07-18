using AutoMapper;
using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Mappings;
using SocialConnect.Application.Features.Messaging.Queries.GetMessages;
using SocialConnect.Application.Tests.Users.Commands;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Tests.Messaging;

public class GetMessagesQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly IMapper _mapper;

    public GetMessagesQueryHandlerTests()
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
    public async Task Handle_WithValidMember_ShouldReturnMessages()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        var conversation = Conversation.CreateGroup("Test Group", currentUserId);
        typeof(Conversation).GetProperty("Id")?.SetValue(conversation, conversationId);

        var membersList = conversation.Members.ToList();
        foreach (var member in membersList)
        {
            typeof(ConversationMember).GetProperty("ConversationId")?.SetValue(member, conversationId);
        }

        var msg1 = Message.Create(conversationId, currentUserId, "Hello 1", MessageType.Text);
        var msg2 = Message.Create(conversationId, currentUserId, "Hello 2", MessageType.Text);
        var messagesList = new List<Message> { msg1, msg2 };

        var membersDbSet = DbSetMockHelper.CreateMockDbSet(membersList);
        var messagesDbSet = DbSetMockHelper.CreateMockDbSet(messagesList);

        _contextMock.Setup(x => x.ConversationMembers).Returns(membersDbSet);
        _contextMock.Setup(x => x.Messages).Returns(messagesDbSet);

        var handler = new GetMessagesQueryHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _mapper
        );

        var query = new GetMessagesQuery(conversationId, 1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.ElementAt(0).Content.Should().Be("Hello 1");
        result.Items.ElementAt(1).Content.Should().Be("Hello 2");
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        var membersList = new List<ConversationMember>(); // Current user is not in conversation
        var membersDbSet = DbSetMockHelper.CreateMockDbSet(membersList);

        _contextMock.Setup(x => x.ConversationMembers).Returns(membersDbSet);

        var handler = new GetMessagesQueryHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _mapper
        );

        var query = new GetMessagesQuery(conversationId, 1, 10);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You are not a member of this conversation.");
    }

    [Fact]
    public async Task Handle_WhenUserNotLoggedIn_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        var handler = new GetMessagesQueryHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _mapper
        );

        var query = new GetMessagesQuery(Guid.NewGuid(), 1, 10);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User must be logged in.");
    }
}
