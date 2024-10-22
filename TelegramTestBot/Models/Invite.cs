using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramTestBot.Models;

public class Invite
{
    [Key]
    public long Id { get; set; }

    public string Code { get; set; }
    public bool IsActive { get; set; }
    public UserRole Role { get; set; }


    public long? CreatedByUserProfileId { get; set; }
    public UserProfile? CreatedByUserProfile { get; set; }
}