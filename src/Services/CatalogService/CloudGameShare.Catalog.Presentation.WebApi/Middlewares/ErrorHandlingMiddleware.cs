using CloudGameShare.BuildingBlocks.Domain.Exceptions;
using CloudGameShare.Catalog.Application.Exceptions;
using FluentValidation;

namespace CloudGameShare.Catalog.Presentation.WebApi.Middlewares;

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
                ValidationException validation => new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Validation failed.",
                    validation.Errors.Select(failure => new FieldError(failure.PropertyName, failure.ErrorMessage)).ToArray()),
                BadHttpRequestException badRequest => new ErrorResponse(badRequest.StatusCode, badRequest.Message),
                DomainException domain => new ErrorResponse(StatusCodes.Status422UnprocessableEntity, domain.Message),
                NotFoundException notFound => new ErrorResponse(StatusCodes.Status404NotFound, notFound.Message),
                _ => new ErrorResponse(StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            if (error.Status == StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Catalog request failed.");

            context.Response.Clear();
            context.Response.StatusCode = error.Status;
            await context.Response.WriteAsJsonAsync(error, context.RequestAborted);
        }
    }

    private sealed record ErrorResponse(int Status, string Title, FieldError[]? Errors = null);
    private sealed record FieldError(string Field, string Error);
}
