using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramTestBot.Services;
using Update = Telegram.Bot.Types.Update;

namespace TelegramTestBot.Utils;

public class BotEventsHandler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BotEventsHandler> _logger;

    public BotEventsHandler(
        IServiceScopeFactory scopeFactory, ITelegramBotClient botClient,
        ILogger<BotEventsHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var message = update.Message!;

        if (update.Type == UpdateType.Message && message?.Text != null)
        {
            await MessagesHandler(update, message);
        }
        else if (update.Type == UpdateType.CallbackQuery && update?.CallbackQuery != null)
        {
            await CallbackHandler(update.CallbackQuery);
        }
    }

    private async Task MessagesHandler(Update update, Message message)
    {
        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var user = await userService.GetUserByTelegramId(message!.Chat.Id);

        if (message.Text == "/start" || message.Text.StartsWith("/") || message.Text == "/")
        {
            var startService = scope.ServiceProvider.GetRequiredService<StartService>();
            await startService.StartCommand(update.Message!);
            return;
        }

        var userEventsService = scope.ServiceProvider.GetRequiredService<UserEventsService>();
        var userEvent = await userEventsService.GetLastUserEvent(telegramId: message!.Chat.Id);

        if (user != null && userEvent != null)
        {
            switch (userEvent.BusinessProcess)
            {
                case BusinessProcess.Registration:
                    {
                        var replyUserProfileService =
                            scope.ServiceProvider.GetRequiredService<ReplyUserProfileService>();
                        await replyUserProfileService.RegistrationMessage(message, user);
                        return;
                    }
                case BusinessProcess.FillOutProfile:
                    {
                        var replyUserProfileService =
                            scope.ServiceProvider.GetRequiredService<ReplyUserProfileService>();
                        await replyUserProfileService.FillOutProfileMessage(message, user);
                        return;
                    }
                case BusinessProcess.ProfileIsFull:
                    {
                        var profileService = scope.ServiceProvider.GetRequiredService<CallbackUserProfileService>();
                        await profileService.ProfileIsFullMessage(message, user);
                        return;
                    }
            }
        }
        else
        {
            var startService = scope.ServiceProvider.GetRequiredService<StartService>();
            await startService.StartCommand(update.Message!);
        }
    }

    private async Task CallbackHandler(CallbackQuery callbackQuery)
    {
        using var scope = _scopeFactory.CreateScope();
        var buttonService = scope.ServiceProvider.GetRequiredService<ButtonService>();
        var button = await buttonService.GetButton(long.Parse(callbackQuery.Data!));

        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var user = await userService.GetUserByTelegramId(callbackQuery.From.Id);

        if (button == null || user == null)
        {
            var commonMessages = scope.ServiceProvider.GetRequiredService<CommonMessages>();
            await commonMessages.SendRegistrationRequiredMessage(callbackQuery.From.Id);
            return;
        }

        var callbackUserProfileUserService =
            scope.ServiceProvider.GetRequiredService<CallbackUserProfileService>();

        switch (button!.BusinessProcess)
        {
            case BusinessProcess.Registration:
                await callbackUserProfileUserService.StartRegistration(button);
                return;

            case BusinessProcess.ViewProfiles:
                await callbackUserProfileUserService.GetAndSendUserProfileInfo(callbackQuery.From.Id);
                return;

            case BusinessProcess.ViewingInviteCode:
                {
                    var inviteService = scope.ServiceProvider.GetRequiredService<InviteService>();
                    await inviteService.CreateAndSendInviteByUserId(callbackQuery.From.Id);
                    return;
                }
            case BusinessProcess.ViewMembersOfUser:
                await callbackUserProfileUserService.SendUsersInvitedByUser(callbackQuery.From.Id);
                return;
        }
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(JsonConvert.SerializeObject(exception));

        await Task.CompletedTask;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}