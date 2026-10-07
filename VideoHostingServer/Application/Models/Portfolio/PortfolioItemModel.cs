namespace Application.Models.Portfolio;

public class PortfolioItemModel
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
