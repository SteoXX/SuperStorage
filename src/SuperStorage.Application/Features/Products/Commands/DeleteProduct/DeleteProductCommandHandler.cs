using MediatR;

namespace SuperStorage.Application.Features.Products.Commands.DeleteProduct;

internal sealed class DeleteProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return false;
        }

        productRepository.Remove(product);

        return true;
    }
}
