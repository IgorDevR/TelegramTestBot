using System.ComponentModel.DataAnnotations;

public class ActionButton
{
    [Key]
    public long Id { get; set; }

    public long UserId { get; set; }
    public long TgId { get; set; }

    public EventType ButtonEvent { get; set; }
    public BusinessProcess BusinessProcess { get; set; }
    public DateTimeOffset Created { get; set; }
    public bool IsInactive { get; set; }
}