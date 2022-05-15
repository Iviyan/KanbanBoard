namespace KanbanBoard.Models;

[Table("comments")]
public class Comment
{
    [Column("id")] public int Id { get; set; }
    [Column("text")] public string Text { get; set; } = null!;
    [Column("user_id")] public int UserId { get; set; }
    [Column("task_id")] public int TaskId { get; set; }
    [Column("creation_date")] public DateTime CreationDate { get; set; }

    [ForeignKey("TaskId")] public virtual Project Task { get; set; } = null!;
    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
}

public class CommentPostDto
{
    public string? Text { get; set; }
    public int? TaskId { get; set; }
}

public record CommentGetDto(int Id, string Text, int UserId, DateTime CreationDate);

public class CommentValidator : AbstractValidator<CommentPostDto>
{
    public CommentValidator()
    {
        RuleFor(comment => comment.Text).NotNull().Length(1, 5000);
        RuleFor(comment => comment.TaskId).NotNull();
    }
}