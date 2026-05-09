using SuperStorage.Application.Abstractions.Messaging;
using SuperStorage.Contracts.Products;

namespace SuperStorage.Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductResponse?>;
