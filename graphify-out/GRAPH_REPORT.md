# Graph Report - SocialConnect  (2026-10-07)

## Corpus Check
- cluster-only mode — file stats not available

## Summary
- 2987 nodes · 6964 edges · 172 communities (99 shown, 73 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 545 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `00d236a8`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- mediatr
- microsoft_entityframeworkcore
- Conversation
- SocialConnect.Application.Common.Interfaces
- IFileStorageService
- Follow
- fluentvalidation
- Result
- Program.cs
- IRequestHandler
- UserProfileDto
- SocialConnect.Infrastructure.Persistence
- AuthResult
- Post
- ApplicationDbContext
- IntegrationTestWebAppFactory
- .Handle_WithInvalidRequest_ShouldThrowValidationException
- FollowUserDto
- ApiResponse
- .Create
- NotificationDto
- User
- CreatePostCommand
- EmailSendingJob
- PostDto
- UserDto
- .Handle
- Message
- Comment
- PostMedia
- MessagesController.cs
- ForgotPasswordCommandHandler
- microsoft_entityframeworkcore_infrastructure
- ConversationMember
- IDomainEvent
- IApplicationDbContext
- EditMessageCommandHandler
- .SaveChangesAsync
- IHasTimestamps
- RefreshToken
- UserProfile
- SocialConnect.Domain.Entities.Users
- CleanupOrphanUploadsJob
- MessageDto
- RabbitMqEventBus
- .Handle
- SocialConnect.Application
- TokenService
- IPostRepository
- IRequest
- BusinessRuleViolationException
- Notification
- .Create
- PostsController.cs
- CacheService
- http
- GetMessagesQueryHandler
- UpdateProfileCommand
- INotificationRepository
- .Ok
- SocialConnect.Application.Features.Users.DTOs
- SocialController
- .SaveChangesAsync
- ServicesTests
- CreateGroupChatCommand
- Entity
- AuditLog
- .Create
- SocialConnect.Infrastructure.Migrations
- SocialConnect.Infrastructure
- UpdateCommentCommand
- CommentDto
- DeleteMessageCommandHandler
- ConversationMemberDto
- SchedulePostCommand
- .Create
- MessageAttachment
- 20260531070113_Initial_WorkExperience.Designer.cs
- 20260531070124_Initial_Education.Designer.cs
- 20260606064106_RemoveProfessionalEntities.Designer.cs
- IMapFrom
- .CreateMockDbSet
- microsoft_entityframeworkcore_migrations
- .GetPostComments
- ICurrentUserService
- DeleteCommentCommand
- MarkAsReadCommandHandler
- MessageAttachmentDto
- GetConversationsQueryHandler
- GetUserNotificationsQueryHandler
- PublishPostCommand
- ReactionDto
- .Handle_WithValidRequest_ShouldCreateMuteAndReturnSuccess
- .GetConversationMessagesAsync
- UserRepository
- .ExecuteAsync_ShouldCleanUpExpiredTokens
- 20260531070103_Initial_Skill.Designer.cs
- 20260531070146_Initial_PostMedia.Designer.cs
- 20260531070156_Initial_Comment.Designer.cs
- 20260531070207_Initial_Reaction.Designer.cs
- 20260531070217_Initial_Follow.Designer.cs
- 20260531070227_Initial_Block.Designer.cs
- 20260531070238_Initial_Mute.Designer.cs
- 20260531070248_Initial_Conversation.Designer.cs
- 20260531070259_Initial_ConversationMember.Designer.cs
- 20260531070309_Initial_Message.Designer.cs
- 20260531070324_Initial_MessageAttachment.Designer.cs
- 20260531070336_Initial_Notification.Designer.cs
- Repository
- ApiControllerBase
- TestAsyncEnumerable
- Migration
- .Handle
- .Handle
- .Handle
- ConversationDto
- Mute
- ScheduledPostPublishJob
- SqliteTestMigrator
- .Handle
- ValidationBehaviorTests.cs
- SocialConnect.API.Tests
- PaginatedList
- GetFeedQuery
- GetUserPostsQuery
- Reaction
- TargetType
- AuditLoggingInterceptor
- PostsIntegrationTests
- TokenService.cs
- Exception
- PresenceHub
- EmailTests
- SocialConnect.API
- .GetReactions
- GetPostCommentsQuery
- ICommentRepository
- ThumbnailGenerationJob
- .MessagingFullLifecycle_ShouldSucceed
- SocialConnect.Domain.Tests
- TestAsyncEnumerator
- TestAsyncEnumerator
- .Create_WithEmptyHash_ShouldThrowArgumentException
- BackgroundJobsRegistrationHostedService
- IIdentityService
- AddCommentCommandHandler
- GetReactionsQuery
- NotificationType
- SocialConnect.Infrastructure.Tests
- DesignTimeCurrentUserService
- CommentAdapter
- LoggingBehavior
- Email
- ChatHub
- CursorPaginatedList
- ValueObject
- MessageType
- PostStatus
- PostType
- ReactionType
- ForgotPasswordCommandHandler.cs
- MessageStatus
- JwtAuthenticationConfigurator
- ChangeTrackerExtensions

## God Nodes (most connected - your core abstractions)
1. `SocialConnect.Application.Common.Interfaces` - 125 edges
2. `Result` - 109 edges
3. `SocialConnect.Application.Common.Models` - 108 edges
4. `IApplicationDbContext` - 98 edges
5. `ApiResponse` - 80 edges
6. `ICurrentUserService` - 65 edges
7. `SocialConnect.Domain.Repositories` - 62 edges
8. `SocialConnect.Domain.Enums` - 61 edges
9. `User` - 59 edges
10. `Post` - 55 edges

## Surprising Connections (you probably didn't know these)
- `ChangePasswordCommandHandlerTests` --references--> `IApplicationDbContext`  [EXTRACTED]
  tests/SocialConnect.Application.Tests/Users/Commands/ChangePasswordCommandHandlerTests.cs → src/SocialConnect.Application/Common/Interfaces/IApplicationDbContext.cs
- `ChangePasswordCommandHandlerTests` --references--> `ICurrentUserService`  [EXTRACTED]
  tests/SocialConnect.Application.Tests/Users/Commands/ChangePasswordCommandHandlerTests.cs → src/SocialConnect.Application/Common/Interfaces/ICurrentUserService.cs
- `ChangePasswordCommandHandlerTests` --references--> `IPasswordHasher`  [EXTRACTED]
  tests/SocialConnect.Application.Tests/Users/Commands/ChangePasswordCommandHandlerTests.cs → src/SocialConnect.Application/Common/Interfaces/IPasswordHasher.cs
- `RevokeTokenCommandHandlerTests` --references--> `IApplicationDbContext`  [EXTRACTED]
  tests/SocialConnect.Application.Tests/Users/Commands/RevokeTokenCommandHandlerTests.cs → src/SocialConnect.Application/Common/Interfaces/IApplicationDbContext.cs
- `VerifyEmailCommandHandlerTests` --references--> `IApplicationDbContext`  [EXTRACTED]
  tests/SocialConnect.Application.Tests/Users/Commands/VerifyEmailCommandHandlerTests.cs → src/SocialConnect.Application/Common/Interfaces/IApplicationDbContext.cs

## Import Cycles
- None detected.

## Communities (172 total, 73 thin omitted)

### Community 0 - "mediatr"
Cohesion: 0.04
Nodes (34): SocialConnect.Application.Features.Auth.Commands.ResetPassword, SocialConnect.Application.Features.Comments.Commands.AddComment, SocialConnect.Application.Features.Posts.Commands.DeletePost, SocialConnect.Application.Features.Comments.Commands.UpdateComment, SocialConnect.Application.Features.Notifications.Commands.MarkAllNotificationsRead, SocialConnect.Application.Features.Auth.Commands.Logout, SocialConnect.Application.Features.Messaging.Commands.AddGroupMember, SocialConnect.Application.Features.Messaging.Commands.DeleteMessage (+26 more)

### Community 1 - "microsoft_entityframeworkcore"
Cohesion: 0.05
Nodes (13): SocialConnect.Domain.Entities.Notifications, SocialConnect.Domain.Entities.Social, SocialConnect.Domain.Entities.Audit, SocialConnect.Infrastructure.Persistence.Interceptors, SocialConnect.Infrastructure.Services, SocialConnect.Infrastructure.Persistence.Repositories, SocialConnect.Application.Tests.Notifications, SocialConnect.Infrastructure.BackgroundJobs (+5 more)

### Community 2 - "Conversation"
Cohesion: 0.05
Nodes (22): AddGroupMemberCommand, AddGroupMemberCommandHandler, RemoveGroupMemberCommand, RemoveGroupMemberCommandHandler, Conversation, CreatedAt, ImageUrl, LastMessageAt (+14 more)

### Community 3 - "SocialConnect.Application.Common.Interfaces"
Cohesion: 0.08
Nodes (20): SocialConnect.Application.Tests.Posts.Queries, SocialConnect.Domain.Tests.ValueObjects, SocialConnect.Domain.Tests.Entities, SocialConnect.Domain.Exceptions, SocialConnect.Application.Tests.Users.Commands, SocialConnect.Domain.Entities.Messaging, SocialConnect.Application.Tests.BackgroundJobs, SocialConnect.Application.Tests.Comments.Commands (+12 more)

### Community 4 - "IFileStorageService"
Cohesion: 0.05
Nodes (14): SocialConnect.Application.Common.Helpers, ImageMetadataReader, IFileStorageService, UploadPostMediaCommand, UploadPostMediaCommandHandler, MediaItemDto, UploadCoverImageCommand, UploadCoverImageCommandHandler (+6 more)

### Community 5 - "Follow"
Cohesion: 0.06
Nodes (19): BlockUserCommand, BlockUserCommandHandler, Block, Blocked, BlockedId, Blocker, BlockerId, CreatedAt (+11 more)

### Community 6 - "fluentvalidation"
Cohesion: 0.06
Nodes (22): ChangePasswordCommandValidator, LoginCommandValidator, RefreshTokenCommand, RefreshTokenCommandValidator, RegisterCommandValidator, ResetPasswordCommand, ResetPasswordCommandValidator, RevokeTokenCommandValidator (+14 more)

### Community 7 - "Result"
Cohesion: 0.07
Nodes (18): Result, Error, IsSuccess, Result, Error, IsSuccess, Value, MarkAllNotificationsReadCommand (+10 more)

### Community 8 - "Program.cs"
Cohesion: 0.05
Nodes (12): SocialConnect.Infrastructure, SocialConnect.API.Configuration, SocialConnect.API.Services, SocialConnect.API.Infrastructure.ExceptionHandling, DatabaseMigrationConfigurator, EnvironmentConfigurator, SwaggerDocumentationConfigurator, GlobalExceptionHandler (+4 more)

### Community 9 - "IRequestHandler"
Cohesion: 0.10
Nodes (19): IDateTime, Now, IPasswordHasher, ITokenService, LoginCommandHandler, RefreshTokenCommandHandler, RegisterCommandHandler, ResetPasswordCommandHandler (+11 more)

### Community 10 - "UserProfileDto"
Cohesion: 0.11
Nodes (13): UsersController, UserProfileDto, Bio, CoverImageUrl, DateOfBirth, FirstName, Headline, LastName (+5 more)

### Community 11 - "SocialConnect.Infrastructure.Persistence"
Cohesion: 0.16
Nodes (9): SocialConnect.Application.Features.Posts.Commands.CreatePost, SocialConnect.Application.Tests.Auth, SocialConnect.Infrastructure.Persistence, SocialConnect.Application.Features.Auth.Commands.Register, SocialConnect.API.Tests.Controllers, SocialConnect.Application.Features.Auth.Commands.Login, SocialConnect.API.Tests, SocialConnect.API.Controllers (+1 more)

### Community 12 - "AuthResult"
Cohesion: 0.09
Nodes (7): AuthResult, RefreshToken, RefreshTokenExpiration, Token, LoginCommand, AuthIntegrationTests, ControllerTests

### Community 13 - "Post"
Cohesion: 0.07
Nodes (19): PostAdapter, Post, Author, AuthorId, CommentCount, Comments, Content, CreatedAt (+11 more)

### Community 14 - "ApplicationDbContext"
Cohesion: 0.07
Nodes (21): ApplicationDbContext, AuditLogs, Blocks, Comments, ConversationMembers, Conversations, Follows, MessageAttachments (+13 more)

### Community 15 - "IntegrationTestWebAppFactory"
Cohesion: 0.11
Nodes (6): Program, AuditLoggingIntegrationTests, CommentsIntegrationTests, IntegrationTestWebAppFactory, SmokeTests, UsersIntegrationTests

### Community 16 - ".Handle_WithInvalidRequest_ShouldThrowValidationException"
Cohesion: 0.08
Nodes (5): PerformanceBehavior, UnhandledExceptionBehavior, ValidationBehavior, TestCommand, ValidationBehaviorTests

### Community 17 - "FollowUserDto"
Cohesion: 0.10
Nodes (13): SocialConnect.Application.Features.Social.DTOs, SocialConnect.Application.Features.Social.Queries.GetFollowers, FollowUserDto, FirstName, Headline, LastName, ProfileImageUrl, UserId (+5 more)

### Community 18 - "ApiResponse"
Cohesion: 0.17
Nodes (6): MessagesController, ApiResponse, Data, Errors, Message, Success

### Community 19 - ".Create"
Cohesion: 0.18
Nodes (3): UserRegisteredDomainEvent, UserId, UserTests

### Community 20 - "NotificationDto"
Cohesion: 0.08
Nodes (14): NotificationService, NotificationProcessingJob, INotificationService, NotificationDto, ActorId, ActorName, ActorProfileImageUrl, Content (+6 more)

### Community 21 - "User"
Cohesion: 0.08
Nodes (16): User, AccessFailedCount, CreatedAt, Email, EmailVerified, IsActive, LockoutEnd, NormalizedEmail (+8 more)

### Community 22 - "CreatePostCommand"
Cohesion: 0.17
Nodes (4): SocialConnect.API.Models, PostsController, UpdatePostRequest, CreatePostCommand

### Community 23 - "EmailSendingJob"
Cohesion: 0.15
Nodes (4): EmailSendingJob, IEmailService, EmailService, BackgroundJobTests

### Community 24 - "PostDto"
Cohesion: 0.11
Nodes (12): PostDto, AuthorId, CommentCount, Content, Id, PostType, PublishedAt, ReactionCount (+4 more)

### Community 25 - "UserDto"
Cohesion: 0.11
Nodes (11): UserDto, CreatedAt, Email, EmailVerified, Id, IsActive, Profile, UserName (+3 more)

### Community 26 - ".Handle"
Cohesion: 0.13
Nodes (6): ToggleReactionCommand, IReactionCountable, ToggleReactionCommandHandler, IReactionRepository, IRepository, ReactionRepository

### Community 27 - "Message"
Cohesion: 0.08
Nodes (16): Message, Attachments, Content, Conversation, ConversationId, CreatedAt, DeletedAt, IsDeleted (+8 more)

### Community 28 - "Comment"
Cohesion: 0.08
Nodes (18): Comment, Author, AuthorId, Content, CreatedAt, DeletedAt, Depth, IsDeleted (+10 more)

### Community 29 - "PostMedia"
Cohesion: 0.10
Nodes (15): PostMedia, CreatedAt, FileSize, MediaType, MediaUrl, OrderIndex, Post, PostId (+7 more)

### Community 30 - "MessagesController.cs"
Cohesion: 0.12
Nodes (6): SocialConnect.API.Hubs, SocialConnect.Application.Features.Messaging.Commands.MarkAsRead, SocialConnect.Application.Features.Reactions.DTOs, SocialConnect.Application.Features.Notifications.DTOs, SocialConnect.Application.Features.Notifications.Queries.GetUserNotifications, SocialConnect.Application.Features.Reactions.Queries.GetReactions

### Community 31 - "ForgotPasswordCommandHandler"
Cohesion: 0.10
Nodes (8): IBackgroundJobService, ClientSettings, ResetPasswordUrl, ForgotPasswordCommand, ForgotPasswordCommandHandler, ForgotPasswordCommandValidator, BackgroundJobService, ForgotPasswordCommandHandlerTests

### Community 32 - "microsoft_entityframeworkcore_infrastructure"
Cohesion: 0.12
Nodes (3): Initial_UserProfile, Initial_RefreshToken, ApplicationDbContextModelSnapshot

### Community 33 - "ConversationMember"
Cohesion: 0.11
Nodes (13): ConversationMember, Conversation, ConversationId, IsMuted, JoinedAt, LastReadAt, Role, User (+5 more)

### Community 34 - "IDomainEvent"
Cohesion: 0.10
Nodes (11): SocialConnect.Domain.Events.Messaging, SocialConnect.Domain.Events.Posts, IDomainEvent, MessageSentDomainEvent, MessageId, PostCreatedDomainEvent, PostId, PostPublishedDomainEvent (+3 more)

### Community 35 - "IApplicationDbContext"
Cohesion: 0.09
Nodes (19): IApplicationDbContext, AuditLogs, Blocks, Comments, ConversationMembers, Conversations, Follows, MessageAttachments (+11 more)

### Community 36 - "EditMessageCommandHandler"
Cohesion: 0.17
Nodes (6): EditMessageCommand, EditMessageCommandHandler, SendMessageCommand, SendMessageCommandHandler, IMessageRepository, EditMessageCommandHandlerTests

### Community 38 - "IHasTimestamps"
Cohesion: 0.12
Nodes (7): IHasTimestamps, CreatedAt, UpdatedAt, ISoftDeletable, DeletedAt, IsDeleted, AuditableEntitySaveChangesInterceptor

### Community 39 - "RefreshToken"
Cohesion: 0.11
Nodes (14): RefreshToken, CreatedAt, CreatedByIp, DeviceInfo, ExpiresAt, IsActive, IsExpired, IsRevoked (+6 more)

### Community 40 - "UserProfile"
Cohesion: 0.11
Nodes (13): UserProfile, Bio, CoverImageUrl, CreatedAt, DateOfBirth, FirstName, Headline, LastName (+5 more)

### Community 41 - "SocialConnect.Domain.Entities.Users"
Cohesion: 0.17
Nodes (3): SocialConnect.Domain.Entities.Users, SocialConnect.Domain.Common, SocialConnect.Domain.Events.Users

### Community 43 - "MessageDto"
Cohesion: 0.11
Nodes (14): MessageDto, Attachments, Content, ConversationId, CreatedAt, Id, IsDeleted, MessageType (+6 more)

### Community 45 - ".Handle"
Cohesion: 0.16
Nodes (3): ValidationException, Errors, RegisterCommand

### Community 46 - "SocialConnect.Application"
Cohesion: 0.15
Nodes (17): AutoMapper, FluentValidation.DependencyInjectionExtensions, Microsoft.EntityFrameworkCore, Microsoft.Extensions.Logging.Abstractions, net9.0, SocialConnect.Application, MediatR, SocialConnect.Domain (+9 more)

### Community 49 - "IRequest"
Cohesion: 0.14
Nodes (5): DeletePostCommand, UpdatePostCommand, UpdatePostCommandHandler, GetPostByIdQuery, GetPostByIdQueryHandler

### Community 50 - "BusinessRuleViolationException"
Cohesion: 0.19
Nodes (3): BusinessRuleViolationException, PostMediaConfiguration, ValueObjectTests

### Community 51 - "Notification"
Cohesion: 0.12
Nodes (13): Notification, Actor, ActorId, Content, CreatedAt, IsRead, ReadAt, TargetId (+5 more)

### Community 52 - ".Create"
Cohesion: 0.26
Nodes (3): PostContent, Value, PostTests

### Community 53 - "PostsController.cs"
Cohesion: 0.18
Nodes (6): SocialConnect.Application.Features.Posts.Queries.GetPostById, SocialConnect.Application.Features.Posts.Queries.GetUserPosts, SocialConnect.Application.Features.Posts.Commands.UploadPostMedia, SocialConnect.Application.Features.Posts.DTOs, SocialConnect.Application.Features.Posts.Queries.SearchPosts, SocialConnect.Application.Features.Posts.Queries.GetFeed

### Community 55 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 56 - "GetMessagesQueryHandler"
Cohesion: 0.24
Nodes (3): GetMessagesQuery, GetMessagesQueryHandler, GetMessagesQueryHandlerTests

### Community 57 - "UpdateProfileCommand"
Cohesion: 0.25
Nodes (3): UpdateProfileCommand, UpdateProfileCommandHandler, UpdateProfileCommandHandlerTests

### Community 60 - "SocialConnect.Application.Features.Users.DTOs"
Cohesion: 0.20
Nodes (5): SocialConnect.Application.Features.Users.Commands.UpdateProfile, SocialConnect.Application.Features.Users.Queries.GetUserProfile, SocialConnect.Application.Features.Users.Queries.SearchUsers, SocialConnect.Application.Features.Users.Queries.GetUserById, SocialConnect.Application.Features.Users.DTOs

### Community 62 - ".SaveChangesAsync"
Cohesion: 0.16
Nodes (3): LogoutCommand, LogoutCommandHandler, LogoutCommandHandlerTests

### Community 64 - "CreateGroupChatCommand"
Cohesion: 0.23
Nodes (3): CreateGroupChatCommand, CreateGroupChatCommandHandler, CreateGroupChatCommandHandlerTests

### Community 65 - "Entity"
Cohesion: 0.13
Nodes (8): AggregateRoot, Version, Entity, DomainEvents, Id, Role, Name, NormalizedName

### Community 66 - "AuditLog"
Cohesion: 0.14
Nodes (12): AuditLog, Action, CreatedAt, EntityId, EntityType, Id, IpAddress, NewValues (+4 more)

### Community 67 - ".Create"
Cohesion: 0.20
Nodes (4): SocialConnect.Domain.Events.Notifications, NotificationCreatedDomainEvent, NotificationId, SocialAndNotificationTests

### Community 69 - "SocialConnect.Infrastructure"
Cohesion: 0.14
Nodes (14): AWSSDK.Extensions.NETCore.Setup, AWSSDK.S3, BCrypt.Net-Next, Hangfire.AspNetCore, Hangfire.InMemory, Hangfire.SqlServer, MailKit, Microsoft.EntityFrameworkCore.SqlServer (+6 more)

### Community 70 - "UpdateCommentCommand"
Cohesion: 0.25
Nodes (3): UpdateCommentCommand, UpdateCommentCommandHandler, UpdateCommentCommandHandlerTests

### Community 71 - "CommentDto"
Cohesion: 0.14
Nodes (11): CommentDto, AuthorId, AuthorName, AuthorProfileImageUrl, Content, CreatedAt, Depth, Id (+3 more)

### Community 72 - "DeleteMessageCommandHandler"
Cohesion: 0.31
Nodes (3): DeleteMessageCommand, DeleteMessageCommandHandler, DeleteMessageCommandHandlerTests

### Community 73 - "ConversationMemberDto"
Cohesion: 0.14
Nodes (10): ConversationMemberDto, FirstName, IsMuted, JoinedAt, LastName, LastReadAt, ProfileImageUrl, Role (+2 more)

### Community 74 - "SchedulePostCommand"
Cohesion: 0.24
Nodes (3): SchedulePostCommand, SchedulePostCommandHandler, SchedulePostCommandHandlerTests

### Community 75 - ".Create"
Cohesion: 0.24
Nodes (3): SearchUsersQuery, SearchUsersQueryHandler, SearchUsersQueryHandlerTests

### Community 76 - "MessageAttachment"
Cohesion: 0.15
Nodes (9): MessageAttachment, ContentType, CreatedAt, FileName, FileSize, FileUrl, Message, MessageId (+1 more)

### Community 80 - "IMapFrom"
Cohesion: 0.17
Nodes (4): SocialConnect.Application, IMapFrom, MappingProfile, DependencyInjection

### Community 81 - ".CreateMockDbSet"
Cohesion: 0.22
Nodes (3): TestAsyncEnumerable, Provider, TestAsyncQueryProvider

### Community 84 - "ICurrentUserService"
Cohesion: 0.17
Nodes (6): ICurrentUserService, Email, IpAddress, UserId, CreatePostCommandHandler, NotificationCommandHandlerTests

### Community 85 - "DeleteCommentCommand"
Cohesion: 0.27
Nodes (3): DeleteCommentCommand, DeleteCommentCommandHandler, DeleteCommentCommandHandlerTests

### Community 86 - "MarkAsReadCommandHandler"
Cohesion: 0.27
Nodes (3): MarkAsReadCommand, MarkAsReadCommandHandler, MarkAsReadCommandHandlerTests

### Community 87 - "MessageAttachmentDto"
Cohesion: 0.15
Nodes (9): MessageAttachmentDto, ContentType, CreatedAt, FileName, FileSize, FileUrl, Id, MessageId (+1 more)

### Community 88 - "GetConversationsQueryHandler"
Cohesion: 0.26
Nodes (3): GetConversationsQuery, GetConversationsQueryHandler, GetConversationsQueryHandlerTests

### Community 90 - "PublishPostCommand"
Cohesion: 0.27
Nodes (3): PublishPostCommand, PublishPostCommandHandler, PublishPostCommandHandlerTests

### Community 91 - "ReactionDto"
Cohesion: 0.15
Nodes (9): ReactionDto, CreatedAt, Id, ReactionType, UserFirstName, UserId, UserLastName, UserName (+1 more)

### Community 92 - ".Handle_WithValidRequest_ShouldCreateMuteAndReturnSuccess"
Cohesion: 0.26
Nodes (3): MuteUserCommand, MuteUserCommandHandler, MuteUserCommandHandlerTests

### Community 109 - "ApiControllerBase"
Cohesion: 0.20
Nodes (3): ApiControllerBase, Mediator, NotificationsController

### Community 110 - "TestAsyncEnumerable"
Cohesion: 0.23
Nodes (3): TestAsyncEnumerable, Provider, TestAsyncQueryProvider

### Community 112 - ".Handle"
Cohesion: 0.30
Nodes (3): ChangePasswordCommand, ChangePasswordCommandHandler, ChangePasswordCommandHandlerTests

### Community 113 - ".Handle"
Cohesion: 0.29
Nodes (3): RevokeTokenCommand, RevokeTokenCommandHandler, RevokeTokenCommandHandlerTests

### Community 114 - ".Handle"
Cohesion: 0.29
Nodes (3): VerifyEmailCommand, VerifyEmailCommandHandler, VerifyEmailCommandHandlerTests

### Community 115 - "ConversationDto"
Cohesion: 0.17
Nodes (9): ConversationDto, Id, ImageUrl, LastMessage, LastMessageAt, Members, Name, Type (+1 more)

### Community 116 - "Mute"
Cohesion: 0.20
Nodes (7): Mute, CreatedAt, Muted, MutedId, Muter, MuterId, MuteConfiguration

### Community 121 - "SocialConnect.API.Tests"
Cohesion: 0.20
Nodes (10): Microsoft.AspNetCore.Mvc.Testing, Testcontainers.MsSql, SocialConnect.API.Tests, coverlet.collector, FluentAssertions, Microsoft.EntityFrameworkCore.Sqlite, Microsoft.NET.Test.Sdk, Moq (+2 more)

### Community 122 - "PaginatedList"
Cohesion: 0.20
Nodes (8): PaginatedList, HasNextPage, HasPreviousPage, Items, PageNumber, PageSize, TotalCount, TotalPages

### Community 126 - "Reaction"
Cohesion: 0.22
Nodes (7): Reaction, CreatedAt, ReactionType, TargetId, TargetType, User, UserId

### Community 127 - "TargetType"
Cohesion: 0.27
Nodes (3): TargetType, Comment, Post

### Community 131 - "Exception"
Cohesion: 0.22
Nodes (4): ForbiddenAccessException, NotFoundException, DomainException, EntityNotFoundException

### Community 134 - "SocialConnect.API"
Cohesion: 0.25
Nodes (8): DotNetEnv, Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.AspNetCore.OpenApi, Microsoft.EntityFrameworkCore.Design, Serilog.AspNetCore, Swashbuckle.AspNetCore, Microsoft.NET.Sdk.Web, SocialConnect.API

### Community 142 - "SocialConnect.Domain.Tests"
Cohesion: 0.25
Nodes (8): net10.0, SocialConnect.Domain.Tests, coverlet.collector, FluentAssertions, Microsoft.NET.Test.Sdk, Moq, xunit, xunit.runner.visualstudio

### Community 150 - "NotificationType"
Cohesion: 0.29
Nodes (6): NotificationType, Mention, NewComment, NewFollower, NewMessage, NewReaction

### Community 151 - "SocialConnect.Infrastructure.Tests"
Cohesion: 0.29
Nodes (7): SocialConnect.Infrastructure.Tests, coverlet.collector, FluentAssertions, Microsoft.NET.Test.Sdk, Moq, xunit, xunit.runner.visualstudio

### Community 152 - "DesignTimeCurrentUserService"
Cohesion: 0.33
Nodes (4): DesignTimeCurrentUserService, Email, IpAddress, UserId

### Community 157 - "CursorPaginatedList"
Cohesion: 0.33
Nodes (4): CursorPaginatedList, HasNextPage, Items, NextCursor

### Community 159 - "MessageType"
Cohesion: 0.33
Nodes (5): MessageType, Document, Image, Text, Video

### Community 160 - "PostStatus"
Cohesion: 0.33
Nodes (5): PostStatus, Deleted, Draft, Published, Scheduled

### Community 161 - "PostType"
Cohesion: 0.33
Nodes (5): PostType, Image, Mixed, Text, Video

### Community 162 - "ReactionType"
Cohesion: 0.33
Nodes (5): ReactionType, Celebrate, Insightful, Like, Love

### Community 167 - "MessageStatus"
Cohesion: 0.40
Nodes (4): MessageStatus, Delivered, Read, Sent

## Knowledge Gaps
- **452 isolated node(s):** `SocialConnect.Application.Tests.Notifications`, `Bio`, `CoverImageUrl`, `DateOfBirth`, `FirstName` (+447 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 1170 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **73 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `IApplicationDbContext` connect `IApplicationDbContext` to `microsoft_entityframeworkcore`, `Conversation`, `IFileStorageService`, `Follow`, `Result`, `IRequestHandler`, `UserProfileDto`, `ThumbnailGenerationJob`, `Post`, `ApplicationDbContext`, `ApiResponse`, `NotificationDto`, `User`, `GetReactionsQuery`, `PostDto`, `UserDto`, `Message`, `Comment`, `PostMedia`, `ForgotPasswordCommandHandler`, `ConversationMember`, `RefreshToken`, `UserProfile`, `CleanupOrphanUploadsJob`, `IRequest`, `Notification`, `GetMessagesQueryHandler`, `UpdateProfileCommand`, `.SaveChangesAsync`, `Entity`, `AuditLog`, `UpdateCommentCommand`, `SchedulePostCommand`, `.Create`, `MessageAttachment`, `DeleteCommentCommand`, `MarkAsReadCommandHandler`, `GetConversationsQueryHandler`, `PublishPostCommand`, `.Handle_WithValidRequest_ShouldCreateMuteAndReturnSuccess`, `.ExecuteAsync_ShouldCleanUpExpiredTokens`, `.Handle`, `.Handle`, `.Handle`, `Mute`, `ScheduledPostPublishJob`, `GetFeedQuery`, `GetUserPostsQuery`, `Reaction`?**
  _High betweenness centrality (0.165) - this node is a cross-community bridge._
- **What connects `SocialConnect.Application.Tests.Notifications`, `Bio`, `CoverImageUrl` to the rest of the system?**
  _452 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `mediatr` be split into smaller, more focused modules?**
  _Cohesion score 0.039208364451082896 - nodes in this community are weakly interconnected._
- **Why does `Result` connect `Result` to `Conversation`, `IFileStorageService`, `Follow`, `fluentvalidation`, `.Handle`, `IRequestHandler`, `AddCommentCommandHandler`, `CreatePostCommand`, `.Handle`, `ForgotPasswordCommandHandler`, `EditMessageCommandHandler`, `IRequest`, `UpdateProfileCommand`, `.SaveChangesAsync`, `CreateGroupChatCommand`, `UpdateCommentCommand`, `DeleteMessageCommandHandler`, `SchedulePostCommand`, `ICurrentUserService`, `DeleteCommentCommand`, `MarkAsReadCommandHandler`, `PublishPostCommand`, `.Handle_WithValidRequest_ShouldCreateMuteAndReturnSuccess`, `.Handle`, `.Handle`, `.Handle`, `.Handle`?**
  _High betweenness centrality (0.081) - this node is a cross-community bridge._
- **Should `microsoft_entityframeworkcore` be split into smaller, more focused modules?**
  _Cohesion score 0.04945054945054945 - nodes in this community are weakly interconnected._
- **Why does `SocialConnect.Application.Common.Interfaces` connect `SocialConnect.Application.Common.Interfaces` to `mediatr`, `microsoft_entityframeworkcore`, `TokenService.cs`, `IFileStorageService`, `Program.cs`, `IRequestHandler`, `SocialConnect.Infrastructure.Persistence`, `IIdentityService`, `EmailSendingJob`, `MessagesController.cs`, `ForgotPasswordCommandHandler`, `ForgotPasswordCommandHandler.cs`, `RabbitMqEventBus`, `PostsController.cs`, `CacheService`, `SocialConnect.Application.Features.Users.DTOs`, `ServicesTests`, `ICurrentUserService`, `ValidationBehaviorTests.cs`?**
  _High betweenness centrality (0.069) - this node is a cross-community bridge._
- **Should `Conversation` be split into smaller, more focused modules?**
  _Cohesion score 0.05322128851540616 - nodes in this community are weakly interconnected._