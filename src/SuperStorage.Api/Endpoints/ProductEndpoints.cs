using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SuperStorage.Application.Features.Products.Commands.CreateProduct;
using SuperStorage.Application.Features.Products.Commands.DeleteProduct;
using SuperStorage.Application.Features.Products.Commands.UpdateProduct;
using SuperStorage.Application.Features.Products.Queries.GetCategoryLookups;
using SuperStorage.Application.Features.Products.Queries.GetProductById;
using SuperStorage.Application.Features.Products.Queries.SearchProducts;
using SuperStorage.Contracts.Auth;
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
            .Produces<PagedResult<ProductListItemResponse>>()
            .RequireAuthorization(AuthPolicies.ProductsRead);

        group.MapGet("/{id:guid}", GetProductByIdAsync)
            .WithName("GetProductById")
            .Produces<ProductResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthPolicies.ProductsRead);

        group.MapPost("/", CreateProductAsync)
            .WithName("CreateProduct")
            .Produces<ProductResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthPolicies.ProductsWrite);

        group.MapPut("/{id:guid}", UpdateProductAsync)
            .WithName("UpdateProduct")
            .Produces<ProductResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthPolicies.ProductsWrite);

        group.MapDelete("/{id:guid}", DeleteProductAsync)
            .WithName("DeleteProduct")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthPolicies.ProductsWrite);

        group.MapGet("/categories/lookup", GetCategoryLookupsAsync)
            .WithName("GetProductCategoryLookups")
            .Produces<IReadOnlyCollection<CategoryLookupResponse>>()
            .RequireAuthorization(AuthPolicies.ProductsRead);

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
                request.CategoryId,
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
        HttpContext httpContext,
        IAntiforgery antiforgery,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(httpContext);

        var result = await sender.Send(
            new CreateProductCommand(
                request.Code,
                request.Sku,
                request.Description,
                request.CategoryId),
            cancellationToken);

        return TypedResults.Created($"/api/products/{result.Id}", result);
    }

    private static async Task<Results<Ok<ProductResponse>, NotFound>> UpdateProductAsync(
        Guid id,
        [FromBody] UpdateProductRequest request,
        HttpContext httpContext,
        IAntiforgery antiforgery,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(httpContext);

        var result = await sender.Send(
            new UpdateProductCommand(
                id,
                request.Description,
                request.CategoryId,
                request.IsActive),
            cancellationToken);

        return result is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(result);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteProductAsync(
        Guid id,
        HttpContext httpContext,
        IAntiforgery antiforgery,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(httpContext);

        var deleted = await sender.Send(new DeleteProductCommand(id), cancellationToken);

        return deleted
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<Ok<IReadOnlyCollection<CategoryLookupResponse>>> GetCategoryLookupsAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoryLookupsQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    private sealed record SearchProductsRequest(
        string? SearchTerm = null,
        Guid? CategoryId = null,
        bool? IsActive = null,
        int PageNumber = 1,
        int PageSize = 20);
}
