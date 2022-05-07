namespace KanbanBoard;

public class RequestData
{
    public int? UserId { get; set; }
    public string? UserEmail { get; set; }
    public Guid DeviceUid { get; set; }
}