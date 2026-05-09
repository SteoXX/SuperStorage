using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Diagnostics;

namespace SuperStorage.Api.DependencyInjection;

internal static class ExceptionHandlerExtensions
{
    public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder app)
    {
        // Keep exception-to-HTTP mapping centralized so Program.cs only describes the pipeline shape.
        return app.UseExceptionHandler(exceptionApp =>
        {
            exceptionApp.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                // FluentValidation failures are client input problems, so return a 400 with field-level errors.
                if (exception is ValidationException validationException)
                {
                    var errors = validationException.Errors
                        .GroupBy(error => error.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(error => error.ErrorMessage).ToArray());

                    await Results.ValidationProblem(errors).ExecuteAsync(context);
                    return;
                }

                // Application conflicts, such as duplicate business keys, should surface as 409 responses.
                if (exception is InvalidOperationException invalidOperationException)
                {
                    await Results.Problem(
                        title: "Conflict",
                        detail: invalidOperationException.Message,
                        statusCode: StatusCodes.Status409Conflict)
                        .ExecuteAsync(context);
                    return;
                }

                // Antiforgery failures are bad client requests, usually stale or user-mismatched CSRF tokens.
                if (exception is AntiforgeryValidationException antiforgeryValidationException)
                {
                    await Results.Problem(
                        title: "Invalid antiforgery token",
                        detail: antiforgeryValidationException.Message,
                        statusCode: StatusCodes.Status400BadRequest)
                        .ExecuteAsync(context);
                    return;
                }

                // Hide unexpected exception details from clients while preserving a standard ProblemDetails shape.
                await Results.Problem().ExecuteAsync(context);
            });
        });
    }
}
