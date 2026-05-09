using MediatR;
using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Contracts.Products;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Products.Queries.GetProductById;

internal sealed class GetProductByIdQueryHandler(IQueryDbContext dbContext)
    : IRequestHandler<GetProductByIdQuery, ProductResponse?>
{
    public async Task<ProductResponse?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.SingleOrDefaultAsync(
            dbContext.Query<Product>().Where(product => product.Id == request.Id),
            cancellationToken);

        return product is null
            ? null
            : new ProductResponse(
                product.Id,
                product.Sku.Value,
                product.Name,
                product.Description,
                product.IsActive);
    }
}
