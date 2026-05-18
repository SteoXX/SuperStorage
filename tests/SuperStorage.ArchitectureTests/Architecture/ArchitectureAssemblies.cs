using System.Reflection;
using SuperStorage.Application.Abstractions.Messaging;
using SuperStorage.Contracts.Auth;
using SuperStorage.Domain.Common;
using SuperStorage.Infrastructure.Persistence;

namespace SuperStorage.ArchitectureTests.Architecture;

internal static class ArchitectureAssemblies
{
    public static readonly Assembly Api = typeof(Program).Assembly;
    public static readonly Assembly Application = typeof(ICommand).Assembly;
    public static readonly Assembly Client = typeof(Client.App).Assembly;
    public static readonly Assembly Contracts = typeof(AuthRoles).Assembly;
    public static readonly Assembly Domain = typeof(Entity<>).Assembly;
    public static readonly Assembly Infrastructure = typeof(SuperStorageDbContext).Assembly;
}
