using DevProfile.Common;
using Microsoft.Extensions.Options;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.AddHttpClient("telegram_bot_client").RemoveAllLoggers()
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        AppSettings? appSettings = sp.GetService<IOptions<AppSettings>>()?.Value;
        ArgumentNullException.ThrowIfNull(appSettings);
        TelegramBotClientOptions options = new(appSettings.BotToken);
        return new TelegramBotClient(options, httpClient);
    });

builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddScoped<ReceiverService>();
builder.Services.AddHostedService<PollingService>();

var host = builder.Build();
host.Run();
