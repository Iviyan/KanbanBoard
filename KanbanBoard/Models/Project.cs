using System.Reflection;

namespace KanbanBoard.Models;

[Table("projects")]
public class Project
{
    [Column("id")] public int Id { get; set; }
    [Column("name")] public string Name { get; set; } = null!;
    [Column("user_id")] public int UserId { get; set; }
    
    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
    public virtual ICollection<Task> Tasks { get; set; } = new HashSet<Task>();
}

public class ProjectPostDto : PatchDtoBase
{
    public string? Name { get; set; }
}

public class ProjectPatchDto : PatchDtoBase
{
    private string? name;

    public string? Name { get => name; set { name = value; SetHasProperty(); } }
}

public record ProjectGetDto(int Id, string Name);

public class ProjectValidator : AbstractValidator<ProjectPostDto>
{
    public ProjectValidator()
    {
        RuleFor(task => task.Name).NotNull().Length(1, 50);
    }
}