using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Identity;
using Domain.Entities.Video;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.Comments;

[Table("tbl_comments")]
[Index(nameof(VideoId))]
public class CommentsEntity : BaseEntity<long>
{
    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public int LikesCount { get; set; } = 0;
    public int DislikeCount { get; set; } = 0;
    public int RepliesCount { get; set; } = 0;

    public bool IsEdited { get; set; } = false;

    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual UserEntity User { get; set; } = null!;

    public long VideoId { get; set; }
    public virtual VideoEntity Video { get; set; } = null!;

    public long? ParentId { get; set; }

    [ForeignKey(nameof(ParentId))]
    public virtual CommentsEntity? Parent { get; set; }

    public virtual ICollection<CommentsEntity> Replies { get; set; } = new List<CommentsEntity>();
}
