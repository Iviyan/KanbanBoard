namespace KanbanBoard.Models;

[Table("users")]
public class User
{
    [Column("id")] public int Id { get; set; }
    [Column("email")] public string Email { get; set; } = null!;
    [Column("password")] public string Password { get; set; } = null!;
    [Column("name")] public string Name { get; set; } = null!;

    public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();
    public virtual ICollection<Task> Tasks { get; set; } = new HashSet<Task>();
}
