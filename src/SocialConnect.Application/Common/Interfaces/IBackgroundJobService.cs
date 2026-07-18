using System.Linq.Expressions;

namespace SocialConnect.Application.Common.Interfaces;

public interface IBackgroundJobService
{
    void Enqueue(Expression<Func<Task>> methodCall);
    void Enqueue<T>(Expression<Func<T, Task>> methodCall);
}
