using DevProfile.Common;
using Quartz;
using Telegram.Bot;

namespace DevProfile.Jobs
{
    public class SendWishJob(ITelegramBotClient bot, ILogger<UpdateHandler> logger, InMemoryData inMemoryData) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("Собираюсь отправить пожелание");

            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FromSerializedString("Asia/Yekaterinburg"));

            if (localTime.Hour == 7)
            {
                foreach (var chatId in inMemoryData.ChatsIds)
                {
                    await bot.SendMessage(chatId, "Доброе утро!");
                }
            }
            else
            {
                foreach (var chatId in inMemoryData.ChatsIds)
                {
                    await bot.SendMessage(chatId, "Спокойной ночи!");
                }
            }

            logger.LogInformation("Отправил пожелание");
        }
    }
}
