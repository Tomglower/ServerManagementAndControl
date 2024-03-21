using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ControlPanel.Core.Models;
using ControlPanel.Core.Result;
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

    public TelegramBotBackgroundService(
        ILogger<TelegramBotBackgroundService> logger,
        IConfiguration config,
        IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _logger = logger;
        _botClient = new TelegramBotClient(_config.GetValue<string>("Token") ?? string.Empty);
        _httpClient = httpClientFactory.CreateClient();
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

    // private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    // {
    //     if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
    //     {
    //         var chatId = update.Message.Chat.Id;
    //         var messageText = update.Message.Text;
    //
    //         _logger.LogInformation($"Received a '{messageText}' message in chat {chatId}.");
    //
    //         // Эхо ответ бота
    //         await botClient.SendTextMessageAsync(chatId, $"You said: {messageText}", cancellationToken: cancellationToken);
    //     }
    // }
  private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
    {
        var chatId = update.Message.Chat.Id;
        var messageText = update.Message.Text;

        _logger.LogInformation($"Received a '{messageText}' message in chat {chatId}.");

        if (IPAddress.TryParse(messageText, out var ipAddress))
        {
            try
            {
                var apiResponse = await GetApiResponse(ipAddress.ToString(), cancellationToken);
                await botClient.SendTextMessageAsync(chatId, apiResponse, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(chatId, "Произошла ошибка при обращении к API.", cancellationToken: cancellationToken);
                _logger.LogError(ex, "Error calling the API.");
            }
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, "Пожалуйста, отправьте валидный IP адрес.", cancellationToken: cancellationToken);
        }
    }
}

  private async Task<string> GetApiResponse(string ipAddress, CancellationToken cancellationToken)
  {
      var machineQueryRequest = new MachineQueryRequest
      {
          Link = ipAddress,
          Query = "node_load1"
      };

      var jsonRequest = JsonSerializer.Serialize(machineQueryRequest);
      var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

      var request = new HttpRequestMessage(HttpMethod.Post, $"http://localhost:5143/Prometheus/GetMetricsPrometheus")
      {
          Content = httpContent
      };

      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.GetValue<string>("bearer"));

      var response = await _httpClient.SendAsync(request, cancellationToken);

      if (!response.IsSuccessStatusCode)
      {
          throw new HttpRequestException($"API request failed with status code: {response.StatusCode}");
      }

      var content = await response.Content.ReadAsStringAsync(cancellationToken);
      return content;
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