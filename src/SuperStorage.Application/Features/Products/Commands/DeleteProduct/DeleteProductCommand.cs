using SuperStorage.Application.Abstractions.Messaging;

namespace SuperStorage.Application.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand<bool>;
