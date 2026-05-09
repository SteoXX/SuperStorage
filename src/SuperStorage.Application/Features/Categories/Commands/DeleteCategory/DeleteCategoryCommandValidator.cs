using FluentValidation;

namespace SuperStorage.Application.Features.Categories.Commands.DeleteCategory;

internal sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}
