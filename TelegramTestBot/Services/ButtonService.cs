using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramTestBot.Data;
using TelegramTestBot.Models;

namespace TelegramTestBot.Services;

public class ButtonService
{
    private readonly MyContext _context;
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<ButtonService> _logger;

    public ButtonService(
        ITelegramBotClient botClient,
        ILogger<ButtonService> logger,
        MyContext context
    )
    {
        _context = context;
        _botClient = botClient;
        _logger = logger;
    }

    public async Task<ActionButton?> GetButton(long id)
    {
        return await _context.Buttons.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<ActionButton> CreateButton(UserProfile userProfile, EventType buttonEvent,
        BusinessProcess businessProcess, bool useExistingButton = false)
    {
        if (useExistingButton)
        {
            var existingButton = await _context.Buttons
                .FirstOrDefaultAsync(b => b.UserId == userProfile.Id && b.ButtonEvent == buttonEvent &&
                                          b.BusinessProcess == businessProcess);

            if (existingButton != null)
            {
                return existingButton;
            }
        }

        var newButton = new ActionButton
        {
            BusinessProcess = businessProcess,
            ButtonEvent = buttonEvent,
            TgId = userProfile.TgId,
            UserId = userProfile.Id,
            Created = DateTimeOffset.UtcNow
        };

        await _context.Buttons.AddAsync(newButton);
        await _context.SaveChangesAsync();

        return newButton;
    }

}