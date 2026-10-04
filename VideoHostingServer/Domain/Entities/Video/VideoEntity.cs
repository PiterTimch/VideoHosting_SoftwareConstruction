using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Comments;
using Domain.Entities.Channel;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.Video;

[Table("tbl_videos")]
[Index(nameof(Slug), IsUnique = true)]
public class VideoEntity : BaseEntity<long>
{
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Image { get; set; }

    [StringLength(255)]
    public string? Video { get; set; }

    [Range(0, long.MaxValue)]
    public long ViewCount { get; set; }

    [Required]
    public long PrivacyId { get; set; }

    [ForeignKey(nameof(PrivacyId))]
    public virtual VideoPrivacyEntity? Privacy { get; set; }

    public long? ChannelId { get; set; }

    [ForeignKey(nameof(ChannelId))]
    public virtual ChannelEntity? Channel { get; set; }

    public virtual ICollection<CommentsEntity> Comments { get; set; } = new List<CommentsEntity>();
    public virtual ICollection<VideoReactionEntity> VideoReactions { get; set; } = new List<VideoReactionEntity>();
}
