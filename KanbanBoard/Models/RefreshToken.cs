using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanBoard.Models;

[Table("refresh_tokens")]
public class RefreshToken
{
    [Column("id")] public Guid Id { get; set; }
    [Column("user_id")] public int UserId { get; set; }
    [Column("device_uid")] public Guid DeviceUid { get; set; }
    [Column("expires")] public DateTime Expires { get; set; }
    
    [ForeignKey("UserId")] public User? User { get; set; }
    
    public bool IsExpired => DateTime.Now >= Expires;
}