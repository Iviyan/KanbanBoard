namespace KanbanBoard.Models;

[Table("project_members")]
public class ProjectMember
{
    [Column("user_id", Order = 0), Key] public int UserId { get; set; }
    [Column("project_id", Order = 1), Key] public int ProjectId { get; set; }
    
    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
    [ForeignKey("ProjectId")] public virtual Project Project { get; set; } = null!;
}

public class ProjectMemberPostDto
{
    public string? Email { get; set; }
}

public class ProjectMemberPostValidator : AbstractValidator<ProjectMemberPostDto>
{
    public ProjectMemberPostValidator()
    {
        RuleFor(task => task.Email).NotNull();
    }
}