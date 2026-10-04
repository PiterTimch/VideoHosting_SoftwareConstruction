namespace Application.Models.Comments;

public class CommentsItemModal
{
    public long Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public int LikesCount { get; set; }
    public int DislikeCount { get; set; }
    public int RepliesCount { get; set; }
    public bool IsEdited { get; set; }

    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserImage { get; set; }
}
