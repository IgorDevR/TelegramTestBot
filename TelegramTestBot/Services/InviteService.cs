using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using TelegramTestBot.Data;
using TelegramTestBot.Models;

namespace TelegramTestBot.Services;

public class InviteService
{
    private readonly MyContext _context;
    private readonly ITelegramBotClient _botClient;
    private readonly CommonMessages _commonMessages;
    private readonly UserService _userService;

    public InviteService(MyContext context, ITelegramBotClient botClient, CommonMessages commonMessages, UserService userService)
    {
        _context = context;
        _botClient = botClient;
        _commonMessages = commonMessages;
        _userService = userService;
    }

    public async Task CreateAndSendInviteByUserId(long chatId)
    {
        var user = await _userService.GetUserProfileByChatId(chatId);

        if (user == null)
        {
            await _commonMessages.SendRegistrationRequiredMessage(chatId);
            return;
        }
        if (user.Role != UserRole.Admin)
        {
            await _commonMessages.SendAccessDeniedForRoleMessage(chatId, UserRole.Admin.ToString());
            return;
        }

        var userInvites = user.Invites;
        if (userInvites.Count == 0)
        {
            await GenerateInviteCode(user, UserRole.Admin);
            await GenerateInviteCode(user, UserRole.Teacher);
        }

        string invites = string.Join("\n",
            userInvites.Select((invite, index) => $"{index + 1}. Role: {invite.Role}, Code: {invite.Code}"));

        await _botClient.SendTextMessageAsync(chatId, $"Ваши коды приглашения:\n\n{invites}");
    }

    public async Task GenerateInviteCode(UserProfile userProfileUserProfileProfile, UserRole role)
    {
        var inviteCode = Guid.NewGuid().ToString("N").Substring(0, 8);
        var invite = new Invite
        {
            Role = role,
            Code = inviteCode,
            IsActive = true,
            CreatedByUserProfileId = userProfileUserProfileProfile.Id
        };
        userProfileUserProfileProfile.Invites.Add(invite);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateInviteCode(string code)
    {
        return await _context.Invites.AnyAsync(i => i.Code == code && i.IsActive);
    }

    public async Task BlockInviteCode(string code)
    {
        var invite = await _context.Invites.FirstOrDefaultAsync(i => i.Code == code);
        if (invite != null)
        {
            invite.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}