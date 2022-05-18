namespace KanbanBoard.Models;

[Table("tasks")]
public class Task
{
    [Column("id")] public int Id { get; set; }
    [Column("name")] public string Name { get; set; } = null!;
    [Column("text")] public string Text { get; set; } = null!;
    [Column("user_id")] public int UserId { get; set; }
    [Column("project_id")] public int ProjectId { get; set; }
    [Column("creation_date")] public DateTime CreationDate { get; set; }
    [Column("status")] public TaskStatus Status { get; set; }

    [ForeignKey("ProjectId")] public virtual Project Project { get; set; } = null!;
    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
}

public class TaskPostDto
{
    public string? Name { get; set; }
    public string? Text { get; set; }
    public TaskStatus? Status { get; set; } = TaskStatus.Planned;
}

public class TaskPatchDto : PatchDtoBase
{
    private string? name;
    private string? text;
    private TaskStatus? status;

    public string? Name { get => name; set { name = value; SetHasProperty(); } }
    public string? Text { get => text; set { text = value; SetHasProperty(); } }
    public TaskStatus? Status { get => status; set { status = value; SetHasProperty(); } }
}

public record TaskGetDto(int Id, string Name, string Text, int UserId, DateTime CreationDate, TaskStatus Status);

public class TaskPostValidator : AbstractValidator<TaskPostDto>
{
    public TaskPostValidator()
    {
        RuleFor(task => task.Name).NotNull().Length(1, 50);
        RuleFor(task => task.Text).NotNull().Length(0, 5000);
        RuleFor(task => task.Status).NotNull();
    }
}

public class TaskPatchValidator : AbstractValidator<TaskPatchDto>
{
    public TaskPatchValidator()
    {
        RuleFor(task => task.Name).Length(1, 50);
        RuleFor(task => task.Text).Length(0, 5000);
    }
}