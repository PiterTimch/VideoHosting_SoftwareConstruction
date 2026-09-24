namespace Application.Models.Video;

public class VideoSeedModel
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PrivacySystemCode { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public string? VideoFile { get; set; }
}
