using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramTestBot.Data;

namespace TelegramTestBot.Services;

public class StartService
{
    private readonly MyContext _context;
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<StartService> _logger;
    private readonly InviteService _inviteService;
    private readonly CallbackUserProfileService _userProfileService;
    private readonly ButtonService _buttonService;
    private readonly UserEventsService _userEventsService;
    private readonly UserService _userService;
    private readonly CommonMessages _commonMessages;

    public StartService(
        ITelegramBotClient botClient,
        ILogger<StartService> logger,
        MyContext context,
        InviteService inviteService, CallbackUserProfileService userProfileService, ButtonService buttonService,
        UserEventsService userEventsService, UserService userService, CommonMessages commonMessages)
    {
        _context = context;
        _inviteService = inviteService;
        _userProfileService = userProfileService;
        _buttonService = buttonService;
        _userEventsService = userEventsService;
        _userService = userService;
        _commonMessages = commonMessages;
        _botClient = botClient;
        _logger = logger;
    }

    public async Task StartCommand(Message message)
    {
        var chatId = message.Chat.Id;
        var messageText = message.Text!;

        var userProfile = await _userService.GetUserProfileByChatId(chatId);
        var lastEvent = await _userEventsService.GetLastUserEvent(telegramId: chatId);

        if (messageText.StartsWith("/start") && userProfile == null)
        {
            userProfile = await _userService.CheckAndCreateUser(message);
            await _commonMessages.SendStartButton(userProfile);

        }
        else if (lastEvent == null || lastEvent.EventType == EventType.Start)
        {
            await _commonMessages.SendRegistrationRequiredMessage(chatId);

        }

        else if (messageText.StartsWith("/invitations") && lastEvent.EventType != EventType.Start)
        {
            await HandleInviteCommand(chatId);
        }
        else if (messageText.StartsWith("/participants") && lastEvent.EventType != EventType.Start)
        {
            await HandleParticipantsCommand(chatId);
        }
        else
        {
            await _botClient.SendTextMessageAsync(chatId,
                "Неизвестная команда. " +
                "Доступные команды: /participants,  /invitations, /start");
        }
    }

    private async Task HandleInviteCommand(long chatId)
    {
        await _inviteService.CreateAndSendInviteByUserId(chatId);
    }

    private async Task HandleParticipantsCommand(long chatId)
    {
        await _userProfileService.SendUsersInvitedByUser(chatId);
    }
}