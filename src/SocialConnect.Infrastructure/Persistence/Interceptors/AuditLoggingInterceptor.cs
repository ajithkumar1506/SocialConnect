using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Domain.Entities.Audit;

namespace SocialConnect.Infrastructure.Persistence.Interceptors;

public class AuditLoggingInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLoggingInterceptor(ICurrentUserService currentUserService, IHttpContextAccessor httpContextAccessor)
    {
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditLogs(DbContext? context)
    {
        if (context == null) return;

        var auditEntries = new List<AuditLog>();
        var currentUserId = _currentUserService.UserId;
        var ipAddress = _currentUserService.IpAddress;
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
            {
                continue;
            }

            var entityType = entry.Entity.GetType().Name;
            var action = entry.State.ToString();

            var idProp = entry.Metadata.FindProperty("Id");
            var entityId = idProp != null ? entry.Property("Id").CurrentValue?.ToString() ?? string.Empty : string.Empty;

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }

            var oldJson = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null;
            var newJson = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null;

            var auditLog = AuditLog.Create(
                currentUserId,
                action,
                entityType,
                entityId,
                oldJson,
                newJson,
                ipAddress,
                userAgent
            );

            auditEntries.Add(auditLog);
        }

        if (auditEntries.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditEntries);
        }
    }
}
