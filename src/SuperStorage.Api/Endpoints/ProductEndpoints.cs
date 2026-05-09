using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SuperStorage.Application.Features.Products.Commands.CreateProduct;
using SuperStorage.Application.Features.Products.Queries.GetProductById;
using SuperStorage.Application.Features.Products.Queries.SearchProducts;
using SuperStorage.Contracts.Common;
using SuperStorage.Contracts.Products;

namespace SuperStorage.Api.Endpoints;

internal static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", SearchProductsAsync)
            .WithName("SearchProducts")
            .Produces<PagedResult<ProductListItemResponse>>();

        group.MapGet("/{id:guid}", GetProductByIdAsync)
            .WithName("GetProductById")
            .Produces<ProductResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateProductAsync)
            .WithName("CreateProduct")
            .Produces<ProductResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<Ok<PagedResult<ProductListItemResponse>>> SearchProductsAsync(
        [AsParameters] SearchProductsRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SearchProductsQuery(
                request.SearchTerm,
                request.IsActive,
                request.PageNumber,
                request.PageSize),
            cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ProductResponse>, NotFound>> GetProductByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductByIdQuery(id), cancellationToken);

        return result is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(result);
    }

    private static async Task<Created<ProductResponse>> CreateProductAsync(
        [FromBody] CreateProductRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateProductCommand(
                request.Sku,
                request.Name,
                request.Description),
            cancellationToken);

        return TypedResults.Created($"/api/products/{result.Id}", result);
    }

    private sealed record SearchProductsRequest(
        string? SearchTerm = null,
        bool? IsActive = null,
        int PageNumber = 1,
        int PageSize = 20);
}
