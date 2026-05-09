using FluentValidation;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Categories.Commands.CreateCategory;

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(Category.NameMaxLength);

        RuleFor(command => command.Description)
            .MaximumLength(Category.DescriptionMaxLength);
    }
}
