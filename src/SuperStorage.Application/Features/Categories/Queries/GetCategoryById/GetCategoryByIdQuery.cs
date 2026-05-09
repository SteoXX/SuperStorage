using SuperStorage.Application.Abstractions.Messaging;
using SuperStorage.Contracts.Categories;

namespace SuperStorage.Application.Features.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IQuery<CategoryResponse?>;
