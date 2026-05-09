namespace SuperStorage.Client.Services.ApiClients;

public sealed class ApiException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
