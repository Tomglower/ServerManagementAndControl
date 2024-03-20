IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<TelegramBotBackgroundService>();
        services.AddLogging(configure => configure.AddConsole());
    })
    .Build();
 
host.Run();