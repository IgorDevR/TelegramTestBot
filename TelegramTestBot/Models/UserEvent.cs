using System.ComponentModel.DataAnnotations;
using TelegramTestBot.Models;

public class UserEvent
{
    [Key]
    public long Id { get; set; }

    public EventType EventType { get; set; }
    public BusinessProcess BusinessProcess { get; set; }
    public DateTimeOffset Created { get; set; }

    public long UserId { get; set; }
    public UserProfile? User { get; set; }
}

public enum BusinessProcess
{
    FillOutProfile = 100,
    ProfileIsFull = 200,
    ViewProfiles = 300,
    ViewingInviteCode = 400,
    ViewMembersOfUser = 500,
    Registration = 600,
}

public enum EventType
{
    Start = 0,

    AddInviteCode = 1000,

    AddName = 1100,
    AddOccupation = 1400,
    AddEmail = 1700,

    ProfileEnd = 1900,

    ViewingProfile = 3000,
    ViewingInviteCode = 3600,
    ViewMembersOfUser = 3700,
    TeacherRegistration = 3800,
    StudentRegistration = 3900,
    AdminRegistration = 4000,
}