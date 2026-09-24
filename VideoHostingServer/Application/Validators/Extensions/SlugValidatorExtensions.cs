using FluentValidation;
using System.Text.RegularExpressions;

namespace Application.Validators.Extensions;

public static class SlugValidatorExtensions
{
    private static readonly Regex SlugRegex = new(@"^[a-z0-9-]+$", RegexOptions.Compiled);

    public static IRuleBuilderOptions<T, string> IsSlug<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(slug => string.IsNullOrEmpty(slug) || SlugRegex.IsMatch(slug))
            .WithMessage("Слаг повинен містити лише латинські малі букви, дефіс та цифри");
    }
}
