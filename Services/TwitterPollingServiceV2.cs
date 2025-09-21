using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TweetFi.Services
{
    public class TwitterPollingServiceV2 : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly TimeSpan _interval;

        public TwitterPollingServiceV2(IServiceScopeFactory scopeFactory, IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _interval = TimeSpan.FromSeconds(30);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[START] Polling service iniciado...");

            // Primeira execução imediata
            await RunCheck(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_interval, stoppingToken);
                await RunCheck(stoppingToken);
            }
        }

        private async Task RunCheck(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var twitterService = scope.ServiceProvider.GetRequiredService<TwitterServiceV2>();

            var targetUsers = _config.GetSection("TargetUsers").Get<string[]>() ?? Array.Empty<string>();

            foreach (var user in targetUsers)
            {
                if (stoppingToken.IsCancellationRequested) break;

                Console.WriteLine($"[CHECK] Buscando tweets de @{user}...");

                var tweet = await twitterService.GetLatestTweetAsync(user);
                if (tweet != null)
                {
                    Console.WriteLine($"[TWEET] @{user}: {tweet.Text} ({tweet.CreatedAt})");
                }
                else
                {
                    Console.WriteLine($"[WARN] Nenhum tweet encontrado para @{user}");
                }
            }
        }
    }
}
