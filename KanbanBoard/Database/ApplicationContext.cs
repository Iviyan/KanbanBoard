using TaskStatus = KanbanBoard.Models.TaskStatus;
using Task = KanbanBoard.Models.Task;

//Scaffold-DbContext "Host=localhost;Port=5432;Database=kanban_board;Username=postgres;Password=123" -OutputDir Models1 -Namespace KanbanBoard.Models1 -DataAnnotations Npgsql.EntityFrameworkCore.PostgreSQL

namespace KanbanBoard.Database;

public class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Task> Tasks { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    
    static ApplicationContext()
        => NpgsqlConnection.GlobalTypeMapper.MapEnum<TaskStatus>("task_status");

    public ApplicationContext() : base()
    {
    }

    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
#if DEBUG
        optionsBuilder.LogTo(m => Debug.WriteLine(m), LogLevel.Trace)
            .EnableSensitiveDataLogging();
#endif
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<TaskStatus>(null, "task_status");
    }
}