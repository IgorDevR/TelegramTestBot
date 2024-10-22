using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramTestBot.Data;
using TelegramTestBot.Models;

namespace TelegramTestBot.Services;

public interface ICommonMessages
{
    public Task AskName(long tgId);
    public Task SendAvailableActions(UserProfile userProfile);
    public Task SendStartButton(UserProfile userProfile);
}
public class CommonMessages
{
    private readonly MyContext _context;
    private readonly ITelegramBotClient _botClient;
    private readonly ButtonService _buttonService;



    public CommonMessages(MyContext context, ITelegramBotClient botClient, ButtonService buttonService)
    {
        _context = context;
        _botClient = botClient;
        _buttonService = buttonService;
    }

    public async Task AskName(long tgId)
    {
        string text = """
                      Как вас зовут? Напишите Ваше имя и фамилию.
                      """;

        await _botClient.SendTextMessageAsync(tgId, text);
    }

    public async Task SendRegistrationRequiredMessage(long chatId)
    {
        await _botClient.SendTextMessageAsync(chatId,
            "Эта информация доступна только зарегистрированным пользователям. Выполните команду /start");
    }
    public async Task SendAccessDeniedForRoleMessage(long chatId, string role)
    {
        await _botClient.SendTextMessageAsync(chatId, $"Эта информация доступна только {role}.");
    }

    public async Task SendAvailableActions(UserProfile userProfile, string afterText = "", string beforeText = "")
    {
        string text = $"""
                       {afterText}Выберите действие.{beforeText}
                       """;

        var buttonViewingProfile =
            await _buttonService.CreateButton(userProfile, EventType.ViewingProfile, BusinessProcess.ViewProfiles);

        var buttonViewingMembersOfUsers =
            await _buttonService.CreateButton(userProfile, EventType.ViewMembersOfUser,
                BusinessProcess.ViewMembersOfUser);

        var buttonViewInviteCode =
            await _buttonService.CreateButton(userProfile, EventType.ViewingInviteCode,
                BusinessProcess.ViewingInviteCode);

        if (_context.ChangeTracker.HasChanges())
            await _context.SaveChangesAsync();

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Профиль", buttonViewingProfile.Id.ToString()), },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Смотреть вашу группу",
                    buttonViewingMembersOfUsers.Id.ToString()),
                InlineKeyboardButton.WithCallbackData("Показать код приглашения", buttonViewInviteCode.Id.ToString())
            }
        });

        await _botClient.SendTextMessageAsync(userProfile.TgId, text, replyMarkup: inlineKeyboard);
    }

    public async Task SendStartButton(UserProfile userProfile)
    {
        var student =
            await _buttonService.CreateButton(userProfile, EventType.StudentRegistration, BusinessProcess.Registration,
                true);
        var teacher =
            await _buttonService.CreateButton(userProfile, EventType.TeacherRegistration, BusinessProcess.Registration,
                true);
        var admin = await _buttonService.CreateButton(userProfile, EventType.AdminRegistration,
            BusinessProcess.Registration, true);

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("Студент", student.Id.ToString()),
            InlineKeyboardButton.WithCallbackData("Учитель", teacher.Id.ToString()),
            InlineKeyboardButton.WithCallbackData("Админ", admin.Id.ToString()),
        });

        await _botClient.SendTextMessageAsync(
            userProfile.TgId,
            "Добро пожаловать! Для начала выберете тип учетной записи. Тестовый код для Админа - 'www', для Учителя - 'qqq'",
            replyMarkup: inlineKeyboard
        );
    }
}