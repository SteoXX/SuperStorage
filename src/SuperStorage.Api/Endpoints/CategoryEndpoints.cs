using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SuperStorage.Application.Features.Categories.Commands.CreateCategory;
using SuperStorage.Application.Features.Categories.Commands.DeleteCategory;
using SuperStorage.Application.Features.Categories.Commands.UpdateCategory;
using SuperStorage.Application.Features.Categories.Queries.GetCategoryById;
using SuperStorage.Application.Features.Categories.Queries.GetCategoryDeleteImpact;
using SuperStorage.Application.Features.Categories.Queries.SearchCategories;
using SuperStorage.Contracts.Auth;
using SuperStorage.Contracts.Categories;
using SuperStorage.Contracts.Common;

namespace SuperStorage.Api.Endpoints;

internal static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/categories")
            .WithTags("Categories");

        group.MapGet("/", SearchCategoriesAsync)
            .WithName("SearchCategories")
            .Produces<PagedResult<CategoryListItemResponse>>()
            .RequireAuthorization(AuthPolicies.CategoriesRead);

        group.MapGet("/{id:guid}", GetCategoryByIdAsync)
            .WithName("GetCategoryById")
            .Produces<CategoryResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthPolicies.CategoriesRead);

        group.MapPost("/", CreateCategoryAsync)
            .WithName("CreateCategory")
            .Produces<CategoryResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthPolicies.CategoriesWrite);

        group.MapPut("/{id:guid}", UpdateCategoryAsync)
            .WithName("UpdateCategory")
            .Produces<CategoryResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthPolicies.CategoriesWrite);

        group.MapGet("/{id:guid}/delete-impact", GetCategoryDeleteImpactAsync)
            .WithName("GetCategoryDeleteImpact")
            .Produces<CategoryDeleteImpactResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthPolicies.CategoriesWrite);

        group.MapDelete("/{id:guid}", DeleteCategoryAsync)
            .WithName("DeleteCategory")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthPolicies.CategoriesWrite);

        return app;
    }

    private static async Task<Ok<PagedResult<CategoryListItemResponse>>> SearchCategoriesAsync(
        [AsParameters] SearchCategoriesRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SearchCategoriesQuery(
                request.SearchTerm,
                request.IsActive,
                request.PageNumber,
                request.PageSize),
            cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<CategoryResponse>, NotFound>> GetCategoryByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoryByIdQuery(id), cancellationToken);

        return result is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(result);
    }

    private static async Task<Created<CategoryResponse>> CreateCategoryAsync(
        [FromBody] CreateCategoryRequest request,
        HttpContext httpContext,
        IAntiforgery antiforgery,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(httpContext);

        var result = await sender.Send(
            new CreateCategoryCommand(
                request.Name,
                request.Description),
            cancellationToken);

        return TypedResults.Created($"/api/categories/{result.Id}", result);
    }

    private static async Task<Results<Ok<CategoryResponse>, NotFound>> UpdateCategoryAsync(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        HttpContext httpContext,
        IAntiforgery antiforgery,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(httpContext);

        var result = await sender.Send(
            new UpdateCategoryCommand(
                id,
                request.Name,
                request.Description,
                request.IsActive),
            cancellationToken);

        return result is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<CategoryDeleteImpactResponse>, NotFound>> GetCategoryDeleteImpactAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoryDeleteImpactQuery(id), cancellationToken);

        return result is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(result);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteCategoryAsync(
        Guid id,
        HttpContext httpContext,
        IAntiforgery antiforgery,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await antiforgery.ValidateRequestAsync(httpContext);

        var deleted = await sender.Send(new DeleteCategoryCommand(id), cancellationToken);

        return deleted
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private sealed record SearchCategoriesRequest(
        string? SearchTerm = null,
        bool? IsActive = null,
        int PageNumber = 1,
        int PageSize = 20);
}
