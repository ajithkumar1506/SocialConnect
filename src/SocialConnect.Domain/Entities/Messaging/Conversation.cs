using SocialConnect.Domain.Common;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Exceptions;

namespace SocialConnect.Domain.Entities.Messaging;

public class Conversation : AggregateRoot, IHasTimestamps
{
    public ConversationType Type { get; private set; }
    public string? Name { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateTimeOffset? LastMessageAt { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private readonly List<ConversationMember> _members = new();
    public IReadOnlyCollection<ConversationMember> Members => _members.AsReadOnly();

    private readonly List<Message> _messages = new();
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    private Conversation() { }

    public static Conversation CreateOneToOne(Guid user1Id, Guid user2Id)
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Type = ConversationType.OneToOne,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        conversation.AddMember(user1Id, ConversationMemberRole.Member);
        conversation.AddMember(user2Id, ConversationMemberRole.Member);

        return conversation;
    }

    public static Conversation CreateGroup(string name, Guid creatorId, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleViolationException("Group conversation must have a name.");
        }

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Type = ConversationType.Group,
            Name = name,
            ImageUrl = imageUrl,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        conversation.AddMember(creatorId, ConversationMemberRole.Admin);

        return conversation;
    }

    public void AddMember(Guid userId, ConversationMemberRole role = ConversationMemberRole.Member)
    {
        if (_members.Any(m => m.UserId == userId))
        {
            return;
        }

        _members.Add(ConversationMember.Create(Id, userId, role));
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
        {
            _members.Remove(member);
        }
    }

    public void SetLastMessageAt(DateTimeOffset timestamp)
    {
        LastMessageAt = timestamp;
    }

    public void UpdateGroupDetails(string name, string? imageUrl)
    {
        if (Type != ConversationType.Group)
        {
            throw new BusinessRuleViolationException(
                "Can only update details for a group conversation."
            );
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleViolationException("Group conversation must have a name.");
        }

        Name = name;
        ImageUrl = imageUrl;
    }
}
