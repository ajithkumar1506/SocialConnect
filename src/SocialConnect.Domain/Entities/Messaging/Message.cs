using SocialConnect.Domain.Common;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Events.Messaging;

namespace SocialConnect.Domain.Entities.Messaging;

public class Message : Entity, IHasTimestamps, ISoftDeletable
{
    public Guid ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public MessageType MessageType { get; private set; }
    public MessageStatus Status { get; private set; }
    public Guid? ParentMessageId { get; private set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Navigation
    public Conversation? Conversation { get; private set; }
    public User? Sender { get; private set; }
    public Message? ParentMessage { get; private set; }

    private readonly List<MessageAttachment> _attachments = new();
    public IReadOnlyCollection<MessageAttachment> Attachments => _attachments.AsReadOnly();

    private Message() { }

    public static Message Create(
        Guid conversationId,
        Guid senderId,
        string content,
        MessageType type,
        Guid? parentId = null
    )
    {
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content,
            MessageType = type,
            Status = MessageStatus.Sent,
            ParentMessageId = parentId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        message.AddDomainEvent(new MessageSentDomainEvent(message.Id));

        return message;
    }

    public void Edit(string newContent)
    {
        Content = newContent;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddAttachment(
        string fileUrl,
        string fileName,
        long fileSize,
        string contentType,
        string? thumbnailUrl = null
    )
    {
        _attachments.Add(
            MessageAttachment.Create(Id, fileUrl, fileName, fileSize, contentType, thumbnailUrl)
        );
    }

    public void MarkAsDelivered()
    {
        if (Status == MessageStatus.Sent)
        {
            Status = MessageStatus.Delivered;
        }
    }

    public void MarkAsRead()
    {
        Status = MessageStatus.Read;
    }
}
