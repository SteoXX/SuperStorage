using SuperStorage.Application.Abstractions.Messaging;
using SuperStorage.Contracts.Categories;

namespace SuperStorage.Application.Features.Categories.Queries.GetCategoryDeleteImpact;

public sealed record GetCategoryDeleteImpactQuery(Guid Id) : IQuery<CategoryDeleteImpactResponse?>;
