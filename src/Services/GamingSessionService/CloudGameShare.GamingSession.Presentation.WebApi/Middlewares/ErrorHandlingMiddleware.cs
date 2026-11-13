using CloudGameShare.GamingSession.Application.Exceptions;
using FluentValidation;
using MongoDB.Driver;

namespace CloudGameShare.GamingSession.Presentation.WebApi.Middlewares;

internal sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted && !context.RequestAborted.IsCancellationRequested)
        {
            var error = exception switch
            {
                ValidationException validation => new ErrorResponse(400, "Validation failed.",
                    validation.Errors.Select(failure => new FieldError(failure.PropertyName, failure.ErrorMessage)).ToArray()),
                BadHttpRequestException badRequest => new ErrorResponse(badRequest.StatusCode, badRequest.Message),
                SessionRequestException { Error: SessionRequestError.NotFound } missing => new ErrorResponse(404, missing.Message),
                SessionRequestException { Error: SessionRequestError.Conflict } conflict => new ErrorResponse(409, conflict.Message),
                SessionRequestException unavailable => new ErrorResponse(503, unavailable.Message),
                MongoConnectionException or TimeoutException => new ErrorResponse(503, "Session storage is temporarily unavailable."),
                _ => new ErrorResponse(500, "An unexpected error occurred.")
            };
            if (error.Status >= 500)
                logger.LogWarning(exception, "Gaming session request failed.");

            context.Response.Clear();
            context.Response.StatusCode = error.Status;
            await context.Response.WriteAsJsonAsync(error, context.RequestAborted);
        }
    }

    private sealed record ErrorResponse(int Status, string Title, FieldError[]? Errors = null);
    private sealed record FieldError(string Field, string Error);
}
