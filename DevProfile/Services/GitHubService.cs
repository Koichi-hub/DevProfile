using Newtonsoft.Json;

namespace DevProfile.Services
{
    public class GitHubService(IHttpClientFactory httpClientFactory)
    {
        public async Task<int> GetNumberOfCommitPerDay()
        {
            var link = @"https://api.github.com/users/Koichi-hub/events";

            HttpClient httpClient = httpClientFactory.CreateClient();
            var httpResponse = await httpClient.GetAsync(link);
            GetActivityResponse response = JsonConvert.DeserializeObject<GetActivityResponse>(await httpResponse.Content.ReadAsStringAsync())!;

            return response.Activities.Count(x => x.Type == "PushEvent");
        }

        public class GetActivityResponse
        {
            public List<Activity> Activities { get; set; } = [];
        }

        public class Activity
        {
            [JsonProperty("type")]
            public string Type { get; set; } = null!;

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }
        }
    }
}
