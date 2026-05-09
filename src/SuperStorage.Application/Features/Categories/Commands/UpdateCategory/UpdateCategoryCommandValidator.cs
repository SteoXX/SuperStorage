using FluentValidation;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Categories.Commands.UpdateCategory;

internal sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(Category.NameMaxLength);

        RuleFor(command => command.Description)
            .MaximumLength(Category.DescriptionMaxLength);
    }
}
