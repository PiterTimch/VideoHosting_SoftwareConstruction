using Application.Models.Search;

namespace Application.Models.Video;

public class VideoSearchModel : BaseSearchParamsModel
{
    public string? Q { get; set; }
    public string? Title { get; set; }
    public string? ChannelName { get; set; }
    public string? CreateYearFrom { get; set; }
    public string? CreateYearTo { get; set; }
    public string? SortBy { get; set; }
    public long? ChannelId { get; set; }
    public bool? IsLiked { get; set; }
}
