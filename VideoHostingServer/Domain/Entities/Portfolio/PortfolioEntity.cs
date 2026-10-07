using Domain.Entities.Video;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Identity;

namespace Domain.Entities.Portfolio;

[Table("tbl_portfolios")]
[Index(nameof(NickName), IsUnique = true)]
public class PortfolioEntity : BaseEntity<long>
{
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string NickName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Specialization { get; set; }

    [StringLength(500)]
    public string? Contacts { get; set; }

    [StringLength(1000)]
    public string? Settings { get; set; }

    [StringLength(255)]
    public string? AvatarImage { get; set; }

    [StringLength(255)]
    public string? BannerImage { get; set; }

    public virtual UserEntity? Freelancer { get; set; }

    public virtual ICollection<VideoEntity>? Videos { get; set; } = new List<VideoEntity>();

    public virtual ICollection<PortfolioSubscriberEntity>? Subscribers { get; set; } = new List<PortfolioSubscriberEntity>();
}
