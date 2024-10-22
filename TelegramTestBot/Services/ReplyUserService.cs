using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramTestBot.Data;
using TelegramTestBot.Models;

namespace TelegramTestBot.Services;

public class ReplyUserProfileService : ICommonMessages
{
    private readonly MyContext _context;
    private readonly ITelegramBotClient _botClient;
    private readonly UserEventsService _userEventsService;
    private readonly ButtonService _buttonService;
    private readonly CommonMessages _commonMessages;
    private readonly UserService _userService;

    public ReplyUserProfileService(MyContext context, ITelegramBotClient botClient, UserEventsService userEventsService,
        ButtonService buttonService, CommonMessages commonMessages, UserService userService)
    {
        _context = context;
        _botClient = botClient;
        _userEventsService = userEventsService;
        _buttonService = buttonService;
        _commonMessages = commonMessages;
        _userService = userService;
    }

    public async Task RegistrationMessage(Message message, UserProfile userProfile)
    {
        var userId = userProfile.Id;
        var tgId = userProfile.TgId;
        var userEvent = await _userEventsService.GetLastUserEvent(userId);

        if (userEvent == null)
            return;

        if (userEvent.EventType == EventType.TeacherRegistration || userEvent.EventType == EventType.AdminRegistration)
        {
            var role = userEvent.EventType == EventType.TeacherRegistration ? UserRole.Teacher : UserRole.Admin;

            var invite = await _context.Invites.FirstOrDefaultAsync(i =>
                i.Code == message.Text && i.Role == role && i.IsActive);

            if (invite == null)
            {
                await WrongInviteCode(tgId);
                return;
            }

            await _userService.SaveInviteAndRole(userProfile, invite);
            await AskName(tgId);
            await _userEventsService.AddNewEvent(userProfile, EventType.AddName, BusinessProcess.FillOutProfile);
        }
        else if (userEvent.EventType == EventType.StudentRegistration)
        {
            await _userEventsService.AddNewEvent(userProfile, EventType.AddName, BusinessProcess.FillOutProfile);
            await FillOutProfileMessage(message, userProfile);
        }
        else
        {
            await SendStartButton(userProfile);
        }
    }

    public async Task FillOutProfileMessage(Message message, UserProfile userProfile)
    {
        var userId = userProfile.Id;
        var tgId = userProfile.TgId;
        var userEvent = await _userEventsService.GetLastUserEvent(userId);

        if (userEvent == null)
            return;

        switch (userEvent.EventType)
        {
            case EventType.AddName:
                await _userService.SaveName(userProfile, message.Text!);
                await AskOccupation(tgId);
                await _userEventsService.AddNewEvent(userProfile, EventType.AddOccupation,
                    BusinessProcess.FillOutProfile);
                break;

            case EventType.AddOccupation:
                await _userService.SaveOccupation(userProfile, message.Text!);
                await AskEmail(tgId);
                await _userEventsService.AddNewEvent(userProfile, EventType.AddEmail, BusinessProcess.FillOutProfile);
                break;

            case EventType.AddEmail:
                await _userService.SaveEmail(userProfile, message.Text!);
                await _userEventsService.AddNewEvent(userProfile, EventType.ProfileEnd, BusinessProcess.ProfileIsFull);

                await _userService.SetProfileToModeration(userId);
                await SendProfileIsComplete(userProfile);
                break;

            case EventType.ProfileEnd:
                await _userEventsService.AddNewEvent(userProfile, EventType.ProfileEnd, BusinessProcess.ProfileIsFull);
                await SendAvailableActions(userProfile);
                break;
        }
    }

    public async Task AskName(long tgId)
    {
        await _commonMessages.AskName(tgId);
    }

    private async Task AskOccupation(long tgId)
    {
        string text = """
                      Ваша сфера дейтельности?
                      """;

        await _botClient.SendTextMessageAsync(tgId, text);
    }

    private async Task AskEmail(long tgId)
    {
        string text = """
                      Ваш адрес электронной почты?
                      """;

        await _botClient.SendTextMessageAsync(tgId, text);
    }

    public async Task SendProfileIsComplete(UserProfile userProfile)
    {
        string text = "Спасибо за заполнение профиля. Он будет направлен на модерацию. ";
        await _commonMessages.SendAvailableActions(userProfile, afterText: text);
    }

    public async Task SendAvailableActions(UserProfile userProfile)
    {
        await _commonMessages.SendAvailableActions(userProfile);
    }

    public async Task SendStartButton(UserProfile userProfile)
    {
        await _commonMessages.SendStartButton(userProfile);
    }

    private async Task WrongInviteCode(long tgId)
    {
        string text = """
                      Неверный код приглашения. Проверьте и попробуйде еще раз.
                      """;

        await _botClient.SendTextMessageAsync(tgId, text);
    }
}