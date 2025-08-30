using FluentResults;
using Newtonsoft.Json;

namespace DevProfile.Services
{
    public class AnimeService(ILogger<AnimeService> logger, IHttpClientFactory httpClientFactory)
    {
        public async Task<Result<string>> GetWaifuImageLink()
        {
            try
            {
                HttpClient httpClient = httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync("https://api.waifu.pics/sfw/waifu");
                return JsonConvert.DeserializeObject<GetWaifuResponse>(await response.Content.ReadAsStringAsync())!.Url;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "");
                return Result.Fail("Ошибка во время получения картинки ;(");
            }
        }

        public class GetWaifuResponse
        {
            public string Url { get; set; } = null!;
        }
    }
}
