IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<TelegramBotBackgroundService>();
        services.AddHttpClient();
        services.AddLogging(configure => configure.AddConsole());
    })
    .Build();
 
host.Run();