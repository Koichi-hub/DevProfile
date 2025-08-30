using DevProfile.Services;
using FluentResults;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DevProfile.Common;

public class UpdateHandler(ITelegramBotClient bot, ILogger<UpdateHandler> logger, InMemoryData inMemoryData, AnimeService animeService) : IUpdateHandler
{
    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogInformation("HandleError: {Exception}", exception);

        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await (update switch
        {
            { Message: { } message } => OnMessage(message),
            { EditedMessage: { } message } => OnMessage(message),
            _ => UnknownUpdateHandlerAsync(update)
        });
    }

    private async Task OnMessage(Message msg)
    {
        if (msg.Text is not { } messageText)
            return;

        if (!inMemoryData.ChatsIds.Contains(msg.Chat.Id))
            inMemoryData.ChatsIds.Add(msg.Chat.Id);

        await (messageText.Split(' ')[0] switch
        {
            "/start" => Start(msg),
            "/anime_waifu" => SendWaifu(msg),
            _ => Usage(msg)
        });
    }

    private async Task Start(Message msg)
    {
        await bot.SendMessage(msg.Chat, "Привет!");
    }

    private async Task SendWaifu(Message msg)
    {
        Result<string> getImageLinkResult = await animeService.GetWaifuImageLink();
        if (getImageLinkResult.IsFailed)
        {
            await bot.SendMessage(msg.Chat, getImageLinkResult.Errors.First().Message);
        }

        await bot.SendPhoto(msg.Chat, getImageLinkResult.Value);
    }

    private async Task Usage(Message msg)
    {
        const string usage = """
            <b>Bot menu</b>:
            /start
            /anime_waifu - получить свою вайфу
            """;
        await bot.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}
