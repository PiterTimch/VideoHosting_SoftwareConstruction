using Microsoft.AspNetCore.Http;

namespace Application.Models.Video;

public class VideoUpdateModel
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IFormFile? Image { get; set; }
    public IFormFile? Video { get; set; }
    public long PrivacyId { get; set; }
}
