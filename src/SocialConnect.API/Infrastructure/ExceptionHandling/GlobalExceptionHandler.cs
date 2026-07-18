using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Application.Common.Exceptions;

namespace SocialConnect.API.Infrastructure.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "Server Error",
            Detail = exception.ToString(),
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Status = (int)HttpStatusCode.BadRequest;
            problemDetails.Title = "Validation Error";
            problemDetails.Detail = "One or more validation errors occurred.";
            problemDetails.Extensions["errors"] = validationException.Errors;
        }
        else if (exception is NotFoundException notFoundException)
        {
            problemDetails.Status = (int)HttpStatusCode.NotFound;
            problemDetails.Title = "Not Found";
            problemDetails.Detail = notFoundException.Message;
        }
        else if (exception is UnauthorizedAccessException)
        {
            problemDetails.Status = (int)HttpStatusCode.Unauthorized;
            problemDetails.Title = "Unauthorized";
            problemDetails.Detail = "You are not authorized to perform this action.";
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
