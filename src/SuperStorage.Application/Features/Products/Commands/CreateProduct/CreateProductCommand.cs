using SuperStorage.Application.Abstractions.Messaging;
using SuperStorage.Contracts.Products;

namespace SuperStorage.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Sku,
    string Name,
    string? Description) : ICommand<ProductResponse>;
