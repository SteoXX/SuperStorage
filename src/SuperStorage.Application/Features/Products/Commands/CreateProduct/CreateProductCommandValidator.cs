using FluentValidation;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.Sku)
            .NotEmpty()
            .MaximumLength(64)
            .Matches("^[A-Za-z0-9._-]+$");

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(Product.NameMaxLength);

        RuleFor(command => command.Description)
            .MaximumLength(Product.DescriptionMaxLength);
    }
}
