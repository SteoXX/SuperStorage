using FluentValidation;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Description)
            .MaximumLength(Product.DescriptionMaxLength);
    }
}
