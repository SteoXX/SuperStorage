using MediatR;
using SuperStorage.Contracts.Products;

namespace SuperStorage.Application.Features.Products.Queries.GetProductById;

internal sealed class GetProductByIdQueryHandler(IProductReadRepository productReadRepository)
    : IRequestHandler<GetProductByIdQuery, ProductResponse?>
{
    public async Task<ProductResponse?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await productReadRepository.GetByIdResponseAsync(request.Id, cancellationToken);
    }
}
