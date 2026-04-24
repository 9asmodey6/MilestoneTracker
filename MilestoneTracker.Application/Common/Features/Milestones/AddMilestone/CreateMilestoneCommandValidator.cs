namespace MilestoneTracker.Application.Common.Features.Milestones.AddMilestone;

using FluentValidation;
using Models;

public class CreateMilestoneCommandValidator : AbstractValidator<CreateMilestoneCommand>
{
    public CreateMilestoneCommandValidator()
    {
        RuleFor(x => x.CreatorId)
            .NotEmpty().WithMessage("ID создателя не найден. Попробуйте перезапустить процесс.");

        RuleFor(x => x.ChildId)
            .NotEmpty().WithMessage("Не выбран ребенок, для которого создается воспоминание.");

        RuleFor(x => x.Category)
            .NotNull().WithMessage("Выберите категорию события.");

        RuleFor(x => x.OccuredAt)
            .NotEmpty().WithMessage("Дата события обязательна.")
            .Must(date => date <= DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("Дата не может быть в будущем. Мы ведь записываем воспоминания, а не предсказания :)");

        RuleFor(x => x.Title)
            .MaximumLength(100).WithMessage("Заголовок не должен превышать 100 символов.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Пожалуйста, опишите событие. Это сердце вашего воспоминания.")
            .MaximumLength(1000).WithMessage("Описание слишком длинное (максимум 1000 символов).");

        RuleFor(x => x.MediaFiles)
            .Must(m => m == null || m.Count <= 10)
            .WithMessage("В одно воспоминание можно добавить не более 10 файлов.");
    }
}