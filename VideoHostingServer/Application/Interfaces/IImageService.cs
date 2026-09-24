using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile file);
    Task<string> SaveImageFromBase64Async(string input);
    Task<string> SaveImageFromUrlAsync(string imageUrl);
    Task DeleteImageAsync(string name);
}
