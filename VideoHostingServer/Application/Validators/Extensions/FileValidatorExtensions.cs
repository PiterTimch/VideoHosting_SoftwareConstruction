using Application.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Validators.Extensions;

public static class FileValidatorExtensions
{
    public static IRuleBuilderOptions<T, IFormFile?> IsImage<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder)
    {
        return ruleBuilder
            .Must(file =>
            {
                if (file == null) return true;
                
                var extension = Path.GetExtension(file.FileName).ToLower();
                return FileConstants.AllowedImageExtensions.Contains(extension) &&
                       FileConstants.AllowedImageContentTypes.Contains(file.ContentType.ToLower());
            })
            .WithMessage($"Недопустимий тип файлу. Дозволені формати: {string.Join(", ", FileConstants.AllowedImageExtensions)}");
    }

    public static IRuleBuilderOptions<T, IFormFile?> IsVideo<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder)
    {
        return ruleBuilder
            .Must(file =>
            {
                if (file == null) return true;

                var extension = Path.GetExtension(file.FileName).ToLower();
                return FileConstants.AllowedVideoExtensions.Contains(extension) &&
                       FileConstants.AllowedVideoContentTypes.Contains(file.ContentType.ToLower());
            })
            .WithMessage($"Недопустимий тип відео. Дозволені формати: {string.Join(", ", FileConstants.AllowedVideoExtensions)}");
    }
}
