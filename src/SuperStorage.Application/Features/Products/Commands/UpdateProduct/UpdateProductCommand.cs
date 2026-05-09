using SuperStorage.Application.Abstractions.Messaging;
using SuperStorage.Contracts.Products;

namespace SuperStorage.Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string? Description,
    Guid? CategoryId,
    bool IsActive) : ICommand<ProductResponse?>;
