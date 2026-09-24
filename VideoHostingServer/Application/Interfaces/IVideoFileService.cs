using Microsoft.AspNetCore.Http;
using Application.Models.VideoProcessing;

namespace Application.Interfaces;

public interface IVideoFileService
{
    Task<string> SaveVideoAsync(IFormFile file);
    Task<string> SaveVideoFromFilePathAsync(string filePath);
    Task<string> SaveVideoWithProgressAsync(string filePath, Action<VideoProgressUpdate> onProgress);
    Task DeleteVideoAsync(string name);
}
