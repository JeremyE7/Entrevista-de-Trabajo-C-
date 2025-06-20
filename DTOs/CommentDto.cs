namespace TodoApi.DTOs
{
    public class CreateCommentDto
    {
        public string CommentText { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
    }

    public class UpdateCommentDto
    {
        public string CommentText { get; set; } = string.Empty;
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public bool IsUpdated { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? ParentCommentId { get; set; }
        public List<CommentDto> Replies { get; set; } = new List<CommentDto>();
    }
}
