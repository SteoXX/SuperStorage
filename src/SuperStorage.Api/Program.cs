using SuperStorage.Api.DependencyInjection;
using SuperStorage.Api.Endpoints;
using SuperStorage.Application;
using SuperStorage.Infrastructure.DependencyInjection;
using SuperStorage.Infrastructure.Persistence.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.SeedIdentityAsync(app.Configuration);

app.UseApiExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Map endpoints
app.MapAuthEndpoints();
app.MapCategoryEndpoints();
app.MapProductEndpoints();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
