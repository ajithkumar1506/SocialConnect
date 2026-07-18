using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Posts.Commands.SchedulePost;

public record SchedulePostCommand(Guid PostId, DateTimeOffset ScheduledAt) : IRequest<Result<bool>>;
