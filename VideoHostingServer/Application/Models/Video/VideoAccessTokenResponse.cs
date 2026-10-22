namespace Application.Models.Video;

public class VideoAccessTokenResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
