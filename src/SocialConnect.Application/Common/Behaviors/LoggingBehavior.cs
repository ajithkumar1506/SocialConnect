using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using SocialConnect.Application.Common.Interfaces;

namespace SocialConnect.Application.Common.Behaviors;

public class LoggingBehavior<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(ILogger<TRequest> logger, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId ?? Guid.Empty;

        _logger.LogInformation(
            "SocialConnect Request: {Name} {@UserId} {@Request}",
            requestName,
            userId,
            request
        );

        return Task.CompletedTask;
    }
}
