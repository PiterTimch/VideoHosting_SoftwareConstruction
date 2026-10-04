using Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Video;

[Table("tbl_video_reactions")]
public class VideoReactionEntity : BaseEntity<long>
{
    public long VideoId { get; set; }
    public VideoEntity Video { get; set; } = null!;

    public long UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public bool IsLike { get; set; }
}
