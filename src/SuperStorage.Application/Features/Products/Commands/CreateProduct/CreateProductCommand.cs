using SuperStorage.Application.Abstractions.Messaging;
using SuperStorage.Contracts.Products;

namespace SuperStorage.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Code,
    string Sku,
    string? Description,
    Guid? CategoryId) : ICommand<ProductResponse>;
