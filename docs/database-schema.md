# Database Schema

SocialConnect uses SQL Server as its primary relational datastore, managed via Entity Framework Core (Code-First).

The schema is divided into multiple tables corresponding to the Aggregate Roots and Entities in our Domain layer.

## Entity-Relationship Diagram (ERD)

```mermaid
erDiagram
    USERS ||--o{ REFRESH_TOKENS : "has"
    USERS ||--o| USER_PROFILES : "has"
    USERS ||--o{ POSTS : "authors"
    USERS ||--o{ COMMENTS : "writes"
    USERS ||--o{ REACTIONS : "leaves"
    USERS ||--o{ FOLLOWS : "follower/following"
    USERS ||--o{ BLOCKS : "blocker/blocked"
    USERS ||--o{ MUTES : "muter/muted"
    USERS ||--o{ CONVERSATION_MEMBERS : "is member of"
    USERS ||--o{ NOTIFICATIONS : "receives/triggers"

    POSTS ||--o{ POST_MEDIA : "contains"
    POSTS ||--o{ COMMENTS : "has"
    POSTS ||--o{ REACTIONS : "has"

    CONVERSATIONS ||--o{ CONVERSATION_MEMBERS : "has members"
    CONVERSATIONS ||--o{ MESSAGES : "contains"
    MESSAGES ||--o{ MESSAGE_ATTACHMENTS : "has"
    USERS ||--o{ MESSAGES : "sends"

    USERS {
        uniqueidentifier Id PK
        string Email
        string UserName
        string PasswordHash
        datetime CreatedAt
    }

    USER_PROFILES {
        uniqueidentifier Id PK
        uniqueidentifier UserId FK
        string FirstName
        string LastName
        string Bio
        string AvatarUrl
    }

    REFRESH_TOKENS {
        uniqueidentifier Id PK
        uniqueidentifier UserId FK
        string Token
        datetime Expires
    }

    POSTS {
        uniqueidentifier Id PK
        uniqueidentifier UserId FK
        string Content
        string Status "Draft | Published | Deleted"
        datetime CreatedAt
    }

    POST_MEDIA {
        uniqueidentifier Id PK
        uniqueidentifier PostId FK
        string MediaUrl
        string MediaType
    }

    COMMENTS {
        uniqueidentifier Id PK
        uniqueidentifier PostId FK
        string Content
        datetime CreatedAt
    }

    REACTIONS {
        uniqueidentifier Id PK
        uniqueidentifier PostId FK
        string Type "Like | Love | Haha | Wow | Sad | Angry"
    }

    CONVERSATIONS {
        uniqueidentifier Id PK
        string Name
        string Type "OneToOne | Group"
        string ImageUrl
        datetime LastMessageAt
    }

    CONVERSATION_MEMBERS {
        uniqueidentifier ConversationId PK/FK
        uniqueidentifier UserId PK/FK
        string Role "Member | Admin"
        datetime LastReadAt
    }

    MESSAGES {
        uniqueidentifier Id PK
        uniqueidentifier ConversationId FK
        uniqueidentifier SenderId FK
        string Content
        string MessageType "Text | Image | Video | File"
        string Status "Sent | Delivered | Read"
        datetime CreatedAt
    }

    MESSAGE_ATTACHMENTS {
        uniqueidentifier Id PK
        uniqueidentifier MessageId FK
        string FileUrl
        string FileName
        long FileSize
        string ContentType
    }

    NOTIFICATIONS {
        uniqueidentifier Id PK
        uniqueidentifier UserId FK
        uniqueidentifier ActorId FK
        string Type "NewFollower | NewComment | NewReaction | NewMessage | Mention"
        string Content
        bool IsRead
        datetime CreatedAt
    }

    FOLLOWS {
        uniqueidentifier FollowerId PK/FK
        uniqueidentifier FollowingId PK/FK
        datetime CreatedAt
    }

    BLOCKS {
        uniqueidentifier BlockerId PK/FK
        uniqueidentifier BlockedId PK/FK
        datetime CreatedAt
    }

    MUTES {
        uniqueidentifier MuterId PK/FK
        uniqueidentifier MutedId PK/FK
        datetime CreatedAt
    }

    AUDIT_LOGS {
        uniqueidentifier Id PK
        uniqueidentifier UserId FK
        string Action "Added | Modified | Deleted"
        string EntityType
        string EntityId
        string OldValues
        string NewValues
        datetime Timestamp
    }
```

## Auditing

Every table inherits from the `Entity` base class, meaning almost all tables include standard audit fields:
*   `CreatedAt`
*   `CreatedBy`
*   `LastModifiedAt`
*   `LastModifiedBy`

These fields are populated automatically by the `AuditableEntitySaveChangesInterceptor` in the Infrastructure layer before EF Core runs the `SAVE` commands against SQL Server.
