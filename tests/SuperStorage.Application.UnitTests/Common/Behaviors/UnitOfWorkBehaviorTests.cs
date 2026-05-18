using MediatR;
using Moq;
using Shouldly;
using SuperStorage.Application.Abstractions.Messaging;
using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Application.Common.Behaviors;

namespace SuperStorage.Application.UnitTests.Common.Behaviors;

public sealed class UnitOfWorkBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldCommitTransaction_WhenRequestIsCommand()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var behavior = new UnitOfWorkBehavior<TestCommand, string>(unitOfWork.Object);
        var request = new TestCommand();
        var nextCalled = false;

        RequestHandlerDelegate<string> next = cancellationToken =>
        {
            nextCalled = true;
            return Task.FromResult("handled");
        };

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.ShouldBe("handled");
        nextCalled.ShouldBeTrue();
        unitOfWork.Verify(work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(work => work.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(work => work.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRollbackTransaction_WhenCommandThrows()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var behavior = new UnitOfWorkBehavior<TestCommand, string>(unitOfWork.Object);

        RequestHandlerDelegate<string> next = _ =>
            throw new InvalidOperationException("Command failed.");

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await behavior.Handle(new TestCommand(), next, CancellationToken.None));

        unitOfWork.Verify(work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(work => work.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(work => work.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldBypassUnitOfWork_WhenRequestIsQuery()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var behavior = new UnitOfWorkBehavior<TestQuery, string>(unitOfWork.Object);

        RequestHandlerDelegate<string> next = _ => Task.FromResult("query-result");

        var result = await behavior.Handle(new TestQuery(), next, CancellationToken.None);

        result.ShouldBe("query-result");
        unitOfWork.Verify(work => work.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(work => work.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(work => work.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed record TestCommand : ICommand<string>;

    private sealed record TestQuery : IQuery<string>;
}
