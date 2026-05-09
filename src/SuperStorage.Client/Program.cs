using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SuperStorage.Client;
using MudBlazor.Services;
using SuperStorage.Client.Auth;
using SuperStorage.Client.Services.ApiClients;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();
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
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CookieAuthenticationStateProvider>());

await builder.Build().RunAsync();
