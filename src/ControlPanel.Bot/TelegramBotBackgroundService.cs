using Microsoft.Extensions.Configuration.UserSecrets;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;

public class TelegramBotBackgroundService : BackgroundService
{
    private readonly ILogger<TelegramBotBackgroundService> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public TelegramBotBackgroundService(ILogger<TelegramBotBackgroundService> logger, IConfiguration config)
    {
        _config = config;
        _logger = logger;
        _botClient = new TelegramBotClient(_config.GetValue<string>("Token") ?? string.Empty);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TelegramBotBackgroundService started.");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { } // Получаем все типы обновлений
        };

        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: stoppingToken);

        var me = await _botClient.GetMeAsync(stoppingToken);
        _logger.LogInformation($"Bot {me.Username} is up and running.");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
        {
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            _logger.LogInformation($"Received a '{messageText}' message in chat {chatId}.");

            // Эхо ответ бота
            await botClient.SendTextMessageAsync(chatId, $"You said: {messageText}", cancellationToken: cancellationToken);
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError(errorMessage);
        return Task.CompletedTask;
    }
}