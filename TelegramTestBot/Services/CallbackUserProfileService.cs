using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramTestBot.Data;
using TelegramTestBot.Models;

namespace TelegramTestBot.Services;

public class CallbackUserProfileService : ICommonMessages
{
    private readonly MyContext _context;
    private readonly ITelegramBotClient _botClient;
    private readonly UserEventsService _userEventsService;
    private readonly ButtonService _buttonService;
    private readonly CommonMessages _commonMessages;
    private readonly UserService _userService;

    public CallbackUserProfileService(MyContext context, ITelegramBotClient botClient,
        UserEventsService userEventsService, ButtonService buttonService, CommonMessages commonMessages, UserService userService)
    {
        _context = context;
        _botClient = botClient;
        _userEventsService = userEventsService;
        _buttonService = buttonService;
        _commonMessages = commonMessages;
        _userService = userService;
    }

    public async Task StartRegistration(ActionButton actionButton)
    {
        var userId = actionButton.UserId;
        var tgId = actionButton.TgId;

        var user = await _userService.GetUserProfileByUserId(userId);

        switch (actionButton.ButtonEvent)
        {
            case EventType.StudentRegistration:
                await AskName(tgId);
                await _userService.SaveInviteAndRole(user, null);
                await _userEventsService.AddNewEvent(user, EventType.StudentRegistration, BusinessProcess.Registration);
                break;

            case EventType.TeacherRegistration:
                await AskInviteCode(tgId);
                await _userEventsService.AddNewEvent(user, EventType.TeacherRegistration, BusinessProcess.Registration);
                break;

            case EventType.AdminRegistration:
                await AskInviteCode(tgId);
                await _userEventsService.AddNewEvent(user, EventType.AdminRegistration, BusinessProcess.Registration);
                break;

            default:
                await SendStartButton(user);
                break;
        }
    }

    public async Task ProfileIsFullMessage(Message message, UserProfile userProfile)
    {
        await SendAvailableActions(userProfile);
    }

    private async Task AskInviteCode(long tgId)
    {
        string text = """
                      Введите код приглашения.
                      """;

        await _botClient.SendTextMessageAsync(tgId, text);
    }

    public async Task AskName(long tgId)
    {
        await _commonMessages.AskName(tgId);
    }

    public async Task SendAvailableActions(UserProfile userProfile)
    {
        await _commonMessages.SendAvailableActions(userProfile);
    }

    public async Task SendStartButton(UserProfile userProfile)
    {
        await _commonMessages.SendStartButton(userProfile);
    }

    public async Task GetAndSendUserProfileInfo(long chatId)
    {
        var user = await _userService.GetUserProfileByChatId(chatId);

        if (user == null)
        {
            await _commonMessages.SendRegistrationRequiredMessage(chatId);
            return;
        }

        string message = $"Имя: {user.FullName}, Email: {user.Email}";
        await _botClient.SendTextMessageAsync(chatId, $"Информация о профиле::\n\n{message}");
    }

    public async Task SendUsersInvitedByUser(long chatId)
    {
        var user = await _userService.GetUserProfileByChatId(chatId);

        if (user == null)
        {
            await _commonMessages.SendRegistrationRequiredMessage(chatId);
            return;
        }

        var userInvites = user.Invites;
        if (!userInvites.Any())
        {
            await _botClient.SendTextMessageAsync(chatId, $"По вашей ссылке еще нет пользовтелей.");
            return;
        }

        var userInviteIds = userInvites.Select(i => i.Id).ToHashSet();

        var membersOfUser = await _context.Users
            .Where(user => user.InviteId != null && userInviteIds.Contains(user.InviteId.Value))
            .ToListAsync();

        string userList = string.Join("\n",
            membersOfUser.Select((user, index) => $"{index + 1}. Имя: {user.FullName}, Email: {user.Email}, Role: {user.Role}"));

        await _botClient.SendTextMessageAsync(chatId, $"Список пользователей по вашей ссылке:\n\n{userList}");
    }
}