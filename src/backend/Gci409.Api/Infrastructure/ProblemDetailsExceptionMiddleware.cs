using FluentValidation;
using Gci409.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Gci409.Api.Infrastructure;

public sealed class ProblemDetailsExceptionMiddleware(RequestDelegate next, ILogger<ProblemDetailsExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("Request {Path} was canceled by the client.", context.Request.Path);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Request {Method} {Path} failed.", context.Request.Method, context.Request.Path);
            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        ProblemDetails problemDetails = exception switch
        {
            global::FluentValidation.ValidationException fluentValidationException => BuildValidationProblemDetails(context, fluentValidationException),
            global::Gci409.Application.Common.ValidationException validationException when validationException.Errors is not null => BuildValidationProblemDetails(context, validationException.Errors),
            global::Gci409.Application.Common.ValidationException validationException => BuildProblemDetails(context, StatusCodes.Status400BadRequest, "Validation failed", validationException.Message),
            ForbiddenException forbiddenException => BuildProblemDetails(context, StatusCodes.Status403Forbidden, "Forbidden", forbiddenException.Message),
            NotFoundException notFoundException => BuildProblemDetails(context, StatusCodes.Status404NotFound, "Resource not found", notFoundException.Message),
            UnauthorizedAccessException unauthorizedAccessException => BuildProblemDetails(context, StatusCodes.Status401Unauthorized, "Unauthorized", unauthorizedAccessException.Message),
            _ => BuildProblemDetails(context, StatusCodes.Status500InternalServerError, "Request failed", "An unexpected error occurred.")
        };

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static ValidationProblemDetails BuildValidationProblemDetails(HttpContext context, global::FluentValidation.ValidationException validationException)
    {
        var errors = validationException.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray());

        return BuildValidationProblemDetails(context, errors);
    }

    private static ValidationProblemDetails BuildValidationProblemDetails(HttpContext context, IReadOnlyDictionary<string, string[]> errors)
    {
        return new ValidationProblemDetails(new Dictionary<string, string[]>(errors))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Type = "https://gci409/errors/validation",
            Detail = "One or more validation errors occurred.",
            Instance = context.Request.Path
        };
    }

    private static ProblemDetails BuildProblemDetails(HttpContext context, int statusCode, string title, string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://gci409/errors/{statusCode}",
            Detail = detail,
            Instance = context.Request.Path
        };
    }
}
