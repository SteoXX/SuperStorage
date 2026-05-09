using SuperStorage.Application.Abstractions.Messaging;
using SuperStorage.Contracts.Products;

namespace SuperStorage.Application.Features.Products.Queries.GetCategoryLookups;

public sealed record GetCategoryLookupsQuery : IQuery<IReadOnlyCollection<CategoryLookupResponse>>;
