using System.Reflection;
using MediatR;
using Shouldly;
using SuperStorage.Application.Abstractions.Messaging;

namespace SuperStorage.ArchitectureTests.Architecture;

public sealed class MediatRNamingTests
{
    [Fact]
    public void Commands_ShouldFollowNamingAndNamespaceConventions()
    {
        var violations = ApplicationTypes()
            .Where(IsCommand)
            .Where(type =>
                !type.Name.EndsWith("Command", StringComparison.Ordinal) ||
                !type.NamespaceContains(".Commands."))
            .Select(FormatType)
            .ToArray();

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void Queries_ShouldFollowNamingAndNamespaceConventions()
    {
        var violations = ApplicationTypes()
            .Where(IsQuery)
            .Where(type =>
                !type.Name.EndsWith("Query", StringComparison.Ordinal) ||
                !type.NamespaceContains(".Queries."))
            .Select(FormatType)
            .ToArray();

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void CommandHandlers_ShouldFollowNamingAndHandleCommands()
    {
        var violations = HandlerTypes()
            .Where(type => type.Name.EndsWith("CommandHandler", StringComparison.Ordinal))
            .Where(type => !HandlesRequest(type, IsCommand))
            .Select(FormatType)
            .ToArray();

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void QueryHandlers_ShouldFollowNamingAndHandleQueries()
    {
        var violations = HandlerTypes()
            .Where(type => type.Name.EndsWith("QueryHandler", StringComparison.Ordinal))
            .Where(type => !HandlesRequest(type, IsQuery))
            .Select(FormatType)
            .ToArray();

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void RequestHandlers_ShouldHaveMatchingRequestNamePrefix()
    {
        var violations = HandlerTypes()
            .SelectMany(GetHandledRequestTypes, (handler, request) => new
            {
                Handler = handler,
                Request = request
            })
            .Where(pair => !pair.Handler.Name.Equals(
                $"{pair.Request.Name}Handler",
                StringComparison.Ordinal))
            .Select(pair => $"{FormatType(pair.Handler)} handles {FormatType(pair.Request)}")
            .ToArray();

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void CommandAndQueryHandlers_ShouldBeInternalSealedClasses()
    {
        var violations = HandlerTypes()
            .Where(type => !type.IsClass || !type.IsSealed || !type.IsNotPublic)
            .Select(FormatType)
            .ToArray();

        violations.ShouldBeEmpty();
    }

    private static IEnumerable<Type> ApplicationTypes()
    {
        return ArchitectureAssemblies.Application
            .GetTypes()
            .Where(type => type is { IsAbstract: false, IsGenericTypeDefinition: false });
    }

    private static IEnumerable<Type> HandlerTypes()
    {
        return ApplicationTypes()
            .Where(type => type.Name.EndsWith("Handler", StringComparison.Ordinal))
            .Where(type => GetHandledRequestTypes(type).Any());
    }

    private static bool IsCommand(Type type)
    {
        return typeof(ICommand).IsAssignableFrom(type) ||
            ImplementsGenericInterface(type, typeof(ICommand<>));
    }

    private static bool IsQuery(Type type)
    {
        return ImplementsGenericInterface(type, typeof(IQuery<>));
    }

    private static bool HandlesRequest(Type handlerType, Func<Type, bool> requestPredicate)
    {
        return GetHandledRequestTypes(handlerType).Any(requestPredicate);
    }

    private static IEnumerable<Type> GetHandledRequestTypes(Type handlerType)
    {
        return handlerType
            .GetInterfaces()
            .Where(IsRequestHandlerInterface)
            .Select(type => type.GetGenericArguments()[0]);
    }

    private static bool IsRequestHandlerInterface(Type interfaceType)
    {
        if (!interfaceType.IsGenericType)
        {
            return false;
        }

        var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();

        return genericTypeDefinition == typeof(IRequestHandler<>) ||
            genericTypeDefinition == typeof(IRequestHandler<,>);
    }

    private static bool ImplementsGenericInterface(Type type, Type genericInterfaceType)
    {
        return type
            .GetInterfaces()
            .Any(interfaceType =>
                interfaceType.IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == genericInterfaceType);
    }

    private static string FormatType(Type type)
    {
        return type.FullName ?? type.Name;
    }
}
