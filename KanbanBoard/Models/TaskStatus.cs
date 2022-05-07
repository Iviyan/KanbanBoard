using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using NpgsqlTypes;

namespace KanbanBoard.Models;

public enum TaskStatus
{
    [PgName("planned")] Planned,
    [PgName("ongoing")] Ongoing,
    [PgName("completed")] Completed
}

// public static class TaskStatusExtensions
// {
//     public static string AsString(this TaskStatus status) => status switch
//     {
//         TaskStatus.Planned => "planned",
//         TaskStatus.Ongoing => "ongoing",
//         TaskStatus.Completed => "completed",
//         _ => null!
//     };
// }