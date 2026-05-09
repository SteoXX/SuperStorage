using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SuperStorage.Client;
using MudBlazor;
using MudBlazor.Services;
using SuperStorage.Client.Auth;
using SuperStorage.Client.Services.ApiClients;
using SuperStorage.Contracts.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices(options =>
{
    options.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
});
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy(AuthPolicies.ProductsRead, policy =>
        policy.RequireRole(
            AuthRoles.Administrator,
            AuthRoles.WarehouseManager,
            AuthRoles.Operator,
            AuthRoles.Viewer));

    options.AddPolicy(AuthPolicies.ProductsWrite, policy =>
        policy.RequireRole(
            AuthRoles.Administrator,
            AuthRoles.WarehouseManager));

    options.AddPolicy(AuthPolicies.UsersManage, policy =>
        policy.RequireRole(AuthRoles.Administrator));
});
builder.Services.AddScoped<ApiHttpMessageHandler>();
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<ApiHttpMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();

    return new HttpClient(handler)
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    };
});
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<ProductsApiClient>();
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CookieAuthenticationStateProvider>());

await builder.Build().RunAsync();
