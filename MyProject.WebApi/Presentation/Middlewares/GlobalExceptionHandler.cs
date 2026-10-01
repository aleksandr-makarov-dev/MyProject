using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyProject.WebApi.Application.Abstract.Exceptions;
using ApplicationException = MyProject.WebApi.Application.Abstract.Exceptions.ApplicationException;
using UnauthorizedAccessException = MyProject.WebApi.Application.Abstract.Exceptions.UnauthorizedAccessException;

namespace MyProject.WebApi.Presentation.Middlewares;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occured. TraceId: {TraceId}", httpContext.TraceIdentifier);

        var statusCode = StatusCodes.Status500InternalServerError;
        var detail = "An internal server error occured.";

        if (exception is ApplicationException applicationException)
        {
            statusCode = GetHttpStatusCode(applicationException);
            detail = applicationException.Message;
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetProblemTitle(statusCode),
                Type = GetProblemType(statusCode),
                Detail = detail
            }
        });
    }

    private static int GetHttpStatusCode(ApplicationException exception)
    {
        return exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            BadRequestException => StatusCodes.Status400BadRequest,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string GetProblemType(int statusCode) =>
        statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            401 => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            403 => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };

    private static string GetProblemTitle(int statusCode) =>
        statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            _ => "Internal Server Error"
        };
}