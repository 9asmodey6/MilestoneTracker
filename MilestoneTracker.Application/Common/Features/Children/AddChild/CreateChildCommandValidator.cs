namespace MilestoneTracker.Application.Common.Features.Children.AddChild;

using FluentValidation;

public class CreateChildCommandValidator : AbstractValidator<CreateChildCommand>
{
    public CreateChildCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Имя не может быть пустым")
            .MaximumLength(50).WithMessage("Имя слишком длинное");

        RuleFor(x => x.Date)
            .NotEmpty()
            .LessThan(DateTime.Now).WithMessage("Дата рождения не может быть в будущем");

        RuleFor(x => x.ParentId)
            .GreaterThan(0);
    }
}