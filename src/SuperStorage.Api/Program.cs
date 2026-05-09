using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using SuperStorage.Api.Endpoints;
using SuperStorage.Application;
using SuperStorage.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Global exception handling middleware
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        // If the exception is validation-related, return a 400 Bad Request with the validation errors
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

        // If the exception is an InvalidOperationException, return a 409 Conflict with the exception message
        if (exception is InvalidOperationException invalidOperationException)
        {
            await Results.Problem(
                title: "Conflict",
                detail: invalidOperationException.Message,
                statusCode: StatusCodes.Status409Conflict)
                .ExecuteAsync(context);
            return;
        }

        // For all other exceptions, return a generic 500 Internal Server Error
        await Results.Problem().ExecuteAsync(context);
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapProductEndpoints();

app.Run();
