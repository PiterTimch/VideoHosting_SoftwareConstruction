namespace Application.Models.Video;

public class VideoItemModel
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long ViewCount { get; set; }
    public string DateCreated { get; set; } = string.Empty;

    public string? Image { get; set; }
    public string? Video { get; set; }

    public VideoPrivacyItemModel? Privacy { get; set; }
}
