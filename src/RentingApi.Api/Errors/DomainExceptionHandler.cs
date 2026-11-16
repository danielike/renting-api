using Microsoft.AspNetCore.Diagnostics;
using RentingApi.Domain.Exceptions;
using RentingApi.Api.Features.Vehicles.Shared;

namespace RentingApi.Api.Errors;

/// <summary>
/// Exception handler for domain exceptions.
/// </summary>
public class DomainExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DomainExceptionHandler> _logger;

    public DomainExceptionHandler(ILogger<DomainExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is DomainException domainException)
        {
            _logger.LogWarning(domainException, "Domain exception occurred: {Message}", domainException.Message);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            httpContext.Response.ContentType = "application/json";

            var error = new ErrorResponse(
                "Domain Error",
                domainException.Message
            );

            await httpContext.Response.WriteAsJsonAsync(error, cancellationToken);
            return true;
        }

        // Handle other exceptions
        _logger.LogError(exception, "An unhandled exception occurred");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var errorResponse = new ErrorResponse(
            "Internal Server Error",
            "An unexpected error occurred. Please try again later."
        );

        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
        return true;
    }
}
