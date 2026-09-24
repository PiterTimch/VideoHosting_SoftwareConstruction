using Application.Models.Video;
using FluentValidation;

namespace Application.Validators.Video;

public class VideoSearchModelValidator : AbstractValidator<VideoSearchModel>
{
    public VideoSearchModelValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Номер сторінки повинен бути більшим або дорівнювати 1");

        RuleFor(x => x.ItemPerPage)
            .InclusiveBetween(1, 100).WithMessage("Кількість елементів на сторінці повинна бути від 1 до 100");

        RuleFor(x => x.Q)
            .MaximumLength(100).WithMessage("Пошуковий запит не повинен перевищувати 100 символів");

        RuleFor(x => x.Title)
            .MaximumLength(255).WithMessage("Назва для пошуку не повинна перевищувати 255 символів");

        RuleFor(x => x.CreateYearFrom)
            .Must(BeValidYear).WithMessage("Рік створення 'від' повинен бути коректним 4-значним роком (наприклад, 2024)")
            .When(x => !string.IsNullOrEmpty(x.CreateYearFrom));

        RuleFor(x => x.CreateYearTo)
            .Must(BeValidYear).WithMessage("Рік створення 'до' повинен бути коректним 4-значним роком (наприклад, 2024)")
            .When(x => !string.IsNullOrEmpty(x.CreateYearTo));

        RuleFor(x => x)
            .Must(HaveValidYearRange).WithMessage("Рік 'від' не може бути більшим за рік 'до'")
            .When(x => !string.IsNullOrEmpty(x.CreateYearFrom) && !string.IsNullOrEmpty(x.CreateYearTo));
    }

    private static bool BeValidYear(string? yearStr)
    {
        if (int.TryParse(yearStr, out int year))
        {
            return year >= 1900 && year <= DateTime.Now.Year + 1;
        }
        return false;
    }

    private static bool HaveValidYearRange(VideoSearchModel model)
    {
        if (int.TryParse(model.CreateYearFrom, out int fromYear) &&
            int.TryParse(model.CreateYearTo, out int toYear))
        {
            return fromYear <= toYear;
        }
        return true;
    }
}
