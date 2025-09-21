using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TweetFi.Services
{
    public class TwitterServiceV2
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _bearerToken;
        private string? _myUserIdCache;

        public TwitterServiceV2(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _bearerToken = config["Twitter:BearerToken"] 
                           ?? throw new Exception("BearerToken não encontrado em appsettings.json ou secrets.");
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
            return client;
        }

        // Retorna o userId do usuário autenticado (cache)
        public async Task<string?> GetMyUserIdAsync()
        {
            if (!string.IsNullOrEmpty(_myUserIdCache))
                return _myUserIdCache;

            try
            {
                using var client = CreateClient();
                var resp = await client.GetAsync("https://api.twitter.com/2/users/me");
                var text = await resp.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] users/me JSON: {text}");

                if (!resp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Falha em GetMyUserIdAsync: {resp.StatusCode}");
                    return null;
                }

                var doc = JsonDocument.Parse(text);
                if (!doc.RootElement.TryGetProperty("data", out var data))
                    return null;

                _myUserIdCache = data.GetProperty("id").GetString();
                return _myUserIdCache;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro em GetMyUserIdAsync: {ex.Message}");
                return null;
            }
        }

        // Retorna o último tweet de um usuário
        public async Task<Tweet?> GetLatestTweetAsync(string username)
        {
            try
            {
                using var client = CreateClient();

                // 1️⃣ Obter userId pelo username
                var userResp = await client.GetAsync($"https://api.twitter.com/2/users/by/username/{username}");
                var userJson = await userResp.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] User JSON: {userJson}");

                if (!userResp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Falha ao obter userId para {username}: {userResp.StatusCode}");
                    return null;
                }

                var userDoc = JsonDocument.Parse(userJson);
                if (!userDoc.RootElement.TryGetProperty("data", out var userData))
                    return null;

                var userId = userData.GetProperty("id").GetString();

                // 2️⃣ Obter tweets do usuário
                var tweetsResp = await client.GetAsync($"https://api.twitter.com/2/users/{userId}/tweets?max_results=5&tweet.fields=created_at");
                var tweetsJson = await tweetsResp.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Tweets JSON: {tweetsJson}");

                if (!tweetsResp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Falha ao obter tweets de {username}: {tweetsResp.StatusCode}");
                    return null;
                }

                var tweetDoc = JsonDocument.Parse(tweetsJson);
                if (!tweetDoc.RootElement.TryGetProperty("data", out var tweetsArray) || tweetsArray.GetArrayLength() == 0)
                    return null;

                var firstTweet = tweetsArray[0];
                return new Tweet
                {
                    Id = firstTweet.GetProperty("id").GetString(),
                    Text = firstTweet.GetProperty("text").GetString(),
                    CreatedAt = firstTweet.TryGetProperty("created_at", out var created)
                                    ? created.GetDateTime()
                                    : DateTime.MinValue
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro em GetLatestTweetAsync: {ex.Message}");
                return null;
            }
        }

        // Curtir um tweet
        public async Task<bool> LikeTweetAsync(string tweetId)
        {
            try
            {
                var userId = await GetMyUserIdAsync();
                if (string.IsNullOrEmpty(userId)) return false;

                using var client = CreateClient();
                var content = new StringContent($"{{\"tweet_id\":\"{tweetId}\"}}", Encoding.UTF8, "application/json");
                var resp = await client.PostAsync($"https://api.twitter.com/2/users/{userId}/likes", content);

                Console.WriteLine($"[INFO] LikeTweetAsync Status: {resp.StatusCode}");
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro em LikeTweetAsync: {ex.Message}");
                return false;
            }
        }

        // Retweetar
        public async Task<bool> RetweetAsync(string tweetId)
        {
            try
            {
                var userId = await GetMyUserIdAsync();
                if (string.IsNullOrEmpty(userId)) return false;

                using var client = CreateClient();
                var content = new StringContent($"{{\"tweet_id\":\"{tweetId}\"}}", Encoding.UTF8, "application/json");
                var resp = await client.PostAsync($"https://api.twitter.com/2/users/{userId}/retweets", content);

                Console.WriteLine($"[INFO] RetweetAsync Status: {resp.StatusCode}");
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro em RetweetAsync: {ex.Message}");
                return false;
            }
        }

        // Responder um tweet
        public async Task<bool> ReplyTweetAsync(string tweetId, string message)
        {
            try
            {
                using var client = CreateClient();

                var jsonContent = $@"{{
                    ""text"": ""{message}"",
                    ""reply"": {{
                        ""in_reply_to_tweet_id"": ""{tweetId}""
                    }}
                }}";

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var resp = await client.PostAsync($"https://api.twitter.com/2/tweets", content);

                Console.WriteLine($"[INFO] ReplyTweetAsync Status: {resp.StatusCode}");
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro em ReplyTweetAsync: {ex.Message}");
                return false;
            }
        }
    }

    public class Tweet
    {
        public string? Id { get; set; }
        public string? Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
