using System.ComponentModel.DataAnnotations;

namespace TelegramTestBot.Models;

public class UserProfile
{
    [Key]
    public long Id { get; set; }

    public long TgId { get; set; }
    public string? TgUsername { get; set; }
    public string? TgLastName { get; set; }
    public string? TgFirstName { get; set; }

    public string? FullName { get; set; }
    public string? Occupation { get; set; }
    public string? Email { get; set; }

    public bool Blocked { get; set; }
    public DateTimeOffset? Registered { get; set; }
    public DateTimeOffset? Updated { get; set; }
    public DateTimeOffset? Moderated { get; set; }

    public UserRole Role { get; set; }
    public ProfileModerationStatus ModerationStatus { get; set; } = ProfileModerationStatus.Creation;

    public long? InviteId { get; set; }

    public List<UserEvent> UserEvents { get; set; }
    public List<Invite> Invites { get; set; }

    public override string ToString()
    {
        return $"User {Id}: {TgUsername} ({TgFirstName} {TgLastName})";
    }
}

public enum UserRole
{
    Unknown = 0,
    Admin = 10,
    Teacher = 20,
    Student = 30,
}

public enum ProfileModerationStatus
{
    Creation = 100,
    OnModeration = 200,
    Moderated = 300,
    TimellyBanned = 400,
    FinallyBanned = 500
}