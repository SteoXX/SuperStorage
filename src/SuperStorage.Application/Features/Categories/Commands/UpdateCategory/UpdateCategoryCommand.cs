using SuperStorage.Application.Abstractions.Messaging;
using SuperStorage.Contracts.Categories;

namespace SuperStorage.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive) : ICommand<CategoryResponse?>;
