namespace Application.Models.Channel;

public class ChannelItemModel
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NickName { get; set; } = string.Empty;
    public int SubscriberCount { get; set; }
    public string? Description { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string? Contacts { get; set; }
    public string? Settings { get; set; }
    public string? AvatarImage { get; set; }
    public string? BannerImage { get; set; }
    public bool IsSubscribed { get; set; }
}
