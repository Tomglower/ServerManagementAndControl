using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ControlPanel.Core.Models;
using ControlPanel.Core.Result;
using Microsoft.Extensions.Configuration.UserSecrets;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramWorker;
using ApiResponse = TelegramWorker.ApiResponse;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
    
    // private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
    //     CancellationToken cancellationToken)
    // {
    //     string id = null;
    //     if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
    //     {
    //         var chatId = update.Message.Chat.Id;
    //         var messageText = update.Message.Text;
    //
    //         _logger.LogInformation($"Received a '{messageText}' message in chat {chatId}.");
    //
    //         if (messageText.StartsWith("/start"))
    //         {
    //             var commandParts = messageText.Split(' ');
    //
    //             if (commandParts.Length > 1)
    //             {
    //                 id = commandParts[1];
    //
    //                 await botClient.SendTextMessageAsync(chatId, $"Привет! Ваш ID: {id}",
    //                     cancellationToken: cancellationToken);
    //                 
    //             }
    //             else
    //             {
    //                 await botClient.SendTextMessageAsync(chatId, "Привет! Как я могу помочь вам сегодня?",
    //                     cancellationToken: cancellationToken);
    //             }
    //         }
    //         else if (IPAddress.TryParse(messageText, out var ipAddress))
    //         {
    //             try
    //             {
    //                 var apiResponse = await GetApiResponse(ipAddress.ToString(), cancellationToken);
    //                 await botClient.SendTextMessageAsync(chatId, apiResponse, cancellationToken: cancellationToken);
    //             }
    //             catch (Exception ex)
    //             {
    //                 await botClient.SendTextMessageAsync(chatId, "Произошла ошибка при обращении к API.",
    //                     cancellationToken: cancellationToken);
    //                 _logger.LogError(ex, "Error calling the API.");
    //             }
    //         }
    //         else
    //         {
    //             await botClient.SendTextMessageAsync(chatId, "Пожалуйста, отправьте валидный IP адрес.",
    //                 cancellationToken: cancellationToken);
    //         }
    //         if (messageText.StartsWith("/getserver"))
    //         {
    //             var userId = "1"; 
    //
    //             try
    //             {
    //                 var servers = await GetServersForUser(userId, cancellationToken);
    //
    //                 if (servers.Any())
    //                 {
    //                     var inlineKeyboard = new InlineKeyboardMarkup(servers.Select(server =>
    //                         InlineKeyboardButton.WithCallbackData(server.Link,server.Link )).ToArray());
    //
    //                     await botClient.SendTextMessageAsync(chatId, "Выберите сервер:", replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
    //                 }
    //                 else
    //                 {
    //                     await botClient.SendTextMessageAsync(chatId, "Список серверов пуст.", cancellationToken: cancellationToken);
    //                 }
    //             }
    //             catch (HttpRequestException ex)
    //             {
    //                 _logger.LogError(ex, "Ошибка при получении списка серверов.");
    //                 await botClient.SendTextMessageAsync(chatId, "Ошибка при получении списка серверов.", cancellationToken: cancellationToken);
    //             }
    //         }
    //        
    //     }
    // }
    //TODO: подумать над сохранением id пользователя, добавления всех полей в ответ и парсинг всех запросов прометеуса 
    
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        string id = "1";
        var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id;

        if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text)
        {
            var messageText = update.Message.Text;
    
            _logger.LogInformation($"Received a '{messageText}' message in chat {chatId}.");
    
            if (messageText.StartsWith("/start"))
            {
                var commandParts = messageText.Split(' ');
    
                if (commandParts.Length > 1)
                {
                    id = commandParts[1];
    
                    await botClient.SendTextMessageAsync(chatId, $"Привет! Ваш ID: {id}",
                        cancellationToken: cancellationToken);
                    
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Привет! Как я могу помочь вам сегодня?",
                        cancellationToken: cancellationToken);
                }
            }
            // else if (IPAddress.TryParse(messageText, out var ipAddress))
            // {
            //     try
            //     {
            //         double loadValue = await GetLoadValueFromApiResponse(ipAddress.ToString(), cancellationToken);
            //         await botClient.SendTextMessageAsync(chatId, loadValue.ToString("F2"), cancellationToken: cancellationToken);
            //
            //     }
            //     catch (Exception ex)
            //     {
            //         await botClient.SendTextMessageAsync(chatId, "Произошла ошибка при обращении к API.",
            //             cancellationToken: cancellationToken);
            //         _logger.LogError(ex, "Error calling the API.");
            //     }
            // }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "Пожалуйста, отправьте валидный IP адрес.",
                    cancellationToken: cancellationToken);
            }
            if (messageText.StartsWith("/getserver"))
            {
                var userId = "1"; 

                try
                {
                    var servers = await GetServersForUser(userId, cancellationToken);

                    if (servers.Any())
                    {
                        var inlineKeyboard = new InlineKeyboardMarkup(servers.Select(server =>
                            InlineKeyboardButton.WithCallbackData(server.Link,server.Link )).ToArray());

                        await botClient.SendTextMessageAsync(chatId, "Выберите сервер:", replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Список серверов пуст.", cancellationToken: cancellationToken);
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Ошибка при получении списка серверов.");
                    await botClient.SendTextMessageAsync(chatId, "Ошибка при получении списка серверов.", cancellationToken: cancellationToken);
                }
            }
            
           
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            // Получаем данные callback_query
            var callbackQuery = update.CallbackQuery;
            var callbackData = callbackQuery.Data;
            var callbackChatId = callbackQuery.Message.Chat.Id;

            // Ответ на callback_query, чтобы убрать "грузящийся" индикатор на кнопке
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);

            // Проверяем, является ли callbackData валидным IP-адресом
            if (IPAddress.TryParse(callbackData, out var ipAddress))
            {
                try
                {
                    // Вызываем метод, который обрабатывает IP-адрес
                    double load = await GetLoadValueFromApiResponse(ipAddress.ToString(),"node_load1", cancellationToken);
                    double cpuUsage = await GetLoadValueFromApiResponse(ipAddress.ToString(),"100 - (avg without(mode)(irate(node_cpu_seconds_total[1m])) * 100)", cancellationToken);
                    double memoryUsage = await GetLoadValueFromApiResponse(ipAddress.ToString(),"(node_memory_MemTotal_bytes - node_memory_MemAvailable_bytes) / 1024 / 1024 / 1024", cancellationToken);
                    double diskUsage = await GetLoadValueFromApiResponse(ipAddress.ToString(),"(sum(node_filesystem_size_bytes - node_filesystem_free_bytes) by (instance)) / 1073741824", cancellationToken);
                    double memoryFull = await GetLoadValueFromApiResponse(ipAddress.ToString(),"round(node_memory_MemTotal_bytes / 1024 / 1024 / 1024)", cancellationToken);
                    double discfull = await GetLoadValueFromApiResponse(ipAddress.ToString(),"sum(node_filesystem_size_bytes) / 1024 / 1024 / 1024", cancellationToken);
                    double networkTransmit = await GetLoadValueFromApiResponse(ipAddress.ToString(),"irate(node_network_transmit_bytes_total[5m])", cancellationToken);
                    double networkReceive = await GetLoadValueFromApiResponse(ipAddress.ToString(),"irate(node_network_transmit_bytes_total[5m])", cancellationToken);
                    string resp =
                        $"IP: {ipAddress}\nЗагрузка: {load.ToString("F2")}\nИспользование процессора: {cpuUsage.ToString("F2")} %\nОперативная память: {memoryUsage.ToString("F2")} / {memoryFull.ToString("F2")} ГБ\nИспользование диска: {diskUsage.ToString("F2")} / {discfull.ToString("F2")} ГБ\nОтданные пакеты: {networkTransmit.ToString("F2")}\nПолученные пакеты: {networkReceive.ToString("F2")}";
                    await botClient.SendTextMessageAsync(chatId, resp, cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(callbackChatId, "Произошла ошибка при обращении к API.",
                        cancellationToken: cancellationToken);
                    _logger.LogError(ex, "Error calling the API.");
                }
            }
            else
            {
                // Если callbackData не является валидным IP, отправляем сообщение об ошибке
                await botClient.SendTextMessageAsync(callbackChatId, "Пожалуйста, отправьте валидный IP адрес.",
                    cancellationToken: cancellationToken);
            }
        }
    }
    private async Task<double> GetLoadValueFromApiResponse(string ipAddress,string parameter, CancellationToken cancellationToken)
    {
        var jsonResponse = await GetApiResponse(ipAddress,parameter, cancellationToken);
    
        var apiResponse = JsonSerializer.Deserialize<ApiResponse>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (apiResponse?.Status == "success" && apiResponse.Data?.Result != null && apiResponse.Data.Result.Any())
        {
            // Предполагаем, что 'value' всегда содержит два элемента: timestamp и значение нагрузки.
            double loadValue = Convert.ToDouble(apiResponse.Data.Result[0].Value[1]);
            return loadValue;
        }

        throw new Exception("Не удалось извлечь значение нагрузки из ответа API.");
    }
    private async Task<IEnumerable<Server>> GetServersForUser(string userId, CancellationToken cancellationToken)
    {
        var userServersRequest = new
        {
            UserId = 1
        };

        var jsonRequest = JsonSerializer.Serialize(userServersRequest);
        var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:5143/Server/GetServer")
        {
            Content = httpContent
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.GetValue<string>("bearer"));

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"API request failed with status code: {response.StatusCode}");
            return Enumerable.Empty<Server>();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var servers = JsonSerializer.Deserialize<IEnumerable<Server>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return servers ?? Enumerable.Empty<Server>();
    }
    private async Task<string> GetApiResponse(string ipAddress, string parameter, CancellationToken cancellationToken)
    {
        var machineQueryRequest = new MachineQueryRequest
        {
            Link = ipAddress,
            Query = parameter
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

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError(errorMessage);
        return Task.CompletedTask;
    }
}