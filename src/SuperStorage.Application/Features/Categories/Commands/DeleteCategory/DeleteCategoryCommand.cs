using SuperStorage.Application.Abstractions.Messaging;

namespace SuperStorage.Application.Features.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand<bool>;
