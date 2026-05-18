using FluentValidation;
using MediatR;
using Shouldly;
using SuperStorage.Application.Common.Behaviors;

namespace SuperStorage.Application.UnitTests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldCallNext_WhenThereAreNoValidators()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);

        RequestHandlerDelegate<string> next = _ => Task.FromResult("handled");

        var result = await behavior.Handle(new TestRequest(""), next, CancellationToken.None);

        result.ShouldBe("handled");
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationSucceeds()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(request => request.Name).NotEmpty();

        var behavior = new ValidationBehavior<TestRequest, string>([validator]);

        RequestHandlerDelegate<string> next = _ => Task.FromResult("handled");

        var result = await behavior.Handle(new TestRequest("Valid"), next, CancellationToken.None);

        result.ShouldBe("handled");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(request => request.Name).NotEmpty();

        var behavior = new ValidationBehavior<TestRequest, string>([validator]);

        RequestHandlerDelegate<string> next = _ => Task.FromResult("handled");

        var exception = await Should.ThrowAsync<ValidationException>(async () =>
            await behavior.Handle(new TestRequest(""), next, CancellationToken.None));

        exception.Errors.ShouldContain(failure => failure.PropertyName == nameof(TestRequest.Name));
    }

    private sealed record TestRequest(string Name);
}
