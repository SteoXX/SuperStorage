using SuperStorage.Application.Abstractions.Messaging;
using SuperStorage.Contracts.Categories;

namespace SuperStorage.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description) : ICommand<CategoryResponse>;
