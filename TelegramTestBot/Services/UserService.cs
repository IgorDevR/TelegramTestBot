using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using TelegramTestBot.Data;
using TelegramTestBot.Models;

namespace TelegramTestBot.Services;

public class UserService
{
    private readonly MyContext _context;

    public UserService(MyContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetUserProfileByChatId(long chatId)
    {
        return await _context.Users.Where(u => u.TgId == chatId)
            .Include(u => u.Invites)
            .FirstOrDefaultAsync();
    }

    public async Task<UserProfile?> GetUserProfileByUserId(long chatId)
    {
        return await _context.Users.Where(u => u.Id == chatId)
            .Include(u => u.Invites)
            .FirstOrDefaultAsync();
    }

    public async Task<UserProfile?> GetUserByTelegramId(long telegramId)
    {
        return await _context.Users
            .Where(u => u.TgId == telegramId)
            .Include(u => u.Invites)
            .FirstOrDefaultAsync();
    }

    public async Task<UserProfile> CheckAndCreateUser(Message message)
    {
        var user = await GetUserByTelegramId(message.Chat.Id);

        if (user == null)
        {
            user = new UserProfile
            {
                TgId = message.Chat.Id,
                TgUsername = message.Chat.Username,
                TgFirstName = message.Chat.FirstName,
                TgLastName = message.Chat.LastName,
                Registered = DateTimeOffset.UtcNow,
                UserEvents = new List<UserEvent>()
                {
                    new UserEvent
                    {
                        Created = DateTimeOffset.UtcNow,
                        EventType = EventType.Start,
                        BusinessProcess = BusinessProcess.Registration,
                        UserId = message.Chat.Id,
                    }
                }
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        return user;
    }

    public async Task SaveInviteAndRole(UserProfile userProfile, Invite? invite)
    {
        userProfile.InviteId = invite?.Id;
        userProfile.Role = invite?.Role ?? UserRole.Student;
        userProfile.Updated = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task SaveName(UserProfile userProfile, string text)
    {
        userProfile.FullName = text;
        userProfile.Updated = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task SaveOccupation(UserProfile userProfile, string text)
    {
        userProfile.Occupation = text;
        userProfile.Updated = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task SaveEmail(UserProfile userProfile, string text)
    {
        userProfile.Email = text;
        userProfile.Updated = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task SetProfileToModeration(long userId)
    {
        var user = await _context.Users.FirstAsync(u => u.Id == userId);
        user.ModerationStatus = ProfileModerationStatus.OnModeration;
        await _context.SaveChangesAsync();
    }
}