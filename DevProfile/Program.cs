using DevProfile.Common;
using DevProfile.Jobs;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using Quartz;
using Telegram.Bot;

var logger = LogManager.GetCurrentClassLogger();

try
{
    logger.Debug("Start app");

    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.Configure<AppSettings>(builder.Configuration);

    builder.Services.AddSingleton<InMemoryData>();

    //NLog
    builder.Logging.ClearProviders();
    builder.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddNLog();
    });

    //Quartz
    builder.Services.AddQuartz(configurator =>
    {
        JobKey jobKey = new("SendWishJob");
        configurator.AddJob<SendWishJob>(opts => opts.WithIdentity(jobKey));

        configurator.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity("SendWishJob-trigger-1")
            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(22, 0).InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg")))
        );

        configurator.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity("SendWishJob-trigger-2")
            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(7, 0).InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg")))
        );
    });
    builder.Services.AddQuartzHostedService(options =>
    {
        options.WaitForJobsToComplete = true;
    });

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
}
catch (Exception exception)
{
    logger.Error(exception, "Stop app, exception=");
}
finally
{
    LogManager.Shutdown();
}
