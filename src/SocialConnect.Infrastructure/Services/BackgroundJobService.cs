using System.Linq.Expressions;
using Hangfire;
using SocialConnect.Application.Common.Interfaces;

namespace SocialConnect.Infrastructure.Services;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public BackgroundJobService(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void Enqueue(Expression<Func<Task>> methodCall)
    {
        _backgroundJobClient.Enqueue(methodCall);
    }

    public void Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        _backgroundJobClient.Enqueue(methodCall);
    }
}
