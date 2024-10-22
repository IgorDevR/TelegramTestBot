using Microsoft.EntityFrameworkCore;
using TelegramTestBot.Data;
using TelegramTestBot.Models;

namespace TelegramTestBot.Services;

public class UserEventsService
{
    private readonly MyContext _context;

    public UserEventsService(MyContext context)
    {
        _context = context;
    }

    public async Task<UserEvent?> GetLastUserEvent(long? userId = null, long? telegramId = null)
    {
        var result = await _context.Users
            .Where(c => (userId == null || c.Id == userId.Value) && (telegramId == null || c.TgId == telegramId.Value))
            .Include(userProfile => userProfile.UserEvents)
            .FirstOrDefaultAsync();

        return result?.UserEvents.LastOrDefault();
    }

    public async Task<UserEvent> AddNewEvent(UserProfile userProfile, EventType eventType,
        BusinessProcess businessProcess)
    {
        var userEvent = new UserEvent
        {
            UserId = userProfile.Id,
            EventType = eventType,
            BusinessProcess = businessProcess,
            Created = DateTimeOffset.UtcNow
        };

        await _context.UserEvents.AddAsync(userEvent);
        await _context.SaveChangesAsync();

        return userEvent;
    }
}